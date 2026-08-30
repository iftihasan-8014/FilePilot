using Microsoft.EntityFrameworkCore;
using System.IO;
using FilePilot.Data;
using FilePilot.Data.Models;
using FilePilot.Data.Models.Enums;
using FilePilot.Services.Models;

namespace FilePilot.Services;

public class ActivityService : IActivityService
{
    private readonly FilePilotDbContext _dbContext;

    public ActivityService(FilePilotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task LogActivityAsync(ActionType actionType, string sourcePath, string? destinationPath, string fileName, long fileSizeBytes, string? fileHash, string? serviceName, string? description, CancellationToken ct = default)
    {
        var log = new ActivityLog
        {
            ActionType = actionType,
            SourcePath = sourcePath,
            DestinationPath = destinationPath,
            FileName = fileName,
            FileSizeBytes = fileSizeBytes,
            FileHash = fileHash,
            ServiceName = serviceName,
            Description = description,
            IsUndone = false,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.ActivityLogs.Add(log);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<List<ActivityLog>> GetRecentActivityAsync(int count, CancellationToken ct = default)
    {
        return await _dbContext.ActivityLogs
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task<bool> UndoActivityAsync(long activityLogId, CancellationToken ct = default)
    {
        var log = await _dbContext.ActivityLogs.FirstOrDefaultAsync(a => a.Id == activityLogId, ct);
        if (log == null || log.IsUndone) return false;

        bool success = false;
        try
        {
            if (log.ActionType == ActionType.Move && !string.IsNullOrEmpty(log.DestinationPath) && File.Exists(log.DestinationPath))
            {
                // Move back to source
                var directory = Path.GetDirectoryName(log.SourcePath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                if (!File.Exists(log.SourcePath))
                {
                    File.Move(log.DestinationPath, log.SourcePath);
                    success = true;
                }
            }
            // Other actions could be implemented here...
        }
        catch
        {
            return false;
        }

        if (success)
        {
            log.IsUndone = true;
            log.UndoneAt = DateTime.UtcNow;
            
            // Log the undo action itself
            var undoLog = new ActivityLog
            {
                ActionType = ActionType.Undo,
                SourcePath = log.DestinationPath ?? string.Empty,
                DestinationPath = log.SourcePath,
                FileName = log.FileName,
                FileSizeBytes = log.FileSizeBytes,
                ServiceName = "ActivityService",
                Description = $"Undid action {log.Id}",
                IsUndone = false,
                CreatedAt = DateTime.UtcNow
            };
            
            _dbContext.ActivityLogs.Add(undoLog);
            await _dbContext.SaveChangesAsync(ct);
        }

        return success;
    }

    public async Task<DashboardStats> GetDashboardStatsAsync(CancellationToken ct = default)
    {
        var logs = await _dbContext.ActivityLogs.AsNoTracking().ToListAsync(ct);
        
        long totalBytesSaved = logs.Where(l => l.ActionType == ActionType.Delete || l.ActionType == ActionType.Quarantine).Sum(l => l.FileSizeBytes);
        string storageSaved = FormatBytes(totalBytesSaved);

        var stats = new DashboardStats
        {
            FilesOrganized = logs.Count(l => l.ActionType == ActionType.Move),
            DuplicatesFound = logs.Count(l => l.ActionType == ActionType.Delete && l.Description != null && l.Description.Contains("duplicate", StringComparison.OrdinalIgnoreCase)),
            StorageSaved = storageSaved,
            ActiveRules = await _dbContext.FileRules.CountAsync(r => r.IsEnabled, ct),
            TotalFilesTracked = logs.Select(l => l.FileName).Distinct().Count()
        };

        return stats;
    }

    private static string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        decimal number = bytes;
        if (number == 0) return "0 B";
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        return string.Format("{0:n1} {1}", number, suffixes[counter]);
    }
}
