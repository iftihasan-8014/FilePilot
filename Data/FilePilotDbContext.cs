using Microsoft.EntityFrameworkCore;
using FilePilot.Data.Models;
using FilePilot.Data.Models.Enums;

namespace FilePilot.Data;

public class FilePilotDbContext : DbContext
{
    public DbSet<FileCategory> FileCategories { get; set; } = null!;
    public DbSet<FileRule> FileRules { get; set; } = null!;
    public DbSet<ActivityLog> ActivityLogs { get; set; } = null!;
    public DbSet<TemporaryFileTracker> TemporaryFileTrackers { get; set; } = null!;
    public DbSet<ProjectContext> ProjectContexts { get; set; } = null!;
    public DbSet<ProjectContextFile> ProjectContextFiles { get; set; } = null!;

    public FilePilotDbContext(DbContextOptions<FilePilotDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Indexes
        modelBuilder.Entity<ActivityLog>()
            .HasIndex(a => a.CreatedAt);
            
        modelBuilder.Entity<FileRule>()
            .HasIndex(r => r.Extension);

        // ActivityLog configuration
        modelBuilder.Entity<ActivityLog>()
            .Property(a => a.ActionType)
            .HasConversion<string>();

        // TemporaryFileTracker configuration
        modelBuilder.Entity<TemporaryFileTracker>()
            .Property(t => t.Reason)
            .HasConversion<string>();

        // FileCategory -> FileRule Relationship (Cascade delete)
        modelBuilder.Entity<FileCategory>()
            .HasMany(c => c.FileRules)
            .WithOne(r => r.FileCategory)
            .HasForeignKey(r => r.FileCategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // ProjectContext -> ProjectContextFile Relationship
        modelBuilder.Entity<ProjectContext>()
            .HasMany(p => p.ProjectContextFiles)
            .WithOne(f => f.ProjectContext)
            .HasForeignKey(f => f.ProjectContextId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed Data for Categories and Rules
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FileCategory>().HasData(
            new FileCategory { Id = 1, Name = "Documents", FolderPath = "Documents", SortOrder = 1, IsEnabled = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new FileCategory { Id = 2, Name = "Images", FolderPath = "Images", SortOrder = 2, IsEnabled = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new FileCategory { Id = 3, Name = "Videos", FolderPath = "Videos", SortOrder = 3, IsEnabled = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new FileCategory { Id = 4, Name = "Audio", FolderPath = "Audio", SortOrder = 4, IsEnabled = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new FileCategory { Id = 5, Name = "Archives", FolderPath = "Archives", SortOrder = 5, IsEnabled = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new FileCategory { Id = 6, Name = "Code", FolderPath = "Code", SortOrder = 6, IsEnabled = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );

        modelBuilder.Entity<FileRule>().HasData(
            // Documents
            new FileRule { Id = 1, FileCategoryId = 1, Extension = ".pdf", Priority = 1, IsEnabled = true, CreatedAt = DateTime.UtcNow },
            new FileRule { Id = 2, FileCategoryId = 1, Extension = ".doc", Priority = 1, IsEnabled = true, CreatedAt = DateTime.UtcNow },
            new FileRule { Id = 3, FileCategoryId = 1, Extension = ".docx", Priority = 1, IsEnabled = true, CreatedAt = DateTime.UtcNow },
            new FileRule { Id = 4, FileCategoryId = 1, Extension = ".txt", Priority = 1, IsEnabled = true, CreatedAt = DateTime.UtcNow },
            new FileRule { Id = 5, FileCategoryId = 1, Extension = ".xlsx", Priority = 1, IsEnabled = true, CreatedAt = DateTime.UtcNow },
            new FileRule { Id = 6, FileCategoryId = 1, Extension = ".pptx", Priority = 1, IsEnabled = true, CreatedAt = DateTime.UtcNow },
            
            // Images
            new FileRule { Id = 7, FileCategoryId = 2, Extension = ".jpg", Priority = 1, IsEnabled = true, CreatedAt = DateTime.UtcNow },
            new FileRule { Id = 8, FileCategoryId = 2, Extension = ".jpeg", Priority = 1, IsEnabled = true, CreatedAt = DateTime.UtcNow },
            new FileRule { Id = 9, FileCategoryId = 2, Extension = ".png", Priority = 1, IsEnabled = true, CreatedAt = DateTime.UtcNow },
            new FileRule { Id = 10, FileCategoryId = 2, Extension = ".gif", Priority = 1, IsEnabled = true, CreatedAt = DateTime.UtcNow },
            
            // Videos
            new FileRule { Id = 11, FileCategoryId = 3, Extension = ".mp4", Priority = 1, IsEnabled = true, CreatedAt = DateTime.UtcNow },
            new FileRule { Id = 12, FileCategoryId = 3, Extension = ".mkv", Priority = 1, IsEnabled = true, CreatedAt = DateTime.UtcNow },
            
            // Audio
            new FileRule { Id = 13, FileCategoryId = 4, Extension = ".mp3", Priority = 1, IsEnabled = true, CreatedAt = DateTime.UtcNow },
            new FileRule { Id = 14, FileCategoryId = 4, Extension = ".wav", Priority = 1, IsEnabled = true, CreatedAt = DateTime.UtcNow },
            
            // Archives
            new FileRule { Id = 15, FileCategoryId = 5, Extension = ".zip", Priority = 1, IsEnabled = true, CreatedAt = DateTime.UtcNow },
            new FileRule { Id = 16, FileCategoryId = 5, Extension = ".rar", Priority = 1, IsEnabled = true, CreatedAt = DateTime.UtcNow },
            new FileRule { Id = 17, FileCategoryId = 5, Extension = ".7z", Priority = 1, IsEnabled = true, CreatedAt = DateTime.UtcNow },
            
            // Code
            new FileRule { Id = 18, FileCategoryId = 6, Extension = ".cs", Priority = 1, IsEnabled = true, CreatedAt = DateTime.UtcNow },
            new FileRule { Id = 19, FileCategoryId = 6, Extension = ".js", Priority = 1, IsEnabled = true, CreatedAt = DateTime.UtcNow },
            new FileRule { Id = 20, FileCategoryId = 6, Extension = ".json", Priority = 1, IsEnabled = true, CreatedAt = DateTime.UtcNow }
        );
    }
}
