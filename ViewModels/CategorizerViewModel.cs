using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FilePilot.Services;
using System;

namespace FilePilot.ViewModels;

public partial class CategorizerViewModel : ObservableObject
{
    private readonly IFileOrganizerService _fileOrganizerService;
    private CancellationTokenSource? _cts;

    // ─── Folder Selection ─────────────────────────────────────────
    [ObservableProperty] private string _selectedFolderPath = string.Empty;

    // ─── Organize State ───────────────────────────────────────────
    [ObservableProperty] private bool _isOrganizing;
    [ObservableProperty] private bool _hasOrganized;
    [ObservableProperty] private string _statusMessage = "Select a folder to organize its files into categories.";
    
    // ─── Results ──────────────────────────────────────────────────
    [ObservableProperty] private int _organizedCount;

    public CategorizerViewModel(IFileOrganizerService fileOrganizerService)
    {
        _fileOrganizerService = fileOrganizerService;
    }

    [RelayCommand]
    private void BrowseFolder()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select a folder to categorize"
        };

        if (dialog.ShowDialog() == true)
        {
            SelectedFolderPath = dialog.FolderName;
            HasOrganized = false;
            StatusMessage = $"Ready to organize: {SelectedFolderPath}";
            OrganizedCount = 0;
        }
    }

    [RelayCommand(CanExecute = nameof(CanStartOrganize))]
    private async Task StartOrganizeAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedFolderPath) || !Directory.Exists(SelectedFolderPath))
        {
            StatusMessage = "⚠ The selected folder does not exist.";
            return;
        }

        try
        {
            _cts = new CancellationTokenSource();
            IsOrganizing = true;
            HasOrganized = false;
            StatusMessage = "Organizing files based on your active rules...";

            // Call the REAL backend service
            int count = await _fileOrganizerService.OrganizeByExtensionAsync(SelectedFolderPath, _cts.Token);

            if (_cts.Token.IsCancellationRequested)
            {
                StatusMessage = "Categorization was cancelled.";
                return;
            }

            OrganizedCount = count;
            HasOrganized = true;

            StatusMessage = OrganizedCount == 0
                ? "✓ No files needed organizing based on current active rules."
                : $"✓ Categorization complete. Successfully organized {OrganizedCount} files.";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Categorization was cancelled.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"⚠ Categorization failed: {ex.Message}";
        }
        finally
        {
            IsOrganizing = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    private bool CanStartOrganize() => !IsOrganizing && !string.IsNullOrWhiteSpace(SelectedFolderPath);

    [RelayCommand(CanExecute = nameof(CanCancelOrganize))]
    private void CancelOrganize()
    {
        _cts?.Cancel();
        StatusMessage = "Cancelling categorization...";
    }

    private bool CanCancelOrganize() => IsOrganizing;

    partial void OnIsOrganizingChanged(bool value)
    {
        StartOrganizeCommand.NotifyCanExecuteChanged();
        CancelOrganizeCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedFolderPathChanged(string value)
    {
        StartOrganizeCommand.NotifyCanExecuteChanged();
    }
}
