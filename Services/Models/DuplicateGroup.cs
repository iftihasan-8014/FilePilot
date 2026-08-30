namespace FilePilot.Services.Models;

public class DuplicateGroup
{
    public string FileHash { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public List<string> FilePaths { get; set; } = new();
}
