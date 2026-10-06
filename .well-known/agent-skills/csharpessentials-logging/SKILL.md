---
name: csharpessentials-logging
description: Use when logging HTTP requests and responses in ASP.NET Core — app.AddRequestResponseLogging(opt => …) with UseLogger(ILoggerFactory, LoggingOptions), UseHandler, IgnorePaths, LogFields selection, and [SkipRequestLogging] / [SkipResponseLogging] / [SkipRequestResponseLogging] endpoint metadata for per-endpoint opt-out.
---

# CSharpEssentials.RequestResponseLogging

Middleware that logs HTTP requests and responses (bodies, headers, path, method, timing, sizes). Configured on `IApplicationBuilder`; no service registration is needed.

## Installation

```bash
dotnet add package CSharpEssentials.RequestResponseLogging
```

## Namespace

```csharp
using CSharpEssentials.RequestResponseLogging;
```

---

## Register

`AddRequestResponseLogging` is an `IApplicationBuilder` extension that adds the middleware. Call it on the built app.

```csharp
var app = builder.Build();

app.AddRequestResponseLogging(opt =>
{
    opt.UseLogger(app.Services.GetRequiredService<ILoggerFactory>(), LoggingOptions.CreateAllFields());
    opt.IgnorePaths("/health", "/metrics");
});
```

- Without `UseLogger` or `UseHandler`, the middleware runs but writes nothing.
- `IgnorePaths` matches by prefix, case-insensitive. A later call replaces the earlier list.
- Request bodies larger than 10 MB are not captured; a size note is logged instead.

---

## Choosing Fields

```csharp
app.AddRequestResponseLogging(opt =>
    opt.UseLogger(app.Services.GetRequiredService<ILoggerFactory>(), logging =>
    {
        logging.LoggingFields = [LogFields.Method, LogFields.Path, LogFields.ResponseTiming];
        logging.HeaderKeys = ["X-Correlation-Id"];
        logging.LoggingLevel = LogLevel.Debug;
        logging.LoggerCategoryName = "Http";
    }));
```

`LogFields`: `Request`, `Response`, `HostName`, `Path`, `Method`, `QueryString`, `Headers`, `ResponseTiming`, `RequestLength`, `ResponseLength`. `LoggingOptions.CreateAllFields()` selects all of them.

---

## Custom Handler

```csharp
app.AddRequestResponseLogging(opt => opt.UseHandler(context =>
{
    Console.WriteLine($"{context.Url} {context.ResponseTime} {context.RequestLength} {context.ResponseLength}");
    return Task.CompletedTask;
}));
```

`RequestResponseContext` exposes `RequestBody`, `ResponseBody`, `ResponseCreationTime`, `ResponseTime`, `RequestLength`, `ResponseLength` and `Url`.

---

## Per-Endpoint Opt-Out

The middleware reads the attributes from endpoint metadata:

| Attribute | Effect |
|---|---|
| `[SkipRequestLogging]` | Request body is not captured |
| `[SkipResponseLogging]` | Response body is not captured |
| `[SkipRequestResponseLogging]` | Neither body is captured |

```csharp
[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    [HttpPost("login")]
    [SkipRequestLogging] // passwords
    public IActionResult Login(LoginRequest request) => Ok();
}

// Minimal APIs
app.MapGet("/export", () => Results.Ok()).WithMetadata(new SkipResponseLoggingAttribute());
```

---

## Best Practices

- Skip request bodies on endpoints that receive passwords, tokens or other PII.
- Add health check and metrics paths to `IgnorePaths`; they are high-frequency and low-value.
- If you call `UseRouting()` explicitly, add the middleware after it, so endpoint metadata (and the skip attributes) is available.
