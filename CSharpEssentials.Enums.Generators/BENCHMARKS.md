# CSharpEssentials.Enums.Generators: design-time benchmarks

Draft. This page describes how the `[StringEnum]` source generator is measured against NetEscapades.EnumGenerators 1.0.0-beta21. Results are pending the full run.

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

- **Host Roslyn.** Every row runs on `Microsoft.CodeAnalysis.CSharp` 5.9.0, the latest stable release on NuGet. SDK 10.0.400 ships a build of the same 5.9 compiler (5.9.0-1). It is shared with the other classes in the suite, one of which needs Roslyn 4.14 or later.
- **Loading.** Each generator loads from its own folder into its own load context. The usage analyzers in both packages are not run.
- **Recommended install.** The NetEscapades meta package brings `NetEscapades.EnumGenerators.RuntimeDependencies`. Without it the generator emits nested copies of `EnumParseOptions` and the other option types in every extensions class. The shared references include that DLL, so NetEscapades uses the shared types, as in the recommended install. The DLL is inert for CSE.
- **Cold.** Each call creates a fresh `CSharpCompilation` from pre-parsed trees and a fresh `CSharpGeneratorDriver`, then runs it. A fresh compilation binds all source symbols again on every call. Referenced-assembly symbols are cached on the shared `MetadataReference` objects, as in the IDE and the compiler server, so metadata loading is not part of the cold cost.
- **Incremental.** Each row warms a driver once on the original compilation. Each call runs that driver on the original compilation with one tree replaced (`ReplaceSyntaxTree`), which is what the IDE does on a keystroke.
- **The edit.** File `Src/Filler0051.cs`: `return x + 51;` becomes `return x + 52;`. It is a method-body change in a file with no attributes, with base-list classes and with no `WithName` call.
- **Step tracking** is off in every measured call.
- **Project shape.** Assembly `Bench.App`, `OutputKind.ConsoleApplication`, nullable enabled, C# latest. `Files` is 200 or 1000 source files: the 30 feature files, `GlobalUsings.cs` with the Web SDK implicit usings, `Program.cs` and filler files for the rest.
- **References.** One set for every row: the host runtime's framework, the ASP.NET Core shared framework, the CSE runtime DLLs and the competitor attribute and runtime DLLs. On net8.0 the CSE runtime DLLs need Microsoft.Extensions.DependencyInjection.Abstractions and Microsoft.Extensions.Logging.Abstractions 9.0.4 from NuGet. Those copies from the output folder replace the older shared framework copies for every row, as SDK conflict resolution does in a CSE app. On the other TFMs the shared framework copies are used.
- **Preprocessor symbols.** The parse options define the symbols the SDK defines for the host TFM in Release. `NETESCAPADES_ENUMGENERATORS_USAGES`, which the meta package's build targets define, is not applied. It only keeps the `[Conditional]` attribute in IL.
- **Options.** One `AnalyzerConfigOptionsProvider` for every row. Global options are the package defaults plus `build_property.RootNamespace=Bench.App` (set by the SDK for every project): `EnumGenerator_EnumMetadataSource=EnumMemberAttribute`, `EnumGenerator_ForceExtensionMembers=false`, `EnumGenerator_ForceInternal=false`. `CSharpEssentialsEnumNaming` stays unset.
- **Cross-TFM rows** run on that TFM's shared framework, so the references differ by TFM. The host Roslyn 5.9.0 also runs its netstandard2.0 build on net8.0 and net9.0 and its net10.0 build on net10.0 and net11.0. Compare rows within one TFM.
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

The two rows read the same input but write different output. Treat the timings as the cost of each library's documented output, not of the same output.

- CSE emits per enum an `{Enum}Extensions` class with `EnumInfo` metadata, and per assembly a module-initializer registry. It also emits a `ModuleInitializerAttribute` polyfill when the target lacks the attribute; every TFM in this suite has it. Self-check: 31 generated files, 560,196 characters.
- NetEscapades emits per enum a larger extensions class (`ToStringFast`, `IsDefined`, `TryParse`, `Parse`, `GetValues`, `GetNames` and more overloads) and no registry. Self-check: 30 generated files, 1,799,820 characters.
- The output sizes are not timings. They show that the rows do different amounts of emit work.

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

Pending the full default-job run. No numbers from quick runs are published.

## Known CSE costs to watch in the full run

From reading the source. Not fixed here.

- `CSharpEssentials.Enums.Generators/StringEnumGenerator.cs:69-71`: the ModuleInitializer step is a `CompilationProvider.Select` that runs on every edit. Its output is a bool, so later steps stay cached.

## Caveats

- Design time only. Nothing here affects the shipped app.
- Code fixes and analyzers in the packages are not measured.
- The input is generated, not a real app.

## How to reproduce

```bash
./benchmarks/run-benchmarks.sh --project generators
dotnet run -c Release --project benchmarks/CSharpEssentials.Generators.Benchmarks -f net10.0 -- --self-check
```
