using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotNetContainerRuntime.Core.Abstractions;
using DotNetContainerRuntime.Core.Specifications;

namespace DotNetContainerRuntime.Linux;

/// <summary>
/// Linux container runtime implementation orchestrating namespaces, cgroups, filesystem, and process management.
/// </summary>
public class LinuxContainerRuntime : IContainerRuntime
{
    private readonly INamespaceManager _namespaceManager;
    private readonly IResourceController _resourceController;
    private readonly IFilesystemManager _filesystemManager;
    private readonly IProcessManager _processManager;
    private readonly Dictionary<string, ContainerContext> _containers = new();

    public LinuxContainerRuntime(
        INamespaceManager? namespaceManager = null,
        IResourceController? resourceController = null,
        IFilesystemManager? filesystemManager = null,
        IProcessManager? processManager = null)
    {
        if (!OperatingSystem.IsLinux())
        {
            throw new PlatformNotSupportedException("Linux container runtime is only supported on Linux.");
        }

        _namespaceManager = namespaceManager ?? new LinuxNamespaceManager();
        _resourceController = resourceController ?? new LinuxCgroupController();
        _filesystemManager = filesystemManager ?? new LinuxFilesystemManager();
        _processManager = processManager ?? new LinuxProcessManager();
    }

    public async Task CreateAsync(
        string containerId,
        string bundlePath,
        CancellationToken cancellationToken = default)
    {
        if (_containers.ContainsKey(containerId))
        {
            throw new InvalidOperationException($"Container {containerId} already exists");
        }

        // Load OCI configuration from bundle
        string configPath = Path.Combine(bundlePath, "config.json");
        if (!File.Exists(configPath))
        {
            throw new InvalidOperationException($"Config file not found: {configPath}");
        }
        
        string configJson = await File.ReadAllTextAsync(configPath, cancellationToken);
        var config = JsonSerializer.Deserialize<RuntimeConfiguration>(configJson) 
            ?? throw new InvalidOperationException("Failed to deserialize config.json");

        // Validate configuration
        ValidateConfiguration(config);

        // Create container context
        var context = new ContainerContext
        {
            Id = containerId,
            Config = config,
            Status = ContainerStatus.Creating,
            CreatedAt = DateTimeOffset.UtcNow,
            Bundle = bundlePath
        };

        _containers[containerId] = context;

        try
        {
            // Setup filesystem
            string rootfsPath = Path.Combine(bundlePath, config.Root?.Path ?? "rootfs");
            bool readOnly = config.Root?.ReadOnly ?? false;
            
            var fsContext = await _filesystemManager.PrepareRootfsAsync(
                containerId,
                rootfsPath,
                readOnly,
                cancellationToken);
            
            context.FilesystemContext = fsContext;
            
            // Mount additional filesystems
            if (config.Mounts != null && config.Mounts.Count > 0)
            {
                await _filesystemManager.MountFilesystemsAsync(fsContext, config.Mounts, cancellationToken);
            }

            // Setup cgroups
            if (config.Linux?.Resources != null)
            {
                var limits = ConvertToResourceLimits(config.Linux.Resources);
                var resourceContext = await _resourceController.CreateResourceGroupAsync(
                    containerId,
                    limits,
                    cancellationToken);
                    
                context.ResourceContext = resourceContext;
            }

            context.Status = ContainerStatus.Created;
        }
        catch
        {
            _containers.Remove(containerId);
            throw;
        }
    }

    public async Task StartAsync(
        string containerId,
        CancellationToken cancellationToken = default)
    {
        if (!_containers.TryGetValue(containerId, out var context))
        {
            throw new InvalidOperationException($"Container {containerId} not found");
        }

        if (context.Status != ContainerStatus.Created)
        {
            throw new InvalidOperationException($"Container {containerId} is not in created state");
        }

        var config = context.Config;

        // Create namespaces
        NamespaceContext? namespaceContext = null;
        if (config.Linux?.Namespaces != null && config.Linux.Namespaces.Count > 0)
        {
            var namespaces = config.Linux.Namespaces.Select(ns => (NamespaceType)Enum.Parse(typeof(NamespaceType), ns.Type, true));
            namespaceContext = await _namespaceManager.CreateNamespacesAsync(namespaces, cancellationToken);
        }

        // Start container process
        var processContext = await _processManager.ExecuteAsync(
            containerId,
            config.Process ?? throw new InvalidOperationException("Process configuration is required"),
            namespaceContext,
            context.ResourceContext ?? throw new InvalidOperationException("Resource context is required"),
            cancellationToken);

        context.Pid = processContext.Pid;
        context.Status = ContainerStatus.Running;

        // Add process to cgroup
        if (context.ResourceContext != null)
        {
            await _resourceController.AddProcessAsync(context.ResourceContext, processContext.Pid, cancellationToken);
        }

        // Wait for process in background
        _ = Task.Run(async () =>
        {
            try
            {
                int exitCode = await _processManager.WaitForExitAsync(processContext, CancellationToken.None);
                context.ExitCode = exitCode;
                context.Status = ContainerStatus.Stopped;
            }
            catch
            {
                context.Status = ContainerStatus.Stopped;
            }
        }, CancellationToken.None);
    }

    public async Task KillAsync(
        string containerId,
        int signal,
        CancellationToken cancellationToken = default)
    {
        if (!_containers.TryGetValue(containerId, out var context))
        {
            throw new InvalidOperationException($"Container {containerId} not found");
        }

        if (context.Pid == null)
        {
            throw new InvalidOperationException($"Container {containerId} has no running process");
        }

        await _processManager.SendSignalAsync(context.Pid.Value, signal, cancellationToken);
    }

    public async Task DeleteAsync(
        string containerId,
        CancellationToken cancellationToken = default)
    {
        if (!_containers.TryGetValue(containerId, out var context))
        {
            throw new InvalidOperationException($"Container {containerId} not found");
        }

        if (context.Status == ContainerStatus.Running)
        {
            throw new InvalidOperationException($"Cannot delete running container {containerId}");
        }

        // Cleanup resources
        if (context.FilesystemContext != null)
        {
            await _filesystemManager.CleanupAsync(context.FilesystemContext, cancellationToken);
        }

        if (context.ResourceContext != null)
        {
            await _resourceController.DeleteResourceGroupAsync(context.ResourceContext, cancellationToken);
        }

        _containers.Remove(containerId);
    }

    public Task<ContainerState> GetStateAsync(
        string containerId,
        CancellationToken cancellationToken = default)
    {
        if (!_containers.TryGetValue(containerId, out var context))
        {
            throw new InvalidOperationException($"Container {containerId} not found");
        }

        var state = new ContainerState
        {
            OciVersion = context.Config.OciVersion,
            Id = containerId,
            Status = context.Status,
            Pid = context.Pid,
            Bundle = context.Bundle
        };

        return Task.FromResult(state);
    }

    public Task<IReadOnlyCollection<string>> ListAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<string> containerIds = _containers.Keys.ToList();
        return Task.FromResult(containerIds);
    }

    private void ValidateConfiguration(RuntimeConfiguration config)
    {
        if (string.IsNullOrEmpty(config.OciVersion))
        {
            throw new InvalidOperationException("OCI version is required");
        }

        if (config.Root == null)
        {
            throw new InvalidOperationException("Root configuration is required");
        }

        if (config.Process == null)
        {
            throw new InvalidOperationException("Process configuration is required");
        }

        if (config.Process.Args == null || config.Process.Args.Count == 0)
        {
            throw new InvalidOperationException("Process args are required");
        }
    }

    private ResourceLimits ConvertToResourceLimits(ResourcesConfiguration resources)
    {
        return new ResourceLimits
        {
            MemoryLimit = resources.Memory?.Limit,
            MemorySwap = resources.Memory?.Swap,
            CpuShares = resources.Cpu?.Shares,
            CpuQuota = resources.Cpu?.Quota,
            CpuPeriod = resources.Cpu?.Period,
            PidsLimit = resources.Pids?.Limit
        };
    }

    public void Dispose()
    {
        // Dispose pattern - managers are created and owned by this runtime
        if (_namespaceManager is IDisposable nsDisposable) nsDisposable.Dispose();
        if (_resourceController is IDisposable rcDisposable) rcDisposable.Dispose();
        if (_filesystemManager is IDisposable fsDisposable) fsDisposable.Dispose();
        if (_processManager is IDisposable pmDisposable) pmDisposable.Dispose();
    }

    private class ContainerContext
    {
        public required string Id { get; init; }
        public required RuntimeConfiguration Config { get; init; }
        public required ContainerStatus Status { get; set; }
        public int? Pid { get; set; }
        public int? ExitCode { get; set; }
        public required DateTimeOffset CreatedAt { get; init; }
        public required string Bundle { get; init; }
        public FilesystemContext? FilesystemContext { get; set; }
        public ResourceControlContext? ResourceContext { get; set; }
    }
}
