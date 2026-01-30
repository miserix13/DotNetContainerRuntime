namespace DotNetContainerRuntime.Core.Abstractions;

/// <summary>
/// Defines the contract for managing cgroups (Linux) or job objects (Windows)
/// </summary>
public interface IResourceController
{
    /// <summary>
    /// Creates a new resource control group
    /// </summary>
    /// <param name="containerId">Container identifier</param>
    /// <param name="resources">Resource limits to apply</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<ResourceControlContext> CreateResourceGroupAsync(string containerId, ResourceLimits resources, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adds a process to a resource control group
    /// </summary>
    /// <param name="context">Resource control context</param>
    /// <param name="pid">Process ID to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddProcessAsync(ResourceControlContext context, int pid, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates resource limits for a control group
    /// </summary>
    /// <param name="context">Resource control context</param>
    /// <param name="resources">New resource limits</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdateLimitsAsync(ResourceControlContext context, ResourceLimits resources, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Removes a resource control group
    /// </summary>
    /// <param name="context">Resource control context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteResourceGroupAsync(ResourceControlContext context, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets resource usage statistics
    /// </summary>
    /// <param name="context">Resource control context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<ResourceUsage> GetUsageAsync(ResourceControlContext context, CancellationToken cancellationToken = default);
}

/// <summary>
/// Context for a resource control group
/// </summary>
public sealed class ResourceControlContext
{
    /// <summary>
    /// Container ID
    /// </summary>
    public required string ContainerId { get; init; }
    
    /// <summary>
    /// Path to the cgroup or handle to job object
    /// </summary>
    public required string Path { get; init; }
    
    /// <summary>
    /// Platform-specific handle (IntPtr for Windows job objects)
    /// </summary>
    public nint? Handle { get; init; }
}

/// <summary>
/// Resource limits to apply
/// </summary>
public sealed class ResourceLimits
{
    /// <summary>
    /// Memory limit in bytes
    /// </summary>
    public long? MemoryLimit { get; init; }
    
    /// <summary>
    /// Memory reservation (soft limit) in bytes
    /// </summary>
    public long? MemoryReservation { get; init; }
    
    /// <summary>
    /// Memory + swap limit in bytes
    /// </summary>
    public long? MemorySwap { get; init; }
    
    /// <summary>
    /// CPU shares (relative weight)
    /// </summary>
    public ulong? CpuShares { get; init; }
    
    /// <summary>
    /// CPU quota in microseconds
    /// </summary>
    public long? CpuQuota { get; init; }
    
    /// <summary>
    /// CPU period in microseconds
    /// </summary>
    public ulong? CpuPeriod { get; init; }
    
    /// <summary>
    /// CPUs to use (e.g., "0-3" or "0,1,4")
    /// </summary>
    public string? CpuSet { get; init; }
    
    /// <summary>
    /// Block I/O weight
    /// </summary>
    public ushort? BlockIOWeight { get; init; }
    
    /// <summary>
    /// Maximum number of PIDs
    /// </summary>
    public long? PidsLimit { get; init; }
}

/// <summary>
/// Resource usage statistics
/// </summary>
public sealed class ResourceUsage
{
    /// <summary>
    /// Memory usage in bytes
    /// </summary>
    public long MemoryUsage { get; init; }
    
    /// <summary>
    /// Maximum memory usage in bytes
    /// </summary>
    public long MemoryMaxUsage { get; init; }
    
    /// <summary>
    /// CPU usage in nanoseconds
    /// </summary>
    public ulong CpuUsage { get; init; }
    
    /// <summary>
    /// Number of processes
    /// </summary>
    public int ProcessCount { get; init; }
}
