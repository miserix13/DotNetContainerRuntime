using System.Runtime.InteropServices;

namespace DotNetContainerRuntime.Linux.Interop;

/// <summary>
/// P/Invoke declarations for Linux process operations
/// </summary>
internal static partial class LinuxProcess
{
    // Clone flags
    public const int SIGCHLD = 17;
    public const int CLONE_VM = 0x00000100;         // Share VM
    public const int CLONE_FS = 0x00000200;         // Share filesystem info
    public const int CLONE_FILES = 0x00000400;      // Share file descriptors
    public const int CLONE_SIGHAND = 0x00000800;    // Share signal handlers
    public const int CLONE_PARENT = 0x00008000;     // Parent of new process is same as caller
    public const int CLONE_THREAD = 0x00010000;     // Same thread group
    public const int CLONE_VFORK = 0x00004000;      // vfork semantics
    
    /// <summary>
    /// Create a child process
    /// </summary>
    [LibraryImport("libc", SetLastError = true)]
    public static partial int fork();
    
    /// <summary>
    /// Execute a program
    /// </summary>
    [LibraryImport("libc", SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    public static partial int execve(string filename, nint argv, nint envp);
    
    /// <summary>
    /// Send a signal to a process
    /// </summary>
    [LibraryImport("libc", SetLastError = true)]
    public static partial int kill(int pid, int sig);
    
    /// <summary>
    /// Wait for process to change state
    /// </summary>
    [LibraryImport("libc", SetLastError = true)]
    public static partial int waitpid(int pid, out int status, int options);
    
    /// <summary>
    /// Get process ID
    /// </summary>
    [LibraryImport("libc")]
    public static partial int getpid();
    
    /// <summary>
    /// Get parent process ID
    /// </summary>
    [LibraryImport("libc")]
    public static partial int getppid();
    
    /// <summary>
    /// Set user ID
    /// </summary>
    [LibraryImport("libc", SetLastError = true)]
    public static partial int setuid(uint uid);
    
    /// <summary>
    /// Set group ID
    /// </summary>
    [LibraryImport("libc", SetLastError = true)]
    public static partial int setgid(uint gid);
    
    /// <summary>
    /// Set supplementary group IDs
    /// </summary>
    [LibraryImport("libc", SetLastError = true)]
    public static partial int setgroups(int size, uint[] list);
    
    /// <summary>
    /// Exit the process
    /// </summary>
    [LibraryImport("libc")]
    public static partial void exit(int status);
    
    // Signal numbers
    public const int SIGTERM = 15;
    public const int SIGKILL = 9;
    public const int SIGINT = 2;
    public const int SIGHUP = 1;
    
    // Wait options
    public const int WNOHANG = 1;
    public const int WUNTRACED = 2;
}
