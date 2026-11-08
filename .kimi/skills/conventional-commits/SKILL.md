---
name: conventional-commits
description: Git commit message conventions for CSharpEssentials. Use when writing or reviewing commit messages.
---

## Git commit conventions

Use the Conventional Commits format: `type(scope): description`

Allowed types: `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `chore`, `ci`, `build`, `revert`. release-please reads them: `feat` gives a minor bump, `fix` and `perf` a patch, and `feat!`, `fix!` or a `BREAKING CHANGE:` footer a major.

The scope matches the affected package or area: `core`, `results`, `errors`, `rules`, `efcore`, `json`, `http`, `maybe`, `any`, `these`, `entity`, `aspnetcore`, `openapi`, `swashbuckle`, `endpoints`, `di`, `mediator`, `validation`, `resilience`, `time`, `enums`, `clone`, `gcpsecretmanager`, `idempotency`, `logging`, `bench`, `aot`, `blog`, `examples`, `deps`.

Examples:
- `feat(results): add MapError overload for async chains`
- `fix(efcore): fix AuditInterceptor not firing on SaveChangesAsync`
- `refactor(any): update Any<T1,T2> implicit operators for netstandard2.1`
- `test(core): add edge cases for StringExtensions.ToSnakeCase`
- `chore(deps): bump SonarAnalyzer.CSharp to 10.x`

Rules:
- Use lowercase after the colon.
- Write the description in the imperative mood ("add", not "added").
