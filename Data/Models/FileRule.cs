using System.ComponentModel.DataAnnotations;

namespace FilePilot.Data.Models;

public class FileRule
{
    public int Id { get; set; }
    
    public int FileCategoryId { get; set; }
    public FileCategory FileCategory { get; set; } = null!;
    
    [Required, MaxLength(50)]
    public string Extension { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string? CustomFolderPath { get; set; }
    
    public int Priority { get; set; }
    
    public bool IsEnabled { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
