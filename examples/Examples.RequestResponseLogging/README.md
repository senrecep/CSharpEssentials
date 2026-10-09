# CSharpEssentials.RequestResponseLogging Example

This Web API project demonstrates how to capture and log HTTP requests and responses using `CSharpEssentials.RequestResponseLogging`.

## Features Demonstrated

| Feature | File | Description |
|---------|------|-------------|
| **Request/Response Logging** | `Program.cs` | `AddRequestResponseLogging` with `LoggingOptions.CreateAllFields()`: request and response bodies, host, path, method, query string, headers, timing and lengths |
| **Header Selection** | `Program.cs` | Only the headers added to `HeaderKeys` (here `Accept-Language`) are logged |
| **Path Filtering** | `Program.cs` | `IgnorePaths("/health")` keeps the health check out of the log |
| **Skip Attributes** | `Controllers/DemoController.cs` | `[SkipRequestLogging]`, `[SkipResponseLogging]` and `[SkipRequestResponseLogging]` on three demo endpoints (not effective with the middleware order of `Program.cs`, see Skip Attributes below) |
| **Custom Handler Helper** | `Infrastructure/StructuredJsonLogWriter.cs` | Formats a `RequestResponseContext` as compact JSON and truncates bodies to 1000 characters. `Program.cs` does not call it; wire it with `UseHandler` (see below) |

## Running the Project

```bash
cd examples/Examples.RequestResponseLogging
dotnet run
```

The API listens on `http://localhost:5000` (Kestrel's default; the project has no launch profile). The log lines appear in the console under the category `RequestResponseLogger`, at `Information` level.

## Test Endpoints

### 1. Simple GET

```bash
curl -s http://localhost:5000/api/demo/hello
```

### 2. POST with Body (request body and the `Accept-Language` header are logged)

```bash
curl -s -X POST http://localhost:5000/api/demo/echo \
  -H "Content-Type: application/json" \
  -H "Accept-Language: tr" \
  -d '{"message":"Hello","repeatCount":3}'
```

### 3. Server Error

```bash
curl -s http://localhost:5000/api/demo/error
```

The controller throws, so the middleware logs `Request: Error occurred during request processing` and the exception type and message in `Response`, and rethrows. The response is a bare 500, because this project registers no exception handler.

### 4. Not Found and Validation Failure

```bash
curl -s http://localhost:5000/api/demo/notfound

curl -s -X POST http://localhost:5000/api/demo/validation \
  -H "Content-Type: application/json" \
  -d '{"name":"","age":10}'
```

Both return a ProblemDetails body (404 and 400), which is logged as the response body. Every request is logged at the configured level (`Information`); the status code is not a logged field.

### 5. Slow Request

```bash
curl -s http://localhost:5000/api/demo/slow
```

The endpoint waits 1.5 seconds, so `ResponseTiming` shows about `00:01.500`.

### 6. Skip Attributes

```bash
curl -s http://localhost:5000/api/demo/skip-request
curl -s http://localhost:5000/api/demo/skip-response
curl -s http://localhost:5000/api/demo/skip-all
```

These endpoints carry `[SkipRequestLogging]`, `[SkipResponseLogging]` and `[SkipRequestResponseLogging]`, which replace the body with `Skipped logging request body` / `Skipped logging response body`. In this project as shipped they have no effect: the middleware reads the attributes from the endpoint that routing selected, and `Program.cs` registers `AddRequestResponseLogging` before `app.UseRouting()`, so the middleware sees no endpoint and logs the bodies of all three. Moving `app.UseRouting()` above `app.AddRequestResponseLogging(...)` makes the attributes work.

### 7. Health Check (excluded by IgnorePaths)

```bash
curl -s http://localhost:5000/health
```

Nothing is logged. `IgnorePaths` matches by prefix, so `/health/live` would be skipped too.

## Sample Log Output

`POST /api/demo/echo` with the request above:

```text
info: RequestResponseLogger[0]
      Request: {"message":"Hello","repeatCount":3}
      Response: {"received":{"message":"Hello","repeatCount":3},"serverTime":"2026-10-09T21:11:10.1774151Z"}
      HostName: localhost:5000
      Path: /api/demo/echo
      Method: POST
      QueryString:
      Headers: [{"Accept-Language":"tr"}]
      ResponseTiming: 00:00.024
      RequestLength: 35
      ResponseLength: 92
```

`RequestLength` and `ResponseLength` count characters of the logged body text. A request without a body reports the length of the text `Empty request body` (18).

## Configuration Options

```csharp
app.AddRequestResponseLogging(opt =>
{
    opt.IgnorePaths("/health");
    var loggingOptions = LoggingOptions.CreateAllFields();
    loggingOptions.HeaderKeys.Add(HeaderNames.AcceptLanguage);
    opt.UseLogger(app.Services.GetRequiredService<ILoggerFactory>(), loggingOptions);
});
```

Place `UseRouting()` before `AddRequestResponseLogging` in your own application so the skip attributes work (see above). `LoggingOptions` also has `LoggingLevel`, `LoggingFields`, `LoggerCategoryName` and `UseSeparateContext`; see the [package README](../../CSharpEssentials.RequestResponseLogging/Readme.MD).

## Custom Handler

`ILogWriter` is internal. For a custom implementation, use `UseHandler(Func<RequestResponseContext, Task> handler)`; it runs in addition to `UseLogger` when both are set. To write the JSON of `StructuredJsonLogWriter`:

```csharp
ILogger logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Http");

app.AddRequestResponseLogging(opt =>
{
    opt.IgnorePaths("/health");
    opt.UseHandler(context => StructuredJsonLogWriter.WriteAsync(context, logger));
});
```

## Security Considerations

1. **Headers**: Only the keys in `HeaderKeys` are logged, so leave `Authorization`, `Cookie` and API key headers out. The keys are compared case-sensitively against the names as the server received them; use `new HashSet<string>(StringComparer.OrdinalIgnoreCase)` for `HeaderKeys` when clients may send the name in another case (HTTP/2 sends lower-case names).
2. **Bodies**: Request and response bodies are logged as they are. Mark endpoints that carry passwords or personal data with `[SkipRequestLogging]`, `[SkipResponseLogging]` or `[SkipRequestResponseLogging]`, and truncate or scrub bodies in a `UseHandler` callback.
3. **Body Size**: A request with a `Content-Length` over 10 MB is not read (`Request body too large`). The response body is buffered in memory, so use `[SkipResponseLogging]` on large or streaming responses.
4. **Path Filtering**: Exclude health checks, metrics and static file endpoints.
