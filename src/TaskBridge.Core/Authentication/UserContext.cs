namespace TaskBridge.Core.Authentication;

/// <summary>
/// Represents user context extracted from JWT token and request metadata.
/// </summary>
public class UserContext
{
    /// <summary>
    /// Gets or sets the user ID (subject).
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the organization ID (multi-tenant context).
    /// </summary>
    public string OrganizationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the organization name.
    /// </summary>
    public string OrganizationName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the team ID.
    /// </summary>
    public string? TeamId { get; set; }

    /// <summary>
    /// Gets or sets the user email.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the list of user roles.
    /// </summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>
    /// Gets or sets the user's IP address.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Validates that the user context has all required fields.
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(UserId) && !string.IsNullOrEmpty(OrganizationId);
    }
}
