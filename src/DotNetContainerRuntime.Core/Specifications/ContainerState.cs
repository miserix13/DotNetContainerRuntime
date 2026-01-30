using System.Text.Json.Serialization;

namespace DotNetContainerRuntime.Core.Specifications;

/// <summary>
/// Represents the runtime state of a container (OCI Runtime Spec)
/// </summary>
public sealed class ContainerState
{
    /// <summary>
    /// OCI specification version
    /// </summary>
    [JsonPropertyName("ociVersion")]
    public required string OciVersion { get; init; }
    
    /// <summary>
    /// Container ID
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }
    
    /// <summary>
    /// Runtime status of the container
    /// </summary>
    [JsonPropertyName("status")]
    [JsonConverter(typeof(JsonStringEnumConverter<ContainerStatus>))]
    public required ContainerStatus Status { get; init; }
    
    /// <summary>
    /// Process ID of the container process on the host (if running)
    /// </summary>
    [JsonPropertyName("pid")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Pid { get; init; }
    
    /// <summary>
    /// Absolute path to the container bundle directory
    /// </summary>
    [JsonPropertyName("bundle")]
    public required string Bundle { get; init; }
    
    /// <summary>
    /// Additional runtime-specific annotations
    /// </summary>
    [JsonPropertyName("annotations")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string>? Annotations { get; init; }
}

/// <summary>
/// Container runtime status
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<ContainerStatus>))]
public enum ContainerStatus
{
    /// <summary>
    /// Container is being created
    /// </summary>
    Creating,
    
    /// <summary>
    /// Container has been created but not started
    /// </summary>
    Created,
    
    /// <summary>
    /// Container process is running
    /// </summary>
    Running,
    
    /// <summary>
    /// Container process has stopped
    /// </summary>
    Stopped
}
