using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FilePilot.Services;
using FilePilot.Services.Models;

namespace FilePilot.ViewModels;

/// <summary>
/// ViewModel for the Duplicate Finder feature.
/// Wires the UI to <see cref="IFileOrganizerService.FindDuplicatesAsync"/>
/// for real, hash-based duplicate detection.
/// </summary>
public partial class DuplicateFinderViewModel : ObservableObject
{
    private readonly IFileOrganizerService _fileOrganizerService;
    private CancellationTokenSource? _cts;

    // ─── Folder Selection ─────────────────────────────────────────
    [ObservableProperty] private string _selectedFolderPath = string.Empty;

    // ─── Scan State ───────────────────────────────────────────────
    [ObservableProperty] private bool _isScanning;
    [ObservableProperty] private bool _hasScanCompleted;
    [ObservableProperty] private string _statusMessage = "Select a folder to begin scanning for duplicate files.";

    // ─── Results ──────────────────────────────────────────────────
    [ObservableProperty] private ObservableCollection<DuplicateGroupItem> _duplicateGroups = [];
    [ObservableProperty] private int _totalGroups;
    [ObservableProperty] private int _totalDuplicateFiles;
    [ObservableProperty] private long _totalWastedBytes;
    [ObservableProperty] private string _totalWastedDisplay = "0 B";

    // ═══════════════════════════════════════════════════════════════
    // Constructor
    // ═══════════════════════════════════════════════════════════════

    public DuplicateFinderViewModel(IFileOrganizerService fileOrganizerService)
    {
        _fileOrganizerService = fileOrganizerService;
    }

    // ═══════════════════════════════════════════════════════════════
    // Commands
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Opens a WPF folder dialog and sets <see cref="SelectedFolderPath"/>.
    /// </summary>
    [RelayCommand]
    private void BrowseFolder()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select a folder to scan for duplicates"
        };

        if (dialog.ShowDialog() == true)
        {
            SelectedFolderPath = dialog.FolderName;
            // Reset previous results when a new folder is chosen
            HasScanCompleted = false;
            DuplicateGroups.Clear();
            StatusMessage = $"Ready to scan: {SelectedFolderPath}";
        }
    }

    /// <summary>
    /// Executes the real duplicate-finding logic via IFileOrganizerService.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanStartScan))]
    private async Task StartScanAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedFolderPath) || !Directory.Exists(SelectedFolderPath))
        {
            StatusMessage = "⚠ The selected folder does not exist.";
            return;
        }

        try
        {
            _cts = new CancellationTokenSource();
            IsScanning = true;
            HasScanCompleted = false;
            DuplicateGroups.Clear();
            StatusMessage = "Scanning... Computing file hashes (this may take a while for large folders).";

            // Call the REAL backend service
            var results = await _fileOrganizerService.FindDuplicatesAsync(SelectedFolderPath, _cts.Token);

            if (_cts.Token.IsCancellationRequested)
            {
                StatusMessage = "Scan was cancelled.";
                return;
            }

            // Transform service results into UI-friendly view models
            int groupIndex = 0;
            foreach (var group in results)
            {
                groupIndex++;
                DuplicateGroups.Add(new DuplicateGroupItem
                {
                    GroupNumber = groupIndex,
                    FileHash = group.FileHash.Length > 16 ? group.FileHash[..16] + "…" : group.FileHash,
                    FileSizeBytes = group.FileSizeBytes,
                    DuplicateCount = group.FilePaths.Count,
                    Files = new ObservableCollection<DuplicateFileItem>(
                        group.FilePaths.Select(p => new DuplicateFileItem
                        {
                            FullPath = p,
                            FileName = Path.GetFileName(p),
                            Directory = Path.GetDirectoryName(p) ?? string.Empty,
                            SizeBytes = group.FileSizeBytes
                        }))
                });
            }

            // Update summary statistics
            TotalGroups = results.Count;
            TotalDuplicateFiles = results.Sum(g => g.FilePaths.Count - 1); // -1 per group = the "extra" copies
            TotalWastedBytes = results.Sum(g => g.FileSizeBytes * (g.FilePaths.Count - 1));
            TotalWastedDisplay = FormatBytes(TotalWastedBytes);
            HasScanCompleted = true;

            StatusMessage = TotalGroups == 0
                ? "✓ No duplicates found — your folder is clean!"
                : $"✓ Scan complete. Found {TotalGroups} duplicate groups ({TotalDuplicateFiles} extra files wasting {TotalWastedDisplay}).";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Scan was cancelled.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"⚠ Scan failed: {ex.Message}";
        }
        finally
        {
            IsScanning = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    private bool CanStartScan() => !IsScanning && !string.IsNullOrWhiteSpace(SelectedFolderPath);

    /// <summary>
    /// Cancels an in-progress scan.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCancelScan))]
    private void CancelScan()
    {
        _cts?.Cancel();
        StatusMessage = "Cancelling scan...";
    }

    private bool CanCancelScan() => IsScanning;

    /// <summary>
    /// Opens the containing folder in Windows Explorer for a specific file.
    /// </summary>
    [RelayCommand]
    private void OpenInExplorer(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) return;

        System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{filePath}\"");
    }

    /// <summary>
    /// Deletes a duplicate file (with confirmation built into the UI state).
    /// </summary>
    [RelayCommand]
    private void DeleteFile(DuplicateFileItem? item)
    {
        if (item is null || !File.Exists(item.FullPath)) return;

        try
        {
            File.Delete(item.FullPath);
            item.IsDeleted = true;

            // Remove from the parent group
            foreach (var group in DuplicateGroups)
            {
                if (group.Files.Remove(item))
                {
                    group.DuplicateCount = group.Files.Count;

                    // Update wasted bytes
                    TotalDuplicateFiles--;
                    TotalWastedBytes -= item.SizeBytes;
                    TotalWastedDisplay = FormatBytes(TotalWastedBytes);

                    // If only 1 file left, it's no longer a "duplicate group"
                    if (group.Files.Count <= 1)
                    {
                        DuplicateGroups.Remove(group);
                        TotalGroups--;
                    }

                    break;
                }
            }

            StatusMessage = $"Deleted: {item.FileName}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"⚠ Could not delete {item.FileName}: {ex.Message}";
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // Property change notification for CanExecute
    // ═══════════════════════════════════════════════════════════════

    partial void OnIsScanningChanged(bool value)
    {
        StartScanCommand.NotifyCanExecuteChanged();
        CancelScanCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedFolderPathChanged(string value)
    {
        StartScanCommand.NotifyCanExecuteChanged();
    }

    // ═══════════════════════════════════════════════════════════════
    // Helpers
    // ═══════════════════════════════════════════════════════════════

    private static string FormatBytes(long bytes)
    {
        string[] suffixes = ["B", "KB", "MB", "GB", "TB"];
        int order = 0;
        double size = bytes;
        while (size >= 1024 && order < suffixes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        return $"{size:0.##} {suffixes[order]}";
    }
}

// ═══════════════════════════════════════════════════════════════════
// Supporting Display Models
// ═══════════════════════════════════════════════════════════════════

/// <summary>
/// Represents one group of files that share the same hash.
/// </summary>
public partial class DuplicateGroupItem : ObservableObject
{
    [ObservableProperty] private int _groupNumber;
    [ObservableProperty] private string _fileHash = string.Empty;
    [ObservableProperty] private long _fileSizeBytes;
    [ObservableProperty] private int _duplicateCount;
    [ObservableProperty] private ObservableCollection<DuplicateFileItem> _files = [];
}

/// <summary>
/// Represents a single file within a duplicate group.
/// </summary>
public partial class DuplicateFileItem : ObservableObject
{
    [ObservableProperty] private string _fullPath = string.Empty;
    [ObservableProperty] private string _fileName = string.Empty;
    [ObservableProperty] private string _directory = string.Empty;
    [ObservableProperty] private long _sizeBytes;
    [ObservableProperty] private bool _isDeleted;
}
