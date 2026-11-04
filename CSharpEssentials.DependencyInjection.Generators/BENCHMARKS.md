# CSharpEssentials.DependencyInjection.Generators: design-time benchmarks

Draft. This page describes how the service registration generator is measured against Injectio 6.3.0. Results are pending the full run.

The generator runs inside the compiler and the IDE, not in the shipped app. The cost measured here is build time and the work the IDE does after a keystroke. Runtime registration and resolve cost are covered in [the package BENCHMARKS.md](../CSharpEssentials.DependencyInjection/BENCHMARKS.md).

---

## What was measured

Class `DependencyInjectionGeneratorComparison` in `benchmarks/CSharpEssentials.Generators.Benchmarks/`. The project is an in-memory web app with 100 services, `Service001` to `Service100`, each with its own `IServiceNNN` interface. The lifetime follows `n % 3`: Scoped, Singleton, Transient. It is the same in both libraries.

Both libraries use the generic explicit-service attribute form. Only the `using` line differs:

```csharp
using CSharpEssentials.DependencyInjection;   // or: using Injectio.Attributes;

[RegisterScoped<IService003>]
public sealed class Service003 : IService003
{
    public int Get() => 3;
}
```

Both READMEs document this form. It gives the same registrations: one descriptor per service, `IServiceNNN` to `ServiceNNN`, with the same lifetime. That makes it the like-for-like form.

| Method | Category | Generators | Like for like? |
|---|---|---|---|
| `EmptyDriver_Cold` | Cold | none | No. Compilation and driver object overhead. A driver with no generators binds nothing, so this is not a floor for generator work |
| `CSharpEssentials_Cold` / `_Incremental` (baseline) | Cold / Incremental | `CSharpEssentials.DependencyInjection.Generators` | Yes |
| `Injectio_Cold` / `_Incremental` | Cold / Incremental | `Injectio.Generators` | Yes |

Parity is checked in `[GlobalSetup]` and with `--self-check`. A failed check stops the run.

- Every row must name exactly the 100 service types in its generated text.
- Every row must register each `IServiceNNN` with the expected lifetime, read from the generated `ServiceDescriptor.{Lifetime}` calls. All 100 pairs must match and no extra pair may appear.
- No generator may throw or report an error, and the output must compile with no errors.
- Cold output on the original project must equal incremental output after the edit, file by file.

## Methodology

- **Host Roslyn.** Every row runs on `Microsoft.CodeAnalysis.CSharp` 5.9.0, the latest stable release on NuGet. SDK 10.0.400 ships a build of the same 5.9 compiler (5.9.0-1). Injectio needs Roslyn 4.14 or later, so the repository's 4.13 pin cannot load it.
- **Loading.** Each generator loads from its own folder into its own load context. Analyzers and code fixes in the packages are not run.
- **Cold.** Each call creates a fresh `CSharpCompilation` from pre-parsed trees and a fresh `CSharpGeneratorDriver`, then runs it. A fresh compilation binds all source symbols again on every call. Referenced-assembly symbols are cached on the shared `MetadataReference` objects, as in the IDE and the compiler server, so metadata loading is not part of the cold cost.
- **Incremental.** Each row warms a driver once on the original compilation. Each call runs that driver on the original compilation with one tree replaced (`ReplaceSyntaxTree`), which is what the IDE does on a keystroke.
- **The edit.** File `Src/Filler0051.cs`: `return x + 51;` becomes `return x + 52;`. It is a method-body change in a file with no attributes, with base-list classes and with no `WithName` call.
- **Step tracking** is off in every measured call.
- **Project shape.** Assembly `Bench.App`, `OutputKind.ConsoleApplication`, nullable enabled, C# latest. CSE also emits `AddAllServices` because the project is an executable. `Files` is 200 or 1000 source files: the 100 feature files, `GlobalUsings.cs` with the Web SDK implicit usings, `Program.cs` and filler files for the rest.
- **References.** One set for every row: the host runtime's framework, the ASP.NET Core shared framework, the CSE runtime DLLs and the competitor attribute DLLs. On net8.0 the CSE runtime DLLs need Microsoft.Extensions.DependencyInjection.Abstractions and Microsoft.Extensions.Logging.Abstractions 9.0.4 from NuGet. Those copies from the output folder replace the older shared framework copies for every row, as SDK conflict resolution does in a CSE app. On the other TFMs the shared framework copies are used.
- **Preprocessor symbols.** The parse options define the symbols the SDK defines for the host TFM in Release. Injectio emits its generic attributes under `#if NET7_0_OR_GREATER`, so without them the generic form does not compile.
- **Options.** One `AnalyzerConfigOptionsProvider` for every row with `build_property.IsTestProject=false` and `build_property.RootNamespace=Bench.App` (set by the SDK for every project). `InjectioName` and `InjectioInternal` stay unset, which is the package default.
- **Cross-TFM rows** run on that TFM's shared framework, so the references differ by TFM. The host Roslyn 5.9.0 also runs its netstandard2.0 build on net8.0 and net9.0 and its net10.0 build on net10.0 and net11.0. Compare rows within one TFM.
- BenchmarkDotNet default job, `[MemoryDiagnoser]`, rows grouped by category, CSE is the baseline in each category.

## Pinned versions

| Package | Version | License |
|---|---|---|
| Microsoft.CodeAnalysis.CSharp (host) | 5.9.0 | MIT |
| Injectio | 6.3.0 (latest stable) | MIT |
| CSharpEssentials.DependencyInjection.Generators | 7.0.0 (project reference) | MIT |

## Semantic differences

Read from the generated text in the self-check:

- CSE registers each service through `ServiceRegistration.Apply(..., RegistrationStrategy.Add)`, which adds the descriptor and logs a duplicate at Debug when a logger is passed. Injectio uses `TryAdd`, so a duplicate is skipped (its default `Duplicate` strategy, `Skip`).
- CSE emits a per-assembly registry with an idempotent `AddBenchAppServices` method, an assembly-level `ServiceModule` marker and, in an executable, an `AddAllServices` aggregate. Self-check: 2 generated files, 46,476 characters.
- Injectio emits its attributes as post-initialization sources, decoration helpers, an idempotent `AddBenchApp(params string[]? tags)` method and, because the shared framework includes `Microsoft.Extensions.Hosting.Abstractions`, an `IHostApplicationBuilder` overload. Self-check: 4 generated files, 83,228 characters.
- CSE's default rule without a type argument registers `I{TypeName}`. Injectio's default without a service type is `SelfWithProxyFactory`: the class and all its implemented interfaces. Neither default is used here, because both rows name the service type.

## Feature matrix

| | CSE | Injectio 6.3.0 |
|---|---|---|
| Attribute form used here | Generic, explicit service type | Generic, explicit service type |
| Default service type | `I{TypeName}`, otherwise the class itself; `As` = `Self`, `SelfWithInterfaces` or `ImplementedInterfaces` | `SelfWithProxyFactory` (the class and all its interfaces, with the interfaces resolving one shared instance; `SelfWithInterfaces` for open generics); also `Self`, `ImplementedInterfaces`, `SelfWithInterfaces` |
| Duplicate handling default | `Add` (logged at Debug); also `TryAdd`, `TryAddEnumerable`, `Replace`, `Throw` | `Skip` (`TryAdd`); also `Replace`, `Append` |
| Keyed services | Yes (`Key`) | Yes (`ServiceKey`) |
| Decorators | Yes (`[Decorates]`) | Yes (`[RegisterDecorator]`) |
| Open generics | Yes (`typeof(IRepository<>)`); open-generic decorators only at runtime | Yes |
| Factories | No factory attribute | Yes (`Factory`) |
| Registration tags | No | Yes |
| Aggregate across assemblies | `AddAllServices` in an executable | No |
| Attributes | In the runtime DLL | Post-initialization embedded sources |
| Minimum Roslyn | 4.8 | 4.14 |
| License | MIT | MIT |

## Incremental cache behaviour

From `--self-check` on net10.0 with `Files=200`, after the filler edit. Lower-level `ForAttributeWithMetadataName` steps, the Roslyn input steps (`AnalyzerConfigOptions`, `CompilationOptions`, `ParseOptions`) and single-value steps that stay Cached are left out. The `Files=1000` counts are the same.

| Row | Step | Reasons |
|---|---|---|
| CSE | `Compilation` | Modified 7 |
| CSE | `Host` | Unchanged 1 |
| CSE | `ServiceTypes` | Cached 100 |
| CSE | `CollectedServices`, `IsTestProject` | Cached 1 each |
| CSE | source outputs | Cached 2 |
| Injectio | `Compilation` | Modified 8 |
| Injectio | `AssemblyName`, `HasHostApplicationBuilder` | Unchanged 1 each |
| Injectio | `ScopedRegistrationsT1`, `SingletonRegistrationsT1`, `TransientRegistrationsT1` | Unchanged 33, 34, 33 |
| Injectio | `AllServiceRegistrations`, `AllDecoratorRegistrations`, `Generation` | Cached 1 each |
| Injectio | source outputs | Cached 1 |

Every source output stays cached after the unrelated edit. The self-check fails if a CSE output does not.

## Results

Pending the full default-job run. No numbers from quick runs are published.

## Known CSE costs to watch in the full run

From reading the source. Not fixed here.

- `CSharpEssentials.DependencyInjection.Generators/ServiceRegistrationGenerator.cs:66-68`: the Host step is a `CompilationProvider.Select` that runs on every edit and walks the references. Its output is equatable, so later steps stay cached.
- `ServiceRegistrationGenerator.cs:44-60`: one `ForAttributeWithMetadataName` provider per attribute name (eight), combined pairwise into one collection.

## Caveats

- Design time only. Nothing here affects the shipped app.
- Code fixes and analyzers in the packages are not measured.
- The input is generated, not a real app.

## How to reproduce

```bash
./benchmarks/run-benchmarks.sh --project generators
dotnet run -c Release --project benchmarks/CSharpEssentials.Generators.Benchmarks -f net10.0 -- --self-check
```
