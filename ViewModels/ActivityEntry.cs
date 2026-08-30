using CommunityToolkit.Mvvm.ComponentModel;
using MaterialDesignThemes.Wpf;

namespace FilePilot.ViewModels;

/// <summary>
/// Represents a single entry in the recent activity feed.
/// </summary>
public partial class ActivityEntry : ObservableObject
{
    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _source = string.Empty;

    [ObservableProperty]
    private string _timeAgo = string.Empty;

    [ObservableProperty]
    private PackIconKind _icon;

    [ObservableProperty]
    private string _accentColor = "#7C4DFF";

    public ActivityEntry() { }

    public ActivityEntry(string description, string source, string timeAgo, PackIconKind icon, string accentColor)
    {
        Description = description;
        Source = source;
        TimeAgo = timeAgo;
        Icon = icon;
        AccentColor = accentColor;
    }
}
