# Blog notes: CSharpEssentials.Endpoints vs FastEndpoints vs Carter

Raw material for a post. These notes are not the post. Every number comes from `CSharpEssentials.Endpoints/BENCHMARKS.md`, which reads `benchmarks/results/endpoints/{tfm}/results/*.csv` and `benchmarks/results/endpoints/aot/aot-report.md`. The machine was an AMD Ryzen 5 5600 running a desktop session, with a 1-minute load average of 2.8-3.9 at the sampled points. The SDK was 11.0.100-rc.1, with BenchmarkDotNet 0.14.0. Primary runtime is .NET 10.0.11.

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

- **CSharpEssentials.Endpoints.**
  - You write an `IEndpoint` type with a static abstract `Map(IEndpointRouteBuilder)`.
  - Groups are `IEndpointGroup` types plus `[EndpointGroup<T>]`.
  - A source generator writes a registry, so mapping is `app.Map{Asm}Endpoints()`.
  - No DI registration, no instances, no runtime packages beyond the shared framework.
  - The route calls stay in user code, so binding, filters, OpenAPI and RDG are the stock ones.
  - An analyzer warns about instance state (CSE1004) and duplicate routes (CSE1005).
  - A reflection scanner (`MapEndpointsFromAssemblies`) exists as an opt-in fallback. It is annotated as not trim/AOT safe.
- **Carter 10.0.0.**
  - `ICarterModule` with `AddRoutes`, optionally the `CarterModule` base class.
  - Modules are found through `DependencyContext` + `GetTypes()` and registered as DI singletons.
  - Routes are still plain Minimal APIs.
  - Brings in FluentValidation, Microsoft.Extensions.DependencyModel and Carter.Analyzers. The analyzers package depends on the Roslyn compiler package; whether that DLL reaches publish output was not checked.
- **FastEndpoints 8.3.0.**
  - A base class per endpoint, assembly scanning by default, and an optional source generator.
  - Its own request binder.
  - Each endpoint is instantiated once at startup and once per request.
  - Its own groups and OpenAPI packages.
  - FluentValidation built in.
  - Native AOT is documented only with FastEndpoints.Generator.

## The numbers that matter (net10 unless stated)

Cold startup is the fair startup figure: a fresh process, timed until every endpoint has answered once. Warm startup is not comparable, because FastEndpoints defers per-endpoint setup to the first request.

1. **Cold start, 10 endpoints: 169.6 ms for CSE vs 218.7 ms for FastEndpoints.** CSE takes 22% less time, and the direction holds on net8, net9 and net11. Plain Minimal APIs take 167.1 ms and Carter 171.4 ms, both ties with CSE.
2. **Cold start, 500 endpoints: 1,120.5 ms for CSE vs 678.9 ms for FastEndpoints.** CSE takes 1.65× as long. This is a CSE loss, and it holds at N=100 too (353.8 vs 313.9 ms) on every TFM. Plain Minimal APIs take 1,080.3 ms (1.59× FastEndpoints, vs 1.65× for CSE). This points to RequestDelegateFactory building each handler at startup, which CSE inherits; not profiled. RDG was off in these JIT runs, and its effect was not measured.
3. **Per request, GET: 8.227 µs for CSE, 8.297 µs Plain, 9.023 µs Carter, 10.126 µs FastEndpoints.** FastEndpoints takes 1.23× as long and allocates 10.06 KB vs 8.69 KB.
4. **Invalid POST: 22.027 µs for CSE vs 46.300 µs for FastEndpoints**, which takes 2.10× as long and throws 2 exceptions per request. The other side: plain Minimal APIs answer the same invalid POST in 19.757 µs, 10% less than CSE, with the same validation engine. Carter is 4% faster than CSE on a valid POST (14.141 vs 14.729 µs).
5. **Native AOT (net10, linux-x64):**
   - CseGenerated publishes with 0 IL warnings, a 10.4 MB binary and 72 ms to the first 200.
   - Plain: 10.3 MB, 71 ms.
   - FastEndpoints + Generator: 0 warnings, 15.2 MB, 81 ms.
   - Carter (8 warnings) and FastEndpoints with reflection discovery (4 warnings) crash at startup with this setup.
   - CSE's own reflection scanner publishes with 4 warnings, and its routes return 404/405.

## Honest caveats to keep in the post

- The startup cost of CSE is the startup cost of Minimal APIs. At 500 endpoints FastEndpoints starts faster likely because it skips RequestDelegateFactory (each route maps to one parameterless delegate); this was inferred from source, not profiled. Do not frame CSE as the fastest at startup. It is about equal to plain Minimal APIs and Carter: within error on net10, about 7% over Plain at N=500 on net8, net9 and net11.
- On net10, the source-generated registry gives no measurable cold-start gain over CSE's own reflection scanner (1,120.5 vs 1,110.5 ms at N=500, a tie). What the generator buys is Native AOT and trimming safety, not milliseconds.
- Validation engines differ: DataAnnotations + `AddValidation()` for Plain and CSE, FluentValidation for Carter and FastEndpoints. The POST rows compare whole stacks, and they exist only on net10 and net11.
- Machine load was 2.8-3.9. CseGenerated and CseReflection run the same request pipeline, yet differ by 3-11% in throughput, so per-request gaps under about 10% are weak evidence. Differences within error are reported as ties.
- Cold-startup allocation comes from BenchmarkDotNet's separate MemoryDiagnoser run, not from the timed launch. At N=500 FastEndpoints allocates 1.6× more (36,798 KB vs 22,749 KB).
- The AOT startup times are a median of 5 on a loaded machine, with three routes. The Carter and FastEndpoints reflection crashes are what happened with these samples. Carter documents no AOT approach, and FastEndpoints documents AOT only with its Generator.
- Carter was benchmarked with one module per endpoint, to match CSE's one type per endpoint. Real Carter apps often put several routes in one module.
- FastEndpoints brings features CSE does not try to cover (its own binder, messaging, job queues). This comparison covers organization, startup, per-request cost and AOT, not feature breadth.

## Possible angles

- "Organization without a framework": a thin layer over Minimal APIs. For: ties Plain in cold startup on net10 and in GET/DELETE on every TFM except net9 DELETE (Plain 2% faster); 0 IL warnings under AOT, 10.4 vs 10.3 MB. Against: +3-7% cold startup over Plain at N≥100 on net8/net9/net11, +17% cold allocation at N=10 on net10 (1,119 vs 959 KB), +4-5% allocation at N=500, and 1.11× Plain's time on an invalid POST with the same validation engine.
- "Where FastEndpoints wins": large-N cold start, and why (it does not use RequestDelegateFactory per handler; Plain Minimal APIs show the same large-N cost, 1,080.3 ms at N=500; not profiled, and RDG was off).
- "What the source generator is for": AOT and trimming, shown by the CseReflection 404s, not startup time.
