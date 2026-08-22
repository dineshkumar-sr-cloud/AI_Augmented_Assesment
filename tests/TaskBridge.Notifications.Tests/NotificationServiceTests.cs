using Moq;
using TaskBridge.Core.Entities;
using TaskBridge.Core.Exceptions;
using TaskBridge.Notifications.Data;
using TaskBridge.Notifications.Models.Dtos;
using TaskBridge.Notifications.Services;
using Xunit;

namespace TaskBridge.Notifications.Tests;

public class NotificationServiceTests
{
    private readonly Mock<INotificationRepository> _mockRepository;
    private readonly Mock<ILogger<NotificationService>> _mockLogger;
    private readonly NotificationService _service;

    public NotificationServiceTests()
    {
        _mockRepository = new Mock<INotificationRepository>();
        _mockLogger = new Mock<ILogger<NotificationService>>();
        _service = new NotificationService(_mockRepository.Object, _mockLogger.Object);
    }

    #region CreateNotificationAsync Tests

    [Fact]
    public async Task CreateNotificationAsync_WithValidData_ReturnsNotificationDto()
    {
        // Arrange
        const string recipientUserId = "user-123";
        const string projectId = "project-456";
        const string eventType = "PROJECT_CREATED";
        const string message = "Project has been created";
        const string organizationId = "org-789";

        var createdNotification = new Notification
        {
            Id = "notif-001",
            OrganizationId = organizationId,
            RecipientUserId = recipientUserId,
            ProjectId = projectId,
            EventType = eventType,
            Message = message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _mockRepository
            .Setup(r => r.CreateAsync(It.IsAny<Notification>(), organizationId))
            .ReturnsAsync(createdNotification);

        // Act
        var result = await _service.CreateNotificationAsync(recipientUserId, projectId, eventType, message, organizationId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(recipientUserId, result.RecipientUserId);
        Assert.Equal(projectId, result.ProjectId);
        Assert.Equal(eventType, result.EventType);
        Assert.Equal(message, result.Message);
        Assert.False(result.IsRead);
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Notification>(), organizationId), Times.Once);
    }

    [Fact]
    public async Task CreateNotificationAsync_WithNullRecipientUserId_ThrowsValidationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateNotificationAsync(null, "project-456", "EVENT", "message", "org-789"));
    }

    [Fact]
    public async Task CreateNotificationAsync_WithEmptyRecipientUserId_ThrowsValidationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateNotificationAsync(string.Empty, "project-456", "EVENT", "message", "org-789"));
    }

    [Fact]
    public async Task CreateNotificationAsync_WithNullProjectId_ThrowsValidationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateNotificationAsync("user-123", null, "EVENT", "message", "org-789"));
    }

    [Fact]
    public async Task CreateNotificationAsync_WithNullEventType_ThrowsValidationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateNotificationAsync("user-123", "project-456", null, "message", "org-789"));
    }

    [Fact]
    public async Task CreateNotificationAsync_WithNullMessage_ThrowsValidationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateNotificationAsync("user-123", "project-456", "EVENT", null, "org-789"));
    }

    [Fact]
    public async Task CreateNotificationAsync_WithNullOrganizationId_ThrowsUnauthorizedException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _service.CreateNotificationAsync("user-123", "project-456", "EVENT", "message", null));
    }

    [Fact]
    public async Task CreateNotificationAsync_WithEmptyOrganizationId_ThrowsUnauthorizedException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _service.CreateNotificationAsync("user-123", "project-456", "EVENT", "message", string.Empty));
    }

    #endregion

    #region CreateBatchNotificationsAsync Tests

    [Fact]
    public async Task CreateBatchNotificationsAsync_WithValidData_ReturnsNotificationDtoList()
    {
        // Arrange
        var recipientUserIds = new List<string> { "user-1", "user-2", "user-3" };
        const string projectId = "project-456";
        const string eventType = "PROJECT_UPDATED";
        const string message = "Project has been updated";
        const string organizationId = "org-789";

        var createdNotifications = recipientUserIds.Select((userId, index) => new Notification
        {
            Id = $"notif-{index:000}",
            OrganizationId = organizationId,
            RecipientUserId = userId,
            ProjectId = projectId,
            EventType = eventType,
            Message = message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        _mockRepository
            .Setup(r => r.CreateBatchAsync(It.IsAny<List<Notification>>(), organizationId))
            .ReturnsAsync(createdNotifications);

        // Act
        var result = await _service.CreateBatchNotificationsAsync(recipientUserIds, projectId, eventType, message, organizationId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.All(result, n => Assert.Equal(projectId, n.ProjectId));
        Assert.All(result, n => Assert.Equal(eventType, n.EventType));
        _mockRepository.Verify(r => r.CreateBatchAsync(It.IsAny<List<Notification>>(), organizationId), Times.Once);
    }

    [Fact]
    public async Task CreateBatchNotificationsAsync_WithEmptyList_ThrowsValidationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateBatchNotificationsAsync(new List<string>(), "project-456", "EVENT", "message", "org-789"));
    }

    [Fact]
    public async Task CreateBatchNotificationsAsync_WithNullList_ThrowsValidationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateBatchNotificationsAsync(null, "project-456", "EVENT", "message", "org-789"));
    }

    [Fact]
    public async Task CreateBatchNotificationsAsync_WithNullOrganizationId_ThrowsUnauthorizedException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _service.CreateBatchNotificationsAsync(new List<string> { "user-1" }, "project-456", "EVENT", "message", null));
    }

    #endregion

    #region GetUnreadNotificationsAsync Tests

    [Fact]
    public async Task GetUnreadNotificationsAsync_WithValidUserId_ReturnsUnreadNotifications()
    {
        // Arrange
        const string userId = "user-123";
        const string organizationId = "org-789";

        var unreadNotifications = new List<Notification>
        {
            new()
            {
                Id = "notif-001",
                OrganizationId = organizationId,
                RecipientUserId = userId,
                ProjectId = "project-1",
                EventType = "PROJECT_CREATED",
                Message = "Project created",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = "notif-002",
                OrganizationId = organizationId,
                RecipientUserId = userId,
                ProjectId = "project-2",
                EventType = "PROJECT_UPDATED",
                Message = "Project updated",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockRepository
            .Setup(r => r.GetUnreadByUserAsync(userId, organizationId))
            .ReturnsAsync(unreadNotifications);

        // Act
        var result = await _service.GetUnreadNotificationsAsync(userId, organizationId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, n => Assert.False(n.IsRead));
        _mockRepository.Verify(r => r.GetUnreadByUserAsync(userId, organizationId), Times.Once);
    }

    [Fact]
    public async Task GetUnreadNotificationsAsync_WithEmptyUserId_ThrowsValidationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.GetUnreadNotificationsAsync(string.Empty, "org-789"));
    }

    [Fact]
    public async Task GetUnreadNotificationsAsync_WithNullOrganizationId_ThrowsUnauthorizedException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _service.GetUnreadNotificationsAsync("user-123", null));
    }

    [Fact]
    public async Task GetUnreadNotificationsAsync_WithNoNotifications_ReturnsEmptyList()
    {
        // Arrange
        const string userId = "user-123";
        const string organizationId = "org-789";

        _mockRepository
            .Setup(r => r.GetUnreadByUserAsync(userId, organizationId))
            .ReturnsAsync(new List<Notification>());

        // Act
        var result = await _service.GetUnreadNotificationsAsync(userId, organizationId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region GetNotificationsAsync Tests

    [Fact]
    public async Task GetNotificationsAsync_WithValidUserId_ReturnsAllNotifications()
    {
        // Arrange
        const string userId = "user-123";
        const string organizationId = "org-789";

        var notifications = new List<Notification>
        {
            new()
            {
                Id = "notif-001",
                OrganizationId = organizationId,
                RecipientUserId = userId,
                ProjectId = "project-1",
                EventType = "PROJECT_CREATED",
                Message = "Project created",
                IsRead = false,
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new()
            {
                Id = "notif-002",
                OrganizationId = organizationId,
                RecipientUserId = userId,
                ProjectId = "project-2",
                EventType = "PROJECT_UPDATED",
                Message = "Project updated",
                IsRead = true,
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                ReadAt = DateTime.UtcNow
            }
        };

        _mockRepository
            .Setup(r => r.GetByUserAsync(userId, organizationId))
            .ReturnsAsync(notifications);

        // Act
        var result = await _service.GetNotificationsAsync(userId, organizationId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Single(result.Where(n => n.IsRead));
        Assert.Single(result.Where(n => !n.IsRead));
    }

    [Fact]
    public async Task GetNotificationsAsync_WithEmptyUserId_ThrowsValidationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.GetNotificationsAsync(string.Empty, "org-789"));
    }

    #endregion

    #region MarkAsReadAsync Tests

    [Fact]
    public async Task MarkAsReadAsync_WithValidNotificationId_ReturnsReadNotification()
    {
        // Arrange
        const string notificationId = "notif-001";
        const string organizationId = "org-789";

        var readNotification = new Notification
        {
            Id = notificationId,
            OrganizationId = organizationId,
            RecipientUserId = "user-123",
            ProjectId = "project-456",
            EventType = "PROJECT_UPDATED",
            Message = "Project updated",
            IsRead = true,
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            ReadAt = DateTime.UtcNow
        };

        _mockRepository
            .Setup(r => r.MarkAsReadAsync(notificationId, organizationId))
            .ReturnsAsync(readNotification);

        // Act
        var result = await _service.MarkAsReadAsync(notificationId, organizationId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsRead);
        Assert.NotNull(result.ReadAt);
        _mockRepository.Verify(r => r.MarkAsReadAsync(notificationId, organizationId), Times.Once);
    }

    [Fact]
    public async Task MarkAsReadAsync_WithEmptyNotificationId_ThrowsValidationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.MarkAsReadAsync(string.Empty, "org-789"));
    }

    [Fact]
    public async Task MarkAsReadAsync_WithNullOrganizationId_ThrowsUnauthorizedException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _service.MarkAsReadAsync("notif-001", null));
    }

    #endregion
}
