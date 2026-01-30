# Plan: Build OCI-Compliant Dual-Platform Container Runtime

Build a hardware-agnostic, OCI-compliant container runtime in .NET 10 that supports simultaneous Windows and Linux container execution through platform abstraction and dual-backend architecture. The runtime will use P/Invoke for system-level operations, Native AOT for performance, and modular design for extensibility.

## Steps

1. **Establish foundation and core abstractions** — Create project structure with separate assemblies for Core (OCI spec models, interfaces), Runtime (lifecycle management), Linux backend (namespaces/cgroups), and Windows backend (HCS/job objects)

2. **Implement Linux container backend** — Build `INamespaceManager`, `ICgroupManager`, `IFilesystemManager` implementations using P/Invoke to call `unshare()`, `setns()`, `clone()`, `pivot_root()` system calls, plus cgroup v2 file operations for resource control in Linux-specific assembly

3. **Implement Windows container backend** — Build Windows implementations using Host Compute Service (HCS) APIs via P/Invoke to `computecore.dll` and job objects via `kernel32.dll` for process isolation and resource management in Windows-specific assembly

4. **Create OCI image and layer management** — Implement OCI image spec support including manifest parsing, layer extraction, OverlayFS (Linux) and WCIFS (Windows) storage drivers, and content-addressable blob handling

5. **Build platform detection and routing layer** — Implement factory pattern that detects runtime platform using `RuntimeInformation.IsOSPlatform()`, instantiates appropriate backends, and routes container operations to Linux or Windows implementations

6. **Develop CLI and lifecycle orchestration** — Create command-line interface using System.CommandLine for OCI lifecycle operations (`create`, `start`, `kill`, `delete`, `state`), state persistence, and Native AOT compilation configuration

## Further Considerations

1. **Privilege requirements** — Container runtimes require elevated privileges (root on Linux, administrator on Windows). Should the runtime validate permissions at startup, or provide clear error messages when operations fail?

2. **Simultaneous dual-OS execution approach** — Full virtualization layer for cross-platform execution

3. **OCI compliance scope** — Should Phase 1 target full OCI runtime spec compliance including all hooks and annotations, or start with minimal viable implementation (create/start/kill/delete) and expand incrementally?
