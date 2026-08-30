using System.ComponentModel.DataAnnotations;
using FilePilot.Data.Models.Enums;

namespace FilePilot.Data.Models;

public class TemporaryFileTracker
{
    public int Id { get; set; }
    
    [Required, MaxLength(1024)]
    public string FilePath { get; set; } = string.Empty;
    
    [MaxLength(1024)]
    public string? OriginalPath { get; set; }
    
    [Required, MaxLength(255)]
    public string FileName { get; set; } = string.Empty;
    
    public TrackerReason Reason { get; set; }
    
    public DateTime ExpiresAt { get; set; }
    
    public bool IsProcessed { get; set; }
    
    public DateTime? ProcessedAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
