using System.Security.Claims;

namespace TaskBridge.Core.Authentication;

/// <summary>
/// Constants and helper methods for JWT claims.
/// </summary>
public static class ClaimTypes
{
    /// <summary>Organization ID claim type.</summary>
    public const string OrganizationId = "organization_id";

    /// <summary>Organization Name claim type.</summary>
    public const string OrganizationName = "organization_name";

    /// <summary>Team ID claim type.</summary>
    public const string TeamId = "team_id";

    /// <summary>Roles claim type.</summary>
    public const string Roles = "roles";
}

/// <summary>
/// Extension methods for extracting claims from ClaimsPrincipal.
/// </summary>
public static class ClaimsExtensions
{
    /// <summary>
    /// Gets the organization ID from the claims principal.
    /// </summary>
    public static string? GetOrganizationId(this ClaimsPrincipal principal)
    {
        return principal?.FindFirst(ClaimTypes.OrganizationId)?.Value;
    }

    /// <summary>
    /// Gets the user ID (subject) from the claims principal.
    /// </summary>
    public static string? GetUserId(this ClaimsPrincipal principal)
    {
        return principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
            ?? principal?.FindFirst("sub")?.Value;
    }

    /// <summary>
    /// Gets the user email from the claims principal.
    /// </summary>
    public static string? GetUserEmail(this ClaimsPrincipal principal)
    {
        return principal?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
    }

    /// <summary>
    /// Gets the team ID from the claims principal.
    /// </summary>
    public static string? GetTeamId(this ClaimsPrincipal principal)
    {
        return principal?.FindFirst(ClaimTypes.TeamId)?.Value;
    }

    /// <summary>
    /// Gets the user roles from the claims principal.
    /// </summary>
    public static List<string> GetRoles(this ClaimsPrincipal principal)
    {
        var roles = principal?.FindAll(System.Security.Claims.ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList() ?? new List<string>();

        return roles;
    }

    /// <summary>
    /// Checks if the user has a specific role.
    /// </summary>
    public static bool HasRole(this ClaimsPrincipal principal, string role)
    {
        return principal?.IsInRole(role) ?? false;
    }
}
