using Microsoft.AspNetCore.Mvc.Filters;
using TaskBridge.Core.Authentication;
using TaskBridge.Core.Exceptions;

namespace TaskBridge.Core.Middleware;

/// <summary>
/// Action filter that extracts and validates user context from JWT claims and request metadata.
/// Ensures multi-tenant isolation by validating organization context.
/// </summary>
public class UserContextFilter : IAsyncActionFilter
{
    private readonly ILogger<UserContextFilter> _logger;

    /// <summary>
    /// Initializes a new instance of the UserContextFilter class.
    /// </summary>
    public UserContextFilter(ILogger<UserContextFilter> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Executes the filter to extract and validate user context.
    /// </summary>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var user = context.HttpContext.User;

        // Extract user context from claims
        var userId = user.GetUserId();
        var organizationId = user.GetOrganizationId();

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(organizationId))
        {
            _logger.LogWarning("Unauthorized access attempt: Missing user ID or organization ID");
            context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.HttpContext.Response.WriteAsJsonAsync(new
            {
                error = "Unauthorized",
                message = "Missing required authentication claims (user ID or organization ID)"
            });
            return;
        }

        // Extract user's IP address for audit logging
        var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString();

        // Build user context
        var userContext = new UserContext
        {
            UserId = userId,
            OrganizationId = organizationId,
            OrganizationName = user.FindFirst(ClaimTypes.OrganizationName)?.Value ?? string.Empty,
            TeamId = user.GetTeamId(),
            Email = user.GetUserEmail(),
            Roles = user.GetRoles(),
            IpAddress = ipAddress
        };

        // Validate user context
        if (!userContext.IsValid())
        {
            _logger.LogWarning("Invalid user context: Organization mismatch or missing fields for user {UserId}", userId);
            context.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.HttpContext.Response.WriteAsJsonAsync(new
            {
                error = "Forbidden",
                message = "Invalid user context or organization mismatch"
            });
            return;
        }

        // Attach user context to HttpContext for use in handlers
        context.HttpContext.Items["UserContext"] = userContext;

        _logger.LogInformation("User context loaded: {UserId} in organization {OrganizationId}",
            userId, organizationId);

        await next();
    }
}
