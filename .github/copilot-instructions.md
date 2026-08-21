# GitHub Copilot Instructions - TaskBridge Project

**Version**: 1.0
**Last Updated**: 2026-08-21
**Owner**: Development Team

---

## 1. Technology Stack & Architecture

### Tech Stack Declaration
- **Backend**: ASP.NET Core 8.0, C# 12
- **Database**: Entity Framework Core 8.0, SQL Server 2019+ / PostgreSQL 13+
- **Frontend**: Angular 18+ (TypeScript)
- **Testing**: xUnit, Moq, TestContainers
- **Architecture**: Layered (Controllers → Services → Repositories → Data Access)
- **Design Pattern**: Service-oriented microservices with multi-tenant isolation
- **API Style**: RESTful with OpenAPI 3.0 documentation

### Project Structure
```
src/
├── TaskBridge.Projects/           # Project Service module
├── TaskBridge.Notifications/      # Notification & Audit Service module
└── TaskBridge.Core/               # Shared entities, interfaces, exceptions

tests/
├── TaskBridge.Projects.Tests/
├── TaskBridge.Notifications.Tests/
└── TaskBridge.Integration.Tests/
```

---

## 2. Coding Standards & Conventions

### Naming Conventions
- **Classes**: PascalCase (e.g., `ProjectService`, `AuditLogRepository`)
- **Methods/Functions**: PascalCase (e.g., `CreateProject`, `GetAuditHistory`)
- **Properties**: PascalCase (e.g., `ProjectId`, `CreatedAt`)
- **Variables/Parameters**: camelCase (e.g., `projectId`, `organizationId`)
- **Constants**: UPPER_SNAKE_CASE (e.g., `MAX_BATCH_SIZE`)
- **Interfaces**: Prefix with `I` (e.g., `IProjectService`, `IAuditRepository`)
- **Database Tables**: Singular form, PascalCase (e.g., `Project`, `AuditLog`, `Notification`)
- **DTOs**: Suffix with `Dto` (e.g., `CreateProjectDto`, `AuditLogDto`)

### Code Organization

#### Services Layer
```csharp
public interface IProjectService
{
    Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto, string organizationId, string userId);
    Task<ProjectDto> UpdateMilestoneStatusAsync(string projectId, string status, string organizationId, string userId);
    Task<List<ProjectDto>> GetProjectsByTeamAsync(string teamId, string organizationId);
    Task DeleteProjectAsync(string projectId, string organizationId, string userId);
}

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _repository;
    private readonly ILogger<ProjectService> _logger;
    
    public ProjectService(IProjectRepository repository, ILogger<ProjectService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    
    // Implementation follows DRY principle
    // All public methods must validate multi-tenant context
    // All business logic must be here, not in controllers
}
```

#### Controller Pattern
```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _service;
    private readonly ILogger<ProjectsController> _logger;
    
    public ProjectsController(IProjectService service, ILogger<ProjectsController> logger)
    {
        _service = service;
        _logger = logger;
    }
    
    // Controllers should:
    // - ONLY handle HTTP concern (serialization, status codes, headers)
    // - NOT contain business logic
    // - Validate inputs and map to DTOs
    // - Log key events
    // - Return typed responses
}
```

### Method Documentation
Every public method MUST have XML documentation:
```csharp
/// <summary>
/// Creates a new project within an organization.
/// </summary>
/// <param name="dto">Project creation details</param>
/// <param name="organizationId">The organization context (required for multi-tenant isolation)</param>
/// <param name="userId">The user performing the action (for audit logging)</param>
/// <returns>The created project DTO</returns>
/// <exception cref="ValidationException">Thrown when input validation fails</exception>
/// <exception cref="UnauthorizedException">Thrown when user lacks permission</exception>
public async Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto, string organizationId, string userId)
{
    // Implementation
}
```

### Async Patterns
- **Always use async/await** for I/O operations (database, HTTP calls, file operations)
- Method names must end with `Async` (e.g., `GetProjectAsync`, `CreateAuditLogAsync`)
- Use `Task<T>` return type for async operations
- **Never block** on async operations (avoid `.Result`, `.Wait()`)

### Error Handling

#### Custom Exceptions (in TaskBridge.Core/Exceptions/)
```csharp
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
```

#### Exception Handling Middleware
```csharp
// Global exception handler - catches all service layer exceptions
// Returns appropriate HTTP status codes:
// - ValidationException → 400 Bad Request
// - UnauthorizedException → 403 Forbidden
// - NotFoundException → 404 Not Found
// - ConflictException → 409 Conflict
// - Generic Exception → 500 Internal Server Error
```

### Logging Standards

**Use structured logging with ILogger:**

```csharp
_logger.LogInformation("Project created: {ProjectId} in organization {OrganizationId} by {UserId}",
    project.Id, organizationId, userId);

_logger.LogWarning("Unauthorized access attempt: User {UserId} tried to access organization {OrganizationId}",
    userId, organizationId);

_logger.LogError(ex, "Failed to update project {ProjectId}: {ErrorMessage}",
    projectId, ex.Message);
```

**Log Levels**:
- **LogError**: Application failures that require immediate attention
- **LogWarning**: Potentially harmful situations (auth failures, invalid inputs)
- **LogInformation**: Key business events (entity creation, milestone changes)
- **LogDebug**: Development diagnostics only
- **LogTrace**: Very detailed flow (use sparingly)

---

## 3. Multi-Tenant Security & Architecture

### Fundamental Rule: Organization Context is Mandatory
**Every database query MUST be scoped to the current organization.**

```csharp
// ✅ CORRECT - Query is scoped to organization
var projects = await _context.Projects
    .Where(p => p.OrganizationId == organizationId)
    .ToListAsync();

// ❌ WRONG - Query lacks organization scope (SECURITY BUG)
var projects = await _context.Projects.ToListAsync();
```

### Data Isolation Pattern

#### Entity Models
Every entity MUST include organizational context:

```csharp
public class Project
{
    public string Id { get; set; }
    public string OrganizationId { get; set; }  // REQUIRED for multi-tenancy
    public string TeamId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }  // User ID for audit
    // ... other properties
}

public class AuditLog
{
    public string Id { get; set; }
    public string OrganizationId { get; set; }  // REQUIRED for multi-tenancy
    public string ProjectId { get; set; }
    public string EventType { get; set; }  // "CREATED", "UPDATED", "DELETED"
    public string EntityType { get; set; }  // "MILESTONE", "PROJECT"
    public string EntityId { get; set; }
    public string ActorId { get; set; }  // User ID
    public string PreviousState { get; set; }  // JSON snapshot
    public string NewState { get; set; }  // JSON snapshot
    public DateTime CreatedAt { get; set; }
    // Audit logs are IMMUTABLE - no updates or deletes
}
```

#### Repository Pattern
```csharp
public interface IProjectRepository
{
    Task<Project> GetByIdAsync(string projectId, string organizationId);
    Task<List<Project>> GetByTeamAsync(string teamId, string organizationId);
    Task<Project> CreateAsync(Project project, string organizationId);
    Task UpdateAsync(Project project, string organizationId);
    Task DeleteAsync(string projectId, string organizationId);
}

public class ProjectRepository : IProjectRepository
{
    private readonly ProjectDbContext _context;
    
    public async Task<Project> GetByIdAsync(string projectId, string organizationId)
    {
        // ✅ CORRECT - scope to organization
        return await _context.Projects
            .SingleOrDefaultAsync(p => p.Id == projectId && p.OrganizationId == organizationId);
    }
}
```

### Authentication & Authorization

#### User Context
Every request must carry:
- `userId`: The authenticated user's ID
- `organizationId`: The user's organization context
- `userRoles`: Roles within that organization (e.g., "Admin", "Editor", "Viewer")

#### Middleware Pattern
```csharp
// Middleware that extracts and validates user context from JWT
public class TenantContextMiddleware
{
    public async Task InvokeAsync(HttpContext context, /* dependencies */)
    {
        var organizationId = context.User.FindFirst("organization_id")?.Value;
        if (string.IsNullOrEmpty(organizationId))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { error = "Missing organization context" });
            return;
        }
        
        // Attach to context for use in handlers
        context.Items["OrganizationId"] = organizationId;
        await _next(context);
    }
}
```

### Input Validation

#### DTO Validation Rules
```csharp
public class CreateProjectDto
{
    [Required(ErrorMessage = "Project name is required")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Project name must be 1-255 characters")]
    public string Name { get; set; }
    
    [StringLength(1000, ErrorMessage = "Description must not exceed 1000 characters")]
    public string Description { get; set; }
    
    [Required(ErrorMessage = "Team ID is required")]
    public string TeamId { get; set; }
}
```

#### Service-Level Validation
```csharp
public async Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto, string organizationId, string userId)
{
    // Validate input
    if (string.IsNullOrWhiteSpace(dto.Name))
        throw new ValidationException("Project name cannot be empty");
    
    // Validate organizational context
    if (string.IsNullOrEmpty(organizationId))
        throw new UnauthorizedException("Organization context is required");
    
    // Verify team belongs to organization
    var team = await _teamRepository.GetByIdAsync(dto.TeamId, organizationId);
    if (team == null)
        throw new NotFoundException($"Team {dto.TeamId} not found in organization");
    
    // Proceed with creation
    var project = new Project { /* ... */ };
    return await _repository.CreateAsync(project, organizationId);
}
```

### Data Exposure Prevention

#### Principle: Never leak organization data across boundaries

```csharp
// ❌ WRONG - Returns all projects without filtering
public class ProjectsController
{
    [HttpGet]
    public async Task<IActionResult> GetAllProjects()
    {
        var projects = await _service.GetAllAsync();  // No org context!
        return Ok(projects);
    }
}

// ✅ CORRECT - Scopes to user's organization
public class ProjectsController
{
    [HttpGet]
    public async Task<IActionResult> GetProjects()
    {
        var organizationId = User.FindFirst("organization_id")?.Value;
        var teamId = User.FindFirst("team_id")?.Value;
        var projects = await _service.GetProjectsByTeamAsync(teamId, organizationId);
        return Ok(projects);
    }
}
```

---

## 4. Data Models & Database Patterns

### Entity Framework Core Configuration

```csharp
public class ProjectDbContext : DbContext
{
    public ProjectDbContext(DbContextOptions<ProjectDbContext> options) : base(options) { }
    
    public DbSet<Project> Projects { get; set; }
    public DbSet<Team> Teams { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure Project entity
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.OrganizationId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            // Indexes for common queries
            entity.HasIndex(e => new { e.OrganizationId, e.TeamId });
            entity.HasIndex(e => new { e.OrganizationId, e.CreatedAt });
        });
        
        // Configure AuditLog entity (immutable)
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrganizationId).IsRequired();
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            // Indexes for audit queries
            entity.HasIndex(e => new { e.OrganizationId, e.ProjectId });
            entity.HasIndex(e => new { e.OrganizationId, e.CreatedAt });
            entity.HasIndex(e => new { e.OrganizationId, e.EventType });
        });
    }
}
```

### Migration Pattern

```bash
# Create new migration
dotnet ef migrations add AddProjectTable

# Apply migration
dotnet ef database update

# Create idempotent SQL script for production deployment
dotnet ef migrations script
```

---

## 5. Testing Standards & Patterns

### Unit Test Structure

```csharp
public class ProjectServiceTests
{
    private readonly IProjectRepository _repositoryMock;
    private readonly ILogger<ProjectService> _loggerMock;
    private readonly ProjectService _service;
    
    public ProjectServiceTests()
    {
        _repositoryMock = new Mock<IProjectRepository>();
        _loggerMock = new Mock<ILogger<ProjectService>>();
        _service = new ProjectService(_repositoryMock.Object, _loggerMock.Object);
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task CreateProjectAsync_WithValidInput_CreatesProjectSuccessfully()
    {
        // Arrange
        var dto = new CreateProjectDto { Name = "Test Project", TeamId = "team-1" };
        var organizationId = "org-1";
        var userId = "user-1";
        
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Project>(), organizationId))
            .ReturnsAsync(new Project { Id = "proj-1", Name = dto.Name });
        
        // Act
        var result = await _service.CreateProjectAsync(dto, organizationId, userId);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Project", result.Name);
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Project>(), organizationId), Times.Once);
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task CreateProjectAsync_WithoutOrganization_ThrowsUnauthorizedException()
    {
        // Arrange
        var dto = new CreateProjectDto { Name = "Test" };
        var organizationId = string.Empty;  // Missing organization
        
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _service.CreateProjectAsync(dto, organizationId, "user-1")
        );
    }
}
```

### Integration Test Structure

```csharp
public class ProjectServiceIntegrationTests : IAsyncLifetime
{
    private readonly TestcontainersDatabase _database;
    private readonly ProjectDbContext _context;
    private readonly IProjectService _service;
    
    public async Task InitializeAsync()
    {
        _database = await TestcontainersDatabase.CreateAsync();
        _context = new ProjectDbContext(_database.GetOptions());
        await _context.Database.MigrateAsync();
        _service = new ProjectService(new ProjectRepository(_context), /* logger */);
    }
    
    [Fact]
    [Trait("Category", "Integration")]
    public async Task CreateAndRetrieveProject_EndToEnd()
    {
        // Arrange
        var organizationId = "org-1";
        var dto = new CreateProjectDto { Name = "Integration Test" };
        
        // Act
        var created = await _service.CreateProjectAsync(dto, organizationId, "user-1");
        var retrieved = await _service.GetProjectAsync(created.Id, organizationId);
        
        // Assert
        Assert.Equal(created.Name, retrieved.Name);
    }
    
    public async Task DisposeAsync()
    {
        await _database.DisposeAsync();
    }
}
```

### Test Naming Convention

```
[UnitUnderTest]_[Scenario]_[ExpectedResult]

Examples:
- CreateProjectAsync_WithValidInput_CreatesProjectSuccessfully
- AuditLogRepository_InsertAuditEntry_EnforcesImmutability
- NotificationService_DispatchNotification_SendsToAllTeamMembers
```

### Test Traits

```csharp
[Trait("Category", "Unit")]       // Fast, isolated
[Trait("Category", "Integration")] // Database, real context
[Trait("Category", "Security")]    // Multi-tenant isolation, auth
```

---

## 6. API Endpoint Patterns

### RESTful Conventions

```
POST   /api/v1/projects                     Create project
GET    /api/v1/projects/{teamId}            Get projects by team
PATCH  /api/v1/projects/{id}/milestone/{status}  Update milestone
DELETE /api/v1/projects/{id}                Delete project

POST   /api/v1/audit                        Record audit event (internal)
GET    /api/v1/audit/{projectId}            Get audit history
GET    /api/v1/notifications/{userId}       Get notifications
PATCH  /api/v1/notifications/{id}/read      Mark as read
```

### Response Format

```csharp
// Success Response
{
    "success": true,
    "data": { /* entity */ },
    "timestamp": "2026-08-21T10:30:00Z"
}

// Error Response
{
    "success": false,
    "error": "Descriptive error message",
    "errorCode": "VALIDATION_ERROR",
    "timestamp": "2026-08-21T10:30:00Z",
    "details": { /* optional detailed errors */ }
}
```

### Status Codes

- **200 OK**: Successful GET/PATCH
- **201 Created**: Successful POST
- **204 No Content**: Successful DELETE
- **400 Bad Request**: Validation errors
- **401 Unauthorized**: Missing/invalid authentication
- **403 Forbidden**: Insufficient permissions or org mismatch
- **404 Not Found**: Resource not found
- **409 Conflict**: Resource conflict (e.g., duplicate)
- **500 Internal Server Error**: Unhandled exceptions

---

## 7. Immutability Pattern (for Audit Logs)

### Enforcement Strategy

```csharp
public class AuditLog
{
    public string Id { get; set; }
    // ... other properties
    
    // NO setters after creation
    // Use private setters for EF Core initialization
    private AuditLog() { }  // Required for EF Core
}

public interface IAuditLogRepository
{
    Task<AuditLog> CreateAsync(AuditLog entry, string organizationId);
    Task<List<AuditLog>> GetByProjectAsync(string projectId, string organizationId, 
        DateTime? fromDate = null, DateTime? toDate = null, string eventType = null);
    // NOTE: No Update() or Delete() methods
}

// Service layer prevents updates
public class AuditLogService
{
    public async Task RecordEventAsync(string projectId, string eventType, 
        string previousState, string newState, string organizationId, string userId)
    {
        var entry = new AuditLog 
        { 
            Id = Guid.NewGuid().ToString(),
            ProjectId = projectId,
            EventType = eventType,
            PreviousState = previousState,
            NewState = newState,
            OrganizationId = organizationId,
            ActorId = userId,
            CreatedAt = DateTime.UtcNow
        };
        
        await _repository.CreateAsync(entry, organizationId);
        // ✅ No possibility to modify after creation
    }
}
```

---

## 8. Dependency Injection & Configuration

### Service Registration Pattern

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProjectServices(this IServiceCollection services)
    {
        // Register repositories
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<ITeamRepository, TeamRepository>();
        
        // Register services
        services.AddScoped<IProjectService, ProjectService>();
        
        // Register DbContext
        services.AddDbContext<ProjectDbContext>((provider, options) =>
        {
            var connectionString = provider.GetRequiredService<IConfiguration>()
                .GetConnectionString("DefaultConnection");
            options.UseSqlServer(connectionString);
        });
        
        return services;
    }
    
    public static IServiceCollection AddNotificationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<INotificationService, NotificationService>();
        
        services.AddDbContext<NotificationDbContext>((provider, options) =>
        {
            var connectionString = provider.GetRequiredService<IConfiguration>()
                .GetConnectionString("DefaultConnection");
            options.UseSqlServer(connectionString);
        });
        
        return services;
    }
}

// In Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProjectServices();
builder.Services.AddNotificationServices();
builder.Services.AddScoped<TenantContextMiddleware>();

var app = builder.Build();
app.UseMiddleware<TenantContextMiddleware>();
app.MapControllers();
app.Run();
```

---

## 9. Security Best Practices

### JWT Token Claims

```csharp
// Expected claims in JWT token
{
    "sub": "user-123",
    "email": "user@company.com",
    "organization_id": "org-456",
    "organization_name": "Acme Corp",
    "team_id": "team-789",
    "roles": ["Editor", "Viewer"],
    "iat": 1692604800,
    "exp": 1692691200
}
```

### Authorization Attribute

```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeOrganizationAttribute : Attribute
{
    // Validates that user's organization matches the requested organization
}

// Usage
[ApiController]
[Route("api/v1/[controller]")]
public class ProjectsController : ControllerBase
{
    [HttpGet("{projectId}")]
    [Authorize]
    [AuthorizeOrganization]
    public async Task<IActionResult> GetProject(string projectId)
    {
        var organizationId = User.FindFirst("organization_id")?.Value;
        // ...
    }
}
```

### Sensitive Data Protection

- **Never log**: Passwords, tokens, API keys, PII
- **Sanitize errors**: Return generic messages to clients, log detailed errors server-side
- **Encrypt in transit**: Always use HTTPS
- **Hash sensitive fields**: Use appropriate hashing for passwords
- **Audit sensitive operations**: Log who accessed/modified what

---

## 10. Git & Commit Standards

### Conventional Commits Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types**:
- `feat`: New feature (e.g., `feat(audit): add immutable audit log service`)
- `fix`: Bug fix (e.g., `fix(auth): enforce organization isolation`)
- `refactor`: Code restructuring without feature changes
- `test`: Adding or updating tests
- `docs`: Documentation updates
- `chore`: Build, dependency, or tooling changes
- `perf`: Performance improvements
- `security`: Security fixes or hardening

**Examples**:
```
feat(notifications): implement audit log model

Add AuditLog entity with immutable design pattern.
Include indexes for org_id and project_id queries.
Enforce immutability at service layer.

Closes #42

feat(audit): add immutable audit log service

Implement IAuditLogService to record milestone changes.
Support event type filtering in audit history queries.
Enforce organization-scoped audit log access.

Breaking change: None
Security: Audit logs capture actor ID and timestamp

feat(notifications): create notification model and repository

Add Notification entity for user notifications.
Implement INotificationRepository with read status tracking.
Add indexes for userId and isRead queries.

security(auth): validate organization context in all queries

Add OrganizationId validation to ProjectRepository methods.
Throw UnauthorizedException if org context mismatched.
Add integration test for org isolation.

Closes #45
```

---

## 11. Copilot Usage Guidelines

### When to Use Copilot

✅ **Use Copilot for**:
- Boilerplate code (CRUD operations, DTOs, validators)
- Test case generation with proper mocking patterns
- Code documentation and XML comments
- Refactoring suggestions within established patterns
- Repetitive patterns (e.g., multiple controllers following same structure)
- SQL query suggestions (review thoroughly for org isolation)
- Logging statements

❌ **Do NOT rely on Copilot for**:
- Security-critical logic (auth, org isolation, encryption)
- Multi-tenant data access patterns (must be reviewed)
- Error handling strategies
- Architectural decisions
- Complex business logic
- Immutability enforcement

### Copilot Prompt Template for This Project

```
You are a senior C# backend engineer reviewing code for a multi-tenant B2B SaaS platform.
Respect this context:
- Technology: ASP.NET Core 8.0, Entity Framework Core, multi-tenant architecture
- Every entity MUST include OrganizationId
- Every database query MUST scope to OrganizationId
- Every service method MUST accept organizationId parameter
- All repositories use async/await patterns
- Use dependency injection throughout
- Follow Conventional Commits for git commits

When generating code:
1. Include XML documentation for all public methods
2. Always include multi-tenant scoping (organizationId checks)
3. Use custom exceptions (ValidationException, UnauthorizedException, NotFoundException)
4. Implement structured logging with ILogger
5. Generate corresponding xUnit test cases
6. Enforce immutability for audit logs

[Your specific request]
```

---

## 12. Code Review Checklist

Every PR must pass this checklist before merge:

- [ ] Code follows naming conventions (PascalCase classes, camelCase vars)
- [ ] All public methods have XML documentation
- [ ] No hardcoded credentials or secrets
- [ ] All database queries scope to `OrganizationId`
- [ ] Exception handling is appropriate (custom exceptions, no generic Exception)
- [ ] Structured logging is in place (`ILogger.LogInformation/LogError`)
- [ ] Unit tests exist and pass
- [ ] Integration tests validate multi-tenant isolation
- [ ] No `.Result` or `.Wait()` calls (async all the way)
- [ ] Dependency injection properly configured
- [ ] Commit messages follow Conventional Commits format
- [ ] Security: No user input reaches database without validation
- [ ] Security: Auth middleware enforces organization context
- [ ] Error messages don't leak sensitive data
- [ ] Test coverage ≥ 80% for modified code

---

## 13. Helpful Resources

- ASP.NET Core Best Practices: https://docs.microsoft.com/en-us/dotnet/core/
- Entity Framework Core: https://docs.microsoft.com/en-us/ef/core/
- xUnit Testing: https://xunit.net/
- OWASP Security: https://owasp.org/
- Microsoft Security Best Practices: https://docs.microsoft.com/en-us/security/

---

**Last Reviewed**: 2026-08-21
**Maintained by**: Development Team
**Questions?** Refer to this document and apply principles consistently. Ask lead engineer for exceptions.
