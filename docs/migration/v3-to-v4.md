# Migrating from 3.x to 4.0

4.0 changes default behavior in `CSharpEssentials.AspNetCore` and `CSharpEssentials.EntityFrameworkCore`.
The new defaults expose less request data and name enums the same way everywhere. Each old behavior can be turned back on with a setting.

This guide describes 4.0. Where a later release changed an item, the item says so; going past 4.x, also read [4.x to 5.0](v4-to-v5.md) and [5.x to 6.0](v5-to-v6.md).

## ProblemDetails (`CSharpEssentials.AspNetCore`)

Configure the output with `AddEnhancedProblemDetails(o => ...)`. Minimal API (`ToProblemResult`), MVC (`ToActionResult`), `GlobalExceptionHandler`, status code pages and framework 404/405 responses all go through `IProblemDetailsService` and produce the same JSON.

| Changed behavior | 3.x | 4.0 default | Restore with |
|---|---|---|---|
| `traceId` | `traceparent` value (`00-…-…-01`) | W3C trace id (32 hex); `HttpContext.TraceIdentifier` when there is no W3C activity | `o.TraceId = TraceIdFormat.TraceparentHeader` |
| `requestId` | written | omitted | `o.IncludeRequestId = true` |
| `user` | written | omitted | `o.IncludeUser = true` |
| `spanId` / `parentSpanId` | written | omitted | `o.IncludeSpanIds = true` |
| `instance` | `"GET /path"` | `"/path"` | `o.Instance = ProblemInstanceFormat.MethodAndPath` |
| `type` | RFC 7231 URIs | RFC 9110 URIs (`null` for unlisted status codes) | `o.TypeUriResolver = ProblemTypeUris.Rfc7231` |
| `errorMessages` | written | omitted | `o.ErrorFields \|= ProblemErrorFields.Messages` |
| `errors` | every `Error` (code, description, type, metadata) | `ErrorType.Validation` errors only, as `{ code, description }`; a response with only non-validation errors has no `errors` (use `errorCodes`) | `o.ErrorFields \|= ProblemErrorFields.AllErrors` |
| Exception message in a 500 | written (via `Error.Exception`) | logged only | `o.ExposeExceptionDetails = env.IsDevelopment()` |

`o.UseLegacyDefaults()` restores all of the rows above except the last one.

Other ProblemDetails changes:

- `GlobalExceptionHandler` maps exceptions with `IExceptionProblemMapper`.
  - Defaults:

    | Exception | Response |
    |---|---|
    | `OperationCanceledException` while the client aborted the request | 499 (other cancellations, such as timeouts: 500) |
    | `BadHttpRequestException` | its own status code |
    | `EnhancedValidationException` | 400 |
    | `DomainException` | the status of its `ErrorType` (3.x: always 400) |
    | Anything else | 500 |

  - In 3.x, `InvalidOperationException`, `ApplicationException` and `ValidationException` returned 400. To keep that, register a mapper with `services.AddExceptionProblemMapper<MyMapper>()`.
- The status code for an `ErrorType` comes from `IErrorStatusCodeMapper`. Replace it with `services.AddErrorStatusCodeMapper<MyMapper>()`, for example to return 401 or 403 for authentication errors.
- `ResultEndpointFilter` now returns a ProblemDetails response on failure. In 3.x it returned `400` with the raw `Error[]`. A registered `IResultErrorMapper` still takes precedence. It is now resolved from the request services on each request, so a scoped mapper works; the `ResultEndpointFilter(IResultErrorMapper?)` constructor is gone (use `new ResultEndpointFilter()` and register the mapper in DI).
- The return types changed:
  - `ToProblemResult` returns `EnhancedProblemHttpResult`.
  - `ToActionResult` returns `EnhancedProblemObjectResult`, which derives from `ObjectResult`.
  - Both resolve the registered options when they execute. Code that cast the result to `ProblemHttpResult` or `BadRequestObjectResult` has to change.
- `ToProblemDetails` builds the object with the default options because it has no request context.
- `EnhancedProblemDetails`:
  - `ErrorCodes` and `ErrorMessages` are `null` when they are not emitted (3.x: never `null`). Code such as `pd.ErrorMessages.Contains(...)` needs a null check.
  - `Errors` is no longer serialized; `ErrorFields` decides what is written.
- The static `ProblemDetails` enhancer is gone. `AddEnhancedProblemDetails(Action<ProblemDetails, HttpContext>)` still works and is registered as an `IProblemDetailsEnricher`. You can also add enrichers with `AddProblemDetailsEnricher<T>()`.
- `AddEnhancedProblemDetails(null)` no longer compiles because two overloads now accept a delegate. Call `AddEnhancedProblemDetails()` instead.
- `GlobalExceptionHandler` resolves mappers and options from the request services for each exception, so mappers can be scoped. Its constructor takes only `IProblemDetailsService` and `ILogger`.
- Minimal API writes with `Microsoft.AspNetCore.Http.Json.JsonOptions`, MVC with `Microsoft.AspNetCore.Mvc.JsonOptions`. The output is identical only when both have the same JSON settings; `ConfigureSystemTextJson()` sets both.
- Call `app.UseEnhancedProblemDetails()`. It runs `UseExceptionHandler()` and `UseStatusCodePages()`.
- Without `AddEnhancedProblemDetails`, the library does not use `IProblemDetailsService` and writes the response itself. A plain `AddProblemDetails()` would replace `traceId` with the `traceparent` value, and Minimal API and MVC output would differ.
- The automatic 400 of `[ApiController]` (invalid model state, for example an unknown value of an enum that is not handled by `UseEnumBinding`) is still MVC's `ValidationProblemDetails`. Call `services.ConfigureInvalidModelStateResponse()` to write it as an enhanced problem with `errorCodes`/`errors` (code = model state key) that honors `ErrorFields`. The description is MVC's model error message; for malformed JSON that is the System.Text.Json parser message (as in MVC's default response). Pass an `errorFactory` or set `JsonOptions.AllowInputFormatterExceptionMessages = false` to hide it. It can be called before or after `AddControllers()`. It has no effect together with `ConfigureModelValidatorResponse()`, which turns the automatic 400 off; use one or the other.
- `EnhancedProblemDetails.ErrorCodes` and `ErrorMessages` are now stored in `Extensions["errorCodes"]` and `Extensions["errorMessages"]`. Writers that serialize the declared `ProblemDetails` type, such as MVC's `[ApiController]` writer, now include them.

## JSON (`CSharpEssentials.Json`)

- New: `new ConditionalStringEnumConverter { AllowUndefinedValues = false }` rejects numbers that are not a defined member (for example `999`) when reading. The default still accepts them, like `JsonStringEnumConverter`. 5.0 removes `AllowUndefinedValues` and always rejects undefined numbers ([4.x to 5.0](v4-to-v5.md#json-csharpessentialsjson)).
- `PolymorphicJsonConverterFactory` no longer handles collection or dictionary interfaces (`IEnumerable<T>`, `IDictionary<,>`, `IReadOnlyList<T>`, …). In 3.x it wrapped them in a `$type` envelope, which also broke `ProblemDetails.Extensions`.
- `JsonOptions.ApplyTo` (used by `ConfigureSystemTextJson()`) keeps the target's `TypeInfoResolver` when the source has none. In 3.x the resolver was set to `null`, and Minimal API failed at startup.

## Enum names (`CSharpEssentials.Json`, `.EntityFrameworkCore`, `.AspNetCore`)

`StringEnumNaming` is now the single naming source (5.0 replaces it with generated enum metadata and makes it obsolete: [4.x to 5.0](v4-to-v5.md#naming-and-generated-helpers-csharpessentialsenums)). JSON, EF Core storage, Swagger schemas and query/route binding all use the JSON naming policy (`snake_case_lower`, with `[JsonStringEnumMemberName]` respected).

| Changed behavior | 3.x | 4.0 default | Restore with |
|---|---|---|---|
| EF Core `ConfigureEnumConventions` stored value | `Core.ToSnakeCase()` (`HTTPStatus` → `httpstatus`, `Value1` → `value_1`) | JSON policy (`http_status`, `value1`) | `ConfigureEnumConventions(o => o.UseLegacySnakeCase = true, assemblies)` (obsolete since 5.0; use `existingStorage: EnumStoredAs.LegacySnakeCase`, see [4.x to 5.0](v4-to-v5.md#ef-core-enum-storage-csharpessentialsentityframeworkcore)) |
| Swagger enum schema names | `Core.ToSnakeCase()` | JSON policy names | None (schema follows JSON) |

Names with a single word or plain PascalCase did not change. Enums with acronyms or digits in member names did. When reading, the new EF converter also accepts the old stored values, so existing rows still load.

> **Warning:** loading is the only part that is tolerant. Queries, joins, unique indexes and `GroupBy` compare against the new spelling, so `Where(x => x.Status == Status.HTTPStatus)` does not match a row stored as `httpstatus`. If an enum changed spelling, either migrate the stored values (`UPDATE … SET status = 'http_status' WHERE status = 'httpstatus'`) or keep `UseLegacySnakeCase = true`.

## New in 4.0: enum query/route binding

`services.AddEnumBinding()` (optional; an obsolete forwarder to `AddEnumConventions()` since 5.0) and `app.UseEnumBinding()` make Minimal API (including `[AsParameters]`) and MVC accept the same spellings as JSON for `[StringEnum]` enums in query and route values:

- the snake_case name;
- the C# member name, case-insensitive;
- the number of a defined member.

Invalid values return a 400 ProblemDetails response with the key as the error code; change the code or message with `o.ErrorFactory`. MVC query models are read recursively (`?filter.inner.status=…`, up to 8 levels).

- MVC enum binding now also validates enum properties of nested query DTOs (up to 8 levels). A request with an invalid value in a nested query property that bound silently in 3.x may now get a 400.

Register `UseEnumBinding()` after `UseAuthentication()`/`UseAuthorization()`; otherwise an invalid enum value of an unauthenticated request returns 400 instead of 401.

## Package dependencies

- `Swashbuckle.AspNetCore` is now `[8.1.0, 10)` (3.x: `[8.1.0, )`, unbounded). Apps on Swashbuckle 7.x must upgrade to 8.x. 6.0 moves the Swagger support to `CSharpEssentials.AspNetCore.Swashbuckle` and requires Swashbuckle 10 (`[10.2.3, 11)`); see [5.x to 6.0](v5-to-v6.md).
- `CSharpEssentials.EntityFrameworkCore` now pins each target framework to its own EF Core major: `net8.0` → EF `[8, 9)`, `net9.0` → EF `[9.0.4, 10)`, `net10.0` → EF `[10.0.1, 11)` (3.x: open-ended). EF APIs such as `ExecuteUpdate` are not binary compatible across majors, so an app must use the EF major that matches its target framework (e.g. a `net8.0` app on EF 9 should target `net9.0`).
- `CSharpEssentials.Resilience` now depends on `Polly.Core` `[8.0.0, 9.0.0)` instead of the full `Polly` package (3.x: `Polly` `[8.0.0, )`). `CSharpEssentials.Resilience` and `CSharpEssentials.Http` no longer bring `Polly` transitively, so code that uses the Polly v7 API (`Policy`, `AsyncRetryPolicy`, …) must reference `Polly` itself. The v8 `ResiliencePipeline` API is in `Polly.Core` and still works. `CSharpEssentials.GcpSecretManager` no longer depends on `Polly`; it depends on `CSharpEssentials.Resilience`, which brings `CSharpEssentials.Results`, `CSharpEssentials.Errors`, `CSharpEssentials.Json`, `CSharpEssentials.Enums` and `Polly.Core` transitively.
- Building the library from source (or referencing it with `ProjectReference`) needs the .NET 11 SDK (`global.json` pins 11.0 RC1). The NuGet packages work with the SDKs of their target frameworks.
- The `net11.0` assets of 4.0.0 and 4.1.0 are built with the .NET 11 RC1 SDK. A patch release rebuilt with the .NET 11 GA SDK follows the GA release. The `net8.0`, `net9.0`, `net10.0` and `netstandard` assets are not affected.

## Resilience (`CSharpEssentials.Resilience`)

- The non-generic `ResiliencePolicy` now retries failed `Result` / `Result<T>` values, as `ResiliencePolicy<T>` already did. In 3.x its `WithRetry` and `WithCircuitBreaker` reacted only to exceptions, so a delegate that returned a failed `Result` ran once. Now a transient failure is retried and counts toward the circuit breaker; `Unauthorized`, `Forbidden`, `NotFound` and `Validation` errors are not retried. Delegates that return a failed `Result` on purpose can run more than once.
- The non-generic `CreateRetryPipeline`, `CreateCircuitBreakerPipeline` and `CreateResiliencePipeline` in `CSharpEssentials.Http` (`HttpClientResilienceExtensions`) use the same rule, so they now retry failed `Result` values and count them toward the circuit breaker.
- Cancellation of the caller's token is no longer turned into an error. When the token passed to `ExecuteAsync` or `RetryIfFailed` is cancelled, `ExecuteAsync`, `RetryIfFailed` and `WithFallback` throw `OperationCanceledException`, including when the caller cancels after an attempt returned a retryable failed `Result` (during the attempt, during the backoff delay, or right after the final attempt). This also applies to policies without a retry strategy (circuit-breaker-only or timeout-only): a cancelled caller and a retryable failed result give `OperationCanceledException`. An exception other than `OperationCanceledException` thrown while the caller is cancelled is still returned as an `Unexpected` error. In 3.x `RetryIfFailed` returned an `Unexpected` error, and `WithFallback` ran the fallback. An `OperationCanceledException` from any other token (for example an `HttpClient` timeout) is still treated as a failure: it is retried, and it triggers the fallback.
- New `RetryIfFailed` overloads take a `Func<Error, bool> shouldRetry` predicate that replaces the default retry classification. Exceptions thrown by the operation are passed to the predicate as `Unexpected` errors.
- `default(ResiliencePolicy)` and `default(ResiliencePolicy<T>)` behave like `Create()`. In 3.x `WithRetry`/`WithFallback` on a default policy threw `NullReferenceException` and `ToPipeline()` returned `null`.

## GCP Secret Manager (`CSharpEssentials.GcpSecretManager`)

- `SecretManagerConfigurationOptions` is now a `sealed record`. `CredentialsPath` and `Loader` are settable inside the `AddGcpSecretManager(options => ...)` lambda and `AddProject` returns the options for chaining. The `AddProject` return-type change is source-compatible but binary-breaking: existing calls keep compiling, but assemblies compiled against 3.x must be recompiled. Code that relied on reference-type class semantics (for example `ReferenceEquals`) is also affected. `Projects` rejects `null` (`ArgumentNullException`).
- The provider no longer writes to `Console`. Set `options.LoggerFactory` to receive logs (default: no logging).
- New dependencies: `Microsoft.Extensions.Logging.Abstractions` and `CSharpEssentials.Resilience` (replaces `Polly`).
- Listing secrets is now actually retried on `ResourceExhausted`/`Unavailable`. In 3.x the listing swallowed the exception, so the retry never fired and a transient failure silently loaded no secrets for that project. Accessing each secret version is now retried the same way (3 retries, 2s/4s/8s backoff), and the failure is logged once after retries are exhausted.
- Creating the client no longer wraps `Build` in a retry. Client creation makes no RPC, so that retry could never trigger.
- Concurrent secret loading within a batch no longer writes to a shared `Dictionary`. Results are merged in secret order, so the first value wins deterministically on key collisions.

## Validation (`CSharpEssentials.Validation`)

- `AddValidator` and `AddValidatorsFromAssembly` use `TryAddEnumerable`, so calling them twice for the same validator no longer registers it twice. Code that resolved `IEnumerable<IValidator<T>>` and relied on the duplicates gets one instance.

## Enums (`CSharpEssentials.Enums`)

- New analyzer CSE0001 (Info): a `[StringEnum]` enum nested in a class or struct gets no generated extension methods, and the analyzer now reports it instead of skipping it silently. 5.0 retires CSE0001 because nested enums are supported (the ID stays reserved).

See [CHANGELOG.md](../../CHANGELOG.md) for the full list of changes in each release.
