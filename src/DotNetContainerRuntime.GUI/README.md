# DotNet Container Runtime GUI

A cross-platform graphical user interface built with AvaloniaUI for managing containers using the DotNetContainerRuntime.

## Features

### Current Implementation

- **Modern Dark Theme**: VS Code-inspired dark theme with professional styling
- **Container List View**: Displays all containers with their:
  - Container ID (monospace font)
  - Status with colored indicator (Running=Green, Created=Blue, Stopped=Gray, Creating=Orange)
  - Process ID (PID)
  - Bundle path
  - Creation timestamp
- **Interactive Controls**:
  - ↻ Refresh - Reload container list
  - + Create - Create new container (stub)
  - ▶ Start - Start selected container (conditionally enabled)
  - ■ Stop - Stop running container (conditionally enabled)
  - 🗑 Delete - Delete stopped/created container (conditionally enabled)
  - 📋 Details - View container details (stub)
  - 📄 Logs - View container logs (stub)
- **Status Bar**: Real-time feedback and operation status
- **Platform Detection**: Shows current OS and CPU architecture
- **Responsive Layout**: Resizable window with minimum dimensions
- **Selection Management**: Click to select containers, commands enable/disable based on state

### Architecture

#### MVVM Pattern

**ViewModels**:
- `MainWindowViewModel` - Main application logic and commands
  - Observable collections for containers
  - Async commands for container operations
  - Status messaging
  - Platform detection
- `ContainerViewModel` - Individual container data model
  - Container properties (ID, status, PID, bundle, etc.)
  - Dynamic status coloring
  - Observable property changes

**Views**:
- `MainWindow.axaml` - Main application window
  - Toolbar with action buttons
  - Container list with data grid
  - Loading overlay
  - Status bar with counters

#### Command Pattern
Uses CommunityToolkit.Mvvm for:
- `[ObservableProperty]` - Auto-generated property change notifications
- `[RelayCommand]` - Auto-generated ICommand implementations
- Conditional command execution (`CanExecute` predicates)

### Technology Stack

- **Framework**: .NET 10.0
- **UI Framework**: AvaloniaUI 11.3.11
- **MVVM Toolkit**: CommunityToolkit.Mvvm
- **References**: 
  - DotNetContainerRuntime.Core
  - DotNetContainerRuntime.Runtime
  - DotNetContainerRuntime.Linux
  - DotNetContainerRuntime.Windows
  - DotNetContainerRuntime.Image

## Running the GUI

```bash
# From repository root
dotnet run --project src/DotNetContainerRuntime.GUI

# Or from GUI directory
cd src/DotNetContainerRuntime.GUI
dotnet run
```

## Demo Mode

The application currently runs with mock data demonstrating:
- 2 sample containers (one running, one created)
- All button states and interactions
- Status updates and loading indicators
- Platform information display

## Future Enhancements

### High Priority
- [ ] Wire up to actual `IContainerRuntime` implementation
- [ ] Create Container Dialog with config.json builder
- [ ] Container Details Window showing full state
- [ ] Real-time log viewer with filtering

### Medium Priority
- [ ] Container resource usage graphs (CPU, Memory)
- [ ] Image management (pull, list, delete)
- [ ] Bundle creation from images
- [ ] OCI config.json editor

### Low Priority
- [ ] Network management UI
- [ ] Volume management UI
- [ ] Export/Import containers
- [ ] Container templates
- [ ] Dark/Light theme toggle
- [ ] Keyboard shortcuts

## Integration Points

When the runtime backends are implemented, integrate:

```csharp
// In MainWindowViewModel constructor or via DI
private readonly IContainerRuntime _runtime;

public MainWindowViewModel(IContainerRuntime runtime)
{
    _runtime = runtime;
    InitializePlatformInfo();
    _ = LoadContainers();
}

// In LoadContainers()
var containerIds = await _runtime.ListAsync();
foreach (var id in containerIds)
{
    var state = await _runtime.GetStateAsync(id);
    Containers.Add(new ContainerViewModel
    {
        Id = state.Id,
        Status = state.Status,
        Pid = state.Pid,
        Bundle = state.Bundle,
        CreatedAt = DateTimeOffset.UtcNow // Add to state if needed
    });
}

// In StartContainer()
await _runtime.StartAsync(SelectedContainer.Id);

// In StopContainer()
await _runtime.KillAsync(SelectedContainer.Id, 15); // SIGTERM

// In DeleteContainer()
await _runtime.DeleteAsync(SelectedContainer.Id);
```

## Screenshots

The GUI features:
- **Toolbar**: Dark themed with icon-labeled buttons
- **Container List**: Grid layout with columns for ID, Status (with colored dot), PID, Bundle, Created
- **Status Bar**: Blue bar showing current operation and container count
- **Loading Overlay**: Semi-transparent overlay during operations

## Development

### Adding New Views

1. Create View in `Views/` directory:
```bash
dotnet new avalonia.window -n MyNewWindow
```

2. Create ViewModel in `ViewModels/`:
```csharp
public partial class MyNewWindowViewModel : ViewModelBase
{
    // Add properties and commands
}
```

3. Register in App or inject via DI

### Styling

The application uses a VS Code-inspired dark theme:
- Background: `#1E1E1E` (main)
- Secondary: `#2D2D30` (toolbar, headers)
- Border: `#3E3E42`
- Accent: `#007ACC` (status bar)
- Text: Default Avalonia text colors

## License

MIT License - See [LICENSE](../../LICENSE) file

## References

- [AvaloniaUI Documentation](https://docs.avaloniaui.net/)
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
