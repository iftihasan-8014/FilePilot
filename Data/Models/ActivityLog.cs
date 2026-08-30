using System.ComponentModel.DataAnnotations;
using FilePilot.Data.Models.Enums;

namespace FilePilot.Data.Models;

public class ActivityLog
{
    public long Id { get; set; }
    
    public ActionType ActionType { get; set; }
    
    [Required, MaxLength(1024)]
    public string SourcePath { get; set; } = string.Empty;
    
    [MaxLength(1024)]
    public string? DestinationPath { get; set; }
    
    [Required, MaxLength(255)]
    public string FileName { get; set; } = string.Empty;
    
    public long FileSizeBytes { get; set; }
    
    [MaxLength(64)]
    public string? FileHash { get; set; }
    
    [MaxLength(100)]
    public string? ServiceName { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public bool IsUndone { get; set; }
    
    public DateTime? UndoneAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
