# CSharpEssentials.AspNetCore

Production-ready ASP.NET Core utilities: global exception handling, structured ProblemDetails, API versioning, enum conventions, and Result endpoint filters.

## Features

- **Enhanced Problem Details**: One configurable ProblemDetails pipeline for Minimal API, MVC, the exception handler and status code pages. It has secure defaults and the extension points `IProblemDetailsEnricher`, `IErrorStatusCodeMapper` and `IExceptionProblemMapper`.
- **Global Exception Handler**: Maps exceptions to ProblemDetails. Unknown exceptions return 500, and the exception message goes to the log, not to the response.
- **Result Endpoint Filter**: Converts `Result<T>` return values to `200 OK` or a ProblemDetails response.
- **Validation Endpoint Filter**: Runs `CSharpEssentials.Validation` validators for a handler argument and returns a 400 ProblemDetails response on failure.
- **Idempotency**: `Idempotency-Key` handling for opted-in endpoints. A retry with the same key and request gets the stored response instead of running the handler again. The store is pluggable (`IIdempotencyStore`); in-memory and `IDistributedCache` stores are included.
- **Enum Binding**: Query and route values for `[StringEnum]` enums accept the same spellings as JSON. Invalid values return 400.
- **Conditional Requests**: `ETag`/`Last-Modified` with `304 Not Modified` on reads (`WithConditionalGet()`, `[ConditionalGet]`), and `If-Match` optimistic concurrency with 412/428 on writes (`WithIfMatch()`, `[IfMatch]`).
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

#### Returning problems from your own code

```csharp
// Minimal API
app.MapGet("/users/{id}", (int id) =>
    Error.NotFound("User.NotFound", "No such user").ToProblemResult())      // IResult; also on Error[] and on a failed Result
    .ProducesProblem(404);                                                  // OpenAPI metadata: application/problem+json (EnhancedProblemDetails)

IResult conflict = Error.Conflict("Order.Duplicate", "Already placed").ToProblemResult(statusCode: 422); // override the mapped status
EnhancedProblemDetails details = Error.NotFound("User.NotFound", "No such user").ToProblemDetails();     // the object, without writing it
```

```csharp
// MVC
public sealed class UsersController : ControllerBase
{
    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        Error error = Error.NotFound("User.NotFound", "No such user");
        return error.ToActionResult(HttpContext);   // or: this.Problem(error), this.Problem(errors), this.Problem(result)
    }
}
```

All of them take an optional `ErrorMetadata extensions` (extra fields on the problem) and `statusCode`. The status comes from the `ErrorType` of the error (`Validation` 400, `Unauthorized` 401, `Forbidden` 403, `NotFound` 404, `Conflict` 409, and so on) unless you pass one or register an `IErrorStatusCodeMapper`; with several errors the highest status wins. `[ValidateModel]` (attribute) and `ConfigureModelValidatorResponse()` return the invalid model state of MVC actions through `ToActionResult`, with the code `validation.{model state key}`.

`services.ConfigureSystemTextJson()` applies the `CSharpEssentials.Json` serializer options (or the `JsonSerializerOptions` you pass, then an optional `configureOptions` callback) to both the MVC and the Minimal API JSON options; it also calls `AddControllers()`.

### API Versioning

```csharp
builder.Services.AddAndConfigureApiVersioning(); // optional: configureOptions (ApiVersioningOptions), configureExplorer (ApiExplorerOptions)

var app = builder.Build();
var usersGroup = app.CreateVersionedGroup("users", version: 1);
usersGroup.MapGet("/{id}", (int id) => Results.Ok(new { Id = id }));
```

`AddAndConfigureApiVersioning` sets version 1.0 as the default, assumes it when the request names no version, reports `api-supported-versions`, reads the version from the `v{version}` URL segment or the `x-api-version` header, and registers the API Explorer with the group format `'v'VVV` and the version substituted in URLs. `CreateVersionedGroup("users", version: 1)` maps `v{version:apiVersion}/users`; `CreateVersionSet(version)` builds just the `ApiVersionSet`.

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
using CSharpEssentials.Validation.Extensions; // AddValidator

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

### Conditional requests

```csharp
using Microsoft.Net.Http.Headers; // EntityTagHeaderValue; not System.Net.Http.Headers

builder.Services.AddConditionalRequests();                      // MVC filters included; call once or many times
builder.Services.AddETagSource<Article, ArticleETagSource>();    // optional: ETag + Last-Modified for a type

// public sealed class Order : IVersioned { public uint Xmin { get; set; } public string Version => Xmin.ToString(); ... }
app.MapGet("/orders/{id}", async Task<Result<Order>> (Guid id, AppDbContext db) =>
        await db.Orders.FindAsync(id) is { } order ? order : Error.NotFound("order.notFound", "Order not found"))
    .AddEndpointFilter<ResultEndpointFilter>()
    .WithConditionalGet();                                      // ETag "{Version}"; a failure is untouched

app.MapPut("/orders/{id}", async Task<Result> (Guid id, UpdateOrder body, HttpContext http, AppDbContext db) =>
{
    Order? order = await db.Orders.FindAsync(id);
    if (order is null)
        return Error.NotFound("order.notFound", "Order not found");
    Preconditions preconditions = http.GetPreconditions();
    if (!preconditions.Matches(order))                          // same ETag as the GET, also for hashed versions
        return ConditionalRequestErrors.PreconditionFailed;     // 412
    if (preconditions.TryGetIfMatchVersion(out string? version))
        db.Entry(order).Property(o => o.Xmin).OriginalValue = uint.Parse(version); // atomic check in SaveChanges
    order.Apply(body);
    await db.SaveChangesAsync();
    return Result.Success();
}).AddEndpointFilter<ResultEndpointFilter>().WithIfMatch(required: true);

public sealed class ArticleETagSource : IETagSource<Article>
{
    public ResourceValidators? GetValidators(Article article) =>
        new(new EntityTagHeaderValue($"\"{article.Revision}\""), article.UpdatedAt); // long Revision
}
```

A `byte[]` row version must be encoded (`Convert.ToBase64String(rowVersion)`); interpolating it gives `"System.Byte[]"`.

MVC: `[ConditionalGet]` on a `[HttpGet, HttpHead]` action or controller, `[IfMatch(Required = true)]` on a write action, or `MapControllers().WithConditionalGet()` / `.WithIfMatch()` for all actions (action > controller > endpoint).

| Request | Response |
|---------|----------|
| `GET`/`HEAD` with a successful value (object, `Result<T>` success, `Ok<T>` or another 2xx `IValueHttpResult`) | `ETag` and `Last-Modified` (second precision, never later than now) are set |
| `If-None-Match` matches (weak comparison, `*` matches any) | `304 Not Modified` without a body; headers already on the response (`Cache-Control`, `Vary`, `Content-Location`) are kept |
| No `If-None-Match`, `If-Modified-Since` not older than `Last-Modified` | 304 (an unparsable date is ignored) |
| Handler already set an `ETag` header | Untouched (not overridden, no 304) |
| Strings, `ProblemDetails`, `null`, failed results, non-2xx results, other methods | Untouched (also a `Result<string>`) |
| `POST`/`PUT`/`PATCH`/`DELETE` without `If-Match`, `required: true` | 428 Precondition Required |
| Malformed `If-Match` (`v1`, `*, "v1"`) | 400 |
| Handler returns `ConditionalRequestErrors.PreconditionFailed` | 412 Precondition Failed (`DefaultErrorStatusCodeMapper`) |

`If-Match` is evaluated only for `POST`, `PUT`, `PATCH` and `DELETE`; safe methods get empty preconditions. `If-Unmodified-Since` is not supported. A header sent on several lines is combined. The 412 comes from `DefaultErrorStatusCodeMapper`: a registered `IResultErrorMapper` decides the `ResultEndpointFilter` response itself, and a custom `IErrorStatusCodeMapper` must map `ConditionalRequestErrors.PreconditionFailedCode` to 412.

Validators: a registered `IETagSource<T>` (exact runtime type, then base classes; interfaces are rejected; resolved per request so scoped dependencies work), otherwise `IETagGenerator`. The default generator turns `IVersioned.Version` into a strong ETag (versions with characters not allowed in an ETag are hashed to base64url SHA-256; an empty version gives none); `versioned.ToETag()` returns the same tag. With `AddConditionalRequests(o => o.UseBodyHashFallback = true)` other values get a weak ETag from the SHA-256 of their JSON. Only buffered values are hashed: `IAsyncEnumerable<T>`, `Stream`, `PipeReader` and `IQueryable` get no ETag, so they are not enumerated or run twice. Return materialized values (a list or array): a lazy `IEnumerable<T>` (`yield`, `AsEnumerable().Select(...)`) is enumerated once for the hash and again for the response. The hash uses the host `JsonOptions` (`ConfigureHttpJsonOptions`), also for MVC actions. It is off by default because it serializes every response twice. `Last-Modified` is capped at now, read from the `TimeProvider` in DI (the system clock when none is registered).

`Preconditions` (`HttpContext.GetPreconditions()`): `HasIfMatch`, `IsWildcard`, `IfMatch`, `Matches(IVersioned?)`, `Matches(ResourceValidators?)`, `Matches(EntityTagHeaderValue?)` (strong comparison, weak tags never match), and `TryGetIfMatchVersion(out string version)`, the value of a single strong tag. That value equals `IVersioned.Version` only when the version is a valid entity tag (numbers such as `uint xmin` or a `long` revision are); hashed versions must be compared with `Matches(IVersioned?)`. Pass it as the original concurrency token of the update (for example EF Core `OriginalValue`) for an atomic check. `WithIfMatch<TBuilder, T>(loadCurrent)` loads the current resource and returns 412 before the handler; that check is not atomic. A weak ETag (the body hash fallback) never matches there, so only `If-Match: *` passes. With `UseIdempotency()`, a retry with the same `Idempotency-Key` replays the stored response without evaluating `If-Match` again; the fingerprint covers method, path, query and body, not headers.

### Structured Error Response

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Validation Error",
  "status": 400,
  "detail": "Email format is incorrect.",
  "instance": "/users",
  "errorCodes": ["User.InvalidEmail"],
  "errors": [
    { "code": "User.InvalidEmail", "description": "Email format is incorrect." }
  ],
  "traceId": "0af7651916cd43dd8448eb211c80319c"
}
```
