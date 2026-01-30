using System.Runtime.InteropServices;

namespace DotNetContainerRuntime.Windows.Interop;

/// <summary>
/// P/Invoke declarations for Windows Job Objects
/// </summary>
internal static partial class WindowsJobObjects
{
    private const string Kernel32Dll = "kernel32.dll";
    
    /// <summary>
    /// Creates a job object
    /// </summary>
    [LibraryImport(Kernel32Dll, SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    public static partial nint CreateJobObjectW(nint lpJobAttributes, string? lpName);
    
    /// <summary>
    /// Assigns a process to a job object
    /// </summary>
    [LibraryImport(Kernel32Dll, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool AssignProcessToJobObject(nint hJob, nint hProcess);
    
    /// <summary>
    /// Sets information for a job object
    /// </summary>
    [LibraryImport(Kernel32Dll, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetInformationJobObject(
        nint hJob,
        JOBOBJECTINFOCLASS JobObjectInformationClass,
        nint lpJobObjectInformation,
        uint cbJobObjectInformationLength);
    
    /// <summary>
    /// Queries information from a job object
    /// </summary>
    [LibraryImport(Kernel32Dll, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool QueryInformationJobObject(
        nint hJob,
        JOBOBJECTINFOCLASS JobObjectInformationClass,
        nint lpJobObjectInformation,
        uint cbJobObjectInformationLength,
        out uint lpReturnLength);
    
    /// <summary>
    /// Terminates all processes in a job
    /// </summary>
    [LibraryImport(Kernel32Dll, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool TerminateJobObject(nint hJob, uint uExitCode);
    
    /// <summary>
    /// Closes a handle
    /// </summary>
    [LibraryImport(Kernel32Dll, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool CloseHandle(nint hObject);
}

/// <summary>
/// Job object information classes
/// </summary>
public enum JOBOBJECTINFOCLASS
{
    JobObjectBasicAccountingInformation = 1,
    JobObjectBasicLimitInformation = 2,
    JobObjectBasicProcessIdList = 3,
    JobObjectBasicUIRestrictions = 4,
    JobObjectSecurityLimitInformation = 5,
    JobObjectEndOfJobTimeInformation = 6,
    JobObjectAssociateCompletionPortInformation = 7,
    JobObjectBasicAndIoAccountingInformation = 8,
    JobObjectExtendedLimitInformation = 9,
    JobObjectJobSetInformation = 10,
    JobObjectGroupInformation = 11,
    JobObjectNotificationLimitInformation = 12,
    JobObjectLimitViolationInformation = 13,
    JobObjectGroupInformationEx = 14,
    JobObjectCpuRateControlInformation = 15,
    JobObjectNetRateControlInformation = 32,
    JobObjectNotificationLimitInformation2 = 33,
    JobObjectLimitViolationInformation2 = 34
}

/// <summary>
/// Basic limit information for a job object
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct JOBOBJECT_BASIC_LIMIT_INFORMATION
{
    public long PerProcessUserTimeLimit;
    public long PerJobUserTimeLimit;
    public uint LimitFlags;
    public nuint MinimumWorkingSetSize;
    public nuint MaximumWorkingSetSize;
    public uint ActiveProcessLimit;
    public nuint Affinity;
    public uint PriorityClass;
    public uint SchedulingClass;
}

/// <summary>
/// Extended limit information for a job object
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
{
    public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
    public IO_COUNTERS IoInfo;
    public nuint ProcessMemoryLimit;
    public nuint JobMemoryLimit;
    public nuint PeakProcessMemoryUsed;
    public nuint PeakJobMemoryUsed;
}

/// <summary>
/// I/O counters
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct IO_COUNTERS
{
    public ulong ReadOperationCount;
    public ulong WriteOperationCount;
    public ulong OtherOperationCount;
    public ulong ReadTransferCount;
    public ulong WriteTransferCount;
    public ulong OtherTransferCount;
}

/// <summary>
/// CPU rate control information
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct JOBOBJECT_CPU_RATE_CONTROL_INFORMATION
{
    public uint ControlFlags;
    public uint Value; // Union: CpuRate, Weight, or MinRate/MaxRate
}

// Job object limit flags
public static class JobObjectLimitFlags
{
    public const uint JOB_OBJECT_LIMIT_WORKINGSET = 0x00000001;
    public const uint JOB_OBJECT_LIMIT_PROCESS_TIME = 0x00000002;
    public const uint JOB_OBJECT_LIMIT_JOB_TIME = 0x00000004;
    public const uint JOB_OBJECT_LIMIT_ACTIVE_PROCESS = 0x00000008;
    public const uint JOB_OBJECT_LIMIT_AFFINITY = 0x00000010;
    public const uint JOB_OBJECT_LIMIT_PRIORITY_CLASS = 0x00000020;
    public const uint JOB_OBJECT_LIMIT_PRESERVE_JOB_TIME = 0x00000040;
    public const uint JOB_OBJECT_LIMIT_SCHEDULING_CLASS = 0x00000080;
    public const uint JOB_OBJECT_LIMIT_PROCESS_MEMORY = 0x00000100;
    public const uint JOB_OBJECT_LIMIT_JOB_MEMORY = 0x00000200;
    public const uint JOB_OBJECT_LIMIT_DIE_ON_UNHANDLED_EXCEPTION = 0x00000400;
    public const uint JOB_OBJECT_LIMIT_BREAKAWAY_OK = 0x00000800;
    public const uint JOB_OBJECT_LIMIT_SILENT_BREAKAWAY_OK = 0x00001000;
    public const uint JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x00002000;
}
