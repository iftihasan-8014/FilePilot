using Microsoft.Extensions.Hosting;

namespace FilePilot.Services;

public interface IMonitorService : IHostedService
{
    void StartMonitoring(string folderPath);
    void StopMonitoring(string folderPath);
    IEnumerable<string> GetMonitoredFolders();
}
