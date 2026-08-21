using Microsoft.AspNetCore.Mvc;
using TaskBridge.Core.Authentication;
using TaskBridge.Core.Exceptions;
using TaskBridge.Notifications.Models.Dtos;
using TaskBridge.Notifications.Services;

namespace TaskBridge.Notifications.Controllers;

/// <summary>
/// API controller for audit log operations.
/// Provides endpoints to record and query immutable audit entries.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AuditController : ControllerBase
{
    private readonly IAuditLogService _service;
    private readonly ILogger<AuditController> _logger;

    /// <summary>
    /// Initializes a new instance of the AuditController class.
    /// </summary>
    public AuditController(IAuditLogService service, ILogger<AuditController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Records a new audit log entry (internal endpoint).
    /// This endpoint is called by the Project Service when a milestone changes.
    /// </summary>
    /// <param name="dto">The audit log entry to record.</param>
    /// <returns>The recorded audit log entry.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuditLogDto>> RecordAuditEvent([FromBody] CreateAuditLogDto dto)
    {
        try
        {
            var userContext = HttpContext.Items["UserContext"] as UserContext;
            if (userContext == null)
                return Unauthorized(new { error = "User context not found" });

            _logger.LogInformation(
                "Recording audit event: {EventType} for project {ProjectId} by {UserId}",
                dto.EventType, dto.ProjectId, userContext.UserId);

            var result = await _service.RecordEventAsync(dto, userContext.OrganizationId);
            return CreatedAtAction(nameof(GetAuditLog), new { id = result.Id }, result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation error recording audit event: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning("Unauthorized audit recording attempt: {Message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording audit event");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Gets audit history for a specific project.
    /// Supports filtering by date range and event type.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="fromDate">Optional start date filter (ISO 8601 format).</param>
    /// <param name="toDate">Optional end date filter (ISO 8601 format).</param>
    /// <param name="eventType">Optional event type filter.</param>
    /// <returns>List of audit entries for the project.</returns>
    [HttpGet("{projectId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<AuditLogDto>>> GetAuditHistory(
        string projectId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? eventType = null)
    {
        try
        {
            var userContext = HttpContext.Items["UserContext"] as UserContext;
            if (userContext == null)
                return Unauthorized(new { error = "User context not found" });

            _logger.LogInformation(
                "Retrieving audit history for project {ProjectId} by {UserId}",
                projectId, userContext.UserId);

            var result = await _service.GetAuditHistoryAsync(projectId, userContext.OrganizationId,
                fromDate, toDate, eventType);

            return Ok(new
            {
                success = true,
                data = result,
                count = result.Count,
                timestamp = DateTime.UtcNow
            });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation error retrieving audit history: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning("Unauthorized audit history access attempt: {Message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit history");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Gets a single audit log entry by ID.
    /// </summary>
    /// <param name="id">The audit log ID.</param>
    /// <returns>The audit log entry.</returns>
    [HttpGet("entry/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuditLogDto>> GetAuditLog(string id)
    {
        try
        {
            var userContext = HttpContext.Items["UserContext"] as UserContext;
            if (userContext == null)
                return Unauthorized(new { error = "User context not found" });

            var result = await _service.GetAuditLogAsync(id, userContext.OrganizationId);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Audit log not found: {Message}", ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning("Unauthorized audit log access attempt: {Message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit log");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Internal server error" });
        }
    }
}

/// <summary>
/// Placeholder for Authorize attribute (would normally come from Microsoft.AspNetCore.Authorization).
/// Used to mark endpoints as requiring authentication.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute
{
}
