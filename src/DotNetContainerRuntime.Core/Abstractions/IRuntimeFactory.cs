namespace DotNetContainerRuntime.Core.Abstractions;

/// <summary>
/// Factory for creating platform-specific runtime implementations
/// </summary>
public interface IRuntimeFactory
{
    /// <summary>
    /// Creates a container runtime instance for the current platform
    /// </summary>
    /// <returns>Platform-specific container runtime</returns>
    IContainerRuntime CreateRuntime();
    
    /// <summary>
    /// Creates a namespace manager (Linux only)
    /// </summary>
    /// <returns>Namespace manager or null on Windows</returns>
    INamespaceManager? CreateNamespaceManager();
    
    /// <summary>
    /// Creates a resource controller (cgroups on Linux, job objects on Windows)
    /// </summary>
    /// <returns>Platform-specific resource controller</returns>
    IResourceController CreateResourceController();
    
    /// <summary>
    /// Creates a filesystem manager
    /// </summary>
    /// <returns>Platform-specific filesystem manager</returns>
    IFilesystemManager CreateFilesystemManager();
    
    /// <summary>
    /// Creates a process manager
    /// </summary>
    /// <returns>Platform-specific process manager</returns>
    IProcessManager CreateProcessManager();
    
    /// <summary>
    /// Gets the current platform
    /// </summary>
    RuntimePlatform Platform { get; }
}

/// <summary>
/// Supported runtime platforms
/// </summary>
public enum RuntimePlatform
{
    /// <summary>
    /// Linux operating system
    /// </summary>
    Linux,
    
    /// <summary>
    /// Windows operating system
    /// </summary>
    Windows
}
