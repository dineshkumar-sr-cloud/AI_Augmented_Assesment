# TaskBridge - AI Augmented Notification & Audit Service

## Project Overview

TaskBridge is a B2B SaaS project collaboration platform for distributed engineering teams. This repository contains the **Notification & Audit Service** alongside the **Project Service**, implementing real-time notifications and immutable audit logging for project milestone changes.

## Technology Stack

### Backend
- **Runtime**: .NET 8.0 / .NET Core
- **Framework**: ASP.NET Core
- **ORM**: Entity Framework Core
- **Language**: C#
- **API**: RESTful with OpenAPI/Swagger documentation

### Frontend (Future)
- **Framework**: Angular 18+
- **Language**: TypeScript
- **UI Components**: Angular Material or Bootstrap
- **State Management**: RxJS with Services

### Database
- **Primary**: SQL Server 2019+ or PostgreSQL 13+
- **Migrations**: Entity Framework Core Code-First Migrations
- **Features**: Multi-tenant isolation with organizational segregation

### Testing
- **Unit Tests**: xUnit
- **Mocking**: Moq
- **Integration Tests**: TestContainers
- **Coverage**: Minimum 80%

### Development Tools
- **IDE**: Visual Studio Code / Visual Studio 2022
- **Version Control**: Git with Conventional Commits
- **AI Assistant**: GitHub Copilot (with Chat enabled)
- **Build System**: .NET CLI
- **Package Manager**: NuGet

## Project Structure

```
taskbridge-api/
├── .github/
│   └── copilot-instructions.md         # Copilot standards & conventions
├── src/
│   ├── TaskBridge.Projects/
│   │   ├── Models/
│   │   ├── Data/
│   │   ├── Services/
│   │   ├── Controllers/
│   │   ├── DTOs/
│   │   └── ProjectDbContext.cs
│   ├── TaskBridge.Notifications/
│   │   ├── Models/
│   │   ├── Data/
│   │   ├── Services/
│   │   ├── Controllers/
│   │   ├── DTOs/
│   │   └── NotificationDbContext.cs
│   └── TaskBridge.Core/
│       ├── Entities/
│       ├── Interfaces/
│       ├── Constants/
│       ├── Enums/
│       └── Exceptions/
├── tests/
│   ├── TaskBridge.Projects.Tests/
│   ├── TaskBridge.Notifications.Tests/
│   └── TaskBridge.Integration.Tests/
├── docs/
│   ├── SPEC.md
│   ├── REVIEW.md
│   ├── ARCHITECTURE.md
│   ├── IMPACT_ANALYSIS.md
│   └── PROMPTS.md
├── .gitignore
├── TaskBridge.sln                      # Solution file
└── README.md
```

## Core Services

### 1. Project Service
- Manages project milestones and state changes
- Triggers notifications and audit events
- Provides project data queries with multi-tenant isolation
- **Base URL**: `/api/v1/projects`

### 2. Notification & Audit Service
- Records immutable audit entries for compliance
- Dispatches notifications to team members
- Provides audit history queries with filters
- **Base URL**: `/api/v1/audit`, `/api/v1/notifications`

## Getting Started

### Prerequisites
- .NET 8.0 SDK or later
- SQL Server 2019+ or PostgreSQL 13+
- Git
- Visual Studio Code or Visual Studio 2022 (optional)

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/dineshkumar-sr-cloud/AI_Augmented_Assesment.git
   cd AI_Augmented_Assesment
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure database connection**
   - Update `appsettings.json` with your database connection string
   - For SQL Server: `Server=localhost;Database=taskbridge;User Id=sa;Password=YourPassword;`
   - For PostgreSQL: `Host=localhost;Database=taskbridge;Username=postgres;Password=password;`

4. **Run migrations**
   ```bash
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access Swagger UI**
   - Navigate to `https://localhost:5001/swagger/index.html`

### Running Tests

```bash
# Unit tests only
dotnet test --filter "Category=Unit"

# Integration tests only
dotnet test --filter "Category=Integration"

# All tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Documentation

- **[SPEC.md](./docs/SPEC.md)** - Technical specification for Notification & Audit Service
- **[REVIEW.md](./docs/REVIEW.md)** - Code review of Project Service with security analysis
- **[ARCHITECTURE.md](./docs/ARCHITECTURE.md)** - System architecture and design decisions
- **[IMPACT_ANALYSIS.md](./docs/IMPACT_ANALYSIS.md)** - Scope change analysis and risk assessment
- **[PROMPTS.md](./docs/PROMPTS.md)** - GitHub Copilot prompts used and methodology
- **[TOOL_STRATEGY.md](./docs/TOOL_STRATEGY.md)** - AI tool strategy and limitations encountered
- **[PR_DESCRIPTION.md](./docs/PR_DESCRIPTION.md)** - Pull request narrative and peer review

## Copilot Instructions

All developers must review `.github/copilot-instructions.md` before writing code. This file establishes:
- Coding standards and conventions
- Multi-tenant security rules
- Architecture patterns
- Testing requirements
- Logging and error handling standards

## API Overview

### Project Service
- `POST /api/v1/projects` - Create project
- `PATCH /api/v1/projects/{id}/milestone/{status}` - Update milestone status
- `GET /api/v1/projects/{teamId}` - Get projects by team
- `DELETE /api/v1/projects/{id}` - Delete project

### Notification & Audit Service
- `POST /api/v1/audit` - Record audit event (internal)
- `GET /api/v1/audit/{projectId}` - Get audit history
- `GET /api/v1/notifications/{userId}` - Get unread notifications
- `PATCH /api/v1/notifications/{id}/read` - Mark notification as read

## Multi-Tenant Architecture

This system is designed for multi-tenant B2B SaaS:
- Every user belongs to an **Organization**
- Projects are scoped to organizations
- Audit logs are organization-scoped
- Notifications respect organizational boundaries
- All queries enforce organization context validation

## Contributing

1. Create a feature branch: `git checkout -b feature/your-feature`
2. Follow Conventional Commits format
3. Ensure all tests pass and coverage ≥ 80%
4. Submit a pull request with comprehensive description
5. Await peer review before merging

## Security Considerations

- JWT-based authentication
- Role-based authorization (RBAC)
- Multi-tenant data isolation
- Audit trails for compliance (SOC 2, GDPR-ready)
- Input validation and sanitization
- Secure error messages (no data leakage)
- HTTPS enforced in production

## Support

For issues or questions:
1. Check existing GitHub Issues
2. Review documentation in `/docs`
3. Contact the TaskBridge team

## License

Proprietary - TaskBridge Inc. All rights reserved.

---

**Last Updated**: 2026-08-21
**Maintained by**: Dinesh Kumar (dineshkumar-sr-cloud)
