using Microsoft.EntityFrameworkCore;
using TaskBridge.Core.Entities;

namespace TaskBridge.Projects.Data;

/// <summary>
/// Database context for the Project Service.
/// Configures entities, relationships, and indexes for optimal data access.
/// </summary>
public class ProjectDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the ProjectDbContext class.
    /// </summary>
    /// <param name="options">The database context options.</param>
    public ProjectDbContext(DbContextOptions<ProjectDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the Projects DbSet.
    /// </summary>
    public DbSet<Project> Projects { get; set; } = null!;

    /// <summary>
    /// Configures the database schema and entity mappings.
    /// </summary>
    /// <param name="modelBuilder">The model builder for entity configuration.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Project entity
        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("Projects");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .ValueGeneratedNever();

            entity.Property(e => e.OrganizationId)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.TeamId)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Description)
                .HasMaxLength(1000);

            entity.Property(e => e.MilestoneStatus)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("PLANNING");

            entity.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.UpdatedAt);
            entity.Property(e => e.UpdatedBy).HasMaxLength(50);

            // Indexes for multi-tenant queries
            entity.HasIndex(e => new { e.OrganizationId, e.TeamId })
                .HasName("IX_Projects_OrgId_TeamId");

            entity.HasIndex(e => new { e.OrganizationId, e.CreatedAt })
                .HasName("IX_Projects_OrgId_CreatedAt");

            entity.HasIndex(e => new { e.OrganizationId, e.MilestoneStatus })
                .HasName("IX_Projects_OrgId_Status");
        });
    }
}
