# CSharpEssentials.Enums.Generators: design-time benchmarks

The `[StringEnum]` source generator measured against NetEscapades.EnumGenerators 1.0.0-beta21. Full default-job results for net8.0, net9.0, net10.0 and net11.0 are in [Results](#results). CSE is faster on a cold run. After an edit, NetEscapades is level or slightly faster; see [Where CSE loses](#where-cse-loses).

The generator runs inside the compiler and the IDE, not in the shipped app. The cost measured here is build time and the work the IDE does after a keystroke. Runtime cost of the generated code is covered in [the package BENCHMARKS.md](../CSharpEssentials.Enums/BENCHMARKS.md).

---

## What was measured

Class `EnumsGeneratorComparison` in `benchmarks/CSharpEssentials.Generators.Benchmarks/`. The project is an in-memory web app with 30 enums, `Enum001` to `Enum030`, each with 8 members (`Alpha` to `Theta`) and default values. Each library gets its opt-in attribute on the same enums:

```csharp
using CSharpEssentials.Enums;         // or: using NetEscapades.EnumGenerators;

[StringEnum]                          // or: [EnumExtensions]
public enum Enum007 { Alpha, Beta, Gamma, Delta, Epsilon, Zeta, Eta, Theta }
```

| Method | Category | Generators | Like for like? |
|---|---|---|---|
| `EmptyDriver_Cold` | Cold | none | No. Compilation and driver object overhead. A driver with no generators binds nothing, so this is not a floor for generator work |
| `CSharpEssentials_Cold` / `_Incremental` (baseline) | Cold / Incremental | `CSharpEssentials.Enums.Generators` | Same input, different output (see below) |
| `NetEscapades_Cold` / `_Incremental` | Cold / Incremental | `NetEscapades.EnumGenerators.Generators` | Same input, different output (see below) |

Parity is checked in `[GlobalSetup]` and with `--self-check`. A failed check stops the run.

- Every row must name exactly the 30 enums in its generated text.
- No generator may throw or report an error, and the output must compile with no errors.
- Cold output on the original project must equal incremental output after the edit, file by file.

## Methodology

- Host Roslyn: every row runs on `Microsoft.CodeAnalysis.CSharp` 5.9.0, the latest stable release on NuGet. SDK 10.0.400 ships a build of the same 5.9 compiler (5.9.0-1). It is shared with the other classes in the suite, one of which needs Roslyn 4.14 or later.
- Loading: each generator loads from its own folder into its own load context. The usage analyzers in both packages are not run.
- Recommended install: the NetEscapades meta package brings `NetEscapades.EnumGenerators.RuntimeDependencies`. Without it the generator emits nested copies of `EnumParseOptions` and the other option types in every extensions class. The shared references include that DLL, so NetEscapades uses the shared types, as in the recommended install. The DLL is inert for CSE.
- Cold: each call creates a fresh `CSharpCompilation` from pre-parsed trees and a fresh `CSharpGeneratorDriver`, then runs it. A fresh compilation binds all source symbols again on every call. Referenced-assembly symbols are cached on the shared `MetadataReference` objects, as in the IDE and the compiler server, so metadata loading is not part of the cold cost.
- Incremental: each row warms a driver once on the original compilation. Each call runs that driver on the original compilation with one tree replaced (`ReplaceSyntaxTree`), which is what the IDE does on a keystroke.
- The edit: in `Src/Filler0051.cs`, `return x + 51;` becomes `return x + 52;`. It is a method-body change in a file with no attributes, with base-list classes and with no `WithName` call.
- Step tracking is off in every measured call.
- Project shape: assembly `Bench.App`, `OutputKind.ConsoleApplication`, nullable enabled, C# latest. `Files` is 200 or 1000 source files: the 30 feature files, `GlobalUsings.cs` with the Web SDK implicit usings, `Program.cs` and filler files for the rest.
- References: one set for every row: the host runtime's framework, the ASP.NET Core shared framework, the CSE runtime DLLs and the competitor attribute and runtime DLLs. On net8.0 the CSE runtime DLLs need Microsoft.Extensions.DependencyInjection.Abstractions and Microsoft.Extensions.Logging.Abstractions 9.0.4 from NuGet. Those copies from the output folder replace the older shared framework copies for every row, as SDK conflict resolution does in a CSE app. On the other TFMs the shared framework copies are used.
- Preprocessor symbols: the parse options define the symbols the SDK defines for the host TFM in Release. `NETESCAPADES_ENUMGENERATORS_USAGES`, which the meta package's build targets define, is not applied. It only keeps the `[Conditional]` attribute in IL.
- Options: one `AnalyzerConfigOptionsProvider` for every row. Global options are the package defaults plus `build_property.RootNamespace=Bench.App` (set by the SDK for every project): `EnumGenerator_EnumMetadataSource=EnumMemberAttribute`, `EnumGenerator_ForceExtensionMembers=false`, `EnumGenerator_ForceInternal=false`. `CSharpEssentialsEnumNaming` stays unset.
- Cross-TFM rows run on that TFM's shared framework, so the references differ by TFM. The host Roslyn 5.9.0 also runs its netstandard2.0 build on net8.0 and net9.0 and its net10.0 build on net10.0 and net11.0. Compare rows within one TFM.
- BenchmarkDotNet default job, `[MemoryDiagnoser]`, rows grouped by category, CSE is the baseline in each category.

## Pinned versions

| Package | Version | License |
|---|---|---|
| Microsoft.CodeAnalysis.CSharp (host) | 5.9.0 | MIT |
| NetEscapades.EnumGenerators.Generators | 1.0.0-beta21 | MIT |
| NetEscapades.EnumGenerators.RuntimeDependencies | 1.0.0-beta21 | MIT |
| CSharpEssentials.Enums.Generators | 7.0.0 (project reference) | MIT |

NetEscapades.EnumGenerators has never shipped a stable release. Every 1.x version on NuGet is a beta, so the suite pins the latest prerelease, 1.0.0-beta21. The generator DLL ships in the `.Generators` package; the meta package `NetEscapades.EnumGenerators` holds only build targets.

## Semantic differences

The two rows read the same input but write different output, so the timings are the cost of each library's documented output and include different amounts of emit work.

- CSE emits per enum an `{Enum}Extensions` class with `EnumInfo` metadata, and per assembly a module-initializer registry. It also emits a `ModuleInitializerAttribute` polyfill when the target lacks the attribute; every TFM in this suite has it. Self-check: 31 generated files, 560,196 characters.
- NetEscapades emits per enum a larger extensions class (`ToStringFast`, `IsDefined`, `TryParse`, `Parse`, `GetValues`, `GetNames` and more overloads) and no registry. Self-check: 30 generated files, 1,799,820 characters.

## Feature matrix

| | CSE | NetEscapades.EnumGenerators 1.0.0-beta21 |
|---|---|---|
| Opt-in attribute | `[StringEnum]` | `[EnumExtensions]` |
| Output | Extensions, `EnumInfo` metadata, module-initializer registry | Extensions only |
| Wire names | Yes: naming policies such as snake_case, aliases, fallback | Names from `[EnumMember]`, `[Display]` or `[Description]` metadata |
| Usage analyzers in the package | Yes | Yes (NEEG004 to NEEG012) |
| Compilation-wide step on each edit | Yes: checks for `ModuleInitializerAttribute` | Reads the compilation; the result is equal after the edit |
| Minimum Roslyn | 4.8 | 4.4 |
| Release state | Stable | Prerelease only |
| License | MIT | MIT |

## Incremental cache behaviour

From `--self-check` on net10.0 with `Files=200`, after the filler edit. Lower-level `ForAttributeWithMetadataName` steps, the Roslyn input steps (`AnalyzerConfigOptions`, `CompilationOptions`, `ParseOptions`) and single-value steps that stay Cached are left out. The `Files=1000` counts are the same.

| Row | Step | Reasons |
|---|---|---|
| CSE | `Compilation` | Modified 3 |
| CSE | `EnumModuleInitializer` | Unchanged 1 |
| CSE | `EnumModels` | Cached 30 |
| CSE | `EnumRegistry`, `EnumGeneratorSettings` | Cached 1 each |
| CSE | source outputs | Cached 31 |
| NetEscapades | `Compilation` | Modified 3 |
| NetEscapades | `InitialExtraction` | Unchanged 30 |
| NetEscapades | `RemovingNulls` | Cached 30 |
| NetEscapades | source outputs | Cached 30 |

Every source output stays cached after the unrelated edit. The self-check fails if a CSE output does not.

## Results

### How these numbers were produced

- BenchmarkDotNet v0.14.0, default job, full runs (no `--quick`), one suite at a time, on a 6-core, 12-thread x64 desktop CPU on Linux (AVX2). The SDK and runtime builds are in the header of each report.
- TFMs: net8.0 (.NET 8.0.30), net9.0 (.NET 9.0.19), net10.0 (.NET 10.0.11) and net11.0 (.NET 11.0.0). The host Roslyn is 5.9.0 on every TFM. Compare rows within one TFM column.
- The 1-minute load average stayed between 2.63 and 3.35 during the generators suite. No row of this class has a CV above 5%.
- Reports: [`benchmarks/results/generators/{tfm}/results/`](../benchmarks/results/generators/), file `CSharpEssentials.Generators.Benchmarks.EnumsGeneratorComparison-report-github.md` (and `.csv`) per TFM. Every number below is copied from the `-report-github.md` files.

How to read the tables: each cell is `Mean (Ratio)` or `Allocated (Alloc Ratio)` for that TFM. The ratio is against the CSE row of the same category, `Files` value and TFM. `EmptyDriver_Cold` is driver overhead only and is not a floor for generator work.

### Time

| Category | Files | Method | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|---|
| Cold | 200 | `EmptyDriver_Cold` | 7.993 μs (0.001) | 6.783 μs (0.001) | 6.251 μs (0.001) | 6.577 μs (0.001) |
| Cold | 200 | `CSharpEssentials_Cold` | 5,866.451 μs (1.000) | 5,355.610 μs (1.000) | 4,942.143 μs (1.000) | 4,763.679 μs (1.000) |
| Cold | 200 | `NetEscapades_Cold` | 8,563.291 μs (1.460) | 8,268.789 μs (1.544) | 7,621.981 μs (1.542) | 7,637.331 μs (1.603) |
| Cold | 1000 | `EmptyDriver_Cold` | 41.541 μs (0.003) | 34.112 μs (0.003) | 33.721 μs (0.003) | 34.909 μs (0.003) |
| Cold | 1000 | `CSharpEssentials_Cold` | 12,500.349 μs (1.000) | 12,155.357 μs (1.001) | 11,034.104 μs (1.001) | 10,611.914 μs (1.001) |
| Cold | 1000 | `NetEscapades_Cold` | 16,581.287 μs (1.327) | 16,420.758 μs (1.352) | 14,317.496 μs (1.298) | 14,740.240 μs (1.391) |
| Incremental | 200 | `CSharpEssentials_Incremental` | 2,579.371 μs (1.00) | 2,168.588 μs (1.00) | 1,954.112 μs (1.00) | 1,896.507 μs (1.00) |
| Incremental | 200 | `NetEscapades_Incremental` | 2,458.410 μs (0.95) | 2,105.485 μs (0.97) | 1,817.222 μs (0.93) | 1,810.005 μs (0.95) |
| Incremental | 1000 | `CSharpEssentials_Incremental` | 3,855.055 μs (1.00) | 3,352.278 μs (1.00) | 3,081.037 μs (1.00) | 2,917.586 μs (1.00) |
| Incremental | 1000 | `NetEscapades_Incremental` | 3,729.231 μs (0.97) | 3,353.753 μs (1.00) | 2,921.062 μs (0.95) | 2,828.807 μs (0.97) |

### Allocation

| Category | Files | Method | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|---|
| Cold | 200 | `EmptyDriver_Cold` | 2.26 KB (0.000) | 2.25 KB (0.000) | 2.19 KB (0.000) | 2.17 KB (0.000) |
| Cold | 200 | `CSharpEssentials_Cold` | 5681.91 KB (1.000) | 5680.99 KB (1.000) | 5675.32 KB (1.000) | 5683.49 KB (1.000) |
| Cold | 200 | `NetEscapades_Cold` | 12484.55 KB (2.197) | 12479.08 KB (2.197) | 12480.07 KB (2.199) | 12480.68 KB (2.196) |
| Cold | 1000 | `EmptyDriver_Cold` | 2.26 KB (0.000) | 2.25 KB (0.000) | 2.19 KB (0.000) | 2.17 KB (0.000) |
| Cold | 1000 | `CSharpEssentials_Cold` | 7163.76 KB (1.000) | 7155.15 KB (1.000) | 7155.66 KB (1.000) | 7150.38 KB (1.000) |
| Cold | 1000 | `NetEscapades_Cold` | 13964.1 KB (1.949) | 13951.82 KB (1.950) | 13949.65 KB (1.949) | 13944.24 KB (1.950) |
| Incremental | 200 | `CSharpEssentials_Incremental` | 612.41 KB (1.00) | 611.65 KB (1.00) | 607.75 KB (1.00) | 612.82 KB (1.00) |
| Incremental | 200 | `NetEscapades_Incremental` | 562.26 KB (0.92) | 559.57 KB (0.91) | 563.94 KB (0.93) | 564.3 KB (0.92) |
| Incremental | 1000 | `CSharpEssentials_Incremental` | 725.62 KB (1.00) | 717.22 KB (1.00) | 714.52 KB (1.00) | 714.42 KB (1.00) |
| Incremental | 1000 | `NetEscapades_Incremental` | 673.59 KB (0.93) | 670.35 KB (0.93) | 666.42 KB (0.93) | 663.16 KB (0.93) |

### Where CSE loses

- Incremental, every TFM and both `Files` values: NetEscapades takes 0.93 to 1.00 of the CSE time and allocates 0.91 to 0.93 of the CSE bytes. On net10.0 at `Files=200` that is 1,817.222 μs and 563.94 KB against 1,954.112 μs and 607.75 KB for CSE. On net9.0 at `Files=1000` the two are level (3,352.278 μs for CSE, 3,353.753 μs for NetEscapades).

Cold, CSE is faster on every row (NetEscapades ratio 1.298 to 1.603) and allocates less (NetEscapades alloc ratio 1.949 to 2.199). The rows do not emit the same output: 31 files and 560,196 characters for CSE, 30 files and 1,799,820 characters for NetEscapades (see [Semantic differences](#semantic-differences)). Part of the cold gap is emit work CSE does not do. After an unrelated edit both libraries keep every output cached, so the incremental rows compare the per-edit pipeline work, and there CSE is slightly behind.

## Known CSE costs

From reading the source. Not fixed here. NetEscapades also reads the compilation on every edit, so this step alone does not explain the small incremental gap in [Where CSE loses](#where-cse-loses). That gap has not been profiled.

- `CSharpEssentials.Enums.Generators/StringEnumGenerator.cs:69-71`: the ModuleInitializer step is a `CompilationProvider.Select` that runs on every edit. Its output is a bool, so later steps stay cached.

## Caveats

- Code fixes and analyzers in the packages are not measured.
- The input is generated, not a real app.

## How to reproduce

```bash
./benchmarks/run-benchmarks.sh --project generators
dotnet run -c Release --project benchmarks/CSharpEssentials.Generators.Benchmarks -f net10.0 -- --self-check
```
