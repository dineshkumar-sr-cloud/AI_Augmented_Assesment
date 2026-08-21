ProjectService implements project business logic and emits audit/notification events to a centralized Notification & Audit Service via a lightweight JSON contract: {organizationId, projectId, action, actor, payload, timestamp}.
Integration contract: events delivered asynchronously (durable message bus preferred) or via idempotent HTTP POST; consumers must accept the JSON shape and return 2xx on success.
Layered flow: API/controller → Service layer (ProjectService) → Repository (EF Core) → DB; on state changes the service emits async events to the Notification & Audit Service.
Notification & Audit Service ingests events, persists audit records and notification objects, and handles fan-out to channels (email/webhook) asynchronously.
Data flow example: inbound HTTP request → auth/tenant check → ProjectService mutation → repository persist → emit event → audit persists + notifications enqueued.
Multi-tenant suitability: tenant isolation enforced by passing and validating organizationId at API, service, and repository boundaries; data partitioning and metadata prevent cross-tenant access.
Scalability & resilience: async eventing decouples services for independent scaling and retries; read models and separate stores can be scaled for heavy query loads.
Security & compliance: events include actor, timestamp, and tenant metadata to support non-repudiable audit trails and retention policies.
Key design decisions: choose async messaging for decoupling and throughput (trade-off: eventual consistency); use EF Core for transactional simplicity (trade-off: scale/query-optimized stores later).
Operational trade-offs considered: per-tenant DBs vs shared DB with logical isolation, synchronous simplicity vs async robustness, and message bus cost vs reliability.
