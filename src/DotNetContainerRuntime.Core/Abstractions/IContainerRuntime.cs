namespace DotNetContainerRuntime.Core.Abstractions;

/// <summary>
/// Defines the contract for a container runtime that manages container lifecycle
/// </summary>
public interface IContainerRuntime
{
    /// <summary>
    /// Creates a new container instance from a bundle
    /// </summary>
    /// <param name="containerId">Unique identifier for the container</param>
    /// <param name="bundlePath">Path to the OCI bundle directory</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task CreateAsync(string containerId, string bundlePath, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Starts the user-specified process in a created container
    /// </summary>
    /// <param name="containerId">Container identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task StartAsync(string containerId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sends a signal to the container process
    /// </summary>
    /// <param name="containerId">Container identifier</param>
    /// <param name="signal">Signal to send (e.g., SIGTERM, SIGKILL)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task KillAsync(string containerId, int signal, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a container and its resources
    /// </summary>
    /// <param name="containerId">Container identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteAsync(string containerId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the current state of a container
    /// </summary>
    /// <param name="containerId">Container identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Container state information</returns>
    Task<Specifications.ContainerState> GetStateAsync(string containerId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lists all containers managed by this runtime
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of container IDs</returns>
    Task<IReadOnlyCollection<string>> ListAsync(CancellationToken cancellationToken = default);
}
