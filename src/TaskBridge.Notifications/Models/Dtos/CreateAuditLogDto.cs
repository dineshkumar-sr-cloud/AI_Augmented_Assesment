namespace TaskBridge.Notifications.Models.Dtos;

/// <summary>
/// DTO for creating an audit log entry.
/// </summary>
public class CreateAuditLogDto
{
    /// <summary>Gets or sets the project ID associated with this audit entry.</summary>
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>Gets or sets the event type (e.g., PROJECT_CREATED, MILESTONE_UPDATED).</summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>Gets or sets the entity type that changed (e.g., PROJECT, MILESTONE).</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Gets or sets the entity ID that was changed.</summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>Gets or sets the actor (user ID) who initiated the change.</summary>
    public string ActorId { get; set; } = string.Empty;

    /// <summary>Gets or sets the IP address of the actor (optional but recommended for security).</summary>
    public string? ActorIpAddress { get; set; }

    /// <summary>Gets or sets the JSON snapshot of the previous state before the change.</summary>
    public string? PreviousState { get; set; }

    /// <summary>Gets or sets the JSON snapshot of the new state after the change.</summary>
    public string? NewState { get; set; }
}
