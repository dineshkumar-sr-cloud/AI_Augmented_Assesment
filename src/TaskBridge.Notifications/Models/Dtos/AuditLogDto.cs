namespace TaskBridge.Notifications.Models.Dtos;

/// <summary>
/// DTO for representing an audit log entry in API responses.
/// </summary>
public class AuditLogDto
{
    /// <summary>Gets or sets the audit log ID.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the project ID.</summary>
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>Gets or sets the event type.</summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>Gets or sets the entity type.</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Gets or sets the entity ID.</summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>Gets or sets the actor (user ID).</summary>
    public string ActorId { get; set; } = string.Empty;

    /// <summary>Gets or sets the actor's IP address.</summary>
    public string? ActorIpAddress { get; set; }

    /// <summary>Gets or sets the previous state snapshot (JSON).</summary>
    public string? PreviousState { get; set; }

    /// <summary>Gets or sets the new state snapshot (JSON).</summary>
    public string? NewState { get; set; }

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTime CreatedAt { get; set; }
}
