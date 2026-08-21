using TaskBridge.Core.Entities;
using TaskBridge.Core.Exceptions;
using TaskBridge.Notifications.Data;
using TaskBridge.Notifications.Models.Dtos;

namespace TaskBridge.Notifications.Services;

/// <summary>
/// Service interface for notification operations.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Creates a new notification.
    /// </summary>
    Task<NotificationDto> CreateNotificationAsync(string recipientUserId, string projectId,
        string eventType, string message, string organizationId);

    /// <summary>
    /// Creates multiple notifications in batch (e.g., for all team members).
    /// </summary>
    Task<List<NotificationDto>> CreateBatchNotificationsAsync(List<string> recipientUserIds,
        string projectId, string eventType, string message, string organizationId);

    /// <summary>
    /// Gets all unread notifications for a user.
    /// </summary>
    Task<List<NotificationDto>> GetUnreadNotificationsAsync(string userId, string organizationId);

    /// <summary>
    /// Gets all notifications for a user.
    /// </summary>
    Task<List<NotificationDto>> GetNotificationsAsync(string userId, string organizationId);

    /// <summary>
    /// Marks a notification as read.
    /// </summary>
    Task<NotificationDto> MarkAsReadAsync(string notificationId, string organizationId);
}

/// <summary>
/// Service for managing notification operations.
/// Handles creation and delivery of notifications to users.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly ILogger<NotificationService> _logger;

    /// <summary>
    /// Initializes a new instance of the NotificationService class.
    /// </summary>
    public NotificationService(INotificationRepository repository, ILogger<NotificationService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new notification.
    /// </summary>
    public async Task<NotificationDto> CreateNotificationAsync(string recipientUserId, string projectId,
        string eventType, string message, string organizationId)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(recipientUserId))
            throw new ValidationException("Recipient user ID is required");

        if (string.IsNullOrWhiteSpace(projectId))
            throw new ValidationException("Project ID is required");

        if (string.IsNullOrWhiteSpace(eventType))
            throw new ValidationException("Event type is required");

        if (string.IsNullOrWhiteSpace(message))
            throw new ValidationException("Message is required");

        // Validate organization context
        if (string.IsNullOrEmpty(organizationId))
            throw new UnauthorizedException("Organization context is required");

        var notification = new Notification
        {
            Id = Guid.NewGuid().ToString(),
            OrganizationId = organizationId,
            RecipientUserId = recipientUserId,
            ProjectId = projectId,
            EventType = eventType,
            Message = message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(notification, organizationId);
        return MapToDto(created);
    }

    /// <summary>
    /// Creates multiple notifications in batch.
    /// </summary>
    public async Task<List<NotificationDto>> CreateBatchNotificationsAsync(List<string> recipientUserIds,
        string projectId, string eventType, string message, string organizationId)
    {
        // Validate input
        if (recipientUserIds == null || recipientUserIds.Count == 0)
            throw new ValidationException("At least one recipient user ID is required");

        if (string.IsNullOrWhiteSpace(projectId))
            throw new ValidationException("Project ID is required");

        if (string.IsNullOrWhiteSpace(eventType))
            throw new ValidationException("Event type is required");

        if (string.IsNullOrWhiteSpace(message))
            throw new ValidationException("Message is required");

        if (string.IsNullOrEmpty(organizationId))
            throw new UnauthorizedException("Organization context is required");

        var notifications = recipientUserIds
            .Select(userId => new Notification
            {
                Id = Guid.NewGuid().ToString(),
                OrganizationId = organizationId,
                RecipientUserId = userId,
                ProjectId = projectId,
                EventType = eventType,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        var created = await _repository.CreateBatchAsync(notifications, organizationId);
        return created.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Gets all unread notifications for a user.
    /// </summary>
    public async Task<List<NotificationDto>> GetUnreadNotificationsAsync(string userId, string organizationId)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ValidationException("User ID is required");

        if (string.IsNullOrEmpty(organizationId))
            throw new UnauthorizedException("Organization context is required");

        var notifications = await _repository.GetUnreadByUserAsync(userId, organizationId);
        return notifications.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Gets all notifications for a user.
    /// </summary>
    public async Task<List<NotificationDto>> GetNotificationsAsync(string userId, string organizationId)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ValidationException("User ID is required");

        if (string.IsNullOrEmpty(organizationId))
            throw new UnauthorizedException("Organization context is required");

        var notifications = await _repository.GetByUserAsync(userId, organizationId);
        return notifications.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Marks a notification as read.
    /// </summary>
    public async Task<NotificationDto> MarkAsReadAsync(string notificationId, string organizationId)
    {
        if (string.IsNullOrEmpty(notificationId))
            throw new ValidationException("Notification ID is required");

        if (string.IsNullOrEmpty(organizationId))
            throw new UnauthorizedException("Organization context is required");

        var notification = await _repository.MarkAsReadAsync(notificationId, organizationId);
        return MapToDto(notification);
    }

    /// <summary>
    /// Maps a Notification entity to a NotificationDto.
    /// </summary>
    private static NotificationDto MapToDto(Notification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            RecipientUserId = notification.RecipientUserId,
            ProjectId = notification.ProjectId,
            EventType = notification.EventType,
            Message = notification.Message,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt,
            ReadAt = notification.ReadAt
        };
    }
}
