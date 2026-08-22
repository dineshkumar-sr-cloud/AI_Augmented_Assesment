using Moq;
using TaskBridge.Core.Entities;
using TaskBridge.Core.Exceptions;
using TaskBridge.Notifications.Data;
using TaskBridge.Notifications.Models.Dtos;
using TaskBridge.Notifications.Services;
using Xunit;

namespace TaskBridge.Notifications.Tests;

public class AuditLogServiceTests
{
    private readonly Mock<IAuditLogRepository> _mockRepository;
    private readonly Mock<ILogger<AuditLogService>> _mockLogger;
    private readonly AuditLogService _service;

    public AuditLogServiceTests()
    {
        _mockRepository = new Mock<IAuditLogRepository>();
        _mockLogger = new Mock<ILogger<AuditLogService>>();
        _service = new AuditLogService(_mockRepository.Object, _mockLogger.Object);
    }

    #region RecordEventAsync Tests

    [Fact]
    public async Task RecordEventAsync_WithValidData_ReturnsAuditLogDto()
    {
        // Arrange
        const string organizationId = "org-789";
        var dto = new CreateAuditLogDto
        {
            ProjectId = "project-456",
            EventType = "MILESTONE_UPDATED",
            EntityType = "Project",
            EntityId = "proj-123",
            ActorId = "user-789",
            ActorIpAddress = "192.168.1.1",
            PreviousState = "{\"status\":\"PLANNING\"}",
            NewState = "{\"status\":\"IN_PROGRESS\"}"
        };

        var createdAuditLog = new AuditLog
        {
            Id = "audit-001",
            OrganizationId = organizationId,
            ProjectId = dto.ProjectId,
            EventType = dto.EventType,
            EntityType = dto.EntityType,
            EntityId = dto.EntityId,
            ActorId = dto.ActorId,
            ActorIpAddress = dto.ActorIpAddress,
            PreviousState = dto.PreviousState,
            NewState = dto.NewState,
            CreatedAt = DateTime.UtcNow
        };

        _mockRepository
            .Setup(r => r.CreateAsync(It.IsAny<AuditLog>(), organizationId))
            .ReturnsAsync(createdAuditLog);

        // Act
        var result = await _service.RecordEventAsync(dto, organizationId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.ProjectId, result.ProjectId);
        Assert.Equal(dto.EventType, result.EventType);
        Assert.Equal(dto.EntityType, result.EntityType);
        Assert.Equal(dto.ActorId, result.ActorId);
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<AuditLog>(), organizationId), Times.Once);
    }

    [Fact]
    public async Task RecordEventAsync_WithNullDto_ThrowsValidationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.RecordEventAsync(null, "org-789"));
    }

    [Fact]
    public async Task RecordEventAsync_WithNullProjectId_ThrowsValidationException()
    {
        // Arrange
        var dto = new CreateAuditLogDto
        {
            ProjectId = null,
            EventType = "MILESTONE_UPDATED",
            ActorId = "user-789"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.RecordEventAsync(dto, "org-789"));
    }

    [Fact]
    public async Task RecordEventAsync_WithNullEventType_ThrowsValidationException()
    {
        // Arrange
        var dto = new CreateAuditLogDto
        {
            ProjectId = "project-456",
            EventType = null,
            ActorId = "user-789"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.RecordEventAsync(dto, "org-789"));
    }

    [Fact]
    public async Task RecordEventAsync_WithNullActorId_ThrowsValidationException()
    {
        // Arrange
        var dto = new CreateAuditLogDto
        {
            ProjectId = "project-456",
            EventType = "MILESTONE_UPDATED",
            ActorId = null
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.RecordEventAsync(dto, "org-789"));
    }

    [Fact]
    public async Task RecordEventAsync_WithNullOrganizationId_ThrowsUnauthorizedException()
    {
        // Arrange
        var dto = new CreateAuditLogDto
        {
            ProjectId = "project-456",
            EventType = "MILESTONE_UPDATED",
            ActorId = "user-789"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _service.RecordEventAsync(dto, null));
    }

    [Fact]
    public async Task RecordEventAsync_WithEmptyOrganizationId_ThrowsUnauthorizedException()
    {
        // Arrange
        var dto = new CreateAuditLogDto
        {
            ProjectId = "project-456",
            EventType = "MILESTONE_UPDATED",
            ActorId = "user-789"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _service.RecordEventAsync(dto, string.Empty));
    }

    #endregion

    #region GetAuditHistoryAsync Tests

    [Fact]
    public async Task GetAuditHistoryAsync_WithValidProjectId_ReturnsAuditLogs()
    {
        // Arrange
        const string projectId = "project-456";
        const string organizationId = "org-789";

        var auditLogs = new List<AuditLog>
        {
            new()
            {
                Id = "audit-001",
                OrganizationId = organizationId,
                ProjectId = projectId,
                EventType = "MILESTONE_UPDATED",
                EntityType = "Project",
                EntityId = "proj-123",
                ActorId = "user-789",
                ActorIpAddress = "192.168.1.1",
                PreviousState = "{\"status\":\"PLANNING\"}",
                NewState = "{\"status\":\"IN_PROGRESS\"}",
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new()
            {
                Id = "audit-002",
                OrganizationId = organizationId,
                ProjectId = projectId,
                EventType = "PROJECT_CREATED",
                EntityType = "Project",
                EntityId = "proj-456",
                ActorId = "user-790",
                ActorIpAddress = "192.168.1.2",
                PreviousState = null,
                NewState = "{\"status\":\"PLANNING\"}",
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            }
        };

        _mockRepository
            .Setup(r => r.GetByProjectAsync(projectId, organizationId, null, null, null))
            .ReturnsAsync(auditLogs);

        // Act
        var result = await _service.GetAuditHistoryAsync(projectId, organizationId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, log => Assert.Equal(projectId, log.ProjectId));
        _mockRepository.Verify(r => r.GetByProjectAsync(projectId, organizationId, null, null, null), Times.Once);
    }

    [Fact]
    public async Task GetAuditHistoryAsync_WithDateFilter_ReturnsFilteredAuditLogs()
    {
        // Arrange
        const string projectId = "project-456";
        const string organizationId = "org-789";
        var fromDate = DateTime.UtcNow.AddDays(-7);
        var toDate = DateTime.UtcNow;

        var filteredLogs = new List<AuditLog>
        {
            new()
            {
                Id = "audit-001",
                OrganizationId = organizationId,
                ProjectId = projectId,
                EventType = "MILESTONE_UPDATED",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            }
        };

        _mockRepository
            .Setup(r => r.GetByProjectAsync(projectId, organizationId, fromDate, toDate, null))
            .ReturnsAsync(filteredLogs);

        // Act
        var result = await _service.GetAuditHistoryAsync(projectId, organizationId, fromDate, toDate);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        _mockRepository.Verify(r => r.GetByProjectAsync(projectId, organizationId, fromDate, toDate, null), Times.Once);
    }

    [Fact]
    public async Task GetAuditHistoryAsync_WithEventTypeFilter_ReturnsFilteredAuditLogs()
    {
        // Arrange
        const string projectId = "project-456";
        const string organizationId = "org-789";
        const string eventType = "MILESTONE_UPDATED";

        var filteredLogs = new List<AuditLog>
        {
            new()
            {
                Id = "audit-001",
                OrganizationId = organizationId,
                ProjectId = projectId,
                EventType = eventType,
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockRepository
            .Setup(r => r.GetByProjectAsync(projectId, organizationId, null, null, eventType))
            .ReturnsAsync(filteredLogs);

        // Act
        var result = await _service.GetAuditHistoryAsync(projectId, organizationId, null, null, eventType);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.All(result, log => Assert.Equal(eventType, log.EventType));
    }

    [Fact]
    public async Task GetAuditHistoryAsync_WithEmptyProjectId_ThrowsValidationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.GetAuditHistoryAsync(string.Empty, "org-789"));
    }

    [Fact]
    public async Task GetAuditHistoryAsync_WithNullOrganizationId_ThrowsUnauthorizedException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _service.GetAuditHistoryAsync("project-456", null));
    }

    [Fact]
    public async Task GetAuditHistoryAsync_WithNoMatches_ReturnsEmptyList()
    {
        // Arrange
        const string projectId = "project-456";
        const string organizationId = "org-789";

        _mockRepository
            .Setup(r => r.GetByProjectAsync(projectId, organizationId, null, null, null))
            .ReturnsAsync(new List<AuditLog>());

        // Act
        var result = await _service.GetAuditHistoryAsync(projectId, organizationId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region GetAuditLogAsync Tests

    [Fact]
    public async Task GetAuditLogAsync_WithValidId_ReturnsAuditLogDto()
    {
        // Arrange
        const string auditLogId = "audit-001";
        const string organizationId = "org-789";

        var auditLog = new AuditLog
        {
            Id = auditLogId,
            OrganizationId = organizationId,
            ProjectId = "project-456",
            EventType = "MILESTONE_UPDATED",
            EntityType = "Project",
            EntityId = "proj-123",
            ActorId = "user-789",
            ActorIpAddress = "192.168.1.1",
            CreatedAt = DateTime.UtcNow
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(auditLogId, organizationId))
            .ReturnsAsync(auditLog);

        // Act
        var result = await _service.GetAuditLogAsync(auditLogId, organizationId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(auditLogId, result.Id);
        Assert.Equal("project-456", result.ProjectId);
        _mockRepository.Verify(r => r.GetByIdAsync(auditLogId, organizationId), Times.Once);
    }

    [Fact]
    public async Task GetAuditLogAsync_WithNonExistentId_ThrowsNotFoundException()
    {
        // Arrange
        const string auditLogId = "non-existent";
        const string organizationId = "org-789";

        _mockRepository
            .Setup(r => r.GetByIdAsync(auditLogId, organizationId))
            .ReturnsAsync((AuditLog)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.GetAuditLogAsync(auditLogId, organizationId));
    }

    [Fact]
    public async Task GetAuditLogAsync_WithEmptyId_ThrowsValidationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.GetAuditLogAsync(string.Empty, "org-789"));
    }

    [Fact]
    public async Task GetAuditLogAsync_WithNullOrganizationId_ThrowsUnauthorizedException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _service.GetAuditLogAsync("audit-001", null));
    }

    #endregion
}
