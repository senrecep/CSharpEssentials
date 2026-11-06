# CSharpEssentials.Endpoints.Generators: design-time benchmarks

This page describes how the endpoints source generator is measured against Immediate.Apis 6.4.0. Full default-job results for net8.0, net9.0, net10.0 and net11.0 are in [Results](#results). CSE is ahead at `Files=200` and well behind Immediate.Apis at `Files=1000`, cold and after an edit; see [Where CSE loses](#where-cse-loses).

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

- Host Roslyn. Every row runs on `Microsoft.CodeAnalysis.CSharp` 5.9.0. That is the latest stable release on NuGet. SDK 10.0.400 ships a build of the same 5.9 compiler (5.9.0-1). Injectio, measured in the same suite, needs Roslyn 4.14 or later.
- Generator builds. The compiler that loads the generators runs on .NET 10 or later. On net10.0 and net11.0 the bench loads Immediate's `roslyn5.9` build. That build targets net10.0, so on net8.0 and net9.0 the bench loads the `roslyn4.8` build of the same package version. CSE ships one build, compiled against Roslyn 4.8.
- Loading. Each generator loads from its own folder into its own load context. Only the generator DLL is loaded, never the code fix DLL next to it. Analyzers and code fixes in the same packages are not run.
- Cold. Each call creates a fresh `CSharpCompilation` from pre-parsed trees and a fresh `CSharpGeneratorDriver`, then runs it. A fresh compilation binds all source symbols again on every call. Referenced-assembly symbols are cached on the shared `MetadataReference` objects, as in the IDE and the compiler server, so metadata loading is not part of the cold cost.
- Incremental. Each row warms a driver once on the original compilation. Each call runs that driver on the original compilation with one tree replaced (`ReplaceSyntaxTree`). The driver is immutable, so every call starts from the same warm state. This is what the IDE does on a keystroke.
- The edit. File `Src/Filler0051.cs`: `return x + 51;` becomes `return x + 52;`. It is a method-body change in a file with no attributes, with base-list classes and with no `WithName` call.
- Step tracking is off in every measured call (`trackIncrementalGeneratorSteps: false`).
- Project shape. Assembly `Bench.App`, `OutputKind.ConsoleApplication`, nullable enabled, C# latest. CSE also emits `MapAllEndpoints` because the project is an executable. `Files` is 200 or 1000 source files: the 50 feature files, `GlobalUsings.cs` with the Web SDK implicit usings, `Program.cs` and filler files for the rest. A filler file holds an interface, a class with three small methods, a record and a class that derives from `List<string>`. Every 10th filler file also maps a route with `.WithName(...)`.
- References. One set for every row in the suite: the host runtime's framework, the ASP.NET Core shared framework, the CSE runtime DLLs and the competitor attribute DLLs. On net8.0 the CSE runtime DLLs need Microsoft.Extensions.DependencyInjection.Abstractions and Microsoft.Extensions.Logging.Abstractions 9.0.4 from NuGet. Those copies from the output folder replace the older shared framework copies for every row, as SDK conflict resolution does in a CSE app. On the other TFMs the shared framework copies are used.
- Preprocessor symbols. The parse options define the symbols the SDK defines for the host TFM in Release (`NET`, `NET10_0`, `NET8_0_OR_GREATER` and so on), because some generators emit code under them.
- Options. One `AnalyzerConfigOptionsProvider` for every row. Global options: `build_property.IsTestProject=false`, `build_property.RootNamespace=Bench.App` (set by the SDK for every project) plus the NetEscapades defaults. No per-file options.
- Cross-TFM rows run on that TFM's shared framework, so the references differ by TFM. The host Roslyn 5.9.0 also runs its netstandard2.0 build on net8.0 and net9.0 and its net10.0 build on net10.0 and net11.0. Immediate also switches from its roslyn5.9 build to its roslyn4.8 build on net8.0 and net9.0. Compare rows within one TFM.
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
| Syntax provider | `CreateSyntaxProvider` on non-interface types with a base list and a static `Map` (either one for a `partial` type), plus one on every `WithName` and `WithMetadata` call | `ForAttributeWithMetadataName` |
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

The report cannot show one CSE cost. `EndpointTypes` and `ReservedNames` come from `CreateSyntaxProvider`. Roslyn caches the predicate per tree, but when the compilation changes it runs the transform again for every node the predicate accepted, in every tree. For `EndpointTypes` that is the 50 endpoint classes. For `ReservedNames` it is every `WithName` and `WithMetadata` call in the project, and each one runs `GetOperation` again. The tracked `ReservedNames` step sits after `Collect`, so the table shows one cached value, not the per-call work. The incremental timing rows include that work. Immediate's `ForAttributeWithMetadataName` providers also run again for every node they matched (the `Unchanged` counts in the self-check), but they match only the 50 attributed types, so their work does not grow with `Files`.

## Results

### How these numbers were produced

- BenchmarkDotNet v0.14.0, default job, full runs (no `--quick`), one suite at a time, on one Linux x64 machine with a 6-core, 12-thread desktop CPU (AVX2). The exact CPU, SDK and runtime builds are in the header of each report.
- TFMs: net8.0 (.NET 8.0.30), net9.0 (.NET 9.0.19), net10.0 (.NET 10.0.11) and net11.0 (.NET 11.0.0). The host Roslyn is 5.9.0 on every TFM. Compare rows within one TFM column.
- The 1-minute load average stayed between 1.77 and 3.37 during the generators suite and the two reruns of this class on net10.0 and net11.0. Five rows still have a CV above 5% after the reruns and are marked `noisy` in the tables: `CSharpEssentials_Cold` and `ImmediateApisWithHandlers_Cold` at `Files=1000` on net10.0 and net11.0, and `ImmediateApisWithHandlers_Incremental` at `Files=200` on net11.0.
- Reports: [`benchmarks/results/generators/{tfm}/results/`](../benchmarks/results/generators/), file `CSharpEssentials.Generators.Benchmarks.EndpointsGeneratorComparison-report-github.md` (and `.csv`) per TFM. Every number below is copied from the `-report-github.md` files.

How to read the tables: each cell is `Mean (Ratio)` or `Allocated (Alloc Ratio)` for that TFM. The ratio is against the CSE row of the same category, `Files` value and TFM. `EmptyDriver_Cold` is driver overhead only and is not a floor for generator work.

### Time

| Category | Files | Method | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|---|
| Cold | 200 | `EmptyDriver_Cold` | 9.936 μs (0.001) | 6.608 μs (0.000) | 6.114 μs (0.000) | 6.510 μs (0.001) |
| Cold | 200 | `CSharpEssentials_Cold` | 13,848.043 μs (1.000) | 13,788.855 μs (1.000) | 12,695.217 μs (1.001) | 11,866.179 μs (1.000) |
| Cold | 200 | `ImmediateApis_Cold` | 15,118.818 μs (1.092) | 14,067.330 μs (1.020) | 12,914.831 μs (1.019) | 12,471.030 μs (1.051) |
| Cold | 200 | `ImmediateApisWithHandlers_Cold` | 82,068.168 μs (5.927) | 73,283.429 μs (5.316) | 70,971.282 μs (5.597) | 69,349.376 μs (5.845) |
| Cold | 1000 | `EmptyDriver_Cold` | 41.166 μs (0.000) | 35.806 μs (0.000) | 34.376 μs (0.000) | 38.567 μs (0.001) |
| Cold | 1000 | `CSharpEssentials_Cold` | 83,341.794 μs (1.000) | 75,983.530 μs (1.000) | 75,440.372 μs (1.008), noisy | 74,564.793 μs (1.007), noisy |
| Cold | 1000 | `ImmediateApis_Cold` | 23,040.080 μs (0.276) | 21,108.214 μs (0.278) | 22,309.011 μs (0.298) | 20,969.176 μs (0.283) |
| Cold | 1000 | `ImmediateApisWithHandlers_Cold` | 91,716.335 μs (1.101) | 86,726.597 μs (1.141) | 92,315.378 μs (1.233), noisy | 90,616.516 μs (1.224), noisy |
| Incremental | 200 | `CSharpEssentials_Incremental` | 6,220.036 μs (1.00) | 5,819.315 μs (1.00) | 5,092.662 μs (1.00) | 5,050.929 μs (1.00) |
| Incremental | 200 | `ImmediateApis_Incremental` | 9,602.102 μs (1.54) | 8,851.833 μs (1.52) | 8,187.010 μs (1.61) | 7,497.199 μs (1.48) |
| Incremental | 200 | `ImmediateApisWithHandlers_Incremental` | 66,048.606 μs (10.62) | 63,156.468 μs (10.85) | 57,725.563 μs (11.34) | 61,495.625 μs (12.18), noisy |
| Incremental | 1000 | `CSharpEssentials_Incremental` | 34,521.508 μs (1.00) | 31,745.005 μs (1.00) | 29,256.639 μs (1.00) | 27,597.948 μs (1.00) |
| Incremental | 1000 | `ImmediateApis_Incremental` | 11,652.656 μs (0.34) | 10,571.983 μs (0.33) | 9,415.331 μs (0.32) | 9,097.143 μs (0.33) |
| Incremental | 1000 | `ImmediateApisWithHandlers_Incremental` | 72,400.476 μs (2.10) | 66,667.645 μs (2.10) | 65,265.458 μs (2.23) | 65,820.755 μs (2.39) |

### Allocation

| Category | Files | Method | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|---|
| Cold | 200 | `EmptyDriver_Cold` | 2.26 KB (0.001) | 2.25 KB (0.001) | 2.19 KB (0.001) | 2.17 KB (0.001) |
| Cold | 200 | `CSharpEssentials_Cold` | 2175.99 KB (1.000) | 2179.98 KB (1.000) | 2156.59 KB (1.000) | 2159.94 KB (1.000) |
| Cold | 200 | `ImmediateApis_Cold` | 5283.15 KB (2.428) | 5279.14 KB (2.422) | 5277.31 KB (2.447) | 5256.86 KB (2.434) |
| Cold | 200 | `ImmediateApisWithHandlers_Cold` | 32435.38 KB (14.906) | 32437.16 KB (14.880) | 32427.56 KB (15.037) | 32413.01 KB (15.006) |
| Cold | 1000 | `EmptyDriver_Cold` | 2.26 KB (0.000) | 2.25 KB (0.000) | 2.19 KB (0.000) | 2.17 KB (0.000) |
| Cold | 1000 | `CSharpEssentials_Cold` | 10191.61 KB (1.000) | 10178.54 KB (1.000) | 10651.53 KB (1.000) | 10634.73 KB (1.000) |
| Cold | 1000 | `ImmediateApis_Cold` | 6746.25 KB (0.662) | 6747.85 KB (0.663) | 6759.02 KB (0.635) | 6772.62 KB (0.637) |
| Cold | 1000 | `ImmediateApisWithHandlers_Cold` | 33885.85 KB (3.325) | 33892.29 KB (3.330) | 33939.11 KB (3.186) | 33931.88 KB (3.191) |
| Incremental | 200 | `CSharpEssentials_Incremental` | 1324.55 KB (1.00) | 1314.29 KB (1.00) | 1294.32 KB (1.00) | 1294.63 KB (1.00) |
| Incremental | 200 | `ImmediateApis_Incremental` | 1485.12 KB (1.12) | 1480.82 KB (1.13) | 1476.03 KB (1.14) | 1466.31 KB (1.13) |
| Incremental | 200 | `ImmediateApisWithHandlers_Incremental` | 24759.68 KB (18.69) | 24753.41 KB (18.83) | 24746.43 KB (19.12) | 24737.63 KB (19.11) |
| Incremental | 1000 | `CSharpEssentials_Incremental` | 5927.76 KB (1.00) | 5903.1 KB (1.00) | 5803.64 KB (1.00) | 5794.04 KB (1.00) |
| Incremental | 1000 | `ImmediateApis_Incremental` | 1587.35 KB (0.27) | 1587.05 KB (0.27) | 1579.01 KB (0.27) | 1590.57 KB (0.27) |
| Incremental | 1000 | `ImmediateApisWithHandlers_Incremental` | 24862.11 KB (4.19) | 24859.13 KB (4.21) | 24849.48 KB (4.28) | 24858.12 KB (4.29) |

### Where CSE loses

At `Files=1000` CSE is slower than Immediate.Apis and allocates more, cold and incremental, on every TFM. At `Files=200` CSE is ahead on every row.

- Cold, `Files=1000`: Immediate.Apis takes 0.276 to 0.298 of the CSE time. On net8.0 that is 23,040.080 μs against 83,341.794 μs, on net9.0 21,108.214 μs against 75,983.530 μs. The net10.0 and net11.0 CSE rows are noisy (CV above 5%) but show the same gap. Immediate.Apis allocates 0.635 to 0.663 of the CSE bytes (6759.02 KB against 10651.53 KB on net10.0).
- Incremental, `Files=1000`: Immediate.Apis takes 0.32 to 0.34 of the CSE time and allocates 0.27 of the CSE bytes on every TFM. On net10.0 that is 9,415.331 μs and 1579.01 KB against 29,256.639 μs and 5803.64 KB.
- Scaling with project size: on net10.0 the CSE incremental row grows from 5,092.662 μs at `Files=200` to 29,256.639 μs at `Files=1000`. The Immediate.Apis row grows from 8,187.010 μs to 9,415.331 μs. The cold rows show the same pattern: CSE grows from 12,695.217 μs to 75,440.372 μs (noisy), Immediate.Apis from 12,914.831 μs to 22,309.011 μs.

At `Files=200`, Immediate.Apis takes 1.019 to 1.092 of the CSE time cold and 1.48 to 1.61 incremental, and allocates 2.422 to 2.447 (cold) and 1.12 to 1.14 (incremental) of the CSE bytes.

The documented Immediate setup with Immediate.Handlers (`ImmediateApisWithHandlers_*`) is slower than CSE on every row, but the gap shrinks with size: 5.316 to 5.927 cold at `Files=200`, 1.101 to 1.233 cold at `Files=1000` (the net10.0 and net11.0 rows there are noisy).

Likely cause, from reading the source and the input (not profiled): the provider that grows with `Files` is the `WithName` and `WithMetadata` reader (see [Known CSE costs](#known-cse-costs)). It is a `CreateSyntaxProvider`, so after any edit Roslyn runs its transform again for every matching invocation in the project, and each run calls `GetOperation`, which binds the containing route call. The filler has one such call per 10 filler files, 15 at `Files=200` and 95 at `Files=1000`. Immediate.Apis uses `ForAttributeWithMetadataName` and matches only the 50 attributed endpoints.

## Known CSE costs

These come from reading the generator source. They are not fixed here. The run in [Where CSE loses](#where-cse-loses) shows the CSE rows growing with `Files` much faster than the Immediate.Apis rows.

- `CSharpEssentials.Endpoints.Generators/EndpointsGenerator.cs:36-40,88-100`: the `EndpointTypes` predicate accepts a non-interface type declaration that has a base list and declares a static `Map` method. A `partial` type needs only one of the two. The transform calls `GetDeclaredSymbol` for each match. In this input only the 50 endpoint classes match: the filler classes have a base list but no `Map`, and the filler route classes have a `Map` but no base list. This provider is a `CreateSyntaxProvider`, so every edit runs the transform again for the 50 matches.
- `EndpointsGenerator.cs:47-49`: the Host step is a `CompilationProvider.Select` that runs on every edit. Its output is equatable, so later steps stay cached.
- `EndpointsGenerator.cs:51-56,121-123` with `EndpointNameReader.cs:26-27`: `GetOperation` on every `WithName` and `WithMetadata` invocation in the project. This is also a `CreateSyntaxProvider`, so every edit runs it again for every such invocation, not only for the edited file. The filler routes exercise this: one call per 10 filler files, 15 at `Files=200` and 95 at `Files=1000`. It is the CSE provider whose work grows with `Files` in this input, and the likely cause of the `Files=1000` losses (not profiled).

## Caveats

- Design time only. Nothing here affects the shipped app.
- Part of CSE's cold cost is binding that the compiler does anyway in a full build: the endpoint declarations and the route calls around each `WithName`. The generator moves that cost earlier; not all of it is extra.
- Code fixes and analyzers in the packages are not measured.
- The input is generated, not a real app. Real projects have larger files and more references.

## How to reproduce

```bash
./benchmarks/run-benchmarks.sh --project generators
dotnet run -c Release --project benchmarks/CSharpEssentials.Generators.Benchmarks -f net10.0 -- --self-check
```
