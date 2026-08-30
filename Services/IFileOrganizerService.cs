using FilePilot.Services.Models;

namespace FilePilot.Services;

public interface IFileOrganizerService
{
    Task<int> OrganizeByExtensionAsync(string sourcePath, CancellationToken ct = default);
    Task<int> OrganizeByDateAsync(string sourcePath, string dateFormat, CancellationToken ct = default);
    Task<List<DuplicateGroup>> FindDuplicatesAsync(string sourcePath, CancellationToken ct = default);
    Task<int> BatchRenameAsync(string folderPath, string pattern, CancellationToken ct = default);
    Task<bool> UndoLastActionAsync(CancellationToken ct = default);
    Task<bool> UndoActionAsync(long activityLogId, CancellationToken ct = default);
}
