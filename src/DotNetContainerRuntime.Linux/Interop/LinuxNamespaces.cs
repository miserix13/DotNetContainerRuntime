using System.Runtime.InteropServices;

namespace DotNetContainerRuntime.Linux.Interop;

/// <summary>
/// P/Invoke declarations for Linux namespace system calls
/// </summary>
internal static partial class LinuxNamespaces
{
    // Namespace flags for clone/unshare
    public const int CLONE_NEWNS = 0x00020000;     // Mount namespace
    public const int CLONE_NEWUTS = 0x04000000;    // UTS namespace
    public const int CLONE_NEWIPC = 0x08000000;    // IPC namespace
    public const int CLONE_NEWUSER = 0x10000000;   // User namespace
    public const int CLONE_NEWPID = 0x20000000;    // PID namespace
    public const int CLONE_NEWNET = 0x40000000;    // Network namespace
    public const int CLONE_NEWCGROUP = 0x02000000; // Cgroup namespace
    public const int CLONE_NEWTIME = 0x00000080;   // Time namespace
    
    /// <summary>
    /// Disassociate parts of the process execution context
    /// </summary>
    [LibraryImport("libc", SetLastError = true)]
    public static partial int unshare(int flags);
    
    /// <summary>
    /// Reassociate thread with a namespace
    /// </summary>
    [LibraryImport("libc", SetLastError = true)]
    public static partial int setns(int fd, int nstype);
    
    /// <summary>
    /// Open a file descriptor
    /// </summary>
    [LibraryImport("libc", SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    public static partial int open(string pathname, int flags, int mode);
    
    /// <summary>
    /// Close a file descriptor
    /// </summary>
    [LibraryImport("libc", SetLastError = true)]
    public static partial int close(int fd);
    
    // File open flags
    public const int O_RDONLY = 0x0000;
    public const int O_CLOEXEC = 0x80000;
}
