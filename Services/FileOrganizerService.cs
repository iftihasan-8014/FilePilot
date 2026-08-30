using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.IO;
using FilePilot.Data;
using FilePilot.Data.Models.Enums;
using FilePilot.Services.Models;

namespace FilePilot.Services;

public class FileOrganizerService : IFileOrganizerService
{
    private readonly FilePilotDbContext _dbContext;
    private readonly IActivityService _activityService;

    public FileOrganizerService(FilePilotDbContext dbContext, IActivityService activityService)
    {
        _dbContext = dbContext;
        _activityService = activityService;
    }

    public async Task<int> OrganizeByExtensionAsync(string sourcePath, CancellationToken ct = default)
    {
        if (!Directory.Exists(sourcePath)) return 0;

        var files = Directory.GetFiles(sourcePath);
        if (files.Length == 0) return 0;

        // Get active rules
        var rules = await _dbContext.FileRules
            .Include(r => r.FileCategory)
            .Where(r => r.IsEnabled && r.FileCategory.IsEnabled)
            .OrderBy(r => r.Priority)
            .ToListAsync(ct);

        int organizedCount = 0;

        foreach (var file in files)
        {
            if (ct.IsCancellationRequested) break;

            var fileInfo = new FileInfo(file);
            var extension = fileInfo.Extension.ToLowerInvariant();

            // Find matching rule
            var rule = rules.FirstOrDefault(r => r.Extension.Equals(extension, StringComparison.OrdinalIgnoreCase));
            if (rule != null)
            {
                var categoryFolder = rule.CustomFolderPath ?? rule.FileCategory.FolderPath;
                var destinationDir = Path.Combine(sourcePath, categoryFolder);

                if (!Directory.Exists(destinationDir))
                {
                    Directory.CreateDirectory(destinationDir);
                }

                var destinationPath = Path.Combine(destinationDir, fileInfo.Name);

                // Ensure unique name if exists
                destinationPath = EnsureUniqueFileName(destinationPath);

                try
                {
                    File.Move(file, destinationPath);
                    
                    await _activityService.LogActivityAsync(
                        ActionType.Move,
                        file,
                        destinationPath,
                        fileInfo.Name,
                        fileInfo.Length,
                        null,
                        "FileOrganizerService",
                        $"Organized by extension into {rule.FileCategory.Name}",
                        ct
                    );
                    
                    organizedCount++;
                }
                catch
                {
                    // Log error or continue
                }
            }
        }

        return organizedCount;
    }

    public async Task<int> OrganizeByDateAsync(string sourcePath, string dateFormat, CancellationToken ct = default)
    {
        // Implementation left for future scope, return 0 for now
        return await Task.FromResult(0);
    }

    public async Task<List<DuplicateGroup>> FindDuplicatesAsync(string sourcePath, CancellationToken ct = default)
    {
        if (!Directory.Exists(sourcePath)) return new List<DuplicateGroup>();

        var allFiles = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);
        
        // Group by size first (quick check)
        var sizeGroups = allFiles
            .Select(f => new FileInfo(f))
            .GroupBy(f => f.Length)
            .Where(g => g.Count() > 1)
            .ToList();

        var duplicateGroups = new List<DuplicateGroup>();

        foreach (var sizeGroup in sizeGroups)
        {
            var hashGroups = new Dictionary<string, List<string>>();

            foreach (var file in sizeGroup)
            {
                if (ct.IsCancellationRequested) break;

                try
                {
                    string hash = ComputeFileHash(file.FullName);
                    if (!hashGroups.ContainsKey(hash))
                    {
                        hashGroups[hash] = new List<string>();
                    }
                    hashGroups[hash].Add(file.FullName);
                }
                catch
                {
                    // Skip files that can't be read
                }
            }

            foreach (var hashGroup in hashGroups.Where(g => g.Value.Count > 1))
            {
                duplicateGroups.Add(new DuplicateGroup
                {
                    FileHash = hashGroup.Key,
                    FileSizeBytes = sizeGroup.Key,
                    FilePaths = hashGroup.Value
                });
            }
        }

        return duplicateGroups;
    }

    public async Task<int> BatchRenameAsync(string folderPath, string pattern, CancellationToken ct = default)
    {
        return await Task.FromResult(0);
    }

    public async Task<bool> UndoLastActionAsync(CancellationToken ct = default)
    {
        var lastLog = await _dbContext.ActivityLogs
            .Where(a => !a.IsUndone && (a.ActionType == ActionType.Move || a.ActionType == ActionType.Rename || a.ActionType == ActionType.Quarantine))
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (lastLog == null) return false;

        return await _activityService.UndoActivityAsync(lastLog.Id, ct);
    }

    public async Task<bool> UndoActionAsync(long activityLogId, CancellationToken ct = default)
    {
        return await _activityService.UndoActivityAsync(activityLogId, ct);
    }

    private string ComputeFileHash(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hashBytes = sha256.ComputeHash(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    private string EnsureUniqueFileName(string filePath)
    {
        if (!File.Exists(filePath)) return filePath;

        string directory = Path.GetDirectoryName(filePath) ?? string.Empty;
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        string extension = Path.GetExtension(filePath);
        
        int counter = 1;
        string newFilePath;
        do
        {
            newFilePath = Path.Combine(directory, $"{fileNameWithoutExtension} ({counter}){extension}");
            counter++;
        } while (File.Exists(newFilePath));

        return newFilePath;
    }
}
