# Notification & Audit Service - Technical Specification

Overview

The Notification & Audit Service (NAS) centralizes event ingestion for audit trails and user notifications. It accepts events emitted by other services (e.g., ProjectService) and persists audit records and notification items. NAS is designed for multi-tenant B2B SaaS workloads with reliable ingestion, idempotency, and horizontal scalability.

Integration contract

- Endpoint (preferred async): POST /api/v1/events
- Payload (JSON):
  {
    "organizationId": "string",
    "projectId": "string",
    "action": "string", // e.g., CREATED, UPDATED, DELETED, MILESTONE_UPDATED
    "actor": "string", // user id
    "payload": { /* free-form JSON snapshot or metadata */ },
    "timestamp": "ISO-8601"
  }

Delivery & semantics

- Preferred delivery is via a durable message bus (Azure Service Bus/Kafka/RabbitMQ). If services POST over HTTP, receivers must accept an idempotency-key header and the JSON body.
- Idempotency: callers should set an Idempotency-Key header where available; NAS must persist the request id and ignore duplicates.
- Acknowledgement: HTTP 2xx indicates accepted; for message bus, the message should be acknowledged only after persistence.
- Retries: NAS supports at-least-once delivery semantics; consumers must handle duplicate events.

API endpoints

- POST /api/v1/events — ingest an event (internal use)
- GET /api/v1/audit/{projectId}?from=&to=&type= — read audit history (tenant-scoped)
- GET /api/v1/notifications/{userId}?unreadOnly=true — read notifications
- PATCH /api/v1/notifications/{id}/read — mark notification read

Security

- All endpoints require mutual TLS or JWT bearer tokens issued by the platform identity provider.
- Required claim: organization_id. Requests lacking this are rejected (403).
- Input validation: payload size limits and schema validation for known action types.

Storage

- Audit store: immutable append-only table with organizationId, eventType, entityId, previousState, newState, actor, timestamp. Consider partitioning by organizationId and time.
- Notifications store: read-optimized table with userId, isRead, deliveredAt, channel metadata.
- Retention and archival policies configurable per-tenant.

Operational considerations

- Backpressure: accept events to a durable queue to avoid blocking callers.
- Monitoring: expose metrics for ingest rate, processing latency, error rate, duplicate rate.
- Error handling: poison queue and DLQ processing for failed messages; surface alerts for backlogged DLQ items.

Schema example (SQL)

CREATE TABLE AuditLog (
  Id varchar(50) PRIMARY KEY,
  OrganizationId varchar(50) NOT NULL,
  ProjectId varchar(50),
  EventType varchar(100) NOT NULL,
  ActorId varchar(50),
  PreviousState nvarchar(max),
  NewState nvarchar(max),
  Payload nvarchar(max),
  CreatedAt datetimeoffset NOT NULL
);

Notes

- Design favors eventual consistency between ProjectService and downstream systems (notifications, search indexes).
- For strict consistency needs (rare), use synchronous write-through patterns with caution due to increased latency and coupling.
