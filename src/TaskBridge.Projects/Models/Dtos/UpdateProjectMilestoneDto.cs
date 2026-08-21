using System.ComponentModel.DataAnnotations;

namespace TaskBridge.Projects.Models.Dtos;

/// <summary>
/// DTO for updating a project milestone status.
/// </summary>
public class UpdateProjectMilestoneDto
{
    /// <summary>
    /// Gets or sets the new milestone status.
    /// Valid values: PLANNING, IN_PROGRESS, COMPLETED, CLOSED, REOPENED.
    /// </summary>
    [Required(ErrorMessage = "Milestone status is required")]
    [StringLength(50, ErrorMessage = "Status must not exceed 50 characters")]
    public string Status { get; set; } = string.Empty;
}
