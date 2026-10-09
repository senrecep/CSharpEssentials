---
name: csharpessentials-aspnetcore
description: Use when wiring CSharpEssentials into ASP.NET Core. Covers AddEnhancedProblemDetails/UseEnhancedProblemDetails with GlobalExceptionHandler (secure 4.0 ProblemDetails defaults), ToProblemResult/ToActionResult, ResultEndpointFilter with IResultErrorMapper, AddEnumConventions/UseEnumBinding/WithEnumWireFormat for [StringEnum] route/query/header/form values and output format, ConfigureInvalidModelStateResponse, MapVersionedGroup, versioned Swagger (CSharpEssentials.AspNetCore.Swashbuckle) and enum schemas for Microsoft.AspNetCore.OpenApi (CSharpEssentials.AspNetCore.OpenApi).
---

# CSharpEssentials.AspNetCore

ASP.NET Core integration: one ProblemDetails pipeline for `Error`/`Result` values and exceptions, automatic `Result<T>`-to-HTTP conversion, enum binding, API versioning and Swagger.

## Installation

```bash
dotnet add package CSharpEssentials.AspNetCore
```

Depends on `Asp.Versioning.*` 8.1+. Swagger (`AddSwagger`, `UseVersionableSwagger`) lives in `CSharpEssentials.AspNetCore.Swashbuckle`, which needs `Swashbuckle.AspNetCore` `[10.2.3, 11)` and `Microsoft.OpenApi` 2.x on every target.

## Namespace

```csharp
using CSharpEssentials.AspNetCore;
```

---

## ProblemDetails + GlobalExceptionHandler

`AddEnhancedProblemDetails` configures one ProblemDetails pipeline (RFC 9457) shared by `ToProblemResult` (Minimal API), `ToActionResult` (MVC), `GlobalExceptionHandler`, status code pages and framework 404/405 responses.

```csharp
builder.Services.AddEnhancedProblemDetails(o =>
{
    o.ExposeExceptionDetails = builder.Environment.IsDevelopment(); // default false
    // o.UseLegacyDefaults();  // restore the 3.x output
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

app.UseEnhancedProblemDetails(); // UseExceptionHandler() + UseStatusCodePages()
```

### Secure defaults (4.0)

| Option | Default | 3.x (`UseLegacyDefaults()`) |
|---|---|---|
| `TraceId` | `TraceIdFormat.W3CTraceId` (32-hex trace id) | `TraceparentHeader` |
| `IncludeRequestId` / `IncludeUser` / `IncludeSpanIds` | `false` | `true` |
| `Instance` | `ProblemInstanceFormat.Path` (`"/path"`) | `MethodAndPath` |
| `ErrorFields` | `ProblemErrorFields.Codes \| ValidationErrors` | `All` |
| `TypeUriResolver` | `ProblemTypeUris.Rfc9110` | `Rfc7231` |
| `ExposeExceptionDetails` | `false` | N/A |

`errors` holds only `ErrorType.Validation` errors as `{ code, description }` (`ValidationErrorsFormat.List`).

### Status codes

Default `IErrorStatusCodeMapper`: Validation → 400, Unauthorized → 401, Forbidden → 403, NotFound → 404, Conflict → 409, Failure / Unexpected / Unknown → 500.

`DefaultExceptionProblemMapper` (always last): `OperationCanceledException` from an aborted request → 499, `BadHttpRequestException` → its status, `EnhancedValidationException` → 400, `DomainException` → status of its `ErrorType`, anything else → 500 with a generic detail (the message is logged, not returned).

### Extension points

```csharp
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
builder.Services.AddProblemDetailsEnricher<TenantEnricher>();          // runs after the built-in enrichment
builder.Services.AddErrorStatusCodeMapper<AuthAwareStatusMapper>();    // replaces the default mapper
builder.Services.AddExceptionProblemMapper<TimeoutExceptionMapper>();  // tried before DefaultExceptionProblemMapper
```

### Results to responses

```csharp
Error error = Error.NotFound("user.not_found", "User not found");
IResult minimalApi = error.ToProblemResult();   // EnhancedProblemHttpResult
IActionResult mvc  = error.ToActionResult();    // EnhancedProblemObjectResult

error.ToProblemResult(statusCode: 422);         // override the mapped status; also an optional ErrorMetadata for extra fields
// inside a controller: this.Problem(error)    // ControllerBase extension; also on Error[] and on a failed Result
EnhancedProblemDetails details = error.ToProblemDetails();   // the object, without writing it
app.MapGet("/users/{id}", GetUser).ProducesProblem(404);    // OpenAPI: application/problem+json
```

`[ValidateModel]` (an MVC filter attribute) and `ConfigureModelValidatorResponse()` return the invalid model state through `ToActionResult` with the code `validation.{key}`. `services.ConfigureSystemTextJson()` applies the `CSharpEssentials.Json` options to the MVC and Minimal API JSON options and calls `AddControllers()`.

---

## ResultEndpointFilter

Converts `Result` / `Result<T>` returns from Minimal API handlers: success → 200 (with the value), failure → ProblemDetails (4.0; 3.x returned 400 with raw `Error[]`).

```csharp
RouteGroupBuilder api = app.MapGroup("/api").AddEndpointFilter<ResultEndpointFilter>();

api.MapGet("/users/{id:guid}", Result<User> (Guid id) =>
    id == Guid.Empty ? Error.NotFound("user.not_found", "User not found") : new User());
```

A registered `IResultErrorMapper` (resolved from the request services, so any lifetime works) replaces the ProblemDetails response:

```csharp
public sealed class LegacyErrorMapper : IResultErrorMapper
{
    public IResult Map(Error[] errors) =>
        errors[0].Type == ErrorType.NotFound ? Results.NotFound() : errors.ToProblemResult();
}
```

```csharp
builder.Services.AddScoped<IResultErrorMapper, LegacyErrorMapper>();
```

---

## Validation Endpoint Filter

```csharp
using CSharpEssentials.Validation.Extensions; // AddValidator

builder.Services.AddValidator<CreateUserRequest, CreateUserRequestValidator>();   // IValidator<T> from CSharpEssentials.Validation

app.MapPost("/users", (CreateUserRequest request) => CreateUser(request)).WithValidation<CreateUserRequest>();
```

Every registered `IValidator<T>` runs in ascending `Order` on each non-null `T` argument; failures return a 400 ProblemDetails response and the handler does not run. A `null` argument passes through. With no registered validator the request throws `InvalidOperationException`; a handler without a `T` parameter throws when the endpoint is built. `ValidationEndpointFilter<T>` can be added directly with `AddEndpointFilter<ValidationEndpointFilter<T>>()` (for a group).

---

## Idempotency

```csharp
builder.Services.AddIdempotency(o => { o.RetentionPeriod = TimeSpan.FromHours(24); o.RequireKey = true; });

app.UseAuthentication();
app.UseAuthorization();
app.UseIdempotency();                      // after auth: keys are scoped to the user

app.MapPost("/orders", CreateOrder).WithIdempotency();   // or [Idempotent] on an MVC action, or on a group
```

Applies to endpoints marked with `IdempotentAttribute` metadata and to POST and PATCH by default (`Methods`). A repeat with the same `Idempotency-Key`, method, path, query and body replays the stored response (`Idempotency-Replayed: true`); the same key with a different request returns 422; a request still running returns 409 with `Retry-After`; a missing key passes through (400 with `RequireKey`). Keys are scoped to the authenticated user (`NameIdentifier`, then `sub`); requests without a scope pass through unless you set `KeyScope` or `AllowUnscopedKeys = true`. Stores: in-memory (default, single instance), `o.UseDistributedCacheStore()` (best effort, needs `IDistributedCache`), or your own `IIdempotencyStore` with `o.UseStore<TStore>(lifetime)`.

---

## Conditional Requests

```csharp
using Microsoft.Net.Http.Headers;   // EntityTagHeaderValue

builder.Services.AddConditionalRequests();
builder.Services.AddETagSource<Article, ArticleETagSource>();   // optional IETagSource<T>

app.MapGet("/orders/{id}", GetOrder).AddEndpointFilter<ResultEndpointFilter>().WithConditionalGet();   // ETag/Last-Modified, 304
app.MapPut("/orders/{id}", UpdateOrder).AddEndpointFilter<ResultEndpointFilter>().WithIfMatch(required: true); // 428 without If-Match

// in the handler
Preconditions preconditions = http.GetPreconditions();
if (!preconditions.Matches(order))
    return ConditionalRequestErrors.PreconditionFailed;   // 412
```

Entities that implement `IVersioned` (`string Version`) get a strong ETag `"{Version}"` (versions with characters not allowed in an ETag are hashed); `IETagSource<T>` overrides that per type and `AddConditionalRequests(o => o.UseBodyHashFallback = true)` adds a weak ETag from the JSON body for other values. `If-Match` is evaluated only for POST, PUT, PATCH and DELETE; a malformed header returns 400. MVC uses `[ConditionalGet]` and `[IfMatch(Required = true)]`, or `MapControllers().WithConditionalGet()` / `.WithIfMatch()`. A registered `IResultErrorMapper` decides the `ResultEndpointFilter` response itself, so it must return 412 for `ConditionalRequestErrors.PreconditionFailed`; a custom `IErrorStatusCodeMapper` must map `ConditionalRequestErrors.PreconditionFailedCode` to 412.

---

## Enum Conventions and Binding

```csharp
builder.Services.AddEnumConventions(c => c with { AcceptNumbers = false })   // required by UseEnumBinding
    .ConfigureErrors((error, key) => Error.Validation($"validation.{key}", error.Message));

app.UseEnumBinding();                  // after routing has selected the endpoint

app.MapGroup("/api/v1").WithEnumWireFormat(EnumWireFormat.Number);   // legacy numeric output
```

`[StringEnum]` enums in route, query, header and form values (Minimal API incl. `[AsParameters]`, and MVC) accept the same spellings as a JSON body. Invalid values return a 400 ProblemDetails response listing the allowed values; the same error is used for Minimal API JSON body errors. Enums without generated metadata keep the framework's behavior unless `AddEnumConventionsWithReflection` opts them in. Use a typed enum parameter instead of a string parameter + action filter + scoped holder (`EnumData<T>`). `[EnumWireFormat]` sets the output format of an MVC controller or action (action > controller > group > `WriteAs`). `AddEnumBinding` is an obsolete forwarder.

## MVC Invalid Model State

```csharp
builder.Services.AddControllers();
builder.Services.ConfigureInvalidModelStateResponse(); // [ApiController] 400s become enhanced ProblemDetails

// Custom error per model state entry:
// builder.Services.ConfigureInvalidModelStateResponse((key, modelError) =>
//     Error.Validation($"validation.{key}", modelError.ErrorMessage));
```

Do not combine with `ConfigureModelValidatorResponse()`, which turns the automatic 400 off.

---

## API Versioning + Swagger

```csharp
builder.Services.AddAndConfigureApiVersioning();   // v1 default, URL segment or x-api-version header
builder.Services.AddSwagger<DefaultConfigureSwaggerOptions>(  // CSharpEssentials.AspNetCore.Swashbuckle
    SecuritySchemes.JwtBearerSchemeName,            // 6.0: the scheme id is an argument
    SecuritySchemes.JwtBearerTokenSecurity);

app.UseVersionableSwagger();                        // one Swagger UI endpoint per API version

// 4.1: "v{version:apiVersion}" route group bound to API version 2
RouteGroupBuilder v2 = app.MapVersionedGroup(2);
v2.MapGet("/health", () => Results.Ok());          // GET /v2/health
```

`CreateVersionedGroup("orders", version: 1)` builds `v{version:apiVersion}/orders` in one call.

Optional route parameters (`{id?}`, `{id:int?}`, `{page=1}`) are described as one path per form (`/orders` + `/orders/{id}`, every path parameter required); the shorter form's operationId is `{operationId}Without{Param}` and a duplicate throws at document generation. `o.AddOptionalRouteParameters(OptionalRouteParameterMode.RequiredOnly)` (on `SwaggerGenOptions`, after `AddSwagger`) keeps one path per operation; avoid the obsolete `LegacyNonCompliant`, it emits invalid OpenAPI. With `Microsoft.AspNetCore.OpenApi` (`CSharpEssentials.AspNetCore.OpenApi`, 6.2.0) the same behavior is opt-in: `services.AddOpenApi("v1", o => o.AddOptionalRouteParameters())` (`OpenApiOptionalRouteParameterMode.SplitPaths` or `RequiredOnly`).

---

## OpenAPI Enum Schemas

A host references one OpenAPI package, never both: `CSharpEssentials.AspNetCore.Swashbuckle` (Swashbuckle) or `CSharpEssentials.AspNetCore.OpenApi` (`Microsoft.AspNetCore.OpenApi`, net10.0 only). Both describe `[StringEnum]` enums the way they are written (wire names, `x-enum-varnames`).

```csharp
builder.Services.AddEnumConventions();                              // CSharpEssentials.AspNetCore
builder.Services.AddOpenApi("v1", o => o.AddEnumConventions());     // CSharpEssentials.AspNetCore.OpenApi, once per document
app.MapOpenApi();

builder.Services.AddSwaggerGen(o => o.AddEnumConventions());        // CSharpEssentials.AspNetCore.Swashbuckle; AddSwagger already calls it
```

---

## Best Practices

- Call `AddEnhancedProblemDetails()` (not plain `AddProblemDetails()`) and `app.UseEnhancedProblemDetails()`
- Keep `ExposeExceptionDetails` off outside development; never put raw exception messages in `ExceptionProblem.Detail`
- `ToProblemResult` / `ToActionResult` return `EnhancedProblemHttpResult` / `EnhancedProblemObjectResult`, not `ProblemHttpResult` / `BadRequestObjectResult`
- Apply `ResultEndpointFilter` at the group level, not per endpoint
- `Error` has `Code` and `Description` (there is no `Error.Message`); binding errors use the model key as the code
