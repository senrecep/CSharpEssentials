# CSharpEssentials.AspNetCore

Production-ready ASP.NET Core utilities: global exception handling, structured ProblemDetails, API versioning, enum conventions, and Result endpoint filters.

## Features

- **Enhanced Problem Details**: One configurable ProblemDetails pipeline for Minimal API, MVC, the exception handler and status code pages. It has secure defaults and the extension points `IProblemDetailsEnricher`, `IErrorStatusCodeMapper` and `IExceptionProblemMapper`.
- **Global Exception Handler**: Maps exceptions to ProblemDetails. Unknown exceptions return 500, and the exception message goes to the log, not to the response.
- **Result Endpoint Filter**: Converts `Result<T>` return values to `200 OK` or a ProblemDetails response.
- **Validation Endpoint Filter**: Runs `CSharpEssentials.Validation` validators for a handler argument and returns a 400 ProblemDetails response on failure.
- **Idempotency**: `Idempotency-Key` handling for opted-in endpoints. A retry with the same key and request gets the stored response instead of running the handler again. The store is pluggable (`IIdempotencyStore`); in-memory and `IDistributedCache` stores are included.
- **Enum Binding**: Query and route values for `[StringEnum]` enums accept the same spellings as JSON. Invalid values return 400.
- **API Versioning**: Pre-configured URL segment and header-based versioning.
- **OpenAPI**: Not part of this package. Add `CSharpEssentials.AspNetCore.OpenApi` (`Microsoft.AspNetCore.OpenApi`) or `CSharpEssentials.AspNetCore.Swashbuckle` (Swagger), one per host.

## Installation

```bash
dotnet add package CSharpEssentials.AspNetCore
```

Depends on `CSharpEssentials.Errors`, `CSharpEssentials.Json`, `CSharpEssentials.Results` and `CSharpEssentials.Validation` (for the validation endpoint filter). None of them references this package, so there is no cycle.

## Usage

### Problem Details

```csharp
builder.Services.AddEnhancedProblemDetails(o =>
{
    o.TraceId = TraceIdFormat.W3CTraceId;                 // default
    o.ErrorFields = ProblemErrorFields.Codes | ProblemErrorFields.ValidationErrors; // default
    o.ExposeExceptionDetails = builder.Environment.IsDevelopment();
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Optional extension points
builder.Services.AddProblemDetailsEnricher<TenantEnricher>();     // add fields to every problem response
builder.Services.AddErrorStatusCodeMapper<AuthAwareStatusMapper>(); // Error -> status code / title
builder.Services.AddExceptionProblemMapper<PaymentExceptionMapper>(); // Exception -> problem

var app = builder.Build();
app.UseEnhancedProblemDetails(); // UseExceptionHandler + UseStatusCodePages
```

| Option | Default | Notes |
|---|---|---|
| `TraceId` | `W3CTraceId` | `TraceparentHeader` or `None` |
| `IncludeRequestId` / `IncludeUser` / `IncludeSpanIds` | `false` | |
| `Instance` | `Path` | `MethodAndPath` or `None` |
| `ErrorFields` | `Codes \| ValidationErrors` | `Messages`, `AllErrors`, `All`, `None` |
| `ValidationErrorsFormat` | `List` | `Dictionary` groups descriptions by code |
| `TypeUriResolver` | `ProblemTypeUris.Rfc9110` | `ProblemTypeUris.Rfc7231` or a custom function |
| `ExposeExceptionDetails` | `false` | Enable only in Development |

`o.UseLegacyDefaults()` restores the 3.x output. See the migration notes in the repository README.

With no W3C `Activity` (tracing disabled), `W3CTraceId` falls back to `HttpContext.TraceIdentifier`. `errors` lists only `ErrorType.Validation` errors; every code is in `errorCodes`.

`services.ConfigureInvalidModelStateResponse()` writes the automatic 400 of `[ApiController]` (invalid model state) through the same pipeline, with the model state key as the error code. It can be called before or after `AddControllers()`. It has no effect together with `ConfigureModelValidatorResponse()`, which turns the automatic 400 off (`SuppressModelStateInvalidFilter = true`); use one or the other.

`errors.ToProblemResult()` (Minimal API) and `errors.ToActionResult(HttpContext)` (MVC) read these options when they execute, so both produce the same JSON.

### API Versioning

```csharp
builder.Services.AddAndConfigureApiVersioning();

var app = builder.Build();
var usersGroup = app.CreateVersionedGroup("users", version: 1);
usersGroup.MapGet("/{id}", (int id) => Results.Ok(new { Id = id }));
```

`MapVersionedGroup(version)` creates a group with only the `v{version:apiVersion}` prefix and the version set, so other route groups can be composed under it:

```csharp
var v2 = app.MapVersionedGroup(2);            // /v2, ApiExplorer group "v2"
v2.MapGet("/ping", () => "pong");             // GET /v2/ping
v2.MapAppsEndpoints();                        // CSharpEssentials.Endpoints registry under /v2
```

### OpenAPI Documents

`AddSwagger`, `UseVersionableSwagger` and the Swagger filters moved to `CSharpEssentials.AspNetCore.Swashbuckle` in 5.0 (same namespaces). For `Microsoft.AspNetCore.OpenApi` use `CSharpEssentials.AspNetCore.OpenApi`. A host references one of them, never both.

### Result Endpoint Filter

```csharp
builder.Services.AddScoped<IResultErrorMapper, DomainErrorMapper>(); // optional, any lifetime

app.MapGet("/users/{id}", (int id) => GetUser(id))
   .AddEndpointFilter<ResultEndpointFilter>();

// Returns 200 with value on success; on failure the mapper's result (resolved per request),
// or a ProblemDetails response when no mapper is registered
```

### Validation Endpoint Filter

```csharp
builder.Services.AddValidator<CreateUserRequest, CreateUserRequestValidator>();

app.MapPost("/users", (CreateUserRequest request) => CreateUser(request))
   .WithValidation<CreateUserRequest>();
```

- Validates each non-null argument of type `T` with every registered `IValidator<T>`, in ascending `Order`. Errors are combined and returned with `ToProblemResult()` (validation errors → 400); the handler does not run.
- A `null` argument (optional body) is passed through.
- No registered `IValidator<T>` throws `InvalidOperationException` at request time, so unvalidated input never reaches the handler. A handler without a `T` parameter throws when the endpoint is built.
- `ValidationEndpointFilter<T>` can also be added directly with `AddEndpointFilter<ValidationEndpointFilter<T>>()`, for example on a route group.

### Idempotency

```csharp
builder.Services.AddIdempotency(o =>
{
    o.RetentionPeriod = TimeSpan.FromHours(24);   // default
    o.RequireKey = true;                           // default false: requests without a key pass through
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseIdempotency();                              // after auth, so the key is scoped to the user

app.MapPost("/orders", CreateOrder).WithIdempotency();

[HttpPost, Idempotent]                             // MVC
public IActionResult Create(CreateOrderRequest request) => ...;
```

How a request with an `Idempotency-Key` header is handled:

| Situation | Response |
|-----------|----------|
| First request | The handler runs. A response that `ShouldStore` accepts (default: 2xx) is stored for `RetentionPeriod` |
| Same key, same request, completed | The stored status, body and allowlisted headers, plus `Idempotency-Replayed: true`. The handler does not run |
| Same key, same request, still running | 409 with `Retry-After` |
| Same key, different method, path, query or body | 422 |
| Missing key | Passes through (400 with `RequireKey`) |
| Empty key, key longer than `MaxKeyLength` (255), several key headers | 400 |
| Handler throws before the response started, or `ShouldStore` rejects the status | The key is released, so the client can retry |
| Handler throws after the response started (for example the client went away mid-write) | The key stays in flight until `InFlightTimeout`, because the work is most likely done |

- Only endpoints with `IdempotentAttribute` metadata (`WithIdempotency()`, `[Idempotent]`, or on a route group) and methods in `Methods` (default POST and PATCH) participate.
- Keys are scoped to the authenticated user (`NameIdentifier`, then `sub`), so a user only ever receives responses stored under the same scope. Requests without a scope pass through; set `KeyScope` (for example to a tenant id, or the issuer plus the subject when several authentication schemes can produce the same `sub`) or `AllowUnscopedKeys = true` to change that. Unscoped keys live in their own namespace and never match a scoped entry.
- Only the headers in `ReplayedHeaders` are replayed (`Content-Type`, `Content-Language`, `Location`, `ETag`, `Cache-Control`). `Set-Cookie` is never stored.
- Put `UseResponseCompression()` before `UseIdempotency()`: the stored body is uncompressed and compression runs again on replay. `Content-Encoding` and `Vary` are not replayed by default; add them only when the handler encodes the body itself.
- A response body larger than `MaxResponseBodySize` (1 MiB) is sent normally but not stored, and a warning is logged.
- The request body is buffered (`EnableBuffering`) to compute the fingerprint; bodies over 30 KB spill to a temporary file.
- `InFlightTimeout` (1 minute) frees the key of a request that never finished, for example after a crash. Keep it longer than your request timeout: a retry after it expires runs the handler again even if the first request is still running. Each reservation has a token, so the late request can no longer overwrite or release the retry's entry (a warning is logged), but its side effects have already happened.
- A store failure after the handler ran is logged and does not change the response; the key then stays in flight until `InFlightTimeout`.

Stores:

| Store | Register with | Notes |
|-------|---------------|-------|
| `InMemoryIdempotencyStore` | default, or `o.UseInMemoryStore()` | Single instance only. Unbounded: every stored response stays in memory for `RetentionPeriod`, so use it for development or small workloads |
| `DistributedCacheIdempotencyStore` | `o.UseDistributedCacheStore()` plus an `IDistributedCache` | Best effort: `IDistributedCache` has no atomic add, so two simultaneous first requests on different instances can both run |
| Your own `IIdempotencyStore` | `o.UseStore<TStore>(lifetime)` | Use an atomic insert (for example `INSERT ... ON CONFLICT DO NOTHING` or Redis `SET NX`) for a strict guarantee, and make `CompleteAsync`/`ReleaseAsync` conditional on the reservation token (`WHERE key = @key AND token = @token`) |

### Enum Conventions: JSON, Binding and Wire Format

```csharp
builder.Services.AddEnumConventions(); // JSON options, binding rules, body errors, output filters
app.UseEnumBinding(); // after routing, UseAuthentication and UseAuthorization (otherwise a 400 hides the 401)

// [StringEnum] enum OrderStatus { Pending, PendingApproval }
// ?status=pending_approval, ?status=PENDING_APPROVAL, ?status=PendingApproval, ?status=1 all bind to OrderStatus.PendingApproval
app.MapGet("/orders", (OrderStatus? status) => Results.Ok(status));
```

`AddEnumConventions(c => c with { ... })` takes the `EnumConventions` of `CSharpEssentials.Enums` (`AcceptNumbers`, `CaseInsensitive`, `WriteAs`, `CanHandle`, ...). Route, query, header and form values follow the same accept rules as a JSON body. This works for Minimal API (including `[AsParameters]`) and MVC (`[FromQuery]`, `[FromRoute]`, `[FromHeader]`, `[FromForm]`, and DTO properties, nested up to 8 levels: `?filter.inner.status=…`). `UseEnumBinding()` throws at startup when `AddEnumConventions()` was not called.

- Nullable, array, `List<T>` and `[Flags]` parameters are supported. Arrays accept repeated keys and comma-separated values (`?s=a&s=b`, `?s=a,b`); flags values are a comma-separated list.
- These values return a 400 ProblemDetails response that lists the allowed values (`'99' is not a valid OrderStatus. Allowed values: pending, pending_approval.`):
  - undefined numbers;
  - unknown names (leading and trailing whitespace is trimmed first, so `?status=%20pending` binds, while `?status=%20bogus%20` is still a 400; an empty or whitespace-only value binds `null` for a nullable parameter and is a 400 for a non-nullable one);
  - commas on enums without `[Flags]`;
  - empty values for non-nullable parameters.
- A missing required value keeps the framework's handling.
- The error code is the key (`status`, `filter.status`, `X-Status`). Change the code or message once for binding and JSON bodies with `.ConfigureErrors((error, key) => Error.Validation($"validation.{key}", error.Message))`.
- Enums without generated metadata (no `[StringEnum]`) keep the framework's binding and output. `AddEnumConventionsWithReflection(c => c with { CanHandle = ... })` opts them in with reflection (not trimming or AOT safe).
- A JSON body error of a Minimal API endpoint (with `RouteHandlerOptions.ThrowOnBadRequest`) is mapped by `GlobalExceptionHandler` to the same 400. MVC reports body errors through model state.
- Empty values of nullable or collection parameters are dropped, so they bind as `null` or are skipped.
- Matched values are rewritten to the C# member name before binding. Link generation that reuses the current route values (ambient values, such as `CreatedAtRoute` without explicit values) therefore emits the member name (`/items/HttpStatus`). Pass the value explicitly to get the JSON spelling.

Legacy numeric output per group, controller or action, in both directions (`WriteAs` is the global default):

```csharp
app.MapGroup("/api/v1").WithEnumWireFormat(EnumWireFormat.Number);  // old clients: 1
app.MapGroup("/api/v2");                                              // "pending_approval"
app.MapGroup("/api/shared").WithEnumWireFormat("X-Enum-Format",       // per request, adds Vary: X-Enum-Format
    ctx => ctx.Request.Headers["X-Enum-Format"] == "string" ? EnumWireFormat.String : EnumWireFormat.Number);

[EnumWireFormat(EnumWireFormat.Number)] // MVC controller or action
public sealed class LegacyOrdersController : ControllerBase { ... }
```

Precedence: action attribute > controller attribute > endpoint or group (`MapControllers().WithEnumWireFormat(...)` for MVC) > `WriteAs`. The host's `JsonOptions` are never changed; the other format is written with a copy. Status codes and headers of the result (`Created` + `Location`, `Results<Ok<T>, NotFound>`) are kept. Reading is not affected.

`AddEnumBinding`/`EnumBindingOptions` are obsolete forwarders to `AddEnumConventions` (see the v4 to v5 migration guide).

### Structured Error Response

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Email format is incorrect.",
  "instance": "/users",
  "traceId": "0af7651916cd43dd8448eb211c80319c",
  "errorCodes": ["User.InvalidEmail"],
  "errors": [
    { "code": "User.InvalidEmail", "description": "Email format is incorrect." }
  ]
}
```
