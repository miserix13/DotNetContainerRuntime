using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DotNetContainerRuntime.Core.Specifications;

namespace DotNetContainerRuntime.GUI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<ContainerViewModel> _containers = new();
    
    [ObservableProperty]
    private ContainerViewModel? _selectedContainer;
    
    [ObservableProperty]
    private string _statusMessage = "Ready";
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string _platformInfo = string.Empty;
    
    public MainWindowViewModel()
    {
        InitializePlatformInfo();
        _ = LoadContainers();
    }
    
    private void InitializePlatformInfo()
    {
        var platform = OperatingSystem.IsWindows() ? "Windows" : "Linux";
        var arch = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture;
        PlatformInfo = $"Platform: {platform} | Architecture: {arch}";
    }
    
    [RelayCommand]
    private async Task LoadContainers()
    {
        IsLoading = true;
        StatusMessage = "Loading containers...";
        
        try
        {
            // TODO: Call IContainerRuntime.ListAsync() when implemented
            await Task.Delay(500); // Simulate loading
            
            // Sample data for demonstration
            Containers.Clear();
            Containers.Add(new ContainerViewModel
            {
                Id = "container-1",
                Status = ContainerStatus.Running,
                Pid = 12345,
                Bundle = "/var/lib/containers/container-1",
                CreatedAt = DateTimeOffset.UtcNow.AddHours(-2)
            });
            Containers.Add(new ContainerViewModel
            {
                Id = "container-2",
                Status = ContainerStatus.Created,
                Bundle = "/var/lib/containers/container-2",
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-30)
            });
            
            StatusMessage = $"Loaded {Containers.Count} container(s)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand(CanExecute = nameof(CanStartContainer))]
    private async Task StartContainer()
    {
        if (SelectedContainer == null) return;
        
        IsLoading = true;
        StatusMessage = $"Starting container {SelectedContainer.Id}...";
        
        try
        {
            // TODO: Call IContainerRuntime.StartAsync()
            await Task.Delay(1000);
            
            SelectedContainer.Status = ContainerStatus.Running;
            SelectedContainer.Pid = Random.Shared.Next(10000, 99999);
            StatusMessage = $"Container {SelectedContainer.Id} started successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error starting container: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private bool CanStartContainer() => SelectedContainer?.Status == ContainerStatus.Created;
    
    [RelayCommand(CanExecute = nameof(CanStopContainer))]
    private async Task StopContainer()
    {
        if (SelectedContainer == null) return;
        
        IsLoading = true;
        StatusMessage = $"Stopping container {SelectedContainer.Id}...";
        
        try
        {
            // TODO: Call IContainerRuntime.KillAsync()
            await Task.Delay(1000);
            
            SelectedContainer.Status = ContainerStatus.Stopped;
            SelectedContainer.Pid = null;
            StatusMessage = $"Container {SelectedContainer.Id} stopped successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error stopping container: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private bool CanStopContainer() => SelectedContainer?.Status == ContainerStatus.Running;
    
    [RelayCommand(CanExecute = nameof(CanDeleteContainer))]
    private async Task DeleteContainer()
    {
        if (SelectedContainer == null) return;
        
        IsLoading = true;
        var containerId = SelectedContainer.Id;
        StatusMessage = $"Deleting container {containerId}...";
        
        try
        {
            // TODO: Call IContainerRuntime.DeleteAsync()
            await Task.Delay(500);
            
            Containers.Remove(SelectedContainer);
            SelectedContainer = null;
            StatusMessage = $"Container {containerId} deleted successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error deleting container: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private bool CanDeleteContainer() => SelectedContainer?.Status == ContainerStatus.Stopped || 
                                          SelectedContainer?.Status == ContainerStatus.Created;
    
    [RelayCommand]
    private void CreateContainer()
    {
        StatusMessage = "Create container dialog (not yet implemented)";
        // TODO: Show create container dialog
    }
    
    [RelayCommand]
    private void ViewLogs()
    {
        if (SelectedContainer == null) return;
        StatusMessage = $"Viewing logs for {SelectedContainer.Id} (not yet implemented)";
        // TODO: Show logs window
    }
    
    [RelayCommand]
    private void ViewDetails()
    {
        if (SelectedContainer == null) return;
        StatusMessage = $"Viewing details for {SelectedContainer.Id} (not yet implemented)";
        // TODO: Show details window
    }
    
    partial void OnSelectedContainerChanged(ContainerViewModel? value)
    {
        StartContainerCommand.NotifyCanExecuteChanged();
        StopContainerCommand.NotifyCanExecuteChanged();
        DeleteContainerCommand.NotifyCanExecuteChanged();
    }
}
