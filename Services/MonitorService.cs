using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.IO;

namespace FilePilot.Services;

public class MonitorService : BackgroundService, IMonitorService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<string, FileSystemWatcher> _watchers = new();
    private readonly ConcurrentDictionary<string, DateTime> _recentlyProcessedFiles = new();
    private readonly TimeSpan _debounceTime = TimeSpan.FromSeconds(2);

    public MonitorService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Add default Downloads folder monitoring for the current user
        string downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        if (Directory.Exists(downloadsFolder))
        {
            StartMonitoring(downloadsFolder);
        }

        return Task.CompletedTask;
    }

    public void StartMonitoring(string folderPath)
    {
        if (_watchers.ContainsKey(folderPath)) return;
        if (!Directory.Exists(folderPath)) return;

        var watcher = new FileSystemWatcher(folderPath)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            Filter = "*.*",
            EnableRaisingEvents = true
        };

        watcher.Created += OnFileCreated;
        watcher.Changed += OnFileCreated;
        
        _watchers.TryAdd(folderPath, watcher);
    }

    public void StopMonitoring(string folderPath)
    {
        if (_watchers.TryRemove(folderPath, out var watcher))
        {
            watcher.EnableRaisingEvents = false;
            watcher.Created -= OnFileCreated;
            watcher.Changed -= OnFileCreated;
            watcher.Dispose();
        }
    }

    public IEnumerable<string> GetMonitoredFolders() => _watchers.Keys;

    private async void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        try
        {
            // Debounce logic to prevent processing the same file multiple times
            if (_recentlyProcessedFiles.TryGetValue(e.FullPath, out var lastProcessedTime))
            {
                if (DateTime.UtcNow - lastProcessedTime < _debounceTime) return;
            }

            _recentlyProcessedFiles[e.FullPath] = DateTime.UtcNow;

            // Cleanup old debounce entries
            foreach (var key in _recentlyProcessedFiles.Keys.ToList())
            {
                if (DateTime.UtcNow - _recentlyProcessedFiles[key] > TimeSpan.FromMinutes(1))
                {
                    _recentlyProcessedFiles.TryRemove(key, out _);
                }
            }

            // Add a small delay to ensure the file is completely written before moving
            await Task.Delay(1000);

            if (!File.Exists(e.FullPath)) return;

            using var scope = _serviceProvider.CreateScope();
            var organizerService = scope.ServiceProvider.GetRequiredService<IFileOrganizerService>();

            string folderPath = Path.GetDirectoryName(e.FullPath) ?? string.Empty;
            if (string.IsNullOrEmpty(folderPath)) return;
            
            // Since OrganizeByExtensionAsync processes a whole folder, we could just trigger it for the folder,
            // or we could create an overload that processes a single file. For now, calling it on the folder
            // might process other unorganized files as well, which is fine.
            await organizerService.OrganizeByExtensionAsync(folderPath);
        }
        catch
        {
            // Ignore exceptions to prevent the background service from crashing the app
        }
    }

    public override void Dispose()
    {
        foreach (var watcher in _watchers.Values)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }
        _watchers.Clear();
        base.Dispose();
    }
}
