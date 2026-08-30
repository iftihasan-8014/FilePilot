namespace FilePilot.Services.Models;

public class DashboardStats
{
    public int FilesOrganized { get; set; }
    public int DuplicatesFound { get; set; }
    public string StorageSaved { get; set; } = "0 MB";
    public int ActiveRules { get; set; }
    public int TotalFilesTracked { get; set; }
}
