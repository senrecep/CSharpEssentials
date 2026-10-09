---
name: csharpessentials-logging
description: Use when logging HTTP requests and responses in ASP.NET Core. Covers app.AddRequestResponseLogging(opt => …) with UseLogger(ILoggerFactory, LoggingOptions), UseHandler, IgnorePaths, LogFields selection, and [SkipRequestLogging] / [SkipResponseLogging] / [SkipRequestResponseLogging] endpoint metadata for per-endpoint opt-out.
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
- A request with a `Content-Length` over 10 MB is not read; `Request body too large: {n} bytes` is logged instead. An empty body is logged as `Empty request body`.
- The response body is buffered in memory until the request completes, so use `[SkipResponseLogging]` on streaming or very large responses.
- If the pipeline throws, the middleware logs `Error occurred during request processing` with the exception type and message, then rethrows.
- `UseHandler` runs in addition to `UseLogger` when both are set.

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

`LogFields`: `Request`, `Response`, `HostName`, `Path`, `Method`, `QueryString`, `Headers`, `ResponseTiming`, `RequestLength`, `ResponseLength`. `LoggingOptions.CreateAllFields()` selects all of them; the default list is empty.

- The `Headers` field writes only the request headers in `HeaderKeys` (`[{"X-Correlation-Id":"..."}]`, `[]` when none is present); with no keys it is empty, so credentials are not logged by default. Names are compared case-sensitively with the spelling received: standard names are normalized by Kestrel, a custom header must match as sent (HTTP/2 sends lower-case). Use `logging.HeaderKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "X-Correlation-Id" }` to ignore the case.
- `UseSeparateContext` (default `true`) writes one named placeholder per field (`{Path}`, `{Method}`) for structured logging providers; `false` writes one formatted text.
- `LoggingLevel` defaults to `Information` and `LoggerCategoryName` to `RequestResponseLogger`.
- `RequestLength` and `ResponseLength` count characters of the logged text, not bytes; a request without a body reports the 18 characters of `Empty request body`.

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
