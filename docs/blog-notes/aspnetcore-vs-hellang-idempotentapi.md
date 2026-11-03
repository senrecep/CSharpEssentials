# Blog notes: CSharpEssentials.AspNetCore, Idempotency and RequestResponseLogging against the framework, Hellang, Marvin, SharpGrip, IdempotentAPI and Serilog (draft)

These are notes for a post, not the post. The numbers will come from `CSharpEssentials.AspNetCore/BENCHMARKS.md`, `CSharpEssentials.Idempotency/BENCHMARKS.md` and `CSharpEssentials.RequestResponseLogging/BENCHMARKS.md` once the full default-job run is committed under `benchmarks/results/aspnetcore/{tfm}/results/`. There are no numbers yet.

The suite runs on .NET 8, 9, 10 and 11 with BenchmarkDotNet 0.14.0. The libraries compared are plain ASP.NET Core minimal APIs and its HttpLogging middleware, Hellang.Middleware.ProblemDetails 6.5.1 (MIT), Marvin.Cache.Headers 7.2.0 (MIT), SharpGrip.FluentValidation.AutoValidation.Endpoints 2.0.0 (MIT) with FluentValidation 12.2.0 (Apache-2.0), IdempotentAPI 2.6.0 with IdempotentAPI.MinimalAPI 3.2.0 and IdempotentAPI.Cache.DistributedCache 2.2.0 (MIT in the repo; the nuspecs carry no license metadata), and Serilog.AspNetCore 10.0.0 (Apache-2.0).

## Framing

- Working title: "What does each ASP.NET Core add-on cost per request?"
- Every number is an overhead: the plain framework endpoint is the baseline, so a ratio reads as "the cost of adding X". The post should say that first, so it is not read as "which library is fastest".
- CSE bundles features that the other libraries each cover one of: problem details (Hellang), conditional GET (Marvin), validation (SharpGrip), idempotency (IdempotentAPI), request logging (HttpLogging, Serilog). The post compares feature by feature, never "CSE against all of them".

## What each library provides

- **ASP.NET Core:** `TypedResults`, `AddProblemDetails` with `UseExceptionHandler` and `UseStatusCodePages`, parameter binding, HttpLogging with `CombineLogs` and body limits, and on net10+ a DataAnnotations `AddValidation()`.
- **Hellang.Middleware.ProblemDetails 6.5.1:** a middleware that turns exceptions and empty error responses into problem+json, with `Map`/`MapToStatusCode` per exception type. It writes through the MVC `ObjectResult` executor, so MVC must be registered. Last release 2022.
- **Marvin.Cache.Headers 7.2.0:** a middleware that buffers the response, computes a strong MD5 ETag, writes `Cache-Control`, `Expires`, `Last-Modified` and `Vary`, and answers 304 and 412 from its validator store.
- **SharpGrip.FluentValidation.AutoValidation.Endpoints 2.0.0:** an endpoint filter that runs the registered FluentValidation validators and writes a validation problem.
- **IdempotentAPI 2.6.0 / MinimalAPI 3.2.0:** an endpoint filter that fingerprints the bound arguments, stores the result object as gzipped Newtonsoft JSON in `IDistributedCache` (or FusionCache), and rebuilds the result on replay. Last release 2024.
- **Serilog.AspNetCore 10.0.0:** one structured event per request with method, path, status and elapsed time.
- **CSE:** `ResultEndpointFilter` (`Result<T>` to HTTP), enhanced problem details with error codes, enum binding that accepts the JSON spelling in query and route values, conditional GET from `IVersioned` or a body hash, `WithValidation<T>`, an idempotency middleware over transport-independent stores and an executor, and a request/response body logger.

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
| Request/response bodies in the log | Yes, full response buffering | Yes, tee, 32 KB default | n/a | n/a | n/a | n/a | No |
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

## The numbers that matter

Pending the full default-job run. These are the rows to pull:

1. ResultFilter Success and Failure: the filter against `TypedResults` and against a hand-written match with CSE types.
2. EnumBinding PassThrough: what `UseEnumBinding` costs on an endpoint without an enum.
3. ConditionalGet BodyHash200 at 64 KB: CSE's second serialization against Marvin's buffering; allocated bytes matter as much as time.
4. ProblemDetails Exception: CSE against the framework and Hellang.
5. ValidationFilter Invalid with 3 rules: CSE against SharpGrip and the hand-written baseline.
6. IdempotentReplay Replay at 256 KB: stored bytes against gzipped Newtonsoft and re-serialization.
7. RequestLogging Bodies at 1 MB: CSE's full buffering against HttpLogging's tee.

## Where CSE is expected to lose (state it, do not soften it)

These predictions come from reading the code. Nothing has been measured yet.

- **The Result filter uses uncached reflection on every request** (`CSharpEssentials.AspNetCore/ResultEndpointFilter/ResultEndpointFilter.cs:22-31`): a service lookup for the error mapper also on success, `GetGenericTypeDefinition`, and three `GetProperty().GetValue()` calls, one boxing a bool. The ManualMatch row shows how much of the cost is the filter.
- **Request logging copies and buffers.** CSE copies the request body several times (`CSharpEssentials.RequestResponseLogging/Infrastructure/Middlewares/BaseMiddleware.cs:153-178`) and buffers the whole response before it sends a byte (`:204-247`), where HttpLogging tees the stream. At 1 MB this should show in both time and allocations. CSE also builds the message before it checks whether logging is enabled (`LogWriters/LoggerFactoryLogWriter.cs:34` against `:41`/`:44`).
- **Conditional GET serializes twice.** The body-hash ETag serializes the whole value to a `byte[]` just to hash it (`CSharpEssentials.AspNetCore/ConditionalRequests/DefaultETagGenerator.cs:29`).
- **Marvin's 304 skips the handler.** Marvin answers a matching `If-None-Match` from its validator store before the endpoint runs; CSE runs the handler and then answers 304. On a 304 Marvin should win by the cost of the handler, and the post should say that this is a design difference, not overhead.

## Where CSE may win or tie

To be confirmed by the run:
- The idempotent replay, where CSE writes stored bytes and IdempotentAPI decompresses, deserializes, rebuilds the result by reflection and serializes again.
- The validation filter against SharpGrip, for the same number of rules.
- Problem details against Hellang, which goes through the MVC `ObjectResult` executor.

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
- The CSE column is a working tree, not a NuGet release.

## Possible angles

- "What each add-on costs": a per-feature overhead table against the plain framework endpoint.
- "Who answers the 304": Marvin's validator store against CSE's after-the-handler check.
- "Storing bytes or storing objects": the two idempotency designs and what replay costs in each.
- "The logger that buffers your response": streaming against full buffering, with the 1 MB rows.
- "Reflection in the hot path": the Result filter's per-request reflection (found by reading the code, sized by the ResultFilter vs ManualMatch rows), as a follow-up fix.
