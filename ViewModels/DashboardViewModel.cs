using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using FilePilot.Services;

namespace FilePilot.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IActivityService _activityService;
    private readonly IFileOrganizerService _fileOrganizerService;

    [ObservableProperty] private int _filesOrganized = 12847;
    [ObservableProperty] private int _duplicatesFound = 342;
    [ObservableProperty] private string _storageSaved = "4.7 GB";
    [ObservableProperty] private int _activeRules = 18;

    [ObservableProperty] private string _filesOrganizedTrend = "+127 today";
    [ObservableProperty] private string _duplicatesTrend = "+23 new";
    [ObservableProperty] private string _storageSavedTrend = "+320 MB today";
    [ObservableProperty] private string _activeRulesTrend = "3 triggered";

    [ObservableProperty] private ObservableCollection<ActivityEntry> _recentActivity = [];

    public DashboardViewModel(IActivityService activityService, IFileOrganizerService fileOrganizerService)
    {
        _activityService = activityService;
        _fileOrganizerService = fileOrganizerService;
        InitializeRecentActivity();
    }

    private void InitializeRecentActivity()
    {
        RecentActivity =
        [
            new ActivityEntry("Organized 47 files into Documents", "File Categorizer", "2 min ago", PackIconKind.FolderMultiple, "#7C4DFF"),
            new ActivityEntry("Found 23 duplicate photos", "Duplicate Finder", "15 min ago", PackIconKind.ContentDuplicate, "#FF9800"),
        ];
    }

    [RelayCommand]
    private void ClearActivity() => RecentActivity.Clear();

    [RelayCommand]
    private async Task ScanFolderAsync()
    {
        var dialog = new OpenFolderDialog();
        dialog.Title = "Select a folder to scan and organize";
        if (dialog.ShowDialog() == true)
        {
            int count = await _fileOrganizerService.OrganizeByExtensionAsync(dialog.FolderName);
            FilesOrganized += count;
            if (count > 0)
                RecentActivity.Insert(0, new ActivityEntry($"Organized {count} files", "File Categorizer", "Just now", PackIconKind.FolderMultiple, "#7C4DFF"));
        }
    }

    [RelayCommand]
    private async Task FindDuplicatesAsync()
    {
        var dialog = new OpenFolderDialog();
        dialog.Title = "Select a folder to find duplicates";
        if (dialog.ShowDialog() == true)
        {
            var duplicates = await _fileOrganizerService.FindDuplicatesAsync(dialog.FolderName);
            int totalDups = duplicates.Sum(d => d.FilePaths.Count - 1);
            DuplicatesFound += totalDups;
            if (totalDups > 0)
                RecentActivity.Insert(0, new ActivityEntry($"Found {totalDups} duplicate files", "Duplicate Finder", "Just now", PackIconKind.ContentDuplicate, "#FF9800"));
        }
    }

    [RelayCommand]
    private async Task BatchRenameAsync()
    {
        var dialog = new OpenFolderDialog();
        dialog.Title = "Select a folder for batch renaming";
        if (dialog.ShowDialog() == true)
        {
            int count = await _fileOrganizerService.BatchRenameAsync(dialog.FolderName, "File_{0}");
            if (count > 0)
                RecentActivity.Insert(0, new ActivityEntry($"Renamed {count} files", "Batch Renamer", "Just now", PackIconKind.RenameBox, "#69F0AE"));
        }
    }
}
