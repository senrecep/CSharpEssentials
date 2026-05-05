# CSharpEssentials.Http

`HttpClient` extensions that return `Result<T>` instead of throwing exceptions. Built-in status-code-to-error mapping, fluent request builder, Polly resilience, and redirect handling.

## Features

- **Result-based HTTP** — `GetFromJsonAsResultAsync`, `PostAsJsonAsResultAsync`, `PutAsJsonAsResultAsync`, `PatchAsJsonAsResultAsync`, `DeleteAsResultAsync`.
- **Automatic Error Mapping** — HTTP codes mapped to `ErrorType` (Validation, Unauthorized, NotFound, Conflict, etc.).
- **Fluent Request Builder** — Chain headers, query params, JSON content, and redirects.
- **Polly Integration** — Retry, timeout, and circuit breaker pipelines.
- **Query String Helpers** — Build URIs from dictionaries or anonymous objects.

## Installation

```bash
dotnet add package CSharpEssentials.Http
```

## Usage

### GET, POST, PUT, PATCH, DELETE as Result

```csharp
using CSharpEssentials.Http;
using CSharpEssentials.ResultPattern;

HttpClient client = new() { BaseAddress = new Uri("https://api.example.com") };

Result<User> user = await client.GetFromJsonAsResultAsync<User>(new Uri("/users/1", UriKind.Relative));

Result<User> created = await client.PostAsJsonAsResultAsync<User>(
    new Uri("/users", UriKind.Relative),
    new { Name = "Alice", Age = 30 });

Result deleted = await client.DeleteAsResultAsync(new Uri("/users/1", UriKind.Relative));
```

### HttpRequestBuilder

```csharp
Result<User> result = await HttpRequestBuilder
    .Get("/users/1")
    .WithHeader("Accept", "application/json")
    .WithQuery("include", "profile")
    .AsResultAsync<User>(client);

Result created = await HttpRequestBuilder
    .Post("/users")
    .WithJsonContent(new { Name = "Bob" })
    .AsResultAsync(client);
```

### Query String Builder

```csharp
Uri uri = new Uri("https://api.example.com/search");
Uri withQuery = uri.WithQueryString("q", "csharp").Value;
Uri withDict = uri.WithQueryString(new Dictionary<string, string?> { { "page", "1" } }).Value;
```

### Polly Resilience

```csharp
var pipeline = HttpClientResilienceExtensions.CreateResiliencePipeline(
    maxRetryAttempts: 3,
    timeout: TimeSpan.FromSeconds(30));

Result<User> result = await pipeline.ExecuteAsResultAsync(async token =>
    await client.GetFromJsonAsResultAsync<User>(new Uri("/users/1"), cancellationToken: token));
```

### Redirect Following

```csharp
Result<User> result = await HttpRequestBuilder
    .Get("/legacy-url")
    .FollowRedirects(maxRedirects: 3)
    .AsResultAsync<User>(client);
```
