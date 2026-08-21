using Microsoft.EntityFrameworkCore;
using TaskBridge.Core.Entities;

namespace TaskBridge.Notifications.Data;

/// <summary>
/// Database context for the Notification & Audit Service.
/// Manages audit logs (immutable) and notification records.
/// </summary>
public class NotificationDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the NotificationDbContext class.
    /// </summary>
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the AuditLogs DbSet.
    /// </summary>
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Notifications DbSet.
    /// </summary>
    public DbSet<Notification> Notifications { get; set; } = null!;

    /// <summary>
    /// Configures the database schema and entity mappings.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure AuditLog entity (immutable)
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .ValueGeneratedNever();

            entity.Property(e => e.OrganizationId)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.ProjectId)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.EventType)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.EntityType)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.EntityId)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.ActorId)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.ActorIpAddress)
                .HasMaxLength(45); // IPv6 max length

            entity.Property(e => e.PreviousState)
                .HasColumnType("NVARCHAR(MAX)");

            entity.Property(e => e.NewState)
                .HasColumnType("NVARCHAR(MAX)");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd();

            // Indexes for audit queries
            entity.HasIndex(e => new { e.OrganizationId, e.ProjectId })
                .HasName("IX_AuditLogs_OrgId_ProjectId");

            entity.HasIndex(e => new { e.OrganizationId, e.CreatedAt })
                .HasName("IX_AuditLogs_OrgId_CreatedAt");

            entity.HasIndex(e => new { e.OrganizationId, e.EventType })
                .HasName("IX_AuditLogs_OrgId_EventType");
        });

        // Configure Notification entity
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notifications");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .ValueGeneratedNever();

            entity.Property(e => e.OrganizationId)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.RecipientUserId)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.ProjectId)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.EventType)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Message)
                .IsRequired()
                .HasColumnType("NVARCHAR(MAX)");

            entity.Property(e => e.IsRead)
                .HasDefaultValue(false);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd();

            // Indexes for notification queries
            entity.HasIndex(e => new { e.OrganizationId, e.RecipientUserId, e.IsRead })
                .HasName("IX_Notifications_OrgId_UserId_Read");

            entity.HasIndex(e => new { e.OrganizationId, e.CreatedAt })
                .HasName("IX_Notifications_OrgId_CreatedAt");
        });
    }
}
