# Blog notes: CSharpEssentials.DependencyInjection vs Scrutor vs manual registration

Raw material for a post. These notes are not the post. Every number comes from `CSharpEssentials.DependencyInjection/BENCHMARKS.md`, which reads `benchmarks/results/di/{tfm}/results/*.csv` and `benchmarks/results/di/aot/aot-report.md`. The machine was an AMD Ryzen 5 5600 running a desktop session, with a 1-minute load average of 1.09-1.76 at the sampled points (net10 ran first, after a busier period: 5-minute average 3.68). The SDK was 11.0.100-rc.1, with BenchmarkDotNet 0.14.0. Primary runtime is .NET 10.0.11. Competitor: Scrutor 7.0.0. Microsoft.Extensions.DependencyInjection 10.0.0.

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

1. Cold startup (fresh process, register + build + resolve every service once). N=50: Manual 24.88 ms, CseGenerated 26.24 ms (1.06×), Scrutor 34.75 ms (1.40×), CseReflection 38.21 ms (1.54×). N=500: 55.70 / 57.83 / 63.89 / 71.20 ms. Same order on every TFM.
2. The generated registry is close to hand-written code, but not equal to it. Cold: +1.36 ms at N=50 and +2.13 ms at N=500, outside error on every TFM except net11 N=50. Warm N=50: 763.58 vs 626.93 ns (1.22×); warm N=500 is a tie on net10 and CseGenerated is faster on net8, net9 and net11.
3. The reflection fallback loses to Scrutor. Cold 38.21 vs 34.75 ms and 71.20 vs 63.89 ms; 1.85× Scrutor's allocation (335.52 vs 181.4 KB at N=50, measured on a 2nd in-process invocation). Warm 2.2-2.3× slower and 3.6-3.7× more allocation (N=50 / N=500).
4. Decoration registration (50 manual registrations + 10 decorations). Manual 952.7 ns / 4.9 KB, CseGenerated 3,572.7 ns / 10.83 KB, CseRuntime 4,817.0 ns / 13.32 KB, Scrutor 11,877.3 ns / 11.46 KB. CseRuntime allocates more than Scrutor on every TFM, and on net11 Scrutor (3,980.5 ns) is faster than CseRuntime (4,581.9 ns).
5. Plain resolve ties on net10: 132.6 / 134.5 / 134.4 ns, 360 B each. Exceptions: net9 CseGenerated 156.5 vs 147.7 ns (1.06×); net8 Scrutor 150.9 vs 146.4 ns (1.03×). Decorated resolve: Manual factory 110.1 ns, CseGenerated 164.6 ns (1.49×), CseRuntime 252.2 ns (2.29×), Scrutor 309.6 ns (2.81×).
6. Native AOT (net10, linux-x64). Manual and CseGenerated: 0 IL warnings, 2.2 MB, run and resolve all 50 services. Scrutor: 12 IL warnings, crashes with "Could not find any registered services for type 'IService0'". CseReflection: 4 IL warnings, crashes with "must have exactly one public constructor, but has 0" (documented as not AOT-safe). No sample roots the input assemblies.

## The reflection finding (state it, do not soften it)

- The Readme says the generated registry "uses no reflection" ("Generated registry" section) and is "reflection-free" ("Migrating from Scrutor"). For registration that holds.
- For decoration it does not. Generated `[Decorates]` code calls `Decorate<T>(factory)`, which runs `ServiceDecoration.TryDecorate` (`Internal/ServiceDecoration.cs:20`). For every matched descriptor `EnsureDecoratable` (lines 90-108) calls `GetConstructors()`, `GetParameters()` and `GetCustomAttribute<ServiceKeyAttribute>()` on the implementation type. Each `Decorate` call also loops over the whole collection.
- It is metadata reflection only: no dynamic code, 0 AOT warnings, and the AOT sample runs. Accurate wording: "reflection-free activation; decoration reads constructor metadata".
- Not fixed in this work. A follow-up is proposed: decide the `[ServiceKey]` check at generation time and skip it on the generated path, or correct the Readme.

## Honest caveats to keep in the post

- Warm registration favours the reflection rows (types loaded, caches filled, scanning code JIT-compiled). Quote the cold table for startup.
- The manual decoration baseline builds the inner service with `new`; CSE and Scrutor resolve it from a hidden keyed registration. Part of the decorated-resolve gap is that extra resolve.
- Plain and Decorated resolve rows measure different services; do not compare across the two.
- The shared input classes carry `[RegisterScoped]` and the input assembly holds the CSE registry, so the Scrutor process also loads the CSE assembly. Small, and borne by Scrutor.
- Scrutor's attribute mode was not measured: it would need different attributes on the shared inputs. Scrutor uses `AsMatchingInterface()` ([IServiceTypeSelector.cs L55-L58](https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/IServiceTypeSelector.cs#L55-L58)).
- AOT warning counts use `TrimmerSingleWarn=false` (higher than a default publish). Rooting the inputs with `TrimmerRootAssembly` was not tested. The 3 ms AOT wall times were taken at load 1.72 and are not a result.
- Outliers not investigated: net8 warm CseReflection 2.1-2.7× its other TFMs; net11 Scrutor decoration about 3× faster than elsewhere; CseRuntime decorated resolve on net10 (252.2 ns vs 203.0-220.3 ns elsewhere), where net10 ran at the highest load.
- Scrutor's keyed-decoration limits come from reading its source at v7.0.0, not from running it.

## Possible angles

- "A scan you pay at every start": the cold table, generated registry 1.04-1.06× hand-written against Scrutor's 1.15-1.42× (cold, all TFMs), and AOT where the scan finds nothing.
- "Where CSE loses": the reflection fallback against Scrutor, the generated registry against hand-written code, decoration against a hand-written factory, CseRuntime allocation and net11 decoration against Scrutor, and the metadata reflection on the generated decoration path. Name the losses first.
- "Keyed services and decoration": what Scrutor's public `Decorate` cannot reach, and what that costs CSE per decoration.

## Newer scenarios (no numbers yet)

Two classes were added after the run above, TryAdd registration and transient decorator resolve. They build and pass their parity check on every TFM, but no full run has been done. `CSharpEssentials.DependencyInjection/BENCHMARKS.md` describes them under "Pending". Do not quote numbers for them until the full run is committed.

What each one asks:
- TryAdd. Register 50, 500 or 2000 services that must not replace an existing registration. Rows: hand-written `TryAddScoped`, the CSE generated registry with `Strategy = RegistrationStrategy.TryAdd`, the CSE reflection fallback, and Scrutor with `RegistrationStrategy.Skip`. Plain `AddScoped` is there as a reference with no duplicate check.
- Transient decorator resolve. Resolve a decorated transient service from the root provider, with no scope. Every resolve builds the decorator again, so the activation path is what the rows show.

Things to say when the numbers exist:
- TryAdd is O(N^2) on an empty collection in all three libraries. Each one scans the collection for every service it adds. N = 2000 shows that. It is not a CSE-only cost.
- The warm caveat from the registration section applies: the reflection rows look cheaper warm than at a real startup.
- CSE's runtime `Decorate<TService, TDecorator>` builds the decorator with `ConstructorInfo.Invoke` and a new argument array on every resolve. The generated `[Decorates]` path calls the constructor directly. Scrutor uses `ActivatorUtilities.CreateInstance`.
- The manual factory builds the inner service with `new`. CSE and Scrutor resolve it from a hidden keyed registration, so part of any gap is that extra resolve.

Expected losses (name them first):
- TryAdd: CseGenerated should be slower than hand-written `TryAddScoped`. Both scan once per service, but CSE's `FindFirst` (`ServiceRegistration.cs:119-130`) runs `foreach` over `IServiceCollection`, which allocates an enumerator per call and makes interface calls per element.
- Transient decorator resolve: CseGenerated and CseRuntime should be slower than the Manual factory. Both resolve the inner service from an extra keyed registration. CseRuntime also pays `ConstructorInfo.Invoke` and a new argument array. The committed scoped decorated-resolve rows already go this way.

Possible angle:
- "TryAdd is a scan": all three libraries pay the same linear scan per service, so the gap between them is what each adds around it.
