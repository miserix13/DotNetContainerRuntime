using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DotNetContainerRuntime.Core.Abstractions;
using DotNetContainerRuntime.Core.Specifications;
using DotNetContainerRuntime.Linux.Interop;

namespace DotNetContainerRuntime.Linux;

/// <summary>
/// Linux filesystem manager implementing OverlayFS for layered container filesystems.
/// </summary>
public class LinuxFilesystemManager : IFilesystemManager
{
    private readonly string _storageRoot;
    private readonly Dictionary<string, MountInfo> _mounts = new();

    public LinuxFilesystemManager(string? storageRoot = null)
    {
        if (!OperatingSystem.IsLinux())
        {
            throw new PlatformNotSupportedException("Linux filesystem operations are only supported on Linux.");
        }

        _storageRoot = storageRoot ?? Path.Combine("/var/lib", "dotnet-container-runtime");
        Directory.CreateDirectory(_storageRoot);
    }

    public async Task<FilesystemContext> PrepareRootfsAsync(
        string containerId,
        string rootfsPath,
        bool readOnly,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string containerRoot = Path.Combine(_storageRoot, containerId);
        string mergedPath = Path.Combine(containerRoot, "rootfs");
        string workPath = Path.Combine(containerRoot, "work");
        string upperPath = Path.Combine(containerRoot, "upper");

        // Create directories
        Directory.CreateDirectory(containerRoot);
        Directory.CreateDirectory(mergedPath);
        
        if (!readOnly)
        {
            Directory.CreateDirectory(workPath);
            Directory.CreateDirectory(upperPath);
            
            // Mount overlayfs
            await MountOverlayAsync(rootfsPath, upperPath, workPath, mergedPath, cancellationToken);
        }
        else
        {
            // For read-only, just bind mount
            await MountFilesystemAsync(rootfsPath, mergedPath, "bind", new[] { "bind", "ro" }, cancellationToken);
        }

        var context = new FilesystemContext
        {
            ContainerId = containerId,
            MountPath = mergedPath,
            UpperPath = readOnly ? null : upperPath,
            WorkPath = readOnly ? null : workPath,
            ReadOnly = readOnly
        };

        _mounts[containerId] = new MountInfo
        {
            ContainerId = containerId,
            RootfsPath = mergedPath,
            UpperPath = upperPath,
            WorkPath = workPath,
            LowerPath = rootfsPath
        };
        
        return context;
    }

    public Task MountFilesystemAsync(
        string source,
        string target,
        string fsType,
        string[]? options,
        CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsLinux())
        {
            throw new PlatformNotSupportedException("Linux mount operations are only supported on Linux.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        ulong flags = ParseMountFlags(options);
        string data = string.Join(",", options?.Where(o => !IsMountFlag(o)) ?? Array.Empty<string>());

        int result = LinuxMount.mount(source, target, fsType, flags, nint.Zero);
        
        if (result != 0)
        {
            int errno = Marshal.GetLastPInvokeError();
            throw new InvalidOperationException(
                $"Failed to mount {source} at {target}. mount() returned {result}, errno: {errno}");
        }

        return Task.CompletedTask;
    }

    public Task UnmountFilesystemAsync(
        string target,
        bool force,
        CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsLinux())
        {
            throw new PlatformNotSupportedException("Linux unmount operations are only supported on Linux.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        int flags = force ? LinuxMount.MNT_FORCE : 0;
        int result = LinuxMount.umount2(target, flags);

        if (result != 0)
        {
            int errno = Marshal.GetLastPInvokeError();
            if (errno != 22 && errno != 2) // Ignore EINVAL and ENOENT
            {
                throw new InvalidOperationException(
                    $"Failed to unmount {target}. umount2() returned {result}, errno: {errno}");
            }
        }

        return Task.CompletedTask;
    }

    public Task PivotRootAsync(
        FilesystemContext context,
        CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsLinux())
        {
            throw new PlatformNotSupportedException("pivot_root is only supported on Linux.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        string newRoot = context.MountPath;
        string putOld = Path.Combine(newRoot, ".pivot_root");
        Directory.CreateDirectory(putOld);

        int result = LinuxMount.pivot_root(newRoot, putOld);

        if (result != 0)
        {
            int errno = Marshal.GetLastPInvokeError();
            throw new InvalidOperationException(
                $"Failed to pivot_root from {newRoot} to {putOld}. pivot_root() returned {result}, errno: {errno}");
        }

        return Task.CompletedTask;
    }

    public Task ChrootAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsLinux())
        {
            throw new PlatformNotSupportedException("chroot is only supported on Linux.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        int result = LinuxMount.chroot(path);

        if (result != 0)
        {
            int errno = Marshal.GetLastPInvokeError();
            throw new InvalidOperationException(
                $"Failed to chroot to {path}. chroot() returned {result}, errno: {errno}");
        }

        // Change to root directory after chroot
        Directory.SetCurrentDirectory("/");

        return Task.CompletedTask;
    }

    public async Task CleanupAsync(
        FilesystemContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Unmount all mount points in reverse order
        foreach (var mountPoint in context.MountPoints.AsEnumerable().Reverse())
        {
            await UnmountFilesystemAsync(mountPoint, force: true, cancellationToken);
        }

        // Unmount the rootfs
        await UnmountFilesystemAsync(context.MountPath, force: true, cancellationToken);

        // Remove container directories
        string containerRoot = Path.Combine(_storageRoot, context.ContainerId);
        if (Directory.Exists(containerRoot))
        {
            try
            {
                Directory.Delete(containerRoot, recursive: true);
            }
            catch (IOException)
            {
                // Best effort cleanup
            }
        }

        _mounts.Remove(context.ContainerId);
    }
    
    public async Task MountFilesystemsAsync(
        FilesystemContext context,
        IEnumerable<MountConfiguration> mounts,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        foreach (var mount in mounts)
        {
            string targetPath = Path.Combine(context.MountPath, mount.Destination.TrimStart('/'));
            
            // Create mount point if it doesn't exist
            if (!Directory.Exists(targetPath) && !File.Exists(targetPath))
            {
                if (mount.Type == "bind" && File.Exists(mount.Source))
                {
                    // Create file for bind mount
                    await File.WriteAllTextAsync(targetPath, string.Empty, cancellationToken);
                }
                else
                {
                    Directory.CreateDirectory(targetPath);
                }
            }

            // Parse mount options
            var options = mount.Options?.ToList() ?? new List<string>();

            await MountFilesystemAsync(
                source: mount.Source ?? "none",
                target: targetPath,
                fsType: mount.Type ?? "tmpfs",
                options: options.ToArray(),
                cancellationToken);
                
            context.MountPoints.Add(targetPath);
        }
    }

    private async Task MountOverlayAsync(
        string lowerPath,
        string upperPath,
        string workPath,
        string targetPath,
        CancellationToken cancellationToken)
    {
        if (!Directory.Exists(lowerPath))
        {
            throw new DirectoryNotFoundException($"Lower directory not found: {lowerPath}");
        }

        string options = $"lowerdir={lowerPath},upperdir={upperPath},workdir={workPath}";

        await MountFilesystemAsync(
            source: "overlay",
            target: targetPath,
            fsType: "overlay",
            options: new[] { options },
            cancellationToken);
    }

    private static ulong ParseMountFlags(string[]? options)
    {
        if (options == null) return 0;

        ulong flags = 0;
        foreach (var opt in options)
        {
            flags |= opt switch
            {
                "ro" or "rdonly" => LinuxMount.MS_RDONLY,
                "nosuid" => LinuxMount.MS_NOSUID,
                "nodev" => LinuxMount.MS_NODEV,
                "noexec" => LinuxMount.MS_NOEXEC,
                "sync" => LinuxMount.MS_SYNCHRONOUS,
                "remount" => LinuxMount.MS_REMOUNT,
                "bind" => LinuxMount.MS_BIND,
                "private" => LinuxMount.MS_PRIVATE,
                "slave" => LinuxMount.MS_SLAVE,
                "shared" => LinuxMount.MS_SHARED,
                _ => 0
            };
        }
        return flags;
    }

    private static bool IsMountFlag(string option)
    {
        return option switch
        {
            "ro" or "rdonly" or "nosuid" or "nodev" or "noexec" or "sync" or
            "remount" or "bind" or "private" or "slave" or "shared" => true,
            _ => false
        };
    }

    public void Dispose()
    {
        // Cleanup all mounts
        foreach (var mountInfo in _mounts.Values.ToList())
        {
            try
            {
                var context = new FilesystemContext
                {
                    ContainerId = mountInfo.ContainerId,
                    MountPath = mountInfo.RootfsPath,
                    UpperPath = mountInfo.UpperPath,
                    WorkPath = mountInfo.WorkPath,
                    ReadOnly = false
                };
                CleanupAsync(context, CancellationToken.None)
                    .GetAwaiter().GetResult();
            }
            catch
            {
                // Best effort cleanup
            }
        }
    }

    private class MountInfo
    {
        public required string ContainerId { get; init; }
        public required string RootfsPath { get; init; }
        public required string UpperPath { get; init; }
        public required string WorkPath { get; init; }
        public required string LowerPath { get; init; }
    }
}
