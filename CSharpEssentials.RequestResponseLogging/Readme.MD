# CSharpEssentials.RequestResponseLogging

ASP.NET Core middleware that logs each HTTP request and response: bodies, selected headers, path, method, query string, timing and sizes. Use it when you need the payloads in your logs, for example while debugging an integration or auditing an internal API.

Benchmarks against the framework's HttpLogging and Serilog.AspNetCore (request and response bodies of 1 KB to 1 MB, a metadata-only event per request) are in [BENCHMARKS.md](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.RequestResponseLogging/BENCHMARKS.md).

## Installation

```bash
dotnet add package CSharpEssentials.RequestResponseLogging
```

Targets net8.0 to net11.0 and is Native AOT compatible. The middleware is added on the application builder; there is no service registration.

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials.RequestResponseLogging
using CSharpEssentials.RequestResponseLogging;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
WebApplication app = builder.Build();

app.UseRouting();
app.AddRequestResponseLogging(opt =>
{
    opt.UseLogger(app.Services.GetRequiredService<ILoggerFactory>(), LoggingOptions.CreateAllFields());
    opt.IgnorePaths("/health");
});

app.MapPost("/echo", (Message message) => message);
app.MapGet("/health", () => "ok");

app.Run();

public sealed record Message(string Text);
```

`POST /echo` with `{"text":"hi"}` writes one log entry under the category `RequestResponseLogger`:

```text
info: RequestResponseLogger[0]
      Request: {"text":"hi"}
      Response: {"text":"hi"}
      HostName: localhost:5000
      Path: /echo
      Method: POST
      QueryString:
      Headers: []
      ResponseTiming: 00:00.012
      RequestLength: 13
      ResponseLength: 13
```

## Registration

`app.AddRequestResponseLogging(Action<RequestResponseOptions>)` adds the middleware. `RequestResponseOptions` methods:

| Method | What it does |
|---|---|
| `UseLogger(ILoggerFactory, LoggingOptions)` | Writes each entry to an `ILogger` with the given options |
| `UseLogger(ILoggerFactory, Action<LoggingOptions>)` | Same, configuring a new `LoggingOptions` |
| `UseHandler(Func<RequestResponseContext, Task>)` | Passes each entry to your code |
| `IgnorePaths(params string[])` | Paths that are not logged at all |

Without `UseLogger` or `UseHandler` the middleware runs but writes nothing. When both are set, both run.

Call it after `UseRouting()` if you call `UseRouting()` yourself. The skip attributes are read from the endpoint that routing selected; before routing there is no endpoint, so the attributes are ignored and the bodies are logged. A `WebApplication` without an explicit `UseRouting()` adds routing first, so there the order does not matter.

## Choosing what is logged

```csharp
app.AddRequestResponseLogging(opt =>
    opt.UseLogger(app.Services.GetRequiredService<ILoggerFactory>(), logging =>
    {
        logging.LoggingFields = [LogFields.Method, LogFields.Path, LogFields.ResponseTiming, LogFields.Headers];
        logging.HeaderKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "X-Correlation-Id" };
        logging.LoggingLevel = LogLevel.Debug;
        logging.LoggerCategoryName = "Http";
    }));
```

`LoggingOptions`:

| Property | Default | Meaning |
|---|---|---|
| `LoggingFields` | empty | The `LogFields` to write. `LoggingOptions.CreateAllFields()` selects all ten |
| `HeaderKeys` | empty | Request headers written by the `Headers` field |
| `LoggingLevel` | `Information` | Log level of each entry |
| `LoggerCategoryName` | `RequestResponseLogger` | Logger category |
| `UseSeparateContext` | `true` | `true` writes one named placeholder per field (`{Path}`, `{Method}`), so structured logging providers get each value as a property. `false` writes one formatted string |

`LogFields`: `Request`, `Response`, `HostName`, `Path`, `Method`, `QueryString`, `Headers`, `ResponseTiming` (`mm:ss.fff`), `RequestLength`, `ResponseLength`.

- `LoggingFields` is read once, when `AddRequestResponseLogging` runs. Set it before that call.
- With no `HeaderKeys` the `Headers` field is `[]`, so `Authorization` and cookies are never logged by accident. With keys it lists the matching headers: `[{"Accept-Language":"tr"}]`.
- Header names are compared with the default `HashSet<string>` comparer, which is case-sensitive, against the name the server received. Kestrel normalizes standard names (`accept-language` matches `Accept-Language`), but custom headers arrive as sent, and HTTP/2 clients send them in lower case. Assign a `HashSet` with `StringComparer.OrdinalIgnoreCase`, as above, to ignore case.
- `RequestLength` and `ResponseLength` count the characters of the logged body text, not bytes. The placeholder texts count too, so a request without a body reports 18, the length of `Empty request body`.
- Without a handler, the request body is read only when `LoggingFields` has `Request` or `RequestLength`, and the response body only when it has `Response` or `ResponseLength`. With a handler both are always captured.

## Ignoring paths

```csharp
opt.IgnorePaths("/health", "/metrics");
```

Matching is by prefix, case-insensitive, ignoring a trailing `/`. `"/health"` also skips `/health/live` and `/healthz`. `"/"` skips only the root path. `null` and empty entries are ignored. A second call replaces the first list.

## Skipping bodies per endpoint

| Attribute | Effect |
|---|---|
| `[SkipRequestLogging]` | The request body is logged as `Skipped logging request body` |
| `[SkipResponseLogging]` | The response body is logged as `Skipped logging response body` |
| `[SkipRequestResponseLogging]` | Both |

The rest of the entry (path, method, timing) is still written. The attributes go on a controller or an action. Minimal API endpoints add them as metadata:

```csharp
[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    [HttpPost("login")]
    [SkipRequestLogging]
    public IActionResult Login(LoginRequest request) => Ok();
}

public sealed record LoginRequest(string User, string Password);
```

```csharp
app.MapGet("/export", () => Results.Ok()).WithMetadata(new SkipResponseLoggingAttribute());
```

## Custom handler

`UseHandler` receives a `RequestResponseContext` after the rest of the pipeline has finished or thrown:

| Member | Type | Value |
|---|---|---|
| `Url` | `string` | Full request URL |
| `RequestBody` | `string?` | Captured request body or a placeholder text |
| `ResponseBody` | `string?` | Captured response body, a placeholder, or the exception text |
| `ResponseCreationTime` | `TimeSpan?` | Time spent in the rest of the pipeline |
| `ResponseTime` | `string` | `ResponseCreationTime` as `mm:ss.fff` |
| `RequestLength` | `int?` | Characters in `RequestBody`, `null` when it is empty |
| `ResponseLength` | `int?` | Characters in `ResponseBody` |

```csharp Program.cs
// dotnet add package CSharpEssentials.RequestResponseLogging
using CSharpEssentials.RequestResponseLogging;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
WebApplication app = builder.Build();
ILogger logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Http");

app.UseRouting();
app.AddRequestResponseLogging(opt => opt.UseHandler(context =>
{
    string? body = context.ResponseBody is { Length: > 200 } text ? text[..200] + "..." : context.ResponseBody;
    logger.LogInformation("{Url} took {Time}: {Body}", context.Url, context.ResponseTime, body);
    return Task.CompletedTask;
}));

app.MapGet("/", () => "hello");
app.Run();
```

## Behavior notes

- Placeholder texts: `Empty request body`, `Empty response body`, `Skipped logging request body`, `Skipped logging response body`.
- A request with a `Content-Length` over 10 MB is not read and is logged as `Request body too large: {n} bytes`. A chunked request body is read up to 10 MB, then logged as `Request body too large: over 10485760 bytes`. The endpoint still receives the whole body.
- Response writes go to the client right away and are copied into memory for the log, so streaming still works. The full copy is kept until the request ends, so put `[SkipResponseLogging]` on endpoints with large responses.
- The first write to `Response.Body` starts the response. Set the status code, headers and cookies before it; changing them afterwards throws `InvalidOperationException`.
- Synchronous writes to `Response.Body` fail when the server forbids synchronous IO (the Kestrel and IIS default). Write asynchronously.
- If the pipeline throws, the entry has the captured request body and `Exception: {type}, Message: {message}` as the response body. It goes to the logger and the handler, then the exception is rethrown. If the handler itself fails on that path, a second entry with `Error writing log: {message}` is written and the original exception still reaches the caller.
- If writing the response to the client fails (the client disconnected), the exception surfaces in the endpoint, often as `IOException` or `OperationCanceledException`.
- When the request body is not read (no handler and no `Request`/`RequestLength` field), `Request.Body` stays as the server gave it and is not seekable. Call `Request.EnableBuffering()` yourself if later code rewinds it.
- Bodies can contain passwords, tokens and personal data. Log them only where that is acceptable, and use `IgnorePaths` and the skip attributes on sensitive endpoints.

## Upgrading from 7.x

- The response is no longer buffered until the endpoint returns. See the notes above about headers after the first write, synchronous IO and client disconnects.
- On an exception the handler now runs before the rethrow, and `RequestBody` holds the captured body instead of `Error occurred during request processing`. Check `ResponseBody` for the `Exception:` prefix.
- `IgnorePaths("/")` now skips only the root path.
- `LoggingFields` is read once at registration.
- `Microsoft.IO.RecyclableMemoryStream` is no longer exposed to your code through this package. Reference it directly if you use it.

## See also

- Agent skill: [csharpessentials-logging](https://github.com/senrecep/CSharpEssentials/blob/main/.well-known/agent-skills/csharpessentials-logging/SKILL.md)
- Example: [examples/Examples.RequestResponseLogging](https://github.com/senrecep/CSharpEssentials/tree/main/examples/Examples.RequestResponseLogging)
- [CSharpEssentials.AspNetCore](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore/Readme.MD) for problem responses that this middleware logs
