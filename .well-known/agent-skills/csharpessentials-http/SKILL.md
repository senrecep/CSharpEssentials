---
name: csharpessentials-http
description: Use when making HTTP calls that should return Result<T> instead of throwing exceptions — GetFromJsonResultAsync, PostAsJsonResultAsync, DeleteResultAsync on HttpClient, and HttpRequestBuilder for fluent multi-header/query-param requests with optional Polly resilience.
---

# CSharpEssentials.Http

HttpClient extensions that return `Result<T>` instead of throwing on 4xx/5xx. Never catch `HttpRequestException` again.

## Installation

```bash
dotnet add package CSharpEssentials.Http
```

## Namespace

```csharp
using CSharpEssentials.Http;
```

---

## Result-Returning Extensions

```csharp
// Register typed client
builder.Services.AddHttpClient<UserApiClient>(c =>
    c.BaseAddress = new Uri("https://api.example.com"));

// GET
Result<User> result = await _client.GetFromJsonResultAsync<User>("/users/1");

// POST
Result<Order> posted = await _client.PostAsJsonResultAsync<Order>("/orders", newOrder);

// PUT
Result<User> updated = await _client.PutAsJsonResultAsync<User>("/users/1", userDto);

// DELETE
Result deleted = await _client.DeleteResultAsync("/orders/1");

// All methods: 2xx → Success, 4xx/5xx → Failure with Error describing the HTTP status
```

---

## HttpRequestBuilder — fluent complex requests

```csharp
var result = await new HttpRequestBuilder(_client)
    .WithUrl("/search")
    .WithQueryParam("q", query)
    .WithQueryParam("page", "1")
    .WithHeader("X-Api-Key", apiKey)
    .WithHeader("X-Correlation-Id", correlationId)
    .GetAsync<SearchResult>();
```

---

## Resilience (Polly)

```csharp
builder.Services.AddHttpClient<ApiClient>()
    .AddRetryPolicy(retryCount: 3)
    .AddCircuitBreakerPolicy(
        handledEventsAllowedBeforeBreaking: 5,
        durationOfBreak: TimeSpan.FromSeconds(30));
```

---

## Best Practices

- Prefer `HttpRequestBuilder` over raw `HttpClient` for multi-header or multi-param requests
- Combine with `Result.ThenAsync()` to chain downstream calls without nested try/catch
- Use typed `HttpClient` classes rather than `IHttpClientFactory` directly for testability
