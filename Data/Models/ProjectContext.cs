using System.ComponentModel.DataAnnotations;

namespace FilePilot.Data.Models;

public class ProjectContext
{
    public int Id { get; set; }
    
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [MaxLength(1024)]
    public string? BaseFolderPath { get; set; }
    
    public bool IsActive { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ProjectContextFile> ProjectContextFiles { get; set; } = new List<ProjectContextFile>();
}
