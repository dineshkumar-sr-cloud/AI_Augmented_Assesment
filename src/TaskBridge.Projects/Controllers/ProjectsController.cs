using Microsoft.AspNetCore.Mvc;
using TaskBridge.Core.Authentication;
using TaskBridge.Core.Exceptions;
using TaskBridge.Projects.Models.Dtos;
using TaskBridge.Projects.Services;

namespace TaskBridge.Projects.Controllers;

/// <summary>
/// API controller for project operations.
/// Provides endpoints to manage projects and milestones.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _service;
    private readonly ILogger<ProjectsController> _logger;

    /// <summary>
    /// Initializes a new instance of the ProjectsController class.
    /// </summary>
    public ProjectsController(IProjectService service, ILogger<ProjectsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new project.
    /// </summary>
    /// <param name="dto">The project creation data.</param>
    /// <returns>The created project.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] CreateProjectDto dto)
    {
        try
        {
            var userContext = HttpContext.Items["UserContext"] as UserContext;
            if (userContext == null)
                return Unauthorized(new { error = "User context not found" });

            _logger.LogInformation(
                "Creating project: {ProjectName} for team {TeamId} by user {UserId}",
                dto.Name, dto.TeamId, userContext.UserId);

            var result = await _service.CreateProjectAsync(dto, userContext.OrganizationId, userContext.UserId);
            return CreatedAtAction(nameof(GetProject), new { id = result.Id }, result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation error creating project: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning("Unauthorized project creation attempt: {Message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Gets all projects for a team.
    /// </summary>
    /// <param name="teamId">The team ID.</param>
    /// <returns>List of projects for the team.</returns>
    [HttpGet("team/{teamId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<ProjectDto>>> GetProjectsByTeam(string teamId)
    {
        try
        {
            var userContext = HttpContext.Items["UserContext"] as UserContext;
            if (userContext == null)
                return Unauthorized(new { error = "User context not found" });

            _logger.LogInformation(
                "Retrieving projects for team {TeamId} by user {UserId}",
                teamId, userContext.UserId);

            var result = await _service.GetProjectsByTeamAsync(teamId, userContext.OrganizationId);

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
            _logger.LogWarning("Validation error retrieving projects: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning("Unauthorized project access attempt: {Message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving projects");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Gets a single project by ID.
    /// </summary>
    /// <param name="id">The project ID.</param>
    /// <returns>The project.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDto>> GetProject(string id)
    {
        try
        {
            var userContext = HttpContext.Items["UserContext"] as UserContext;
            if (userContext == null)
                return Unauthorized(new { error = "User context not found" });

            var result = await _service.GetProjectAsync(id, userContext.OrganizationId);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Project not found: {Message}", ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning("Unauthorized project access attempt: {Message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving project");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Updates a project milestone status.
    /// </summary>
    /// <param name="id">The project ID.</param>
    /// <param name="status">The new milestone status.</param>
    /// <returns>The updated project.</returns>
    [HttpPatch("{id}/milestone/{status}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDto>> UpdateMilestoneStatus(string id, string status)
    {
        try
        {
            var userContext = HttpContext.Items["UserContext"] as UserContext;
            if (userContext == null)
                return Unauthorized(new { error = "User context not found" });

            _logger.LogInformation(
                "Updating project {ProjectId} milestone status to {Status} by user {UserId}",
                id, status, userContext.UserId);

            var result = await _service.UpdateMilestoneStatusAsync(id, status, userContext.OrganizationId, userContext.UserId);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation error updating milestone: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Project not found: {Message}", ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning("Unauthorized milestone update attempt: {Message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating milestone status");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Deletes a project.
    /// </summary>
    /// <param name="id">The project ID.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProject(string id)
    {
        try
        {
            var userContext = HttpContext.Items["UserContext"] as UserContext;
            if (userContext == null)
                return Unauthorized(new { error = "User context not found" });

            _logger.LogInformation(
                "Deleting project {ProjectId} by user {UserId}",
                id, userContext.UserId);

            await _service.DeleteProjectAsync(id, userContext.OrganizationId, userContext.UserId);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Project not found: {Message}", ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning("Unauthorized project deletion attempt: {Message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting project");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Internal server error" });
        }
    }
}
