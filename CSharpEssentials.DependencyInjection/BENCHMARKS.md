# CSharpEssentials.DependencyInjection: Performance benchmarks

Compared with Scrutor 7.0.0 and manual Microsoft.Extensions.DependencyInjection registration, measured with BenchmarkDotNet v0.14.0 on an AMD Ryzen 5 5600 under .NET 8, 9, 10 and 11.

The same suite has two newer classes that have not had a full run yet, TryAdd registration at N = 50, 500 and 2000 and transient decorator resolve. They are described in [Pending: TryAdd and transient decorator resolve](#pending-tryadd-and-transient-decorator-resolve).

---

## What was measured

Four BenchmarkDotNet classes in `benchmarks/CSharpEssentials.DependencyInjection.Benchmarks/` plus a Native AOT script. Every class uses `[MemoryDiagnoser]`. A self-check (`DiSelfCheck`) runs before the measurements and fails unless every variant registers the same (service type, implementation type, lifetime) set and every service resolves to its own implementation; for decoration it requires exactly IService0..IService9 to be wrapped once.

| # | Class | Measured unit | Columns |
|---|---|---|---|
| 1 | `ColdRegistrationBenchmarks` | A fresh process per sample (`RunStrategy.ColdStart`, 15 launches, 1 iteration, no warmup): new `ServiceCollection`, registration, `BuildServiceProvider`, one scope, one resolve of every service, dispose. JIT, assembly loading, reflection scanning and the container's first-resolve work are inside the measurement. This is the like-for-like startup number. N = 50, 500 | Manual, CseGenerated, CseReflection, Scrutor |
| 2 | `RegistrationBenchmarks` | New `ServiceCollection` + registration of N services, warm and in-process. Baseline is the empty collection. N = 50, 500 | Baseline, Manual, CseGenerated, CseReflection, Scrutor |
| 3 | `DecorationBenchmarks` | New collection, the 50 services registered by hand, then 10 of them (IService0..IService9) decorated, warm. The unit includes the 50 manual registrations for every column | Manual, CseGenerated, CseRuntime, Scrutor |
| 4 | `ResolveBenchmarks` | Provider built once per variant in setup (N = 50); per operation: create a scope, resolve one service, dispose. *Plain* resolves IService15 (depends on IService14) from each library's own registration. *Decorated* resolves IService0 (no dependency) after decorating IService0..IService9 on top of the manual registration | Plain: Manual, CseGenerated, Scrutor. Decorated: Manual, CseGenerated, CseRuntime, Scrutor |
| 5 | Native AOT (`benchmarks/di-aot/run-di-aot.sh`, not BenchmarkDotNet) | One sample per variant, `PublishAot=true`, net10, linux-x64: register S50, decorate 10, build, resolve all 50, check values | Manual, CseGenerated, CseReflection, Scrutor |

Input: `generate-services.py` writes one class library per N (`S50`, `S500`) shared by every variant. It holds N pairs `IService{i}` / `[RegisterScoped] Service{i}`; every service whose index is a positive multiple of 5 takes `IService{i-1}` as a constructor dependency. It also holds `ManualS{N}.Register` (hand-written `AddScoped<IService{i}, Service{i}>()` calls). A separate `S{N}.Decorators` library holds `Service{i}Decorator` for i < 10 with `[Decorates(typeof(IService{i}))]`, and `ManualDecorationS{N}.Apply`.

Variants:

| Variant | Call |
|---|---|
| Manual | `ManualS{N}.Register(services)`, hand-written `AddScoped` ([docs](https://learn.microsoft.com/dotnet/core/extensions/dependency-injection/service-registration)) |
| CseGenerated | `services.AddS{N}Services()`, the source-generated registry |
| CseReflection | `services.AddServicesFromAssemblies(asm)`, the opt-in reflection fallback |
| Scrutor | `services.Scan(s => s.FromAssemblyOf<IService0>().AddClasses().AsMatchingInterface().WithScopedLifetime())`. `AsMatchingInterface` registers the class as the interface named `I{TypeName}`, which is what the shared input needs ([IServiceTypeSelector.cs L55-L58](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/IServiceTypeSelector.cs#L55-L58); scanning idiom from the [README](https://github.com/khellang/Scrutor/blob/v7.0.0/README.md#scanning)) |
| Manual decoration | `ServiceCollectionDescriptorExtensions.Replace(services, ServiceDescriptor.Scoped<IService{i}>(sp => new Service{i}Decorator(new Service{i}(...))))`. The framework has no decoration API ([Replace](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.extensions.servicecollectiondescriptorextensions.replace)) |
| CseGenerated decoration | `services.AddS50DecoratorsServices()`, generated from `[Decorates]` |
| CseRuntime decoration | `ServiceCollectionDecorationExtensions.Decorate<IService{i}, Service{i}Decorator>(services)` × 10 |
| Scrutor decoration | `Scrutor`'s `Decorate<IService{i}, Service{i}Decorator>(services)` × 10 ([README](https://github.com/khellang/Scrutor/blob/v7.0.0/README.md#decoration)) |

Scrutor's attribute mode (`[ServiceDescriptor]` + `.UsingAttributes()`) has no column: it needs Scrutor's own attributes on the input classes, so the variants could no longer share identical inputs.

The numbers come from the reports committed under `benchmarks/results/di/{tfm}/results/` and `benchmarks/results/di/aot/aot-report.md`.

Environment:

| Property | Value |
|---|---|
| Machine | AMD Ryzen 5 5600, 6 cores / 12 threads (x64, RyuJIT AVX2) |
| OS | CachyOS, Linux kernel 7.3.0-4-cachyos |
| Machine load | Desktop session in the background. Run order net10 → net9 → net8 → net11. 1-minute load average 1.76 at the net10 start (5- and 15-minute averages 3.68 / 4.16, after a busier period), 1.37 net9, 1.19 net8, 1.09 net11, 1.15 at the end. The AOT script started at 1.72 (4.10 / 4.32) |
| Tool | BenchmarkDotNet v0.14.0, [MemoryDiagnoser]; default job for warm classes, ColdStart job (15 launches) for the cold class |
| .NET SDK | 11.0.100-rc.1.26425.128 |
| Runtimes tested | .NET 8.0.30, .NET 9.0.19, .NET 10.0.11, .NET 11.0.0 |
| Scrutor | 7.0.0 |
| Microsoft.Extensions.DependencyInjection | 10.0.0 (benchmark project) |
| CSharpEssentials.DependencyInjection (CSE) | 7.0.0 working tree, referenced as a project |

---

## Executive summary

Primary runtime: net10. A tie is a difference within the sum of the two BenchmarkDotNet error margins.

| Finding | Details |
|---|---|
| Cold startup: CseGenerated is 1.04-1.06× Manual (small CSE loss) | net10: 26.24 vs 24.88 ms at N=50, 57.83 vs 55.70 ms at N=500. Outside error on every TFM except net11 N=50 (tie). Allocation (2nd in-process invocation): 116.84 vs 117.07 KB (N=50), 1,019.41 vs 1,190.34 KB (N=500) |
| Cold startup: Scrutor takes 1.32× (N=50) and 1.10× (N=500) the CseGenerated time | net10: 34.75 vs 26.24 ms, 63.89 vs 57.83 ms; Scrutor allocation (2nd in-process invocation) 181.4 / 1,729.87 KB. Same 1.31-1.34× / 1.10-1.11× on every TFM. Scrutor's cold numbers include loading the CSE assembly, because the shared inputs carry CSE attributes (see Caveats) |
| CseReflection is slower than Scrutor and allocates more, cold and warm (CSE loss) | Cold net10: 38.21 vs 34.75 ms (1.10×) and 71.20 vs 63.89 ms (1.11×); allocation (2nd in-process invocation) 335.52 vs 181.4 KB and 3,195.63 vs 1,729.87 KB (1.85×). Warm net10: 108,702 vs 49,016 ns (2.22×) and 1,162,353 vs 505,048 ns (2.30×); 176,395 vs 49,370 B (3.57×). Slower than Scrutor on every TFM |
| Warm registration: CseGenerated vs Manual is mixed | net10: 763.58 vs 626.93 ns at N=50 (1.22×, CSE loss), 6,444.59 vs 6,375.29 ns at N=500 (tie). net11 N=50 1.15× (loss); net9 N=50 tie; CseGenerated is faster than Manual at N=500 on net8, net9 and net11 (0.91-0.96×) and at N=50 on net8. Allocation 4,464 vs 4,376 B (+88 B, the CSE marker descriptor) |
| Warm registration favours the reflection variants | After the first scan the types are loaded, reflection caches are filled and the scanning code is JIT-compiled. Even so, Scrutor takes 78× and CseReflection 173× the Manual time at N=50 on net10. Use the cold table for startup decisions |
| Decoration registration: every library is slower than manual factory replacement (CSE loss) | net10, 50 registrations + 10 decorations: Manual 952.7 ns / 4.9 KB, CseGenerated 3,572.7 ns / 10.83 KB (3.75×), CseRuntime 4,817.0 ns / 13.32 KB (5.06×), Scrutor 11,877.3 ns / 11.46 KB (12.47×) |
| CseRuntime decoration allocates more than Scrutor on every TFM, and is slower on net11 (CSE loss) | 13.01-14.26 KB vs 11.31-11.46 KB. net11: Scrutor 3,980.5 ns vs CseRuntime 4,581.9 ns (1.15×) and CseGenerated 3,459.0 ns. Scrutor drops from 11,877.3 ns (net10) to 3,980.5 ns (net11); not investigated |
| Plain resolve: ties on net10 and net11; net9 CseGenerated and net8 Scrutor differ | net10: 132.6 / 134.5 / 134.4 ns (Manual / CseGenerated / Scrutor), 360 B each. net9: CseGenerated 156.5 vs 147.7 ns (1.06×, outside error, CSE loss). net8: Scrutor 150.9 vs 146.4 ns (1.03×, outside error) |
| Decorated resolve: CSE paths slower than manual factory decoration (CSE loss) | net10: Manual 110.1 ns / 360 B, CseGenerated 164.6 ns / 360 B (1.49×), CseRuntime 252.2 ns / 392 B (2.29×), Scrutor 309.6 ns / 392 B (2.81×). CseGenerated is 1.45-1.69× Manual on every TFM. The manual factory builds the inner service with `new`; CSE and Scrutor resolve it from a hidden keyed registration (see Caveats) |
| Native AOT: CseGenerated and Manual publish with 0 IL warnings and run; Scrutor and CseReflection crash | Scrutor: 12 IL warnings, `DecorationException` at startup. CseReflection: 4 IL warnings, `InvalidOperationException` at startup; it is documented as not AOT-safe (`[RequiresUnreferencedCode]`/`[RequiresDynamicCode]`). No sample roots the input assemblies |
| Known CSE finding, not fixed here: the generated decoration path uses metadata reflection | `ServiceDecoration.TryDecorate` (`CSharpEssentials.DependencyInjection/Internal/ServiceDecoration.cs:20`) calls `EnsureDecoratable` (lines 90-108), which runs `GetConstructors()`, `GetParameters()` and `GetCustomAttribute<ServiceKeyAttribute>()` on the implementation type for every matched descriptor, also when called from the generated `[Decorates]` code. Each `Decorate` call also scans the whole collection. The Readme says the generated registry "uses no reflection" (Readme.MD, "Generated registry") and is "reflection-free" ("Migrating from Scrutor"). Precisely: activation is reflection-free and AOT-safe (0 warnings, sample runs), but decoration does metadata reflection. A follow-up is proposed |

---

## Results by scenario

### 1. Cold startup (fresh process: register + build + resolve every service once)

net10, mean of 15 launches. "vs Manual" divides by the Manual mean.

| Variant | N=50 mean | vs Manual | N=50 alloc (2nd invocation) | N=500 mean | vs Manual | N=500 alloc (2nd invocation) |
|---|---:|---:|---:|---:|---:|---:|
| Manual | 24.88 ms | 1.00 | 117.07 KB | 55.70 ms | 1.00 | 1,190.34 KB |
| CseGenerated | 26.24 ms | 1.06 | 116.84 KB | 57.83 ms | 1.04 | 1,019.41 KB |
| CseReflection | 38.21 ms | 1.54 | 335.52 KB | 71.20 ms | 1.28 | 3,195.63 KB |
| Scrutor | 34.75 ms | 1.40 | 181.4 KB | 63.89 ms | 1.15 | 1,729.87 KB |

- CseGenerated adds 1.36 ms (N=50) and 2.13 ms (N=500) over Manual; both outside error.
- CseReflection is the slowest variant at both sizes and allocates 2.7-2.9× Manual.
- CseGenerated allocates less than Manual at N=500 (0.86×; 0.90, 0.98 and 0.99× on net8, net9, net11). Observed, not explained.
- Scrutor's StdDev at N=50 is 1.79 ms against 0.28-0.71 ms for every other net10 cold row.
- The input assembly carries `[RegisterScoped]` and the CSE generated registry, so the Scrutor process also loads the CSE assembly; that cost is inside Scrutor's rows. Scrutor's attribute mode is not measured (it needs its own attributes on the inputs).
- The allocation columns come from a second invocation in the same process (see Caveats), not from the timed first call.

### 2. Warm registration (in-process, N services into a new collection)

net10. The BenchmarkDotNet Ratio column in the raw report is relative to the empty-collection Baseline (12.84 ns), so it is not quoted here. "vs Manual" below is recomputed from the means.

| Variant | N=50 mean | vs Manual | N=50 alloc | N=500 mean | vs Manual | N=500 alloc |
|---|---:|---:|---:|---:|---:|---:|
| Manual | 626.93 ns | 1.00 | 4,376 B | 6,375.29 ns | 1.00 | 40,416 B |
| CseGenerated | 763.58 ns | 1.22 | 4,464 B | 6,444.59 ns | 1.01 (tie) | 40,504 B |
| CseReflection | 108,702.31 ns | 173.4 | 176,395 B | 1,162,353.03 ns | 182.3 | 1,792,085 B |
| Scrutor | 49,016.03 ns | 78.2 | 49,370 B | 505,048.37 ns | 79.2 | 489,619 B |

Warm numbers favour the reflection rows: type loading, reflection caches and JIT of the scanning code are paid once by the self-check and the warmup and are outside this unit. A real application registers once per process, which is what section 1 measures. CseGenerated's extra 88 B (128 B on net8 and net9) is constant across N, consistent with the one marker descriptor CSE adds per registered assembly; not profiled.

### 3. Decoration (50 manual registrations, then decorate IService0..IService9)

net10. Every row includes the same 50 manual `AddScoped` calls (626.93 ns in section 2, measured separately).

| Variant | Mean | vs Manual | Allocated | vs Manual |
|---|---:|---:|---:|---:|
| Manual (`Replace` with a factory) | 952.7 ns | 1.00 | 4.9 KB | 1.00 |
| CseGenerated (`[Decorates]`) | 3,572.7 ns | 3.75 | 10.83 KB | 2.21 |
| CseRuntime (`Decorate<,>`) | 4,817.0 ns | 5.06 | 13.32 KB | 2.72 |
| Scrutor (`Decorate<,>`) | 11,877.3 ns | 12.47 | 11.46 KB | 2.34 |

- All three library paths move the original to a hidden registration and add a factory; the manual row only swaps one descriptor.
- CSE's `TryDecorate` walks the whole collection for each decorated service and reflects over the implementation's constructors for each match (see the executive summary). This is consistent with part of the CseGenerated and CseRuntime cost; not profiled (Scrutor also walks the collection).
- CseRuntime allocates more than Scrutor on every TFM. On net11 Scrutor (3,980.5 ns) is faster than CseRuntime (4,581.9 ns).

### 4. Resolve (one scope + one resolve, provider built in setup, N=50)

net10. Plain and Decorated resolve different services (IService15 with one dependency vs IService0 with none), so the two categories are not comparable with each other.

| Category | Variant | Mean | vs Manual | Allocated |
|---|---|---:|---:|---:|
| Plain | Manual | 132.6 ns | 1.00 | 360 B |
| Plain | CseGenerated | 134.5 ns | 1.01 (tie) | 360 B |
| Plain | Scrutor | 134.4 ns | 1.01 (tie) | 360 B |
| Decorated | Manual (factory) | 110.1 ns | 1.00 | 360 B |
| Decorated | CseGenerated | 164.6 ns | 1.49 | 360 B |
| Decorated | CseRuntime | 252.2 ns | 2.29 | 392 B |
| Decorated | Scrutor | 309.6 ns | 2.81 | 392 B |

- Plain: every variant leaves the same descriptors, so the rows should tie; they do on net10 and net11 (net9: CseGenerated 1.06×; net8: Scrutor 1.03×; see Cross-TFM).
- Decorated: CseGenerated's factory calls the decorator constructor directly; CseRuntime calls `ConstructorInfo.Invoke` (`Internal/DecoratorActivator.cs:50`); Scrutor uses `ActivatorUtilities.CreateInstance`. CseGenerated is 1.88× faster than Scrutor and allocates 32 B less.
- CseRuntime on net10 (252.2 ns) is above its net8, net9 and net11 values (203.0-220.3 ns). net10 ran first, at the highest load.

### 5. Native AOT (net10, linux-x64, N=50 + 10 decorators)

> Script-driven, not BenchmarkDotNet. `dotnet publish -c Release -r linux-x64 -p:TreatWarningsAsErrors=false -p:TrimmerSingleWarn=false` from a clean obj. `TrimmerSingleWarn=false` lists each warning per member, so the counts are higher than a default publish shows. No sample has a `TrimmerRootAssembly`, so reflection-based variants see only what the trimmer kept.

| Variant | AOT publish | IL warnings (distinct) | Native binary | Exit code | All 50 services resolved with the right values |
|---|---|---:|---:|---:|---|
| ManualAot | yes | 0 | 2.2 MB | 0 | yes (`resolved 50 checksum 11225 ok`) |
| CseGeneratedAot | yes | 0 | 2.2 MB | 0 | yes (`resolved 50 checksum 11225 ok`) |
| CseReflectionAot | yes | 4 (IL2026 ×2, IL3050 ×2; 2 analyzer, 2 ILC) | 2.4 MB [1] | 134 | no: crashed |
| ScrutorAot | yes | 12 (IL2026 ×1, IL2055 ×2, IL2067 ×1, IL2070 ×4, IL2072 ×2, IL3050 ×2; all ILC) | 2.3 MB [1] | 134 | no: crashed |

[1] The binary does not register or resolve every service, so its size is not comparable with the working variants.

- CseReflection: `InvalidOperationException: Decorator '...Service0Decorator' must have exactly one public constructor, but has 0.` The trimmer removed the constructor that the reflection fallback looks up. The warnings point at the `AddServicesFromAssemblies` call site, which is annotated `[RequiresUnreferencedCode]`/`[RequiresDynamicCode]`; the Readme says to use the generated method for AOT.
- Scrutor: `Scrutor.DecorationException: Could not find any registered services for type '...IService0'.` The scan found no classes after trimming. The warnings come from inside Scrutor (`GetTypes`, `GetInterfaces`, `MakeGenericType`, `ActivatorUtilities.CreateInstance`). Whether rooting the input assembly makes it run was not tested.
- Wall time was 3 ms for both working variants (median of 5). The run started at load 1.72 (5-minute average 4.10), so this is not a startup result.

---

## Cross-TFM

Mean (and allocation where it differs) per TFM. Run order net10 → net9 → net8 → net11.

| Scenario | Variant | net8 | net9 | net10 | net11 |
|---|---|---|---|---|---|
| Cold N=50 | Manual | 22.18 ms | 21.90 ms | 24.88 ms | 26.24 ms |
| | CseGenerated | 23.47 ms | 22.95 ms | 26.24 ms | 27.55 ms (tie) |
| | CseReflection | 35.76 ms | 33.73 ms | 38.21 ms | 39.84 ms |
| | Scrutor | 31.48 ms | 30.12 ms | 34.75 ms | 36.05 ms |
| Cold N=500 | Manual | 49.24 ms / 1,187 KB | 47.51 ms / 1,091 KB | 55.70 ms / 1,190 KB | 60.65 ms / 1,087 KB |
| | CseGenerated | 51.63 ms / 1,065 KB | 49.59 ms / 1,064 KB | 57.83 ms / 1,019 KB | 63.34 ms / 1,075 KB |
| | CseReflection | 66.47 ms / 3,872 KB | 61.98 ms / 3,284 KB | 71.20 ms / 3,196 KB | 76.43 ms / 3,175 KB |
| | Scrutor | 57.32 ms / 1,723 KB | 55.01 ms / 1,818 KB | 63.89 ms / 1,730 KB | 69.47 ms / 1,814 KB |
| Warm N=50 | Manual | 813.77 ns | 829.77 ns | 626.93 ns | 578.54 ns |
| | CseGenerated | 783.88 ns | 824.09 ns (tie) | 763.58 ns | 663.32 ns |
| | CseReflection | 280,865 ns | 135,002 ns | 108,702 ns | 103,959 ns |
| | Scrutor | 61,056 ns | 61,700 ns | 49,016 ns | 49,465 ns |
| Warm N=500 | Manual | 7,604.84 ns | 7,568.25 ns | 6,375.29 ns | 6,234.48 ns |
| | CseGenerated | 6,902.01 ns | 6,899.45 ns | 6,444.59 ns (tie) | 5,970.93 ns |
| | CseReflection | 3,014,905 ns | 1,422,668 ns | 1,162,353 ns | 1,140,328 ns |
| | Scrutor | 647,772 ns | 594,832 ns | 505,048 ns | 509,275 ns |
| Decoration | Manual | 1,148 ns / 4.9 KB | 1,187 ns / 4.9 KB | 952.7 ns / 4.9 KB | 879.0 ns / 4.9 KB |
| | CseGenerated | 4,349 ns / 10.9 KB | 4,377 ns / 10.9 KB | 3,572.7 ns / 10.83 KB | 3,459.0 ns / 10.6 KB |
| | CseRuntime | 6,025 ns / 14.26 KB | 5,601 ns / 13.95 KB | 4,817.0 ns / 13.32 KB | 4,581.9 ns / 13.01 KB |
| | Scrutor | 13,108 ns / 11.46 KB | 12,500 ns / 11.46 KB | 11,877.3 ns / 11.46 KB | 3,980.5 ns / 11.31 KB |
| Plain resolve | Manual | 146.4 ns | 147.7 ns | 132.6 ns | 141.1 ns |
| | CseGenerated | 147.3 ns (tie) | 156.5 ns | 134.5 ns (tie) | 140.0 ns (tie) |
| | Scrutor | 150.9 ns (outside error) | 146.7 ns | 134.4 ns | 140.4 ns |
| Decorated resolve | Manual | 126.8 ns | 123.5 ns | 110.1 ns | 109.8 ns |
| | CseGenerated | 190.6 ns | 178.5 ns | 164.6 ns | 185.6 ns |
| | CseRuntime | 220.3 ns | 213.7 ns | 252.2 ns | 203.0 ns |
| | Scrutor | 350.0 ns | 329.0 ns | 309.6 ns | 310.6 ns |

- The ordering holds on every TFM for cold startup (Manual < CseGenerated < Scrutor < CseReflection) and decorated resolve (Manual < CseGenerated < CseRuntime < Scrutor).
- net8 warm CseReflection (280.9 µs at N=50, 3.01 ms at N=500) is 2.1-2.7× its net9-net11 values (2.1× net9, 2.6-2.7× net10/net11). Not investigated; treat it as an outlier.
- net11 decoration: Scrutor drops to 3,980.5 ns (11,877-13,108 ns elsewhere) and beats CseRuntime. Not investigated.
- Allocation is flat across TFMs for warm registration (4,376-4,504 B at N=50 for Manual and CseGenerated) and resolve (360 / 392 B).

---

## Pending: TryAdd and transient decorator resolve

**Status: draft.** These two classes build on net8.0 to net11.0, and their parity self-check passes on every TFM. No full default-job run has been done, so this section has no numbers yet. The reports will be committed under `benchmarks/results/di/{tfm}/results/` next to the ones above. Short-job (`--quick`) numbers are not published.

The rules are the same as for the classes above: `[MemoryDiagnoser]`, the hand-written Microsoft.Extensions.DependencyInjection code as the baseline, a doc URL above every Scrutor and framework row, and a `[GlobalSetup]` parity check that `--self-check` also runs. Scrutor is pinned to 7.0.0, the latest stable release on NuGet on 2026-11-04 (MIT).

| Class | Measured unit | Rows | Label |
|---|---|---|---|
| `TryAddBenchmarks` | Register N scoped services with TryAdd semantics into a new collection, warm and in-process. `N` = 50, 500 or 2000 | ManualTryAdd (baseline, `TryAddScoped` per service), CseGenerated (`[RegisterScoped(Strategy = RegistrationStrategy.TryAdd)]` and the generated registry), CseReflection (`AddServicesFromAssemblies`), Scrutor (`UsingRegistrationStrategy(RegistrationStrategy.Skip)`), ManualAdd (`AddScoped` per service) | The four TryAdd rows are like-for-like. ManualAdd is a reference only: it has no duplicate check |
| `TransientDecoratorBenchmarks` | One resolve of a decorated transient `IService0` from the root provider, built once in setup | Manual (baseline, a hand-written `Replace` factory), CseGenerated (`[Decorates]`), CseRuntime (`Decorate<TService, TDecorator>`), Scrutor (`Decorate<TService, TDecorator>`) | Like-for-like: same services, decorators, lifetime and resolve |

Inputs: `TryAddBenchmarks` uses three new generated libraries, T50, T500 and T2000, from `generate-services.py`. They hold the same interfaces, classes and dependency rule as S50 and S500. The only change is the `Strategy = RegistrationStrategy.TryAdd` attribute property. The S50 and S500 output of the script did not change. `TransientDecoratorBenchmarks` registers S50's IService0 to IService9 as transient by hand, then decorates all ten with the S50.Decorators classes.

Methodology:
- On an empty collection every TryAdd row scans the collection once per added descriptor, so all four are O(N^2). Microsoft `TryAdd` loops the collection, Scrutor `Skip` calls Microsoft `TryAdd` ([RegistrationStrategy.cs L47-L50](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/RegistrationStrategy.cs#L47-L50)), and CSE loops it in `ServiceRegistration.FindFirst` (`ServiceRegistration.cs:33-35,119-130`). N = 2000 is there to show that growth. It is shared by all three libraries, not a CSE cost.
- Warm is not like-for-like for the reflection rows, as in section 2: CseReflection and Scrutor look cheaper here than in a real application, which registers once per process.
- `ResolveBenchmarks` resolves scoped decorated services in a new scope per operation. `TransientDecoratorBenchmarks` drops the scope and uses transients, so every resolve activates the decorator and its inner service again. The services are not disposable, so the root provider does not track them.

What the parity check enforces:
- TryAdd: for every variant and N, the collection holds exactly N scoped type registrations of the T{N} types, plus one CSE marker for the two CSE rows, and a scope resolves `0..N-1`. Separately, each of the four TryAdd variants runs over a collection that already holds `IService0`, and `IService0` must still have exactly one descriptor.
- Transient decoration: in every variant IService0 to IService9 resolve to 1000 + i, the last non-keyed descriptor of each is transient, and two resolves of IService0 return different instances.

Semantic differences:
- The manual decoration factory builds the inner service with `new`. CSE and Scrutor resolve the inner service from a hidden keyed registration. Part of the gap is that extra resolve, as in section 4.
- CseGenerated calls the decorator constructor from generated code. CseRuntime builds the decorator with `ConstructorInfo.Invoke` and a new argument array per resolve (`Internal/DecoratorActivator.cs:42-51`), after the keyed inner lookup (`Internal/ServiceDecoration.cs:119`). Scrutor uses `ActivatorUtilities.CreateInstance` per resolve ([DecorationStrategy.cs L37](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/DecorationStrategy.cs#L37)).

Feature matrix for these classes, from reading each library's source and docs (not measured):

| Feature | CSharpEssentials.DependencyInjection | Scrutor 7.0.0 | Manual Microsoft.Extensions.DependencyInjection |
|---|---|---|---|
| TryAdd cost model | A linear scan per descriptor in `FindFirst` | `Skip` calls Microsoft `TryAdd`, a linear scan per descriptor | `TryAdd` loops the collection per descriptor |
| Transient decorator activation per resolve | Generated: a direct constructor call in a lambda. Runtime: `ConstructorInfo.Invoke`, an argument array and a keyed inner lookup | `ActivatorUtilities.CreateInstance` | A factory lambda |

---

## Feature matrix

CSE cells come from this repository (csproj, `Readme.MD`, `ServiceSourceWriter.cs`, `Internal/*.cs`). Scrutor links point at tag [`v7.0.0`](https://github.com/khellang/Scrutor/tree/v7.0.0) (commit `7f315da`, matches the 7.0.0 nuspec). Framework links point at Learn or `dotnet/runtime` tag `v10.0.0`. The "measured" row comes from section 5; every other row comes from reading source or docs, not from running code.

| Feature | CSharpEssentials.DependencyInjection 7.0.0 | Scrutor 7.0.0 | Manual Microsoft.Extensions.DependencyInjection |
|---|---|---|---|
| Runtime reflection: registration | Generated path: none; emits `new ServiceDescriptor(typeof(S), key, typeof(I), lifetime)`. Opt-in `AddServicesFromAssemblies` uses `Assembly.GetTypes()` (`Internal/AssemblyServiceScanner.cs:93`) | Yes: `Assembly.GetTypes()` ([ReflectionExtensions.cs L226](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/ReflectionExtensions.cs#L226)), `MakeGenericType` ([L276](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/ReflectionExtensions.cs#L276)), attribute reads ([AttributeSelector.cs](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/AttributeSelector.cs)) | None for registration. The default provider activates services by reflection / IL emit in all three columns ([ILEmitBackend in the csproj](https://github.com/dotnet/runtime/blob/v10.0.0/src/libraries/Microsoft.Extensions.DependencyInjection/src/Microsoft.Extensions.DependencyInjection.csproj)) |
| Runtime reflection: decoration | Generated `[Decorates]` and factory `Decorate<T>(Func<...>)`: activation is a direct constructor call, but `EnsureDecoratable` runs `GetConstructors`/`GetParameters`/`GetCustomAttribute<ServiceKeyAttribute>` per matched descriptor (`Internal/ServiceDecoration.cs:90-108`); 0 AOT warnings. Runtime `Decorate<TService,TDecorator>`: `GetConstructors` + constructor invoke (`Internal/DecoratorActivator.cs`). Open generic: `MakeGenericType` | Type decorators: `ActivatorUtilities.CreateInstance` ([DecorationStrategy.cs L37](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/DecorationStrategy.cs#L37)); open generic `MakeGenericType` ([OpenGenericDecorationStrategy.cs](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/OpenGenericDecorationStrategy.cs)) | No decoration API ([registration methods](https://learn.microsoft.com/dotnet/core/extensions/dependency-injection/service-registration#registration-methods)); a hand-written factory needs no reflection |
| Source generator | Yes, bundled with analyzers and a code fix | No: the [7.0.0 package](https://www.nuget.org/packages/Scrutor/7.0.0) has no `analyzers/` folder | N/A |
| `IsAotCompatible` / trim annotations | `IsAotCompatible=true` on net8.0+ (not netstandard2.1). `[RequiresUnreferencedCode]`+`[RequiresDynamicCode]` on `AddServicesFromAssemblies` and `Decorate(Type, Type)`; `[DynamicallyAccessedMembers]` on `Decorate<TService,TDecorator>` | None: no flag in [Scrutor.csproj](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/Scrutor.csproj), no trim attributes in `src/Scrutor/` | Registration APIs carry `[DynamicallyAccessedMembers(PublicConstructors)]` ([ServiceCollectionServiceExtensions.cs L68](https://github.com/dotnet/runtime/blob/v10.0.0/src/libraries/Microsoft.Extensions.DependencyInjection.Abstractions/src/ServiceCollectionServiceExtensions.cs#L68)) |
| Native AOT publish without warnings, and runs (measured) | Generated: yes, 0 warnings, runs. Reflection fallback: 4 warnings, crashes | No: 12 warnings (`TrimmerSingleWarn=false`), crashes | Yes: 0 warnings, runs |
| Extra runtime dependencies | `Microsoft.Extensions.DependencyInjection.Abstractions` and `Microsoft.Extensions.Logging.Abstractions`, both `[9.0.4,)` | `Microsoft.Extensions.DependencyInjection.Abstractions` 10.0.0 and `Microsoft.Extensions.DependencyModel` 10.0.0 on every TFM ([nuspec](https://www.nuget.org/api/v2/package/Scrutor/7.0.0)) | None beyond the DI packages |
| Convention-based scanning | No "register every class" scan. Default service type rule only: `I{TypeName}`, else self | Yes: `Scan(...).AddClasses(...).AsMatchingInterface()`, namespace/attribute filters ([README](https://github.com/khellang/Scrutor/blob/v7.0.0/README.md#scanning)) | No |
| Attribute-based registration | Primary model: `[RegisterScoped]`/`[RegisterSingleton]`/`[RegisterTransient]` with `Key`, `As`, `Strategy`; `[ExcludeFromRegistration]` | Opt-in `[ServiceDescriptor]` + `.UsingAttributes()`, still a runtime scan ([ServiceDescriptorAttribute.cs](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/ServiceDescriptorAttribute.cs)) | No |
| Keyed services: registration | Yes, `Key = <constant>` | Yes, `WithServiceKey(...)` ([ILifetimeSelector.cs L37-43](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/ILifetimeSelector.cs#L37-L43)) and `ServiceDescriptorAttribute.ServiceKey` | Yes, `AddKeyed{Lifetime}` ([keyed services](https://learn.microsoft.com/dotnet/core/extensions/dependency-injection#keyed-services)) |
| Keyed services: decoration | Yes: `Decorate<,>(serviceKey: ...)` and `[Decorates(..., Key = ...)]`; `[ServiceKey]`/`[FromKeyedServices]` honoured on the decorator constructor; a decorated implementation that injects `[ServiceKey]` is rejected (`Internal/ServiceDecoration.cs:90-107`) | Not through the public `Decorate`/`TryDecorate` overloads: they pass `serviceKey: null` ([Decoration.cs L262](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/ServiceCollectionExtensions.Decoration.cs#L262)) and matching requires equal keys ([DecorationStrategy.cs L18-20](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/DecorationStrategy.cs#L18-L20)); a custom strategy works for `string` keys only (source reading, not run) | No API; re-register with a keyed factory |
| Open-generic decoration | Runtime `Decorate(typeof(IRepo<>), typeof(CachedRepo<>))` over closed registrations; throws if an open-generic registration exists; not generated from attributes (CSE2008) | Closed registrations only; open definitions excluded ([OpenGenericDecorationStrategy.cs L17-21](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/OpenGenericDecorationStrategy.cs#L17-L21)) | No |
| Registration strategies, key-aware | `Add`, `TryAdd`, `TryAddEnumerable`, `Replace`, `Throw`; all match service type + key | `Append`, `Skip`, `Throw`, `Replace` ([RegistrationStrategy.cs](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/RegistrationStrategy.cs)). `Skip` is key-aware (MS `TryAdd`); `Throw` and `Replace` ignore the key ([ServiceCollectionExtensions.cs L9-12](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/ServiceCollectionExtensions.cs#L9-L12), [RegistrationStrategy.cs L70-105](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/RegistrationStrategy.cs#L70-L105)); no `TryAddEnumerable` | `TryAdd*`, `TryAddEnumerable`, `Replace`, `RemoveAll` ([docs](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.extensions.servicecollectiondescriptorextensions)); `TryAdd`/`Replace` compare type + key; no throw-on-duplicate |
| Compile-time diagnostics | 9 analyzer rules CSE2001-CSE2009 (missing interface, `Throw` conflict, bad decorator constructor, captive dependency, ...) + a code fix | None; errors at runtime (`DecorationException`, `DuplicateTypeRegistrationException`, `MissingTypeRegistrationException` in [src/Scrutor](https://github.com/khellang/Scrutor/tree/v7.0.0/src/Scrutor)) | None from the library; runtime `ValidateScopes` / `ValidateOnBuild` ([ServiceProviderOptions](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.serviceprovideroptions)) |
| DI registration required | No setup call: `services.Add{Assembly}Services()` or `AddAllServices()` | No setup call: `services.Scan(...)` ([README](https://github.com/khellang/Scrutor/blob/v7.0.0/README.md#scanning)) | N/A |

---

## Caveats

- Warm registration is not like-for-like. The reflection variants have loaded types, filled reflection caches and JIT-compiled their scanning code before the measured unit. Their warm numbers are lower bounds; the cold table is the startup comparison.
- The shared inputs carry CSE attributes. The input classes have `[RegisterScoped]` and the input assembly contains the CSE generated registry, so the Scrutor process also loads the CSE assembly. That small cost is inside Scrutor's cold numbers.
- There is no Scrutor attribute column. Scrutor's `[ServiceDescriptor]` mode would need different attributes on the shared input classes. It was not measured.
- Scrutor is configured with `AsMatchingInterface()` (cited from [IServiceTypeSelector.cs L55-L58](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/IServiceTypeSelector.cs#L55-L58)), which produces the same descriptors as the other variants; the self-check verifies it. `AsImplementedInterfaces()` or `AsSelfWithInterfaces()` would register more descriptors and were not measured.
- The manual decoration baseline does less work. `ManualDecorationS50` builds the inner service with `new Service{i}()` inside the factory; CSE and Scrutor resolve the inner service from a hidden keyed registration. Part of the decorated-resolve gap is that extra container resolve. It is still the gap a user sees when moving from a hand-written factory to a decoration library.
- Plain and Decorated resolve rows measure different services (IService15 with a dependency, IService0 without). Compare rows only within a category.
- The generated decoration path uses reflection. `EnsureDecoratable` (`Internal/ServiceDecoration.cs:90-108`) runs `GetConstructors()`/`GetParameters()`/`GetCustomAttribute<ServiceKeyAttribute>()` for every matched descriptor, and each `Decorate` call loops over the whole collection, also for `[Decorates]` code. This is metadata reflection only: no dynamic code, 0 AOT warnings, and the AOT sample runs. It contradicts the Readme wording "uses no reflection" ("Generated registry" section) and "reflection-free registry" ("Migrating from Scrutor"). Not fixed in this work; a follow-up is proposed (decide `[ServiceKey]` use at generation time and skip the check on the generated path, or correct the Readme).
- AOT warning counts use `TrimmerSingleWarn=false` and are higher than a default publish shows. No sample uses `TrimmerRootAssembly`; rooting the input assemblies might change the Scrutor and CseReflection outcome and was not tested. AOT wall times (3 ms) were taken on a loaded machine and are not a result.
- Cold allocation is not first-call allocation. In each launch BenchmarkDotNet 0.14 runs the unit a second time in the same process, after the timed invocation, to collect GC stats (`// GC:` line after `// AfterActualRun` in the run log). The allocation column therefore excludes JIT, type loading and first-call reflection-cache allocations; it is the managed allocation of one register + build + resolve-all in an already-warm process.
- net10, the primary runtime, ran first at the highest load (1-minute 1.76; 5-minute 3.68). CseRuntime decorated resolve on net10 (252.2 ns) is above its other TFMs (203.0-220.3 ns). Gaps of a few percent are weak evidence even when outside error.
- These outliers were not investigated: net8 warm CseReflection is 2.1-2.7× its other TFMs; net11 Scrutor decoration is about 3× faster than on the other TFMs; net9 plain resolve has CseGenerated 1.06× Manual; net8 plain resolve has Scrutor 1.03× Manual.
- A difference within the sum of the two error margins is reported as a tie.
- The CSE column is the 7.0.0 working tree on branch `perf/bench-di`, which may differ from the last version published to NuGet.

---

## How to reproduce

```bash
cd benchmarks
./run-benchmarks.sh --project di                       # all TFMs (net8-net11), every class, committed configuration
./run-benchmarks.sh --project di --tfm net10.0         # one TFM
./run-benchmarks.sh --project di --quick --tfm net10.0 # one TFM, Short job
./di-aot/run-di-aot.sh                                 # Native AOT report (all four samples)
python3 CSharpEssentials.DependencyInjection.Benchmarks/generate-services.py   # regenerate the S50/S500 and T50/T500/T2000 input libraries
```

`--quick` without `--tfm` runs on net9.0, the script default for this suite. Its patterns are `*Registration*` (which also matches `ColdRegistrationBenchmarks`), `*Decoration*`, `*Resolve*`, `*.TryAddBenchmarks.*` and `*.TransientDecoratorBenchmarks.*`; the Short job's numbers are not comparable with the committed ones. Results are stored per framework under `benchmarks/results/di/{tfm}/results/` (GitHub Markdown, CSV, JSON and HTML). The AOT report is `benchmarks/results/di/aot/aot-report.md`; raw publish and run logs go to `benchmarks/results/di/aot/logs/`.
