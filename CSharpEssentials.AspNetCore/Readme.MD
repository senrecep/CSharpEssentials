# CSharpEssentials.AspNetCore

ASP.NET Core integration for CSharpEssentials. It turns `Error` and `Result` values and unhandled exceptions into RFC 9457 problem responses, sends typed results as HTTP results, and adds request validation, `Idempotency-Key` handling, conditional requests (`ETag`, `If-Match`), `[StringEnum]` binding and API versioning. Use it in any Minimal API or MVC app that returns CSharpEssentials results.

Benchmarks against plain ASP.NET Core, Hellang.Middleware.ProblemDetails, Marvin.Cache.Headers and SharpGrip.FluentValidation.AutoValidation are in [BENCHMARKS.md](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore/BENCHMARKS.md).

## Installation

```bash
dotnet add package CSharpEssentials.AspNetCore
```

Targets net8.0 to net11.0. It brings in CSharpEssentials.Errors, Results, Validation, Idempotency and Json, and `Asp.Versioning` 8.1 or later. Everything is in the `CSharpEssentials.AspNetCore` namespace.

OpenAPI support is in separate packages. Use one of them, never both:

- [CSharpEssentials.AspNetCore.OpenApi](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore.OpenApi/Readme.MD) for `Microsoft.AspNetCore.OpenApi` (`AddOpenApi`).
- [CSharpEssentials.AspNetCore.Swashbuckle](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore.Swashbuckle/Readme.MD) for Swashbuckle (`AddSwagger`, `UseVersionableSwagger`).

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials.AspNetCore
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEnhancedProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

WebApplication app = builder.Build();
app.UseEnhancedProblemDetails();

// Typed result: 200 with the user, or a 404 problem. OpenAPI lists both.
app.MapGet("/users/{id:int}", (int id) => FindUser(id).ToHttpResult());

// Result<T>: the filter writes 200 or a problem response.
app.MapGet("/orders/{id:int}", Result<string> (int id) =>
        id > 0 ? $"order {id}" : Error.Validation("order.id", "The id must be positive."))
    .AddEndpointFilter<ResultEndpointFilter>();

// Unhandled exception: 500 problem, the message only goes to the log.
app.MapGet("/boom", string () => throw new InvalidOperationException("connection string"));

app.Run();

static Result<User, NotFoundError> FindUser(int id) =>
    id == 1 ? new User(1, "Ada") : new NotFoundError("user.not_found", $"User {id} was not found.");

public sealed record User(int Id, string Name);
```

`GET /users/2` returns:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "User 2 was not found.",
  "instance": "/users/2",
  "errorCodes": ["user.not_found"],
  "traceId": "2c4a278d5994259e1c89e7bed03a104a"
}
```

## Problem details

### Registration

```csharp
builder.Services.AddEnhancedProblemDetails(o => o.ExposeExceptionDetails = builder.Environment.IsDevelopment());
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

app.UseEnhancedProblemDetails();   // UseExceptionHandler() + UseStatusCodePages()
```

`AddEnhancedProblemDetails` calls `AddProblemDetails()` and routes every problem response through one pipeline: the CSharpEssentials result helpers, `GlobalExceptionHandler`, status code pages and the framework's own 404 and 405 responses. It has three overloads:

| Overload | Use |
|---|---|
| `AddEnhancedProblemDetails()` | Default options |
| `AddEnhancedProblemDetails(Action<EnhancedProblemDetailsOptions>)` | Change the options below |
| `AddEnhancedProblemDetails(Action<ProblemDetails, HttpContext>)` | Default options plus a delegate that runs on every response, as an enricher |

### Options

| `EnhancedProblemDetailsOptions` | Default | Other values |
|---|---|---|
| `TraceId` | `TraceIdFormat.W3CTraceId` (32 hex chars, falls back to `HttpContext.TraceIdentifier` without an `Activity`) | `TraceparentHeader`, `None` |
| `IncludeRequestId`, `IncludeUser`, `IncludeSpanIds` | `false` | `true` |
| `Instance` | `ProblemInstanceFormat.Path` (`"/users/2"`) | `MethodAndPath`, `None` |
| `ErrorFields` | `ProblemErrorFields.Codes \| ValidationErrors` | `Messages`, `AllErrors`, `All`, `None` (flags) |
| `ValidationErrorsFormat` | `List` | `Dictionary`, `ByField` |
| `TypeUriResolver` | `ProblemTypeUris.Rfc9110` | `ProblemTypeUris.Rfc7231` or any `Func<int, string?>` |
| `ExposeExceptionDetails` | `false` | `true` in development only |

`o.UseLegacyDefaults()` turns on the older verbose output (`TraceparentHeader`, all `Include*` on, `MethodAndPath`, `All`, `Rfc7231`).

`ErrorFields` decides the extension members: `Codes` writes `errorCodes` (every error code), `ValidationErrors` writes `errors` (only `ErrorType.Validation` errors), `Messages` writes `errorMessages`, and `AllErrors` writes each `Error` with its type and metadata.

`ValidationErrorsFormat` shapes `errors`:

- `List`: `[{ "code": "name", "description": "..." }]`, a `List<ProblemValidationError>` in `Extensions["errors"]`.
- `Dictionary`: descriptions grouped by code.
- `ByField`: descriptions grouped by the `field` metadata entry (`ErrorMetadata.FieldKey`), the shape of ASP.NET Core's `ValidationProblemDetails`. CSharpEssentials.Validation, the JSON body mapper and the model state helpers set `field`. Each path segment goes through the `PropertyNamingPolicy` of the JSON options that write the response, so `Lines[1].Sku` becomes `lines[1].sku`. An error without `field` is grouped by its code.

```json
"errors": {
  "name": ["'Name' must not be empty."],
  "email": ["'Email' must not be empty."]
}
```

### Status codes and titles

The default `IErrorStatusCodeMapper` (`DefaultErrorStatusCodeMapper`) maps by `ErrorType`:

| `ErrorType` | Status | Title |
|---|---|---|
| `Validation` | 400 | Validation Error |
| `Unauthorized` | 401 | Unauthorized |
| `Forbidden` | 403 | Forbidden |
| `NotFound` | 404 | Not Found |
| `Conflict` | 409 | Conflict |
| `Domain` | 422 | Domain Error |
| `Failure`, `Unexpected`, `Unknown` | 500 | Server Failure, Unexpected Error, Unknown Error |

Some codes map regardless of type: `ConditionalRequestErrors.PreconditionFailedCode` (`Http.PreconditionFailed`) to 412, `Idempotency.KeyReused` to 422, `Idempotency.InFlight` to 409 (with `Retry-After`), and `Idempotency.KeyMissing`, `KeyInvalid` and `ScopeRequired` to 400. Idempotency errors also get a top-level `code` member.

With several errors, `SelectPrimaryError` picks the one with the highest status. That error gives the status, the title and the `detail`. A status you pass yourself that differs from the type's status gets the HTTP reason phrase as title.

### Returning problems from your code

| Method | Returns | Where |
|---|---|---|
| `error.ToProblemResult()` | `IResult` (`EnhancedProblemHttpResult`) | Minimal API |
| `error.ToActionResult(HttpContext)` | `IActionResult` (`EnhancedProblemObjectResult`) | MVC |
| `this.Problem(error)` | `IActionResult` | Inside a `ControllerBase` |
| `error.ToProblemDetails()` | `EnhancedProblemDetails` | Anywhere, no HTTP context |

Each one exists for `Error`, `Error[]` and a failed `Result` or `Result<T>` (a successful result throws `InvalidOperationException`), and takes optional `ErrorMetadata? extensions` (extra members on the response) and `int? statusCode`. `ToProblemResult` and `ToActionResult` read the registered options and mapper when they run. `ToProblemDetails` always uses the default options and mapper, without request details:

```csharp Program.cs
// dotnet add package CSharpEssentials.AspNetCore
using System.Text.Json;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

EnhancedProblemDetails notFound = Error.NotFound("user.not_found", "User 42 was not found.").ToProblemDetails();
Console.WriteLine(JsonSerializer.Serialize(notFound));

Error[] errors =
[
    Error.Validation("name", "Name is required."),
    Error.Conflict("user.email_taken", "The email is taken."),
];
EnhancedProblemDetails combined = errors.ToProblemDetails();
Console.WriteLine($"{combined.Status} {combined.Title}: {string.Join(", ", combined.ErrorCodes ?? [])}");

Result<int> closed = Error.Domain("order.closed", "The order is closed.");
Console.WriteLine(closed.ToProblemDetails(statusCode: 409).Title);

Error crash = Error.Exception(new InvalidOperationException("password=hunter2"));
Console.WriteLine(JsonSerializer.Serialize(crash.ToProblemDetails()));
```

Output:

```text
{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.5","title":"Not Found","status":404,"detail":"User 42 was not found.","errorCodes":["user.not_found"]}
409 Conflict: name, user.email_taken
Conflict
{"type":"https://tools.ietf.org/html/rfc9110#section-15.6.1","title":"Unexpected Error","status":500,"detail":"An unexpected error occurred.","errorCodes":["InvalidOperationException"]}
```

`EnhancedProblemDetails` extends `ProblemDetails` with `Errors` (`Error[]`), `ErrorCodes` and `ErrorMessages` (both `HashSet<string>?`, stored in `Extensions`).

For OpenAPI, `ProducesProblem(status)` declares an `application/problem+json` response of `EnhancedProblemDetails`; `ProducesProblem<TProblemDetails>(status)` uses your own type.

```csharp
app.MapGet("/legacy/{id:int}", (int id) => Error.NotFound("item.not_found", "Not found.").ToProblemResult())
    .ProducesProblem(StatusCodes.Status404NotFound);
```

### Server errors are redacted

While `ExposeExceptionDetails` is `false`, every problem with status 500 or higher hides server details, whichever helper built it:

- `detail` is `An unexpected error occurred.`;
- each 5xx error gets that description, so `errors` and `errorMessages` carry no exception text;
- the metadata keys `exceptionType`, `exceptionMessage`, `exceptionStackTrace`, `innerException`, `exception` and `stackTrace` are removed from errors and from your `extensions`.

Codes, titles and 4xx responses are unchanged. A `Detail` you set in an `ExceptionProblem` is written as given.

### Exceptions

`GlobalExceptionHandler` is an `IExceptionHandler`. It logs the exception and asks each `IExceptionProblemMapper` in turn: yours (in registration order), then the JSON body mapper, then `DefaultExceptionProblemMapper`, which always answers:

| Exception | Response |
|---|---|
| `OperationCanceledException` after the client aborted | 499 (`DefaultExceptionProblemMapper.ClientClosedRequest`) |
| `BadHttpRequestException` | Its status, with a fixed `detail` (`The request is invalid.`, `The request timed out.`, `The request body is too large.`, ...). The exception message is never written |
| `EnhancedValidationException` (namespace `CSharpEssentials.Exceptions`) | 400 with its errors |
| `DomainException` (namespace `CSharpEssentials.Exceptions`) | The status of its error |
| Anything else | 500, title `An unexpected error occurred.` |

A JSON request body that fails to bind (Minimal APIs throw it only with `RouteHandlerOptions.ThrowOnBadRequest = true`) becomes a 400 validation problem with one error per field. The code is the JSON path without `$.` as the client sent it (`items[1].quantity`). The description is fixed: `The value is invalid.`, or `The field is required.` for a missing required member. Any other `JsonException` stays a 500.

```json
{ "status": 400, "title": "Validation Error", "detail": "The value is invalid.", "errorCodes": ["items[1].quantity"],
  "errors": [{ "code": "items[1].quantity", "description": "The value is invalid." }] }
```

### Extension points

```csharp
using System.Diagnostics.CodeAnalysis;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Errors;

public sealed class TenantEnricher : IProblemDetailsEnricher
{
    public void Enrich(ProblemDetailsContext context) =>
        context.ProblemDetails.Extensions["tenant"] = context.HttpContext.Request.Headers["X-Tenant"].ToString();
}

public sealed class AuthAwareStatusMapper : DefaultErrorStatusCodeMapper
{
    public override int GetStatusCode(Error error) =>
        error.Code == "auth.expired" ? StatusCodes.Status401Unauthorized : base.GetStatusCode(error);
}

public sealed class TimeoutExceptionMapper : IExceptionProblemMapper
{
    public bool TryMap(HttpContext httpContext, Exception exception, [NotNullWhen(true)] out ExceptionProblem? problem)
    {
        problem = exception is TimeoutException
            ? new ExceptionProblem(StatusCodes.Status504GatewayTimeout, Detail: "The upstream service timed out.")
            : null;
        return problem is not null;
    }
}
```

```csharp
builder.Services.AddProblemDetailsEnricher<TenantEnricher>();          // runs after the built-in enrichment, in registration order
builder.Services.AddErrorStatusCodeMapper<AuthAwareStatusMapper>();    // replaces the default mapper
builder.Services.AddExceptionProblemMapper<TimeoutExceptionMapper>();  // tried before the built-in mappers
```

All three take an optional `ServiceLifetime` (default `Singleton`). `ExceptionProblem` has `StatusCode`, `Title`, `Detail` and `Errors`, all optional. `IErrorStatusCodeMapper` has `GetStatusCode`, `SelectPrimaryError`, `GetTitle` and `GetDeclaredStatusCodes`; all are virtual on `DefaultErrorStatusCodeMapper`. Override `GetDeclaredStatusCodes` when you map a typed error to another status by code, so the OpenAPI document lists it.

### MVC model validation

```csharp
builder.Services.AddControllers();
builder.Services.ConfigureInvalidModelStateResponse();
```

`ConfigureInvalidModelStateResponse()` writes the automatic 400 of `[ApiController]` through the same pipeline. Each model error becomes an `ErrorType.Validation` error whose code and `field` are the model state key; a JSON body error gets the same codes and fixed descriptions as above. Pass `(key, modelError) => Error.Validation(...)` to build the errors yourself. It can be called before or after `AddControllers()`.

The alternative is `ConfigureModelValidatorResponse()`, which turns the automatic 400 off and adds `[ValidateModel]` as a global filter (the attribute can also go on single actions). It uses the code `validation.{key}`. Use one of the two, not both.

### Native AOT

Problem responses serialize when reflection-based JSON is off. The package adds source-generated metadata for its own types after your resolvers. Values of your own types in `ErrorMetadata` or enricher output must be in your `JsonSerializerContext`.

## Typed results

`ToHttpResult()` sends a `Result<T, E1>`, `Result<T, E1, E2>` or `Result<T, E1, E2, E3>` from CSharpEssentials.Results:

```csharp
app.MapGet("/orders/{id:int}", async (int id, IOrderService orders, CancellationToken ct) =>
    await orders.GetAsync(id, ct).ToHttpResult());              // HttpResult<Ok<OrderDto>, NotFoundError, ConflictError>

app.MapPost("/orders", (CreateOrder command, IOrderService orders) =>
    orders.Create(command).ToCreated(o => $"/orders/{o.Id}"));  // HttpResult<Created<OrderDto>, ValidationError, ConflictError>
```

| Method | Success response |
|---|---|
| `ToHttpResult()`, `ToOk()` | 200 with the value |
| `ToHttpResult(v => ...)` | Any `IResult` that is also an `IEndpointMetadataProvider`, such as `TypedResults.Accepted(uri, v)` |
| `ToCreated(v => location)` | 201 with the value and `Location` |
| `ToNoContent()` | 204 |

Each works on the result, on `Task<Result<...>>` and on `ValueTask<Result<...>>`, and throws `InvalidOperationException` for a `default` result. The error slots must implement `IError`; convert an exception slot with `MapError` first.

The error goes out like `ToProblemResult()`: the same code, description, type and metadata, through the registered mapper, options and enrichers.

`HttpResult<TSuccess, E1..E3>` is an `IEndpointMetadataProvider`. The OpenAPI document lists the success response plus one problem response per status that `GetDeclaredStatusCodes` returns for each slot. The default reads `[ErrorKind]` from the slot type, which `NotFoundError`, `ConflictError`, `ValidationError` and the other typed errors (and their subclasses) carry. A slot of type `Error`, or of a type without `[ErrorKind]`, declares nothing; add `ProducesProblem(status)` for it.

### Forgetting `ToHttpResult()`

A typed result returned as is would be written as a 200 with the error inside. `AddEnhancedProblemDetails` adds a converter to the Minimal API and MVC JSON options that throws `InvalidOperationException` when a typed result is the root of a response body, before anything is written, so the request ends as a 500. Nested typed results (an item in a list, a property) are written as `{"case":...,"value":...}`. Under Native AOT those options cannot write or read a typed result at all and throw `NotSupportedException`. The CSharpEssentials.Endpoints analyzer reports the mistake at build time as `CSE1013`.

## Result endpoint filter

`ResultEndpointFilter` converts a `Result` or `Result<T>` returned by a Minimal API handler: success becomes 200 with the value (an empty 200 for a plain `Result`), failure a problem response. It also converts `Result<T, E1..E3>`, and a `default` result becomes a 500 problem. Add it per endpoint or on a group:

```csharp
RouteGroupBuilder api = app.MapGroup("/api").AddEndpointFilter<ResultEndpointFilter>();

api.MapGet("/users/{id:guid}", Result<User> (Guid id) =>
    id == Guid.Empty ? Error.NotFound("user.not_found", "User not found.") : new User(id));
```

A registered `IResultErrorMapper` replaces the problem response. It is resolved per request, so any lifetime works:

```csharp
public sealed class LegacyErrorMapper : IResultErrorMapper
{
    public IResult Map(Error[] errors) =>
        errors[0].Type == ErrorType.NotFound ? Results.NotFound() : errors.ToProblemResult();
}

builder.Services.AddScoped<IResultErrorMapper, LegacyErrorMapper>();
```

Prefer `ToHttpResult()` for typed results: with the filter, the OpenAPI document only sees the handler's return type.

## Validation endpoint filter

`WithValidation<T>()` runs the `IValidator<T>` validators of CSharpEssentials.Validation on the handler's `T` argument and returns a 400 problem before the handler runs:

```csharp Program.cs
// dotnet add package CSharpEssentials.AspNetCore
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Extensions;
using CSharpEssentials.Validation.Validators;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddEnhancedProblemDetails();
builder.Services.AddValidator<CreateUser, CreateUserValidator>();

WebApplication app = builder.Build();

app.MapPost("/users", (CreateUser request) => Results.Created($"/users/{request.Name}", request))
    .WithValidation<CreateUser>();

app.Run();

public sealed record CreateUser(string? Name, int Age);

public sealed class CreateUserValidator : Validator<CreateUser>
{
    protected override ValueTask Configure(CreateUser model, RuleContext<CreateUser> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Name).NotEmpty().MaxLength(50);
        rules.For(() => model.Age).GreaterThan(0);
        return ValueTask.CompletedTask;
    }
}
```

```json
{ "title": "Validation Error", "status": 400, "detail": "'Name' must not be empty.",
  "errorCodes": ["Name.NotEmpty", "Age.GreaterThan"],
  "errors": [{ "code": "Name.NotEmpty", "description": "'Name' must not be empty." },
             { "code": "Age.GreaterThan", "description": "'Age' must be greater than 0." }] }
```

- Every registered `IValidator<T>` runs, in ascending `Order`. A `null` argument (an optional body) passes through.
- If no `IValidator<T>` is registered, the request throws `InvalidOperationException`. A handler without a `T` parameter throws when the endpoint is built.
- It declares a 400 problem response for OpenAPI.
- `WithValidation<T>()` is for a single endpoint. On a group use `AddEndpointFilter<ValidationEndpointFilter<T>>()`.

## Idempotency

The middleware handles the `Idempotency-Key` header for endpoints you opt in. A retry with the same key and the same request gets the stored response instead of running the handler again. Records go to an `IIdempotencyRecordStore` from [CSharpEssentials.Idempotency](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Idempotency/Readme.MD), which explains the store contract and guarantees in depth.

```csharp
builder.Services.AddIdempotency(o =>
{
    o.RetentionPeriod = TimeSpan.FromHours(24);   // default
    o.RequireKey = true;                           // default false: requests without a key pass through
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseIdempotency();                              // after auth, so keys are scoped to the caller

app.MapPost("/orders", CreateOrder).WithIdempotency();
```

```csharp
[HttpPost, Idempotent]                             // MVC action or controller
public IActionResult Create(CreateOrderRequest request) => Ok();
```

Only endpoints with `.WithIdempotency()` (endpoint or group) or `[Idempotent]` take part, and only for methods in `Methods` (POST and PATCH by default; add PUT or DELETE yourself).

| Request with an `Idempotency-Key` | Response |
|---|---|
| First request | The handler runs; the response is stored |
| Same key and request, completed | The stored status, body and allowlisted headers, plus `Idempotency-Replayed: true` |
| Same key and request, still running | 409 `Idempotency.InFlight` with `Retry-After` |
| Same key, other method, path, query or body | 422 `Idempotency.KeyReused` |
| No key | Passes through, or 400 `Idempotency.KeyMissing` with `RequireKey` |
| Empty key, longer than `MaxKeyLength` (255), several headers | 400 `Idempotency.KeyInvalid` |
| Key but no scope resolved | `UnscopedKeyBehavior`: `Skip` (default) runs without idempotency, `Reject` returns 400 `Idempotency.ScopeRequired`, `Global` uses a global key |

### Options

| `IdempotencyOptions` | Default |
|---|---|
| `Mode` | `IdempotencyMode.Enforce` (also `Observe`, `Off`) |
| `HeaderName`, `ReplayedHeaderName` | `Idempotency-Key`, `Idempotency-Replayed` |
| `Methods` | POST, PATCH |
| `RequireKey` | `false` |
| `RetentionPeriod` | 24 hours |
| `InFlightTimeout` (lease) | 1 minute, capped by `MaxInFlightTimeout` (15 minutes) |
| `ServerErrorRetention` | 10 minutes, how long a stored 5xx is replayed |
| `RetryAfter` | 1 second |
| `ShouldStore` | `IdempotencyStorePolicies.Default`: 2xx and 3xx stored, 5xx stored for `ServerErrorRetention`, 4xx released. `SuccessOnly` stores 2xx only |
| `NeverStoreStatusCodes` | 503, 504: the key stays in flight until the lease ends |
| `MaxKeyLength`, `MaxResponseBodySize` | 255, 1 MiB |
| `ScopeResolver` | `IdempotencyScopes.User` |
| `UnscopedKeyBehavior` | `Skip` |
| `Namespace` | `http` |
| `ReplayedHeaders` | `Content-Type`, `Content-Language`, `Location`, `ETag`, `Cache-Control`, `Retry-After` |
| `RequireExecutionFilter` | `false` |

A stored response is an opaque, versioned payload (`IdempotentResponse`: status, allowlisted headers, body). A payload that cannot be read is answered with 500 and the handler does not run again.

Stores: in-memory by default (single instance, unbounded), `o.UseInMemoryStore()`, `o.UseDistributedCacheStore()` (needs an `IDistributedCache`, best effort), or `o.UseStore<TStore>(ServiceLifetime lifetime = Scoped)` for your own store. With several instances use an atomic store such as the EF Core or Redis store.

`AddIdempotency` throws when `InFlightTimeout` exceeds `RetentionPeriod` or `MaxInFlightTimeout`, when `ServerErrorRetention` is not in (0, `RetentionPeriod`], or when `NeverStoreStatusCodes` is empty or holds a code outside 100 to 599. Options are read through `IOptionsMonitor<IdempotencyOptions>` on each request, so a reloaded configuration section (for example `Mode`) applies without a restart; the store is fixed at startup.

### Per-endpoint settings and profiles

```csharp
builder.Services.AddIdempotency(o => o.AddProfile("payments", p =>
{
    p.ServerErrorRetention = TimeSpan.FromHours(24);
    p.StoreKey = "payments";   // a keyed IIdempotencyRecordStore
}));

app.MapPost("/payments", Pay).WithIdempotency("payments");
app.MapPost("/imports", Import).WithIdempotency(o => { o.InFlightTimeout = TimeSpan.FromMinutes(10); o.RequireKey = true; });
```

```csharp
[HttpPost, Idempotent(Profile = "payments", LeaseSeconds = 300)]
public IActionResult Charge(ChargeRequest request) => Ok();
```

`IdempotencyEndpointOptions` overrides `Mode`, `RetentionPeriod`, `InFlightTimeout`, `ServerErrorRetention`, `RequireKey`, `ShouldStore`, `UnscopedKeyBehavior`, `StoreKey` and `Profile`. `[Idempotent]` has `Profile`, `Mode`, `RetentionSeconds`, `LeaseSeconds`, `ServerErrorRetentionSeconds`, `RequireKey`, `UnscopedKeyBehavior` and `StoreKey`. Each setting comes from the endpoint, else its profile, else the global options; an endpoint overrides its group or controller. An unknown profile or invalid merged settings throw `InvalidOperationException` on that endpoint's requests.

### What happens to the key

`.WithIdempotency()` and `[Idempotent]` install a filter that marks when the handler starts (`IIdempotencyExecutionFeature.HandlerStarted`, endpoint metadata `IdempotencyFilterInstalledMetadata`). The middleware uses the mark:

- A response produced before the handler started (rate limiter, binding failure, model validation) releases the key, so the client can retry.
- A 503 or 504, an exception or a client abort after the handler started keeps the key in flight until `InFlightTimeout`, because the work may be done.
- A response that cannot be stored (`Content-Encoding` set by the handler, `text/event-stream`, larger than `MaxResponseBodySize`) is released for 4xx and kept in flight otherwise.
- Everything else goes to `ShouldStore(IdempotencyOutcome)`, which receives `StatusCode` and `HandlerStarted`.

An endpoint with idempotency metadata but no filter (for example `MapControllers().WithIdempotency()` without `[Idempotent]`) stores only 2xx and releases everything else. It logs `Idempotency.ExecutionFilterMissing` once, or throws with `RequireExecutionFilter = true`.

Keep `InFlightTimeout` at least as long as the server's request timeout and the client's total retry budget. Put `UseResponseCompression()` before `UseIdempotency()`, since the stored body is uncompressed. See [Lease and retry budget](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Idempotency/Readme.MD#lease-and-retry-budget).

### Scopes and machine callers

Keys are scoped by `ScopeResolver`, which reads `HttpContext.User`, so one caller never gets another caller's stored response. `IdempotencyScopes` (namespace `CSharpEssentials.Idempotency`):

| Resolver | Scope |
|---|---|
| `User` (default) | `NameIdentifier`, then `sub` |
| `Tenant(claimType)` | The tenant claim |
| `UserAndTenant(tenantClaimType)` | Both |
| `Client` | `client_id`, then `azp`, for client-credential callers |
| `Custom(kind, principal => value)` | Your own claim; `user`, `tenant`, `client` and `global` are reserved kinds |
| `Global` | One shared scope |

Anonymous requests and machine callers have no user, so with the default `Skip` they run without idempotency. Set `ScopeResolver = IdempotencyScopes.Client` for client-credential callers, or `UnscopedKeyBehavior = Reject` to make the gap visible. The Mediator idempotency behavior rejects unscoped keys by default; give both layers the same resolver. `AddIdempotency` also registers an `IIdempotencyScopeAccessor` over `HttpContext.User` for the Mediator behavior.

### Rollout with `Observe`

`Observe` answers exactly like `Off` (no replay, no 400, 409 or 422) but stores new keys as `Enforce` would and logs and counts what `Enforce` would have done. Switch to `Enforce` per endpoint or through configuration. `Observe` does not prevent duplicates. Decisions are counted on `idempotency.decisions` (meter `CSharpEssentials.Idempotency`) with `source` = `http`. See [Rollout: Observe → Enforce](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Idempotency/Readme.MD#rollout-observe--enforce).

```csharp
builder.Services.AddIdempotency(o => o.Mode = IdempotencyMode.Observe);
app.MapPost("/payments", Pay).WithIdempotency(o => o.Mode = IdempotencyMode.Enforce);
```

### Money movement

The middleware reserves the key before the handler and settles it after, outside your transaction. For a guarantee tied to the database write, pass the key to a Mediator command backed by the EF Core store and parse it with `IdempotencyKeyHeader.TryParse(header, out string? key)` (or `TryParse(header, maxKeyLength, out key)`), which unquotes a structured-field `"abc"` the same way the middleware does. See [Transactional store for money movement](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Idempotency/Readme.MD#transactional-store-for-money-movement).

```csharp
app.MapPost("/payments/{orderId:guid}/capture", async (
    Guid orderId, CaptureRequest body, [FromHeader(Name = "Idempotency-Key")] string? header,
    ISender sender, CancellationToken ct) =>
{
    if (!IdempotencyKeyHeader.TryParse(header, out string? key))
        return Error.Validation(IdempotencyErrorCodes.KeyInvalid, "Send a valid Idempotency-Key header.").ToProblemResult();

    Result<Receipt> result = await sender.Send(new CapturePaymentCommand(orderId, key, body.Amount), ct);
    return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemResult();
}).WithIdempotency("payments");
```

### OpenAPI

`AddIdempotencyConventions()` in the OpenApi or Swashbuckle package documents the key header, `Idempotency-Replayed` and the 400, 409 and 422 responses of idempotent endpoints.

## Enum conventions, binding and wire format

```csharp
builder.Services.AddEnumConventions();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseEnumBinding();     // after routing and auth, otherwise a 400 hides the 401

// [StringEnum] enum OrderStatus { Pending, PendingApproval }
// ?status=pending_approval, PENDING_APPROVAL, PendingApproval and 1 all bind to OrderStatus.PendingApproval
app.MapGet("/orders", (OrderStatus? status) => Results.Ok(status));
```

`AddEnumConventions(c => c with { ... })` takes the `EnumConventions` record of CSharpEssentials.Enums (`AcceptNumbers`, `CaseInsensitive`, `WriteAs`, `CanHandle`, ...). It configures the JSON options, the binding rules, body errors and output filters, and returns an `EnumConventionsBuilder` (`Services`, `ConfigureErrors`). `UseEnumBinding()` throws at startup without it.

- Route, query, header and form values of `[StringEnum]` enums accept the same spellings as a JSON body, in Minimal APIs (including `[AsParameters]`) and MVC (`[FromQuery]`, `[FromRoute]`, `[FromHeader]`, `[FromForm]`, DTO properties nested up to 8 levels).
- Nullable, array, `List<T>` and `[Flags]` parameters work. Arrays accept repeated keys and comma lists (`?s=a&s=b`, `?s=a,b`).
- An unknown name, an undefined number, a comma on a non-flags enum, or an empty value for a non-nullable parameter returns a 400 problem listing the allowed values: `'lost' is not a valid OrderStatus. Allowed values: pending, pending_approval, shipped.` The error code is the key (`status`, `filter.status`, `X-Status`). Whitespace is trimmed, and an empty value binds `null` for a nullable parameter.
- Change the error once for binding and JSON bodies: `.ConfigureErrors((error, key) => Error.Validation($"validation.{key}", error.Message))`.
- Enums without `[StringEnum]` or generator metadata keep the framework behavior. `AddEnumConventionsWithReflection(c => c with { CanHandle = ... })` opts them in through reflection (not trimming or AOT safe).
- Matched values are rewritten to the C# member name before binding, so link generation from ambient route values emits the member name.
- `AddEnumBinding` and `EnumBindingOptions` are obsolete forwarders to `AddEnumConventions`.

Output format per group, controller or action:

```csharp
app.MapGroup("/api/v1").WithEnumWireFormat(EnumWireFormat.Number);       // writes 1
app.MapGroup("/api/v2");                                                   // writes "pending_approval"
app.MapGroup("/api/shared").WithEnumWireFormat("X-Enum-Format", ctx =>      // per request, adds Vary: X-Enum-Format
    ctx.Request.Headers["X-Enum-Format"] == "number" ? EnumWireFormat.Number : EnumWireFormat.String);

[EnumWireFormat(EnumWireFormat.Number)]                                    // MVC controller or action
public sealed class LegacyOrdersController : ControllerBase { }
```

Precedence: action attribute, controller attribute, endpoint or group (`MapControllers().WithEnumWireFormat(...)`), then `WriteAs`. The host's JSON options are not changed; the other format is written with a copy. Status codes and headers are kept, and reading is not affected. `IEnumWireFormatMetadata` (`VaryHeader`, `SelectFormat(HttpContext)`) is the metadata both forms add.

## Conditional requests

```csharp Program.cs
// dotnet add package CSharpEssentials.AspNetCore
using System.Collections.Concurrent;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddEnhancedProblemDetails();
builder.Services.AddConditionalRequests();

WebApplication app = builder.Build();
var articles = new ConcurrentDictionary<int, Article> { [1] = new Article(1, "Hello", 1) };

app.MapGet("/articles/{id:int}", Result<Article> (int id) =>
        articles.TryGetValue(id, out Article? article) ? article : Error.NotFound("article.not_found", "No such article."))
    .AddEndpointFilter<ResultEndpointFilter>()
    .WithConditionalGet();                       // ETag "1"; 304 for If-None-Match: "1"

app.MapPut("/articles/{id:int}", Result<Article> (int id, string title, HttpContext http) =>
    {
        if (!articles.TryGetValue(id, out Article? current))
            return Error.NotFound("article.not_found", "No such article.");
        if (!http.GetPreconditions().Matches(current))
            return ConditionalRequestErrors.PreconditionFailed;   // 412
        return articles[id] = current with { Title = title, Revision = current.Revision + 1 };
    })
    .AddEndpointFilter<ResultEndpointFilter>()
    .WithIfMatch(required: true);                // 428 without If-Match

app.Run();

public sealed record Article(int Id, string Title, int Revision) : IVersioned
{
    public string Version => Revision.ToString(System.Globalization.CultureInfo.InvariantCulture);
}
```

`AddConditionalRequests(Action<ConditionalRequestOptions>?)` registers the services and the MVC filters; calling it again is safe. The validators of a value come from:

1. a registered `IETagSource<T>` (`builder.Services.AddETagSource<Article, ArticleETagSource>()`), matched on the exact runtime type, then base classes, resolved per request;
2. otherwise `IETagGenerator`. The default turns `IVersioned.Version` into a strong ETag `"{Version}"`; a version with characters not allowed in an ETag is hashed (base64url SHA-256), an empty one gives none. `versioned.ToETag()` returns the same tag;
3. with `o.UseBodyHashFallback = true`, other values get a weak ETag from the SHA-256 of their JSON. It is off by default because it serializes each response twice. Streams, `IAsyncEnumerable<T>` and `IQueryable` are never hashed.

```csharp
using Microsoft.Net.Http.Headers;   // EntityTagHeaderValue, not System.Net.Http.Headers

public sealed class ArticleETagSource : IETagSource<Article>
{
    public ResourceValidators? GetValidators(Article article) =>
        new(new EntityTagHeaderValue($"\"{article.Revision}\""), DateTimeOffset.UtcNow);
}
```

`ResourceValidators(EntityTagHeaderValue? ETag, DateTimeOffset? LastModified)`. Encode a `byte[]` row version with `Convert.ToBase64String`.

| Request | Response |
|---|---|
| GET/HEAD returning a value, a `Result<T>` success, `Ok<T>` or a typed `ToHttpResult()` success | `ETag` and `Last-Modified` (second precision, never later than now) are set |
| `If-None-Match` matches (weak comparison, `*` matches anything) | 304 without a body |
| No `If-None-Match`, `If-Modified-Since` not older than `Last-Modified` | 304 |
| The handler set `ETag` itself; strings, `ProblemDetails`, failures, non-2xx | Untouched |
| POST/PUT/PATCH/DELETE without `If-Match`, `required: true` | 428 |
| Malformed `If-Match` (`v1`, `*, "v1"`) | 400 |
| Handler returns `ConditionalRequestErrors.PreconditionFailed` | 412 |

`HttpContext.GetPreconditions()` returns `Preconditions`: `HasIfMatch`, `IsWildcard`, `IfMatch`, `Matches(IVersioned?)`, `Matches(ResourceValidators?)`, `Matches(EntityTagHeaderValue?)` (strong comparison) and `TryGetIfMatchVersion(out string version)`. Pass that version as the original concurrency token of your update (for example EF Core `OriginalValue`) for an atomic check; it equals `IVersioned.Version` only for versions that are valid ETags, such as numbers. `WithIfMatch<TBuilder, T>(loadCurrent, required)` loads the resource and returns 412 before the handler runs, which is not atomic.

- `If-Match` is evaluated only for POST, PUT, PATCH and DELETE. `If-Unmodified-Since` is not supported.
- The 412 comes from `DefaultErrorStatusCodeMapper`. A custom `IErrorStatusCodeMapper` must map `ConditionalRequestErrors.PreconditionFailedCode`, and an `IResultErrorMapper` must return 412 for it.
- MVC: `[ConditionalGet]` on a GET action or controller, `[IfMatch(Required = true)]` on a write action, or `MapControllers().WithConditionalGet()` and `.WithIfMatch()`.
- OpenAPI: endpoints with `WithIfMatch()` or `[IfMatch]` declare 400 and 412 problem responses, plus 428 when the header is required. MVC actions need `AddConditionalRequests()` for this. A status the endpoint already declares is kept.
- With `UseIdempotency()`, a retry with the same key replays the stored response without checking `If-Match` again.

## API versioning

```csharp
builder.Services.AddAndConfigureApiVersioning();

RouteGroupBuilder v2 = app.MapVersionedGroup(2);        // /v2, API version 2
v2.MapGet("/ping", () => "pong");                         // GET /v2/ping

RouteGroupBuilder users = app.CreateVersionedGroup("users", version: 1);   // /v1/users
users.MapGet("/{id:int}", (int id) => Results.Ok(new { id }));
```

`AddAndConfigureApiVersioning(configureOptions, configureExplorer)` sets version 1.0 as the default, assumes it when a request names none, reports `api-supported-versions`, reads the version from the `v{version}` URL segment or the `x-api-version` header, and registers the API explorer with group names `'v'VVV`. Both callbacks are optional and run after these defaults.

- `MapVersionedGroup(version)`: a `v{version:apiVersion}` group with its version set, to compose other groups under.
- `CreateVersionedGroup(route, version = 1)`: `v{version:apiVersion}/{route}` in one call.
- `CreateVersionSet(version = 1)`: just the `ApiVersionSet`.

## JSON options

`services.ConfigureSystemTextJson(jsonSerializerOptions, configureOptions)` copies the CSharpEssentials.Json serializer options (or the options you pass) into both the Minimal API and the MVC JSON options, then runs `configureOptions`. It also calls `AddControllers()`.

## See also

- Example: [examples/Examples.AspNetCore](https://github.com/senrecep/CSharpEssentials/tree/main/examples/Examples.AspNetCore) calls every feature above and prints the responses.
- Agent skill: [csharpessentials-aspnetcore](https://github.com/senrecep/CSharpEssentials/blob/main/.well-known/agent-skills/csharpessentials-aspnetcore/SKILL.md)
- [CSharpEssentials.Results](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD), [CSharpEssentials.Errors](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Errors/Readme.MD), [CSharpEssentials.Validation](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Validation/Readme.MD), [CSharpEssentials.Idempotency](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Idempotency/Readme.MD), [CSharpEssentials.Enums](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Enums/Readme.MD)
