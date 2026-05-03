# CSharpEssentials.RequestResponseLogging Example

This Web API project demonstrates how to capture and log every HTTP request and response using `CSharpEssentials.RequestResponseLogging`.

## Features Demonstrated

| Feature | File | Description |
|---------|------|-------------|
| **Request/Response Logging** | `Program.cs` | Captures full HTTP cycle including body, headers, and timing |
| **Custom Log Writer** | `Infrastructure/StructuredJsonLogWriter.cs` | Formats logs as structured JSON with sensitive data masking |
| **Path Filtering** | `Program.cs` | Excludes health check endpoints from logging |
| **Body Truncation** | `StructuredJsonLogWriter.cs` | Prevents memory issues with large payloads |
| **Sensitive Header Masking** | `StructuredJsonLogWriter.cs` | Masks `Authorization`, `Cookie`, `X-Api-Key` |

## Running the Project

```bash
cd examples/Examples.RequestResponseLogging
dotnet run
```

The API will start on `https://localhost:5001`.

## Test Endpoints

Use `curl` or any HTTP client to test the endpoints and observe the log output in the console.

### 1. Simple GET (logged as Information)

```bash
curl -s https://localhost:5001/api/demo/hello
```

### 2. POST with Body (request body is logged)

```bash
curl -s -X POST https://localhost:5001/api/demo/echo \
  -H "Content-Type: application/json" \
  -d '{"message":"Hello","repeatCount":3}'
```

### 3. Server Error (logged as Error)

```bash
curl -s https://localhost:5001/api/demo/error
```

### 4. Validation Failure (logged as Warning)

```bash
curl -s -X POST https://localhost:5001/api/demo/validation \
  -H "Content-Type: application/json" \
  -d '{"name":"","age":10}'
```

### 5. Slow Request (logs duration > 1.5s)

```bash
curl -s https://localhost:5001/api/demo/slow
```

### 6. Health Check (excluded from logging by PathFilter)

```bash
curl -s https://localhost:5001/health
```

## Sample Log Output

```json
{
  "timestamp": "2025-01-15T10:30:45.123Z",
  "traceId": "0HMP3...",
  "durationMs": 12.45,
  "request": {
    "method": "POST",
    "path": "/api/demo/echo",
    "queryString": "",
    "headers": {
      "content-type": "application/json",
      "authorization": "Bear****1234"
    },
    "body": "{\"message\":\"Hello\",\"repeatCount\":3}"
  },
  "response": {
    "statusCode": 200,
    "headers": { "content-type": "application/json; charset=utf-8" },
    "body": "{\"received\":{...},\"serverTime\":\"2025-01-15T10:30:45.135Z\"}"
  }
}
```

## Configuration Options

```csharp
builder.Services.AddRequestResponseLogging(options =>
{
    options.UseLogWriter<StructuredJsonLogWriter>();

    // Toggle what gets captured
    options.LogRequestBody = true;
    options.LogResponseBody = true;
    options.LogRequestHeaders = true;
    options.LogResponseHeaders = true;

    // Exclude certain paths from logging
    options.PathFilter = path => !path.StartsWith("/health");

    // Limit body size to prevent memory issues
    options.MaxBodyLength = 1024 * 64; // 64 KB
});
```

## Custom Log Writer

The `ILogWriter` interface allows complete control over how logs are persisted:

```csharp
public interface ILogWriter
{
    Task WriteAsync(RequestResponseContext context, CancellationToken cancellationToken = default);
}
```

Implementations can write to:
- Console / Structured JSON (shown here)
- Files with rotation
- Databases (SQL Server, PostgreSQL, MongoDB)
- Message queues (RabbitMQ, Kafka, Azure Service Bus)
- Cloud logging services (AWS CloudWatch, Azure Application Insights, GCP Logging)

## Security Considerations

1. **Sensitive Data Masking**: Always mask `Authorization`, `Cookie`, and API key headers.
2. **Body Size Limits**: Set `MaxBodyLength` to prevent out-of-memory errors with file uploads.
3. **Path Filtering**: Exclude health checks, metrics, and static file endpoints.
4. **PII Scrubbing**: Extend the log writer to scan request/response bodies for PII.
