namespace DotNetContainerRuntime.Core.Abstractions;

/// <summary>
/// Defines the contract for executing processes in containers
/// </summary>
public interface IProcessManager
{
    /// <summary>
    /// Executes a process inside a container
    /// </summary>
    /// <param name="containerId">Container identifier</param>
    /// <param name="processConfig">Process configuration</param>
    /// <param name="namespaceContext">Namespace context (for Linux)</param>
    /// <param name="resourceContext">Resource control context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Process context with PID</returns>
    Task<ProcessContext> ExecuteAsync(
        string containerId,
        Specifications.ProcessConfiguration processConfig,
        NamespaceContext? namespaceContext,
        ResourceControlContext resourceContext,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Waits for a process to exit
    /// </summary>
    /// <param name="context">Process context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Exit code</returns>
    Task<int> WaitForExitAsync(ProcessContext context, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sends a signal to a process
    /// </summary>
    /// <param name="pid">Process ID</param>
    /// <param name="signal">Signal number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendSignalAsync(int pid, int signal, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if a process is running
    /// </summary>
    /// <param name="pid">Process ID</param>
    /// <returns>True if the process is running</returns>
    bool IsProcessRunning(int pid);
}

/// <summary>
/// Context for a container process
/// </summary>
public sealed class ProcessContext
{
    /// <summary>
    /// Container ID
    /// </summary>
    public required string ContainerId { get; init; }
    
    /// <summary>
    /// Process ID
    /// </summary>
    public required int Pid { get; init; }
    
    /// <summary>
    /// Process start time
    /// </summary>
    public DateTimeOffset StartTime { get; init; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Platform-specific process handle
    /// </summary>
    public nint? Handle { get; init; }
}
