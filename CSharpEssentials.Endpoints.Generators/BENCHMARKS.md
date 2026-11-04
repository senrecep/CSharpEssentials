# CSharpEssentials.Endpoints.Generators: design-time benchmarks

Draft. This page describes how the endpoints source generator is measured against Immediate.Apis 6.4.0. Results are pending the full run.

The generator runs inside the compiler and the IDE, not in the shipped app. The cost measured here is build time and the work the IDE does after a keystroke. It has no effect on request handling.

---

## What was measured

Class `EndpointsGeneratorComparison` in `benchmarks/CSharpEssentials.Generators.Benchmarks/`. The project is an in-memory web app with 50 endpoints. Each library gets its documented idiom for the same 50 endpoints: same names (`Endpoint001` to `Endpoint050`), same route (`/endpointNNN/{id:int}`) and the same result (`id`).

CSE input:

```csharp
public sealed class Endpoint007 : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app.MapGet("/endpoint007/{id:int}", static (int id) => TypedResults.Ok(id));
}
```

Immediate input:

```csharp
[Handler]
[MapGet("/endpoint007/{id:int}")]
public static partial class Endpoint007
{
    public sealed record Query(int Id);

    private static ValueTask<int> HandleAsync(Query query, CancellationToken token) => ValueTask.FromResult(query.Id);
}
```

The two inputs are different files, because each library needs its own annotation. The filler files, the references, the options and the edit are the same. Label: same project, each library's documented idiom.

| Method | Category | Generators | Like for like? |
|---|---|---|---|
| `EmptyDriver_Cold` | Cold | none | No. Compilation and driver object overhead. A driver with no generators binds nothing, so this is not a floor for generator work |
| `CSharpEssentials_Cold` / `_Incremental` (baseline) | Cold / Incremental | `CSharpEssentials.Endpoints.Generators` | Yes |
| `ImmediateApis_Cold` / `_Incremental` | Cold / Incremental | `Immediate.Apis.Generators` only | Yes. Head to head: endpoint discovery and mapping code |
| `ImmediateApisWithHandlers_Cold` / `_Incremental` | Cold / Incremental | `Immediate.Apis.Generators` and `Immediate.Handlers.Generators` | No. The documented setup. Immediate.Handlers also emits a handler class and DI registration per endpoint |

Parity is checked in `[GlobalSetup]` and with `--self-check`. A failed check stops the run.

- Every row must name exactly the 50 endpoint types in its generated text.
- No generator may throw or report an error.
- The CSE and ImmediateApisWithHandlers outputs must compile with no errors. ImmediateApis alone cannot compile without the Handlers output, so its compile check is skipped.
- Cold output on the original project must equal incremental output after the edit, file by file.

## Methodology

- **Host Roslyn.** Every row runs on `Microsoft.CodeAnalysis.CSharp` 5.9.0. That is the latest stable release on NuGet. SDK 10.0.400 ships a build of the same 5.9 compiler (5.9.0-1). Injectio, measured in the same suite, needs Roslyn 4.14 or later.
- **Generator builds.** The compiler that loads the generators runs on .NET 10 or later. On net10.0 and net11.0 the bench loads Immediate's `roslyn5.9` build. That build targets net10.0, so on net8.0 and net9.0 the bench loads the `roslyn4.8` build of the same package version. CSE ships one build, compiled against Roslyn 4.8.
- **Loading.** Each generator loads from its own folder into its own load context. Only the generator DLL is loaded, never the code fix DLL next to it. Analyzers and code fixes in the same packages are not run.
- **Cold.** Each call creates a fresh `CSharpCompilation` from pre-parsed trees and a fresh `CSharpGeneratorDriver`, then runs it. A fresh compilation binds all source symbols again on every call. Referenced-assembly symbols are cached on the shared `MetadataReference` objects, as in the IDE and the compiler server, so metadata loading is not part of the cold cost.
- **Incremental.** Each row warms a driver once on the original compilation. Each call runs that driver on the original compilation with one tree replaced (`ReplaceSyntaxTree`). The driver is immutable, so every call starts from the same warm state. This is what the IDE does on a keystroke.
- **The edit.** File `Src/Filler0051.cs`: `return x + 51;` becomes `return x + 52;`. It is a method-body change in a file with no attributes, with base-list classes and with no `WithName` call.
- **Step tracking** is off in every measured call (`trackIncrementalGeneratorSteps: false`).
- **Project shape.** Assembly `Bench.App`, `OutputKind.ConsoleApplication`, nullable enabled, C# latest. CSE also emits `MapAllEndpoints` because the project is an executable. `Files` is 200 or 1000 source files: the 50 feature files, `GlobalUsings.cs` with the Web SDK implicit usings, `Program.cs` and filler files for the rest. A filler file holds an interface, a class with three small methods, a record and a class that derives from `List<string>`. Every 10th filler file also maps a route with `.WithName(...)`.
- **References.** One set for every row in the suite: the host runtime's framework, the ASP.NET Core shared framework, the CSE runtime DLLs and the competitor attribute DLLs. On net8.0 the CSE runtime DLLs need Microsoft.Extensions.DependencyInjection.Abstractions and Microsoft.Extensions.Logging.Abstractions 9.0.4 from NuGet. Those copies from the output folder replace the older shared framework copies for every row, as SDK conflict resolution does in a CSE app. On the other TFMs the shared framework copies are used.
- **Preprocessor symbols.** The parse options define the symbols the SDK defines for the host TFM in Release (`NET`, `NET10_0`, `NET8_0_OR_GREATER` and so on), because some generators emit code under them.
- **Options.** One `AnalyzerConfigOptionsProvider` for every row. Global options: `build_property.IsTestProject=false`, `build_property.RootNamespace=Bench.App` (set by the SDK for every project) plus the NetEscapades defaults. No per-file options.
- **Cross-TFM rows** run on that TFM's shared framework, so the references differ by TFM. The host Roslyn 5.9.0 also runs its netstandard2.0 build on net8.0 and net9.0 and its net10.0 build on net10.0 and net11.0. Immediate also switches from its roslyn5.9 build to its roslyn4.8 build on net8.0 and net9.0. Compare rows within one TFM.
- BenchmarkDotNet default job, `[MemoryDiagnoser]`, rows grouped by category, CSE is the baseline in each category.

## Pinned versions

| Package | Version | License |
|---|---|---|
| Microsoft.CodeAnalysis.CSharp (host) | 5.9.0 | MIT |
| Immediate.Apis | 6.4.0 (latest stable) | MIT |
| Immediate.Handlers | 4.2.0 (latest stable) | MIT |
| CSharpEssentials.Endpoints.Generators | 7.0.0 (project reference) | MIT |

## Semantic differences

- CSE emits one registry file per assembly that calls each endpoint's own `Map` method, plus an aggregate file in an executable. The route calls stay in user code. Self-check: 2 generated files, 13,205 characters at 200 files and 15,285 at 1000 files. The registry also lists explicit endpoint names from `WithName` calls, so it grows with the filler routes.
- Immediate.Apis emits one file per endpoint with the route mapping and handler glue, plus one `MapBenchAppEndpoints` file. Self-check: 51 files, 81,264 characters.
- With Immediate.Handlers the documented setup also emits a handler class per endpoint and `AddBenchAppHandlers`. Self-check: 102 files, 260,115 characters.
- The output sizes are not timings. They show that the rows do different amounts of emit work.

## Feature matrix

| | CSE | Immediate.Apis 6.4.0 |
|---|---|---|
| How an endpoint is found | Implements `IEndpoint` (a base list) | `[Handler]` plus `[MapGet]` and the other verb attributes |
| Syntax provider | `CreateSyntaxProvider` on every non-interface type with a base list | `ForAttributeWithMetadataName` |
| Extra per-invocation provider | Yes: `WithName` and `WithMetadata` calls | No |
| Compilation-wide step on each edit | Yes: the Host step walks the references | Yes: reads the compilation for the assembly name; the result is equal after the edit, so later steps stay cached |
| Generated per endpoint | One registry entry that calls `TEndpoint.Map`; route calls stay in user code | A generated mapping method plus handler glue; the handler class comes from Immediate.Handlers |
| Cross-assembly aggregate | `MapAllEndpoints` in an executable | No. One `Map{Assembly}Endpoints` per assembly |
| Endpoint groups | `IEndpointGroup` | `[RouteGroup]` and `[MapGroup<T>]` |
| Second generator needed | No | Yes, Immediate.Handlers |
| Roslyn builds shipped | 4.8 | 4.8, 5.0 and 5.9 |
| Step tracking names | Public constants | Present (`WithTrackingName`) |
| License | MIT | MIT |

## Incremental cache behaviour

From `--self-check` on net10.0 with `Files=200`, after the filler edit. The counts are the outputs of each tracked step by `IncrementalStepRunReason`. Lower-level `ForAttributeWithMetadataName` steps, the Roslyn input steps (`AnalyzerConfigOptions`, `CompilationOptions`, `ParseOptions`) and single-value steps that stay Cached are left out. The `Files=1000` counts are the same.

| Row | Step | Reasons |
|---|---|---|
| CSE | `Compilation` | Modified 1 |
| CSE | `Host` | Unchanged 1 |
| CSE | `EndpointTypes` | Cached 50 |
| CSE | `ReservedNames`, `CollectedEndpoints`, `IsTestProject` | Cached 1 each |
| CSE | source outputs | Cached 2 |
| Immediate.Apis | `Compilation` | Modified 3 |
| Immediate.Apis | `AssemblyName` | Unchanged 1 |
| Immediate.Apis | `Endpoints` | Cached 50 |
| Immediate.Apis | source outputs | Cached 51 |
| Immediate.Handlers | `Behaviors`, `AssemblyName` | Unchanged 1 each |
| Immediate.Handlers | `Handlers`, `HandlersWithBehaviors` | Cached 50 each |
| Immediate.Handlers | source outputs | Cached 51 |

Every source output stays cached after the unrelated edit. The self-check fails if a CSE output does not.

The report cannot show one CSE cost. `EndpointTypes` comes from `CreateSyntaxProvider`. Roslyn caches the predicate per tree, but when the compilation changes it runs the transform again for every node the predicate accepted, in every tree. The predicate accepts every non-interface type with a base list, so each edit runs `GetDeclaredSymbol` again for every base-list type in the project, not only for the edited file. Most of those types are not endpoints, so they are filtered out before the tracked step and do not appear in the table. The incremental timing rows include that work. Immediate's `ForAttributeWithMetadataName` providers also run again for every node they matched (the `Unchanged` counts in the self-check), but they match only the 50 attributed types, so their work does not grow with `Files`.

## Results

Pending the full default-job run. No numbers from quick runs are published.

## Known CSE costs to watch in the full run

These come from reading the generator source. They are not fixed here.

- `CSharpEssentials.Endpoints.Generators/EndpointsGenerator.cs:35-36,87-92`: the predicate accepts any class, struct or record declaration with a `BaseList` (interfaces excluded), and the transform calls `GetDeclaredSymbol` for each. Cold cost grows with the number of base-list types in the project, which is why `Files=1000` exists. Because the provider is `CreateSyntaxProvider`, every edit, even a method-body edit in an unrelated file, runs the transform again for every base-list type in the project. Expect the CSE incremental row to grow with `Files` while the Immediate.Apis incremental row stays nearly flat.
- `EndpointsGenerator.cs:46-48`: the Host step is a `CompilationProvider.Select` that runs on every edit. Its output is equatable, so later steps stay cached.
- `EndpointsGenerator.cs:50-51` with `EndpointNameReader.cs:26-27`: `GetOperation` on every `WithName` and `WithMetadata` invocation. This is also a `CreateSyntaxProvider`, so every edit runs it again for every such invocation in the project. The filler routes exercise this (one per 10 filler files).

## Caveats

- Design time only. Nothing here affects the shipped app.
- Part of CSE's cold `GetDeclaredSymbol` cost is declaration binding that the compiler does anyway in a full build. The generator moves that cost earlier; not all of it is extra.
- Code fixes and analyzers in the packages are not measured.
- The input is generated, not a real app. Real projects have larger files and more references.

## How to reproduce

```bash
./benchmarks/run-benchmarks.sh --project generators
dotnet run -c Release --project benchmarks/CSharpEssentials.Generators.Benchmarks -f net10.0 -- --self-check
```
