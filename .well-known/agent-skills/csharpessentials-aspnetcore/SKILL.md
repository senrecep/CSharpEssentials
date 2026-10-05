---
name: csharpessentials-aspnetcore
description: Use when wiring CSharpEssentials Result<T> into ASP.NET Core — GlobalExceptionHandler maps unhandled exceptions to ProblemDetails, ResultEndpointFilter converts Result<T> returns to HTTP responses, and ConfigureSwaggerOptions adds per-version Swagger docs.
---

# CSharpEssentials.AspNetCore

ASP.NET Core integration for functional patterns: error-to-ProblemDetails mapping and automatic Result<T>-to-HTTP conversion.

## Installation

```bash
dotnet add package CSharpEssentials.AspNetCore
```

## Namespace

```csharp
using CSharpEssentials.AspNetCore;
```

---

## GlobalExceptionHandler + ProblemDetails

`AddEnhancedProblemDetails` configures one ProblemDetails pipeline (RFC 9457) shared by `ToProblemResult` (Minimal API), `ToActionResult` (MVC), `GlobalExceptionHandler`, status code pages and framework 404/405 responses.

```csharp
// Program.cs
builder.Services.AddEnhancedProblemDetails(o =>
{
    o.ExposeExceptionDetails = builder.Environment.IsDevelopment(); // default false
    // o.UseLegacyDefaults();  // restore the 3.x output
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

app.UseEnhancedProblemDetails(); // UseExceptionHandler() + UseStatusCodePages()
```

4.0 defaults: `traceId` is the 32-hex W3C trace id; `requestId`, `user`, `spanId`/`parentSpanId` and `errorMessages` are omitted; `instance` is `"/path"`; `type` uses RFC 9110 URIs; `errors` holds only `ErrorType.Validation` errors as `{ code, description }`. Options: `TraceId`, `IncludeRequestId`, `IncludeUser`, `IncludeSpanIds`, `Instance`, `ErrorFields` (`ProblemErrorFields` flags), `ValidationErrorsFormat`, `TypeUriResolver`, `ExposeExceptionDetails`.

```csharp
// ErrorType → HTTP status (default IErrorStatusCodeMapper):
// Validation → 400, Unauthorized → 401, Forbidden → 403, NotFound → 404, Conflict → 409
// Failure / Unexpected / Unknown → 500
```

Exception mapping (`DefaultExceptionProblemMapper`): `OperationCanceledException` → 499, `BadHttpRequestException` → its status, `EnhancedValidationException` → 400, `DomainException` → status of its `ErrorType`, anything else → 500 (message logged, not returned).

Extension points:

```csharp
builder.Services.AddProblemDetailsEnricher<TenantEnricher>();         // IProblemDetailsEnricher
builder.Services.AddErrorStatusCodeMapper<AuthAwareStatusMapper>();   // IErrorStatusCodeMapper (replaces default)
builder.Services.AddExceptionProblemMapper<PaymentExceptionMapper>(); // IExceptionProblemMapper (tried before default)
```

---

## ResultEndpointFilter

Converts `Result<T>` returns from minimal API handlers into HTTP responses automatically.

```csharp
// Apply to a group
app.MapGroup("/api").AddEndpointFilter<ResultEndpointFilter>();

// Handler just returns Result<T>
app.MapGet("/users/{id}", async (Guid id, UserService svc) =>
    await svc.GetUserAsync(id));   // returns Result<User>

// IsSuccess  → 200 OK with JSON body
// IsFailure  → ProblemDetails response (4.0; 3.x returned 400 with raw Error[])
```

A registered `IResultErrorMapper` takes precedence over the ProblemDetails response:

```csharp
public class MyErrorMapper : IResultErrorMapper
{
    public int MapToStatusCode(ErrorType errorType) => errorType switch
    {
        ErrorType.NotFound   => 404,
        ErrorType.Validation => 422,
        _                    => 500
    };
}

builder.Services.AddSingleton<IResultErrorMapper, MyErrorMapper>();
```

---

## Enum Query/Route Binding

```csharp
builder.Services.AddEnumBinding();   // optional: EnumBindingOptions (CanBind, NamingPolicy, AllowIntegerValues)
app.UseEnumBinding();                // after routing selected the endpoint
```

`[StringEnum]` enums in query/route values (Minimal API incl. `[AsParameters]`, and MVC) accept the snake_case name, the C# member name (case-insensitive) or the number of a defined member. Invalid values return a 400 ProblemDetails response. Swagger enum schemas use the same JSON names.

---

## API Versioning + Swagger

```csharp
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.ReportApiVersions = true;
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    foreach (var desc in app.DescribeApiVersions())
        options.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json", desc.GroupName);
});
```

---

## Best Practices

- Call `AddEnhancedProblemDetails()` (not plain `AddProblemDetails()`) and `app.UseEnhancedProblemDetails()`
- `ToProblemResult` / `ToActionResult` return `EnhancedProblemHttpResult` / `EnhancedProblemObjectResult`, not `ProblemHttpResult` / `BadRequestObjectResult`
- Apply `ResultEndpointFilter` at the group level, not per-endpoint
- `error.Description` is the field name — not `error.Message`
