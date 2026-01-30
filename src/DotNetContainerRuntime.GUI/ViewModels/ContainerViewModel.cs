using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DotNetContainerRuntime.Core.Specifications;

namespace DotNetContainerRuntime.GUI.ViewModels;

public partial class ContainerViewModel : ObservableObject
{
    [ObservableProperty]
    private string _id = string.Empty;
    
    [ObservableProperty]
    private ContainerStatus _status;
    
    [ObservableProperty]
    private int? _pid;
    
    [ObservableProperty]
    private string _bundle = string.Empty;
    
    [ObservableProperty]
    private DateTimeOffset _createdAt = DateTimeOffset.UtcNow;
    
    [ObservableProperty]
    private bool _isSelected;
    
    public string StatusText => Status.ToString();
    
    public string StatusColor => Status switch
    {
        ContainerStatus.Running => "#00C896",
        ContainerStatus.Created => "#0DB7ED",
        ContainerStatus.Stopped => "#6B7684",
        ContainerStatus.Creating => "#FFA726",
        _ => "#6B7684"
    };
}
