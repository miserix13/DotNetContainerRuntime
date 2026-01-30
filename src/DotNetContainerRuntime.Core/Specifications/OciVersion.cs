namespace DotNetContainerRuntime.Core.Specifications;

/// <summary>
/// OCI specification version information
/// </summary>
public static class OciVersion
{
    /// <summary>
    /// OCI Runtime Specification version implemented by this runtime
    /// </summary>
    public const string RuntimeSpec = "1.2.0";
    
    /// <summary>
    /// OCI Image Specification version supported by this runtime
    /// </summary>
    public const string ImageSpec = "1.1.0";
}
