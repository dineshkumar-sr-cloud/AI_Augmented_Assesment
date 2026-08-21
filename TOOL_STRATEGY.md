# TOOL_STRATEGY

This document records how developer tooling (including AI-assisted tools) were used, common scenarios, and limitations.

## Usage Log (high level)
- 2026-08-21: Generated xUnit tests for ProjectService using templated prompts; human review applied.
- 2026-08-21: Drafted SPEC.md and ARCHITECTURE.md with AI-assisted outlines; human edited for correctness.

## Typical Scenarios
- Generating unit test scaffolding (mocks, Arrange/Act/Assert)
- Drafting service-level documentation (specs, architecture summaries)
- Producing conventional commit messages and PR templates

## Limitations & Known Issues
- AI-generated code may omit tenant-scoping; always verify organizationId in queries.
- Do not rely on generated secrets or credentials — always source from config/secret store.
- Generated SQL or LINQ may be suboptimal — review for indexes and projections.
- Generated tests may require additional setup (DI, DbContext) to run in CI.

## Recommendations
- Treat AI outputs as a starting point; require one human reviewer for security and architecture-sensitive items.
- Run static analysis and tests before merging any AI-generated code.
