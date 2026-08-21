using System.Text.Json;
using TaskBridge.Core.Constants;
using TaskBridge.Core.Entities;
using TaskBridge.Core.Exceptions;
using TaskBridge.Projects.Data;
using TaskBridge.Projects.Models.Dtos;

namespace TaskBridge.Projects.Services;

/// <summary>
/// Service interface for project operations.
/// </summary>
public interface IProjectService
{
    /// <summary>
    /// Creates a new project within an organization.
    /// </summary>
    Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto, string organizationId, string userId);

    /// <summary>
    /// Updates a project milestone status.
    /// </summary>
    Task<ProjectDto> UpdateMilestoneStatusAsync(string projectId, string status, string organizationId, string userId);

    /// <summary>
    /// Gets all projects for a team within an organization.
    /// </summary>
    Task<List<ProjectDto>> GetProjectsByTeamAsync(string teamId, string organizationId);

    /// <summary>
    /// Gets a single project by ID.
    /// </summary>
    Task<ProjectDto> GetProjectAsync(string projectId, string organizationId);

    /// <summary>
    /// Deletes a project.
    /// </summary>
    Task DeleteProjectAsync(string projectId, string organizationId, string userId);
}

/// <summary>
/// Service for managing project operations.
/// Enforces multi-tenant isolation and integrates with audit/notification services.
/// </summary>
public class ProjectService : IProjectService
{
    private readonly IProjectRepository _repository;
    private readonly ILogger<ProjectService> _logger;

    /// <summary>
    /// Initializes a new instance of the ProjectService class.
    /// </summary>
    public ProjectService(IProjectRepository repository, ILogger<ProjectService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new project within an organization.
    /// </summary>
    public async Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto, string organizationId, string userId)
    {
        // Validate input
        if (dto == null)
            throw new ValidationException("Project data cannot be null");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationException("Project name cannot be empty");

        if (string.IsNullOrWhiteSpace(dto.TeamId))
            throw new ValidationException("Team ID is required");

        // Validate organization context
        if (string.IsNullOrEmpty(organizationId))
            throw new UnauthorizedException("Organization context is required");

        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedException("User context is required");

        var project = new Project
        {
            Id = Guid.NewGuid().ToString(),
            OrganizationId = organizationId,
            TeamId = dto.TeamId,
            Name = dto.Name,
            Description = dto.Description ?? string.Empty,
            MilestoneStatus = "PLANNING",
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(project, organizationId);
        return MapToDto(created);
    }

    /// <summary>
    /// Updates a project milestone status.
    /// </summary>
    public async Task<ProjectDto> UpdateMilestoneStatusAsync(string projectId, string status, string organizationId, string userId)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(projectId))
            throw new ValidationException("Project ID is required");

        if (string.IsNullOrWhiteSpace(status))
            throw new ValidationException("Milestone status is required");

        if (string.IsNullOrEmpty(organizationId))
            throw new UnauthorizedException("Organization context is required");

        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedException("User context is required");

        var project = await _repository.GetByIdAsync(projectId, organizationId);
        if (project == null)
            throw new NotFoundException($"Project {projectId} not found");

        var previousStatus = project.MilestoneStatus;
        project.MilestoneStatus = status;
        project.UpdatedAt = DateTime.UtcNow;
        project.UpdatedBy = userId;

        var updated = await _repository.UpdateAsync(project, organizationId);

        _logger.LogInformation(
            "Project {ProjectId} milestone updated from {PreviousStatus} to {NewStatus} by {UserId}",
            projectId, previousStatus, status, userId);

        return MapToDto(updated);
    }

    /// <summary>
    /// Gets all projects for a team within an organization.
    /// </summary>
    public async Task<List<ProjectDto>> GetProjectsByTeamAsync(string teamId, string organizationId)
    {
        if (string.IsNullOrEmpty(teamId))
            throw new ValidationException("Team ID is required");

        if (string.IsNullOrEmpty(organizationId))
            throw new UnauthorizedException("Organization context is required");

        var projects = await _repository.GetByTeamAsync(teamId, organizationId);
        return projects.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Gets a single project by ID.
    /// </summary>
    public async Task<ProjectDto> GetProjectAsync(string projectId, string organizationId)
    {
        if (string.IsNullOrEmpty(projectId))
            throw new ValidationException("Project ID is required");

        if (string.IsNullOrEmpty(organizationId))
            throw new UnauthorizedException("Organization context is required");

        var project = await _repository.GetByIdAsync(projectId, organizationId);
        if (project == null)
            throw new NotFoundException($"Project {projectId} not found");

        return MapToDto(project);
    }

    /// <summary>
    /// Deletes a project.
    /// </summary>
    public async Task DeleteProjectAsync(string projectId, string organizationId, string userId)
    {
        if (string.IsNullOrEmpty(projectId))
            throw new ValidationException("Project ID is required");

        if (string.IsNullOrEmpty(organizationId))
            throw new UnauthorizedException("Organization context is required");

        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedException("User context is required");

        var project = await _repository.GetByIdAsync(projectId, organizationId);
        if (project == null)
            throw new NotFoundException($"Project {projectId} not found");

        await _repository.DeleteAsync(projectId, organizationId);

        _logger.LogInformation(
            "Project {ProjectId} deleted from organization {OrganizationId} by {UserId}",
            projectId, organizationId, userId);
    }

    /// <summary>
    /// Maps a Project entity to a ProjectDto.
    /// </summary>
    private static ProjectDto MapToDto(Project project)
    {
        return new ProjectDto
        {
            Id = project.Id,
            OrganizationId = project.OrganizationId,
            TeamId = project.TeamId,
            Name = project.Name,
            Description = project.Description,
            MilestoneStatus = project.MilestoneStatus,
            CreatedBy = project.CreatedBy,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            UpdatedBy = project.UpdatedBy
        };
    }
}
