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

Catches unhandled exceptions and converts them to RFC 9457 ProblemDetails responses using `ErrorType → HTTP status` mapping.

```csharp
// Program.cs
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

app.UseExceptionHandler();

// ErrorType → HTTP status mapping:
// Validation    → 400
// Unauthorized  → 401
// Forbidden     → 403
// NotFound      → 404
// Conflict      → 409
// Failure       → 422
// Unexpected    → 500
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
// IsFailure  → ProblemDetails with status from ErrorType
```

Custom error mapping:

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

- Register `GlobalExceptionHandler` before `AddProblemDetails`
- Apply `ResultEndpointFilter` at the group level, not per-endpoint
- `error.Description` is the field name — not `error.Message`
