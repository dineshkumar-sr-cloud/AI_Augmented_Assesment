namespace TaskBridge.Notifications.Models.Dtos;

/// <summary>
/// DTO for representing a notification in API responses.
/// </summary>
public class NotificationDto
{
    /// <summary>Gets or sets the notification ID.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the recipient user ID.</summary>
    public string RecipientUserId { get; set; } = string.Empty;

    /// <summary>Gets or sets the project ID.</summary>
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>Gets or sets the event type.</summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>Gets or sets the notification message.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the notification has been read.</summary>
    public bool IsRead { get; set; }

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the read timestamp.</summary>
    public DateTime? ReadAt { get; set; }
}
