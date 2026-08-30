using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FilePilot.Services;

namespace FilePilot.ViewModels;

/// <summary>
/// ViewModel for the Settings page.
/// Controls auto-start and monitoring preferences.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly IStartupService _startupService;
    private readonly IMonitorService _monitorService;

    [ObservableProperty]
    private bool _isAutoStartEnabled;

    [ObservableProperty]
    private bool _isMonitoringEnabled = true;

    [ObservableProperty]
    private string _monitoredFolder = string.Empty;

    public SettingsViewModel(IStartupService startupService, IMonitorService monitorService)
    {
        _startupService = startupService;
        _monitorService = monitorService;

        IsAutoStartEnabled = _startupService.IsAutoStartEnabled();
        MonitoredFolder = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
    }

    [RelayCommand]
    private void ToggleAutoStart()
    {
        if (IsAutoStartEnabled)
        {
            _startupService.DisableAutoStart();
            IsAutoStartEnabled = false;
        }
        else
        {
            _startupService.EnableAutoStart();
            IsAutoStartEnabled = true;
        }
    }

    [RelayCommand]
    private void ToggleMonitoring()
    {
        if (IsMonitoringEnabled)
        {
            _monitorService.StopMonitoring(MonitoredFolder);
            IsMonitoringEnabled = false;
        }
        else
        {
            _monitorService.StartMonitoring(MonitoredFolder);
            IsMonitoringEnabled = true;
        }
    }
}
