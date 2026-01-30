using System.Runtime.InteropServices;

namespace DotNetContainerRuntime.Linux.Interop;

/// <summary>
/// P/Invoke declarations for Linux mount operations
/// </summary>
internal static partial class LinuxMount
{
    // Mount flags
    public const ulong MS_RDONLY = 1;           // Mount read-only
    public const ulong MS_NOSUID = 2;           // Ignore suid and sgid bits
    public const ulong MS_NODEV = 4;            // Disallow access to device special files
    public const ulong MS_NOEXEC = 8;           // Disallow program execution
    public const ulong MS_SYNCHRONOUS = 16;     // Writes are synced at once
    public const ulong MS_REMOUNT = 32;         // Alter flags of a mounted FS
    public const ulong MS_BIND = 4096;          // Bind directory at different place
    public const ulong MS_MOVE = 8192;          // Move a subtree
    public const ulong MS_REC = 16384;          // Recursive bind mount
    public const ulong MS_PRIVATE = 1 << 18;    // Change to private
    public const ulong MS_SLAVE = 1 << 19;      // Change to slave
    public const ulong MS_SHARED = 1 << 20;     // Change to shared
    
    /// <summary>
    /// Mount a filesystem
    /// </summary>
    [LibraryImport("libc", SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    public static partial int mount(
        string? source,
        string target,
        string? filesystemtype,
        ulong mountflags,
        nint data);
    
    /// <summary>
    /// Unmount a filesystem
    /// </summary>
    [LibraryImport("libc", SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    public static partial int umount(string target);
    
    /// <summary>
    /// Unmount a filesystem with flags
    /// </summary>
    [LibraryImport("libc", SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    public static partial int umount2(string target, int flags);
    
    /// <summary>
    /// Change the root filesystem
    /// </summary>
    [LibraryImport("libc", SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    public static partial int pivot_root(string new_root, string put_old);
    
    /// <summary>
    /// Change the root directory
    /// </summary>
    [LibraryImport("libc", SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    public static partial int chroot(string path);
    
    /// <summary>
    /// Change current working directory
    /// </summary>
    [LibraryImport("libc", SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    public static partial int chdir(string path);
    
    // Unmount flags
    public const int MNT_FORCE = 1;      // Force unmount
    public const int MNT_DETACH = 2;     // Lazy unmount
    public const int MNT_EXPIRE = 4;     // Mark for expiry
    public const int UMOUNT_NOFOLLOW = 8; // Don't follow symlinks
}
