---
name: project-guard
description: CSharpEssentials project guardrails. Use before committing or when reviewing changes to catch common violations.
---

## Project Guardrails

Before committing, verify the following:

### Code Quality
- No `dynamic` usage unless explicitly justified.
- No `#pragma warning disable` — fix the root cause instead.
- No `// TODO` comments in committed code — track as GitHub issues.
- All public APIs have explicit nullable annotations (`string?` vs `string`).
- New concrete classes are `sealed` unless designed for inheritance.

### Multi-Targeting
- Code compiles under all declared target frameworks (`net9.0`, `netstandard2.1`, `netstandard2.0` where applicable).
- Conditional compilation (`#if NET9_0_OR_GREATER`, `#if NETSTANDARD`) is used for framework-specific APIs.

### Project Files
- No `Version=` attributes in `.csproj` files — use `Directory.Packages.props`.
- No new NuGet packages added without checking existing entries in `Directory.Packages.props`.

### Tests
- All tests pass: `dotnet test`
- Tests cover both success and failure paths for Result/Maybe types.
