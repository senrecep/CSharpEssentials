# CSharpEssentials.EntityFrameworkCore.Generators: design-time benchmarks

Draft. This page describes how the CSE3001 analyzer is measured. Results are pending the full run.

Despite the project name, `CSharpEssentials.EntityFrameworkCore.Generators` holds no source generator. It holds one diagnostic analyzer, `IgnoreQueryFiltersAnalyzer` (CSE3001), which reports the parameterless EF Core `IgnoreQueryFilters()` when the named overload exists. It cannot run under `CSharpGeneratorDriver`, so it is measured with `CompilationWithAnalyzers` instead. The quality audit had left this package out as design-time only. The suite measures it anyway, so its IDE cost is known.

The analyzer runs inside the compiler and the IDE, not in the shipped app. Runtime cost of the EF Core helpers is covered in [the package BENCHMARKS.md](../CSharpEssentials.EntityFrameworkCore/BENCHMARKS.md).

---

## What was measured

Class `QueryFilterAnalyzerComparison` in `benchmarks/CSharpEssentials.Generators.Benchmarks/`. No other library ships this rule, so there is no competitor. The comparison is against a no-op analyzer with the same registration shape.

Input:

- `EfStub.cs`: a source stub of the EF Core 10 API, not the EF Core assembly. It declares `Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions` with both overloads: `IgnoreQueryFilters<T>(this IQueryable<T>)` and `IgnoreQueryFilters<T>(this IQueryable<T>, IReadOnlyCollection<string>)`. The analyzer looks only for these by metadata name and parameter shape. The stub keeps every TFM the same and avoids EF Core 10, which ships net10.0 only.
- 50 query files `Query001.cs` to `Query050.cs`. Each has an entity, an interface and a class whose method calls `IgnoreQueryFilters()` twice and `IgnoreQueryFilters(SoftDeleteOnly)` once. That is 150 calls, of which CSE3001 reports 100.
- Filler files for the rest of `Files` (200 or 1000), plus `GlobalUsings.cs` and `Program.cs`.

| Method | Category | Work | Like for like? |
|---|---|---|---|
| `NoOpAnalyzer_Compilation` | Compilation | `GetAnalyzerDiagnosticsAsync()` with `NoOpInvocationAnalyzer` | Floor. Building operations binds every method body; both rows pay for it |
| `CSharpEssentials_Compilation` (baseline) | Compilation | The same with `IgnoreQueryFiltersAnalyzer` | The difference over the floor is CSE's own cost |
| `NoOpAnalyzer_Document` | Document | Edit `Query025.cs` (a method-body change), then `GetAnalyzerSemanticDiagnosticsAsync` on that file only | Floor |
| `CSharpEssentials_Document` (baseline) | Document | The same with `IgnoreQueryFiltersAnalyzer` | The difference over the floor is CSE's own cost |

`NoOpInvocationAnalyzer` uses the same registration as CSE3001: `ConfigureGeneratedCodeAnalysis(None)`, `EnableConcurrentExecution()` and an invocation operation action registered at compilation start. Its callback is empty. It declares one enabled descriptor (`BENCH0001`, Info) that it never reports, because the driver skips an analyzer with no enabled diagnostics.

Parity is checked in `[GlobalSetup]` and with `--self-check`. A failed check stops the run.

- The input compiles with no errors.
- CSE reports exactly 100 diagnostics in Compilation and 2 in Document, all `CSE3001`.
- The no-op analyzer reports 0. A separate run with `logAnalyzerExecutionTime: true` checks that its telemetry shows 1 operation action, so the driver did not skip it.
- Each benchmark method returns the same counts.

There is no cold and incremental output check, because analyzers keep no incremental state.

## Methodology

- **Host Roslyn.** `Microsoft.CodeAnalysis.CSharp` 5.9.0, the latest stable release on NuGet. SDK 10.0.400 ships a build of the same 5.9 compiler (5.9.0-1). It is shared with the generator classes in the suite.
- **Fresh state per call.** Each call builds a fresh compilation (Compilation rows) or a fresh edited compilation (Document rows) and a fresh `CompilationWithAnalyzers`. Results are cached per instance, so reusing either would measure a cache hit. A fresh compilation binds all source symbols again on every call. Referenced-assembly symbols are cached on the shared `MetadataReference` objects, as in the IDE and the compiler server, so metadata loading is not part of the cost.
- **Single-threaded.** `concurrentAnalysis: false` for lower noise. The IDE runs analyzers concurrently.
- **No incremental API.** Analyzers have no incremental pipeline. The Document row is the closest IDE equivalent of a keystroke: the semantic diagnostics of one edited file.
- **Project shape.** Assembly `Bench.App`, `OutputKind.ConsoleApplication`, nullable enabled, C# latest, the same filler files as the generator classes.
- **References.** The same set as the generator classes: the host runtime's framework, the ASP.NET Core shared framework, the CSE runtime DLLs and the competitor attribute DLLs. No EF Core assembly.
- **Cross-TFM rows** run on that TFM's shared framework, so the references differ by TFM. The host Roslyn 5.9.0 also runs its netstandard2.0 build on net8.0 and net9.0 and its net10.0 build on net10.0 and net11.0. Compare rows within one TFM.
- BenchmarkDotNet default job, `[MemoryDiagnoser]`, rows grouped by category, CSE is the baseline in each category.

## Pinned versions

| Package | Version | License |
|---|---|---|
| Microsoft.CodeAnalysis.CSharp (host) | 5.9.0 | MIT |
| CSharpEssentials.EntityFrameworkCore.Generators | 7.0.0 (project reference) | MIT |

## Feature summary

| | CSE3001 |
|---|---|
| Rule | Reports `IgnoreQueryFilters()` without filter keys, which turns off every query filter |
| Default severity | Info |
| When it runs | Only when the referenced EF Core has the named overload `IgnoreQueryFilters(IReadOnlyCollection<string>)` (EF Core 10 or later); otherwise no operation action is registered |
| Concurrent execution | On |
| Generated code | Skipped |
| Code fix | `CSharpEssentials.EntityFrameworkCore.CodeFixes`; not measured |

## Results

Pending the full default-job run. No numbers from quick runs are published.

## Caveats

- Design time only. Nothing here affects the shipped app.
- The input uses a source stub of the EF Core 10 API, not the EF Core assembly. A real project also references EF Core and its dependencies. Those references are not part of this input, so absolute times are lower than in a real project.
- Analysis is single-threaded, unlike the IDE.
- The code fix is not measured.

## How to reproduce

```bash
./benchmarks/run-benchmarks.sh --project generators
dotnet run -c Release --project benchmarks/CSharpEssentials.Generators.Benchmarks -f net10.0 -- --self-check
```
