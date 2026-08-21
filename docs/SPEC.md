# Technical Specification - TaskBridge Notification & Audit Service

**Version**: 1.0  
**Status**: Draft for Review  
**Date**: 2026-08-21  
**Prepared by**: AI-Assisted Development (GitHub Copilot + Human Review)

---

## 1. Executive Summary

The Notification & Audit Service is a critical component of the TaskBridge platform that provides real-time notification dispatch and immutable audit logging for project milestone changes. This service sits between the Project Service and clients, ensuring compliance, accountability, and timely team member notifications in a multi-tenant B2B SaaS environment.

### Key Objectives
- Emit real-time notifications when project milestones change (created, updated, closed)
- Maintain immutable, tamper-proof audit logs for compliance
- Enable audit history queries with flexible filtering capabilities
- Enforce multi-tenant data isolation in all operations
- Support future scope changes (e.g., new milestone event types)

---

## 2. Data Models

### 2.1 AuditLog Entity

**Purpose**: Immutable record of state changes for compliance and audit trails.

```csharp
public class AuditLog : BaseEntity
{
    public string ProjectId { get; set; }              // Project identifier
    public string EventType { get; set; }              // e.g., PROJECT_CREATED, MILESTONE_UPDATED
    public string EntityType { get; set; }             // e.g., PROJECT, MILESTONE
    public string EntityId { get; set; }               // ID of the changed entity
    public string ActorId { get; set; }                // User ID who made the change
    public string? ActorIpAddress { get; set; }        // IP address for security audit (future)
    public string? PreviousState { get; set; }         // JSON snapshot before change
    public string? NewState { get; set; }              // JSON snapshot after change
    // Note: No UpdatedAt, UpdatedBy - immutable by design
}
```

**Field Specifications**:
| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | string(50) | PK, NOT NULL | UUID format, auto-generated |
| OrganizationId | string(50) | NOT NULL, FK | Multi-tenant context |
| ProjectId | string(50) | NOT NULL, IX | Project reference |
| EventType | string(50) | NOT NULL, IX | PROJECT_CREATED, MILESTONE_UPDATED, PROJECT_DELETED, MILESTONE_REOPENED |
| EntityType | string(50) | NOT NULL | PROJECT or MILESTONE |
| EntityId | string(50) | NOT NULL | Reference to changed entity |
| ActorId | string(50) | NOT NULL, IX | User who initiated change |
| ActorIpAddress | string(45) | NULL | IPv4/IPv6 address |
| PreviousState | NVARCHAR(MAX) | NULL | JSON serialization |
| NewState | NVARCHAR(MAX) | NULL | JSON serialization |
| CreatedAt | DateTime | NOT NULL, default=GETUTCDATE() | UTC timestamp |

**Immutability Guarantee**: No UPDATE or DELETE operations permitted at repository layer.

---

### 2.2 Notification Entity

**Purpose**: User notification records for real-time awareness of project changes.

```csharp
public class Notification : BaseEntity
{
    public string RecipientUserId { get; set; }        // Recipient user ID
    public string ProjectId { get; set; }              // Project reference
    public string EventType { get; set; }              // Event that triggered notification
    public string Message { get; set; }                // Human-readable message
    public bool IsRead { get; set; }                   // Read status flag
    public DateTime? ReadAt { get; set; }              // When user read the notification
}
```

**Field Specifications**:
| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | string(50) | PK, NOT NULL | UUID format |
| OrganizationId | string(50) | NOT NULL, IX | Multi-tenant context |
| RecipientUserId | string(50) | NOT NULL, IX | User receiving notification |
| ProjectId | string(50) | NOT NULL | Project reference |
| EventType | string(50) | NOT NULL | Type of change that triggered notification |
| Message | NVARCHAR(MAX) | NOT NULL | Notification message text |
| IsRead | bit | NOT NULL, default=0 | Read/unread status |
| ReadAt | DateTime | NULL | Timestamp when read |
| CreatedAt | DateTime | NOT NULL, default=GETUTCDATE() | UTC timestamp |

**Indexes**:
- `IX_Notifications_OrgId_UserId_Read` - Efficient unread notification queries
- `IX_Notifications_OrgId_CreatedAt` - Time-based queries

---

### 2.3 Project Entity (Updated)

**Extension for Audit Integration**:

```csharp
public class Project : BaseEntity
{
    public string TeamId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string MilestoneStatus { get; set; }        // PLANNING, IN_PROGRESS, COMPLETED, CLOSED, REOPENED
    public string CreatedBy { get; set; }              // For audit
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }             // For audit
}
```

**Milestone Status Values**:
- `PLANNING` - Initial state
- `IN_PROGRESS` - Work underway
- `COMPLETED` - Finished
- `CLOSED` - Archived
- `REOPENED` - Reactivated (future scope change)

---

## 3. API Contracts

### 3.1 Audit Endpoints

#### `POST /api/v1/audit` - Record Audit Event

**Purpose**: Internal endpoint for Project Service to record state changes.

**Request**:
```json
{
  "projectId": "proj-123",
  "eventType": "MILESTONE_UPDATED",
  "entityType": "PROJECT",
  "entityId": "proj-123",
  "actorId": "user-456",
  "actorIpAddress": "192.168.1.100",
  "previousState": "{\"status\":\"PLANNING\"}",
  "newState": "{\"status\":\"IN_PROGRESS\"}"
}
```

**Response (201 Created)**:
```json
{
  "id": "audit-789",
  "projectId": "proj-123",
  "eventType": "MILESTONE_UPDATED",
  "entityType": "PROJECT",
  "entityId": "proj-123",
  "actorId": "user-456",
  "actorIpAddress": "192.168.1.100",
  "previousState": "{\"status\":\"PLANNING\"}",
  "newState": "{\"status\":\"IN_PROGRESS\"}",
  "createdAt": "2026-08-21T10:30:00Z"
}
```

**Validation Rules**:
- `projectId`, `eventType`, `actorId` are required
- `eventType` must be valid (see EventTypes constants)
- User must be authenticated
- Organization context must match token claims

---

#### `GET /api/v1/audit/{projectId}` - Get Audit History

**Purpose**: Query audit entries for a project with optional filtering.

**Query Parameters**:
- `fromDate` (optional): ISO 8601 start date (e.g., `2026-08-01T00:00:00Z`)
- `toDate` (optional): ISO 8601 end date
- `eventType` (optional): Filter by event type (e.g., `PROJECT_CREATED`)

**Example Request**:
```
GET /api/v1/audit/proj-123?fromDate=2026-08-01T00:00:00Z&toDate=2026-08-31T23:59:59Z&eventType=MILESTONE_UPDATED
```

**Response (200 OK)**:
```json
{
  "success": true,
  "data": [
    {
      "id": "audit-789",
      "projectId": "proj-123",
      "eventType": "MILESTONE_UPDATED",
      "entityType": "PROJECT",
      "entityId": "proj-123",
      "actorId": "user-456",
      "actorIpAddress": "192.168.1.100",
      "previousState": "{...}",
      "newState": "{...}",
      "createdAt": "2026-08-21T10:30:00Z"
    }
  ],
  "count": 1,
  "timestamp": "2026-08-21T11:00:00Z"
}
```

**Authorization**: User must have access to the project's organization.

---

### 3.2 Notification Endpoints

#### `GET /api/v1/notifications/unread` - Get Unread Notifications

**Purpose**: Retrieve unread notifications for current user.

**Response (200 OK)**:
```json
{
  "success": true,
  "data": [
    {
      "id": "notif-001",
      "recipientUserId": "user-456",
      "projectId": "proj-123",
      "eventType": "MILESTONE_UPDATED",
      "message": "Project 'Acme Portal' milestone updated to IN_PROGRESS",
      "isRead": false,
      "createdAt": "2026-08-21T10:30:00Z",
      "readAt": null
    }
  ],
  "count": 3,
  "timestamp": "2026-08-21T11:00:00Z"
}
```

---

#### `PATCH /api/v1/notifications/{id}/read` - Mark as Read

**Purpose**: Mark a notification as read.

**Response (200 OK)**:
```json
{
  "success": true,
  "data": {
    "id": "notif-001",
    "recipientUserId": "user-456",
    "projectId": "proj-123",
    "eventType": "MILESTONE_UPDATED",
    "message": "Project 'Acme Portal' milestone updated to IN_PROGRESS",
    "isRead": true,
    "createdAt": "2026-08-21T10:30:00Z",
    "readAt": "2026-08-21T11:00:00Z"
  },
  "timestamp": "2026-08-21T11:00:00Z"
}
```

---

## 4. Integration Points

### 4.1 Project Service → Audit/Notification Service

**When Project Milestone Changes**:

```
Project Service receives request to update milestone status
    ↓
ProjectService.UpdateMilestoneStatusAsync() executes
    ↓
Repository persists updated project
    ↓
Call AuditLogService.RecordEventAsync(
    projectId: proj-123,
    eventType: MILESTONE_UPDATED,
    previousState: old milestone status,
    newState: new milestone status
)
    ↓
Call NotificationService.CreateBatchNotificationsAsync(
    recipientUserIds: [list of team members],
    projectId: proj-123,
    eventType: MILESTONE_UPDATED,
    message: "Project X milestone updated to Y"
)
    ↓
Notifications delivered, audit entry recorded
```

**Inter-Service Contract**:
- Project Service knows how to construct AuditLogDto and pass to Notification Service
- Audit/Notification Service validates organization context independently
- No cross-service database access; only through REST or service-to-service calls
- Both services enforce multi-tenant isolation

---

### 4.2 Data Flow Diagram

```
┌─────────────────────┐
│ API Client Request  │
│ (Authenticated)     │
└──────────┬──────────┘
           │
           ↓
┌──────────────────────────────────────────┐
│ ProjectsController                       │
│ - Extract user context from JWT claims   │
│ - Validate organization context          │
│ - Delegate to ProjectService             │
└──────────┬──────────────────────────────┘
           │
           ↓
┌──────────────────────────────────────────┐
│ ProjectService                           │
│ - Validate input & auth                  │
│ - Call ProjectRepository.UpdateAsync()   │
│ - Trigger audit & notification events    │
└──────────┬──────────────────────────────┘
           │
           ├─────────────────────────────────┐
           │                                 │
           ↓                                 ↓
    ┌─────────────┐              ┌──────────────────────┐
    │ ProjectDb   │              │ NotificationService  │
    │ - Projects  │              │ - CreateBatch...()   │
    │ table       │              │ - AuditLogService    │
    └─────────────┘              └──────────┬───────────┘
                                             │
                                             ├──────────┐
                                             │          │
                                             ↓          ↓
                                      ┌──────────┐  ┌─────────┐
                                      │AuditDb   │  │NotifDb  │
                                      │-AuditLog │  │-Notif   │
                                      │ table    │  │ table   │
                                      └──────────┘  └─────────┘
```

---

## 5. Constraints & Design Decisions

### 5.1 Immutability Enforcement

**Constraint**: Audit logs cannot be modified or deleted after creation.

**Implementation**:
1. **Repository Layer**: No `Update()` or `Delete()` methods on `IAuditLogRepository`
2. **Service Layer**: No business logic to modify audit entries
3. **Database**: No foreign key triggers to cascade deletes
4. **Code Review**: Reviewers verify no backdoor mutations exist

**Rationale**: Ensures compliance with audit standards (SOC 2, GDPR, etc.) and prevents tampering.

---

### 5.2 Multi-Tenant Data Isolation

**Constraint**: Users may only access data from their organization.

**Implementation**:
1. **JWT Claims**: Token includes `organization_id` claim
2. **User Context Filter**: Every request validates org context via `UserContextFilter`
3. **Query Scoping**: All repository queries include `.Where(e => e.OrganizationId == organizationId)`
4. **Service Validation**: Services validate organization match before returning data

**Rationale**: Prevents data leakage in multi-tenant B2B SaaS.

---

### 5.3 Validation & Authorization Rules

| Rule | Enforcement | Level |
|------|-------------|-------|
| Organization context required for all operations | JWT claim validation | Filter/Controller |
| Project ID must belong to user's organization | Repository query scoping | Service/Repository |
| User must be authenticated for all endpoints | `[Authorize]` attribute | Controller |
| Audit entries cannot be modified | Repository interface | Repository |
| Audit history queryable only by org members | Query scoping | Repository |
| IP address capture for audit (future) | Optional dto field | Service |

---

## 6. Event Types & Scenarios

### 6.1 Event Types

```csharp
public static class EventTypes
{
    public const string ProjectCreated = "PROJECT_CREATED";
    public const string MilestoneUpdated = "MILESTONE_UPDATED";
    public const string ProjectDeleted = "PROJECT_DELETED";
    public const string MilestoneReopened = "MILESTONE_REOPENED"; // Future
}
```

### 6.2 Notification Triggers

| Event | Who Gets Notified | Message |
|-------|------------------|----------|
| PROJECT_CREATED | All team members | "Project X created in team Y" |
| MILESTONE_UPDATED | All team members | "Project X milestone changed from A to B" |
| PROJECT_DELETED | All team members | "Project X has been deleted" |
| MILESTONE_REOPENED | All team members | "Project X milestone has been reopened" |

---

## 7. Copilot Assistance Notes

### What Copilot Generated Well
✅ **Data model structures** - Entity definitions with proper attributes  
✅ **Repository boilerplate** - CRUD operation patterns  
✅ **Service layer signatures** - Async method patterns  
✅ **Controller skeleton** - HTTP endpoint structure  
✅ **Logging statements** - Structured logging templates  

### What Required Human Judgment
❌ **Immutability pattern** - Copilot initially generated Update/Delete methods; manual removal needed  
❌ **Multi-tenant scoping** - AI didn't consistently add org context checks; human review added missing validations  
❌ **Security decisions** - IP address capture design needed security review  
❌ **Architecture integration points** - Service-to-service contracts required human design  
❌ **Error handling strategy** - Custom exception usage patterns needed manual definition  

---

## 8. Acceptance Criteria

- [x] Audit logs are immutable (no modify/delete)
- [x] Audit history queryable by project ID with date range and event type filters
- [x] Notifications delivered to all team members on milestone change
- [x] Multi-tenant isolation enforced on all queries
- [x] API responses include proper error messages and status codes
- [x] All public methods documented with XML comments
- [x] Async/await patterns used throughout
- [ ] ≥80% test coverage
- [ ] Scalable to handle high-volume audit events

---

**Document Status**: Ready for Technical Review  
**Next Step**: Code Review (REVIEW.md) and Test Implementation
