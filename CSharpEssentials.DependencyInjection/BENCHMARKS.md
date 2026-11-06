# CSharpEssentials.DependencyInjection: Performance benchmarks

Compared with Scrutor 7.0.0 and manual Microsoft.Extensions.DependencyInjection registration, measured with BenchmarkDotNet v0.14.0 on an AMD Ryzen 5 5600 under .NET 8, 9, 10 and 11.

---

## What was measured

Six BenchmarkDotNet classes in `benchmarks/CSharpEssentials.DependencyInjection.Benchmarks/` plus a Native AOT script. Every class uses `[MemoryDiagnoser]`. A self-check (`DiSelfCheck`) runs before the measurements and fails unless every variant registers the same (service type, implementation type, lifetime) set and every service resolves to its own implementation; for decoration it requires exactly IService0..IService9 to be wrapped once.

| # | Class | Measured unit | Columns |
|---|---|---|---|
| 1 | `ColdRegistrationBenchmarks` | A fresh process per sample (`RunStrategy.ColdStart`, 15 launches, 1 iteration, no warmup): new `ServiceCollection`, registration, `BuildServiceProvider`, one scope, one resolve of every service, dispose. JIT, assembly loading, reflection scanning and the container's first-resolve work are inside the measurement. This is the like-for-like startup number. N = 50, 500 | Manual, CseGenerated, CseReflection, Scrutor |
| 2 | `RegistrationBenchmarks` | New `ServiceCollection` + registration of N services, warm and in-process. Baseline is the empty collection. N = 50, 500 | Baseline, Manual, CseGenerated, CseReflection, Scrutor |
| 3 | `DecorationBenchmarks` | New collection, the 50 services registered by hand, then 10 of them (IService0..IService9) decorated, warm. The unit includes the 50 manual registrations for every column | Manual, CseGenerated, CseRuntime, Scrutor |
| 4 | `ResolveBenchmarks` | Provider built once per variant in setup (N = 50); per operation: create a scope, resolve one service, dispose. *Plain* resolves IService15 (depends on IService14) from each library's own registration. *Decorated* resolves IService0 (no dependency) after decorating IService0..IService9 on top of the manual registration | Plain: Manual, CseGenerated, Scrutor. Decorated: Manual, CseGenerated, CseRuntime, Scrutor |
| 5 | Native AOT (`benchmarks/di-aot/run-di-aot.sh`, not BenchmarkDotNet) | One sample per variant, `PublishAot=true`, net10, linux-x64: register S50, decorate 10, build, resolve all 50, check values | Manual, CseGenerated, CseReflection, Scrutor |
| 6 | `TryAddBenchmarks` | Register N scoped services with TryAdd semantics into a new collection, warm and in-process. N = 50, 500, 2000 | ManualTryAdd (baseline, `TryAddScoped` per service), CseGenerated (`[RegisterScoped(Strategy = RegistrationStrategy.TryAdd)]` and the generated registry), CseReflection (`AddServicesFromAssemblies`), Scrutor (`UsingRegistrationStrategy(RegistrationStrategy.Skip)`), ManualAdd (`AddScoped` per service, reference only: no duplicate check) |
| 7 | `TransientDecoratorBenchmarks` | One resolve of a decorated transient `IService0` from the root provider, built once in setup | Manual (baseline, a hand-written `Replace` factory), CseGenerated (`[Decorates]`), CseRuntime (`Decorate<TService, TDecorator>`), Scrutor (`Decorate<TService, TDecorator>`) |

Input: `generate-services.py` writes one class library per N (`S50`, `S500`) shared by every variant. It holds N pairs `IService{i}` / `[RegisterScoped] Service{i}`; every service whose index is a positive multiple of 5 takes `IService{i-1}` as a constructor dependency. It also holds `ManualS{N}.Register` (hand-written `AddScoped<IService{i}, Service{i}>()` calls). A separate `S{N}.Decorators` library holds `Service{i}Decorator` for i < 10 with `[Decorates(typeof(IService{i}))]`, and `ManualDecorationS{N}.Apply`. `TryAddBenchmarks` uses three more generated libraries, T50, T500 and T2000, with the same interfaces, classes and dependency rule; the only change is `Strategy = RegistrationStrategy.TryAdd` on the attribute. `TransientDecoratorBenchmarks` registers S50's IService0 to IService9 as transient by hand, then decorates all ten with the S50.Decorators classes.

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

### How these numbers were produced

- Full BenchmarkDotNet runs (no `--quick`), one TFM after another on one machine, in the order net8.0, net9.0, net10.0, net11.0. Warm classes use the BenchmarkDotNet default job. `ColdRegistrationBenchmarks` uses its own ColdStart job (15 launches, 1 iteration, no warmup).
- 1-minute load average 1.55 at the start of the suite and 1.90 at the end (only those two readings exist). No row in this suite had a coefficient of variation above 5% after the run, so no row is marked noisy.
- Reports: `benchmarks/results/di/{tfm}/results/*-report-github.md` (also `.csv`, `.html` and JSON), for example [net10.0 ColdRegistrationBenchmarks](../benchmarks/results/di/net10.0/results/CSharpEssentials.DependencyInjection.Benchmarks.ColdRegistrationBenchmarks-report-github.md).
- The Native AOT table (section 5) is not part of this run. It comes from `benchmarks/results/di/aot/aot-report.md`, a separate script run on 2026-11-02 at a 1-minute load average of 1.72.

Environment:

| Property | Value |
|---|---|
| Machine | AMD Ryzen 5 5600, 6 cores / 12 threads (x64, RyuJIT AVX2) |
| OS | CachyOS, Linux kernel 7.3.0-4-cachyos |
| Machine load | Desktop session in the background; 1-minute load average 1.55 at the suite start, 1.90 at the end |
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
| Cold startup: CseGenerated ties Manual on net10 and net11, and loses by 1.05-1.06× on net8 and net9 | net10: 26.44 vs 25.39 ms at N=50 (1.04, tie), 59.90 vs 56.89 ms at N=500 (1.05, tie). net8: 23.76 vs 22.38 ms (1.06) and 52.65 vs 50.20 ms (1.05), outside error. Allocation (2nd in-process invocation) net10: 116.84 vs 117.4 KB (N=50), 1019.41 vs 1094.34 KB (N=500). net11 N=500: CseGenerated allocates more than Manual, 1171.02 vs 1087.08 KB (1.08); net11 N=50: 116.78 vs 116.65 KB (Alloc Ratio 1.00) |
| Cold startup: Scrutor takes 1.35-1.43× Manual at N=50 and 1.13-1.16× at N=500 | net10: Scrutor 35.37 ms vs CseGenerated 26.44 ms (N=50), 65.75 vs 59.90 ms (N=500). Scrutor allocation (2nd in-process invocation) 181.77 / 1825.91 KB. Scrutor's cold numbers include loading the CSE assembly, because the shared inputs carry CSE attributes (see Caveats) |
| CSE loss: CseReflection is slower than Scrutor and allocates more, cold and warm, on every TFM | Cold net10: 39.29 vs 35.37 ms and 72.52 vs 65.75 ms; allocation 335.8 vs 181.77 KB and 3291.73 vs 1825.91 KB. Warm net10: 108,035.06 vs 52,844.93 ns (2.04×) and 1,171,299.79 vs 511,994.57 ns (2.29×); 176395 vs 49370 B (3.57×) and 1771567 vs 489616 B (3.62×). net8 warm: 4.86× and 4.98× Scrutor |
| Warm registration: CseGenerated vs Manual is mixed | CSE loss at N=50 on net10 (782.79 vs 667.47 ns, 1.17×) and net11 (713.26 vs 613.54 ns, 1.16×). CseGenerated is faster than Manual at N=50 on net8 and net9 and at N=500 on every TFM (0.93-0.97×). Allocation 4464 vs 4376 B on net10 and net11, 4504 vs 4376 B on net8 and net9 (the CSE marker descriptor) |
| Warm registration favours the reflection variants | After the first scan the types are loaded, reflection caches are filled and the scanning code is JIT-compiled. Even so, on net10 Scrutor takes 79.2× and CseReflection 161.9× the Manual time at N=50. Use the cold table for startup decisions |
| CSE loss: decoration registration is slower than manual factory replacement | net10, 50 registrations + 10 decorations: Manual 1.010 μs / 4.9 KB, CseGenerated 2.137 μs / 7.09 KB (2.12×), CseRuntime 3.732 μs / 10.99 KB (3.70×), Scrutor 12.217 μs / 11.46 KB (12.10×). CseGenerated is 2.12-2.25× Manual on every TFM |
| Decoration registration: both CSE paths are faster than Scrutor and allocate less on every TFM | CseRuntime / Scrutor time ratio 0.31-0.36 on net8-net10 and 0.83 on net11 (3,512.7 vs 4,253.0 ns). Allocation 10.99 vs 11.46 KB (net8-net10), 10.76 vs 11.31 KB (net11). Scrutor drops from 12.217 μs (net10) to 4,253.0 ns (net11); not investigated |
| Plain resolve: CSE loss on net10, ties on net8 and net11 (CseGenerated) | net10: Manual 137.9 ns, CseGenerated 147.6 ns (1.07), Scrutor 145.8 ns (1.06), both outside error, 360 B each. net9: CseGenerated 153.7 ns vs Manual 158.2 ns (0.97). net8: all three tie. CSE loss on net11: CseGenerated 147.7 ns vs Scrutor 144.9 ns, outside error (CseGenerated ties Manual 148.5 ns there) |
| CSE loss: decorated resolve is slower than manual factory decoration | net10: Manual 115.2 ns, CseGenerated 162.5 ns (1.41), CseRuntime 178.8 ns (1.55), Scrutor 323.0 ns (2.80). CseGenerated is 1.28-1.69× Manual across TFMs. CSE allocates 360 B like Manual; Scrutor 392 B. On net11 CseRuntime (184.1 ns) is faster than CseGenerated (193.8 ns) |
| CSE loss: transient decorator resolve is slower than the manual factory | net10: Manual 24.73 ns, CseGenerated 38.39 ns (1.55), CseRuntime 55.34 ns (2.24), Scrutor 163.95 ns (6.63). CSE allocates 48 B like Manual; Scrutor 80 B. CseGenerated is 1.45-1.72× Manual across TFMs |
| CSE loss: TryAdd registration, CseGenerated is slower than hand-written `TryAddScoped` on every TFM and N | net10: 1.38× (N=50), 1.43× (N=500), 1.448× (N=2000); range 1.10-1.58× across TFMs. Allocation 1.00-1.03×. CseGenerated is faster than Scrutor `Skip` (0.06× at N=50, 0.70× at N=2000 on net10) |
| CSE loss: TryAdd, CseReflection is the slowest row on every TFM and N | net10: 204,885.2 vs Scrutor 54,055.4 ns at N=50 (3.79×), 16,089,884.4 vs 6,624,435.2 ns at N=2000 (2.43×). net8 N=50: 622,921.4 vs 62,597.2 ns (9.95×), 357.84 vs 50.18 KB |
| Native AOT (separate 2026-11-02 run): CseGenerated and Manual publish with 0 IL warnings and run; Scrutor and CseReflection crash | Scrutor: 12 IL warnings, `DecorationException` at startup. CseReflection: 4 IL warnings, `InvalidOperationException` at startup; it is documented as not AOT-safe (`[RequiresUnreferencedCode]`/`[RequiresDynamicCode]`). No sample roots the input assemblies |
| Generated decoration still reads constructor metadata, once per type | Generated `[Decorates]` code calls `ServiceRegistration.Decorate(services, DecoratorRegistration[])`, which runs `ServiceDecoration.DecorateAll` (one pass over the collection for all decorators). `EnsureDecoratable` (`Internal/ServiceDecoration.cs:196-215`) reads the implementation's constructors to reject `[ServiceKey]` injection, caches the answer per type for the process, and skips types the generator already checked. In this suite the decorated services are registered by hand in S50, so the read happens once per type. The Readme describes this read |

---

## Results by scenario

### 1. Cold startup (fresh process: register + build + resolve every service once)

net10, mean of 15 launches. "vs Manual" is the BenchmarkDotNet Ratio column.

| Variant | N=50 mean | vs Manual | N=50 alloc (2nd invocation) | N=500 mean | vs Manual | N=500 alloc (2nd invocation) |
|---|---:|---:|---:|---:|---:|---:|
| Manual | 25.39 ms | 1.00 | 117.4 KB | 56.89 ms | 1.00 | 1094.34 KB |
| CseGenerated | 26.44 ms | 1.04 (tie) | 116.84 KB | 59.90 ms | 1.05 (tie) | 1019.41 KB |
| CseReflection | 39.29 ms | 1.55 | 335.8 KB | 72.52 ms | 1.28 | 3291.73 KB |
| Scrutor | 35.37 ms | 1.39 | 181.77 KB | 65.75 ms | 1.16 | 1825.91 KB |

- CseGenerated adds 1.05 ms (N=50) and 3.01 ms (N=500) over Manual on net10, inside the summed BenchmarkDotNet error (0.855 + 0.375 ms and 1.629 + 3.004 ms). On net8 and net9 the gap is outside error (1.05-1.06×).
- CseReflection is the slowest variant at both sizes on every TFM and allocates 2.85-3.55× Manual.
- CseGenerated allocates less than Manual at N=500 on net8, net9 and net10 (0.93, 0.98, 0.93) and more on net11 (1171.02 vs 1087.08 KB, 1.08). Observed, not explained.
- The input assembly carries `[RegisterScoped]` and the CSE generated registry, so the Scrutor process also loads the CSE assembly; that cost is inside Scrutor's rows. Scrutor's attribute mode is not measured (it needs its own attributes on the inputs).
- The allocation columns come from a second invocation in the same process (see Caveats), not from the timed first call.

### 2. Warm registration (in-process, N services into a new collection)

net10. The BenchmarkDotNet Ratio column in the raw report is relative to the empty-collection Baseline (13.61 ns at N=50, 13.19 ns at N=500), so it is not quoted here. "vs Manual" below is computed from the means.

| Variant | N=50 mean | vs Manual | N=50 alloc | N=500 mean | vs Manual | N=500 alloc |
|---|---:|---:|---:|---:|---:|---:|
| Manual | 667.47 ns | 1.00 | 4376 B | 7,153.27 ns | 1.00 | 40416 B |
| CseGenerated | 782.79 ns | 1.17 | 4464 B | 6,816.07 ns | 0.95 | 40504 B |
| CseReflection | 108,035.06 ns | 161.9 | 176395 B | 1,171,299.79 ns | 163.7 | 1771567 B |
| Scrutor | 52,844.93 ns | 79.2 | 49370 B | 511,994.57 ns | 71.6 | 489616 B |

Warm numbers favour the reflection rows: type loading, reflection caches and JIT of the scanning code are paid once by the self-check and the warmup and are outside this unit. A real application registers once per process, which is what section 1 measures. CseGenerated's extra 88 B (128 B on net8 and net9) is constant across N, consistent with the one marker descriptor CSE adds per registered assembly; not profiled.

### 3. Decoration (50 manual registrations, then decorate IService0..IService9)

net10. Every row includes the same 50 manual `AddScoped` calls (667.47 ns in section 2, measured separately).

| Variant | Mean | vs Manual | Allocated | vs Manual |
|---|---:|---:|---:|---:|
| Manual (`Replace` with a factory) | 1.010 μs | 1.00 | 4.9 KB | 1.00 |
| CseGenerated (`[Decorates]`) | 2.137 μs | 2.12 | 7.09 KB | 1.45 |
| CseRuntime (`Decorate<,>`) | 3.732 μs | 3.70 | 10.99 KB | 2.24 |
| Scrutor (`Decorate<,>`) | 12.217 μs | 12.10 | 11.46 KB | 2.34 |

- All three library paths move the original to a hidden registration and add a factory; the manual row only swaps one descriptor.
- CseGenerated applies all ten decorators in one pass over the collection (`ServiceDecoration.DecorateAll`). CseRuntime makes ten `Decorate` calls, each walking the collection, and builds an activator per decorator (`Internal/DecoratorActivator.cs`). Scrutor also walks the collection per call. Not profiled.
- Both CSE paths are faster than Scrutor and allocate less on every TFM. On net11 Scrutor drops to 4,253.0 ns (12.217-13.315 μs elsewhere), still above CseRuntime (3,512.7 ns).

### 4. Resolve (one scope + one resolve, provider built in setup, N=50)

net10. Plain and Decorated resolve different services (IService15 with one dependency vs IService0 with none), so the two categories are not comparable with each other.

| Category | Variant | Mean | vs Manual | Allocated |
|---|---|---:|---:|---:|
| Plain | Manual | 137.9 ns | 1.00 | 360 B |
| Plain | CseGenerated | 147.6 ns | 1.07 | 360 B |
| Plain | Scrutor | 145.8 ns | 1.06 | 360 B |
| Decorated | Manual (factory) | 115.2 ns | 1.00 | 360 B |
| Decorated | CseGenerated | 162.5 ns | 1.41 | 360 B |
| Decorated | CseRuntime | 178.8 ns | 1.55 | 360 B |
| Decorated | Scrutor | 323.0 ns | 2.80 | 392 B |

- Plain: every variant leaves the same descriptors, so the rows should tie. They tie on net8. On net10 CseGenerated (1.07) and Scrutor (1.06) are outside error against Manual; on net9 both are below Manual (0.97, 0.98). On net11 Scrutor (144.9 ns) is faster than CseGenerated (147.7 ns), outside error. Treat these few-percent gaps as weak evidence.
- Decorated: CseGenerated's factory calls the decorator constructor directly. CseRuntime calls a `ConstructorInvoker` with fixed-arity `Invoke` on net8 and later (`Internal/DecoratorActivator.cs:59-73`), with no argument array for up to four parameters. Scrutor uses `ActivatorUtilities.CreateInstance`. On net10 CseGenerated is 0.50× Scrutor's time and allocates 32 B less.
- The gap between the two CSE paths is small and changes sign: CseRuntime / CseGenerated is 1.13 (net8), 1.02 (net9), 1.10 (net10) and 0.95 (net11, 184.1 vs 193.8 ns).

### 5. Native AOT (net10, linux-x64, N=50 + 10 decorators)

> Script-driven, not BenchmarkDotNet, and not part of the run above: `benchmarks/results/di/aot/aot-report.md` was generated on 2026-11-02 at a 1-minute load average of 1.72. `dotnet publish -c Release -r linux-x64 -p:TreatWarningsAsErrors=false -p:TrimmerSingleWarn=false` from a clean obj. `TrimmerSingleWarn=false` lists each warning per member, so the counts are higher than a default publish shows. No sample has a `TrimmerRootAssembly`, so reflection-based variants see only what the trimmer kept.

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

### 6. TryAdd registration (N scoped services with TryAdd semantics, warm)

net10. "vs ManualTryAdd" is the BenchmarkDotNet Ratio column. ManualAdd is a reference with no duplicate check.

| Variant | N=50 mean | vs ManualTryAdd | N=50 alloc | N=500 mean | vs ManualTryAdd | N=500 alloc | N=2000 mean | vs ManualTryAdd | N=2000 alloc |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| ManualTryAdd | 2,507.5 ns | 1.00 | 4.27 KB | 196,985.5 ns | 1.00 | 39.47 KB | 3,200,071.1 ns | 1.000 | 157.27 KB |
| CseGenerated | 3,471.6 ns | 1.38 | 4.36 KB | 282,310.3 ns | 1.43 | 39.55 KB | 4,635,207.2 ns | 1.448 | 157.35 KB |
| CseReflection | 204,885.2 ns | 81.71 | 243.97 KB | 2,434,246.0 ns | 12.36 | 2485.75 KB | 16,089,884.4 ns | 5.028 | 10835.06 KB |
| Scrutor | 54,055.4 ns | 21.56 | 48.22 KB | 786,290.3 ns | 3.99 | 478.68 KB | 6,624,435.2 ns | 2.070 | 1937.52 KB |
| ManualAdd | 679.0 ns | 0.27 | 4.27 KB | 6,880.5 ns | 0.03 | 39.47 KB | 30,856.2 ns | 0.010 | 157.27 KB |

- On an empty collection every TryAdd row scans the collection once per added descriptor, so all four are O(N^2). framework `TryAdd` loops the collection, Scrutor `Skip` calls framework `TryAdd` ([RegistrationStrategy.cs L47-L50](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/RegistrationStrategy.cs#L47-L50)), and CSE loops it in `ServiceRegistration.FindFirst` (`ServiceRegistration.cs:141-154`, an indexed loop). ManualTryAdd grows from 2,507.5 ns to 3,200,071.1 ns between N=50 and N=2000 while ManualAdd grows from 679.0 ns to 30,856.2 ns. The growth is shared by all three libraries, not a CSE cost.
- CSE loss: CseGenerated takes 1.38-1.448× ManualTryAdd on net10 and 1.10-1.58× across TFMs for the same scan. Not profiled.
- CseGenerated is faster than Scrutor at every N; the gap narrows as the quadratic scan dominates (0.06× at N=50, 0.70× at N=2000 on net10).
- CseReflection is the slowest row at every N on every TFM, 2.43-3.79× Scrutor on net10 and up to 9.95× on net8. The warm caveat from section 2 applies to both reflection rows.

### 7. Transient decorator resolve (root provider, no scope)

net10. Every resolve activates the decorator and its inner service again. The services are not disposable, so the root provider does not track them.

| Variant | Mean | vs Manual | Allocated | vs Manual |
|---|---:|---:|---:|---:|
| Manual (`Replace` factory) | 24.73 ns | 1.00 | 48 B | 1.00 |
| CseGenerated (`[Decorates]`) | 38.39 ns | 1.55 | 48 B | 1.00 |
| CseRuntime (`Decorate<,>`) | 55.34 ns | 2.24 | 48 B | 1.00 |
| Scrutor (`Decorate<,>`) | 163.95 ns | 6.63 | 80 B | 1.67 |

- The manual factory builds the inner service with `new`. CSE and Scrutor resolve the inner service from a hidden keyed registration, so part of the CSE gap is that extra resolve, as in section 4.
- CseRuntime adds a `ConstructorInvoker` call on top of CseGenerated's direct constructor call; Scrutor calls `ActivatorUtilities.CreateInstance` per resolve ([DecorationStrategy.cs L37](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/DecorationStrategy.cs#L37)).
- CSE loss on every TFM: CseGenerated 1.45-1.72× and CseRuntime 1.85-2.54× Manual.

What the parity check enforces for sections 6 and 7:
- TryAdd: for every variant and N, the collection holds exactly N scoped type registrations of the T{N} types, plus one CSE marker for the two CSE rows, and a scope resolves `0..N-1`. Separately, each of the four TryAdd variants runs over a collection that already holds `IService0`, and `IService0` must still have exactly one descriptor.
- Transient decoration: in every variant IService0 to IService9 resolve to 1000 + i, the last non-keyed descriptor of each is transient, and two resolves of IService0 return different instances.

Feature matrix for these classes, from reading each library's source and docs (not measured):

| Feature | CSharpEssentials.DependencyInjection | Scrutor 7.0.0 | Manual Microsoft.Extensions.DependencyInjection |
|---|---|---|---|
| TryAdd cost model | A linear scan per descriptor in `FindFirst` | `Skip` calls framework `TryAdd`, a linear scan per descriptor | `TryAdd` loops the collection per descriptor |
| Transient decorator activation per resolve | Generated: a direct constructor call in a lambda. Runtime: a `ConstructorInvoker` call (an argument array only above four parameters or on netstandard2.1) and a keyed inner lookup | `ActivatorUtilities.CreateInstance` | A factory lambda |

---

## Cross-TFM

Mean per TFM, copied from each report (units as printed). Run order net8 → net9 → net10 → net11.

| Scenario | Variant | net8 | net9 | net10 | net11 |
|---|---|---|---|---|---|
| Cold N=50 | Manual | 22.38 ms | 21.97 ms | 25.39 ms | 27.73 ms |
| | CseGenerated | 23.76 ms | 23.35 ms | 26.44 ms (tie) | 27.83 ms (tie) |
| | CseReflection | 36.24 ms | 34.01 ms | 39.29 ms | 40.58 ms |
| | Scrutor | 31.93 ms | 30.34 ms | 35.37 ms | 37.36 ms |
| Cold N=500 | Manual | 50.20 ms / 1091.45 KB | 48.11 ms / 1090.67 KB | 56.89 ms / 1094.34 KB | 63.40 ms / 1087.08 KB |
| | CseGenerated | 52.65 ms / 1016.52 KB | 50.56 ms / 1063.8 KB | 59.90 ms / 1019.41 KB (tie) | 65.55 ms / 1171.02 KB (tie) |
| | CseReflection | 67.78 ms / 3871.52 KB | 62.68 ms / 3187.77 KB | 72.52 ms / 3291.73 KB | 78.49 ms / 3174.6 KB |
| | Scrutor | 58.33 ms / 1819.11 KB | 55.92 ms / 1818.33 KB | 65.75 ms / 1825.91 KB | 71.87 ms / 1718.49 KB |
| Warm N=50 | Manual | 1,009.74 ns | 875.31 ns | 667.47 ns | 613.54 ns |
| | CseGenerated | 822.18 ns | 859.47 ns | 782.79 ns | 713.26 ns |
| | CseReflection | 294,961.48 ns | 135,379.49 ns | 108,035.06 ns | 100,462.64 ns |
| | Scrutor | 60,646.33 ns | 60,125.94 ns | 52,844.93 ns | 48,885.55 ns |
| Warm N=500 | Manual | 8,005.99 ns | 8,069.82 ns | 7,153.27 ns | 6,480.18 ns |
| | CseGenerated | 7,410.30 ns | 7,560.98 ns | 6,816.07 ns | 6,256.79 ns |
| | CseReflection | 3,024,397.58 ns | 1,485,624.12 ns | 1,171,299.79 ns | 1,126,231.31 ns |
| | Scrutor | 607,868.88 ns | 606,253.35 ns | 511,994.57 ns | 495,688.60 ns |
| Decoration | Manual | 1.200 μs / 4.9 KB | 1.239 μs / 4.9 KB | 1.010 μs / 4.9 KB | 952.9 ns / 4.9 KB |
| | CseGenerated | 2.583 μs / 7.16 KB | 2.644 μs / 7.16 KB | 2.137 μs / 7.09 KB | 2,144.3 ns / 7.02 KB |
| | CseRuntime | 4.721 μs / 10.99 KB | 4.515 μs / 10.99 KB | 3.732 μs / 10.99 KB | 3,512.7 ns / 10.76 KB |
| | Scrutor | 13.315 μs / 11.46 KB | 12.647 μs / 11.46 KB | 12.217 μs / 11.46 KB | 4,253.0 ns / 11.31 KB |
| Plain resolve | Manual | 153.5 ns | 158.2 ns | 137.9 ns | 148.5 ns |
| | CseGenerated | 154.0 ns (tie) | 153.7 ns | 147.6 ns | 147.7 ns (tie) |
| | Scrutor | 154.2 ns (tie) | 155.8 ns | 145.8 ns | 144.9 ns |
| Decorated resolve | Manual | 150.1 ns | 127.1 ns | 115.2 ns | 114.6 ns |
| | CseGenerated | 191.7 ns | 193.4 ns | 162.5 ns | 193.8 ns |
| | CseRuntime | 217.1 ns | 196.4 ns | 178.8 ns | 184.1 ns |
| | Scrutor | 349.5 ns | 337.5 ns | 323.0 ns | 321.2 ns |
| Transient decorated resolve | Manual | 30.68 ns | 27.17 ns | 24.73 ns | 22.72 ns |
| | CseGenerated | 45.29 ns | 39.27 ns | 38.39 ns | 38.98 ns |
| | CseRuntime | 61.24 ns | 50.22 ns | 55.34 ns | 57.80 ns |
| | Scrutor | 165.75 ns | 160.36 ns | 163.95 ns | 156.90 ns |
| TryAdd N=50 | ManualTryAdd | 2,712.1 ns | 2,856.6 ns | 2,507.5 ns | 2,441.6 ns |
| | CseGenerated | 3,755.8 ns | 3,450.2 ns | 3,471.6 ns | 3,274.4 ns |
| | CseReflection | 622,921.4 ns | 232,065.2 ns | 204,885.2 ns | 189,398.0 ns |
| | Scrutor | 62,597.2 ns | 62,046.9 ns | 54,055.4 ns | 52,748.3 ns |
| TryAdd N=2000 | ManualTryAdd | 3,195,788.2 ns | 3,161,995.7 ns | 3,200,071.1 ns | 3,220,705.8 ns |
| | CseGenerated | 5,060,493.7 ns | 4,083,063.9 ns | 4,635,207.2 ns | 4,585,995.7 ns |
| | CseReflection | 35,599,446.4 ns | 16,674,039.6 ns | 16,089,884.4 ns | 15,441,648.8 ns |
| | Scrutor | 7,181,262.7 ns | 6,386,956.5 ns | 6,624,435.2 ns | 6,021,110.7 ns |

- The ordering Manual < CseGenerated < Scrutor < CseReflection holds on every TFM for cold startup. For decorated resolve, Manual is fastest and Scrutor slowest on every TFM; the two CSE rows swap on net11.
- net8 CseReflection is 2.0-3.3× its net9-net11 values in warm registration (294,961.48 ns vs 100,462.64-135,379.49 ns at N=50) and in TryAdd (622,921.4 ns vs 189,398.0-232,065.2 ns at N=50). Not investigated.
- net11 decoration: Scrutor drops to 4,253.0 ns (12.217-13.315 μs elsewhere). Not investigated.
- net8 decorated-resolve Manual (150.1 ns) is above its net9-net11 values (114.6-127.1 ns), so the net8 CSE ratios against Manual are lower (1.28, 1.45).
- The TryAdd N=500 rows are in the reports and follow the same order; they are left out of this table for size.
- Allocation is flat across TFMs for warm registration (4376-4504 B at N=50 for Manual and CseGenerated) and resolve (360 / 392 B).

---

## Feature matrix

CSE cells come from this repository (csproj, `Readme.MD`, `ServiceSourceWriter.cs`, `Internal/*.cs`). Scrutor links point at tag [`v7.0.0`](https://github.com/khellang/Scrutor/tree/v7.0.0) (commit `7f315da`, matches the 7.0.0 nuspec). Framework links point at Learn or `dotnet/runtime` tag `v10.0.0`. The "measured" row comes from section 5; every other row comes from reading source or docs, not from running code.

| Feature | CSharpEssentials.DependencyInjection 7.0.0 | Scrutor 7.0.0 | Manual Microsoft.Extensions.DependencyInjection |
|---|---|---|---|
| Runtime reflection: registration | Generated path: none; emits `new ServiceDescriptor(typeof(S), key, typeof(I), lifetime)`. Opt-in `AddServicesFromAssemblies` uses `Assembly.GetTypes()` (`Internal/AssemblyServiceScanner.cs:98`) | Yes: `Assembly.GetTypes()` ([ReflectionExtensions.cs L226](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/ReflectionExtensions.cs#L226)), `MakeGenericType` ([L276](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/ReflectionExtensions.cs#L276)), attribute reads ([AttributeSelector.cs](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/AttributeSelector.cs)) | None for registration. The default provider activates services by reflection / IL emit in all three columns ([ILEmitBackend in the csproj](https://github.com/dotnet/runtime/blob/v10.0.0/src/libraries/Microsoft.Extensions.DependencyInjection/src/Microsoft.Extensions.DependencyInjection.csproj)) |
| Runtime reflection: decoration | Generated `[Decorates]` and factory `Decorate<T>(Func<...>)`: activation is a direct constructor call. `EnsureDecoratable` reads `GetConstructors`/`GetParameters`/`GetCustomAttribute<ServiceKeyAttribute>` once per implementation type per process (cached), and the generated path skips types the generator already checked (`Internal/ServiceDecoration.cs:196-215`); 0 AOT warnings. Runtime `Decorate<TService,TDecorator>`: `GetConstructors` once, then a `ConstructorInvoker` per resolve on net8+ (`Internal/DecoratorActivator.cs`). Open generic: `MakeGenericType` | Type decorators: `ActivatorUtilities.CreateInstance` ([DecorationStrategy.cs L37](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/DecorationStrategy.cs#L37)); open generic `MakeGenericType` ([OpenGenericDecorationStrategy.cs](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/OpenGenericDecorationStrategy.cs)) | No decoration API ([registration methods](https://learn.microsoft.com/dotnet/core/extensions/dependency-injection/service-registration#registration-methods)); a hand-written factory needs no reflection |
| Source generator | Yes, bundled with analyzers and a code fix | No: the [7.0.0 package](https://www.nuget.org/packages/Scrutor/7.0.0) has no `analyzers/` folder | N/A |
| `IsAotCompatible` / trim annotations | `IsAotCompatible=true` on net8.0+ (not netstandard2.1). `[RequiresUnreferencedCode]`+`[RequiresDynamicCode]` on `AddServicesFromAssemblies` and `Decorate(Type, Type)`; `[DynamicallyAccessedMembers]` on `Decorate<TService,TDecorator>` | None: no flag in [Scrutor.csproj](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/Scrutor.csproj), no trim attributes in `src/Scrutor/` | Registration APIs carry `[DynamicallyAccessedMembers(PublicConstructors)]` ([ServiceCollectionServiceExtensions.cs L68](https://github.com/dotnet/runtime/blob/v10.0.0/src/libraries/Microsoft.Extensions.DependencyInjection.Abstractions/src/ServiceCollectionServiceExtensions.cs#L68)) |
| Native AOT publish without warnings, and runs (measured) | Generated: yes, 0 warnings, runs. Reflection fallback: 4 warnings, crashes | No: 12 warnings (`TrimmerSingleWarn=false`), crashes | Yes: 0 warnings, runs |
| Extra runtime dependencies | `Microsoft.Extensions.DependencyInjection.Abstractions` and `Microsoft.Extensions.Logging.Abstractions`, both `[9.0.4,)` | `Microsoft.Extensions.DependencyInjection.Abstractions` 10.0.0 and `Microsoft.Extensions.DependencyModel` 10.0.0 on every TFM ([nuspec](https://www.nuget.org/api/v2/package/Scrutor/7.0.0)) | None beyond the DI packages |
| Convention-based scanning | No "register every class" scan. Default service type rule only: `I{TypeName}`, else self | Yes: `Scan(...).AddClasses(...).AsMatchingInterface()`, namespace/attribute filters ([README](https://github.com/khellang/Scrutor/blob/v7.0.0/README.md#scanning)) | No |
| Attribute-based registration | Primary model: `[RegisterScoped]`/`[RegisterSingleton]`/`[RegisterTransient]` with `Key`, `As`, `Strategy`; `[ExcludeFromRegistration]` | Opt-in `[ServiceDescriptor]` + `.UsingAttributes()`, still a runtime scan ([ServiceDescriptorAttribute.cs](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/ServiceDescriptorAttribute.cs)) | No |
| Keyed services: registration | Yes, `Key = <constant>` | Yes, `WithServiceKey(...)` ([ILifetimeSelector.cs L37-43](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/ILifetimeSelector.cs#L37-L43)) and `ServiceDescriptorAttribute.ServiceKey` | Yes, `AddKeyed{Lifetime}` ([keyed services](https://learn.microsoft.com/dotnet/core/extensions/dependency-injection#keyed-services)) |
| Keyed services: decoration | Yes: `Decorate<,>(serviceKey: ...)` and `[Decorates(..., Key = ...)]`; `[ServiceKey]`/`[FromKeyedServices]` honoured on the decorator constructor; a decorated implementation that injects `[ServiceKey]` is rejected (`Internal/ServiceDecoration.cs:196-215`) | Not through the public `Decorate`/`TryDecorate` overloads: they pass `serviceKey: null` ([Decoration.cs L262](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/ServiceCollectionExtensions.Decoration.cs#L262)) and matching requires equal keys ([DecorationStrategy.cs L18-20](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/DecorationStrategy.cs#L18-L20)); a custom strategy works for `string` keys only (source reading, not run) | No API; re-register with a keyed factory |
| Open-generic decoration | Runtime `Decorate(typeof(IRepo<>), typeof(CachedRepo<>))` over closed registrations; throws if an open-generic registration exists; not generated from attributes (CSE2008) | Closed registrations only; open definitions excluded ([OpenGenericDecorationStrategy.cs L17-21](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/OpenGenericDecorationStrategy.cs#L17-L21)) | No |
| Registration strategies, key-aware | `Add`, `TryAdd`, `TryAddEnumerable`, `Replace`, `Throw`; all match service type + key | `Append`, `Skip`, `Throw`, `Replace` ([RegistrationStrategy.cs](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/RegistrationStrategy.cs)). `Skip` is key-aware (MS `TryAdd`); `Throw` and `Replace` ignore the key ([ServiceCollectionExtensions.cs L9-12](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/ServiceCollectionExtensions.cs#L9-L12), [RegistrationStrategy.cs L70-105](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/RegistrationStrategy.cs#L70-L105)); no `TryAddEnumerable` | `TryAdd*`, `TryAddEnumerable`, `Replace`, `RemoveAll` ([docs](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.extensions.servicecollectiondescriptorextensions)); `TryAdd`/`Replace` compare type + key; no throw-on-duplicate |
| Compile-time diagnostics | 9 analyzer rules CSE2001-CSE2009 (missing interface, `Throw` conflict, bad decorator constructor, captive dependency, ...) + a code fix | None; errors at runtime (`DecorationException`, `DuplicateTypeRegistrationException`, `MissingTypeRegistrationException` in [src/Scrutor](https://github.com/khellang/Scrutor/tree/v7.0.0/src/Scrutor)) | None from the library; runtime `ValidateScopes` / `ValidateOnBuild` ([ServiceProviderOptions](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.serviceprovideroptions)) |
| DI registration required | No setup call: `services.Add{Assembly}Services()` or `AddAllServices()` | No setup call: `services.Scan(...)` ([README](https://github.com/khellang/Scrutor/blob/v7.0.0/README.md#scanning)) | N/A |

---

## Caveats

- Warm registration is not like-for-like. The reflection variants have loaded types, filled reflection caches and JIT-compiled their scanning code before the measured unit. Their warm numbers are lower bounds; the cold table is the startup comparison. This applies to the TryAdd class too.
- The shared inputs carry CSE attributes. The input classes have `[RegisterScoped]` and the input assembly contains the CSE generated registry, so the Scrutor process also loads the CSE assembly. That small cost is inside Scrutor's cold numbers.
- There is no Scrutor attribute column. Scrutor's `[ServiceDescriptor]` mode would need different attributes on the shared input classes. It was not measured.
- Scrutor is configured with `AsMatchingInterface()` (cited from [IServiceTypeSelector.cs L55-L58](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/IServiceTypeSelector.cs#L55-L58)), which produces the same descriptors as the other variants; the self-check verifies it. `AsImplementedInterfaces()` or `AsSelfWithInterfaces()` would register more descriptors and were not measured.
- The manual decoration baseline does less work. `ManualDecorationS50` builds the inner service with `new Service{i}()` inside the factory; CSE and Scrutor resolve the inner service from a hidden keyed registration. Part of the decorated-resolve and transient-resolve gap is that extra container resolve. It is still the gap a user sees when moving from a hand-written factory to a decoration library.
- Plain and Decorated resolve rows measure different services (IService15 with a dependency, IService0 without). Compare rows only within a category.
- Decoration reads constructor metadata. `EnsureDecoratable` (`Internal/ServiceDecoration.cs:196-215`) runs `GetConstructors()`/`GetParameters()`/`GetCustomAttribute<ServiceKeyAttribute>()` once per implementation type per process, also on the generated path, unless the generator already checked that type. This is metadata reflection only: no dynamic code, 0 AOT warnings, and the AOT sample runs. The Readme describes it.
- AOT warning counts use `TrimmerSingleWarn=false` and are higher than a default publish shows. No sample uses `TrimmerRootAssembly`; rooting the input assemblies might change the Scrutor and CseReflection outcome and was not tested. The AOT report predates this run (2026-11-02, load 1.72), and its wall times (3 ms) are not a result.
- Cold allocation is not first-call allocation. In each launch BenchmarkDotNet 0.14 runs the unit a second time in the same process, after the timed invocation, to collect GC stats (`// GC:` line after `// AfterActualRun` in the run log). The allocation column therefore excludes JIT, type loading and first-call reflection-cache allocations; it is the managed allocation of one register + build + resolve-all in an already-warm process.
- Only two load readings exist for the run (1.55 at the start, 1.90 at the end), with a desktop session in the background. Gaps of a few percent, such as plain resolve on net9 and net10, are weak evidence even when outside error.
- These outliers were not investigated: net8 CseReflection is 2.0-3.3× its other TFMs in warm registration and TryAdd; net11 Scrutor decoration is about 3× faster than on the other TFMs; net8 decorated-resolve Manual is above its other TFMs; on net11 CseRuntime decorated resolve is faster than CseGenerated.
- A difference within the sum of the two error margins is reported as a tie.
- The CSE column is the 7.0.0 working tree on the local integration branch `integ/all` (all open pull requests merged), which may differ from the last version published to NuGet.

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
