# Deliverable README

This deliverable contains the TaskBridge solution and supporting documentation for the Notification & Audit Service and Project Service.

Technology stack
- Backend: ASP.NET Core 8.0 (C# 12)
- ORM: Entity Framework Core 8.0
- Database: SQL Server / PostgreSQL (migration-ready)
- Testing: xUnit, Moq, Microsoft.NET.Test.Sdk
- CI: GitHub Actions (recommended)
- Messaging: Durable message bus recommended (e.g., Azure Service Bus, RabbitMQ, Kafka)

How to run tests

```bash
# from repository root
dotnet test
```

Repository layout

- src/ - application projects (TaskBridge.Core, TaskBridge.Projects, TaskBridge.Notifications)
- tests/ - xUnit test projects and integration placeholders
- docs/ - optional documentation

Contact
- Maintainer: dineshkumar-sr-cloud
