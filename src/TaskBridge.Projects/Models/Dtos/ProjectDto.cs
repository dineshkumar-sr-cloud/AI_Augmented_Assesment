namespace TaskBridge.Projects.Models.Dtos;

/// <summary>
/// DTO for representing a project in API responses.
/// </summary>
public class ProjectDto
{
    /// <summary>Gets or sets the project ID.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the team ID.</summary>
    public string TeamId { get; set; } = string.Empty;

    /// <summary>Gets or sets the organization ID.</summary>
    public string OrganizationId { get; set; } = string.Empty;

    /// <summary>Gets or sets the project name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the project description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the milestone status.</summary>
    public string MilestoneStatus { get; set; } = string.Empty;

    /// <summary>Gets or sets the user ID who created the project.</summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the last update timestamp.</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Gets or sets the user ID who last updated the project.</summary>
    public string? UpdatedBy { get; set; }
}
