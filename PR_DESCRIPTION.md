# Pull Request Description Template

## Title
<short, conventional-commit style title>

## Summary
One-sentence summary of changes.

## Background / Motivation
Why this change is needed.

## Changes
- Bullet-list of changes (files, modules, behavior)

## Testing
- Unit tests added/updated
- Integration tests added/updated
- Manual verification steps

## AI Assistance Disclosure
This PR contains content generated or scaffolded with AI assistance. Items to note:
- AI was used to draft tests, documentation, or boilerplate only.
- All AI-generated code and text were reviewed by a human engineer.
- Security-critical logic, multi-tenant checks, and auth were reviewed and validated manually.

## Peer Review Simulation (Suggested checklist)
- [ ] Architecture & design rationale makes sense
- [ ] Multi-tenant scoping validated (organizationId present on queries)
- [ ] No secrets or credentials in code
- [ ] Exception handling uses project custom exceptions
- [ ] Logging is structured and omits sensitive data
- [ ] Unit & integration tests cover key scenarios
- [ ] Documentation updated as needed

## Rollout / Backout
- Steps to deploy
- Backout plan if regressions occur

## Notes
Link to relevant issue(s) or RFCs.
