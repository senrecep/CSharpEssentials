---
name: csharpessentials-aspnetcore
description: Use when wiring CSharpEssentials into ASP.NET Core. Covers AddEnhancedProblemDetails/UseEnhancedProblemDetails with GlobalExceptionHandler (secure 4.0 ProblemDetails defaults), ToProblemResult/ToActionResult, ResultEndpointFilter with IResultErrorMapper, AddEnumConventions/UseEnumBinding/WithEnumWireFormat for [StringEnum] route/query/header/form values and output format, ConfigureInvalidModelStateResponse, MapVersionedGroup and versioned Swagger.
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
```

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

## Best Practices

- Call `AddEnhancedProblemDetails()` (not plain `AddProblemDetails()`) and `app.UseEnhancedProblemDetails()`
- Keep `ExposeExceptionDetails` off outside development; never put raw exception messages in `ExceptionProblem.Detail`
- `ToProblemResult` / `ToActionResult` return `EnhancedProblemHttpResult` / `EnhancedProblemObjectResult`, not `ProblemHttpResult` / `BadRequestObjectResult`
- Apply `ResultEndpointFilter` at the group level, not per endpoint
- `error.Description` is the field name, not `error.Message`
