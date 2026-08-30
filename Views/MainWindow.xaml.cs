using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using FilePilot.ViewModels;

namespace FilePilot.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = _viewModel;

        // Enable grouping on the navigation ListBox
        SetupNavigationGrouping();

        // Wire sidebar ListBox selection to NavigateToCommand
        NavigationListBox.SelectionChanged += (s, e) =>
        {
            if (NavigationListBox.SelectedItem is NavigationItem item)
            {
                _viewModel.NavigateToCommand.Execute(item);
            }
        };

        // Navigate to Dashboard on startup
        _viewModel.NavigateToCommand.Execute(_viewModel.NavigationItems[0]);
    }

    /// <summary>
    /// Groups the navigation items by their Category property.
    /// </summary>
    private void SetupNavigationGrouping()
    {
        var collectionView = CollectionViewSource.GetDefaultView(_viewModel.NavigationItems);
        collectionView.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
    }
}
