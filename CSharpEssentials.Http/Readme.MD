# CSharpEssentials.Http

`CSharpEssentials.Http` bridges `HttpClient` to the `Result` pattern. It turns HTTP calls into type-safe, functional results with automatic status-code-to-error mapping, fluent request builders, and Polly resilience integration.

## 🚀 Features

- **Result-based HTTP calls**: `GetFromJsonAsResultAsync`, `PostAsJsonAsResultAsync`, `PutAsJsonAsResultAsync`, `PatchAsJsonAsResultAsync`, `DeleteAsResultAsync`, `SendAsResultAsync`
- **Automatic error mapping**: HTTP status codes mapped to `ErrorType` (Validation, Unauthorized, Forbidden, NotFound, Conflict, Unexpected)
- **JSON integration**: Uses `EnhancedJsonSerializerOptions` from `CSharpEssentials.Json` by default
- **Null-safe deserialization**: Empty or null response bodies yield a structured `NotFound` error
- **Fluent request builder**: Chain methods to build and execute requests declaratively
- **Query string helpers**: Build URIs with query parameters from dictionaries or anonymous objects
- **Manual redirect following**: Follow 3xx redirects manually with full request cloning, configurable limits, and Result-based error handling
- **Polly resilience**: Retry, timeout, and circuit breaker pipelines ready to use

## 📦 Installation

```bash
dotnet add package CSharpEssentials.Http
```

## 🛠 Usage

### 1. GET Request as Result

```csharp
using CSharpEssentials.Http;
using CSharpEssentials.ResultPattern;

HttpClient client = new() { BaseAddress = new Uri("https://api.example.com") };

Result<User> userResult = await client.GetFromJsonAsResultAsync<User>(new Uri("/users/1", UriKind.Relative));

userResult.Match(
    onSuccess: user => Console.WriteLine(user.Name),
    onFailure: errors => Console.WriteLine(errors[0].Description)
);
```

### 2. POST, PUT, PATCH Requests as Result

```csharp
var payload = new { Name = "Alice", Age = 30 };

Result<User> postResult = await client.PostAsJsonAsResultAsync<User>(
    new Uri("/users", UriKind.Relative),
    payload);

Result<User> putResult = await client.PutAsJsonAsResultAsync<User>(
    new Uri("/users/1", UriKind.Relative),
    payload);

Result<User> patchResult = await client.PatchAsJsonAsResultAsync<User>(
    new Uri("/users/1", UriKind.Relative),
    new { Age = 31 });
```

### 3. DELETE and Raw Send as Result

```csharp
Result deleteResult = await client.DeleteAsResultAsync(new Uri("/users/1", UriKind.Relative));

using var request = new HttpRequestMessage(HttpMethod.Head, new Uri("/health", UriKind.Relative));
Result headResult = await client.SendAsResultAsync(request);
```

### 4. Status Code Mapping

```csharp
using System.Net;
using CSharpEssentials.Http;

ErrorType type = HttpStatusCodeMapper.ToErrorType(HttpStatusCode.NotFound);
// type == ErrorType.NotFound

Error error = HttpStatusCodeMapper.ToError(HttpStatusCode.BadRequest);
// error.Code == "Http.400"
```

### 5. Query String Builder

```csharp
Uri baseUri = new("https://api.example.com/search");

// Single parameter
Uri uri1 = baseUri.WithQueryString("q", "csharp");

// Multiple parameters from dictionary
var filters = new Dictionary<string, string?> { { "page", "1" }, { "limit", "10" } };
Uri uri2 = baseUri.WithQueryString(filters);

// From anonymous object
Uri uri3 = baseUri.WithQueryString(new { Sort = "desc" });
```

### 6. HttpRequestBuilder (Fluent API)

```csharp
Result<User> result = await HttpRequestBuilder
    .Get("/users/1")
    .WithHeader("Accept", "application/json")
    .WithHeader("x-api-key", "secret")
    .WithQuery("include", "profile")
    .AsResultAsync<User>(client);

Result created = await HttpRequestBuilder
    .Post("/users")
    .WithJsonContent(new { Name = "Bob" })
    .AsResultAsync(client);
```

### 7. Polly Resilience Pipelines

```csharp
using CSharpEssentials.Http;
using Polly;

// Retry + Timeout combined
var pipeline = HttpClientResilienceExtensions.CreateResiliencePipeline(
    maxRetryAttempts: 3,
    timeout: TimeSpan.FromSeconds(30),
    retryDelay: TimeSpan.FromSeconds(1));

Result<User> result = await pipeline.ExecuteAsResultAsync(async token =>
    await client.GetFromJsonAsResultAsync<User>(new Uri("/users/1"), cancellationToken: token));

// Individual pipelines
ResiliencePipeline retryOnly = HttpClientResilienceExtensions.CreateRetryPipeline(maxRetryAttempts: 3);
ResiliencePipeline timeoutOnly = HttpClientResilienceExtensions.CreateTimeoutPipeline(TimeSpan.FromSeconds(10));
```

### 8. Redirect Following

```csharp
using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("/legacy-url", UriKind.Relative));

// Manual redirect following with Result-based error handling
Result<User> result = await client.SendWithRedirectsAsResultAsync<User>(request, maxRedirects: 5);

// HttpRequestBuilder with redirect support
Result<User> result2 = await HttpRequestBuilder
    .Get("/legacy-url")
    .FollowRedirects(maxRedirects: 3)
    .AsResultAsync<User>(client);
```

### 9. HttpContent Extensions

```csharp
using HttpResponseMessage response = await client.GetAsync(uri);

Result<string> textResult = await response.Content.ReadAsStringAsResultAsync();
Result<User> jsonResult = await response.Content.ReadFromJsonAsResultAsync<User>();
```
