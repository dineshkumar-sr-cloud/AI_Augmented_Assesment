using Microsoft.EntityFrameworkCore;
using TaskBridge.Core.Entities;
using TaskBridge.Core.Exceptions;

namespace TaskBridge.Projects.Data;

/// <summary>
/// Repository for Project entity data access operations.
/// All operations enforce multi-tenant isolation via OrganizationId.
/// </summary>
public interface IProjectRepository
{
    /// <summary>
    /// Gets a project by ID, scoped to the organization.
    /// </summary>
    Task<Project?> GetByIdAsync(string projectId, string organizationId);

    /// <summary>
    /// Gets all projects for a team within an organization.
    /// </summary>
    Task<List<Project>> GetByTeamAsync(string teamId, string organizationId);

    /// <summary>
    /// Creates a new project.
    /// </summary>
    Task<Project> CreateAsync(Project project, string organizationId);

    /// <summary>
    /// Updates an existing project.
    /// </summary>
    Task<Project> UpdateAsync(Project project, string organizationId);

    /// <summary>
    /// Deletes a project.
    /// </summary>
    Task DeleteAsync(string projectId, string organizationId);
}

/// <summary>
/// Implementation of IProjectRepository using Entity Framework Core.
/// </summary>
public class ProjectRepository : IProjectRepository
{
    private readonly ProjectDbContext _context;
    private readonly ILogger<ProjectRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the ProjectRepository class.
    /// </summary>
    public ProjectRepository(ProjectDbContext context, ILogger<ProjectRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets a project by ID, scoped to the organization.
    /// </summary>
    public async Task<Project?> GetByIdAsync(string projectId, string organizationId)
    {
        if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(organizationId))
        {
            _logger.LogWarning("GetByIdAsync called with empty projectId or organizationId");
            return null;
        }

        return await _context.Projects
            .SingleOrDefaultAsync(p => p.Id == projectId && p.OrganizationId == organizationId);
    }

    /// <summary>
    /// Gets all projects for a team within an organization.
    /// </summary>
    public async Task<List<Project>> GetByTeamAsync(string teamId, string organizationId)
    {
        if (string.IsNullOrEmpty(teamId) || string.IsNullOrEmpty(organizationId))
        {
            _logger.LogWarning("GetByTeamAsync called with empty teamId or organizationId");
            return new List<Project>();
        }

        return await _context.Projects
            .Where(p => p.TeamId == teamId && p.OrganizationId == organizationId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Creates a new project.
    /// </summary>
    public async Task<Project> CreateAsync(Project project, string organizationId)
    {
        if (project == null)
            throw new ValidationException("Project cannot be null");

        if (string.IsNullOrEmpty(organizationId))
            throw new ValidationException("Organization ID is required");

        project.OrganizationId = organizationId;
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Project created: {ProjectId} in organization {OrganizationId} by {UserId}",
            project.Id, organizationId, project.CreatedBy);

        return project;
    }

    /// <summary>
    /// Updates an existing project.
    /// </summary>
    public async Task<Project> UpdateAsync(Project project, string organizationId)
    {
        if (project == null)
            throw new ValidationException("Project cannot be null");

        if (string.IsNullOrEmpty(organizationId))
            throw new ValidationException("Organization ID is required");

        var existing = await GetByIdAsync(project.Id, organizationId);
        if (existing == null)
            throw new NotFoundException($"Project {project.Id} not found in organization {organizationId}");

        existing.Name = project.Name;
        existing.Description = project.Description;
        existing.MilestoneStatus = project.MilestoneStatus;
        existing.UpdatedAt = DateTime.UtcNow;
        existing.UpdatedBy = project.UpdatedBy;

        _context.Projects.Update(existing);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Project updated: {ProjectId} in organization {OrganizationId} by {UserId}",
            project.Id, organizationId, project.UpdatedBy);

        return existing;
    }

    /// <summary>
    /// Deletes a project.
    /// </summary>
    public async Task DeleteAsync(string projectId, string organizationId)
    {
        if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(organizationId))
            throw new ValidationException("Project ID and Organization ID are required");

        var project = await GetByIdAsync(projectId, organizationId);
        if (project == null)
            throw new NotFoundException($"Project {projectId} not found in organization {organizationId}");

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Project deleted: {ProjectId} from organization {OrganizationId}",
            projectId, organizationId);
    }
}
