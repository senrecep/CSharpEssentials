---
name: csharpessentials-http
description: Use when making HTTP calls that should return Result/Result<T> instead of throwing. Covers GetFromJsonAsResultAsync, PostAsJsonAsResultAsync, PutAsJsonAsResultAsync, PatchAsJsonAsResultAsync, DeleteAsResultAsync, SendAsResultAsync on HttpClient, the fluent HttpRequestBuilder (WithHeader, WithQuery, WithJsonContent, FollowRedirects, AsResultAsync), WithQueryString/ToQueryString, HttpStatusCodeMapper, and Polly.Core-based retry/timeout/circuit-breaker policies.
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
using CSharpEssentials.ResultPattern;   // Result, Result<T>
using CSharpEssentials.Resilience;      // ResiliencePolicy (for the resilience helpers)
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
- Non-2xx → `HttpStatusCodeMapper.ToError(statusCode)`: code `Http.<status>`, type from `ToErrorType` (400/411/413-417/422 → `Validation`, 401/407 → `Unauthorized`, 403 → `Forbidden`, 404/410 → `NotFound`, 409/412/429 → `Conflict`, 408 and 5xx → `Unexpected`, 402/405/406 and any other status → `Failure`).
- Transport exceptions → `Unexpected` error. Cancelling your own token throws `OperationCanceledException`; an HTTP timeout (`TaskCanceledException` wrapping `TimeoutException`, e.g. `HttpClient.Timeout`) → `Unexpected` error with code `Http.Timeout` (a Polly `TimeoutRejectedException` keeps its type name as code), and any other cancellation → `Unexpected` error (both threw before 6.5.0).
- JSON uses `EnhancedJsonSerializerOptions.DefaultOptions` unless you pass `options`.
- Also available: `PostAsResultAsync` / `PutAsResultAsync` (raw `HttpContent`), `SendAsResultAsync` / `SendAsResultAsync<T>` (`HttpRequestMessage`), `SendWithRedirectsAsResultAsync`, and `HttpContent.ReadAsStringAsResultAsync` / `ReadFromJsonAsResultAsync<T>`.

---

## HttpRequestBuilder

```csharp
Result<User> result = await HttpRequestBuilder
    .Get("https://api.example.com/users/1")
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

Factories: `Get`, `Post`, `Put`, `Patch`, `Delete` (string or `Uri`). Builders: `WithMethod`, `WithUri`, `WithHeader`, `WithHeaders`, `WithQuery` (name/value, dictionary or object value), `WithRoute` (fills `{name}` placeholders), `WithEnumConventions`, `WithContent`, `WithJsonContent`, `FollowRedirects`. `Build()` returns `Result<HttpRequestMessage>`.

`WithQuery` and `Uri.WithQueryString` need an absolute URI; with a relative one (`.Get("/users/1")`) they throw `InvalidOperationException` instead of returning a failed `Result`. Without query values a relative URI works with `BaseAddress`.

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

## SSRF Guard (.NET 9+, not available on netstandard2.1)

Use it for any client that calls a URL a user or tenant supplied (webhooks, link previews, import-by-URL).

```csharp
services.AddHttpClient("webhooks")
    .AddSsrfGuard(o =>
    {
        o.MaxResponseContentLength = 1_048_576;
        o.Timeout = TimeSpan.FromSeconds(10);
    });
```

- Defaults: https only on port 443, at most 3 redirects, no proxy, no cookie container, never HTTP/3 (QUIC skips `ConnectCallback`). Private, loopback, link-local, metadata, multicast and reserved IPv4/IPv6 ranges are blocked, including Teredo, ORCHID (`2001:10::/28`), benchmarking (`2001:2::/48`), documentation (`2001:db8::/32`, `3fff::/20`), SRv6 SIDs (`5f00::/16`), discard-only (`100::/64`), SIIT (`::ffff:0:0:0/96`), 6to4 relay anycast (`192.88.99.0/24`), `fec0::/10`, `64:ff9b:1::/48` and IPv6 forms that embed a blocked IPv4 address.
- Every resolved address is checked, and the socket connects to an address that was checked. A name with even one private address is rejected.
- Every redirect hop is checked again. `Authorization`, `Proxy-Authorization` and `Cookie` are removed on cross-origin hops.
- Exceptions: `AllowedNetworks`/`BlockedNetworks` (`IPNetwork.Parse("10.0.0.0/8")`) and `AllowedHosts`/`BlockedHosts` (`"*.example.com"` matches subdomains, not `example.com`; case, trailing dot and IDN form are normalized). Block lists win. Use host wildcards only for domains whose DNS you control.
- Custom rules: set `options.AddressPolicy`/`options.RequestPolicy` for one client, or register `IOutboundAddressPolicy`/`IOutboundRequestPolicy` in DI for every guarded client. Options win over the container.
- A blocked request throws `SsrfBlockedException` (`Reason`, `BlockedAddress`; the message never contains the resolved address). The `*AsResultAsync` extensions return `ErrorType.Forbidden` with code `Http.SsrfBlocked` and `reason` metadata, also for `Timeout` and `ResponseTooLarge`. While buffering a body it may arrive wrapped in an `HttpRequestException`; check the `InnerException` chain.
- Do not retry `SsrfBlockedException` in resilience handlers.
- Do not replace the primary handler or turn on a proxy or cookies after `AddSsrfGuard`; the first request will throw.

---

## Best Practices

- Use `HttpRequestBuilder` when a request needs headers, query values or redirects.
- Chain follow-up calls with `Result` combinators (`Bind`, `Map`, `Match`) instead of nested try/catch.
- Register typed clients with `IHttpClientFactory` (`AddHttpClient<T>`) and keep the extensions on the injected `HttpClient`.
