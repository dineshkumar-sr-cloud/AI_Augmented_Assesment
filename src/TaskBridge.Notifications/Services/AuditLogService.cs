using System.Text.Json;
using TaskBridge.Core.Entities;
using TaskBridge.Core.Exceptions;
using TaskBridge.Notifications.Data;
using TaskBridge.Notifications.Models.Dtos;

namespace TaskBridge.Notifications.Services;

/// <summary>
/// Service interface for audit log operations.
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Records an immutable audit log entry for a state change.
    /// </summary>
    Task<AuditLogDto> RecordEventAsync(CreateAuditLogDto dto, string organizationId);

    /// <summary>
    /// Retrieves audit history for a project with optional filters.
    /// </summary>
    Task<List<AuditLogDto>> GetAuditHistoryAsync(string projectId, string organizationId,
        DateTime? fromDate = null, DateTime? toDate = null, string? eventType = null);

    /// <summary>
    /// Gets a single audit log entry by ID.
    /// </summary>
    Task<AuditLogDto> GetAuditLogAsync(string auditLogId, string organizationId);
}

/// <summary>
/// Service for managing audit log operations.
/// Enforces immutability: audit logs cannot be modified or deleted after creation.
/// </summary>
public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _repository;
    private readonly ILogger<AuditLogService> _logger;

    /// <summary>
    /// Initializes a new instance of the AuditLogService class.
    /// </summary>
    public AuditLogService(IAuditLogRepository repository, ILogger<AuditLogService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Records an immutable audit log entry for a state change.
    /// </summary>
    public async Task<AuditLogDto> RecordEventAsync(CreateAuditLogDto dto, string organizationId)
    {
        // Validate input
        if (dto == null)
            throw new ValidationException("Audit log data cannot be null");

        if (string.IsNullOrWhiteSpace(dto.ProjectId))
            throw new ValidationException("Project ID is required");

        if (string.IsNullOrWhiteSpace(dto.EventType))
            throw new ValidationException("Event type is required");

        if (string.IsNullOrWhiteSpace(dto.ActorId))
            throw new ValidationException("Actor ID is required");

        // Validate organization context
        if (string.IsNullOrEmpty(organizationId))
            throw new UnauthorizedException("Organization context is required");

        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid().ToString(),
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

        var created = await _repository.CreateAsync(auditLog, organizationId);
        return MapToDto(created);
    }

    /// <summary>
    /// Retrieves audit history for a project with optional filters.
    /// </summary>
    public async Task<List<AuditLogDto>> GetAuditHistoryAsync(string projectId, string organizationId,
        DateTime? fromDate = null, DateTime? toDate = null, string? eventType = null)
    {
        // Validate input
        if (string.IsNullOrEmpty(projectId))
            throw new ValidationException("Project ID is required");

        if (string.IsNullOrEmpty(organizationId))
            throw new UnauthorizedException("Organization context is required");

        var auditLogs = await _repository.GetByProjectAsync(projectId, organizationId, fromDate, toDate, eventType);
        return auditLogs.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Gets a single audit log entry by ID.
    /// </summary>
    public async Task<AuditLogDto> GetAuditLogAsync(string auditLogId, string organizationId)
    {
        if (string.IsNullOrEmpty(auditLogId))
            throw new ValidationException("Audit log ID is required");

        if (string.IsNullOrEmpty(organizationId))
            throw new UnauthorizedException("Organization context is required");

        var auditLog = await _repository.GetByIdAsync(auditLogId, organizationId);
        if (auditLog == null)
            throw new NotFoundException($"Audit log {auditLogId} not found");

        return MapToDto(auditLog);
    }

    /// <summary>
    /// Maps an AuditLog entity to an AuditLogDto.
    /// </summary>
    private static AuditLogDto MapToDto(AuditLog auditLog)
    {
        return new AuditLogDto
        {
            Id = auditLog.Id,
            ProjectId = auditLog.ProjectId,
            EventType = auditLog.EventType,
            EntityType = auditLog.EntityType,
            EntityId = auditLog.EntityId,
            ActorId = auditLog.ActorId,
            ActorIpAddress = auditLog.ActorIpAddress,
            PreviousState = auditLog.PreviousState,
            NewState = auditLog.NewState,
            CreatedAt = auditLog.CreatedAt
        };
    }
}
