using System.Runtime.InteropServices;

namespace DotNetContainerRuntime.Linux.Interop;

/// <summary>
/// P/Invoke declarations for Linux capabilities
/// </summary>
internal static partial class LinuxCapabilities
{
    // Capability constants
    public const int CAP_CHOWN = 0;
    public const int CAP_DAC_OVERRIDE = 1;
    public const int CAP_DAC_READ_SEARCH = 2;
    public const int CAP_FOWNER = 3;
    public const int CAP_FSETID = 4;
    public const int CAP_KILL = 5;
    public const int CAP_SETGID = 6;
    public const int CAP_SETUID = 7;
    public const int CAP_SETPCAP = 8;
    public const int CAP_NET_BIND_SERVICE = 10;
    public const int CAP_NET_RAW = 13;
    public const int CAP_SYS_CHROOT = 18;
    public const int CAP_SYS_ADMIN = 21;
    public const int CAP_AUDIT_WRITE = 29;
    public const int CAP_SETFCAP = 31;
    
    /// <summary>
    /// Set process capabilities
    /// </summary>
    [LibraryImport("cap", SetLastError = true)]
    public static partial int cap_set_proc(nint cap_p);
    
    /// <summary>
    /// Get process capabilities
    /// </summary>
    [LibraryImport("cap", SetLastError = true)]
    public static partial nint cap_get_proc();
    
    /// <summary>
    /// Free capability structure
    /// </summary>
    [LibraryImport("cap", SetLastError = true)]
    public static partial int cap_free(nint obj_d);
    
    /// <summary>
    /// Set no new privileges flag
    /// </summary>
    [LibraryImport("libc", SetLastError = true)]
    public static partial int prctl(int option, ulong arg2, ulong arg3, ulong arg4, ulong arg5);
    
    // prctl options
    public const int PR_SET_NO_NEW_PRIVS = 38;
    public const int PR_GET_NO_NEW_PRIVS = 39;
}
