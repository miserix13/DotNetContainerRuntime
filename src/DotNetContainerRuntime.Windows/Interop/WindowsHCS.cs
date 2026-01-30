using System.Runtime.InteropServices;

namespace DotNetContainerRuntime.Windows.Interop;

/// <summary>
/// P/Invoke declarations for Windows Host Compute Service (HCS)
/// </summary>
internal static partial class WindowsHCS
{
    private const string ComputeCoreDll = "computecore.dll";
    
    /// <summary>
    /// Creates a compute system (container)
    /// </summary>
    [LibraryImport(ComputeCoreDll, SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    public static partial int HcsCreateComputeSystem(
        string id,
        string configuration,
        nint identity,
        nint securityDescriptor,
        out nint computeSystem,
        out nint result);
    
    /// <summary>
    /// Starts a compute system
    /// </summary>
    [LibraryImport(ComputeCoreDll, SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    public static partial int HcsStartComputeSystem(
        nint computeSystem,
        string? options,
        out nint result);
    
    /// <summary>
    /// Shuts down a compute system
    /// </summary>
    [LibraryImport(ComputeCoreDll, SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    public static partial int HcsShutdownComputeSystem(
        nint computeSystem,
        string? options,
        out nint result);
    
    /// <summary>
    /// Terminates a compute system
    /// </summary>
    [LibraryImport(ComputeCoreDll, SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    public static partial int HcsTerminateComputeSystem(
        nint computeSystem,
        string? options,
        out nint result);
    
    /// <summary>
    /// Gets the properties of a compute system
    /// </summary>
    [LibraryImport(ComputeCoreDll, SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    public static partial int HcsGetComputeSystemProperties(
        nint computeSystem,
        string? propertyQuery,
        out nint properties,
        out nint result);
    
    /// <summary>
    /// Closes a compute system handle
    /// </summary>
    [LibraryImport(ComputeCoreDll, SetLastError = true)]
    public static partial int HcsCloseComputeSystem(nint computeSystem);
    
    /// <summary>
    /// Creates a process in a compute system
    /// </summary>
    [LibraryImport(ComputeCoreDll, SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    public static partial int HcsCreateProcess(
        nint computeSystem,
        string processParameters,
        nint processInformation,
        nint securityDescriptor,
        out nint process,
        out nint result);
    
    /// <summary>
    /// Terminates a process in a compute system
    /// </summary>
    [LibraryImport(ComputeCoreDll, SetLastError = true)]
    public static partial int HcsTerminateProcess(
        nint process,
        out nint result);
    
    /// <summary>
    /// Closes a process handle
    /// </summary>
    [LibraryImport(ComputeCoreDll, SetLastError = true)]
    public static partial int HcsCloseProcess(nint process);
    
    /// <summary>
    /// Waits for a process to exit
    /// </summary>
    [LibraryImport(ComputeCoreDll, SetLastError = true)]
    public static partial int HcsWaitForProcessExit(
        nint process,
        uint timeoutMs,
        out nint result);
    
    /// <summary>
    /// Gets process information
    /// </summary>
    [LibraryImport(ComputeCoreDll, SetLastError = true)]
    public static partial int HcsGetProcessInfo(
        nint process,
        out nint processInformation,
        out nint result);
    
    /// <summary>
    /// Frees memory allocated by HCS
    /// </summary>
    [LibraryImport(ComputeCoreDll, SetLastError = true)]
    public static partial void HcsFreeMemory(nint buffer);
}
