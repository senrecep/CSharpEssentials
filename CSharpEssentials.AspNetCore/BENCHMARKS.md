# CSharpEssentials.AspNetCore: Performance benchmarks (draft)

This page compares CSharpEssentials.AspNetCore (CSE) with plain ASP.NET Core minimal APIs (BCL), Hellang.Middleware.ProblemDetails 6.5.1, Marvin.Cache.Headers 7.2.0 and SharpGrip.FluentValidation.AutoValidation.Endpoints 2.0.0 (with FluentValidation 12.2.0). It covers the `Result<T>` endpoint filter, enum binding, conditional GET (ETag and 304), problem+json for exceptions, empty error statuses and errors returned from code, and the validation endpoint filter. The suite uses BenchmarkDotNet v0.14.0 and targets .NET 8, 9, 10 and 11.

The same project, `benchmarks/CSharpEssentials.AspNetCore.Comparison.Benchmarks/` (suite key `aspnetcore`), also holds the idempotency classes, documented in [CSharpEssentials.Idempotency/BENCHMARKS.md](../CSharpEssentials.Idempotency/BENCHMARKS.md), and the request logging classes, documented in [CSharpEssentials.RequestResponseLogging/BENCHMARKS.md](../CSharpEssentials.RequestResponseLogging/BENCHMARKS.md).

**Status: draft.** The suite builds, and its parity self-check passes on every TFM. No full default-job run has been done, so this page has no numbers yet (see [Results](#results)).

The competitors are pinned exactly in `Directory.Packages.props`. Each pin is the latest stable release on NuGet as of 2026-11-03:

| Package | Version | Published | License (nuspec expression) | Asset the suite loads |
|---|---|---|---|---|
| Hellang.Middleware.ProblemDetails | 6.5.1 | 2022-08-09 | MIT | `lib/net6.0` on every TFM (the latest release; master has unreleased net8.0 work) |
| Marvin.Cache.Headers | 7.2.0 | 2024-09-25 | MIT | `lib/net8.0` on every TFM |
| SharpGrip.FluentValidation.AutoValidation.Endpoints | 2.0.0 | 2026-02-05 | MIT | `lib/net8.0` to `lib/net10.0` (net11 loads `lib/net10.0`) |
| FluentValidation | 12.2.0 | latest stable | Apache-2.0 | `lib/net8.0` |

The repo's own FluentValidation range `[11.11.0,)` resolves to 11.11.0, which the Validation suite runs. For this suite only, `Directory.Packages.props` pins 12.2.0, because competitors run their latest stable release. Transitive pinning is off for this project, as it is for the DI, mediator, endpoints and Http suites: with it on, restore fails with NU1109, because Serilog.AspNetCore 10 (a logging competitor in the same project) needs Microsoft.Extensions.* and System.Text.Json 10.0.0, above the repo's `[9.0.4,)` pins. `Microsoft.AspNetCore.TestHost` is pinned per TFM (8.0.30, 9.0.4, 10.0.11) to match each shared framework major.

What each TFM runs (`--self-check` prints this as its first line):

| TFM | ASP.NET Core | CSE.AspNetCore build | CSE.Results, Errors, Validation build |
|---|---|---|---|
| net8.0 | 8.0.30 | net8.0 | netstandard2.1 |
| net9.0 | 9.0.19 | net9.0 | net9.0 |
| net10.0 | 10.0.11 | net10.0 | net10.0 |
| net11.0 | 11.0.0-rc.1.26425.128 | net11.0 | net11.0 |

The SharpGrip assembly reports `AssemblyInformationalVersion` 1.0.0 although the package is 2.0.0; the restore assets file confirms 2.0.0 on every TFM.

---

## What was measured

The code is in `benchmarks/CSharpEssentials.AspNetCore.Comparison.Benchmarks/`, in five classes for this package.
- Every class has `[MemoryDiagnoser]`.
- These are overhead measurements, so the plain framework row is the baseline in every category, and the ratio reads as "the cost of adding X".
- Each variant runs on its own Production host over `TestServer` (`WebApplication.CreateSlimBuilder` + `UseTestServer`). Production matters: the framework and Hellang add exception details in Development. Logging providers are cleared.
- Every request reads the whole response body. Every handler increments a `HandlerCounter`, so parity can tell an executed request from a short-circuited one.
- Each class checks parity in `[GlobalSetup]`, and `--self-check` runs the same check for every parameter value. A mismatch fails the run.
- Each competitor and BCL row has a comment above it that links to the documentation it follows; each CSE row links to its Readme section.

| # | Class | Measured unit | Rows | Parity |
|---|---|---|---|---|
| 1 | `ResultFilterComparison` | GET /orders/{id}, `Outcome` = Success (200 `OrderDto`) or Failure (404) | BCL `Results<Ok<OrderDto>, ProblemHttpResult>` (baseline), CSE `ResultEndpointFilter` over `Result<OrderDto>`, CSE hand-written match with `ToProblemResult()` (labelled) | Success: status, body bytes, handler calls. Failure: status 404, `application/problem+json`, JSON `status` 404, handler calls |
| 2 | `EnumBindingComparison` | GET /orders?status=…, `Input` = `PendingApproval`, `pending_approval`, `bogus`; PassThrough GET /ping | BCL binding (baseline), CSE `AddEnumConventions` + `UseEnumBinding` | Canonical: both 200 `"PendingApproval"`. Normalized: CSE 200, the framework's outcome recorded. Invalid: both 400 with handler calls 0, CSE problem+json |
| 3 | `ConditionalGetComparison` | GET /articles/7, JSON of exactly `Size` = 1 KB or 64 KB | BodyHash200/304: plain (baseline), CSE `UseBodyHashFallback` + `WithConditionalGet`, Marvin defaults. Versioned200/304: hand-written `ETag: "17"` (baseline), CSE `IVersioned` | 200: body bytes, ETag presence. 304: empty body; handler calls Marvin 0, the others 1 |
| 4 | `ProblemDetailsComparison` | Exception (handler throws), StatusCode (empty 404), ErrorResult (404 from code) | BCL `AddProblemDetails` + `UseExceptionHandler` + `UseStatusCodePages` (baseline), CSE `AddEnhancedProblemDetails` + `GlobalExceptionHandler`, Hellang `UseProblemDetails`; ErrorResult: BCL `TypedResults.Problem` (baseline) and CSE `ToProblemResult()` | Status, `application/problem+json`, JSON `status`, handler calls 1 |
| 5 | `ValidationFilterComparison` | POST /users, `Rules` = 1 or 3, `Outcome` = Valid or Invalid | BCL hand-written checks + `TypedResults.ValidationProblem` (baseline), CSE `WithValidation<CreateUser>`, SharpGrip `AddFluentValidationAutoValidation` | Valid: 200 `"ada"`, handler calls 1. Invalid: 400, N errors in the body, handler calls 0 |

### Inputs

- `OrderDto("order-42", "pending", 3)`, `CreateUser(Name, Email, Age)`, `ArticleDto(Id, Title, Body)` and `VersionedArticleDto` (same JSON, `[JsonIgnore] Version => "17"`). Strings are ASCII `[a-z0-9-]`, with no decimals or dates.
- `ArticleDto.Body` is pseudo-random `[a-z0-9]` text from a fixed `Random` seed, sized so the System.Text.Json output is exactly `Size` bytes. It is built once per parameter value.
- Valid body `{"name":"ada","email":"ada-bench-local","age":36}`, invalid body `{"name":"","email":"","age":0}`. Rules=1 is Name not empty; Rules=3 adds Email not empty and Age greater than 0. There is no email-format rule, because the regexes differ.
- Each library gets one validator type holding N rules (`CreateUserValidator{N}` for CSE, `CreateUserFluentValidator{N}` for FluentValidation).
- Each 304 row sends the ETag its own host returned in setup.

---

## Semantic differences

These were seen in the self-check output on all four TFMs and are not timed.

- **Result filter failure body.** The framework writes `{"type":…,"title":"Order not found","status":404}` (plus a `traceId` on net9 and later). CSE writes title `Not Found`, `detail`, `instance`, `errorCodes: ["order.notFound"]` and a trace id. The ManualMatch row uses the same CSE types without the filter, so ManualMatch minus ResultFilter is the filter's own cost.
- **Enum spelling.** On every TFM the framework rejects `pending_approval` with an empty 400 and does not run the handler; CSE binds it to `PendingApproval`. On `bogus` both answer 400, but only CSE writes a problem+json body listing the allowed values. The Normalized and Invalid rows are not like-for-like.
- **ETag from the body.** CSE writes a weak `W/"<base64url SHA-256 of the JSON>"` from a second serialization and no other cache header. Marvin writes a strong MD5 ETag over its store key and the body, buffers the whole response in a `MemoryStream`, adds `Cache-Control`, `Expires`, `Last-Modified` and `Vary`, and stores the validator in its in-memory store.
- **Who decides the 304.** Marvin answers a matching `If-None-Match` from its validator store before the endpoint runs (handler calls 0). CSE runs the handler, hashes the value and then answers 304 (handler calls 1). The BodyHash304 rows therefore measure different work; the Versioned304 rows (hand-written against CSE `IVersioned`) both run the handler. Marvin has no `IVersioned` equivalent by default (only a custom `IETagInjector`), so it has no Versioned row.
- **Problem bodies.** Titles differ: for an exception the framework says `An error occurred while processing your request.` on net8-net10 and `Internal Server Error` on net11, CSE `An unexpected error occurred.`, Hellang `Internal Server Error`. Type URIs differ (Hellang uses `https://httpstatuses.io/{status}`). CSE adds `instance`, `traceId` and, for errors from code, `errorCodes`; Hellang adds `traceId` on every TFM and the framework on net9 and later. With no `Activity` running, every trace id here is `HttpContext.TraceIdentifier`. Parity compares status, media type and the `status` member only.
- **Hellang needs MVC.** Its middleware writes through the MVC `ObjectResult` executor, so its host registers `AddControllers()`, because 6.5.1's `AddProblemDetails` registers MVC's `ObjectResultExecutor`. The upstream minimal-API sample added that call after the 6.5.1 release (2a04612); the sample at the 6.5.1 commit 2c41d08 omits it. Its `AddProblemDetails` overload is ambiguous with the framework's parameterless one when both namespaces are imported; the suite calls the framework one by its static class.
- **Hellang has no ErrorResult row.** It has no error-value API; its minimal-API sample returns a ProblemDetails subclass through the framework's `Results.BadRequest(problem)`.
- **Validation bodies.** The framework and SharpGrip write `"errors": {"Name": ["'Name' must not be empty."], …}`. CSE writes `"errors": [{"code": "Name.NotEmpty", "description": …}]`, `errorCodes`, a `detail` with the first message, `instance` and a trace id. All three use `application/problem+json`. SharpGrip runs a different validator engine (FluentValidation 12.2.0).
- **No DataAnnotations row.** The framework's net10+ `AddValidation()` has no net8/net9 support and no N-rule validator class to match, so it is not included.

---

## Feature matrix

This table comes from reading CSE source, the competitors' source and docs at their release commits, and the self-check output. None of it was measured.

| Feature | CSE | BCL (minimal API + AddProblemDetails) | Hellang 6.5.1 | Marvin 7.2.0 | SharpGrip 2.0.0 |
|---|---|---|---|---|---|
| Unhandled exception to problem+json | Yes | Yes | Yes | n/a | n/a |
| Empty error status to problem+json | Yes | Yes (`UseStatusCodePages`) | Yes | n/a | n/a |
| Exception type to status mapping | `IExceptionProblemMapper` | Custom `IExceptionHandler`; net9+ `ExceptionHandlerOptions.StatusCodeSelector` | `Map` / `MapToStatusCode` | n/a | n/a |
| Domain error value to status | `Error`/`ErrorType` + `IErrorStatusCodeMapper` | No | No | n/a | n/a |
| Error codes list in body | `errorCodes` / `errors` | No (`ValidationProblem` dictionary only) | No | n/a | n/a |
| `Result<T>` to HTTP result filter | `ResultEndpointFilter` | No | No | n/a | n/a |
| Validation endpoint filter | `WithValidation<T>` (CSE validators) | net10+ `AddValidation` (DataAnnotations) | n/a | n/a | Yes (FluentValidation) |
| Query/route string-enum spelling normalization | Yes | No | n/a | n/a | n/a |
| ETag from a version property | `IVersioned` / `IETagSource` | By hand | n/a | Custom `IETagInjector` | n/a |
| ETag from the body | Weak SHA-256 (opt-in) | No | n/a | Strong MD5 (default) | n/a |
| 304 decided | After the handler | By hand | n/a | From the validator store, before the handler | n/a |
| If-Match 412/428 | Yes | By hand | n/a | 412 only, no 428 (`HttpCacheHeadersMiddleware.cs:565`) | n/a |
| Cache-Control/Expires generation | No | No | No | Yes | n/a |
| Needs MVC services | No (minimal API paths) | No | Yes | No (only its attribute types use MVC namespaces) | No |
| Latest release | Working tree | Framework | 2022-08-09 | 2024-09-25 | 2026-02-05 |
| AOT/trim flagged | No (reflection sites listed under Caveats) | Yes with source-generated JSON | No | No | No (FluentValidation expressions) |
| License | MIT | MIT | MIT | MIT | MIT (FluentValidation Apache-2.0) |

---

## Results

**Pending the full default-job run.** The numbers will come from the default BenchmarkDotNet job on net8.0, net9.0, net10.0 and net11.0, and the reports will be committed under `benchmarks/results/aspnetcore/{tfm}/results/`. Short-job (`--quick`) numbers are not published here.

---

## Caveats

- **TestServer, not Kestrel.** Sockets, TLS and HTTP parsing are not measured. Each number is the middleware and endpoint pipeline plus the in-memory transport, which every row pays the same way.
- **Expected CSE costs, from reading the code** (not yet measured):
  - `ResultEndpointFilter/ResultEndpointFilter.cs:22-31`: every request resolves `IResultErrorMapper` from `RequestServices` (also on success), calls `GetGenericTypeDefinition`, and runs three uncached `GetProperty().GetValue()` calls, one boxing a bool.
  - `EnumBinding/EnumBindingMiddleware.cs:27`: the `GetValue` lambda captures `this`, so a delegate is allocated per request even on a cache hit (PassThrough shows it); `:122,147,150` allocate a `List<string>` pair and a `string[]` per value even when nothing changes (Canonical); `:71` copies the whole query into a dictionary on the first change (Normalized).
  - `ConditionalRequests/DefaultETagGenerator.cs:29`: the whole value is serialized to a `byte[]` just to hash it, so the 64 KB BodyHash200 row allocates about 64 KB more.
  - `Validation/ValidationEndpointFilter.cs:29,33-35,53`: every request runs `OfType().ToArray()`, `OrderBy` over `GetServices`, and `Distinct` on failure.
  - `ProblemDetails/EnhancedProblemDetailsWriter.cs:64-71` uses the reflection-based `WriteAsJsonAsync(object, Type, options)` overload. This matters for trimming and AOT, not for these JIT numbers.
- Hellang 6.5.1 is a 2022 release built for net6.0; every TFM runs that build.
- The CSE column is the working tree on branch `perf/bench-aspnetcore`, which may not match the version published on NuGet.

---

## How to reproduce

```bash
cd benchmarks
./run-benchmarks.sh --project aspnetcore                       # all TFMs (net8-net11), default job
./run-benchmarks.sh --project aspnetcore --tfm net10.0         # one TFM
./run-benchmarks.sh --project aspnetcore --quick --tfm net10.0 # one TFM, Short job, one filter per class
dotnet run -c Release -f net10.0 --project CSharpEssentials.AspNetCore.Comparison.Benchmarks -- --self-check
```

Without `--tfm`, `--quick` runs on net9.0, the script default. Short-job numbers cannot be compared with default-job numbers. Results are stored per framework under `benchmarks/results/aspnetcore/{tfm}/results/`.
