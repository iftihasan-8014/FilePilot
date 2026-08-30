using System.ComponentModel.DataAnnotations;

namespace FilePilot.Data.Models;

public class FileCategory
{
    public int Id { get; set; }
    
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string FolderPath { get; set; } = string.Empty;
    
    public int SortOrder { get; set; }
    
    public bool IsEnabled { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<FileRule> FileRules { get; set; } = new List<FileRule>();
}
