using System.ComponentModel.DataAnnotations;

namespace TaskBridge.Projects.Models.Dtos;

/// <summary>
/// DTO for creating a new project.
/// </summary>
public class CreateProjectDto
{
    /// <summary>
    /// Gets or sets the project name (required, 1-255 characters).
    /// </summary>
    [Required(ErrorMessage = "Project name is required")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Project name must be 1-255 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project description (optional, max 1000 characters).
    /// </summary>
    [StringLength(1000, ErrorMessage = "Description must not exceed 1000 characters")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the team ID that will own this project (required).
    /// </summary>
    [Required(ErrorMessage = "Team ID is required")]
    [StringLength(50, ErrorMessage = "Team ID must not exceed 50 characters")]
    public string TeamId { get; set; } = string.Empty;
}
