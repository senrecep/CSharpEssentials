---
paths: ["**/*.cs", "**/*.csproj", "Directory.*.props"]
---

## Project Guardrails

### Code Quality
- No `dynamic` usage.
- No `#pragma warning disable` — fix the root cause instead.
- No `// TODO` comments in committed code — track as GitHub issues.
- All public APIs have explicit nullable annotations (`string?` vs `string`).
- New concrete classes are `sealed` unless designed for inheritance.

### Multi-Targeting
- Code must compile under all declared target frameworks of its project (`net11.0`, `net10.0`, `net9.0`, `net8.0`, `netstandard2.1`, `netstandard2.0`, depending on the package).
- Use conditional compilation for framework-specific APIs:
  ```csharp
  #if NET9_0_OR_GREATER
      // Use newer API
  #else
      // Fallback for netstandard
  #endif
  ```
- Never use `net9.0`-only APIs in code that targets `netstandard2.0` without a `#if` guard.

### Project Files
- No `Version=` attributes in `.csproj` files — use `Directory.Packages.props`.
- No new NuGet packages without checking existing entries in `Directory.Packages.props`.
- Test/example projects must have `<IsPackable>false</IsPackable>`.

### Before Committing
- `dotnet build` — zero errors, zero warnings.
- `dotnet test` — all tests pass.
- Both success and failure paths tested for Result/Maybe types.
