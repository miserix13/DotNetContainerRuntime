using System.Text.Json.Serialization;

namespace DotNetContainerRuntime.Core.Specifications;

/// <summary>
/// OCI Runtime Specification config.json
/// </summary>
public sealed class RuntimeConfiguration
{
    /// <summary>
    /// OCI specification version
    /// </summary>
    [JsonPropertyName("ociVersion")]
    public required string OciVersion { get; init; }
    
    /// <summary>
    /// Root filesystem configuration
    /// </summary>
    [JsonPropertyName("root")]
    public RootConfiguration? Root { get; init; }
    
    /// <summary>
    /// Process configuration
    /// </summary>
    [JsonPropertyName("process")]
    public ProcessConfiguration? Process { get; init; }
    
    /// <summary>
    /// Hostname for the container
    /// </summary>
    [JsonPropertyName("hostname")]
    public string? Hostname { get; init; }
    
    /// <summary>
    /// Mount points to configure in the container
    /// </summary>
    [JsonPropertyName("mounts")]
    public List<MountConfiguration>? Mounts { get; init; }
    
    /// <summary>
    /// Hooks for lifecycle events
    /// </summary>
    [JsonPropertyName("hooks")]
    public HooksConfiguration? Hooks { get; init; }
    
    /// <summary>
    /// Arbitrary metadata
    /// </summary>
    [JsonPropertyName("annotations")]
    public Dictionary<string, string>? Annotations { get; init; }
    
    /// <summary>
    /// Linux-specific configuration
    /// </summary>
    [JsonPropertyName("linux")]
    public LinuxConfiguration? Linux { get; init; }
    
    /// <summary>
    /// Windows-specific configuration
    /// </summary>
    [JsonPropertyName("windows")]
    public WindowsConfiguration? Windows { get; init; }
}

/// <summary>
/// Root filesystem configuration
/// </summary>
public sealed class RootConfiguration
{
    /// <summary>
    /// Path to the root filesystem
    /// </summary>
    [JsonPropertyName("path")]
    public required string Path { get; init; }
    
    /// <summary>
    /// Whether the root filesystem should be read-only
    /// </summary>
    [JsonPropertyName("readonly")]
    public bool ReadOnly { get; init; }
}

/// <summary>
/// Process configuration
/// </summary>
public sealed class ProcessConfiguration
{
    /// <summary>
    /// Whether the process runs in a terminal
    /// </summary>
    [JsonPropertyName("terminal")]
    public bool Terminal { get; init; }
    
    /// <summary>
    /// Console size (if terminal is true)
    /// </summary>
    [JsonPropertyName("consoleSize")]
    public ConsoleSizeConfiguration? ConsoleSize { get; init; }
    
    /// <summary>
    /// User and group information
    /// </summary>
    [JsonPropertyName("user")]
    public required UserConfiguration User { get; init; }
    
    /// <summary>
    /// Arguments array for the process
    /// </summary>
    [JsonPropertyName("args")]
    public List<string>? Args { get; init; }
    
    /// <summary>
    /// Environment variables
    /// </summary>
    [JsonPropertyName("env")]
    public List<string>? Env { get; init; }
    
    /// <summary>
    /// Working directory
    /// </summary>
    [JsonPropertyName("cwd")]
    public required string Cwd { get; init; }
    
    /// <summary>
    /// Capabilities for the process (Linux)
    /// </summary>
    [JsonPropertyName("capabilities")]
    public CapabilitiesConfiguration? Capabilities { get; init; }
    
    /// <summary>
    /// Resource limits
    /// </summary>
    [JsonPropertyName("rlimits")]
    public List<RlimitConfiguration>? Rlimits { get; init; }
    
    /// <summary>
    /// Disable gaining additional privileges
    /// </summary>
    [JsonPropertyName("noNewPrivileges")]
    public bool NoNewPrivileges { get; init; }
    
    /// <summary>
    /// AppArmor profile
    /// </summary>
    [JsonPropertyName("apparmorProfile")]
    public string? ApparmorProfile { get; init; }
    
    /// <summary>
    /// SELinux label
    /// </summary>
    [JsonPropertyName("selinuxLabel")]
    public string? SelinuxLabel { get; init; }
}

/// <summary>
/// Console size configuration
/// </summary>
public sealed class ConsoleSizeConfiguration
{
    [JsonPropertyName("height")]
    public uint Height { get; init; }
    
    [JsonPropertyName("width")]
    public uint Width { get; init; }
}

/// <summary>
/// User and group configuration
/// </summary>
public sealed class UserConfiguration
{
    [JsonPropertyName("uid")]
    public uint Uid { get; init; }
    
    [JsonPropertyName("gid")]
    public uint Gid { get; init; }
    
    [JsonPropertyName("additionalGids")]
    public List<uint>? AdditionalGids { get; init; }
    
    [JsonPropertyName("username")]
    public string? Username { get; init; }
}

/// <summary>
/// Linux capabilities configuration
/// </summary>
public sealed class CapabilitiesConfiguration
{
    [JsonPropertyName("effective")]
    public List<string>? Effective { get; init; }
    
    [JsonPropertyName("bounding")]
    public List<string>? Bounding { get; init; }
    
    [JsonPropertyName("inheritable")]
    public List<string>? Inheritable { get; init; }
    
    [JsonPropertyName("permitted")]
    public List<string>? Permitted { get; init; }
    
    [JsonPropertyName("ambient")]
    public List<string>? Ambient { get; init; }
}

/// <summary>
/// Resource limit configuration
/// </summary>
public sealed class RlimitConfiguration
{
    [JsonPropertyName("type")]
    public required string Type { get; init; }
    
    [JsonPropertyName("hard")]
    public ulong Hard { get; init; }
    
    [JsonPropertyName("soft")]
    public ulong Soft { get; init; }
}

/// <summary>
/// Mount configuration
/// </summary>
public sealed class MountConfiguration
{
    [JsonPropertyName("destination")]
    public required string Destination { get; init; }
    
    [JsonPropertyName("source")]
    public string? Source { get; init; }
    
    [JsonPropertyName("type")]
    public string? Type { get; init; }
    
    [JsonPropertyName("options")]
    public List<string>? Options { get; init; }
}

/// <summary>
/// Lifecycle hooks configuration
/// </summary>
public sealed class HooksConfiguration
{
    [JsonPropertyName("prestart")]
    public List<HookConfiguration>? Prestart { get; init; }
    
    [JsonPropertyName("createRuntime")]
    public List<HookConfiguration>? CreateRuntime { get; init; }
    
    [JsonPropertyName("createContainer")]
    public List<HookConfiguration>? CreateContainer { get; init; }
    
    [JsonPropertyName("startContainer")]
    public List<HookConfiguration>? StartContainer { get; init; }
    
    [JsonPropertyName("poststart")]
    public List<HookConfiguration>? Poststart { get; init; }
    
    [JsonPropertyName("poststop")]
    public List<HookConfiguration>? Poststop { get; init; }
}

/// <summary>
/// Hook configuration
/// </summary>
public sealed class HookConfiguration
{
    [JsonPropertyName("path")]
    public required string Path { get; init; }
    
    [JsonPropertyName("args")]
    public List<string>? Args { get; init; }
    
    [JsonPropertyName("env")]
    public List<string>? Env { get; init; }
    
    [JsonPropertyName("timeout")]
    public int? Timeout { get; init; }
}

/// <summary>
/// Linux-specific configuration
/// </summary>
public sealed class LinuxConfiguration
{
    [JsonPropertyName("namespaces")]
    public List<NamespaceConfiguration>? Namespaces { get; init; }
    
    [JsonPropertyName("uidMappings")]
    public List<IdMappingConfiguration>? UidMappings { get; init; }
    
    [JsonPropertyName("gidMappings")]
    public List<IdMappingConfiguration>? GidMappings { get; init; }
    
    [JsonPropertyName("devices")]
    public List<DeviceConfiguration>? Devices { get; init; }
    
    [JsonPropertyName("cgroupsPath")]
    public string? CgroupsPath { get; init; }
    
    [JsonPropertyName("resources")]
    public ResourcesConfiguration? Resources { get; init; }
    
    [JsonPropertyName("seccomp")]
    public SeccompConfiguration? Seccomp { get; init; }
    
    [JsonPropertyName("rootfsPropagation")]
    public string? RootfsPropagation { get; init; }
    
    [JsonPropertyName("maskedPaths")]
    public List<string>? MaskedPaths { get; init; }
    
    [JsonPropertyName("readonlyPaths")]
    public List<string>? ReadonlyPaths { get; init; }
}

/// <summary>
/// Namespace configuration
/// </summary>
public sealed class NamespaceConfiguration
{
    [JsonPropertyName("type")]
    public required string Type { get; init; }
    
    [JsonPropertyName("path")]
    public string? Path { get; init; }
}

/// <summary>
/// User/Group ID mapping configuration
/// </summary>
public sealed class IdMappingConfiguration
{
    [JsonPropertyName("containerID")]
    public uint ContainerID { get; init; }
    
    [JsonPropertyName("hostID")]
    public uint HostID { get; init; }
    
    [JsonPropertyName("size")]
    public uint Size { get; init; }
}

/// <summary>
/// Device configuration
/// </summary>
public sealed class DeviceConfiguration
{
    [JsonPropertyName("type")]
    public required string Type { get; init; }
    
    [JsonPropertyName("path")]
    public required string Path { get; init; }
    
    [JsonPropertyName("major")]
    public long? Major { get; init; }
    
    [JsonPropertyName("minor")]
    public long? Minor { get; init; }
    
    [JsonPropertyName("fileMode")]
    public uint? FileMode { get; init; }
    
    [JsonPropertyName("uid")]
    public uint? Uid { get; init; }
    
    [JsonPropertyName("gid")]
    public uint? Gid { get; init; }
}

/// <summary>
/// Resource limits configuration (cgroups)
/// </summary>
public sealed class ResourcesConfiguration
{
    [JsonPropertyName("memory")]
    public MemoryConfiguration? Memory { get; init; }
    
    [JsonPropertyName("cpu")]
    public CpuConfiguration? Cpu { get; init; }
    
    [JsonPropertyName("blockIO")]
    public BlockIOConfiguration? BlockIO { get; init; }
    
    [JsonPropertyName("pids")]
    public PidsConfiguration? Pids { get; init; }
}

/// <summary>
/// Memory resource configuration
/// </summary>
public sealed class MemoryConfiguration
{
    [JsonPropertyName("limit")]
    public long? Limit { get; init; }
    
    [JsonPropertyName("reservation")]
    public long? Reservation { get; init; }
    
    [JsonPropertyName("swap")]
    public long? Swap { get; init; }
}

/// <summary>
/// CPU resource configuration
/// </summary>
public sealed class CpuConfiguration
{
    [JsonPropertyName("shares")]
    public ulong? Shares { get; init; }
    
    [JsonPropertyName("quota")]
    public long? Quota { get; init; }
    
    [JsonPropertyName("period")]
    public ulong? Period { get; init; }
    
    [JsonPropertyName("cpus")]
    public string? Cpus { get; init; }
}

/// <summary>
/// Block I/O resource configuration
/// </summary>
public sealed class BlockIOConfiguration
{
    [JsonPropertyName("weight")]
    public ushort? Weight { get; init; }
}

/// <summary>
/// PIDs resource configuration
/// </summary>
public sealed class PidsConfiguration
{
    [JsonPropertyName("limit")]
    public long Limit { get; init; }
}

/// <summary>
/// Seccomp configuration
/// </summary>
public sealed class SeccompConfiguration
{
    [JsonPropertyName("defaultAction")]
    public required string DefaultAction { get; init; }
    
    [JsonPropertyName("architectures")]
    public List<string>? Architectures { get; init; }
    
    [JsonPropertyName("syscalls")]
    public List<SyscallConfiguration>? Syscalls { get; init; }
}

/// <summary>
/// Syscall configuration for seccomp
/// </summary>
public sealed class SyscallConfiguration
{
    [JsonPropertyName("names")]
    public required List<string> Names { get; init; }
    
    [JsonPropertyName("action")]
    public required string Action { get; init; }
}

/// <summary>
/// Windows-specific configuration
/// </summary>
public sealed class WindowsConfiguration
{
    [JsonPropertyName("layerFolders")]
    public List<string>? LayerFolders { get; init; }
    
    [JsonPropertyName("hyperv")]
    public HyperVConfiguration? HyperV { get; init; }
    
    [JsonPropertyName("network")]
    public WindowsNetworkConfiguration? Network { get; init; }
    
    [JsonPropertyName("resources")]
    public WindowsResourcesConfiguration? Resources { get; init; }
}

/// <summary>
/// Hyper-V configuration for Windows containers
/// </summary>
public sealed class HyperVConfiguration
{
    [JsonPropertyName("utilityVMPath")]
    public string? UtilityVMPath { get; init; }
}

/// <summary>
/// Windows network configuration
/// </summary>
public sealed class WindowsNetworkConfiguration
{
    [JsonPropertyName("endpointList")]
    public List<string>? EndpointList { get; init; }
    
    [JsonPropertyName("allowUnqualifiedDNSQuery")]
    public bool AllowUnqualifiedDNSQuery { get; init; }
}

/// <summary>
/// Windows resource configuration
/// </summary>
public sealed class WindowsResourcesConfiguration
{
    [JsonPropertyName("memory")]
    public WindowsMemoryConfiguration? Memory { get; init; }
    
    [JsonPropertyName("cpu")]
    public WindowsCpuConfiguration? Cpu { get; init; }
    
    [JsonPropertyName("storage")]
    public WindowsStorageConfiguration? Storage { get; init; }
}

/// <summary>
/// Windows memory configuration
/// </summary>
public sealed class WindowsMemoryConfiguration
{
    [JsonPropertyName("limit")]
    public ulong? Limit { get; init; }
}

/// <summary>
/// Windows CPU configuration
/// </summary>
public sealed class WindowsCpuConfiguration
{
    [JsonPropertyName("count")]
    public ulong? Count { get; init; }
    
    [JsonPropertyName("shares")]
    public ushort? Shares { get; init; }
    
    [JsonPropertyName("maximum")]
    public ushort? Maximum { get; init; }
}

/// <summary>
/// Windows storage configuration
/// </summary>
public sealed class WindowsStorageConfiguration
{
    [JsonPropertyName("iops")]
    public ulong? Iops { get; init; }
    
    [JsonPropertyName("bps")]
    public ulong? Bps { get; init; }
    
    [JsonPropertyName("sandboxSize")]
    public ulong? SandboxSize { get; init; }
}
