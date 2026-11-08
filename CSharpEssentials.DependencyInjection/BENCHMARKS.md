# CSharpEssentials.DependencyInjection: Performance benchmarks

Compared with Scrutor 7.0.0 and manual Microsoft.Extensions.DependencyInjection registration, measured with BenchmarkDotNet v0.14.0 on a 6-core, 12-thread x64 desktop CPU on Linux under .NET 8, 9, 10 and 11.

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
- `ColdRegistrationBenchmarks`, `RegistrationBenchmarks`, `ResolveBenchmarks` and `TryAddBenchmarks` were rerun on every TFM after pull requests #230-#256 merged. No load reading was recorded for the rerun. `DecorationBenchmarks` and `TransientDecoratorBenchmarks` were not rerun; their numbers come from the first run, which had a 1-minute load average of 1.55 at the start of the suite and 1.90 at the end. No row in either run had a coefficient of variation above 5%, so no row is marked noisy.
- Reports: `benchmarks/results/di/{tfm}/results/*-report-github.md` (also `.csv`, `.html` and JSON), for example [net10.0 ColdRegistrationBenchmarks](../benchmarks/results/di/net10.0/results/CSharpEssentials.DependencyInjection.Benchmarks.ColdRegistrationBenchmarks-report-github.md).
- The Native AOT table (section 5) is not part of this run. It comes from `benchmarks/results/di/aot/aot-report.md`, a separate script run on 2026-11-06 at a 1-minute load average of 2.85.

Environment:

| Property | Value |
|---|---|
| Machine | 6-core, 12-thread x64 desktop CPU (RyuJIT AVX2) |
| OS | Linux, kernel 7.3.0 |
| Machine load | Desktop session in the background; first run: 1-minute load average 1.55 at the suite start, 1.90 at the end; rerun: not recorded |
| Tool | BenchmarkDotNet v0.14.0, [MemoryDiagnoser]; default job for warm classes, ColdStart job (15 launches) for the cold class |
| .NET SDK | 11.0.100-rc.3.26522.148 (rerun classes); 11.0.100-rc.1.26425.128 (decoration and transient decorator classes) |
| Runtimes tested | Rerun classes: .NET 8.0.33, 9.0.22, 10.0.14, 11.0.0. Decoration and transient decorator classes: .NET 8.0.30, 9.0.19, 10.0.11, 11.0.0 |
| Scrutor | 7.0.0 |
| Microsoft.Extensions.DependencyInjection | 10.0.0 (benchmark project) |
| CSharpEssentials.DependencyInjection (CSE) | 7.0.0 working tree, referenced as a project |

---

## Executive summary

Primary runtime: net10. A tie is a difference within the sum of the two BenchmarkDotNet error margins.

| Finding | Details |
|---|---|
| CSE loss: cold startup, CseGenerated takes 1.04-1.06× Manual on every TFM, outside error | net10: 26.35 vs 25.08 ms at N=50 (1.05), 58.71 vs 56.53 ms at N=500 (1.04). net8: 23.82 vs 22.76 ms (1.05) and 52.97 vs 50.62 ms (1.05). Allocation (2nd in-process invocation) net10: 116.84 vs 117.4 KB (N=50), 1067.09 vs 1094.62 KB (N=500). CseGenerated allocates more than Manual at N=500 on net11, 1170.74 vs 1086.8 KB (1.08), and net9, 1111.52 vs 1090.39 KB (1.02); net11 N=50: 116.78 vs 116.65 KB (Alloc Ratio 1.00) |
| Cold startup: Scrutor takes 1.38-1.41× Manual at N=50 and 1.15-1.16× at N=500 | net10: Scrutor 34.53 ms vs CseGenerated 26.35 ms (N=50), 64.80 vs 58.71 ms (N=500). Scrutor allocation (2nd in-process invocation) 181.77 / 1826.2 KB. Scrutor's cold numbers include loading the CSE assembly, because the shared inputs carry CSE attributes (see Caveats) |
| CSE loss: CseReflection is slower than Scrutor and allocates more, cold and warm, on every TFM (one cold tie: net11 N=50) | Cold net10: 35.46 vs 34.53 ms and 67.22 vs 64.80 ms; allocation 255.02 vs 181.77 KB and 2482.84 vs 1826.2 KB. Warm net10: 73,160.42 vs 48,426.92 ns (1.51×) and 776,549.00 vs 516,692.60 ns (1.50×); 95411 vs 49370 B (1.93×) and 940382 vs 489617 B (1.92×). net8 warm: 3.86× and 3.92× Scrutor |
| Warm registration: CseGenerated vs Manual is mixed | CSE loss at N=50 on net10 (730.99 vs 626.52 ns, 1.17×) and net11 (646.28 vs 572.14 ns, 1.13×). CseGenerated ties Manual at N=50 on net8 and net9 and is faster at N=500 on every TFM (0.92-0.95×). Allocation 4464 vs 4376 B on net10 and net11, 4504 vs 4376 B on net8 and net9 (the CSE marker descriptor) |
| Warm registration favours the reflection variants (see section 2) | Even so, on net10 Scrutor takes 77.3× and CseReflection 116.8× the Manual time at N=50 |
| CSE loss: decoration registration is slower than manual factory replacement | net10, 50 registrations + 10 decorations: Manual 1.010 μs / 4.9 KB, CseGenerated 2.137 μs / 7.09 KB (2.12×), CseRuntime 3.732 μs / 10.99 KB (3.70×), Scrutor 12.217 μs / 11.46 KB (12.10×). CseGenerated is 2.12-2.25× Manual on every TFM |
| Decoration registration: both CSE paths are faster than Scrutor and allocate less on every TFM | CseRuntime / Scrutor time ratio 0.31-0.36 on net8-net10 and 0.83 on net11 (3,512.7 vs 4,253.0 ns). Allocation 10.99 vs 11.46 KB (net8-net10), 10.76 vs 11.31 KB (net11). Scrutor drops from 12.217 μs (net10) to 4,253.0 ns (net11); not investigated |
| Plain resolve: all three variants tie on every TFM | net10: Manual 137.9 ns, CseGenerated 137.8 ns (1.00), Scrutor 136.4 ns (0.99), 360 B each |
| CSE loss: decorated resolve is slower than manual factory decoration | net10: Manual 111.0 ns, CseGenerated 129.7 ns (1.17), CseRuntime 138.1 ns (1.24), Scrutor 391.7 ns (3.53). CseGenerated is 1.11-1.20× Manual across TFMs. CSE allocates 360 B like Manual; Scrutor 392 B |
| CSE loss: transient decorator resolve is slower than the manual factory | net10: Manual 24.73 ns, CseGenerated 38.39 ns (1.55), CseRuntime 55.34 ns (2.24), Scrutor 163.95 ns (6.63). CSE allocates 48 B like Manual; Scrutor 80 B. CseGenerated is 1.45-1.72× Manual across TFMs |
| CSE loss: TryAdd registration, CseGenerated is slower than hand-written `TryAddScoped` on every TFM and N except two ties on net9 | net10: 1.19× (N=50), 1.19× (N=500), 1.228× (N=2000); range 1.028-1.30× across TFMs where it loses; net9 N=50 and N=500 tie (1.00). Allocation 1.00-1.03×. CseGenerated is faster than Scrutor `Skip` (0.06× at N=50, 0.59× at N=2000 on net10) |
| CSE loss: TryAdd, CseReflection is the slowest row on every TFM and N | net10: 156,846.6 vs Scrutor 50,982.9 ns at N=50 (3.08×), 12,177,000.5 vs 6,661,029.8 ns at N=2000 (1.83×). net8 N=50: 549,792.7 vs 62,969.7 ns (8.73×), 270.54 vs 50.18 KB |
| Native AOT (separate 2026-11-06 run): CseGenerated and Manual publish with 0 IL warnings and run; Scrutor and CseReflection crash | Scrutor: 12 IL warnings, `DecorationException` at startup. CseReflection: 4 IL warnings, `InvalidOperationException` at startup; it is documented as not AOT-safe (`[RequiresUnreferencedCode]`/`[RequiresDynamicCode]`). No sample roots the input assemblies |
| Generated decoration still reads constructor metadata, once per type | See Caveats. In this suite the decorated services are registered by hand in S50, so the read happens once per type |

---

## Results by scenario

### 1. Cold startup (fresh process: register + build + resolve every service once)

net10, mean of 15 launches. "vs Manual" is the BenchmarkDotNet Ratio column.

| Variant | N=50 mean | vs Manual | N=50 alloc (2nd invocation) | N=500 mean | vs Manual | N=500 alloc (2nd invocation) |
|---|---:|---:|---:|---:|---:|---:|
| Manual | 25.08 ms | 1.00 | 117.4 KB | 56.53 ms | 1.00 | 1094.62 KB |
| CseGenerated | 26.35 ms | 1.05 | 116.84 KB | 58.71 ms | 1.04 | 1067.09 KB |
| CseReflection | 35.46 ms | 1.41 | 255.02 KB | 67.22 ms | 1.19 | 2482.84 KB |
| Scrutor | 34.53 ms | 1.38 | 181.77 KB | 64.80 ms | 1.15 | 1826.2 KB |

- CseGenerated adds 1.27 ms (N=50) and 2.18 ms (N=500) over Manual on net10, outside the summed BenchmarkDotNet error (0.541 + 0.228 ms and 0.720 + 0.807 ms). The gap is outside error on every TFM (1.04-1.06×).
- CseReflection is the slowest variant at both sizes on every TFM (a tie with Scrutor at N=50 on net11) and allocates 2.17-2.72× Manual.
- CseGenerated allocates less than Manual at N=500 on net8 and net10 (0.94, 0.97) and more on net9 and net11 (1.02, and 1170.74 vs 1086.8 KB, 1.08). Observed, not explained.
- The allocation columns come from a second invocation in the same process (see Caveats), not from the timed first call.

### 2. Warm registration (in-process, N services into a new collection)

net10. The BenchmarkDotNet Ratio column in the raw report is relative to the empty-collection Baseline (12.52 ns at N=50, 12.51 ns at N=500), so it is not quoted here. "vs Manual" below is computed from the means.

| Variant | N=50 mean | vs Manual | N=50 alloc | N=500 mean | vs Manual | N=500 alloc |
|---|---:|---:|---:|---:|---:|---:|
| Manual | 626.52 ns | 1.00 | 4376 B | 6,740.29 ns | 1.00 | 40416 B |
| CseGenerated | 730.99 ns | 1.17 | 4464 B | 6,265.01 ns | 0.93 | 40504 B |
| CseReflection | 73,160.42 ns | 116.8 | 95411 B | 776,549.00 ns | 115.2 | 940382 B |
| Scrutor | 48,426.92 ns | 77.3 | 49370 B | 516,692.60 ns | 76.7 | 489617 B |

Warm numbers favour the reflection rows: type loading, reflection caches and JIT of the scanning code are paid once by the self-check and the warmup and are outside this unit. A real application registers once per process, which is what section 1 measures. CseGenerated's extra 88 B (128 B on net8 and net9) is constant across N, consistent with the one marker descriptor CSE adds per registered assembly; not profiled.

### 3. Decoration (50 manual registrations, then decorate IService0..IService9)

net10. Every row includes the same 50 manual `AddScoped` calls (626.52 ns in section 2, measured separately in the rerun).

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
| Plain | CseGenerated | 137.8 ns | 1.00 | 360 B |
| Plain | Scrutor | 136.4 ns | 0.99 | 360 B |
| Decorated | Manual (factory) | 111.0 ns | 1.00 | 360 B |
| Decorated | CseGenerated | 129.7 ns | 1.17 | 360 B |
| Decorated | CseRuntime | 138.1 ns | 1.24 | 360 B |
| Decorated | Scrutor | 391.7 ns | 3.53 | 392 B |

- Plain: every variant leaves the same descriptors, so the rows should tie, and they tie on every TFM.
- Decorated: CseGenerated's factory calls the decorator constructor directly. CseRuntime calls a `ConstructorInvoker` with fixed-arity `Invoke` on net8 and later (`Internal/DecoratorActivator.cs:59-73`), with no argument array for up to four parameters. Scrutor uses `ActivatorUtilities.CreateInstance`. The hidden inner of a scoped decorated service is registered as transient (pull request #252), so the scope does not cache the inner a second time. On net10 CseGenerated is 0.33× Scrutor's time and allocates 32 B less.
- CseRuntime is slower than CseGenerated on every TFM: CseRuntime / CseGenerated is 1.13 (net8), 1.15 (net9), 1.06 (net10) and 1.12 (net11).

### 5. Native AOT (net10, linux-x64, N=50 + 10 decorators)

> Script-driven, not BenchmarkDotNet (see "How these numbers were produced"). `dotnet publish -c Release -r linux-x64 -p:TreatWarningsAsErrors=false -p:TrimmerSingleWarn=false` from a clean obj. `TrimmerSingleWarn=false` lists each warning per member, so the counts are higher than a default publish shows. No sample has a `TrimmerRootAssembly`, so reflection-based variants see only what the trimmer kept.

| Variant | AOT publish | IL warnings (distinct) | Native binary | Exit code | All 50 services resolved with the right values |
|---|---|---:|---:|---:|---|
| ManualAot | yes | 0 | 2.2 MB | 0 | yes (`resolved 50 checksum 11225 ok`) |
| CseGeneratedAot | yes | 0 | 2.2 MB | 0 | yes (`resolved 50 checksum 11225 ok`) |
| CseReflectionAot | yes | 4 (IL2026 ×2, IL3050 ×2; 2 analyzer, 2 ILC) | 2.4 MB [1] | 134 | no: crashed |
| ScrutorAot | yes | 12 (IL2026 ×1, IL2055 ×2, IL2067 ×1, IL2070 ×4, IL2072 ×2, IL3050 ×2; all ILC) | 2.3 MB [1] | 134 | no: crashed |

[1] The binary does not register or resolve every service, so its size is not comparable with the working variants.

- CseReflection: `InvalidOperationException: Decorator '...Service0Decorator' must have exactly one public constructor, but has 0.` The trimmer removed the constructor that the reflection fallback looks up. The warnings point at the `AddServicesFromAssemblies` call site, which is annotated `[RequiresUnreferencedCode]`/`[RequiresDynamicCode]`; the Readme says to use the generated method for AOT.
- Scrutor: `Scrutor.DecorationException: Could not find any registered services for type '...IService0'.` The scan found no classes after trimming. The warnings come from inside Scrutor (`GetTypes`, `GetInterfaces`, `MakeGenericType`, `ActivatorUtilities.CreateInstance`). Whether rooting the input assembly makes it run was not tested.
- Wall time was 3 ms for both working variants (median of 5). The run started at load 2.85 (5-minute average 3.01), so this is not a startup result.

### 6. TryAdd registration (N scoped services with TryAdd semantics, warm)

net10. "vs ManualTryAdd" is the BenchmarkDotNet Ratio column. ManualAdd is a reference with no duplicate check.

| Variant | N=50 mean | vs ManualTryAdd | N=50 alloc | N=500 mean | vs ManualTryAdd | N=500 alloc | N=2000 mean | vs ManualTryAdd | N=2000 alloc |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| ManualTryAdd | 2,485.4 ns | 1.00 | 4.27 KB | 196,393.3 ns | 1.00 | 39.47 KB | 3,197,772.4 ns | 1.000 | 157.27 KB |
| CseGenerated | 2,957.6 ns | 1.19 | 4.36 KB | 233,466.0 ns | 1.19 | 39.55 KB | 3,927,624.1 ns | 1.228 | 157.35 KB |
| CseReflection | 156,846.6 ns | 63.11 | 163.33 KB | 1,925,097.2 ns | 9.80 | 1641.02 KB | 12,177,000.5 ns | 3.808 | 7131.89 KB |
| Scrutor | 50,982.9 ns | 20.51 | 48.22 KB | 774,211.8 ns | 3.94 | 478.15 KB | 6,661,029.8 ns | 2.083 | 1937.52 KB |
| ManualAdd | 618.2 ns | 0.25 | 4.27 KB | 6,578.5 ns | 0.03 | 39.47 KB | 27,978.0 ns | 0.009 | 157.27 KB |

- On an empty collection every TryAdd row scans the collection once per added descriptor, so all four are O(N^2). framework `TryAdd` loops the collection, Scrutor `Skip` calls framework `TryAdd` ([RegistrationStrategy.cs L47-L50](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/RegistrationStrategy.cs#L47-L50)), and CSE loops it in `ServiceRegistration.FindFirst` (`ServiceRegistration.cs:141-157`, an indexed loop). ManualTryAdd grows from 2,485.4 ns to 3,197,772.4 ns between N=50 and N=2000 while ManualAdd grows from 618.2 ns to 27,978.0 ns. The growth is shared by all three libraries, not a CSE cost.
- CSE loss: CseGenerated takes 1.19-1.228× ManualTryAdd on net10 and 1.028-1.30× across TFMs for the same scan, except net9 N=50 and N=500, where it ties (1.00). Not profiled.
- CseGenerated is faster than Scrutor at every N; the gap narrows as the quadratic scan dominates (0.06× at N=50, 0.59× at N=2000 on net10).
- CseReflection is the slowest row at every N on every TFM, 1.83-3.08× Scrutor on net10 and up to 8.73× on net8. The warm caveat from section 2 applies to both reflection rows.

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
| Cold N=50 | Manual | 22.76 ms | 21.89 ms | 25.08 ms | 26.25 ms |
| | CseGenerated | 23.82 ms | 23.28 ms | 26.35 ms | 27.46 ms |
| | CseReflection | 33.30 ms | 31.07 ms | 35.46 ms | 36.65 ms |
| | Scrutor | 32.12 ms | 30.17 ms | 34.53 ms | 36.10 ms |
| Cold N=500 | Manual | 50.62 ms / 1187.49 KB | 47.97 ms / 1090.39 KB | 56.53 ms / 1094.62 KB | 60.69 ms / 1086.8 KB |
| | CseGenerated | 52.97 ms / 1112.57 KB | 49.94 ms / 1111.52 KB | 58.71 ms / 1067.09 KB | 63.45 ms / 1170.74 KB |
| | CseReflection | 62.68 ms / 3016.13 KB | 57.43 ms / 2379.25 KB | 67.22 ms / 2482.84 KB | 71.22 ms / 2381.99 KB |
| | Scrutor | 58.24 ms / 1819.11 KB | 55.59 ms / 1818.05 KB | 64.80 ms / 1826.2 KB | 69.71 ms / 1814.54 KB |
| Warm N=50 | Manual | 822.99 ns | 822.93 ns | 626.52 ns | 572.14 ns |
| | CseGenerated | 808.68 ns (tie) | 822.18 ns (tie) | 730.99 ns | 646.28 ns |
| | CseReflection | 233,239.48 ns | 82,048.34 ns | 73,160.42 ns | 69,310.63 ns |
| | Scrutor | 60,383.56 ns | 59,493.03 ns | 48,426.92 ns | 48,281.53 ns |
| Warm N=500 | Manual | 7,513.59 ns | 7,562.48 ns | 6,740.29 ns | 6,070.67 ns |
| | CseGenerated | 7,109.25 ns | 6,984.07 ns | 6,265.01 ns | 5,698.18 ns |
| | CseReflection | 2,424,657.28 ns | 878,419.56 ns | 776,549.00 ns | 775,188.90 ns |
| | Scrutor | 618,845.28 ns | 596,364.57 ns | 516,692.60 ns | 488,318.15 ns |
| Decoration | Manual | 1.200 μs / 4.9 KB | 1.239 μs / 4.9 KB | 1.010 μs / 4.9 KB | 952.9 ns / 4.9 KB |
| | CseGenerated | 2.583 μs / 7.16 KB | 2.644 μs / 7.16 KB | 2.137 μs / 7.09 KB | 2,144.3 ns / 7.02 KB |
| | CseRuntime | 4.721 μs / 10.99 KB | 4.515 μs / 10.99 KB | 3.732 μs / 10.99 KB | 3,512.7 ns / 10.76 KB |
| | Scrutor | 13.315 μs / 11.46 KB | 12.647 μs / 11.46 KB | 12.217 μs / 11.46 KB | 4,253.0 ns / 11.31 KB |
| Plain resolve | Manual | 151.4 ns | 147.6 ns | 137.9 ns | 142.7 ns |
| | CseGenerated | 152.4 ns (tie) | 148.0 ns (tie) | 137.8 ns (tie) | 142.7 ns (tie) |
| | Scrutor | 153.0 ns (tie) | 149.2 ns (tie) | 136.4 ns (tie) | 141.8 ns (tie) |
| Decorated resolve | Manual | 125.4 ns | 116.8 ns | 111.0 ns | 110.0 ns |
| | CseGenerated | 146.4 ns | 140.5 ns | 129.7 ns | 122.1 ns |
| | CseRuntime | 165.4 ns | 160.9 ns | 138.1 ns | 136.3 ns |
| | Scrutor | 354.6 ns | 345.2 ns | 391.7 ns | 320.0 ns |
| Transient decorated resolve | Manual | 30.68 ns | 27.17 ns | 24.73 ns | 22.72 ns |
| | CseGenerated | 45.29 ns | 39.27 ns | 38.39 ns | 38.98 ns |
| | CseRuntime | 61.24 ns | 50.22 ns | 55.34 ns | 57.80 ns |
| | Scrutor | 165.75 ns | 160.36 ns | 163.95 ns | 156.90 ns |
| TryAdd N=50 | ManualTryAdd | 2,634.5 ns | 2,869.9 ns | 2,485.4 ns | 2,330.9 ns |
| | CseGenerated | 3,089.0 ns | 2,875.6 ns (tie) | 2,957.6 ns | 2,888.8 ns |
| | CseReflection | 549,792.7 ns | 175,940.1 ns | 156,846.6 ns | 147,152.5 ns |
| | Scrutor | 62,969.7 ns | 63,050.8 ns | 50,982.9 ns | 50,510.5 ns |
| TryAdd N=2000 | ManualTryAdd | 3,155,563.0 ns | 3,165,061.7 ns | 3,197,772.4 ns | 3,159,819.6 ns |
| | CseGenerated | 4,102,866.8 ns | 3,252,110.0 ns | 3,927,624.1 ns | 3,919,471.3 ns |
| | CseReflection | 29,396,348.5 ns | 12,468,702.6 ns | 12,177,000.5 ns | 11,827,754.0 ns |
| | Scrutor | 7,114,450.0 ns | 6,395,738.2 ns | 6,661,029.8 ns | 6,025,751.2 ns |

- The ordering Manual < CseGenerated < Scrutor < CseReflection holds on every TFM for cold startup. For decorated resolve, the order Manual < CseGenerated < CseRuntime < Scrutor holds on every TFM.
- net8 CseReflection is 2.4-3.7× its net9-net11 values in warm registration (233,239.48 ns vs 69,310.63-82,048.34 ns at N=50) and in TryAdd (549,792.7 ns vs 147,152.5-175,940.1 ns at N=50). Not investigated.
- net11 decoration: Scrutor drops to 4,253.0 ns (12.217-13.315 μs elsewhere). Not investigated.
- net8 decorated-resolve Manual (125.4 ns) is above its net9-net11 values (110.0-116.8 ns).
- The TryAdd N=500 rows are in the reports and follow the same order; they are left out of this table for size.
- Allocation is flat across TFMs for warm registration (4376-4504 B at N=50 for Manual and CseGenerated) and resolve (360 / 392 B).

---

## Feature matrix

CSE cells come from this repository (csproj, `Readme.MD`, `ServiceSourceWriter.cs`, `Internal/*.cs`). Scrutor links point at tag [`v7.0.0`](https://github.com/khellang/Scrutor/tree/v7.0.0) (commit `7f315da`, matches the 7.0.0 nuspec). Framework links point at Learn or `dotnet/runtime` tag `v10.0.0`. The "measured" row comes from section 5; every other row comes from reading source or docs, not from running code.

| Feature | CSharpEssentials.DependencyInjection 7.0.0 | Scrutor 7.0.0 | Manual Microsoft.Extensions.DependencyInjection |
|---|---|---|---|
| Runtime reflection: registration | Generated path: none; emits `new ServiceDescriptor(typeof(S), key, typeof(I), lifetime)`. Opt-in `AddServicesFromAssemblies` uses `Assembly.GetTypes()` (`Internal/AssemblyServiceScanner.cs:101`) | Yes: `Assembly.GetTypes()` ([ReflectionExtensions.cs L226](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/ReflectionExtensions.cs#L226)), `MakeGenericType` ([L276](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/ReflectionExtensions.cs#L276)), attribute reads ([AttributeSelector.cs](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/AttributeSelector.cs)) | None for registration. The default provider activates services by reflection / IL emit in all three columns ([ILEmitBackend in the csproj](https://github.com/dotnet/runtime/blob/v10.0.0/src/libraries/Microsoft.Extensions.DependencyInjection/src/Microsoft.Extensions.DependencyInjection.csproj)) |
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
- Scrutor is configured with `AsMatchingInterface()` (cited from [IServiceTypeSelector.cs L55-L58](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/IServiceTypeSelector.cs#L55-L58)), which produces the same descriptors as the other variants; the self-check verifies it. `AsImplementedInterfaces()` or `AsSelfWithInterfaces()` would register more descriptors and were not measured.
- The manual decoration baseline does less work. `ManualDecorationS50` builds the inner service with `new Service{i}()` inside the factory; CSE and Scrutor resolve the inner service from a hidden keyed registration. Part of the decorated-resolve and transient-resolve gap is that extra container resolve. It is still the gap a user sees when moving from a hand-written factory to a decoration library.
- Plain and Decorated resolve rows measure different services (IService15 with a dependency, IService0 without). Compare rows only within a category.
- Decoration reads constructor metadata. Generated `[Decorates]` code calls `ServiceRegistration.Decorate(services, DecoratorRegistration[])`, which applies all decorators of one order in a single pass (`ServiceDecoration.DecorateAll`). `EnsureDecoratable` (`Internal/ServiceDecoration.cs:196-215`) runs `GetConstructors()`/`GetParameters()`/`GetCustomAttribute<ServiceKeyAttribute>()` once per implementation type per process, also on the generated path, unless the generator already checked that type. This is metadata reflection only: no dynamic code, 0 AOT warnings, and the AOT sample runs. The Readme describes it.
- Rooting the AOT input assemblies with `TrimmerRootAssembly` might change the Scrutor and CseReflection outcome and was not tested. The AOT wall times (3 ms) are not a result.
- Cold allocation is not first-call allocation. In each launch BenchmarkDotNet 0.14 runs the unit a second time in the same process, after the timed invocation, to collect GC stats (`// GC:` line after `// AfterActualRun` in the run log). The allocation column therefore excludes JIT, type loading and first-call reflection-cache allocations; it is the managed allocation of one register + build + resolve-all in an already-warm process.
- Only two load readings exist, both for the first run (1.55 at the start, 1.90 at the end), with a desktop session in the background; none exist for the rerun. Gaps of a few percent, such as cold CseGenerated against Manual, are weak evidence even when outside error.
- The outliers listed under Cross-TFM were not investigated.
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

`--quick` without `--tfm` runs on net9.0, the script default for this suite. Its patterns are `*Registration*` (which also matches `ColdRegistrationBenchmarks`), `*Decoration*`, `*Resolve*`, `*.TryAddBenchmarks.*` and `*.TransientDecoratorBenchmarks.*`; the Short job's numbers are not comparable with the committed ones. Results are stored per framework under `benchmarks/results/di/{tfm}/results/` (Markdown, CSV, JSON and HTML). The AOT report is `benchmarks/results/di/aot/aot-report.md`; raw publish and run logs go to `benchmarks/results/di/aot/logs/`.
