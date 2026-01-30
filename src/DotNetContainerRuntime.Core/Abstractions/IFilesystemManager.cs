namespace DotNetContainerRuntime.Core.Abstractions;

/// <summary>
/// Defines the contract for managing container filesystems
/// </summary>
public interface IFilesystemManager
{
    /// <summary>
    /// Prepares the root filesystem for a container
    /// </summary>
    /// <param name="containerId">Container identifier</param>
    /// <param name="rootfsPath">Path to the rootfs directory</param>
    /// <param name="readOnly">Whether the rootfs should be read-only</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<FilesystemContext> PrepareRootfsAsync(string containerId, string rootfsPath, bool readOnly, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Mounts filesystems for a container
    /// </summary>
    /// <param name="context">Filesystem context</param>
    /// <param name="mounts">Mount configurations</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task MountFilesystemsAsync(FilesystemContext context, IEnumerable<Specifications.MountConfiguration> mounts, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Changes the root filesystem (pivot_root or chroot)
    /// </summary>
    /// <param name="context">Filesystem context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PivotRootAsync(FilesystemContext context, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Unmounts filesystems and cleans up
    /// </summary>
    /// <param name="context">Filesystem context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task CleanupAsync(FilesystemContext context, CancellationToken cancellationToken = default);
}

/// <summary>
/// Context for container filesystem operations
/// </summary>
public sealed class FilesystemContext
{
    /// <summary>
    /// Container ID
    /// </summary>
    public required string ContainerId { get; init; }
    
    /// <summary>
    /// Path to the merged/mounted rootfs
    /// </summary>
    public required string MountPath { get; init; }
    
    /// <summary>
    /// Path to the upper layer (if using overlay)
    /// </summary>
    public string? UpperPath { get; init; }
    
    /// <summary>
    /// Path to the work directory (if using overlay)
    /// </summary>
    public string? WorkPath { get; init; }
    
    /// <summary>
    /// Whether the rootfs is read-only
    /// </summary>
    public bool ReadOnly { get; init; }
    
    /// <summary>
    /// List of active mount points
    /// </summary>
    public List<string> MountPoints { get; init; } = new();
}
