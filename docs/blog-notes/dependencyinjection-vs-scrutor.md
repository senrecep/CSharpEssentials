# Blog notes: CSharpEssentials.DependencyInjection vs Scrutor vs manual registration

Raw material for a post. These notes are not the post. Every number comes from `CSharpEssentials.DependencyInjection/BENCHMARKS.md`, which reads `benchmarks/results/di/{tfm}/results/*.csv` and `benchmarks/results/di/aot/aot-report.md`. The machine was an AMD Ryzen 5 5600 running a desktop session. Full BenchmarkDotNet runs (default job for the warm classes, a 15-launch ColdStart job for the cold class) on net8.0, net9.0, net10.0 and net11.0, in that order, with a 1-minute load average of 1.55 at the start of the suite and 1.90 at the end. No row had a coefficient of variation above 5%. The SDK was 11.0.100-rc.1, with BenchmarkDotNet 0.14.0. Primary runtime is .NET 10.0.11. Competitor: Scrutor 7.0.0. Microsoft.Extensions.DependencyInjection 10.0.0. Reports: `benchmarks/results/di/{tfm}/results/*-report-github.md`. The AOT result is from a separate script run on 2026-11-02.

## The problem

- A growing app ends up with hundreds of `services.AddScoped<IFoo, Foo>()` lines, and a forgotten line fails only at runtime.
- The usual fix is assembly scanning at startup. It costs reflection at every process start. In the AOT sample here (no `TrimmerRootAssembly`), Scrutor's scan found no classes after trimming and the app crashed; whether rooting the input assembly fixes that was not tested.
- Decoration (wrapping a registered service) has no framework API, so it is either a hand-written factory or a library.
- The question to answer: can registration and decoration be declared on the class, without paying for a scan at startup, per resolve, or under Native AOT?

## What the framework already provides

- `Add{Lifetime}`, `TryAdd*`, `TryAddEnumerable`, `Replace`, `RemoveAll`, keyed services (`AddKeyed*`, `[FromKeyedServices]`).
- No scanning, no attributes, no decoration API. Decoration by hand means `Replace` with a factory that builds the decorator.
- Runtime checks `ValidateScopes` / `ValidateOnBuild`. No compile-time diagnostics.

## What each library adds

- CSharpEssentials.DependencyInjection (CSE):
  - `[RegisterScoped]` / `[RegisterSingleton]` / `[RegisterTransient]` on the class, with `Key`, `As`, `Strategy`.
  - A source generator writes `Add{Assembly}Services()` (and `AddAllServices()` in applications) as plain `ServiceDescriptor` adds.
  - `[Decorates]` generates decorator factories; `Decorate<TService, TDecorator>()` works at runtime, keyed or not.
  - 9 analyzer rules (CSE2001-CSE2009) and a code fix.
  - Opt-in reflection fallback `AddServicesFromAssemblies`, annotated `[RequiresUnreferencedCode]`/`[RequiresDynamicCode]`.
  - `IsAotCompatible` on net8.0+. Depends on DI.Abstractions and Logging.Abstractions.
- Scrutor 7.0.0:
  - Fluent runtime scanning: `Scan(s => s.FromAssemblyOf<T>().AddClasses().AsMatchingInterface()...)`; opt-in `[ServiceDescriptor]` attributes, still scanned at runtime.
  - `Decorate` / `TryDecorate` through `ActivatorUtilities.CreateInstance`; the public overloads do not decorate keyed registrations.
  - No generator, no analyzers, no trim annotations. Depends on DI.Abstractions and DependencyModel 10.0.0.

## The numbers that matter (net10 unless stated)

1. Cold startup (fresh process, register + build + resolve every service once). N=50: Manual 25.39 ms, CseGenerated 26.44 ms (1.04), Scrutor 35.37 ms (1.39), CseReflection 39.29 ms (1.55). N=500: 56.89 / 59.90 / 65.75 / 72.52 ms. Same order on every TFM.
2. The generated registry is close to hand-written code, but not equal to it on every runtime. Cold: a tie with Manual on net10 and net11 at both N, and 1.05-1.06× Manual (outside error) on net8 and net9. Warm N=50: 782.79 vs 667.47 ns on net10 (1.17×, loss) and 713.26 vs 613.54 ns on net11 (1.16×); CseGenerated is faster than Manual at N=500 on every TFM and at N=50 on net8 and net9. Cold allocation at net11 N=500 is higher than Manual: 1171.02 vs 1087.08 KB.
3. The reflection fallback loses to Scrutor on every TFM. Cold net10 39.29 vs 35.37 ms and 72.52 vs 65.75 ms; 335.8 vs 181.77 KB at N=50 (measured on a 2nd in-process invocation). Warm net10 2.04× and 2.29× slower and 3.57× / 3.62× more allocation (N=50 / N=500); on net8 4.86× and 4.98× slower.
4. Decoration registration (50 manual registrations + 10 decorations), net10. Manual 1.010 μs / 4.9 KB, CseGenerated 2.137 μs / 7.09 KB (2.12×), CseRuntime 3.732 μs / 10.99 KB (3.70×), Scrutor 12.217 μs / 11.46 KB (12.10×). Both CSE paths are faster than Scrutor and allocate less on every TFM, including net11, where Scrutor drops to 4,253.0 ns against CseRuntime 3,512.7 ns.
5. Plain resolve, net10: Manual 137.9 ns, CseGenerated 147.6 ns (1.07), Scrutor 145.8 ns (1.06), 360 B each, both outside error. All three tie on net8. Decorated resolve: Manual factory 115.2 ns, CseGenerated 162.5 ns (1.41), CseRuntime 178.8 ns (1.55), Scrutor 323.0 ns (2.80); CSE 360 B, Scrutor 392 B. On net11 CseRuntime (184.1 ns) beats CseGenerated (193.8 ns).
6. Transient decorator resolve (root provider, no scope), net10: Manual 24.73 ns, CseGenerated 38.39 ns (1.55), CseRuntime 55.34 ns (2.24), Scrutor 163.95 ns (6.63). CSE 48 B like Manual, Scrutor 80 B.
7. TryAdd registration, net10, ManualTryAdd / CseGenerated / Scrutor / CseReflection: N=50 2,507.5 / 3,471.6 / 54,055.4 / 204,885.2 ns; N=2000 3,200,071.1 / 4,635,207.2 / 6,624,435.2 / 16,089,884.4 ns. Plain `AddScoped` takes 679.0 ns and 30,856.2 ns, so the TryAdd scan is quadratic for everyone. CseGenerated is 1.10-1.58× ManualTryAdd across TFMs.
8. Native AOT (net10, linux-x64, separate 2026-11-02 run). Manual and CseGenerated: 0 IL warnings, 2.2 MB, run and resolve all 50 services. Scrutor: 12 IL warnings, crashes with "Could not find any registered services for type 'IService0'". CseReflection: 4 IL warnings, crashes with "must have exactly one public constructor, but has 0" (documented as not AOT-safe). No sample roots the input assemblies.

## Where CSE loses (lead with these)

- CseReflection against Scrutor: slower cold and warm on every TFM, more allocation, and 2.43-3.79× slower in TryAdd on net10 (9.95× on net8 at N=50).
- CseGenerated against hand-written code: warm registration at N=50 on net10 and net11 (1.17×, 1.16×), cold on net8 and net9 (1.05-1.06×), TryAdd on every TFM (1.10-1.58×).
- Decoration against a hand-written factory: registration 2.12-2.25× Manual, decorated resolve 1.28-1.69×, transient decorated resolve 1.45-1.72× (CseGenerated, across TFMs).
- Plain resolve on net10: CseGenerated 147.6 vs 137.9 ns (Scrutor is at 145.8 ns there too).
- Plain resolve on net11: CseGenerated 147.7 ns vs Scrutor 144.9 ns, outside error.
- Allocation against hand-written code: CseGenerated warm registration 4464 vs 4376 B (net10, N=50) and 40504 vs 40416 B (net10, N=500), cold net11 1171.02 vs 1087.08 KB (N=500) and 116.78 vs 116.65 KB (N=50), decoration registration 7.09 vs 4.9 KB (net10), TryAdd 1.00-1.03× ManualTryAdd.

## Decoration and reflection

- Registration on the generated path does no runtime reflection.
- Decoration reads constructor metadata. Generated `[Decorates]` code calls `ServiceRegistration.Decorate(services, DecoratorRegistration[])`, which applies all decorators in one pass over the collection (`ServiceDecoration.DecorateAll`). `EnsureDecoratable` (`Internal/ServiceDecoration.cs:196-215`) reads the implementation's constructors to reject `[ServiceKey]` injection, caches the answer per type for the process, and skips types the generator already checked at compile time.
- It is metadata reflection only: no dynamic code, 0 AOT warnings, and the AOT sample runs. The Readme now describes this read.
- Runtime `Decorate<TService, TDecorator>` reads the constructor once and calls it through a `ConstructorInvoker` on net8 and later, with no argument array for up to four parameters.

## Honest caveats to keep in the post

- Warm registration favours the reflection rows (types loaded, caches filled, scanning code JIT-compiled). Quote the cold table for startup.
- The manual decoration baseline builds the inner service with `new`; CSE and Scrutor resolve it from a hidden keyed registration. Part of the decorated-resolve and transient-resolve gap is that extra resolve.
- Plain and Decorated resolve rows measure different services; do not compare across the two.
- The shared input classes carry `[RegisterScoped]` and the input assembly holds the CSE registry, so the Scrutor process also loads the CSE assembly. Small, and borne by Scrutor.
- Scrutor's attribute mode was not measured: it would need different attributes on the shared inputs. Scrutor uses `AsMatchingInterface()` ([IServiceTypeSelector.cs L55-L58](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/IServiceTypeSelector.cs#L55-L58)).
- AOT warning counts use `TrimmerSingleWarn=false` (higher than a default publish). Rooting the inputs with `TrimmerRootAssembly` was not tested. The 3 ms AOT wall times were taken at load 1.72 and are not a result.
- Outliers not investigated: net8 CseReflection 2.0-3.3× its other TFMs in warm registration and TryAdd; net11 Scrutor decoration about 3× faster than elsewhere; on net11 CseRuntime decorated resolve is faster than CseGenerated.
- Only two load readings exist (1.55 at the start, 1.90 at the end). Few-percent gaps, such as plain resolve on net10, are weak evidence.
- Scrutor's keyed-decoration limits come from reading its source at v7.0.0, not from running it.

## Possible angles

- "A scan you pay at every start": the cold table, generated registry 1.01-1.06× hand-written against Scrutor's 1.13-1.43× (cold, all TFMs), and AOT where the scan finds nothing.
- "Where CSE loses": the list above. Name the losses first.
- "Keyed services and decoration": what Scrutor's public `Decorate` cannot reach, and what decoration costs against a hand-written factory.
- "TryAdd is a scan": all three libraries pay the same linear scan per service (ManualTryAdd 3,200,071.1 ns at N=2000 on net10 against 30,856.2 ns for plain `AddScoped`), so the gap between them is what each adds around it. CseGenerated adds 1.448× there; Scrutor 2.070×.
