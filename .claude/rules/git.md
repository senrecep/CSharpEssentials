---
paths: [".git/**", "**/*.md", "CONTRIBUTING.md"]
---

## Git Commit Conventions

Use Conventional Commits format: `type(scope): description`

Allowed types: `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `chore`, `ci`, `build`, `revert`

Scope should match the affected package or area: `core`, `results`, `rules`, `efcore`, `json`, `maybe`, `any`, `entity`, `aspnetcore`, `time`, `enums`, `clone`, `gcpsecretmanager`, `logging`, `deps`

Examples:
- `feat(results): add MapError overload for async chains`
- `fix(efcore): fix AuditInterceptor not firing on SaveChangesAsync`
- `refactor(any): update Any<T1,T2> implicit operators for netstandard2.0`
- `test(core): add edge cases for StringExtensions.ToSnakeCase`
- `chore(deps): bump SonarAnalyzer.CSharp to 10.x`

Rules:
- Use lowercase after the colon.
- Description must use imperative mood ("add" not "added").
