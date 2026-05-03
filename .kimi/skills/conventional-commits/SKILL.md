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
- `feat(auth): add OAuth login support`
- `fix(api): fix user query returning null`
- `docs(readme): update installation instructions`
- `refactor(results): update Result types for multi-targeting`
- `test(efcore): add AuditInterceptor tests`
- `chore(deps): bump package versions`

Rules:
- Use lowercase after the colon.
- Scope should match the affected package or area (core, results, rules, efcore, json, etc.).
- Description must be imperative mood ("add" not "added").
