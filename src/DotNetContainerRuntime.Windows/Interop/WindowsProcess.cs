using System.Runtime.InteropServices;

namespace DotNetContainerRuntime.Windows.Interop;

/// <summary>
/// P/Invoke declarations for Windows process operations
/// </summary>
internal static partial class WindowsProcess
{
    private const string Kernel32Dll = "kernel32.dll";
    
    /// <summary>
    /// Creates a new process
    /// </summary>
    [DllImport(Kernel32Dll, SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CreateProcessW(
        string? lpApplicationName,
        string? lpCommandLine,
        nint lpProcessAttributes,
        nint lpThreadAttributes,
        [MarshalAs(UnmanagedType.Bool)] bool bInheritHandles,
        uint dwCreationFlags,
        nint lpEnvironment,
        string? lpCurrentDirectory,
        ref STARTUPINFO lpStartupInfo,
        out PROCESS_INFORMATION lpProcessInformation);
    
    /// <summary>
    /// Opens an existing process
    /// </summary>
    [LibraryImport(Kernel32Dll, SetLastError = true)]
    public static partial nint OpenProcess(uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint dwProcessId);
    
    /// <summary>
    /// Terminates a process
    /// </summary>
    [LibraryImport(Kernel32Dll, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool TerminateProcess(nint hProcess, uint uExitCode);
    
    /// <summary>
    /// Gets the exit code of a process
    /// </summary>
    [LibraryImport(Kernel32Dll, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetExitCodeProcess(nint hProcess, out uint lpExitCode);
    
    /// <summary>
    /// Waits for a process to complete
    /// </summary>
    [LibraryImport(Kernel32Dll, SetLastError = true)]
    public static partial uint WaitForSingleObject(nint hHandle, uint dwMilliseconds);
    
    /// <summary>
    /// Gets the current process ID
    /// </summary>
    [LibraryImport(Kernel32Dll)]
    public static partial uint GetCurrentProcessId();
}

/// <summary>
/// Startup information for a process
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct STARTUPINFO
{
    public uint cb;
    public string? lpReserved;
    public string? lpDesktop;
    public string? lpTitle;
    public uint dwX;
    public uint dwY;
    public uint dwXSize;
    public uint dwYSize;
    public uint dwXCountChars;
    public uint dwYCountChars;
    public uint dwFillAttribute;
    public uint dwFlags;
    public ushort wShowWindow;
    public ushort cbReserved2;
    public nint lpReserved2;
    public nint hStdInput;
    public nint hStdOutput;
    public nint hStdError;
}

/// <summary>
/// Process information
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct PROCESS_INFORMATION
{
    public nint hProcess;
    public nint hThread;
    public uint dwProcessId;
    public uint dwThreadId;
}

// Process creation flags
public static class ProcessCreationFlags
{
    public const uint CREATE_SUSPENDED = 0x00000004;
    public const uint CREATE_NEW_CONSOLE = 0x00000010;
    public const uint CREATE_NO_WINDOW = 0x08000000;
    public const uint CREATE_BREAKAWAY_FROM_JOB = 0x01000000;
}

// Process access rights
public static class ProcessAccessRights
{
    public const uint PROCESS_TERMINATE = 0x0001;
    public const uint PROCESS_CREATE_THREAD = 0x0002;
    public const uint PROCESS_SET_SESSIONID = 0x0004;
    public const uint PROCESS_VM_OPERATION = 0x0008;
    public const uint PROCESS_VM_READ = 0x0010;
    public const uint PROCESS_VM_WRITE = 0x0020;
    public const uint PROCESS_DUP_HANDLE = 0x0040;
    public const uint PROCESS_CREATE_PROCESS = 0x0080;
    public const uint PROCESS_SET_QUOTA = 0x0100;
    public const uint PROCESS_SET_INFORMATION = 0x0200;
    public const uint PROCESS_QUERY_INFORMATION = 0x0400;
    public const uint PROCESS_SUSPEND_RESUME = 0x0800;
    public const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;
    public const uint PROCESS_ALL_ACCESS = 0x1F0FFF;
}

// Wait results
public static class WaitResults
{
    public const uint WAIT_OBJECT_0 = 0x00000000;
    public const uint WAIT_ABANDONED = 0x00000080;
    public const uint WAIT_TIMEOUT = 0x00000102;
    public const uint WAIT_FAILED = 0xFFFFFFFF;
    public const uint INFINITE = 0xFFFFFFFF;
}
