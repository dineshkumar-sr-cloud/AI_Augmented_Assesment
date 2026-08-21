using Microsoft.AspNetCore.Mvc;
using TaskBridge.Core.Authentication;
using TaskBridge.Core.Exceptions;
using TaskBridge.Notifications.Models.Dtos;
using TaskBridge.Notifications.Services;

namespace TaskBridge.Notifications.Controllers;

/// <summary>
/// API controller for notification operations.
/// Provides endpoints to retrieve and manage user notifications.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _service;
    private readonly ILogger<NotificationsController> _logger;

    /// <summary>
    /// Initializes a new instance of the NotificationsController class.
    /// </summary>
    public NotificationsController(INotificationService service, ILogger<NotificationsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Gets all unread notifications for the current user.
    /// </summary>
    /// <returns>List of unread notifications.</returns>
    [HttpGet("unread")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<NotificationDto>>> GetUnreadNotifications()
    {
        try
        {
            var userContext = HttpContext.Items["UserContext"] as UserContext;
            if (userContext == null)
                return Unauthorized(new { error = "User context not found" });

            _logger.LogInformation(
                "Retrieving unread notifications for user {UserId}",
                userContext.UserId);

            var result = await _service.GetUnreadNotificationsAsync(userContext.UserId, userContext.OrganizationId);

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
            _logger.LogWarning("Validation error retrieving unread notifications: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning("Unauthorized notification access attempt: {Message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unread notifications");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Gets all notifications for the current user.
    /// </summary>
    /// <returns>List of all notifications (read and unread).</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<NotificationDto>>> GetNotifications()
    {
        try
        {
            var userContext = HttpContext.Items["UserContext"] as UserContext;
            if (userContext == null)
                return Unauthorized(new { error = "User context not found" });

            _logger.LogInformation(
                "Retrieving all notifications for user {UserId}",
                userContext.UserId);

            var result = await _service.GetNotificationsAsync(userContext.UserId, userContext.OrganizationId);

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
            _logger.LogWarning("Validation error retrieving notifications: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning("Unauthorized notification access attempt: {Message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notifications");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Marks a notification as read.
    /// </summary>
    /// <param name="id">The notification ID.</param>
    /// <returns>The updated notification.</returns>
    [HttpPatch("{id}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationDto>> MarkAsRead(string id)
    {
        try
        {
            var userContext = HttpContext.Items["UserContext"] as UserContext;
            if (userContext == null)
                return Unauthorized(new { error = "User context not found" });

            _logger.LogInformation(
                "Marking notification {NotificationId} as read by user {UserId}",
                id, userContext.UserId);

            var result = await _service.MarkAsReadAsync(id, userContext.OrganizationId);

            return Ok(new
            {
                success = true,
                data = result,
                timestamp = DateTime.UtcNow
            });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Notification not found: {Message}", ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning("Unauthorized notification update attempt: {Message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification as read");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Internal server error" });
        }
    }
}
