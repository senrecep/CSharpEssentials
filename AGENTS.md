# CSharpEssentials — Agent Instructions

## What

CSharpEssentials is a modular .NET NuGet ecosystem (23 packages) that bridges OOP and Functional Programming in C#. Core patterns: Result/Maybe monads, Discriminated Unions (Any<T1,T2,...>), composable Rules engine, DDD base classes (EntityBase), EF Core interceptors/pagination, ASP.NET Core utilities, source-generated Minimal API endpoints (CSharpEssentials.Endpoints) and attribute-based DI registration (CSharpEssentials.DependencyInjection). Multi-targets: .NET 11/10/9 (some packages also net8.0), netstandard2.1 (a few also netstandard2.0). CSharpEssentials.EntityFrameworkCore targets net10.0/net9.0/net8.0 only (no net11.0), each pinned to its EF Core major. Current version: 4.0.0.

## Why

- **Nullable + TreatWarningsAsErrors**: Prevents null reference bugs at compile time; every package must be null-safe by design.
- **Modular packages**: Users take only what they need; CSharpEssentials meta-package bundles core functional modules.
- **SonarAnalyzer.CSharp**: Static analysis in every build via Directory.Build.props — consistent quality across all packages without per-project config.
- **Central Package Management (Directory.Packages.props)**: Single version source-of-truth; prevents version drift across packages.
- **No abstract layers for the sake of it**: Every abstraction (IDateTimeProvider, IDomainEventPublisher) exists to enable testability or infrastructure-swapping, not ceremony.

## How

- Build: `dotnet build`
- Test: `dotnet test`
- Pack: `dotnet pack`
- Publish: `./build-and-publish-nugets.sh`
- First-time setup: `git config core.hooksPath .githooks` (activates pre-commit badge validation)
- Naming: PascalCase types, camelCase locals, `_camelCase` private fields
- File layout: One public type per file, filename matches type name
- Tests live in `CSharpEssentials.Tests/`
- Endpoints and DependencyInjection keep their source generators/analyzers in `<Package>.Generators/` and code fixes in `<Package>.CodeFixes/` (netstandard2.0, not packable); both are bundled into the package under `analyzers/dotnet/cs`. The Enums generator is packed from `CSharpEssentials.Enums` itself

## Don't

- Don't use `dynamic` type — defeats the purpose of the type-safe libraries.
- Don't suppress warnings with `#pragma warning disable` — fix the root cause.
- Don't add `// TODO` to committed code — either implement it or track it as an issue.
- Don't add new packages to `Directory.Packages.props` without checking existing entries.
- Don't break multi-targeting — test against all declared target frameworks.
- Don't add docstrings or comments unless explicitly asked.
- Don't create placeholder/stub implementations.

## Boundaries

- **Always**: Write tests, follow nullable annotations, use conventional commits, run build before committing.
- **Ask first**: Adding new NuGet packages, changing shared abstractions (interfaces in .Core/.Entity), bumping major version, removing public API.
- **Never**: Commit secrets, edit `.snupkg`/`.nupkg` artifacts, push directly to main, suppress TreatWarningsAsErrors.
