---
name: conventional-commits
description: Git commit message conventions for CSharpEssentials. Use when writing or reviewing commit messages.
---

## Git Commit Conventions

Use Conventional Commits format:

```
type(scope): description
```

Allowed types: `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `chore`, `ci`, `build`, `revert`

Examples:
- `feat(results): add MapError overload for async chains`
- `fix(efcore): fix AuditInterceptor not firing on SaveChangesAsync`
- `docs(readme): update installation instructions`
- `refactor(results): update Result types for multi-targeting`
- `test(efcore): add AuditInterceptor tests`
- `chore(deps): bump package versions`

Rules:
- Use lowercase after the colon.
- Scope should match the affected package or area (core, results, errors, rules, efcore, json, http, maybe, any, these, entity, aspnetcore, openapi, swashbuckle, endpoints, di, mediator, validation, resilience, time, enums, clone, gcpsecretmanager, logging, examples, deps).
- Description must be imperative mood ("add" not "added").
