using CommunityToolkit.Mvvm.ComponentModel;
using MaterialDesignThemes.Wpf;

namespace FilePilot.ViewModels;

/// <summary>
/// ViewModel for pages that are not yet implemented.
/// Displays the page name, icon, and a "Coming Soon" message.
/// </summary>
public partial class PlaceholderPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string _pageTitle = "Coming Soon";

    [ObservableProperty]
    private PackIconKind _pageIcon = PackIconKind.InformationOutline;
}
