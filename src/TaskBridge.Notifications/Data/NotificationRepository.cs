using TaskBridge.Core.Entities;
using TaskBridge.Core.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace TaskBridge.Notifications.Data;

/// <summary>
/// Repository interface for Notification operations.
/// </summary>
public interface INotificationRepository
{
    /// <summary>
    /// Gets a notification by ID.
    /// </summary>
    Task<Notification?> GetByIdAsync(string notificationId, string organizationId);

    /// <summary>
    /// Gets all unread notifications for a user.
    /// </summary>
    Task<List<Notification>> GetUnreadByUserAsync(string userId, string organizationId);

    /// <summary>
    /// Gets all notifications for a user.
    /// </summary>
    Task<List<Notification>> GetByUserAsync(string userId, string organizationId);

    /// <summary>
    /// Creates a new notification.
    /// </summary>
    Task<Notification> CreateAsync(Notification notification, string organizationId);

    /// <summary>
    /// Marks a notification as read.
    /// </summary>
    Task<Notification> MarkAsReadAsync(string notificationId, string organizationId);

    /// <summary>
    /// Creates multiple notifications in batch.
    /// </summary>
    Task<List<Notification>> CreateBatchAsync(List<Notification> notifications, string organizationId);
}

/// <summary>
/// Implementation of INotificationRepository using Entity Framework Core.
/// </summary>
public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _context;
    private readonly ILogger<NotificationRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the NotificationRepository class.
    /// </summary>
    public NotificationRepository(NotificationDbContext context, ILogger<NotificationRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets a notification by ID.
    /// </summary>
    public async Task<Notification?> GetByIdAsync(string notificationId, string organizationId)
    {
        if (string.IsNullOrEmpty(notificationId) || string.IsNullOrEmpty(organizationId))
        {
            _logger.LogWarning("GetByIdAsync called with empty notificationId or organizationId");
            return null;
        }

        return await _context.Notifications
            .SingleOrDefaultAsync(n => n.Id == notificationId && n.OrganizationId == organizationId);
    }

    /// <summary>
    /// Gets all unread notifications for a user.
    /// </summary>
    public async Task<List<Notification>> GetUnreadByUserAsync(string userId, string organizationId)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(organizationId))
        {
            _logger.LogWarning("GetUnreadByUserAsync called with empty userId or organizationId");
            return new List<Notification>();
        }

        return await _context.Notifications
            .Where(n => n.RecipientUserId == userId && 
                        n.OrganizationId == organizationId && 
                        !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all notifications for a user.
    /// </summary>
    public async Task<List<Notification>> GetByUserAsync(string userId, string organizationId)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(organizationId))
        {
            _logger.LogWarning("GetByUserAsync called with empty userId or organizationId");
            return new List<Notification>();
        }

        return await _context.Notifications
            .Where(n => n.RecipientUserId == userId && n.OrganizationId == organizationId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Creates a new notification.
    /// </summary>
    public async Task<Notification> CreateAsync(Notification notification, string organizationId)
    {
        if (notification == null)
            throw new ValidationException("Notification cannot be null");

        if (string.IsNullOrEmpty(organizationId))
            throw new ValidationException("Organization ID is required");

        if (string.IsNullOrEmpty(notification.RecipientUserId))
            throw new ValidationException("Recipient user ID is required");

        notification.OrganizationId = organizationId;
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Notification created: {NotificationId} for user {UserId} in organization {OrganizationId}",
            notification.Id, notification.RecipientUserId, organizationId);

        return notification;
    }

    /// <summary>
    /// Marks a notification as read.
    /// </summary>
    public async Task<Notification> MarkAsReadAsync(string notificationId, string organizationId)
    {
        if (string.IsNullOrEmpty(notificationId) || string.IsNullOrEmpty(organizationId))
            throw new ValidationException("Notification ID and Organization ID are required");

        var notification = await GetByIdAsync(notificationId, organizationId);
        if (notification == null)
            throw new NotFoundException($"Notification {notificationId} not found");

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;

        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Notification marked as read: {NotificationId} for user {UserId}",
            notificationId, notification.RecipientUserId);

        return notification;
    }

    /// <summary>
    /// Creates multiple notifications in batch.
    /// </summary>
    public async Task<List<Notification>> CreateBatchAsync(List<Notification> notifications, string organizationId)
    {
        if (notifications == null || notifications.Count == 0)
            throw new ValidationException("Notifications list cannot be null or empty");

        if (string.IsNullOrEmpty(organizationId))
            throw new ValidationException("Organization ID is required");

        foreach (var notification in notifications)
        {
            notification.OrganizationId = organizationId;
        }

        _context.Notifications.AddRange(notifications);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Batch of {Count} notifications created in organization {OrganizationId}",
            notifications.Count, organizationId);

        return notifications;
    }
}
