# PROMPTS

This document records prompts used with AI assistance for code generation, documentation, and tests, and includes a section for Post-Generation Corrections.

## Example prompts

- "Generate xUnit tests for ProjectService covering Create, Update, Get, Delete scenarios using Moq." 
- "Draft a technical specification for a Notification & Audit Service that ingests JSON events and persists audit logs." 

## Post-Generation Corrections

1. Verify tenant-scoping: ensure every generated data access call includes organizationId filters.
2. Replace placeholder secrets and credentials with configuration values — never hardcode.
3. Validate generated exception handling: confirm custom exceptions (ValidationException, UnauthorizedException, NotFoundException) are used appropriately.
4. Sanitize and review generated SQL or EF queries for injection risk and performance (indexes, projections).
5. Confirm generated unit tests mock dependencies and do not interact with real external resources.
6. Run static analysis and unit tests; fix failures before merge.

## How to use

- Use prompts as templates. Always review and adapt outputs for security, performance, and architecture guidelines before committing.
