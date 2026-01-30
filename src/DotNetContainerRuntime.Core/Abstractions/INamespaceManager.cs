namespace DotNetContainerRuntime.Core.Abstractions;

/// <summary>
/// Defines the contract for managing Linux namespaces
/// </summary>
public interface INamespaceManager
{
    /// <summary>
    /// Creates and enters the specified namespaces
    /// </summary>
    /// <param name="namespaces">List of namespace types to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<NamespaceContext> CreateNamespacesAsync(IEnumerable<NamespaceType> namespaces, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Joins an existing namespace
    /// </summary>
    /// <param name="namespacePath">Path to the namespace file descriptor</param>
    /// <param name="namespaceType">Type of namespace</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task JoinNamespaceAsync(string namespacePath, NamespaceType namespaceType, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the path to a process's namespace
    /// </summary>
    /// <param name="pid">Process ID</param>
    /// <param name="namespaceType">Namespace type</param>
    /// <returns>Path to namespace file</returns>
    string GetNamespacePath(int pid, NamespaceType namespaceType);
}

/// <summary>
/// Context information for created namespaces
/// </summary>
public sealed class NamespaceContext
{
    /// <summary>
    /// File descriptors for created namespaces
    /// </summary>
    public required Dictionary<NamespaceType, int> FileDescriptors { get; init; }
    
    /// <summary>
    /// Paths to namespace files
    /// </summary>
    public required Dictionary<NamespaceType, string> Paths { get; init; }
}

/// <summary>
/// Linux namespace types
/// </summary>
public enum NamespaceType
{
    /// <summary>
    /// PID namespace - Process isolation
    /// </summary>
    Pid,
    
    /// <summary>
    /// Network namespace - Network stack isolation
    /// </summary>
    Network,
    
    /// <summary>
    /// Mount namespace - Filesystem mount isolation
    /// </summary>
    Mount,
    
    /// <summary>
    /// IPC namespace - Inter-process communication isolation
    /// </summary>
    Ipc,
    
    /// <summary>
    /// UTS namespace - Hostname isolation
    /// </summary>
    Uts,
    
    /// <summary>
    /// User namespace - UID/GID mapping
    /// </summary>
    User,
    
    /// <summary>
    /// Cgroup namespace - Cgroup hierarchy isolation
    /// </summary>
    Cgroup,
    
    /// <summary>
    /// Time namespace - System time isolation
    /// </summary>
    Time
}
