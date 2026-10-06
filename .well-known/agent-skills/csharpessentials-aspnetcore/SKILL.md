---
name: csharpessentials-aspnetcore
description: Use when wiring CSharpEssentials into ASP.NET Core. Covers AddEnhancedProblemDetails/UseEnhancedProblemDetails with GlobalExceptionHandler (secure 4.0 ProblemDetails defaults), ToProblemResult/ToActionResult, ResultEndpointFilter with IResultErrorMapper, AddEnumBinding/UseEnumBinding for [StringEnum] query/route values, ConfigureInvalidModelStateResponse, MapVersionedGroup and versioned Swagger.
---

# CSharpEssentials.AspNetCore

ASP.NET Core integration: one ProblemDetails pipeline for `Error`/`Result` values and exceptions, automatic `Result<T>`-to-HTTP conversion, enum binding, API versioning and Swagger.

## Installation

```bash
dotnet add package CSharpEssentials.AspNetCore
```

Depends on `Asp.Versioning.*` 8.1+ and `Swashbuckle.AspNetCore` `[8.1.0, 10)`.

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

## Enum Query/Route Binding

```csharp
builder.Services.AddEnumBinding(o =>   // optional; UseEnumBinding works with the defaults
{
    o.AllowIntegerValues = false;
    o.ErrorFactory = (key, enumType, names) =>
        Error.Validation($"validation.{key}", $"Use one of: {string.Join(", ", names)}");
});

app.UseEnumBinding();                  // after routing has selected the endpoint
```

`[StringEnum]` enums in query/route values (Minimal API incl. `[AsParameters]`, and MVC) accept the snake_case name, the C# member name (case-insensitive) or, unless disabled, the number of a defined member. Invalid values return a 400 ProblemDetails response. `EnumBindingOptions` also has `CanBind` and `NamingPolicy`. Swagger enum schemas use the same names.

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
builder.Services.AddSwagger<DefaultConfigureSwaggerOptions>(SecuritySchemes.JwtBearerTokenSecurity);

app.UseVersionableSwagger();                        // one Swagger UI endpoint per API version

// 4.1: "v{version:apiVersion}" route group bound to API version 2
RouteGroupBuilder v2 = app.MapVersionedGroup(2);
v2.MapGet("/health", () => Results.Ok());          // GET /v2/health
```

`CreateVersionedGroup("orders", version: 1)` builds `v{version:apiVersion}/orders` in one call.

---

## Best Practices

- Call `AddEnhancedProblemDetails()` (not plain `AddProblemDetails()`) and `app.UseEnhancedProblemDetails()`
- Keep `ExposeExceptionDetails` off outside development; never put raw exception messages in `ExceptionProblem.Detail`
- `ToProblemResult` / `ToActionResult` return `EnhancedProblemHttpResult` / `EnhancedProblemObjectResult`, not `ProblemHttpResult` / `BadRequestObjectResult`
- Apply `ResultEndpointFilter` at the group level, not per endpoint
- `error.Description` is the field name, not `error.Message`
