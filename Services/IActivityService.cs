using FilePilot.Data.Models;
using FilePilot.Data.Models.Enums;
using FilePilot.Services.Models;

namespace FilePilot.Services;

public interface IActivityService
{
    Task LogActivityAsync(ActionType actionType, string sourcePath, string? destinationPath, string fileName, long fileSizeBytes, string? fileHash, string? serviceName, string? description, CancellationToken ct = default);
    Task<List<ActivityLog>> GetRecentActivityAsync(int count, CancellationToken ct = default);
    Task<bool> UndoActivityAsync(long activityLogId, CancellationToken ct = default);
    Task<DashboardStats> GetDashboardStatsAsync(CancellationToken ct = default);
}
