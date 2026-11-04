# CSharpEssentials.RequestResponseLogging

Configurable middleware for logging HTTP requests and responses in ASP.NET Core.

> Benchmarks against the framework's HttpLogging and Serilog.AspNetCore (request and response bodies of 1 KB to 1 MB,
> a metadata-only event per request) are in
> [BENCHMARKS.md](./BENCHMARKS.md). That page is a draft: results are pending the full run.

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

## Upgrading to 8.0

- When the request pipeline throws, the handler set with `UseHandler` now runs before the exception is rethrown, so it sees failed requests too. The logged `RequestBody` holds the captured request body instead of `Error occurred during request processing`; the exception type and message are still in `ResponseBody`. If your handler assumed every call was a completed request, check `ResponseBody` for the `Exception:` prefix.
- The response is no longer buffered until the endpoint finishes. Each write goes to the client right away and is copied into the log, so streamed responses (server-sent events, large downloads, flushed chunks) reach the client as they are written. This changes four things:
  - The first write or flush to `Response.Body` starts the response. Set the status code, headers and cookies before it. Changing them afterwards now throws `InvalidOperationException` (headers are read-only once the response has started), and `Response.OnStarting` callbacks run at the first write instead of when the endpoint returns. Before 8.0, `await context.Response.WriteAsync("x"); context.Response.StatusCode = 201;` worked; now the second line throws.
  - If writing to the client fails (for example the client disconnected), the exception is thrown inside the endpoint and reaches the caller. A single entry records it as `Exception: {type}, Message: {message}` and goes to the logger and the `UseHandler` handler. Before 8.0 a single entry showed `Error capturing response body: {message}`. Handle or filter the exception (often an `IOException` or `OperationCanceledException`) where you handle aborted requests.
  - Once the endpoint has written part of the response, an exception thrown later can no longer be turned into a fresh error response by an outer exception handler, because the status code and headers are already sent. This is the same as running without the middleware.
  - Synchronous writes and flushes to `Response.Body` (for example disposing a `StreamWriter` over it) now reach the server directly, so they fail when the server does not allow synchronous IO (the Kestrel and IIS default). Before, the middleware's memory buffer accepted them. Write asynchronously, or set `AllowSynchronousIO`.
- `IgnorePaths("/")` now skips only the root path. Before, it skipped every request except the root, because `"/"` trimmed to an empty prefix that every path starts with. A `null` or empty entry is now skipped instead of throwing or matching everything. To skip all requests, leave the middleware out.
- `LoggingFields` is now read once, when `AddRequestResponseLogging` runs. Adding or removing fields on the `LoggingOptions` instance after that has no effect. Before, the logger read the list on every request. Set the fields before you register the middleware.
- Without a `UseHandler` handler, the middleware now reads the request body only when `LoggingFields` contains `Request` or `RequestLength`, and the response body only when it contains `Response` or `ResponseLength`. What gets logged is the same. When the request body is not read, `Request.Body` is left as the server gave it, so it is no longer seekable for later middleware or endpoints. If your code rewinds the request body, call `Request.EnableBuffering()` yourself. With a handler, both bodies are still captured on every request.
- `Microsoft.IO.RecyclableMemoryStream` is still installed with the package and used at runtime, but your code can no longer compile against it through this package. If you use `RecyclableMemoryStreamManager` or other `Microsoft.IO` types yourself, add `Microsoft.IO.RecyclableMemoryStream` to your project.

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

`IgnorePaths` matches by prefix, ignoring case and a trailing `/`: `"/health"` also skips `/health/live` and `/healthz`. `"/"` skips only the root path, and `null` or empty entries are skipped. Calling it again replaces the earlier list.

Each response write goes to the client right away and is also copied into memory for the log, so streamed responses still stream; the whole response body is held in memory until the request finishes. Use `[SkipResponseLogging]` (or, without a handler, leave `Response` and `ResponseLength` out of `LoggingFields`) on endpoints with very large responses. A request with a `Content-Length` over 10 MB is not read and is logged as `Request body too large: {n} bytes`; a body without a `Content-Length` (chunked) is read up to 10 MB, then logged as `Request body too large: over 10485760 bytes`, and the endpoint still receives the whole body; an empty body is logged as `Empty request body`, an empty response as `Empty response body`. If writing the response to the client fails, the exception is thrown inside the endpoint and handled like any other pipeline exception below. If the request pipeline throws, the middleware logs the captured request body with the exception type and message in the response field (`Exception: {type}, Message: {message}`), passes the same context to the `UseHandler` handler, and rethrows. A handler failure on this path is written to the logger as a second entry with `Error writing log: {message}` in the response field, and a logger failure is ignored, so the original exception still reaches the caller.

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

`UseHandler` receives the `RequestResponseContext` of every logged request after the rest of the pipeline has completed or thrown. It runs in addition to the logger when both are set. The context exposes `Url`, `RequestBody`, `ResponseBody`, `ResponseCreationTime` (a `TimeSpan`), `ResponseTime` (`mm:ss.fff` text), `RequestLength` and `ResponseLength`.

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
