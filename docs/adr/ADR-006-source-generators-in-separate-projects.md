# ADR-006: Source Generators Ship from Separate netstandard2.0 Projects Inside the Runtime Package

- **Status:** Accepted
- **Date:** 2026-10-06
- **Issues:** #48 (epic), #49 (this ADR), #50 (implementation)
- **Supersedes:** nothing. ADR-004 (Enums generator merged into `CSharpEssentials.Enums`, recorded in the maintainers' roadmap) was in force for `CSharpEssentials.Enums` when this ADR was written. [ADR-007](ADR-007-enum-conventions.md) moved the Enums generator to this layout in 5.0 (`CSharpEssentials.Enums.Generators`).

---

## Context

Version 4.1 adds two generator-backed packages: `CSharpEssentials.Endpoints` and `CSharpEssentials.DependencyInjection`. Both need a Roslyn source generator and a diagnostic analyzer delivered through the same NuGet package as their runtime API.

The only existing mechanism at the time was ADR-004 (`CSharpEssentials.Enums`): one project multi-targets `net11.0;net10.0;net9.0;netstandard2.1;netstandard2.0`. The generator sources compile only in the `netstandard2.0` TFM, and that TFM's output dll is packed a second time under `analyzers/dotnet/cs`. The same dll is therefore both the `lib/netstandard2.0` runtime asset and the analyzer.

That mechanism does not fit the new packages:

1. **Endpoints cannot produce a netstandard2.0 build.** The runtime targets `net11.0;net10.0;net9.0;net8.0` with `<FrameworkReference Include="Microsoft.AspNetCore.App" />`. A `FrameworkReference` is unavailable on `netstandard2.0`, and every runtime type (`IEndpointRouteBuilder`, `RouteGroupBuilder`, static abstract interface members) is unavailable there. Adding a `netstandard2.0` TFM means excluding every runtime file by condition, which amounts to two projects in one `.csproj` without the isolation of two projects.
2. **A DI runtime dll would be loaded as an analyzer.** `CSharpEssentials.DependencyInjection` targets `netstandard2.1` and depends on `Microsoft.Extensions.DependencyInjection.Abstractions` and `Microsoft.Extensions.Logging.Abstractions`. If its `netstandard2.x` runtime dll is packed under `analyzers/`, the compiler loads it as an analyzer assembly. The compiler cannot resolve those dependencies (or a `netstandard2.1` dll on .NET Framework-hosted compilers in Visual Studio), which produces analyzer load failures (`CS8034`/`AD0001`-class warnings). Every compilation would also pay to load runtime code that has no compile-time role.
3. **Roslyn dependency leaks into runtime assets.** Under ADR-004, the `lib/netstandard2.0` asset of Enums contains generator types that reference `Microsoft.CodeAnalysis`. This is harmless for Enums, but should not be repeated.
4. **Compiler version coupling.** `Directory.Packages.props` pins `Microsoft.CodeAnalysis.CSharp` to `[4.13.0,)`. A generator compiled against a Roslyn version newer than the consuming compiler fails to load with `CS9057`. Endpoints supports `net8.0`, so the generator must load on the .NET 8 SDK (8.0.1xx, Roslyn 4.8).

## Decision

### Project layout

Each generator-backed package consists of two projects:

| Project | TFM | Packable | Contents |
|---|---|---|---|
| `CSharpEssentials.<Area>` | runtime TFMs of the package | yes | public runtime API and attributes |
| `CSharpEssentials.<Area>.Generators` | `netstandard2.0` | `IsPackable=false` | `IIncrementalGenerator` and `DiagnosticAnalyzer` types |

- The runtime project references the generator project with `ReferenceOutputAssembly="false"` and packs the generator dll into its own nupkg at `analyzers/dotnet/cs`. The generator project never produces a package of its own.
- In-repo consumers (tests, examples) reference the generator project directly with `OutputItemType="Analyzer" ReferenceOutputAssembly="false"`, in addition to the runtime project reference.
- Shared generator settings live in `build/Generators.props`: `netstandard2.0`, `IsPackable=false`, `EnforceExtendedAnalyzerRules=true`, `IsRoslynComponent=true`, the Roslyn pin, and release-tracking `AdditionalFiles`. Pack wiring lives in `build/PackGenerator.targets` (or inline in the runtime `.csproj` if one line suffices).
- The generator assembly contains no runtime types. It matches attributes and interfaces by metadata name (string), so it does not reference the runtime project.

### Roslyn version

- Generator projects reference `Microsoft.CodeAnalysis.CSharp` with `VersionOverride="4.8.0"`. The override applies to generator projects only; the central version (`[4.13.0,)`) is unchanged for every other project.
- Minimum supported SDK for consumers of generator-backed packages: **8.0.1xx** (Roslyn 4.8). Generators must not use APIs newer than Roslyn 4.8 (`ForAttributeWithMetadataName`, `CreateSyntaxProvider`, and `RegisterSourceOutput` are all available). This prevents `CS9057`.
- An SDK 8.0.1xx load check cannot run on the current local machine (9/10/11 SDKs only). The pin guarantees compatibility; CI may add an explicit 8.0.1xx job.

### Diagnostics

- Diagnostics are reported **only** by a separate `DiagnosticAnalyzer` in the generator assembly. Generators never call `ReportDiagnostic`. Diagnostic locations and symbols in the generator pipeline would break incremental caching (locations are not value-equatable across edits), and an analyzer reports on the IDE's analysis schedule instead of the generation schedule.
- Generator pipeline models are value-equatable records with equatable collections. No `ISymbol`, `SyntaxNode`, `Location` or `Compilation` crosses a pipeline step.
- When the analyzer reports an **error**, the generator skips the offending type so the error comes from the analyzer and not as a secondary compile error in generated code. The two share the same model-building code.

### Release tracking

- Every analyzer project has `AnalyzerReleases.Shipped.md` and `AnalyzerReleases.Unshipped.md` as `AdditionalFiles` and references `Microsoft.CodeAnalysis.Analyzers`. RS2008 (and the rest of the release-tracking rules) must pass under `TreatWarningsAsErrors` without `#pragma` or `[SuppressMessage]`.
- New rules go to `Unshipped.md`. On release, they move to `Shipped.md` under the release version.

### Diagnostic ID blocks

| Block | Owner | Notes |
|---|---|---|
| `CSE0001` to `CSE0999` | Enums and future cross-cutting rules | `CSE0001` retired in 5.0 and reserved, `CSE0002` to `CSE0016` used by ADR-007 (`CSE0011` is unused) |
| `CSE1001` to `CSE1999` | `CSharpEssentials.Endpoints` | see [CSharpEssentials.Endpoints-DESIGN.md](../design/CSharpEssentials.Endpoints-DESIGN.md) |
| `CSE2001` to `CSE2999` | `CSharpEssentials.DependencyInjection` | see [CSharpEssentials.DependencyInjection-DESIGN.md](../design/CSharpEssentials.DependencyInjection-DESIGN.md) |
| `CSE3001` to `CSE3999` | `CSharpEssentials.EntityFrameworkCore` | `CSE3001` reports parameterless `IgnoreQueryFilters()` when EF Core 10 named filters are available; see the package README |

IDs are never reused or renumbered, including IDs reserved for deferred (P3) rules.

### Severity policy

- **Error:** only when the generated code would not compile, or would certainly fail at runtime (for example, an inaccessible type, a group nesting cycle, or a decorator without a usable constructor).
- **Warning:** the code compiles and runs, but almost certainly not as intended.
- **Info:** a deliberate skip or fallback that the user should be able to see (for example, an abstract endpoint that is not mapped, or a default service resolution that fell back to self-registration).

Users may change severities through `.editorconfig`. The defaults follow this policy.

### Test harness

- Generator tests use `CSharpGeneratorDriver` with snapshot verification through **Verify.SourceGenerators** and **Verify.Xunit** (the test project uses xUnit v2). These are test-only packages, approved by the owner on 2026-10-05. Versions go into `Directory.Packages.props`, referenced only from `CSharpEssentials.Tests`.
- Every generator has an incremental-caching test: a second run over an unchanged (or irrelevantly changed) compilation must report `Cached`/`Unchanged` for its tracked steps (`IncrementalStepRunReason`).
- Every diagnostic ID has a positive and a negative test.

## Consequences

**Positive**
- Runtime TFMs and generator TFMs are independent. Endpoints stays `net8.0+` with `FrameworkReference`, and DI stays `netstandard2.1` + `net8.0+`.
- The compiler loads only the generator dll, which has no runtime dependencies.
- Consumers still add one `PackageReference` and get both the runtime API and the generator.
- Consumers on SDK 8.0.1xx can load the generator (no `CS9057`).
- Diagnostics do not affect generator caching.

**Negative**
- Each generator-backed package needs two projects and pack wiring. `build/Generators.props` mitigates this.
- In-repo consumers need an extra `ProjectReference` to the generator project, because analyzers do not flow through `ProjectReference` the way they flow through packages.
- Generators are limited to the Roslyn 4.8 API surface.
- The generator matches runtime types by metadata-name strings. Renaming a runtime attribute or interface requires updating the generator. Snapshot tests catch the mismatch.

**Neutral**
- `CSharpEssentials.Enums` kept the ADR-004 mechanism in 4.1. Moving it to this layout was out of scope for 4.1; ADR-007 did it in 5.0.
- The publish script packs only packable projects, so `*.Generators` projects (`IsPackable=false`) are skipped automatically.

### Implementation

- A generator project imports the shared settings and adds nothing else to its `.csproj` except sources and its two release-tracking files:

  ```xml
  <Project Sdk="Microsoft.NET.Sdk">
    <Import Project="..\build\Generators.props" />
  </Project>
  ```

  `build/Generators.props` sets `netstandard2.0`, `IsPackable=false`, `IncludeSymbols=false`, `IsRoslynComponent=true`, `EnforceExtendedAnalyzerRules=true` and `DebugType=embedded` (the pdb travels inside the analyzer dll, so no separate symbol file is needed in `analyzers/dotnet/cs`). It references `Microsoft.CodeAnalysis.CSharp` with `VersionOverride="4.8.0"` and `Microsoft.CodeAnalysis.Analyzers`, both `PrivateAssets="all"`, and adds `AnalyzerReleases.Shipped.md`/`AnalyzerReleases.Unshipped.md` as `AdditionalFiles`. SourceLink and SonarAnalyzer still come from `Directory.Build.props`; generator code satisfies Sonar instead of suppressing it.
- Roslyn 4.8 brings `System.Collections.Immutable` 7.0, which has no collection builder for `ImmutableArray<T>`. Generator code uses `ImmutableArray.Create(...)` instead of collection expressions for immutable arrays (`CS9210` otherwise).
- `build-and-publish-nugets.sh` skips directories named `*.Generators`, because it detects non-packable projects by text and the `IsPackable=false` setting lives in the imported props file.
- Pack wiring (`build/PackGenerator.targets`) landed with the first runtime package that owns a generator (Endpoints, #52), where the nupkg layout is verified by a test. Enums, Endpoints and DependencyInjection use it; each runtime project also packs its `.CodeFixes` project the same way.
- In-repo consumers reference the generator project directly:

  ```xml
  <ProjectReference Include="..\CSharpEssentials.<Area>.Generators\CSharpEssentials.<Area>.Generators.csproj"
                    OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
  ```

- Test harness: `CSharpEssentials.Tests/Generators/` (`GeneratorHarness`, `AnalyzerHarness`, `IncrementalCaching`, `AnalyzerAssembly`). Snapshots live in a `Snapshots/` folder next to the test file, `*.verified.*` files are committed and `*.received.*` files are ignored. Verify disables diff tools on build servers (it detects `GITHUB_ACTION`), so CI runs the snapshot tests without extra settings.

## Alternatives Considered

| Alternative | Rejected because |
|---|---|
| ADR-004 single-project multi-TFM (add `netstandard2.0` to the runtime project) | Impossible for Endpoints (`FrameworkReference`, `net8.0+` APIs). For DI, the runtime dll and its dependencies would load as an analyzer. |
| Separate packable `CSharpEssentials.<Area>.Generators` NuGet package | Two packages per feature, version skew between runtime and generator, and a worse install experience. Opt-in is already provided by the interface or attribute markers. |
| Reference the newest Roslyn (`[4.13.0,)`) in generators | `CS9057` on SDK 8.0.1xx, while Endpoints supports `net8.0`. |
| Report diagnostics from the generator | Breaks incremental caching and couples diagnostics to generation. The Roslyn team recommends a separate analyzer. |
| Reflection-only registration (no generator) | Startup reflection is not trim/AOT safe. Reflection remains available only as an explicit `[RequiresUnreferencedCode]` fallback. |
| Hand-written snapshot comparison instead of Verify | Reimplements diffing and approval workflow. Verify packages are test-only and owner-approved. |
