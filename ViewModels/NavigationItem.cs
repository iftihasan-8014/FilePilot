using CommunityToolkit.Mvvm.ComponentModel;
using MaterialDesignThemes.Wpf;

namespace FilePilot.ViewModels;

/// <summary>
/// Represents a single navigation entry in the sidebar.
/// </summary>
public partial class NavigationItem : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private PackIconKind _icon;

    [ObservableProperty]
    private string _category = string.Empty;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private string _badge = string.Empty;

    public NavigationItem() { }

    public NavigationItem(string name, PackIconKind icon, string category, string badge = "")
    {
        Name = name;
        Icon = icon;
        Category = category;
        Badge = badge;
    }
}
