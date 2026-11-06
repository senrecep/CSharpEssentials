# Blog notes: CSharpEssentials.Endpoints vs FastEndpoints vs Carter

Raw material for a post, not the post itself. Every number comes from `CSharpEssentials.Endpoints/BENCHMARKS.md`, which reads `benchmarks/results/endpoints/{tfm}/results/*-report-github.md` and `benchmarks/results/endpoints/aot/aot-report.md`.

How these numbers were produced: BenchmarkDotNet 0.14.0, full runs of the endpoints suite (default job, plus a cold-start job for `ColdStartupBenchmarks`) on net8.0, net9.0, net10.0 and net11.0, run on 2026-11-05 with the net9.0 throughput class rerun on 2026-11-06. AMD Ryzen 5 5600 with a desktop session in the background; 1-minute load average 2.18 to 4.72 over the suite. SDK 11.0.100-rc.1. Primary runtime is .NET 10.0.11. No row of this suite stayed above 5% CV after reruns. The Native AOT numbers come from an earlier script run on 2026-11-01 (load average 2.84).

## The problem

- Minimal APIs scale badly as a code layout. A large `Program.cs`, or extension methods split by hand, ends up with hundreds of `app.MapX` calls.
- The usual fixes add a framework.
  - Carter: modules found by assembly scanning and created through DI.
  - FastEndpoints: a base class per endpoint, its own binder, its own pipeline, and an instance per request.
- The question to answer: can you get the organization (one type per endpoint, groups, discovery) without giving up plain Minimal APIs, and without paying for it at startup, per request, or under Native AOT?

## What the framework already gives you

- `MapGroup` / `RouteGroupBuilder` for prefixes, tags, filters and auth on a group of routes.
- `IResult` / `TypedResults`, endpoint filters, and OpenAPI metadata.
- .NET 10 `AddValidation()` with DataAnnotations.
- The Request Delegate Generator (RDG). It generates binding code at compile time and turns on automatically with `PublishAot`.
- What is missing is discovery and structure. Nothing finds your endpoint types and maps them. Nothing stops endpoints from keeping state. Nothing gives a group a type you can attach endpoints to.

## How each library fills the gap

- CSharpEssentials.Endpoints:
  - You write an `IEndpoint` type with a static abstract `Map(IEndpointRouteBuilder)`.
  - Groups are `IEndpointGroup` types plus `[EndpointGroup<T>]`.
  - A source generator writes a registry, so mapping is `app.Map{Asm}Endpoints()`.
  - No DI registration, no instances, no runtime packages beyond the shared framework.
  - The route calls stay in user code, so binding, filters, OpenAPI and RDG are the stock ones.
  - An analyzer warns about instance state (CSE1004) and duplicate routes (CSE1005).
  - A reflection scanner (`MapEndpointsFromAssemblies`) exists as an opt-in fallback. It is annotated as not trim/AOT safe.
- Carter 10.0.0:
  - `ICarterModule` with `AddRoutes`, optionally the `CarterModule` base class.
  - Modules are found through `DependencyContext` + `GetTypes()` and registered as DI singletons.
  - Routes are still plain Minimal APIs.
  - Brings in FluentValidation, Microsoft.Extensions.DependencyModel and Carter.Analyzers. The analyzers package depends on the Roslyn compiler package; whether that DLL reaches publish output was not checked.
- FastEndpoints 8.3.0:
  - A base class per endpoint, assembly scanning by default, and an optional source generator.
  - Its own request binder.
  - Each endpoint is instantiated once at startup and once per request.
  - Its own groups and OpenAPI packages.
  - FluentValidation built in.
  - Native AOT is documented only with FastEndpoints.Generator.

## The numbers (net10 unless stated)

Cold startup is the fair startup figure: a fresh process, timed until every endpoint has answered once. Warm startup is not comparable, because FastEndpoints defers per-endpoint setup to the first request.

1. Cold start, 10 endpoints: 164.9 ms for CSE vs 213.5 ms for FastEndpoints. CSE takes 23% less time, and the direction holds on net8, net9 and net11 (FastEndpoints takes 1.26-1.31× the CSE time). Plain Minimal APIs take 160.4 ms and Carter 167.6 ms, both ties with CSE.
2. Cold start, 500 endpoints: 1,116.3 ms for CSE vs 688.4 ms for FastEndpoints. CSE takes 1.62× as long. This is a CSE loss, and it holds at N=100 too (339.6 vs 296.7 ms) on every TFM. Plain Minimal APIs take 1,041.8 ms (1.51× FastEndpoints). This points to RequestDelegateFactory building each handler at startup, which CSE inherits; not profiled. RDG was off in these JIT runs, and its effect was not measured. CSE itself adds a ratio of 1.07 over Plain at N=500 (1.06-1.08 across TFMs).
3. Per request, GET: 8.855 μs for CSE, 8.723 μs Plain, 8.907 μs Carter (all ties), 11.165 μs FastEndpoints. FastEndpoints takes 1.26× as long and allocates 10.07 KB vs 8.69 KB.
4. Invalid POST: 24.369 μs for CSE vs 50.133 μs for FastEndpoints, which takes 2.06× as long and throws 2 exceptions per request. Plain Minimal APIs answer the same invalid POST in 23.880 μs, a tie with CSE. Carter takes 32.138 μs. On a valid POST, Carter takes 10% less time than CSE (15.620 vs 17.325 μs) and allocates less (11.38 vs 11.67 KB).
5. Native AOT (net10, linux-x64, 2026-11-01 run):
   - CseGenerated publishes with 0 IL warnings, a 10.4 MB binary and 72 ms to the first 200.
   - Plain: 10.3 MB, 71 ms.
   - FastEndpoints + Generator: 0 warnings, 15.2 MB, 81 ms.
   - Carter (8 warnings) and FastEndpoints with reflection discovery (4 warnings) crash at startup with this setup.
   - CSE's own reflection scanner publishes with 4 warnings, and its routes return 404/405.
6. Operation names, 500 endpoints: reading every `OperationNaming.TypeName` name takes 12.280 μs against 6.381 μs for hand-written `WithName` names (1.92×) and 7.396 μs for `Custom` names. A CSE loss: every TypeName read takes a lock.

## Where CSE loses (keep this list in the post)

- Cold start at N=100 and N=500 against FastEndpoints, on every TFM.
- Cold start against Plain at N≥100, every TFM (ratio 1.02-1.08), and at N=10 on net9 (167.9 vs 164.1 ms).
- Cold allocation against Plain: N=10 on net8 (1175.2 KB vs 978.4 KB), net9 and net10 (alloc ratio 1.01), and N=100 and N=500 on every TFM (alloc ratio 1.03-1.06). Against Carter: net10 at every N and net11 at N=100 and N=500 (net11 N=500: 22.98 vs 22.69 MB).
- Warm startup at N=500: against Plain on net8 (509.850 vs 501.513 ms), against Carter on net10 (721.454 vs 710.012 ms); 4.3-4.9% more allocation than Plain on every TFM. Warm allocation is also above Plain at N=10 and N=100 on every TFM, and above Carter on net10 at N=100 and N=500 and on net11 at N=10.
- Valid POST against Carter on net10 (17.325 vs 15.620 μs) and net11 (20.02 vs 18.82 μs).
- Per-request allocation 0.01 KB above Plain on valid POST (net10, net11) and on DELETE (net11), and above Carter on DELETE (net11: 9.32 vs 9.31 KB).
- Name reads: TypeName 1.67-1.92× and Custom 1.16-1.22× the hand-written `WithName` read time.
- Naming at build: TypeName takes longer than None on net8, net10 and net11 and allocates 1.07-1.08× as much (net10: 16.83 vs 15.62 MB).
- The per-type `MapGroup("")` allocates 3-4% more than direct mapping (net10: 15.6 vs 15.1 MB); its time is a tie on every TFM.

## Caveats to keep in the post

- The startup cost of CSE is mostly the startup cost of Minimal APIs. At 500 endpoints FastEndpoints starts faster likely because it skips RequestDelegateFactory (each route maps to one parameterless delegate); this was inferred from source, not profiled. Do not frame CSE as the fastest at startup. It ties Carter on net10 and net11 at every N, and takes 2-8% longer than Plain at N≥100 on every TFM.
- On net10, the source-generated registry gives no measurable cold-start gain over CSE's own reflection scanner (1,116.3 vs 1,123.2 ms at N=500, a tie). The generator pays off in Native AOT and trimming safety, not in startup time.
- Validation engines differ: DataAnnotations + `AddValidation()` for Plain and CSE, FluentValidation for Carter and FastEndpoints. The POST rows compare whole stacks, and they exist only on net10 and net11.
- Machine load was 2.18 to 4.72. CseGenerated and CseReflection run the same request pipeline, yet differ by up to 8.6% in throughput (net11 POST valid), so per-request gaps under about 10% are weak evidence. Differences within error are reported as ties.
- Cold-startup allocation comes from BenchmarkDotNet's separate MemoryDiagnoser run, not from the timed launch. At N=500 FastEndpoints allocates 1.6× more (35.97 MB vs 22.4 MB).
- The AOT startup times are a median of 5 on a loaded machine, with three routes, from the 2026-11-01 run. The Carter and FastEndpoints reflection crashes are what happened with these samples. Carter documents no AOT approach, and FastEndpoints documents AOT only with its Generator.
- Carter was benchmarked with one module per endpoint, to match CSE's one type per endpoint. Real Carter apps often put several routes in one module.
- FastEndpoints brings features CSE does not try to cover (its own binder, messaging, job queues). This comparison covers organization, startup, per-request cost and AOT, not feature breadth.

## Possible angles

- "Organization without a framework": a thin layer over Minimal APIs. For: ties Plain on every throughput route and TFM, ties Carter in cold startup on net10 and net11, 0 IL warnings under AOT, 10.4 vs 10.3 MB. Against: 2-8% cold startup over Plain at N≥100, 4.3-4.9% more warm allocation at N=500, and Carter is faster on a valid POST.
- "Where FastEndpoints wins": large-N cold start, and why (it does not use RequestDelegateFactory per handler; Plain Minimal APIs show the same large-N cost, 1,041.8 ms at N=500 on net10; not profiled, and RDG was off).
- "What the source generator is for": AOT and trimming, shown by the CseReflection 404s, not startup time.
- "What naming and the per-type group cost at 500 endpoints" (next section).

## Operation naming and the empty-prefix group (500 endpoints)

These three classes ran in the same full run. `CSharpEssentials.Endpoints/BENCHMARKS.md` has the full tables under "Operation naming and the empty-prefix group".

What each one asks:
- Operation naming. What does an endpoint name cost at startup? CSE's `OperationNaming.None`, `TypeName` and `Custom(type.Name)` on the generated library, and plain `MapGet` with and without `WithName(...)`. Every row reads every name, because TypeName resolves names on the first read.
- Name reads. What does reading 500 names cost once the app is built? TypeName names against plain `EndpointNameMetadata` names.
- The empty-prefix group. CSE wraps each endpoint type in `MapGroup("")`. What does that cost against mapping straight onto the app, with the same routes and handlers?

What the numbers say (net10 unless stated):
- Naming at build: None 703.9 ms, TypeName 714.4 ms (ratio 1.01), Custom 711.4 ms (1.01). TypeName vs Custom is a tie on net9, net10 and net11 and slower only on net8 (523.0 vs 507.6 ms). TypeName allocates 16.83 MB against 15.62 MB for None. Hand-written: 747.7 ms without names, 753.5 ms with `WithName` (compare inside a category only).
- Name reads: TypeName 12.280 μs, Custom 7.396 μs, hand-written 6.381 μs for 500 reads. TypeName is the slowest on every TFM (1.67-1.92× the hand-written read). The lock on every read was predicted from the source before the run.
- The empty-prefix group: 759.5 ms grouped vs 757.0 ms direct, a tie on every TFM. It allocates 15.6 MB vs 15.1 MB (alloc ratio 1.03-1.04 across TFMs).

Things to say:
- FastEndpoints always names its endpoints and has no switch to turn that off, so it has no row. Its naming cost is inside its startup numbers above.
- Carter has no naming policy. Its users call `WithName` per route, which the hand-written row covers. Carter's `CarterModule` base class wraps each module in a group too. That was read from the source, not measured.
- The whole CSE startup cost against plain Minimal APIs is in the startup numbers above. The group row isolates one part of it: allocation, not time.
