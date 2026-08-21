namespace TaskBridge.Core.Entities;

/// <summary>
/// Represents an immutable audit log entry.
/// Audit entries capture state changes for compliance and tracking purposes.
/// These entries cannot be modified or deleted once created.
/// </summary>
public class AuditLog : BaseEntity
{
    /// <summary>
    /// Gets or sets the project ID associated with this audit entry.
    /// </summary>
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the event type (e.g., CREATED, UPDATED, DELETED, MILESTONE_REOPENED).
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the entity type that changed (e.g., PROJECT, MILESTONE).
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the entity ID that was changed.
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the actor (user ID) who initiated the change.
    /// </summary>
    public string ActorId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the IP address of the actor (for security and compliance).
    /// </summary>
    public string? ActorIpAddress { get; set; }

    /// <summary>
    /// Gets or sets the JSON snapshot of the previous state before the change.
    /// </summary>
    public string? PreviousState { get; set; }

    /// <summary>
    /// Gets or sets the JSON snapshot of the new state after the change.
    /// </summary>
    public string? NewState { get; set; }

    // NOTE: AuditLog has no UpdatedAt or UpdatedBy properties by design.
    // Immutability is enforced: once created, an audit entry cannot be modified.
}
