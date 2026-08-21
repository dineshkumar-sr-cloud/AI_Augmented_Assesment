namespace TaskBridge.Core.Entities;

/// <summary>
/// Represents a notification record sent to a user.
/// Notifications inform users of project milestone changes and audit events.
/// </summary>
public class Notification : BaseEntity
{
    /// <summary>
    /// Gets or sets the user ID who will receive this notification.
    /// </summary>
    public string RecipientUserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project ID associated with this notification.
    /// </summary>
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the event type that triggered this notification.
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the notification message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the notification has been read.
    /// </summary>
    public bool IsRead { get; set; } = false;

    /// <summary>
    /// Gets or sets the timestamp when the notification was read (UTC).
    /// </summary>
    public DateTime? ReadAt { get; set; }
}
