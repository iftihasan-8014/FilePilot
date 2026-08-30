using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;

using FilePilot.Services;

namespace FilePilot.ViewModels;

/// <summary>
/// Root ViewModel for the main application shell.
/// Manages navigation, dashboard statistics, and status bar state.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    // ─── Navigation ───────────────────────────────────────────────

    [ObservableProperty]
    private ObservableCollection<NavigationItem> _navigationItems = [];

    [ObservableProperty]
    private NavigationItem? _selectedNavigationItem;

    [ObservableProperty]
    private string _currentPageTitle = "Dashboard";

    [ObservableProperty]
    private PackIconKind _currentPageIcon = PackIconKind.ViewDashboard;

    [ObservableProperty]
    private bool _isSidebarExpanded = true;

    /// <summary>
    /// The currently displayed child ViewModel. DataTemplates in
    /// MainWindow.xaml map each ViewModel type to its View.
    /// </summary>
    [ObservableProperty]
    private ObservableObject? _currentViewModel;

    // ─── Status Bar ───────────────────────────────────────────────

    [ObservableProperty]
    private bool _isMonitoringActive = true;

    [ObservableProperty]
    private int _backgroundTasks = 2;

    [ObservableProperty]
    private string _lastScanTime = "2 minutes ago";

    [ObservableProperty]
    private int _totalFilesTracked = 45_219;

    [ObservableProperty]
    private bool _isAutoStartEnabled;

    // ─── Search ───────────────────────────────────────────────────

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    // ═══════════════════════════════════════════════════════════════
    // Services & Constructor
    // ═══════════════════════════════════════════════════════════════

    private readonly IMonitorService _monitorService;
    private readonly IStartupService _startupService;
    private readonly IServiceProvider _serviceProvider;

    public MainViewModel(
        IMonitorService monitorService,
        IStartupService startupService,
        IServiceProvider serviceProvider)
    {
        _monitorService = monitorService;
        _startupService = startupService;
        _serviceProvider = serviceProvider;

        IsAutoStartEnabled = _startupService.IsAutoStartEnabled();

        InitializeNavigation();
    }

    // ═══════════════════════════════════════════════════════════════
    // Commands
    // ═══════════════════════════════════════════════════════════════

    [RelayCommand]
    private void ToggleSidebar()
    {
        IsSidebarExpanded = !IsSidebarExpanded;
    }

    [RelayCommand]
    private void ToggleMonitoring()
    {
        IsMonitoringActive = !IsMonitoringActive;

        string downloadsFolder = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

        if (IsMonitoringActive)
        {
            _monitorService.StartMonitoring(downloadsFolder);
        }
        else
        {
            _monitorService.StopMonitoring(downloadsFolder);
        }
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
    private void NavigateTo(NavigationItem? item)
    {
        if (item is null) return;

        // Deselect all, then select the clicked item
        foreach (var nav in NavigationItems)
            nav.IsSelected = false;

        item.IsSelected = true;
        SelectedNavigationItem = item;
        CurrentPageTitle = item.Name;
        CurrentPageIcon = item.Icon;

        // Route to the appropriate ViewModel
        if (item.Name == "Dashboard")
        {
            CurrentViewModel = _serviceProvider.GetRequiredService<DashboardViewModel>();
        }
        else if (item.Name == "Settings")
        {
            CurrentViewModel = _serviceProvider.GetRequiredService<SettingsViewModel>();
        }
        else if (item.Name == "Duplicate Finder")
        {
            CurrentViewModel = _serviceProvider.GetRequiredService<DuplicateFinderViewModel>();
        }
        else if (item.Name == "File Categorizer")
        {
            CurrentViewModel = _serviceProvider.GetRequiredService<CategorizerViewModel>();
        }
        else
        {
            // For any non-implemented page, use a placeholder ViewModel
            // that shows the page name and icon
            var placeholder = _serviceProvider.GetRequiredService<PlaceholderPageViewModel>();
            placeholder.PageTitle = item.Name;
            placeholder.PageIcon = item.Icon;
            CurrentViewModel = placeholder;
        }
    }

    [RelayCommand]
    private void Search()
    {
        // Placeholder — backend will handle search logic
    }

    [RelayCommand]
    private void OpenSettings()
    {
        // Navigate to Settings through the same mechanism
        CurrentPageTitle = "Settings";
        CurrentPageIcon = PackIconKind.Cog;

        // Deselect sidebar items
        foreach (var nav in NavigationItems)
            nav.IsSelected = false;

        CurrentViewModel = _serviceProvider.GetRequiredService<SettingsViewModel>();
    }

    // ═══════════════════════════════════════════════════════════════
    // Initialization Helpers
    // ═══════════════════════════════════════════════════════════════

    private void InitializeNavigation()
    {
        NavigationItems =
        [
            // ── Core Features ──
            new("Dashboard",         PackIconKind.ViewDashboard,    "Overview"),
            new("File Categorizer",  PackIconKind.FolderMultiple,   "Core Features", "NEW"),
            new("Custom Rules",      PackIconKind.RulerSquareCompass, "Core Features"),
            new("Duplicate Finder",  PackIconKind.ContentDuplicate, "Core Features", "342"),
            new("Date Sorting",      PackIconKind.CalendarClock,     "Core Features"),
            new("Real-Time Monitor", PackIconKind.MonitorEye,       "Core Features"),
            new("Large File Finder", PackIconKind.FileChart,        "Core Features"),
            new("Batch Renamer",     PackIconKind.RenameBox,        "Core Features"),
            new("Activity Log",      PackIconKind.History,          "Core Features"),

            // ── Advanced Features ──
            new("Smart Text Sort",   PackIconKind.TextSearch,       "Advanced"),
            new("OCR Categorizer",   PackIconKind.TextRecognition,  "Advanced"),
            new("Self-Destruct",     PackIconKind.TimerSand,        "Advanced"),
            new("Smart Quarantine",  PackIconKind.ShieldLock,       "Advanced"),
            new("Project Groups",    PackIconKind.FolderStar,       "Advanced"),
        ];

        // Select Dashboard by default
        NavigationItems[0].IsSelected = true;
        SelectedNavigationItem = NavigationItems[0];
    }
}
