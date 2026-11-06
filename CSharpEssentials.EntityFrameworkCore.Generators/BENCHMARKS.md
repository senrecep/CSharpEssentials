# CSharpEssentials.EntityFrameworkCore.Generators: design-time benchmarks

This page describes how the CSE3001 analyzer is measured. Full default-job results for net8.0, net9.0, net10.0 and net11.0 are in [Results](#results). On the cleanest pair, one edited file in a 1,000-file project, the no-op analyzer takes 0.89 to 0.91 of the CSE3001 time on every TFM; see [What the rule costs over the floor](#what-the-rule-costs-over-the-floor).

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

- Host Roslyn. `Microsoft.CodeAnalysis.CSharp` 5.9.0, the latest stable release on NuGet. SDK 10.0.400 ships a build of the same 5.9 compiler (5.9.0-1). It is shared with the generator classes in the suite.
- Fresh state per call. Each call builds a fresh compilation (Compilation rows) or a fresh edited compilation (Document rows) and a fresh `CompilationWithAnalyzers`. Results are cached per instance, so reusing either would measure a cache hit. A fresh compilation binds all source symbols again on every call. Referenced-assembly symbols are cached on the shared `MetadataReference` objects, as in the IDE and the compiler server, so metadata loading is not part of the cost.
- Single-threaded. `concurrentAnalysis: false` for lower noise. The IDE runs analyzers concurrently.
- No incremental API. Analyzers have no incremental pipeline. The Document row is the closest IDE equivalent of a keystroke: the semantic diagnostics of one edited file.
- Project shape. Assembly `Bench.App`, `OutputKind.ConsoleApplication`, nullable enabled, C# latest, the same filler files as the generator classes.
- References. The same set as the generator classes: the host runtime's framework, the ASP.NET Core shared framework, the CSE runtime DLLs and the competitor attribute DLLs. No EF Core assembly.
- Cross-TFM rows run on that TFM's shared framework, so the references differ by TFM. The host Roslyn 5.9.0 also runs its netstandard2.0 build on net8.0 and net9.0 and its net10.0 build on net10.0 and net11.0. Compare rows within one TFM.
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

### How these numbers were produced

- BenchmarkDotNet v0.14.0, default job, full runs (no `--quick`), one suite at a time, on one Linux x64 machine with a 6-core, 12-thread desktop CPU (AVX2). The exact CPU, SDK and runtime builds are in the header of each report.
- TFMs: net8.0 (.NET 8.0.30), net9.0 (.NET 9.0.19), net10.0 (.NET 10.0.11) and net11.0 (.NET 11.0.0). The host Roslyn is 5.9.0 on every TFM. Compare rows within one TFM column.
- The 1-minute load average stayed between 1.77 and 6.52 during the generators suite and the two reruns of this class on every TFM. Nine rows still have a CV above 5% after the reruns and are marked `noisy` in the tables. Every TFM has at least one noisy row at `Compilation`, `Files=200`.
- Reports: [`benchmarks/results/generators/{tfm}/results/`](../benchmarks/results/generators/), file `CSharpEssentials.Generators.Benchmarks.QueryFilterAnalyzerComparison-report-github.md` (and `.csv`) per TFM. Every number below is copied from the `-report-github.md` files.

How to read the tables: each cell is `Mean (Ratio)` or `Allocated (Alloc Ratio)` for that TFM. The ratio is against the CSE row of the same category, `Files` value and TFM. The no-op analyzer is the floor, so the CSE cost is the gap between the two rows, not a loss to a competitor.

### Time

| Category | Files | Method | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|---|
| Compilation | 200 | `NoOpAnalyzer_Compilation` | 116.303 ms (0.84), noisy | 107.353 ms (0.78) | 103.280 ms (0.74) | 123.566 ms (0.90), noisy |
| Compilation | 200 | `CSharpEssentials_Compilation` | 138.675 ms (1.00), noisy | 138.569 ms (1.01), noisy | 142.563 ms (1.02), noisy | 140.999 ms (1.02), noisy |
| Compilation | 1000 | `NoOpAnalyzer_Compilation` | 647.059 ms (0.89) | 631.812 ms (0.98) | 613.849 ms (1.00) | 599.239 ms (0.97) |
| Compilation | 1000 | `CSharpEssentials_Compilation` | 725.921 ms (1.00) | 642.773 ms (1.00) | 616.659 ms (1.00) | 616.412 ms (1.00), noisy |
| Document | 200 | `NoOpAnalyzer_Document` | 2.990 ms (0.83) | 2.715 ms (0.80) | 2.623 ms (0.76) | 2.531 ms (0.78) |
| Document | 200 | `CSharpEssentials_Document` | 3.616 ms (1.00) | 3.404 ms (1.00) | 3.506 ms (1.02), noisy | 3.298 ms (1.02), noisy |
| Document | 1000 | `NoOpAnalyzer_Document` | 8.994 ms (0.91) | 8.414 ms (0.89) | 8.350 ms (0.91) | 8.331 ms (0.90) |
| Document | 1000 | `CSharpEssentials_Document` | 9.881 ms (1.00) | 9.405 ms (1.00) | 9.176 ms (1.00) | 9.218 ms (1.00) |

### Allocation

| Category | Files | Method | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|---|
| Compilation | 200 | `NoOpAnalyzer_Compilation` | 38450.1 KB (0.92) | 37962.37 KB (0.89) | 38523.54 KB (0.92) | 38262.87 KB (0.91) |
| Compilation | 200 | `CSharpEssentials_Compilation` | 41962.03 KB (1.00) | 42822.46 KB (1.00) | 41877.07 KB (1.00) | 41951.53 KB (1.00) |
| Compilation | 1000 | `NoOpAnalyzer_Compilation` | 204254.09 KB (0.96) | 207850.09 KB (1.01) | 200841.48 KB (0.95) | 200074.02 KB (0.96) |
| Compilation | 1000 | `CSharpEssentials_Compilation` | 213201.74 KB (1.00) | 206207.87 KB (1.00) | 210655.31 KB (1.00) | 209297.73 KB (1.00) |
| Document | 200 | `NoOpAnalyzer_Document` | 772.95 KB (0.91) | 771.93 KB (0.91) | 769.05 KB (0.91) | 765.01 KB (0.91) |
| Document | 200 | `CSharpEssentials_Document` | 846.4 KB (1.00) | 845.57 KB (1.00) | 844.17 KB (1.00) | 839.28 KB (1.00) |
| Document | 1000 | `NoOpAnalyzer_Document` | 2663.82 KB (0.97) | 2656.42 KB (0.97) | 2653.01 KB (0.97) | 2654.48 KB (0.97) |
| Document | 1000 | `CSharpEssentials_Document` | 2738 KB (1.00) | 2729.68 KB (1.00) | 2727.68 KB (1.00) | 2727.06 KB (1.00) |

### What the rule costs over the floor

There is no competitor, so there is no "Where CSE loses" list. In time every CSE row is at or above the no-op floor. In allocation every CSE row is above it except Compilation at `Files=1000` on net9.0, where the no-op row allocates 207850.09 KB (1.01) against 206207.87 KB for CSE.

- Document, `Files=1000`, the closest equivalent of a keystroke. No row is noisy. CSE takes 9.881 ms against 8.994 ms on net8.0, 9.405 ms against 8.414 ms on net9.0, 9.176 ms against 8.350 ms on net10.0 and 9.218 ms against 8.331 ms on net11.0 (no-op ratio 0.89 to 0.91). It allocates 2738 KB, 2729.68 KB, 2727.68 KB and 2727.06 KB against 2663.82 KB, 2656.42 KB, 2653.01 KB and 2654.48 KB (ratio 0.97).
- Document, `Files=200`: the no-op row takes 0.76 to 0.83 of the CSE time. The clean pairs are 3.616 ms against 2.990 ms on net8.0 and 3.404 ms against 2.715 ms on net9.0. The net10.0 and net11.0 CSE rows are noisy.
- Compilation, `Files=1000`: the no-op row takes 0.89 of the CSE time on net8.0 (725.921 ms against 647.059 ms) and 0.97 to 1.00 on the other TFMs. The net11.0 CSE row is noisy.
- Compilation, `Files=200`: every TFM has a noisy row in this pair, so the gap cannot be read from this run.

On the clean Document rows the gap does not shrink as `Files` grows: compare 3.616 ms against 2.990 ms at `Files=200` with 9.881 ms against 8.994 ms at `Files=1000` on net8.0. From reading `IgnoreQueryFiltersAnalyzer.cs` (not profiled): the compilation start action looks up two types with `GetTypeByMetadataName` and reads the `IgnoreQueryFilters` overloads before it registers the operation action. The no-op analyzer does no lookup. The IDE runs that start action once per compilation, so after every edit.

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
