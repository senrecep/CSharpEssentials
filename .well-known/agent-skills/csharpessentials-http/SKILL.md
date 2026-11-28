---
name: csharpessentials-http
description: Use when calling HTTP APIs that should return Result or Result<T> instead of throwing. Covers GetFromJsonAsResultAsync, PostAsJsonAsResultAsync, PutAsJsonAsResultAsync, PatchAsJsonAsResultAsync, DeleteAsResultAsync, SendAsResultAsync, HttpRequestBuilder, WithQueryString, HttpStatusCodeMapper, SendWithRedirectsAsResultAsync, CreateResiliencePolicy, Idempotency-Key helpers (SetIdempotencyKey, WithIdempotencyKey, IdempotencyKeyHandler, RetryUnsafeMethodsOnlyWithIdempotencyKey) and the AddSsrfGuard client for user-supplied URLs.
---

# CSharpEssentials.Http

`HttpClient` extensions that return `Result` / `Result<T>`. A non-2xx status or a transport error becomes an `Error`; only cancelling your own token throws.

## Install

```bash
dotnet add package CSharpEssentials.Http
```

## Namespaces

```csharp
using CSharpEssentials.Http;          // all types of this package
using CSharpEssentials.ResultPattern; // Result, Result<T>
using CSharpEssentials.Errors;        // Error, ErrorType
```

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials.Http
using System.Net;
using System.Text;
using CSharpEssentials.Http;
using CSharpEssentials.ResultPattern;

// Stub server so the program runs offline. Use a real HttpClient in apps.
var handler = new StubHandler(request => request.RequestUri!.AbsolutePath switch
{
    "/users/1" => new HttpResponseMessage(HttpStatusCode.OK)
    {
        Content = new StringContent("""{"id":1,"name":"Ada"}""", Encoding.UTF8, "application/json"),
    },
    "/users" => new HttpResponseMessage(HttpStatusCode.Created)
    {
        Content = new StringContent("""{"id":2,"name":"Linus"}""", Encoding.UTF8, "application/json"),
    },
    _ => new HttpResponseMessage(HttpStatusCode.NotFound),
});
using var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.com") };

// Extension methods take a Uri, not a string.
Result<User> user = await client.GetFromJsonAsResultAsync<User>(new Uri("/users/1", UriKind.Relative));
Console.WriteLine(user.Value.Name); // Ada

Result<User> created = await client.PostAsJsonAsResultAsync<User>(new Uri("/users", UriKind.Relative), new { Name = "Linus" });
Console.WriteLine(created.Value.Id); // 2

// The builder takes strings, route values and query values.
Result<User> viaBuilder = await HttpRequestBuilder
    .Get("/users/{id}")
    .WithRoute("id", 1)
    .WithQuery("include", "profile")
    .WithHeader("Accept-Language", "en")
    .AsResultAsync<User>(client);
Console.WriteLine(viaBuilder.Value.Name); // Ada

Result<User> missing = await client.GetFromJsonAsResultAsync<User>(new Uri("/users/9", UriKind.Relative));
if (missing.IsFailure)
    Console.WriteLine($"{missing.FirstError.Code} {missing.FirstError.Type}"); // Http.404 NotFound

public sealed record User(int Id, string Name);

public sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = respond(request);
        response.RequestMessage = request;
        return Task.FromResult(response);
    }
}
```

## Making calls

| Method on `HttpClient` | Returns |
|---|---|
| `GetFromJsonAsResultAsync<T>(Uri?)` | `Result<T>` |
| `PostAsJsonAsResultAsync<T>(Uri?, object value)` / `PutAsJsonAsResultAsync<T>` / `PatchAsJsonAsResultAsync<T>` | `Result<T>` |
| `PostAsResultAsync(Uri?, HttpContent)` / `PutAsResultAsync(Uri?, HttpContent)` | `Result` |
| `DeleteAsResultAsync(Uri?)` | `Result` |
| `SendAsResultAsync(HttpRequestMessage)` | `Result` |
| `SendAsResultAsync<T>(HttpRequestMessage)` | `Result<T>` |
| `SendWithRedirectsAsResultAsync(request, maxRedirects = 5)` and `<T>` | `Result` / `Result<T>` |

- Typed methods take optional `JsonSerializerOptions? options` (default `EnhancedJsonSerializerOptions.DefaultOptions`), then `CancellationToken`. Pass the token by name: `cancellationToken: ct`.
- AOT / trimming: use the `JsonTypeInfo<T>` overloads, e.g. `GetFromJsonAsResultAsync(uri, AppJson.Default.User)`, `PostAsJsonAsResultAsync(uri, value, AppJson.Default.NewUser, AppJson.Default.User)`, builder `.WithJsonTypeInfoContent(value, typeInfo).AsResultAsync(client, typeInfo)`.
- `HttpContent` readers: `content.ReadAsStringAsResultAsync()`, `content.ReadFromJsonAsResultAsync<T>()`.

## Handling results

| Outcome | Error code | `ErrorType` |
|---|---|---|
| 400, 411, 413-417 | `Http.<status>` | `Validation` |
| 422 | `Http.422` | `Domain` |
| 401, 407 | `Http.<status>` | `Unauthorized` |
| 403 | `Http.403` | `Forbidden` |
| 404, 410 | `Http.<status>` | `NotFound` |
| 409, 412, 429 | `Http.<status>` | `Conflict` |
| 408, 5xx | `Http.<status>` | `Unexpected` |
| other non-2xx | `Http.<status>` | `Failure` |
| 2xx with body `null` | `NotFound` | `NotFound` |
| 2xx with empty or malformed body (typed call) | `JsonException` | `Validation` |
| `HttpClient.Timeout` | `Http.Timeout` | `Unexpected` |
| SSRF guard blocked | `Http.SsrfBlocked` (+ `reason` metadata) | `Forbidden` |
| other exception | exception type name | `Unexpected` |
| your own token cancelled | throws `OperationCanceledException` | |

`HttpStatusCodeMapper.ToError(statusCode, description?)` and `ToErrorType(statusCode)` give the same mapping for your own code.

## Building requests

```csharp
Result<HttpRequestMessage> built = HttpRequestBuilder
    .Post("/projects/{project}/tickets")          // Get, Post, Put, Patch, Delete (string or Uri)
    .WithRoute("project", "web app")              // fills {project}, escaped
    .WithQuery("tag", new[] { "auth", "bug" })    // repeated keys; null values skipped
    .WithHeader("X-Source", "docs")               // WithHeaders(Dictionary<string, string>)
    .WithJsonContent(new { Title = "Fix login" }) // or WithContent(HttpContent), WithJsonTypeInfoContent
    .WithIdempotencyKey()                         // same key on every Build() of this builder
    .Build();                                      // or AsResultAsync(client) / AsResultAsync<T>(client)
```

- Also: `WithMethod`, `WithUri`, `WithQuery(Dictionary<string, string?>)`, `WithEnumConventions(conventions, EnumWireFormat?)`, `FollowRedirects(maxRedirects = 5)`.
- `[StringEnum]` enums are written as wire names in route and query (`pending_approval`); `[Flags]` become repeated keys; an undefined value fails with `HttpRequestBuilder.InvalidEnumValue`.
- Builder errors: `HttpRequestBuilder.UriRequired`, `RouteParameterNotFound`, `RouteValueRequired`, `InvalidEnumValue`, `ClientRequired`.
- The builder does not dispose content given to `WithContent`.

Query strings without the builder (all return `Result<Uri>` or `Result<string>`):

```csharp
Uri uri = new("https://api.example.com/search");
Result<Uri> one = uri.WithQueryString("q", "c# tips");
Result<Uri> fromDict = uri.WithQueryString(new Dictionary<string, string?> { ["page"] = "2" });
Result<Uri> fromObject = uri.WithQueryString(new { Q = "csharp", Page = 2 }); // reflection, not AOT-safe
Result<string> text = new Dictionary<string, string?> { ["a"] = "1" }.ToQueryString();
```

## Following redirects

`SendWithRedirectsAsResultAsync` and `FollowRedirects()` only see a 3xx if the handler has `AllowAutoRedirect = false` (default handlers follow redirects themselves). They follow 301, 302, 303, 307, 308; 303 and POST after 301/302 become GET without body; cross-origin hops drop `Authorization`, `Proxy-Authorization`, `Cookie`. Errors: `Http.RedirectLimitExceeded`, `Http.MissingRedirectLocation`, `Http.CrossOriginRedirect` (credentials in `DefaultRequestHeaders`).

## Retrying

```csharp
ResiliencePolicy policy = HttpClientResilienceExtensions.CreateResiliencePolicy(
    maxRetryAttempts: 3, timeout: TimeSpan.FromSeconds(30), retryDelay: TimeSpan.FromSeconds(1));
Result<User> user = await policy.ExecuteAsync(ct =>
    client.GetFromJsonAsResultAsync<User>(new Uri("/users/1", UriKind.Relative), cancellationToken: ct));
```

- `CreateRetryPolicy`, `CreateTimeoutPolicy`, `CreateCircuitBreakerPolicy` (minimumThroughput default 5), `CreateResiliencePolicy` return `ResiliencePolicy` (namespace `CSharpEssentials.Resilience`). `CreateRetryPipeline`, `CreateTimeoutPipeline`, `CreateCircuitBreakerPipeline`, `CreateResiliencePipeline` return Polly pipelines; run them with `pipeline.ExecuteAsResultAsync(ct => ...)`. All but the timeout ones have `<T>` forms.
- Not retried: `Unauthorized`, `Forbidden`, `NotFound`, `Validation`, `Domain` errors, that is 400, 401, 403, 404, 407, 410, 411, 413-417 and 422. Everything else is retried: 5xx, 408, 409, 412, 429, and other 4xx such as 405 (they map to `Failure`).

## Idempotency keys

```csharp
string key = request.SetIdempotencyKey();                 // HttpRequestMessage; or SetIdempotencyKey("my-key")
var client = new HttpClient(new IdempotencyKeyHandler(new HttpClientHandler())); // adds a key to POST/PATCH without one

services.AddHttpClient("payments")
    .AddIdempotencyKeys()                                 // .NET 9+, register before the resilience handler
    .AddStandardResilienceHandler(o => o.Retry.RetryUnsafeMethodsOnlyWithIdempotencyKey()); // Microsoft.Extensions.Http.Resilience
```

- `RetryUnsafeMethodsOnlyWithIdempotencyKey(IdempotentRetryOptions?)` extends any `RetryStrategyOptions<HttpResponseMessage>`. It retries GET/HEAD/PUT/DELETE/OPTIONS on no response, 408, 429, 5xx; POST/PATCH only with a key, on no response, 408, 429, 503, 504 and 409 `Idempotency.InFlight`. Never: `Idempotency-Replayed: true`, caller cancellation, `SsrfBlockedException`.
- `IdempotentRetry.ShouldRetryAsync(request, response, exception, options, ct)` is the decision; `IdempotentRetry.GetRetryAfter(response, maxRetryAfter)` reads `Retry-After`. `IdempotentRetryOptions`: `MaxRetryAfter` 30 s, `ProblemReadTimeout` 2 s.
- In a Polly pipeline you run yourself around `HttpClient`, build a new `HttpRequestMessage` per attempt from a builder with `WithIdempotencyKey()`, and set `context.Properties.Set(IdempotentRetry.RequestMessageKey, request)` so attempts without a response can be judged.

## SSRF guard (.NET 9+)

For any client that fetches URLs a user or tenant supplies.

```csharp Program.cs
// dotnet add package CSharpEssentials.Http
using CSharpEssentials.Http;
using CSharpEssentials.ResultPattern;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddHttpClient("webhooks").AddSsrfGuard(o =>
{
    o.MaxResponseContentLength = 1_048_576;
    o.Timeout = TimeSpan.FromSeconds(10);
});
using ServiceProvider provider = services.BuildServiceProvider();
HttpClient client = provider.GetRequiredService<IHttpClientFactory>().CreateClient("webhooks");

// Blocked before any socket opens: loopback address and plain http.
foreach (string url in new[] { "https://127.0.0.1/", "http://example.com/" })
{
    Result result = await client.SendAsResultAsync(new HttpRequestMessage(HttpMethod.Get, url));
    Console.WriteLine($"{url} {result.FirstError.Code} {result.FirstError.Metadata?["reason"]}");
}
// https://127.0.0.1/ Http.SsrfBlocked AddressNotAllowed
// http://example.com/ Http.SsrfBlocked RequestNotAllowed
```

- Defaults: https on 443 only, 3 redirects, no proxy, no cookies, no HTTP/3. Private, loopback, link-local, metadata, multicast, documentation and reserved IPv4/IPv6 ranges are blocked, including IPv6 forms that embed them.
- `SsrfGuardOptions`: `AllowHttp`, `AllowedPorts`, `MaxRedirects`, `MaxResponseContentLength`, `Timeout`, `AllowedNetworks`, `BlockedNetworks` (`IPNetwork.Parse("10.0.0.0/8")`), `AllowedHosts`, `BlockedHosts` (`"*.example.com"` = subdomains only), `AddressPolicy`, `RequestPolicy`. Block lists win over allow lists.
- Custom rules: implement `IOutboundAddressPolicy` / `IOutboundRequestPolicy`; set them on the options for one client or register them in DI for all guarded clients. Defaults: `DefaultOutboundAddressPolicy.Instance`, `new DefaultOutboundRequestPolicy(options)`.
- Raw calls throw `SsrfBlockedException` (`Reason`, `RequestUri`, `BlockedAddress`); it can be wrapped in an `HttpRequestException` while buffering a body.

## Rules

- These do not exist: `GetAsResultAsync`, `GetAsResultAsync<T>`, `PatchAsResultAsync(HttpContent)`, `DeleteAsResultAsync<T>`, `PostAsResultAsync<T>`. Use `SendAsResultAsync` or the builder for other shapes.
- Extension methods take `Uri`, not `string`. Use `new Uri("/path", UriKind.Relative)` with a `BaseAddress`. The builder factories take strings.
- Do not use a typed `<T>` call for a 204 endpoint: the empty body fails with `Validation`. Use the untyped method.
- Do not pass a literal `null` for options (`GetFromJsonAsResultAsync<T>(uri, null)` is ambiguous, CS0121). Omit it.
- A failed result is not an exception: check `IsFailure` / `FirstError` instead of try/catch. Only your own cancelled token throws.
- 422 is `ErrorType.Domain`, not `Validation`.
- `HttpClient` cannot send the same `HttpRequestMessage` twice. Build a new one per retry; the builder can `Build()` repeatedly.
- `FollowRedirects` needs `AllowAutoRedirect = false` on the handler to take effect.
- Do not replace the primary handler, add a proxy or enable cookies after `AddSsrfGuard`; the first request throws `InvalidOperationException`. Synchronous `Send` is not supported with the guard.
- Do not retry `SsrfBlockedException`.
- `AddSsrfGuard` and `AddIdempotencyKeys` do not exist on netstandard2.1.
- No analyzer diagnostics ship with this package.
