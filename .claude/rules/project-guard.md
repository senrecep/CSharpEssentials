---
paths: ["**/*.cs", "**/*.csproj", "Directory.*.props"]
---

## Project guardrails

Before committing, verify the following.

### Code quality
- No `dynamic` usage.
- No `#pragma warning disable`. Fix the root cause instead.
- No `// TODO` comments in committed code. Track them as GitHub issues.
- All public APIs have explicit nullable annotations (`string?` vs `string`).
- New concrete classes are `sealed` unless designed for inheritance.

### Multi-targeting
- Code compiles under all declared target frameworks of its project (`net11.0`, `net10.0`, `net9.0`, `net8.0`, `netstandard2.1`, `netstandard2.0`, depending on the package).
- Use conditional compilation for framework-specific APIs:
  ```csharp
  #if NET9_0_OR_GREATER
      // newer API
  #else
      // fallback for net8.0 and netstandard
  #endif
  ```
  Other symbols in use: `NETSTANDARD`, `NETSTANDARD2_0`, and `CSHARP_ESSENTIALS_ENABLE_UNIONS` (defined on net11.0).
- Never use an API newer than a project's lowest target without a `#if` guard.

### Project files
- No `Version=` attributes in `.csproj` files. Use `Directory.Packages.props`.
- No new NuGet packages without checking existing entries in `Directory.Packages.props`.
- Test and example projects have `<IsPackable>false</IsPackable>`.

### Build and tests
- `dotnet build`: zero errors, zero warnings.
- `dotnet test`: all tests pass.
- Both success and failure paths are tested for Result/Maybe types.
