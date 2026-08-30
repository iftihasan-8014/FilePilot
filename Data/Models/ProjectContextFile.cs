using System.ComponentModel.DataAnnotations;

namespace FilePilot.Data.Models;

public class ProjectContextFile
{
    public long Id { get; set; }
    
    public int ProjectContextId { get; set; }
    public ProjectContext ProjectContext { get; set; } = null!;
    
    [Required, MaxLength(1024)]
    public string FilePath { get; set; } = string.Empty;
    
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
