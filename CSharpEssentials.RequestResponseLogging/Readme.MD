# CSharpEssentials.RequestResponseLogging

Configurable middleware for logging HTTP requests and responses in ASP.NET Core.

## Features

- **Detailed Logging**: Request/Response bodies, headers, path, method, timing, sizes.
- **Configurable Fields**: Choose exactly what to log via `LoggingOptions`.
- **Path Filtering**: Ignore specific paths (e.g., health checks).
- **Attributes**: `[SkipRequestLogging]`, `[SkipResponseLogging]`, `[SkipRequestResponseLogging]` for fine-grained control.
- **Custom Handler**: `UseHandler` hands every `RequestResponseContext` to your own code.

## Installation

```bash
dotnet add package CSharpEssentials.RequestResponseLogging
```

## Usage

### Registration

```csharp
using CSharpEssentials.RequestResponseLogging;

var app = builder.Build();

app.UseRouting();
app.AddRequestResponseLogging(opt =>
{
    opt.UseLogger(app.Services.GetRequiredService<ILoggerFactory>(), LoggingOptions.CreateAllFields());
    opt.IgnorePaths("/health", "/metrics");
});
app.MapControllers();
```

Without `UseLogger` (or `UseHandler`), the middleware runs but writes nothing.

Register it after `UseRouting()` when you call `UseRouting()` yourself. The skip attributes are read from the endpoint selected by routing, and a middleware that runs before routing sees no endpoint, so the attributes have no effect and the bodies are logged. (A `WebApplication` without an explicit `UseRouting()` adds routing at the start of the pipeline, so there it works anywhere.)

`IgnorePaths` matches by prefix, ignoring case and a trailing `/`: `"/health"` also skips `/health/live` and `/healthz`. Calling it again replaces the earlier list.

The response body is held in memory until the request finishes, then copied to the client, so the response is not streamed. Use `[SkipResponseLogging]` on streaming endpoints (server-sent events, large downloads). A request with a `Content-Length` over 10 MB is not read and is logged as `Request body too large: {n} bytes`; an empty body is logged as `Empty request body`, an empty response as `Empty response body`. If the request pipeline throws, the middleware logs `Error occurred during request processing` with the exception type and message in the response field, and rethrows.

The level, fields and format come from `LoggingOptions`:

| Property | Default | Notes |
|---|---|---|
| `LoggingFields` | none | `Request`, `Response`, `HostName`, `Path`, `Method`, `QueryString`, `Headers`, `ResponseTiming` (`mm:ss.fff`), `RequestLength`, `ResponseLength`. `LoggingOptions.CreateAllFields()` selects all of them |
| `HeaderKeys` | none | The request headers written by the `Headers` field (`HeaderKeys.Add("Accept-Language")`). With no keys the field is empty, so credentials are not logged by default; with keys it is a list of the matching headers (`[{"Accept-Language":"tr"}]`, `[]` when none is on the request). Names are compared case-sensitively with the spelling the server received: Kestrel normalizes the standard names (`accept-language` matches `Accept-Language`), but a custom header must match as sent (`X-Correlation-Id` does not match `x-correlation-id`, which is how HTTP/2 clients send it). Assign `new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "X-Correlation-Id" }` to ignore the case |
| `LoggingLevel` | `Information` | |
| `LoggerCategoryName` | `RequestResponseLogger` | |
| `UseSeparateContext` | `true` | `true` writes one named placeholder per field (`{Path}`, `{Method}`, ...) so structured logging providers get each value as a property; `false` writes one formatted text |

`RequestLength` and `ResponseLength` are the number of characters of the logged body text, not bytes and not the `Content-Length`. The texts above count too, so a request without a body reports the 18 characters of `Empty request body`.

Bodies can contain personal data and secrets; log them only where that is acceptable, ignore sensitive paths and mark endpoints with the skip attributes.

### Per-Endpoint Control

```csharp
using CSharpEssentials.RequestResponseLogging;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    [HttpPost("login")]
    [SkipRequestLogging] // don't log the request body (passwords)
    public IActionResult Login(LoginRequest request) => Ok();

    [HttpGet("large-data")]
    [SkipResponseLogging] // don't log the response body
    public IActionResult GetData() => Ok();
}
```

`[SkipRequestResponseLogging]` skips both. The attributes are read from endpoint metadata, so Minimal APIs use `.WithMetadata(new SkipRequestLoggingAttribute())`. A skipped body is logged as `Skipped logging request body` / `Skipped logging response body`.

### Custom Fields

```csharp
app.AddRequestResponseLogging(opt =>
    opt.UseLogger(app.Services.GetRequiredService<ILoggerFactory>(), logging =>
    {
        logging.LoggingFields = [LogFields.Method, LogFields.Path, LogFields.ResponseTiming];
        logging.LoggingLevel = LogLevel.Debug;
    }));
```

### Custom Handler

`UseHandler` receives the `RequestResponseContext` of every logged request after the rest of the pipeline has completed. It runs in addition to the logger when both are set. The context exposes `Url`, `RequestBody`, `ResponseBody`, `ResponseCreationTime` (a `TimeSpan`), `ResponseTime` (`mm:ss.fff` text), `RequestLength` and `ResponseLength`.

```csharp
var logger = app.Services.GetRequiredService<ILogger<Program>>();

app.AddRequestResponseLogging(opt =>
{
    opt.IgnorePaths("/health");
    opt.UseHandler(context =>
    {
        logger.LogInformation("{Url} took {Time}, response {Length} characters", context.Url, context.ResponseTime, context.ResponseLength);
        return Task.CompletedTask;
    });
});
```
