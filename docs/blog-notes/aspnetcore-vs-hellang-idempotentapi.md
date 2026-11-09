# Blog notes: CSharpEssentials.AspNetCore, Idempotency and RequestResponseLogging against the framework, Hellang, Marvin, SharpGrip, IdempotentAPI and Serilog

Notes for a post, not the post. Every number comes from `CSharpEssentials.AspNetCore/BENCHMARKS.md`, `CSharpEssentials.Idempotency/BENCHMARKS.md` and `CSharpEssentials.RequestResponseLogging/BENCHMARKS.md`, which copy the full default-job reports under `benchmarks/results/aspnetcore/{tfm}/results/` and `benchmarks/results/idempotency/{tfm}/results/`.

The suite runs on .NET 8, 9, 10 and 11 with BenchmarkDotNet 0.14.0. The libraries compared are plain ASP.NET Core minimal APIs and its HttpLogging middleware, Hellang.Middleware.ProblemDetails 6.5.1 (MIT), Marvin.Cache.Headers 7.2.0 (MIT), SharpGrip.FluentValidation.AutoValidation.Endpoints 2.0.0 (MIT) with FluentValidation 12.2.0 (Apache-2.0), IdempotentAPI 2.6.0 with IdempotentAPI.MinimalAPI 3.2.0 and IdempotentAPI.Cache.DistributedCache 2.2.0 (MIT in the repo; the nuspecs carry no license metadata), and Serilog.AspNetCore 10.0.0 (Apache-2.0).

## Framing

- Working title: "What does each ASP.NET Core add-on cost per request?"
- Every number is an overhead: the plain framework endpoint is the baseline, so a ratio reads as "the cost of adding X". The post should say that first, so it is not read as "which library is fastest".
- CSE bundles features that the other libraries each cover one of: problem details (Hellang), conditional GET (Marvin), validation (SharpGrip), idempotency (IdempotentAPI), request logging (HttpLogging, Serilog). The post compares feature by feature, never "CSE against all of them".

## What each library provides

- ASP.NET Core: `TypedResults`, `AddProblemDetails` with `UseExceptionHandler` and `UseStatusCodePages`, parameter binding, HttpLogging with `CombineLogs` and body limits, and on net10+ a DataAnnotations `AddValidation()`.
- Hellang.Middleware.ProblemDetails 6.5.1: a middleware that turns exceptions and empty error responses into problem+json, with `Map`/`MapToStatusCode` per exception type. It writes through the MVC `ObjectResult` executor, so MVC must be registered. Last release 2022.
- Marvin.Cache.Headers 7.2.0: a middleware that buffers the response, computes a strong MD5 ETag, writes `Cache-Control`, `Expires`, `Last-Modified` and `Vary`, and answers 304 and 412 from its validator store.
- SharpGrip.FluentValidation.AutoValidation.Endpoints 2.0.0: an endpoint filter that runs the registered FluentValidation validators and writes a validation problem.
- IdempotentAPI 2.6.0 / MinimalAPI 3.2.0: an endpoint filter that fingerprints the bound arguments, stores the result object as gzipped Newtonsoft JSON in `IDistributedCache` (or FusionCache), and rebuilds the result on replay. Last release 2024.
- Serilog.AspNetCore 10.0.0: one structured event per request with method, path, status and elapsed time.
- CSE: `ResultEndpointFilter` (`Result<T>` to HTTP), enhanced problem details with error codes, enum binding that accepts the JSON spelling in query and route values, conditional GET from `IVersioned` or a body hash, `WithValidation<T>`, an idempotency middleware over transport-independent stores and an executor, and a request/response body logger.

## Feature matrix

| Feature | CSE | Framework | Hellang 6.5.1 | Marvin 7.2.0 | SharpGrip 2.0.0 | IdempotentAPI 2.6.0 | Serilog.AspNetCore 10.0.0 |
|---|---|---|---|---|---|---|---|
| Exception to problem+json | Yes | Yes | Yes | n/a | n/a | n/a | n/a |
| Error codes in the problem body | Yes | No | No | n/a | n/a | n/a | n/a |
| `Result<T>` endpoint filter | Yes | No | No | n/a | n/a | n/a | n/a |
| Validation filter | CSE validators | net10+ DataAnnotations | n/a | n/a | FluentValidation | n/a | n/a |
| Enum JSON spelling in query/route | Yes | No | n/a | n/a | n/a | n/a | n/a |
| ETag from the body | Weak SHA-256, opt-in | No | n/a | Strong MD5 | n/a | n/a | n/a |
| 304 decided | After the handler | By hand | n/a | Before the handler | n/a | n/a | n/a |
| Idempotent replay | Response bytes | No | n/a | n/a | n/a | Result object, gzipped Newtonsoft | n/a |
| Replay header | `Idempotency-Replayed` | n/a | n/a | n/a | n/a | None | n/a |
| Idempotency without HTTP (executor, inbox) | Yes | No | n/a | n/a | n/a | No | n/a |
| Request/response bodies in the log | Yes, tee plus a full copy | Yes, tee, 32 KB default | n/a | n/a | n/a | n/a | No |
| Status code in the log | No | Yes | n/a | n/a | n/a | n/a | Yes |
| Needs MVC | No | No | Yes | No | No | Builds MVC filter contexts | No |

## Methodology

- One BenchmarkDotNet project, `benchmarks/CSharpEssentials.AspNetCore.Comparison.Benchmarks/`, with twelve classes, each with `[MemoryDiagnoser]`.
- Every variant runs on its own Production `TestServer` host; every request reads the whole response body; every handler bumps a counter that parity reads, so a short-circuited or replayed request is told apart from an executed one.
- Every class checks parity in `[GlobalSetup]`, and `--self-check` runs the same checks on every TFM without BenchmarkDotNet. Status, media type, body bytes (or the problem `status` member where bodies legitimately differ) and handler calls must match, or the run fails.
- Sized payloads (articles, replay responses, logged bodies) are pseudo-random `[a-z0-9]` from fixed seeds. Repeated characters would make IdempotentAPI's gzip artificially cheap.
- The logging hosts enable exactly the middleware's category through a counting provider or sink that renders every message, so no library skips work because logging is off.
- Rows that do different work are labelled, not presented as like-for-like: Marvin's 304 that skips the handler, the CSE problem bodies with extra fields, the framework rejecting the JSON enum spelling, CSE's default in-memory idempotency store, the HttpLogging rows with default 32 KB limits, and the metadata-only Serilog row.
- Every competitor row follows its documented usage and carries a link to that documentation.

## How these numbers were produced

- BenchmarkDotNet v0.14.0, default job (full runs), `[MemoryDiagnoser]`; SDK 11.0.100-rc.1; .NET 8.0.30, 9.0.19, 10.0.11 and 11.0.0 (11.0.26.42628), as the report headers print them. `IdempotentReplayComparison`, `RequestLoggingComparison`, `RequestLoggingMetadataComparison`, the four idempotency executor, store, contention and key classes and the `CSharpEssentials.Idempotency.Benchmarks` suite were rerun after #230-#256 with SDK 11.0.100-rc.3 and .NET 8.0.33, 9.0.22, 10.0.14 and 11.0.0 (11.0.26.52348). A range with no TFM named spans net8.0 to net11.0 (net9.0 to net11.0 for the `CSharpEssentials.Idempotency.Benchmarks` rows). Differences and multiples in the prose are computed from the means and allocations in the tables.
- One machine, a 6-core, 12-thread x64 desktop CPU on Linux; the suites ran one after another.
- `aspnetcore` suite: 1-minute load average 1.60 at the start and 2.02 at the end. Classes with a row above 5% CV were rerun up to twice, at a 1-minute load of 1.41 to 6.96. Three net11.0 `Bcl_Inline` validation baselines (CV 5.9% to 8.5%) stayed above 5% CV and are marked noisy in the package pages. In the later rerun, nine idempotent replay rows on net8.0 and net9.0 (seven of them 256 KB rows) and one net8.0 `HttpLogging_Defaults` row at 1 KB are above 5% CV and are marked noisy too.
- `CSharpEssentials.Idempotency.Benchmarks` suite (net9.0 to net11.0): two net9.0 rows are above 5% CV and are marked noisy.
- Reports: `benchmarks/results/aspnetcore/{tfm}/results/` and `benchmarks/results/idempotency/{tfm}/results/`. Full tables, with every row and TFM, are in the three package pages named above.

## The numbers that matter

Format: mean, ratio to the plain framework row, allocated. net10.0 unless another TFM is named.

1. ResultFilter. Success: filter 11.231 μs (1.19), 8.74 KB; hand-written match 9.395 μs (0.99); `TypedResults` 9.475 μs. On net8.0, net9.0 and net11.0 the filter is 0.99 to 1.02. Failure: filter 13.547 μs (1.23), manual match 12.940 μs (1.18), so most of the failure cost is the larger CSE problem body.
2. EnumBinding PassThrough: 0.98 to 1.04 of the plain request on every TFM; 0.06 KB more allocated on net8.0 and net9.0, none on net10.0 and net11.0.
3. ConditionalGet BodyHash200 at 64 KB: plain 29.195 μs, 73.75 KB; CSE 87.999 μs (3.02), 74.93 KB; Marvin 294.214 μs (10.08), 949.18 KB.
4. ProblemDetails Exception: framework 28.31 μs, CSE 29.68 μs (1.05), Hellang 28.05 μs (0.99). Hellang is faster than CSE on every TFM.
5. ValidationFilter Invalid, 3 rules: hand-written 17.03 μs, CSE 30.39 μs (1.79), 16 KB; SharpGrip 43.86 μs (2.58), 25 KB.
6. IdempotentReplay Replay at 256 KB: plain 103.25 μs, 268.93 KB; CSE 85.63 μs (0.83), 276.81 KB; IdempotentAPI 1,498.60 μs (14.52), 4517.6 KB.
7. RequestLogging Bodies at 1 MB: plain 432.244 μs; CSE 2,107.251 μs (4.88), 9289.16 KB; HttpLogging_Bodies 4,256.239 μs (9.85), 19045.49 KB.

## Where CSE loses (state it, do not soften it)

- Problem details for an exception: Hellang is faster on every TFM. Hellang against CSE: 50.37 against 58.12 μs (net8.0), 36.17 against 38.95 μs (net9.0), 28.05 against 29.68 μs (net10.0), 26.97 against 28.18 μs (net11.0).
- Marvin's 304 skips the handler. BodyHash304 at 64 KB: Marvin 11.743 μs, CSE 63.311 μs (net10.0); Marvin is 4.0 to 5.4 times faster on every TFM. CSE runs the handler, serializes and hashes 64 KB, so its 304 takes longer than the plain 200 that sends the whole body (30.608 μs). That is a design difference, and the post should say so. At 1 KB on net11.0 Marvin is also faster (16.05 against 16.86 μs).
- Request logging with small bodies: at 1 KB, CSE is slower than HttpLogging_Bodies on every TFM (net10.0: 16.685 against 13.661 μs). It allocates less on net8.0 to net10.0 and more on net11.0 (23.8 against 22.8 KB).
- HttpLogging with its defaults: at 1 KB it logs both bodies plus headers and is still faster than CSE on every TFM (net10.0: 13.993 against 16.685 μs); it allocates less only on net11.0. From 64 KB it truncates each body at 32 KB, so it does less work.
- Metadata-only logging: CSE allocates a little more than HttpLogging on every TFM (10.22 against 10.17 KB on net10.0); time is within error on every TFM (net10.0: 8.724 against 8.918 μs).
- Result filter on net10.0: 1.19 of `TypedResults` on Success, where the other TFMs show 0.99 to 1.02.
- Deterministic keys: `ToUuidV5` is 1.08 to 1.11 of a hand-written UUIDv5 on net9.0 to net11.0 and allocates 152 to 184 B against none. On net8.0 (the netstandard2.1 build) it is 1.77 and 488 B.

## Where CSE wins or ties

Measured, every TFM:
- Idempotent replay against IdempotentAPI: CSE is faster and allocates less at 1 KB and 256 KB, for a replay and for a new key (net10.0, 1 KB replay: 14.34 μs against 92.90 μs).
- Validation against SharpGrip: CSE is faster and allocates less in all eight rows (net10.0, 1 rule, Valid: 15.80 μs against 29.39 μs).
- Empty error status against Hellang: CSE 1.01 to 1.17 of the framework, Hellang 1.26 to 1.82.
- Conditional GET 200 with a body-hash ETag against Marvin: CSE is faster and allocates far less at 1 KB and 64 KB.
- Request logging at 64 KB and 1 MB against HttpLogging_Bodies: CSE is faster and allocates less. The metadata row is faster than Serilog (net10.0: 8.724 against 10.866 μs).
- Small overhead: `UseEnumBinding` on an endpoint without enums (0.98 to 1.04), and `IVersioned` ETags (1.00 to 1.12 of a hand-written ETag).

Features only CSE ships (not a speed claim):
- Error codes in every problem body, and `Result<T>` straight to HTTP.
- The JSON enum spelling accepted in query and route values.
- Idempotency without HTTP: the executor and the message inbox.

## Caveats to keep in the post

- `TestServer`, not Kestrel: sockets, TLS and HTTP parsing are not measured, so on a real network these overheads are a smaller share of each request.
- Hellang 6.5.1 is a 2022 net6.0 build; IdempotentAPI's last release is from 2024 and its nuspecs carry no license metadata (the repo is MIT).
- IdempotentAPI fingerprints injected services too: a handler that injects a service whose serializable state changes gets a 400 on every retry. The suite captures its counter for that reason; the post can mention it as a usage note.
- In a long NewKey run, `MemoryDistributedCache` reaches its 200 MB limit and refuses writes for both libraries; the self-check showed that the replay of a fresh key still works after that.
- The logging rows render messages but write them nowhere, so sink I/O is not included for any library.
- Seven 256 KB idempotent replay rows on net8.0 and net9.0 are above 5% CV (for example the plain replay baseline on net8.0, 187.21 μs, CV 14.8%). Quote net10.0 or net11.0 for those rows.
- The CSE column is the working tree on branch `integ/all`, not a NuGet release.

## Possible angles

- "What each add-on costs": a per-feature overhead table against the plain framework endpoint.
- "Who answers the 304": Marvin's validator store against CSE's after-the-handler check, with the 64 KB numbers.
- "Storing bytes or storing objects": the two idempotency designs and what replay costs in each.
- "Teeing the response": CSE and HttpLogging both tee; CSE keeps a full copy, HttpLogging a 32 KB prefix by default, and the 1 MB rows show what each costs.
