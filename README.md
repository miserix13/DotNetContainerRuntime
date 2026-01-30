# DotNetContainerRuntime

A hardware-agnostic, OCI-compliant container runtime built on .NET 10.0 and written in C#. This runtime supports simultaneous Windows and Linux container execution through platform abstraction and dual-backend architecture.

## Features

- **OCI Compliance**: Implements the Open Container Initiative (OCI) Runtime Specification v1.2.0
- **Cross-Platform**: Supports both Linux and Windows containers
- **Hardware Agnostic**: Works across different CPU architectures (x86_64, ARM64)
- **Native AOT**: CLI compiled with Native AOT for fast startup and low memory footprint
- **Dual OS Support**: Can manage both Windows and Linux containers simultaneously

## Architecture

The runtime is organized into modular assemblies:

- **DotNetContainerRuntime.Core**: Core abstractions, interfaces, and OCI specification models
- **DotNetContainerRuntime.Runtime**: Main runtime logic and lifecycle management
- **DotNetContainerRuntime.Linux**: Linux-specific implementations (namespaces, cgroups, overlayfs)
- **DotNetContainerRuntime.Windows**: Windows-specific implementations (HCS, job objects, WCIFS)
- **DotNetContainerRuntime.Image**: OCI image format support and layer management
- **DotNetContainerRuntime.CLI**: Command-line interface (Native AOT compiled)
- **DotNetContainerRuntime.GUI**: Cross-platform GUI built with AvaloniaUI for container management

## Current Implementation Status

### ✅ Phase 1: Foundation (Completed)
- [x] Project structure with 6 assemblies + GUI
- [x] OCI Runtime Specification models (config.json, state.json)
- [x] Unit test suite with 41 passing tests
- [x] Cross-platform GUI with Docker-inspired design
- [x] Core abstractions and interfaces:
  - `IContainerRuntime`: Lifecycle management (create, start, kill, delete, state)
  - `INamespaceManager`: Linux namespace management
  - `IResourceController`: Resource control (cgroups/job objects)
  - `IFilesystemManager`: Rootfs and mount management
  - `IProcessManager`: Process execution
  - `IRuntimeFactory`: Platform-specific factory pattern
- [x] Linux P/Invoke declarations:
  - Namespaces: `unshare()`, `setns()`
  - Mounts: `mount()`, `umount()`, `pivot_root()`
  - Process: `fork()`, `execve()`, `kill()`, `waitpid()`
  - Capabilities: Linux capabilities management
- [x] Windows P/Invoke declarations:
  - Host Compute Service (HCS) APIs
  - Job Objects for resource control
  - Process creation and management
- [x] Native AOT configuration for CLI
- [x] AvaloniaUI-based GUI application:
  - MVVM architecture with CommunityToolkit.Mvvm
  - Container lifecycle management (start, stop, delete, refresh)
  - Docker Desktop-inspired UI design
  - Real-time container status monitoring
  - Demo mode with sample containers

### 🚧 Phase 2: Linux Implementation (In Progress - 85% Complete)
- [x] **LinuxNamespaceManager** - Fully implemented and interface-aligned
  - Namespace creation with unshare()
  - Namespace joining with setns()
  - Namespace info and file descriptor management
- [x] **LinuxCgroupController** - Fully implemented and interface-aligned
  - Cgroups v2 unified hierarchy support
  - CPU, memory, I/O, and PID limits
  - Resource usage tracking
- [x] **LinuxFilesystemManager** - Fully implemented and interface-aligned
  - OverlayFS layered filesystem
  - Mount/unmount operations
  - Pivot root and chroot support
- [x] **LinuxProcessManager** - Fully implemented and interface-aligned
  - Fork/exec process creation
  - Signal handling and process waiting
  - User/group ID management
- [x] **LinuxContainerRuntime** - Fully implemented orchestrator
  - Coordinates all Linux components
  - Container lifecycle management
- [x] **Interface Alignment** - COMPLETED
  - All implementations match Core interface signatures
  - Context objects properly used
  - Solution builds without errors
- [x] **Unit Tests** - COMPLETED (25 tests, 100% passing)
  - Tests for all 5 Linux components
  - Platform detection tests
  - Error handling verification
  - Mock-based testing with Moq
- [ ] Integration tests on Linux with elevated privileges
- [ ] Real container lifecycle testing
- [ ] Advanced features (capabilities, rlimits)

#### Phase 3: Windows Implementation
- [ ] Implement `WindowsHcsRuntime`
- [ ] Implement `WindowsJobObjectController`
- [ ] Implement `WindowsFilesystemManager` (WCIFS)
- [ ] Implement `WindowsProcessManager`
- [ ] Basic container lifecycle on Windows

#### Phase 4: Image Support
- [ ] OCI image manifest parsing
- [ ] Layer extraction and caching
- [ ] Content-addressable storage
- [ ] Registry client integration

#### Phase 5: CLI and Integration
- [ ] Command-line interface with System.CommandLine
- [ ] State persistence
- [ ] Hooks support
- [ ] Comprehensive testing

## Prerequisites

- .NET 10.0 SDK or later
- **Linux**: Root privileges for namespace/cgroup operations
- **Windows**: Windows Server 2019+ or Windows 10/11 Enterprise/Pro with containers feature enabled

## Building

```bash
# Clone the repository
git clone <
dotnet test

# Run the GUI
dotnet run --project src/DotNetContainerRuntime.GUIntainerRuntime

# Build the solution
dotnet build

# Run tests (when available)
dotnet test

# Publish CLI with Native AOT
dotnet p

### GUI Application

Launch the graphical interface for container management:

```bash
dotnet run --project src/DotNetContainerRuntime.GUI
```

**Features:**
- Visual container lifecycle management
- Real-time status monitoring with color-coded indicators
- Docker Desktop-inspired interface
- Create, start, stop, and delete containers
- View container details and logs
- Cross-platform (Windows, Linux, macOS)

### CLIublish src/DotNetContainerRuntime.CLI -c Release
```

## Usage (Coming Soon)

```bash
# Create a container
dotnetcr create <container-id> <bundle-path>

# Start a container
dotnetcr start <container-id>

# Get container state
dotnetcr state <container-id>

# Kill a container
dotnetcr kill <container-id> [signal]

# Delete a container
dotnetcr delete <container-id>

# List containers
dotnetcr list
```

## Technical Details

### Linux Container Isolation
- **Namespaces**: PID, Network, Mount, IPC, UTS, User, Cgroup, Time
- **Cgroups v2**: CPU, memory, I/O, PIDs limits
- **Security**: Seccomp, capabilities, AppArmor/SELinux
- **Filesystem**: OverlayFS for efficient layering

### Windows Container Isolation
- **Process Isolation**: HCS with shared kernel
- **Hyper-V Isolation**: Lightweight VM per container
- **Job Objects**: Resource limits and process grouping
- **Filesystem**: WCIFS for layered storage

## Contributing

Contributions are welcome! This project is currently in early development.

## License

MIT License - see [LICENSE](LICENSE) for details

## References

- [OCI Runtime Specification](https://github.com/opencontainers/runtime-spec)
- [OCI Image Specification](https://github.com/opencontainers/image-spec)
- [Linux Namespaces Documentation](https://man7.org/linux/man-pages/man7/namespaces.7.html)
- [Windows Containers Documentation](https://docs.microsoft.com/en-us/virtualization/windowscontainers/)
