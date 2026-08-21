namespace TaskBridge.Core.Entities;

/// <summary>
/// Represents a project entity in the TaskBridge system.
/// Projects are scoped to organizations and managed by teams.
/// </summary>
public class Project : BaseEntity
{
    /// <summary>
    /// Gets or sets the team ID that owns this project.
    /// </summary>
    public string TeamId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current milestone status (PLANNING, IN_PROGRESS, COMPLETED, CLOSED).
    /// </summary>
    public string MilestoneStatus { get; set; } = "PLANNING";

    /// <summary>
    /// Gets or sets the user ID who created this project (for audit purposes).
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when this project was last modified (UTC).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the user ID who last modified this project.
    /// </summary>
    public string? UpdatedBy { get; set; }
}
