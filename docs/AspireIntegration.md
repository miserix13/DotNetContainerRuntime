# .NET Aspire Integration Strategy

## Overview

This document outlines the strategy for integrating **DotNetContainerRuntime** with **.NET Aspire's Distributed Control Plane (DCP)** and Resource service to provide seamless orchestration and management capabilities.

## .NET Aspire Architecture

### Distributed Control Plane (DCP)

DCP is Aspire's core orchestration component that manages distributed applications:

- **Purpose**: Kubernetes-like control plane for local development and production deployments
- **Components**:
  - `DcpExecutor`: Main orchestration engine that creates and manages DCP resources
  - `DcpHost`: Manages DCP lifecycle and container runtime health checks
  - `KubernetesService`: Handles Kubernetes-style resource CRUD operations
  - Custom Resources: `Container`, `Executable`, `Service`, `Endpoint`, `ContainerNetwork`

### Key Concepts

1. **Custom Resources**: Kubernetes-style resource definitions
   - `Container`: Container workload specifications
   - `Executable`: Native process workload specifications
   - `Service`: Service discovery and networking
   - `Endpoint`: External endpoint definitions
   - `ContainerNetwork`: Network topology

2. **Resource Model**:
   ```
   DistributedApplication
   ├── DistributedApplicationModel (user-defined resources)
   ├── DcpExecutor (orchestration logic)
   └── DcpHost (DCP process management)
   ```

3. **Lifecycle Management**:
   - Prepare phase: Convert application model to DCP resources
   - Create phase: Submit resources to DCP
   - Watch phase: Monitor resource state changes
   - Update phase: Handle resource lifecycle events

## Integration Points

### 1. Container Runtime Provider

**Objective**: Replace or extend DCP's container runtime abstraction with our OCI-compliant runtime.

**Current DCP Implementation**:
```csharp
// DCP relies on Docker/Podman through DcpContainersInfo
internal sealed class DcpContainersInfo
{
    public string? Runtime { get; set; }  // "docker" or "podman"
    public bool Installed { get; set; }
    public bool Running { get; set; }
    public string? HostName { get; set; }
}
```

**Integration Strategy**:
```csharp
// Create an adapter that implements IDcpContainerRuntime
public interface IDcpContainerRuntime
{
    Task<ContainerInfo> CreateContainerAsync(ContainerSpec spec, CancellationToken ct);
    Task StartContainerAsync(string containerId, CancellationToken ct);
    Task StopContainerAsync(string containerId, CancellationToken ct);
    Task DeleteContainerAsync(string containerId, CancellationToken ct);
    Task<ContainerState> GetContainerStateAsync(string containerId, CancellationToken ct);
}

public class DotNetContainerRuntimeAdapter : IDcpContainerRuntime
{
    private readonly IContainerRuntime _runtime;
    private readonly IRuntimeFactory _runtimeFactory;
    
    public async Task<ContainerInfo> CreateContainerAsync(ContainerSpec spec, CancellationToken ct)
    {
        // Convert DCP ContainerSpec to OCI RuntimeConfiguration
        var ociConfig = ConvertToOciConfig(spec);
        
        // Use our runtime
        await _runtime.CreateAsync(spec.ContainerName, ociConfig, ct);
        
        return new ContainerInfo
        {
            Id = spec.ContainerName,
            State = ContainerStatus.Created
        };
    }
    
    private RuntimeConfiguration ConvertToOciConfig(ContainerSpec spec)
    {
        return new RuntimeConfiguration
        {
            Version = OciVersion.Current,
            Root = new Root 
            { 
                Path = spec.Image  // Map to OCI image layers
            },
            Process = new ProcessConfiguration
            {
                Args = spec.Args,
                Env = spec.Environment,
                Cwd = spec.WorkingDirectory
            },
            Mounts = spec.VolumeMounts?.Select(vm => new Mount
            {
                Destination = vm.MountPath,
                Source = vm.Source,
                Type = "bind"
            }).ToArray(),
            Linux = new LinuxConfiguration
            {
                Namespaces = new[]
                {
                    new Namespace { Type = NamespaceType.Pid },
                    new Namespace { Type = NamespaceType.Network },
                    new Namespace { Type = NamespaceType.Mount },
                    new Namespace { Type = NamespaceType.Ipc }
                },
                Resources = ConvertResources(spec.ResourceLimits)
            }
        };
    }
}
```

**Files to Create**:
- `src/DotNetContainerRuntime.Aspire/DcpContainerRuntimeAdapter.cs`
- `src/DotNetContainerRuntime.Aspire/DcpResourceConverter.cs`
- `src/DotNetContainerRuntime.Aspire/DcpStateMapper.cs`

### 2. Resource Service Integration

**Objective**: Provide our runtime as a resource that can be added to Aspire applications.

**Aspire Resource Pattern**:
```csharp
// Example: How Aspire adds resources
var builder = DistributedApplication.CreateBuilder(args);
builder.AddContainer("mycontainer", "nginx")
       .WithEnvironment("VAR", "value");
```

**Our Integration**:
```csharp
public static class DotNetContainerRuntimeExtensions
{
    public static IResourceBuilder<ContainerResource> AddDotNetContainer(
        this IDistributedApplicationBuilder builder,
        string name,
        string image)
    {
        var container = new ContainerResource(name);
        
        // Register our runtime as the container provider
        builder.Services.AddSingleton<IContainerRuntime>(sp =>
        {
            var factory = sp.GetRequiredService<IRuntimeFactory>();
            return factory.CreateRuntime(RuntimePlatform.Linux);
        });
        
        return builder.AddResource(container)
                     .WithAnnotation(new ContainerImageAnnotation { Image = image })
                     .WithAnnotation(new DotNetContainerRuntimeAnnotation());
    }
    
    public static IResourceBuilder<ContainerResource> WithOciConfig(
        this IResourceBuilder<ContainerResource> builder,
        Action<RuntimeConfiguration> configure)
    {
        var config = new RuntimeConfiguration();
        configure(config);
        return builder.WithAnnotation(new OciConfigAnnotation(config));
    }
}

// Usage:
var builder = DistributedApplication.CreateBuilder(args);
builder.AddDotNetContainer("redis", "redis:latest")
       .WithOciConfig(config =>
       {
           config.Linux.Resources = new ResourcesConfiguration
           {
               Memory = new MemoryLimit { Limit = 512 * 1024 * 1024 }
           };
       })
       .WithEndpoint(6379, scheme: "tcp");
```

**Files to Create**:
- `src/DotNetContainerRuntime.Aspire/AspireResourceExtensions.cs`
- `src/DotNetContainerRuntime.Aspire/Annotations/DotNetContainerRuntimeAnnotation.cs`
- `src/DotNetContainerRuntime.Aspire/Annotations/OciConfigAnnotation.cs`

### 3. DCP Executor Integration

**Objective**: Hook into DCP's execution pipeline to use our runtime.

**DCP Execution Flow**:
```
DistributedApplication.Run()
  ├── DcpHost.StartAsync()
  │   └── EnsureDcpContainerRuntimeAsync()
  ├── DcpExecutor.RunApplicationAsync()
  │   ├── PrepareContainers()
  │   ├── CreateAllDcpObjectsAsync<Container>()
  │   └── CreateContainerAsync()
  └── Watch for resource changes
```

**Integration Approach**:
```csharp
// Register our executor in the DI container
public static class AspireDcpIntegration
{
    public static IHostApplicationBuilder UseDotNetContainerRuntime(
        this IHostApplicationBuilder builder)
    {
        // Replace default container executor
        builder.Services.AddSingleton<IDcpExecutor, DotNetContainerDcpExecutor>();
        
        // Register our runtime components
        builder.Services.AddSingleton<IRuntimeFactory, DefaultRuntimeFactory>();
        builder.Services.AddTransient<IContainerRuntime>(sp =>
        {
            var factory = sp.GetRequiredService<IRuntimeFactory>();
            var platform = OperatingSystem.IsLinux() 
                ? RuntimePlatform.Linux 
                : RuntimePlatform.Windows;
            return factory.CreateRuntime(platform);
        });
        
        return builder;
    }
}

public class DotNetContainerDcpExecutor : IDcpExecutor
{
    private readonly IContainerRuntime _runtime;
    private readonly ILogger<DotNetContainerDcpExecutor> _logger;
    
    public async Task RunApplicationAsync(CancellationToken cancellationToken)
    {
        // Convert DCP resources to our runtime format
        foreach (var container in _appResources.OfType<Container>())
        {
            var ociConfig = ConvertToOciConfig(container);
            await _runtime.CreateAsync(container.Metadata.Name, ociConfig, cancellationToken);
            await _runtime.StartAsync(container.Metadata.Name, cancellationToken);
        }
    }
    
    public async Task StartResourceAsync(IResourceReference resourceRef, CancellationToken ct)
    {
        var resource = (RenderedModelResource)resourceRef;
        await _runtime.StartAsync(resource.DcpResourceName, ct);
    }
    
    public async Task StopResourceAsync(IResourceReference resourceRef, CancellationToken ct)
    {
        var resource = (RenderedModelResource)resourceRef;
        await _runtime.KillAsync(resource.DcpResourceName, Signal.SIGTERM, ct);
    }
}
```

**Files to Create**:
- `src/DotNetContainerRuntime.Aspire/DotNetContainerDcpExecutor.cs`
- `src/DotNetContainerRuntime.Aspire/AspireDcpIntegration.cs`

### 4. Service Discovery Integration

**Objective**: Enable service discovery between containers managed by our runtime.

**DCP Service Model**:
```csharp
// DCP uses Service and Endpoint resources for discovery
internal class Service : CustomResource
{
    public ServiceSpec Spec { get; set; }
    public ServiceStatus Status { get; set; }
}

internal class ServiceSpec
{
    public string Protocol { get; set; }  // TCP, UDP
    public int Port { get; set; }
    public string Address { get; set; }
    public AddressAllocationModes AddressAllocationMode { get; set; }
}
```

**Our Integration**:
```csharp
public class ServiceDiscoveryManager : IServiceDiscoveryManager
{
    private readonly ConcurrentDictionary<string, ServiceEndpoint> _services = new();
    
    public async Task RegisterServiceAsync(
        string serviceName,
        string containerId,
        EndpointAnnotation endpoint,
        CancellationToken ct)
    {
        // Get container's network configuration
        var state = await _runtime.GetStateAsync(containerId, ct);
        
        var serviceEndpoint = new ServiceEndpoint
        {
            Name = serviceName,
            Address = state.Annotations?["network.address"] ?? "localhost",
            Port = endpoint.Port ?? 0,
            Protocol = endpoint.Protocol ?? "tcp",
            ContainerId = containerId
        };
        
        _services[serviceName] = serviceEndpoint;
        
        // Publish to DCP
        await PublishToDcpAsync(serviceEndpoint, ct);
    }
    
    public async Task<ServiceEndpoint?> ResolveServiceAsync(
        string serviceName,
        CancellationToken ct)
    {
        return _services.TryGetValue(serviceName, out var endpoint) 
            ? endpoint 
            : null;
    }
}
```

**Files to Create**:
- `src/DotNetContainerRuntime.Aspire/ServiceDiscoveryManager.cs`
- `src/DotNetContainerRuntime.Aspire/ServiceEndpoint.cs`

### 5. Telemetry and Observability

**Objective**: Integrate with Aspire Dashboard for monitoring.

**DCP Telemetry Integration**:
```csharp
public class AspireTelemetryProvider : ITelemetryProvider
{
    private readonly ResourceNotificationService _notificationService;
    
    public async Task ReportContainerStateAsync(
        string containerId,
        ContainerState state,
        CancellationToken ct)
    {
        var snapshot = new CustomResourceSnapshot
        {
            ResourceType = "Container",
            Metadata = new ResourceMetadata
            {
                Name = containerId,
                State = MapContainerStatus(state.Status)
            },
            Properties = new[]
            {
                new ResourceProperty("Status", state.Status.ToString()),
                new ResourceProperty("Pid", state.Pid?.ToString() ?? "N/A"),
                new ResourceProperty("Created", state.Annotations?["created"] ?? ""),
                new ResourceProperty("Bundle", state.Bundle ?? "")
            }
        };
        
        await _notificationService.PublishUpdateAsync(
            containerId,
            snapshot,
            ct);
    }
    
    private string MapContainerStatus(ContainerStatus status) => status switch
    {
        ContainerStatus.Creating => "Starting",
        ContainerStatus.Created => "Running",
        ContainerStatus.Running => "Running",
        ContainerStatus.Stopped => "Exited",
        _ => "Unknown"
    };
}
```

**Files to Create**:
- `src/DotNetContainerRuntime.Aspire/AspireTelemetryProvider.cs`
- `src/DotNetContainerRuntime.Aspire/ResourceSnapshotBuilder.cs`

## Implementation Phases

### Phase 1: Foundation (Week 1-2)
- [ ] Create `DotNetContainerRuntime.Aspire` project
- [ ] Fork and study Aspire repository
- [ ] Implement `DcpContainerRuntimeAdapter`
- [ ] Create basic resource converters (DCP ↔ OCI)
- [ ] Unit tests for converters

### Phase 2: Resource Extensions (Week 3-4)
- [ ] Implement `AddDotNetContainer` extension method
- [ ] Create custom annotations for OCI configuration
- [ ] Support for multi-container applications
- [ ] Integration tests with Aspire AppHost

### Phase 3: DCP Integration (Week 5-6)
- [ ] Implement `DotNetContainerDcpExecutor`
- [ ] Hook into DCP lifecycle events
- [ ] Service discovery implementation
- [ ] Network topology management

### Phase 4: Observability (Week 7-8)
- [ ] Aspire Dashboard integration
- [ ] Resource notification service
- [ ] Metrics and logging
- [ ] Performance monitoring

### Phase 5: Testing & Polish (Week 9-10)
- [ ] End-to-end integration tests
- [ ] Sample applications
- [ ] Documentation
- [ ] Performance benchmarking

## Project Structure

```
src/
├── DotNetContainerRuntime.Aspire/
│   ├── Adapters/
│   │   ├── DcpContainerRuntimeAdapter.cs
│   │   └── DcpResourceConverter.cs
│   ├── Annotations/
│   │   ├── DotNetContainerRuntimeAnnotation.cs
│   │   └── OciConfigAnnotation.cs
│   ├── Executors/
│   │   ├── DotNetContainerDcpExecutor.cs
│   │   └── ResourcePreparer.cs
│   ├── Extensions/
│   │   ├── AspireResourceExtensions.cs
│   │   └── AspireDcpIntegration.cs
│   ├── Services/
│   │   ├── ServiceDiscoveryManager.cs
│   │   └── AspireTelemetryProvider.cs
│   └── DotNetContainerRuntime.Aspire.csproj
│
tests/
├── DotNetContainerRuntime.Aspire.Tests/
│   ├── Adapters/
│   ├── Converters/
│   ├── Integration/
│   └── DotNetContainerRuntime.Aspire.Tests.csproj
│
samples/
└── AspireSample/
    ├── AspireSample.AppHost/
    │   ├── Program.cs
    │   └── appsettings.json
    ├── AspireSample.ApiService/
    └── AspireSample.Web/
```

## Dependencies

```xml
<ItemGroup>
  <!-- Aspire Hosting -->
  <PackageReference Include="Aspire.Hosting" Version="13.1.0" />
  <PackageReference Include="Aspire.Hosting.AppHost" Version="13.1.0" />
  
  <!-- Our Runtime -->
  <ProjectReference Include="..\DotNetContainerRuntime.Core\DotNetContainerRuntime.Core.csproj" />
  <ProjectReference Include="..\DotNetContainerRuntime.Runtime\DotNetContainerRuntime.Runtime.csproj" />
  <ProjectReference Include="..\DotNetContainerRuntime.Linux\DotNetContainerRuntime.Linux.csproj" />
  <ProjectReference Include="..\DotNetContainerRuntime.Windows\DotNetContainerRuntime.Windows.csproj" />
</ItemGroup>
```

## Sample Application

```csharp
// Program.cs in AppHost
var builder = DistributedApplication.CreateBuilder(args);

// Use our container runtime
builder.UseDotNetContainerRuntime();

// Add resources using our runtime
var redis = builder.AddDotNetContainer("redis", "redis:latest")
    .WithOciConfig(config =>
    {
        config.Linux.Resources = new ResourcesConfiguration
        {
            Memory = new MemoryLimit { Limit = 512 * 1024 * 1024 },
            Cpu = new CpuLimit { Shares = 1024 }
        };
    })
    .WithEndpoint(6379, scheme: "tcp");

var api = builder.AddProject<Projects.ApiService>("api")
    .WithReference(redis);

builder.AddProject<Projects.Web>("frontend")
    .WithReference(api);

builder.Build().Run();
```

## Benefits of Integration

1. **Unified Development Experience**: Use Aspire's familiar API with our OCI-compliant runtime
2. **Cross-Platform Support**: Seamlessly run Linux and Windows containers
3. **Full OCI Compliance**: Leverage complete OCI specification support
4. **Advanced Features**: Native AOT, hardware-agnostic design, dual OS support
5. **Aspire Ecosystem**: Access to Aspire Dashboard, integrations, and tooling
6. **Flexibility**: Choose between Docker/Podman or our runtime on a per-resource basis

## Testing Strategy

1. **Unit Tests**: Test converters, adapters, and individual components
2. **Integration Tests**: Test with actual Aspire AppHost
3. **E2E Tests**: Full application scenarios with multiple services
4. **Performance Tests**: Compare with default Docker/Podman implementation
5. **Compatibility Tests**: Ensure compliance with OCI and Aspire specs

## References

- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [Aspire GitHub Repository](https://github.com/dotnet/aspire)
- [OCI Runtime Specification](https://github.com/opencontainers/runtime-spec)
- [DCP Architecture (Aspire source)](https://github.com/dotnet/aspire/tree/main/src/Aspire.Hosting/Dcp)
