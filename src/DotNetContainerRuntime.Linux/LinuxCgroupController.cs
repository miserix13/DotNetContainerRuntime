using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DotNetContainerRuntime.Core.Abstractions;
using DotNetContainerRuntime.Core.Specifications;

namespace DotNetContainerRuntime.Linux;

/// <summary>
/// Linux cgroups v2 implementation for resource control.
/// Uses the unified hierarchy at /sys/fs/cgroup.
/// </summary>
public class LinuxCgroupController : IResourceController
{
    private const string CgroupRoot = "/sys/fs/cgroup";
    private readonly string _cgroupBasePath;

    [DllImport("libc", SetLastError = true)]
    private static extern int kill(int pid, int sig);

    public LinuxCgroupController(string? basePath = null)
    {
        if (!OperatingSystem.IsLinux())
        {
            throw new PlatformNotSupportedException("Linux cgroups are only supported on Linux.");
        }

        _cgroupBasePath = basePath ?? Path.Combine(CgroupRoot, "dotnet-container-runtime");
    }

    public async Task<ResourceControlContext> CreateResourceGroupAsync(
        string containerId,
        ResourceLimits limits,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string cgroupPath = Path.Combine(_cgroupBasePath, containerId);

        // Create cgroup directory
        if (!Directory.Exists(cgroupPath))
        {
            Directory.CreateDirectory(cgroupPath);
        }

        // Enable controllers in parent
        await EnableControllersAsync(_cgroupBasePath, cancellationToken);

        var context = new ResourceControlContext
        {
            ContainerId = containerId,
            Path = cgroupPath
        };

        // Set resource limits
        await UpdateLimitsAsync(context, limits, cancellationToken);
        
        return context;
    }

    public async Task UpdateLimitsAsync(
        ResourceControlContext context,
        ResourceLimits limits,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string cgroupPath = context.Path;

        if (!Directory.Exists(cgroupPath))
        {
            throw new InvalidOperationException($"Cgroup {context.ContainerId} does not exist at {cgroupPath}");
        }

        // Set CPU limits
        if (limits.CpuShares.HasValue || limits.CpuQuota.HasValue || limits.CpuPeriod.HasValue)
        {
            await SetCpuLimitsAsync(cgroupPath, limits, cancellationToken);
        }

        // Set memory limits
        if (limits.MemoryLimit.HasValue || limits.MemorySwap.HasValue)
        {
            await SetMemoryLimitsAsync(cgroupPath, limits, cancellationToken);
        }

        // Set I/O limits
        if (limits.BlockIOWeight.HasValue)
        {
            await SetIoLimitsAsync(cgroupPath, limits, cancellationToken);
        }

        // Set PID limits
        if (limits.PidsLimit.HasValue)
        {
            await SetPidLimitAsync(cgroupPath, limits.PidsLimit.Value, cancellationToken);
        }
    }

    public async Task<ResourceUsage> GetUsageAsync(
        ResourceControlContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string cgroupPath = context.Path;

        if (!Directory.Exists(cgroupPath))
        {
            throw new InvalidOperationException($"Cgroup {context.ContainerId} does not exist at {cgroupPath}");
        }

        // Collect all usage values first
        ulong cpuUsage = 0;
        long memoryUsage = 0;
        long memoryMaxUsage = 0;
        int processCount = 0;

        // Read CPU usage
        string cpuStatPath = Path.Combine(cgroupPath, "cpu.stat");
        if (File.Exists(cpuStatPath))
        {
            var cpuStats = await ParseKeyValueFileAsync(cpuStatPath, cancellationToken);
            if (cpuStats.TryGetValue("usage_usec", out var cpuUsageStr) && 
                long.TryParse(cpuUsageStr, out var cpuUsageLong))
            {
                cpuUsage = (ulong)(cpuUsageLong * 1000); // Convert microseconds to nanoseconds
            }
        }

        // Read memory usage
        string memoryCurrentPath = Path.Combine(cgroupPath, "memory.current");
        if (File.Exists(memoryCurrentPath))
        {
            string content = await File.ReadAllTextAsync(memoryCurrentPath, cancellationToken);
            if (long.TryParse(content.Trim(), out var memUsage))
            {
                memoryUsage = memUsage;
            }
        }

        // Read memory max
        string memoryMaxPath = Path.Combine(cgroupPath, "memory.max");
        if (File.Exists(memoryMaxPath))
        {
            string content = await File.ReadAllTextAsync(memoryMaxPath, cancellationToken);
            if (content.Trim() != "max" && long.TryParse(content.Trim(), out var memMax))
            {
                memoryMaxUsage = memMax;
            }
        }

        // Read I/O stats
        string ioStatPath = Path.Combine(cgroupPath, "io.stat");
        if (File.Exists(ioStatPath))
        {
            var lines = await File.ReadAllLinesAsync(ioStatPath, cancellationToken);
            long totalRead = 0, totalWrite = 0;

            foreach (var line in lines)
            {
                var stats = ParseIoStatLine(line);
                if (stats.TryGetValue("rbytes", out var readBytes))
                {
                    totalRead += long.Parse(readBytes);
                }
                if (stats.TryGetValue("wbytes", out var writeBytes))
                {
                    totalWrite += long.Parse(writeBytes);
                }
            }

            // Note: ResourceUsage doesn't have IoReadBytes/IoWriteBytes properties
            // These would need to be tracked separately if needed
        }

        // Read PID count
        string pidsCurrentPath = Path.Combine(cgroupPath, "pids.current");
        if (File.Exists(pidsCurrentPath))
        {
            string content = await File.ReadAllTextAsync(pidsCurrentPath, cancellationToken);
            if (int.TryParse(content.Trim(), out var pidCount))
            {
                processCount = pidCount;
            }
        }

        return new ResourceUsage
        {
            CpuUsage = cpuUsage,
            MemoryUsage = memoryUsage,
            MemoryMaxUsage = memoryMaxUsage,
            ProcessCount = processCount
        };
    }

    public async Task AddProcessAsync(
        ResourceControlContext context,
        int pid,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string procsPath = Path.Combine(context.Path, "cgroup.procs");

        await File.WriteAllTextAsync(procsPath, pid.ToString(), cancellationToken);
    }

    public async Task DeleteResourceGroupAsync(
        ResourceControlContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string cgroupPath = context.Path;

        if (!Directory.Exists(cgroupPath))
        {
            return; // Already deleted
        }

        // Kill all processes in the cgroup first
        await KillAllProcessesAsync(cgroupPath, cancellationToken);

        // Wait a bit for processes to exit
        await Task.Delay(100, cancellationToken);

        // Remove the cgroup directory
        try
        {
            Directory.Delete(cgroupPath, false);
        }
        catch (IOException)
        {
            // Cgroup may still have processes, try again
            await Task.Delay(500, cancellationToken);
            Directory.Delete(cgroupPath, false);
        }
    }

    private async Task EnableControllersAsync(string cgroupPath, CancellationToken cancellationToken)
    {
        string subtreeControlPath = Path.Combine(cgroupPath, "cgroup.subtree_control");
        
        if (!File.Exists(subtreeControlPath))
        {
            return;
        }

        // Enable cpu, memory, io, and pids controllers
        string controllers = "+cpu +memory +io +pids";
        try
        {
            await File.WriteAllTextAsync(subtreeControlPath, controllers, cancellationToken);
        }
        catch (IOException)
        {
            // Controllers may already be enabled
        }
    }

    private async Task SetCpuLimitsAsync(
        string cgroupPath,
        ResourceLimits limits,
        CancellationToken cancellationToken)
    {
        // Set CPU weight (replaces cpu.shares in v1)
        if (limits.CpuShares.HasValue)
        {
            string cpuWeightPath = Path.Combine(cgroupPath, "cpu.weight");
            // Convert shares (2-262144) to weight (1-10000)
            long weight = Math.Clamp((long)limits.CpuShares.Value / 26, 1, 10000);
            await File.WriteAllTextAsync(cpuWeightPath, weight.ToString(), cancellationToken);
        }

        // Set CPU max (quota/period)
        if (limits.CpuQuota.HasValue || limits.CpuPeriod.HasValue)
        {
            string cpuMaxPath = Path.Combine(cgroupPath, "cpu.max");
            long quota = limits.CpuQuota ?? 100000;
            long period = (long)(limits.CpuPeriod ?? 100000);
            string value = quota == -1 ? "max" : $"{quota} {period}";
            await File.WriteAllTextAsync(cpuMaxPath, value, cancellationToken);
        }
    }

    private async Task SetMemoryLimitsAsync(
        string cgroupPath,
        ResourceLimits limits,
        CancellationToken cancellationToken)
    {
        if (limits.MemoryLimit.HasValue)
        {
            string memoryMaxPath = Path.Combine(cgroupPath, "memory.max");
            string value = limits.MemoryLimit.Value == -1 ? "max" : limits.MemoryLimit.Value.ToString();
            await File.WriteAllTextAsync(memoryMaxPath, value, cancellationToken);
        }

        if (limits.MemorySwap.HasValue)
        {
            string swapMaxPath = Path.Combine(cgroupPath, "memory.swap.max");
            string value = limits.MemorySwap.Value == -1 ? "max" : limits.MemorySwap.Value.ToString();
            await File.WriteAllTextAsync(swapMaxPath, value, cancellationToken);
        }
    }

    private async Task SetIoLimitsAsync(
        string cgroupPath,
        ResourceLimits limits,
        CancellationToken cancellationToken)
    {
        if (limits.BlockIOWeight.HasValue)
        {
            string ioWeightPath = Path.Combine(cgroupPath, "io.weight");
            long weight = Math.Clamp((long)limits.BlockIOWeight.Value, 1L, 10000L);
            await File.WriteAllTextAsync(ioWeightPath, $"default {weight}", cancellationToken);
        }
    }

    private async Task SetPidLimitAsync(
        string cgroupPath,
        long limit,
        CancellationToken cancellationToken)
    {
        string pidsMaxPath = Path.Combine(cgroupPath, "pids.max");
        string value = limit == -1 ? "max" : limit.ToString();
        await File.WriteAllTextAsync(pidsMaxPath, value, cancellationToken);
    }

    private async Task<Dictionary<string, string>> ParseKeyValueFileAsync(
        string filePath,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, string>();
        var lines = await File.ReadAllLinesAsync(filePath, cancellationToken);

        foreach (var line in lines)
        {
            var parts = line.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                result[parts[0]] = parts[1];
            }
        }

        return result;
    }

    private Dictionary<string, string> ParseIoStatLine(string line)
    {
        var result = new Dictionary<string, string>();
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // First part is device ID, rest are key=value pairs
        for (int i = 1; i < parts.Length; i++)
        {
            var kv = parts[i].Split('=');
            if (kv.Length == 2)
            {
                result[kv[0]] = kv[1];
            }
        }

        return result;
    }

    private async Task KillAllProcessesAsync(string cgroupPath, CancellationToken cancellationToken)
    {
        string killPath = Path.Combine(cgroupPath, "cgroup.kill");
        
        if (File.Exists(killPath))
        {
            // cgroup.kill is available in newer kernels (5.14+)
            await File.WriteAllTextAsync(killPath, "1", cancellationToken);
        }
        else
        {
            // Fallback: manually kill processes
            string procsPath = Path.Combine(cgroupPath, "cgroup.procs");
            if (File.Exists(procsPath))
            {
                var pids = await File.ReadAllLinesAsync(procsPath, cancellationToken);
                foreach (var pidStr in pids)
                {
                    if (int.TryParse(pidStr.Trim(), out int pid))
                    {
                        try
                        {
                            kill(pid, 9); // SIGKILL
                        }
                        catch
                        {
                            // Process may have already exited
                        }
                    }
                }
            }
        }
    }

    public void Dispose()
    {
        // Cleanup is handled by DeleteResourceGroupAsync
    }
}
