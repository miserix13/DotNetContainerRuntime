using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DotNetContainerRuntime.Core.Abstractions;
using DotNetContainerRuntime.Core.Specifications;
using DotNetContainerRuntime.Linux.Interop;

namespace DotNetContainerRuntime.Linux;

/// <summary>
/// Linux implementation of namespace management using unshare() and setns() syscalls.
/// </summary>
public class LinuxNamespaceManager : INamespaceManager
{
    private readonly Dictionary<NamespaceType, int> _namespaceFds = new();

    public Task<NamespaceContext> CreateNamespacesAsync(
        IEnumerable<NamespaceType> namespaces,
        CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsLinux())
        {
            throw new PlatformNotSupportedException("Linux namespaces are only supported on Linux.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var namespaceList = namespaces.ToList();

        // Build flags for unshare syscall
        int flags = 0;
        foreach (var ns in namespaceList)
        {
            flags |= ns switch
            {
                NamespaceType.Pid => LinuxNamespaces.CLONE_NEWPID,
                NamespaceType.Network => LinuxNamespaces.CLONE_NEWNET,
                NamespaceType.Mount => LinuxNamespaces.CLONE_NEWNS,
                NamespaceType.Ipc => LinuxNamespaces.CLONE_NEWIPC,
                NamespaceType.Uts => LinuxNamespaces.CLONE_NEWUTS,
                NamespaceType.User => LinuxNamespaces.CLONE_NEWUSER,
                NamespaceType.Cgroup => LinuxNamespaces.CLONE_NEWCGROUP,
                NamespaceType.Time => LinuxNamespaces.CLONE_NEWTIME,
                _ => throw new ArgumentException($"Unknown namespace type: {ns}")
            };
        }

        // Create namespaces using unshare
        int result = LinuxNamespaces.unshare(flags);
        if (result != 0)
        {
            int errno = Marshal.GetLastPInvokeError();
            throw new InvalidOperationException(
                $"Failed to create namespaces. unshare() returned {result}, errno: {errno}");
        }

        // Open file descriptors for each created namespace
        var fileDescriptors = new Dictionary<NamespaceType, int>();
        var paths = new Dictionary<NamespaceType, string>();
        int pid = LinuxProcess.getpid();
        
        foreach (var ns in namespaceList)
        {
            string path = GetNamespacePath(pid, ns);
            int fd = LinuxNamespaces.open(path, LinuxNamespaces.O_RDONLY, 0);
            if (fd != -1)
            {
                fileDescriptors[ns] = fd;
                paths[ns] = path;
            }
        }
        
        return Task.FromResult(new NamespaceContext
        {
            FileDescriptors = fileDescriptors,
            Paths = paths
        });
    }

    public Task JoinNamespaceAsync(
        string namespacePath,
        NamespaceType namespaceType,
        CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsLinux())
        {
            throw new PlatformNotSupportedException("Linux namespaces are only supported on Linux.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        int flag = namespaceType switch
        {
            NamespaceType.Pid => LinuxNamespaces.CLONE_NEWPID,
            NamespaceType.Network => LinuxNamespaces.CLONE_NEWNET,
            NamespaceType.Mount => LinuxNamespaces.CLONE_NEWNS,
            NamespaceType.Ipc => LinuxNamespaces.CLONE_NEWIPC,
            NamespaceType.Uts => LinuxNamespaces.CLONE_NEWUTS,
            NamespaceType.User => LinuxNamespaces.CLONE_NEWUSER,
            NamespaceType.Cgroup => LinuxNamespaces.CLONE_NEWCGROUP,
            NamespaceType.Time => LinuxNamespaces.CLONE_NEWTIME,
            _ => throw new ArgumentException($"Unknown namespace type: {namespaceType}")
        };

        // Open the namespace file descriptor
        int fd = LinuxNamespaces.open(namespacePath, LinuxNamespaces.O_RDONLY, 0);
        if (fd == -1)
        {
            int errno = Marshal.GetLastPInvokeError();
            throw new InvalidOperationException(
                $"Failed to open namespace at {namespacePath}. open() returned {fd}, errno: {errno}");
        }

        try
        {
            int result = LinuxNamespaces.setns(fd, flag);
            if (result != 0)
            {
                int errno = Marshal.GetLastPInvokeError();
                throw new InvalidOperationException(
                    $"Failed to join namespace. setns() returned {result}, errno: {errno}");
            }
        }
        finally
        {
            LinuxNamespaces.close(fd);
        }

        return Task.CompletedTask;
    }

    public string GetNamespacePath(int pid, NamespaceType namespaceType)
    {
        string nsType = namespaceType switch
        {
            NamespaceType.Pid => "pid",
            NamespaceType.Network => "net",
            NamespaceType.Mount => "mnt",
            NamespaceType.Ipc => "ipc",
            NamespaceType.Uts => "uts",
            NamespaceType.User => "user",
            NamespaceType.Cgroup => "cgroup",
            NamespaceType.Time => "time",
            _ => throw new ArgumentException($"Unknown namespace type: {namespaceType}")
        };

        return $"/proc/{pid}/ns/{nsType}";
    }

    public Task CloseNamespaceAsync(
        int namespaceFd,
        CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsLinux())
        {
            throw new PlatformNotSupportedException("Linux namespaces are only supported on Linux.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        int result = LinuxNamespaces.close(namespaceFd);
        if (result != 0)
        {
            int errno = Marshal.GetLastPInvokeError();
            throw new InvalidOperationException(
                $"Failed to close namespace fd {namespaceFd}. close() returned {result}, errno: {errno}");
        }

        // Remove from tracking
        var toRemove = _namespaceFds.Where(kvp => kvp.Value == namespaceFd).Select(kvp => kvp.Key).ToList();
        foreach (var key in toRemove)
        {
            _namespaceFds.Remove(key);
        }

        return Task.CompletedTask;
    }

    public Task<Dictionary<NamespaceType, string>> GetNamespaceInfoAsync(
        int pid,
        CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsLinux())
        {
            throw new PlatformNotSupportedException("Linux namespaces are only supported on Linux.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var info = new Dictionary<NamespaceType, string>();
        var types = Enum.GetValues<NamespaceType>();

        foreach (var type in types)
        {
            string? nsType = type switch
            {
                NamespaceType.Pid => "pid",
                NamespaceType.Network => "net",
                NamespaceType.Mount => "mnt",
                NamespaceType.Ipc => "ipc",
                NamespaceType.Uts => "uts",
                NamespaceType.User => "user",
                NamespaceType.Cgroup => "cgroup",
                NamespaceType.Time => "time",
                _ => null
            };

            if (nsType == null) continue;

            string nsPath = $"/proc/{pid}/ns/{nsType}";
            if (File.Exists(nsPath))
            {
                string target = File.ResolveLinkTarget(nsPath, false)?.FullName ?? nsPath;
                info[type] = target;
            }
        }

        return Task.FromResult(info);
    }

    public void Dispose()
    {
        // Close all open namespace file descriptors
        foreach (var fd in _namespaceFds.Values)
        {
            try
            {
                LinuxNamespaces.close(fd);
            }
            catch
            {
                // Best effort cleanup
            }
        }
        _namespaceFds.Clear();
    }
}
