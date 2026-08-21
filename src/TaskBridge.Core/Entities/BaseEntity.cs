namespace TaskBridge.Core.Entities;

/// <summary>
/// Base entity class for all domain entities with common properties.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for this entity.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the organization ID for multi-tenant isolation.
    /// This property is required for all entities in a B2B SaaS context.
    /// </summary>
    public string OrganizationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when this entity was created (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
