using TaskBridge.Core.Entities;
using TaskBridge.Core.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace TaskBridge.Notifications.Data;

/// <summary>
/// Repository interface for AuditLog operations.
/// Audit logs are immutable - no update or delete operations are provided.
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Gets an audit log entry by ID.
    /// </summary>
    Task<AuditLog?> GetByIdAsync(string auditLogId, string organizationId);

    /// <summary>
    /// Gets audit log entries for a project with optional filters.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="organizationId">The organization ID for multi-tenant isolation.</param>
    /// <param name="fromDate">Optional start date filter.</param>
    /// <param name="toDate">Optional end date filter.</param>
    /// <param name="eventType">Optional event type filter.</param>
    /// <returns>List of audit log entries matching the criteria.</returns>
    Task<List<AuditLog>> GetByProjectAsync(string projectId, string organizationId, 
        DateTime? fromDate = null, DateTime? toDate = null, string? eventType = null);

    /// <summary>
    /// Creates an immutable audit log entry.
    /// </summary>
    Task<AuditLog> CreateAsync(AuditLog auditLog, string organizationId);

    // NOTE: No Update() or Delete() methods - audit logs are immutable by design.
}

/// <summary>
/// Implementation of IAuditLogRepository using Entity Framework Core.
/// Enforces immutability - audit logs cannot be modified after creation.
/// </summary>
public class AuditLogRepository : IAuditLogRepository
{
    private readonly NotificationDbContext _context;
    private readonly ILogger<AuditLogRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the AuditLogRepository class.
    /// </summary>
    public AuditLogRepository(NotificationDbContext context, ILogger<AuditLogRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets an audit log entry by ID.
    /// </summary>
    public async Task<AuditLog?> GetByIdAsync(string auditLogId, string organizationId)
    {
        if (string.IsNullOrEmpty(auditLogId) || string.IsNullOrEmpty(organizationId))
        {
            _logger.LogWarning("GetByIdAsync called with empty auditLogId or organizationId");
            return null;
        }

        return await _context.AuditLogs
            .SingleOrDefaultAsync(a => a.Id == auditLogId && a.OrganizationId == organizationId);
    }

    /// <summary>
    /// Gets audit log entries for a project with optional filters.
    /// </summary>
    public async Task<List<AuditLog>> GetByProjectAsync(string projectId, string organizationId,
        DateTime? fromDate = null, DateTime? toDate = null, string? eventType = null)
    {
        if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(organizationId))
        {
            _logger.LogWarning("GetByProjectAsync called with empty projectId or organizationId");
            return new List<AuditLog>();
        }

        var query = _context.AuditLogs
            .Where(a => a.ProjectId == projectId && a.OrganizationId == organizationId);

        if (fromDate.HasValue)
            query = query.Where(a => a.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(a => a.CreatedAt <= toDate.Value);

        if (!string.IsNullOrEmpty(eventType))
            query = query.Where(a => a.EventType == eventType);

        return await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Creates an immutable audit log entry.
    /// Once created, this entry cannot be modified or deleted.
    /// </summary>
    public async Task<AuditLog> CreateAsync(AuditLog auditLog, string organizationId)
    {
        if (auditLog == null)
            throw new ValidationException("Audit log cannot be null");

        if (string.IsNullOrEmpty(organizationId))
            throw new ValidationException("Organization ID is required");

        if (string.IsNullOrEmpty(auditLog.ProjectId))
            throw new ValidationException("Project ID is required");

        if (string.IsNullOrEmpty(auditLog.EventType))
            throw new ValidationException("Event type is required");

        auditLog.OrganizationId = organizationId;
        auditLog.CreatedAt = DateTime.UtcNow;

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Audit log created: {AuditLogId} for project {ProjectId} in organization {OrganizationId}. Event: {EventType} by {ActorId}",
            auditLog.Id, auditLog.ProjectId, organizationId, auditLog.EventType, auditLog.ActorId);

        return auditLog;
    }
}
