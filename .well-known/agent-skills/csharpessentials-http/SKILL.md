---
name: csharpessentials-http
description: Use when making HTTP calls that should return Result/Result<T> instead of throwing — GetFromJsonAsResultAsync, PostAsJsonAsResultAsync, PutAsJsonAsResultAsync, PatchAsJsonAsResultAsync, DeleteAsResultAsync, SendAsResultAsync on HttpClient, the fluent HttpRequestBuilder (WithHeader, WithQuery, WithJsonContent, FollowRedirects, AsResultAsync), WithQueryString/ToQueryString, HttpStatusCodeMapper, and Polly.Core-based retry/timeout/circuit-breaker policies.
---

# CSharpEssentials.Http

`HttpClient` extensions that return `Result` / `Result<T>` instead of throwing on 4xx/5xx or transport errors.

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

All methods take a `Uri` (use `UriKind.Relative` with a `BaseAddress`).

```csharp
HttpClient client = new() { BaseAddress = new Uri("https://api.example.com") };

Result<User> user = await client.GetFromJsonAsResultAsync<User>(new Uri("/users/1", UriKind.Relative));

Result<User> created = await client.PostAsJsonAsResultAsync<User>(
    new Uri("/users", UriKind.Relative),
    new { Name = "Alice", Age = 30 });

Result<User> updated = await client.PutAsJsonAsResultAsync<User>(new Uri("/users/1", UriKind.Relative), new { Name = "Alice" });
Result<User> patched = await client.PatchAsJsonAsResultAsync<User>(new Uri("/users/1", UriKind.Relative), new { Age = 31 });
Result deleted = await client.DeleteAsResultAsync(new Uri("/users/1", UriKind.Relative));
```

- 2xx → success. A 2xx with an empty or undeserializable body → `NotFound` error.
- Non-2xx → `HttpStatusCodeMapper.ToError(statusCode)`: code `Http.<status>`, type from `ToErrorType` (400/422 → `Validation`, 401 → `Unauthorized`, 403 → `Forbidden`, 404 → `NotFound`, 409/429 → `Conflict`, 5xx → `Unexpected`).
- Transport exceptions → `Unexpected` error. Cancelling your own token throws `OperationCanceledException`.
- JSON uses `EnhancedJsonSerializerOptions.DefaultOptions` unless you pass `options`.
- Also available: `PostAsResultAsync` / `PutAsResultAsync` (raw `HttpContent`), `SendAsResultAsync` / `SendAsResultAsync<T>` (`HttpRequestMessage`), `SendWithRedirectsAsResultAsync`, and `HttpContent.ReadAsStringAsResultAsync` / `ReadFromJsonAsResultAsync<T>`.

---

## HttpRequestBuilder

```csharp
Result<User> result = await HttpRequestBuilder
    .Get("/users/1")
    .WithHeader("Accept", "application/json")
    .WithQuery("include", "profile")
    .AsResultAsync<User>(client);

Result posted = await HttpRequestBuilder
    .Post("/users")
    .WithJsonContent(new { Name = "Bob" })
    .AsResultAsync(client);

Result<User> redirected = await HttpRequestBuilder
    .Get("/legacy-url")
    .FollowRedirects(maxRedirects: 3)
    .AsResultAsync<User>(client);
```

Factories: `Get`, `Post`, `Put`, `Patch`, `Delete` (string or `Uri`). Builders: `WithMethod`, `WithUri`, `WithHeader`, `WithHeaders`, `WithQuery` (name/value or dictionary), `WithContent`, `WithJsonContent`, `FollowRedirects`. `Build()` returns `Result<HttpRequestMessage>`.

---

## Query Strings

```csharp
Uri uri = new("https://api.example.com/search");
Result<Uri> single = uri.WithQueryString("q", "csharp");
Result<Uri> fromDictionary = uri.WithQueryString(new Dictionary<string, string?> { ["page"] = "1" });
Result<Uri> fromObject = uri.WithQueryString(new { q = "csharp", page = 2 });
Result<string> query = new { q = "csharp" }.ToQueryString();
```

---

## Resilience

Backed by `CSharpEssentials.Resilience` and `Polly.Core` 8 (not the full Polly package).

```csharp
ResiliencePolicy policy = HttpClientResilienceExtensions.CreateResiliencePolicy(
    maxRetryAttempts: 3,
    timeout: TimeSpan.FromSeconds(30));

Result<User> result = await policy.ExecuteAsync(token =>
    client.GetFromJsonAsResultAsync<User>(new Uri("/users/1", UriKind.Relative), cancellationToken: token));
```

- Policies: `CreateRetryPolicy`, `CreateTimeoutPolicy`, `CreateCircuitBreakerPolicy`, `CreateResiliencePolicy` (plus generic `<T>` forms).
- Raw Polly pipelines: `CreateRetryPipeline`, `CreateTimeoutPipeline`, `CreateCircuitBreakerPipeline`, `CreateResiliencePipeline`, run with `pipeline.ExecuteAsResultAsync(...)`.
- Since 4.0 the non-generic policies and pipelines also retry failed `Result` values. `Unauthorized`, `Forbidden`, `NotFound` and `Validation` errors are not retried.

---

## Best Practices

- Use `HttpRequestBuilder` when a request needs headers, query values or redirects.
- Chain follow-up calls with `Result` combinators (`Bind`, `Map`, `Match`) instead of nested try/catch.
- Register typed clients with `IHttpClientFactory` (`AddHttpClient<T>`) and keep the extensions on the injected `HttpClient`.
