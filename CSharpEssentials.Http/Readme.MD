# CSharpEssentials.Http

`HttpClient` extensions that return `Result` and `Result<T>` instead of throwing. A non-success status becomes an `Error` with a matching `ErrorType`. The package also has a request builder, query string helpers, redirect following, retry helpers, `Idempotency-Key` support and an SSRF guard for user-supplied URLs.

Benchmarks against HttpClient with System.Net.Http.Json, Refit, Flurl.Http, RestSharp and Microsoft.Extensions.Http.Resilience are in [BENCHMARKS.md](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Http/BENCHMARKS.md).

## Install

```bash
dotnet add package CSharpEssentials.Http
```

Targets net9.0, net10.0, net11.0 and netstandard2.1. The SSRF guard and `AddIdempotencyKeys()` need .NET 9 or later.

```csharp
using CSharpEssentials.Http;          // every type in this package
using CSharpEssentials.ResultPattern; // Result, Result<T>
using CSharpEssentials.Errors;        // Error, ErrorType
```

## Quick start

The programs on this page use a stub `HttpMessageHandler` in place of a server, so they run offline. In an application, use a normal `HttpClient` or `IHttpClientFactory`.

```csharp Program.cs
// dotnet add package CSharpEssentials.Http
using System.Net;
using System.Text;
using CSharpEssentials.Http;
using CSharpEssentials.ResultPattern;

var handler = new StubHandler(request => (request.Method.Method, request.RequestUri!.AbsolutePath) switch
{
    ("GET", "/users/1") => Json(HttpStatusCode.OK, """{"id":1,"name":"Ada"}"""),
    ("POST", "/users") => Json(HttpStatusCode.Created, """{"id":2,"name":"Linus"}"""),
    ("DELETE", "/users/1") => new HttpResponseMessage(HttpStatusCode.NoContent),
    _ => new HttpResponseMessage(HttpStatusCode.NotFound),
});
using var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.com") };

Result<User> user = await client.GetFromJsonAsResultAsync<User>(new Uri("/users/1", UriKind.Relative));
Console.WriteLine(user.Value.Name); // Ada

Result<User> created = await client.PostAsJsonAsResultAsync<User>(new Uri("/users", UriKind.Relative), new { Name = "Linus" });
Console.WriteLine(created.Value.Id); // 2

Result deleted = await client.DeleteAsResultAsync(new Uri("/users/1", UriKind.Relative));
Console.WriteLine(deleted.IsSuccess); // True

Result<User> missing = await client.GetFromJsonAsResultAsync<User>(new Uri("/users/9", UriKind.Relative));
Console.WriteLine($"{missing.FirstError.Code} {missing.FirstError.Type}"); // Http.404 NotFound

static HttpResponseMessage Json(HttpStatusCode status, string json) =>
    new(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

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

## Result calls on HttpClient

All of them are extension methods on `HttpClient` in `HttpClientResultExtensions`. They take a `Uri?`; with a `BaseAddress`, pass `new Uri("/path", UriKind.Relative)`.

| Method | Body sent | Returns |
|---|---|---|
| `GetFromJsonAsResultAsync<T>(uri)` | none | `Result<T>` |
| `PostAsJsonAsResultAsync<T>(uri, value)` | `value` as JSON | `Result<T>` |
| `PutAsJsonAsResultAsync<T>(uri, value)` | `value` as JSON | `Result<T>` |
| `PatchAsJsonAsResultAsync<T>(uri, value)` | `value` as JSON | `Result<T>` |
| `PostAsResultAsync(uri, content)` | your `HttpContent` | `Result` |
| `PutAsResultAsync(uri, content)` | your `HttpContent` | `Result` |
| `DeleteAsResultAsync(uri)` | none | `Result` |
| `SendAsResultAsync(request)` | your `HttpRequestMessage` | `Result` |
| `SendAsResultAsync<T>(request)` | your `HttpRequestMessage` | `Result<T>` |

Every typed method has an optional `JsonSerializerOptions? options` (default `EnhancedJsonSerializerOptions.DefaultOptions` from `CSharpEssentials.Json`) and a `JsonTypeInfo<T>` overload for source-generated JSON. All take an optional `CancellationToken`.

How a response becomes a result:

- 2xx: success. The typed methods read the body as `T`.
- 2xx with the JSON literal `null`: `NotFound` error.
- 2xx with an empty or malformed body: `Validation` error. A typed call on a `204 No Content` therefore fails; use the untyped method for endpoints that return no body.
- Any other status: the mapped error described next. The response body is not read.

## Status codes

`HttpStatusCodeMapper.ToError(statusCode, description = null)` builds the error. Its code is `Http.<status>` and its default description is `HTTP request failed with status code 404 (NotFound).`. `HttpStatusCodeMapper.ToErrorType(statusCode)` returns only the type.

| Status | `ErrorType` |
|---|---|
| 400, 411, 413, 414, 415, 416, 417 | `Validation` |
| 422 | `Domain` |
| 401, 407 | `Unauthorized` |
| 403 | `Forbidden` |
| 404, 410 | `NotFound` |
| 409, 412, 429 | `Conflict` |
| 408 and every 5xx | `Unexpected` |
| 402, 405, 406 and every other status | `Failure` |

## Errors, timeouts and cancellation

Every `*AsResultAsync` method catches exceptions and returns them as errors, with one exception: cancelling your own `CancellationToken` throws `OperationCanceledException`.

| What happened | Error code | `ErrorType` |
|---|---|---|
| `HttpClient.Timeout` elapsed | `Http.Timeout` | `Unexpected` |
| Polly timeout (`TimeoutRejectedException`) | `TimeoutRejectedException` | `Unexpected` |
| Body or request value is not valid JSON (`JsonException`) | `JsonException` | `Validation` |
| SSRF guard blocked the request | `Http.SsrfBlocked` | `Forbidden` |
| Any other exception, such as a refused connection | exception type name | `Unexpected` |

`HttpContentExtensions` reads an `HttpContent` you already have with the same rules: `ReadAsStringAsResultAsync()` and `ReadFromJsonAsResultAsync<T>()`.

```csharp Program.cs
// dotnet add package CSharpEssentials.Http
using System.Net;
using System.Text;
using CSharpEssentials.Errors;
using CSharpEssentials.Http;
using CSharpEssentials.ResultPattern;

var handler = new StubHandler(async (request, ct) =>
{
    switch (request.RequestUri!.AbsolutePath)
    {
        case "/slow":
            await Task.Delay(TimeSpan.FromSeconds(10), ct);
            return new HttpResponseMessage(HttpStatusCode.OK);
        case "/broken-json":
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ not json", Encoding.UTF8, "application/json") };
        case "/null":
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("null", Encoding.UTF8, "application/json") };
        case "/no-content":
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        case "/rule":
            return new HttpResponseMessage(HttpStatusCode.UnprocessableEntity);
        default:
            throw new HttpRequestException("Connection refused.");
    }
});
using var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.com"), Timeout = TimeSpan.FromMilliseconds(200) };

foreach (string path in new[] { "/slow", "/broken-json", "/null", "/no-content", "/rule", "/down" })
{
    Result<Item> result = await client.GetFromJsonAsResultAsync<Item>(new Uri(path, UriKind.Relative));
    Console.WriteLine($"{path,-12} {result.FirstError.Type,-10} {result.FirstError.Code}");
}
// /slow        Unexpected Http.Timeout
// /broken-json Validation JsonException
// /null        NotFound   NotFound
// /no-content  Validation JsonException
// /rule        Domain     Http.422
// /down        Unexpected HttpRequestException

// The untyped call accepts 204.
Result noContent = await client.SendAsResultAsync(new HttpRequestMessage(HttpMethod.Get, new Uri("/no-content", UriKind.Relative)));
Console.WriteLine($"untyped 204: {noContent.IsSuccess}");

// Map a status yourself, for example in a hand-written client.
Error conflict = HttpStatusCodeMapper.ToError(HttpStatusCode.Conflict, "The order changed.");
Console.WriteLine($"{conflict.Code} {conflict.Type} {conflict.Description}"); // Http.409 Conflict The order changed.
Console.WriteLine(HttpStatusCodeMapper.ToErrorType(HttpStatusCode.BadGateway)); // Unexpected

// Read content you already hold.
using var content = new StringContent("""{"name":"pen"}""", Encoding.UTF8, "application/json");
Result<Item> item = await content.ReadFromJsonAsResultAsync<Item>();
Result<string> text = await content.ReadAsStringAsResultAsync();
Console.WriteLine($"{item.Value.Name} / {text.Value}"); // pen / {"name":"pen"}

// Your own token: cancelling it throws instead of returning an error.
using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
try
{
    await client.GetFromJsonAsResultAsync<Item>(new Uri("/slow", UriKind.Relative), cancellationToken: cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("caller cancelled: OperationCanceledException");
}

public sealed record Item(string Name);

public sealed class StubHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond) : HttpMessageHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await respond(request, cancellationToken);
        response.RequestMessage = request;
        return response;
    }
}
```

## Request builder

`HttpRequestBuilder` collects method, URI, headers, query values, route values and body, then sends them with `AsResultAsync(client)` or `AsResultAsync<T>(client)`. `Build()` returns the `Result<HttpRequestMessage>` without sending.

- Start: `Get`, `Post`, `Put`, `Patch`, `Delete` (string or `Uri`). Change later with `WithMethod` and `WithUri`.
- Headers: `WithHeader(name, value)` and `WithHeaders(dictionary)`. A content header such as `Content-Type` goes on the body and replaces the value the content had. Calling `WithHeader` again with the same content header adds a second value.
- Query: `WithQuery(name, string)`, `WithQuery(name, object?)` and `WithQuery(dictionary)`. Null values are skipped. A collection becomes repeated keys. Values are escaped and merged with any query already in the URI, before a fragment. Works for relative and absolute URIs.
- Route: `WithRoute(name, value)` fills `{name}` in the URI, escaped. A missing placeholder or a null value fails `Build()` with `HttpRequestBuilder.RouteParameterNotFound` or `HttpRequestBuilder.RouteValueRequired`.
- Body: `WithJsonContent(value, options?)`, `WithJsonTypeInfoContent(value, typeInfo)` or `WithContent(httpContent)`. The last call wins.
- Other: `WithEnumConventions`, `WithIdempotencyKey`, `FollowRedirects(maxRedirects = 5)`.

A builder can send again, for example inside a retry. JSON content is created fresh on each `Build()`. Content passed to `WithContent` is not disposed by the builder: dispose it yourself when you are done, and do not reuse content that can be read only once, such as a `StreamContent` over a network stream.

```csharp Program.cs
// dotnet add package CSharpEssentials.Http
using System.Net;
using System.Text;
using CSharpEssentials.Http;
using CSharpEssentials.ResultPattern;

var handler = new StubHandler(request =>
{
    string body = request.Content?.ReadAsStringAsync().Result ?? "";
    Console.WriteLine($"server got {request.Method} {request.RequestUri!.AbsoluteUri} lang={request.Headers.AcceptLanguage} body={body}");
    return new HttpResponseMessage(HttpStatusCode.OK)
    {
        Content = new StringContent("""{"id":7,"title":"Fix login"}""", Encoding.UTF8, "application/json"),
    };
});
using var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.com") };

Result<Ticket> ticket = await HttpRequestBuilder
    .Get("/projects/{project}/tickets/{id}")
    .WithRoute("project", "web app")
    .WithRoute("id", 7)
    .WithQuery("include", "comments")
    .WithQuery("tag", new[] { "auth", "bug" })
    .WithQuery("assignee", (object?)null)
    .WithHeader("Accept-Language", "en")
    .AsResultAsync<Ticket>(client);
Console.WriteLine(ticket.Value.Title);
// server got GET https://api.example.com/projects/web%20app/tickets/7?include=comments&tag=auth&tag=bug ...

Result created = await HttpRequestBuilder
    .Post("/tickets")
    .WithHeaders(new Dictionary<string, string> { ["X-Request-Source"] = "docs" })
    .WithJsonContent(new { Title = "New ticket" })
    .AsResultAsync(client);
Console.WriteLine(created.IsSuccess);

// Build without sending.
Result<HttpRequestMessage> built = HttpRequestBuilder.Delete("/tickets/{id}").WithQuery("force", true).Build();
Console.WriteLine($"{built.Value.Method} {built.Value.RequestUri}"); // DELETE /tickets/{id}?force=True

Result<HttpRequestMessage> bad = HttpRequestBuilder.Get("/tickets").WithRoute("id", 1).Build();
Console.WriteLine(bad.FirstError.Code); // HttpRequestBuilder.RouteParameterNotFound

public sealed record Ticket(int Id, string Title);

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

`Build()` and `AsResultAsync` return validation errors instead of throwing: `HttpRequestBuilder.UriRequired`, `HttpRequestBuilder.InvalidEnumValue`, `HttpRequestBuilder.ClientRequired` (null client) and the route errors above.

### Enums in route, query and body

Enums marked `[StringEnum]` (from `CSharpEssentials.Enums`) are written by their wire names in routes and query strings. `[Flags]` values and enum collections become repeated query keys. A value that is not defined fails with `HttpRequestBuilder.InvalidEnumValue` instead of being sent. `WithEnumConventions(conventions, writeAs)` picks the conventions; `EnumWireFormat.Number` sends numbers to servers that only accept integers. The same conventions apply to the body written by `WithJsonContent`, where an undefined value is sent as its number. Plain enums without `[StringEnum]` keep the framework behavior.

```csharp Program.cs
// dotnet add package CSharpEssentials.Http
using CSharpEssentials.Enums;
using CSharpEssentials.Http;

Uri named = HttpRequestBuilder
    .Get("https://api.example.com/orders/{status}")
    .WithRoute("status", OrderStatus.PendingApproval)
    .WithQuery("permissions", Permissions.Read | Permissions.Write)
    .Build().Value.RequestUri!;
Console.WriteLine(named); // https://api.example.com/orders/pending_approval?permissions=read&permissions=write

Uri numbers = HttpRequestBuilder
    .Get("https://api.example.com/orders/{status}")
    .WithEnumConventions(EnumConventions.Default, EnumWireFormat.Number)
    .WithRoute("status", OrderStatus.PendingApproval)
    .Build().Value.RequestUri!;
Console.WriteLine(numbers); // https://api.example.com/orders/1

[StringEnum]
public enum OrderStatus { Draft, PendingApproval }

[Flags, StringEnum]
public enum Permissions { None = 0, Read = 1, Write = 2 }
```

To read enum names back from responses tolerantly (wire names, member names, numbers, an `[EnumFallback]` member for values a newer server adds), pass options built with `new JsonSerializerOptions(JsonSerializerDefaults.Web).AddEnumConventions(conventions, EnumReadMode.Data, writeAs)` to `AsResultAsync<T>(client, options)`. Roll out readers before writers: a server switches from numbers to names only after every client reads names.

## Query strings

`QueryStringExtensions` builds query strings outside the builder. Each method returns a `Result`, so a bad key comes back as a validation error.

- `uri.WithQueryString(name, value)`: one pair. The `object?` overload also takes enums and collections, with optional `EnumConventions` and `EnumWireFormat`.
- `uri.WithQueryString(dictionary)`: a `Dictionary<string, string?>`.
- `uri.WithQueryString(object)`: the public properties of an object, names as keys.
- `dictionary.ToQueryString()` and `obj.ToQueryString()`: the query text without `?`.

Null values are skipped, keys and values are escaped, numbers and dates use the invariant culture. The object overloads read properties by reflection and are marked `RequiresUnreferencedCode`, so avoid them in trimmed or Native AOT apps.

```csharp Program.cs
// dotnet add package CSharpEssentials.Http
using CSharpEssentials.Http;
using CSharpEssentials.ResultPattern;

Uri search = new("https://api.example.com/search?lang=en#top");

Console.WriteLine(search.WithQueryString("q", "c# tips").Value.AbsoluteUri);
// https://api.example.com/search?lang=en&q=c%23%20tips#top

Console.WriteLine(search.WithQueryString(new Dictionary<string, string?> { ["page"] = "2", ["sort"] = null }).Value.AbsoluteUri);
// https://api.example.com/search?lang=en&page=2#top

Console.WriteLine(search.WithQueryString(new { Q = "csharp", Page = 2, Tags = new[] { "a", "b" } }).Value.AbsoluteUri);
// https://api.example.com/search?lang=en&Q=csharp&Page=2&Tags=a&Tags=b#top

Result<string> query = new Dictionary<string, string?> { ["from"] = "2026-01-01", ["to"] = "2026-02-01" }.ToQueryString();
Console.WriteLine(query.Value); // from=2026-01-01&to=2026-02-01

Result<Uri> bad = search.WithQueryString("", "x");
Console.WriteLine(bad.FirstError.Code); // QueryString.NameRequired
```

## Source-generated JSON and Native AOT

The overloads that take `JsonSerializerOptions` are marked `RequiresUnreferencedCode` and `RequiresDynamicCode`. In trimmed or Native AOT apps, pass a `JsonTypeInfo<T>` from a `JsonSerializerContext`. Every typed method has this overload, including `SendWithRedirectsAsResultAsync`, `ReadFromJsonAsResultAsync` and `AsResultAsync`. On the builder, use `WithJsonTypeInfoContent` for the body.

Because of these overloads, passing a literal `null` as the options argument does not compile (CS0121). Leave the argument out or cast it: `(JsonSerializerOptions?)null`.

```csharp Program.cs
// dotnet add package CSharpEssentials.Http
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpEssentials.Http;
using CSharpEssentials.ResultPattern;

var handler = new StubHandler(request => new HttpResponseMessage(HttpStatusCode.OK)
{
    Content = new StringContent("""{"id":3,"name":"Grace"}""", Encoding.UTF8, "application/json"),
});
using var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.com") };
var uri = new Uri("/users", UriKind.Relative);

Result<User> read = await client.GetFromJsonAsResultAsync(uri, AppJson.Default.User);
Result<User> posted = await client.PostAsJsonAsResultAsync(uri, new NewUser("Grace"), AppJson.Default.NewUser, AppJson.Default.User);
Result<User> built = await HttpRequestBuilder.Post("/users")
    .WithJsonTypeInfoContent(new NewUser("Grace"), AppJson.Default.NewUser)
    .AsResultAsync(client, AppJson.Default.User);

Console.WriteLine($"{read.Value.Name} {posted.Value.Id} {built.Value.Name}"); // Grace 3 Grace

public sealed record User(int Id, string Name);
public sealed record NewUser(string Name);

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(NewUser))]
internal sealed partial class AppJson : JsonSerializerContext;

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

## Redirects

`SendWithRedirectsAsResultAsync(request, maxRedirects = 5)` (with typed and `JsonTypeInfo<T>` forms) and the builder's `FollowRedirects(maxRedirects)` follow redirects themselves and return the final response as a `Result`.

These only see a 3xx when the handler does not follow it first. `HttpClientHandler`, `SocketsHttpHandler` and the default `IHttpClientFactory` handler have `AllowAutoRedirect = true`. Set it to `false` to use these methods.

- Only 301, 302, 303, 307 and 308 are followed. Any other status, including 304, is returned as its own error, such as `Http.304`.
- 303, and 301 or 302 after a POST, continue as a GET with no body. 307 and 308 keep the method and body.
- A hop to another scheme, host or port drops `Authorization`, `Proxy-Authorization` and `Cookie`.
- If `HttpClient.DefaultRequestHeaders` holds one of those headers, a cross-origin redirect is refused with `Http.CrossOriginRedirect` (`Forbidden`), because default headers cannot be removed per hop.
- More hops than `maxRedirects` give `Http.RedirectLimitExceeded`; a 3xx without `Location` gives `Http.MissingRedirectLocation` (both `Unexpected`).

```csharp Program.cs
// dotnet add package CSharpEssentials.Http
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using CSharpEssentials.Http;
using CSharpEssentials.ResultPattern;

var handler = new StubHandler(request =>
{
    string path = request.RequestUri!.AbsolutePath;
    if (path == "/old")
        return Redirect(HttpStatusCode.MovedPermanently, "/users/1");
    if (path == "/loop")
        return Redirect(HttpStatusCode.Found, "/loop");
    if (path == "/elsewhere")
        return Redirect(HttpStatusCode.TemporaryRedirect, "https://other.example.com/users/1");
    return new HttpResponseMessage(HttpStatusCode.OK)
    {
        Content = new StringContent("""{"id":1,"name":"Ada"}""", Encoding.UTF8, "application/json"),
    };
});
using var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.com") };

Result<User> moved = await client.SendWithRedirectsAsResultAsync<User>(new HttpRequestMessage(HttpMethod.Get, "/old"));
Console.WriteLine(moved.Value.Name); // Ada

Result loop = await HttpRequestBuilder.Get("/loop").FollowRedirects(maxRedirects: 3).AsResultAsync(client);
Console.WriteLine(loop.FirstError.Code); // Http.RedirectLimitExceeded

client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "secret");
Result crossOrigin = await HttpRequestBuilder.Get("/elsewhere").FollowRedirects().AsResultAsync(client);
Console.WriteLine(crossOrigin.FirstError.Code); // Http.CrossOriginRedirect

static HttpResponseMessage Redirect(HttpStatusCode status, string location)
{
    var response = new HttpResponseMessage(status);
    response.Headers.Location = new Uri(location, UriKind.RelativeOrAbsolute);
    return response;
}

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

## Retry helpers

`HttpClientResilienceExtensions` creates policies from [CSharpEssentials.Resilience](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Resilience/Readme.MD) with HTTP-friendly defaults. A failed `Result` is retried unless its type is `Unauthorized`, `Forbidden`, `NotFound`, `Validation` or `Domain`. For HTTP that means 400, 401, 403, 404, 407, 410, 411, 413 to 417 and 422 are not retried. Every other failure is retried, including 5xx, 408, 409, 412, 429 and 4xx statuses that map to `Failure` such as 405.

| Returns `ResiliencePolicy` | Returns a Polly `ResiliencePipeline` | Defaults |
|---|---|---|
| `CreateRetryPolicy(maxRetryAttempts, delay)` | `CreateRetryPipeline(...)` | 3 retries, 1 s exponential |
| `CreateTimeoutPolicy(timeout)` | `CreateTimeoutPipeline(timeout)` | |
| `CreateCircuitBreakerPolicy(minimumThroughput, samplingDuration, breakDuration)` | `CreateCircuitBreakerPipeline(...)` | 5 calls, 1 min, 30 s |
| `CreateResiliencePolicy(maxRetryAttempts, timeout, retryDelay)` | `CreateResiliencePipeline(...)` | 3 retries, 30 s timeout per attempt |

Each one except the timeout has a generic `<T>` form that returns `ResiliencePolicy<T>` or `ResiliencePipeline<Result<T>>`. Run a pipeline with `pipeline.ExecuteAsResultAsync(ct => ...)`.

```csharp Program.cs
// dotnet add package CSharpEssentials.Http
using System.Net;
using System.Text;
using CSharpEssentials.Http;
using CSharpEssentials.Resilience;
using CSharpEssentials.ResultPattern;
using Polly;

int calls = 0;
var handler = new StubHandler(request => ++calls < 3
    ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
    : new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""{"id":1,"name":"Ada"}""", Encoding.UTF8, "application/json") });
using var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.com") };
var uri = new Uri("/users/1", UriKind.Relative);

ResiliencePolicy policy = HttpClientResilienceExtensions.CreateResiliencePolicy(
    maxRetryAttempts: 3, timeout: TimeSpan.FromSeconds(10), retryDelay: TimeSpan.FromMilliseconds(10));
Result<User> user = await policy.ExecuteAsync(ct => client.GetFromJsonAsResultAsync<User>(uri, cancellationToken: ct));
Console.WriteLine($"{user.Value.Name} after {calls} calls"); // Ada after 3 calls

calls = 0;
ResiliencePipeline<Result<User>> pipeline = HttpClientResilienceExtensions.CreateRetryPipeline<User>(maxRetryAttempts: 3, delay: TimeSpan.FromMilliseconds(10));
Result<User> again = await pipeline.ExecuteAsResultAsync(ct => client.GetFromJsonAsResultAsync<User>(uri, cancellationToken: ct));
Console.WriteLine($"{again.Value.Name} after {calls} calls"); // Ada after 3 calls

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

Retrying a POST or PATCH this way repeats it. Use the idempotency helpers below when the server must not run it twice.

## Idempotency keys and safe retries

These helpers are the client side of the [`Idempotency-Key` contract](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Idempotency/Readme.MD). A POST or PATCH may be retried only if every attempt carries the same key, so the server can recognize the repeat.

Setting a key:

- `request.SetIdempotencyKey(key = null)` sets the `Idempotency-Key` header on an `HttpRequestMessage`, replacing any earlier one, and returns the key. With no argument it generates a random UUIDv4 (`Guid.NewGuid()`); a version 7 key would reveal when the request was made. A key may hold only visible ASCII other than `"` and `\`; anything else throws `ArgumentException`.
- `builder.WithIdempotencyKey(key = null)` fixes the key when it is called, so every `Build()` of that builder sends the same key. A new operation needs a new builder. For a key that survives a restart, pass a deterministic one such as `IdempotencyKeys.For(...)` from `CSharpEssentials.Idempotency`.
- `IdempotencyKeyHandler` is a `DelegatingHandler` that adds a generated key to a POST or PATCH without one and leaves other requests alone. Use `new IdempotencyKeyHandler(innerHandler)`, or `services.AddHttpClient(...).AddIdempotencyKeys()` on .NET 9+. Register it before the retry handler, so it runs once per call and not once per attempt.

Deciding whether to retry: `IdempotentRetry.ShouldRetryAsync(request, response, exception, options, cancellationToken)` returns the decision for one outcome. `RetryUnsafeMethodsOnlyWithIdempotencyKey(options?)` plugs it into a Polly `RetryStrategyOptions<HttpResponseMessage>` as `ShouldHandle`, and makes 409 InFlight, 429 and 503 wait for `Retry-After`.

| Outcome | GET, HEAD, PUT, DELETE, OPTIONS | POST, PATCH with a key | POST, PATCH without a key |
|---|---|---|---|
| Response has `Idempotency-Replayed: true` | no | no | no |
| No response (`HttpRequestException`, timeout) | yes | yes | no |
| 408, 429, 503, 504 | yes | yes | no |
| 500, 502 and other 5xx | yes | no | no |
| 409 with problem+json `code` `Idempotency.InFlight` | only with a key | yes | no |
| Other 409, other 4xx, 2xx | no | no | no |
| Caller cancelled the token, or `SsrfBlockedException` | no | no | no |

- The 409 body is read only when it is `application/problem+json`, at most 4 KiB, within `IdempotentRetryOptions.ProblemReadTimeout` (default 2 s). The bytes are put back, so the body stays readable.
- `IdempotentRetry.GetRetryAfter(response, maxRetryAfter)` reads `Retry-After` as seconds or an HTTP date (measured from the response `Date` header) and caps it at `maxRetryAfter`. The strategy caps it at `IdempotentRetryOptions.MaxRetryAfter` (default 30 s). Without the header, the delay from your retry options, or the `DelayGenerator` set before the call, applies.
- The request comes from `response.RequestMessage`. When no response arrived it comes from `context.Properties` under `IdempotentRetry.RequestMessageKey`. `AddStandardResilienceHandler` sets that property; in a pipeline you run yourself, set it, or failed attempts without a response are not retried.
- `HttpClient` refuses to send the same `HttpRequestMessage` twice. Inside a `DelegatingHandler` (such as `AddStandardResilienceHandler`) the retry resends the message itself. In a pipeline you run around `HttpClient`, build a new message per attempt from a builder that holds the key.

```csharp Program.cs
// dotnet add package CSharpEssentials.Http
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using CSharpEssentials.Http;
using Polly;
using Polly.Retry;

int attempts = 0;
var server = new StubHandler(request =>
{
    attempts++;
    string key = request.Headers.TryGetValues("Idempotency-Key", out var values) ? values.Single() : "(none)";
    Console.WriteLine($"server: attempt {attempts}, {request.Method}, key {key}");
    return attempts == 1
        ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
        : new HttpResponseMessage(HttpStatusCode.Created) { Content = new StringContent("{}", Encoding.UTF8, "application/json") };
});
using var client = new HttpClient(server) { BaseAddress = new Uri("https://api.example.com") };

ResiliencePipeline<HttpResponseMessage> pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddRetry(new RetryStrategyOptions<HttpResponseMessage> { MaxRetryAttempts = 3, Delay = TimeSpan.FromMilliseconds(10) }
        .RetryUnsafeMethodsOnlyWithIdempotencyKey())
    .Build();

// With a key: the 503 is retried and both attempts carry the same key.
HttpRequestBuilder payment = HttpRequestBuilder.Post("/payments").WithJsonContent(new { Amount = 10 }).WithIdempotencyKey();
using HttpResponseMessage paid = await pipeline.ExecuteAsync(ct => new ValueTask<HttpResponseMessage>(client.SendAsync(payment.Build().Value, ct)));
Console.WriteLine($"with key: {(int)paid.StatusCode}");

// Without a key: a POST is not retried.
attempts = 0;
HttpRequestBuilder unsafePost = HttpRequestBuilder.Post("/payments").WithJsonContent(new { Amount = 10 });
using HttpResponseMessage notRetried = await pipeline.ExecuteAsync(ct => new ValueTask<HttpResponseMessage>(client.SendAsync(unsafePost.Build().Value, ct)));
Console.WriteLine($"without key: {(int)notRetried.StatusCode} after {attempts} attempt");

// IdempotencyKeyHandler adds a key to POST and PATCH that have none.
attempts = 1;
using var keyed = new HttpClient(new IdempotencyKeyHandler(server)) { BaseAddress = client.BaseAddress };
using HttpResponseMessage auto = await keyed.PostAsync("/orders", new StringContent("{}")); // the server prints a generated key

// Set a key by hand.
using var request = new HttpRequestMessage(HttpMethod.Patch, "https://api.example.com/orders/1");
string generated = request.SetIdempotencyKey();
request.SetIdempotencyKey("capture-order-1");
Console.WriteLine($"generated {generated.Length} chars, now {request.Headers.GetValues("Idempotency-Key").Single()}");

// The retry decision on its own.
var post = new HttpRequestMessage(HttpMethod.Post, "https://api.example.com/payments");
post.SetIdempotencyKey();
var busy = new HttpResponseMessage(HttpStatusCode.TooManyRequests) { RequestMessage = post };
busy.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(90));
Console.WriteLine(await IdempotentRetry.ShouldRetryAsync(post, busy, exception: null));                          // True
Console.WriteLine(await IdempotentRetry.ShouldRetryAsync(post, new HttpResponseMessage(HttpStatusCode.BadGateway), null)); // False
Console.WriteLine(IdempotentRetry.GetRetryAfter(busy, maxRetryAfter: TimeSpan.FromSeconds(30)));                   // 00:00:30

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

With `IHttpClientFactory` and `Microsoft.Extensions.Http.Resilience` (a package you add yourself):

```csharp
services.AddHttpClient("payments", c => c.BaseAddress = new Uri("https://payments.example.com"))
    .AddIdempotencyKeys()
    .AddStandardResilienceHandler(o => o.Retry.RetryUnsafeMethodsOnlyWithIdempotencyKey());
```

## SSRF guard

Use `AddSsrfGuard()` on a named or typed client that calls URLs a user, tenant or third party supplies: webhooks, link previews, image fetchers, import by URL. Without it, such a URL can reach `http://169.254.169.254/` (cloud metadata), `http://localhost:6379/` or an internal admin page. Available on .NET 9 and later.

What the guard does:

- Resolves DNS itself and checks every address a name returns. One blocked address rejects the request. The socket then connects to an address that was already checked, so DNS rebinding between check and connect does not work. IP literals such as `https://127.0.0.1/` and `https://[::1]/` get the same check.
- Allows only `https` on port 443 by default.
- Follows redirects itself (`AllowAutoRedirect` is off), up to `MaxRedirects` (3), and checks scheme, port and address again on every hop. Redirect rules are the same as in [Redirects](#redirects).
- Turns off the proxy and the cookie container, and never uses HTTP/3 (QUIC skips the connect check).
- Checks the final request once more right before the socket handler, so a handler added after `AddSsrfGuard` cannot change the URI to a blocked scheme or port, or switch to HTTP/3.
- Optionally limits response size and total time.

A blocked request throws `SsrfBlockedException`, an `HttpRequestException` with `Reason` (`RequestNotAllowed`, `HostNotAllowed`, `AddressNotAllowed`, `RedirectLimitExceeded`, `ResponseTooLarge`, `Timeout`), `RequestUri` and `BlockedAddress`. Its message never contains the resolved address, the query string or user info. The `*AsResultAsync` methods return it as `Forbidden` with code `Http.SsrfBlocked` and the reason under the `reason` metadata key. That includes `Timeout` and `ResponseTooLarge`, so check `reason` to tell a slow response from a blocked target.

```csharp Program.cs
// dotnet add package CSharpEssentials.Http
using System.Net;
using CSharpEssentials.Http;
using CSharpEssentials.ResultPattern;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddHttpClient("webhooks")
    .AddSsrfGuard(options =>
    {
        options.MaxResponseContentLength = 1_048_576; // 1 MB, checked while the body streams
        options.Timeout = TimeSpan.FromSeconds(10);   // all hops plus reading the body
    });
using ServiceProvider provider = services.BuildServiceProvider();
HttpClient client = provider.GetRequiredService<IHttpClientFactory>().CreateClient("webhooks");

// None of these targets is contacted: each is refused before a socket opens.
foreach (string target in new[] { "https://127.0.0.1/admin", "https://169.254.169.254/latest/meta-data", "http://example.com/hook" })
{
    Result result = await client.SendAsResultAsync(new HttpRequestMessage(HttpMethod.Get, target));
    Console.WriteLine($"{target} -> {result.FirstError.Code} ({result.FirstError.Metadata?["reason"]})");
}
// https://127.0.0.1/admin -> Http.SsrfBlocked (AddressNotAllowed)
// https://169.254.169.254/latest/meta-data -> Http.SsrfBlocked (AddressNotAllowed)
// http://example.com/hook -> Http.SsrfBlocked (RequestNotAllowed)

// Plain HttpClient calls throw the exception.
try
{
    await client.GetAsync("https://[::1]/");
}
catch (SsrfBlockedException ex)
{
    Console.WriteLine($"{ex.Reason} {ex.BlockedAddress} {ex.RequestUri}"); // AddressNotAllowed ::1 https://[::1]/
}

// The default policies can be called directly.
Console.WriteLine(DefaultOutboundAddressPolicy.Instance.IsAllowed(IPAddress.Parse("10.0.0.5"), new Uri("https://x/")));  // False
Console.WriteLine(DefaultOutboundAddressPolicy.Instance.IsAllowed(IPAddress.Parse("8.8.8.8"), new Uri("https://x/")));   // True
Console.WriteLine(new DefaultOutboundRequestPolicy(new SsrfGuardOptions()).IsAllowed(new Uri("https://example.com:8443/"))); // False
```

### Options

| `SsrfGuardOptions` | Default | Meaning |
|---|---|---|
| `AllowHttp` | `false` | Also allow `http` (adds port 80 to the default ports) |
| `AllowedPorts` | empty: 443, plus 80 with `AllowHttp` | Ports a request or redirect may use. Any entry replaces the defaults |
| `MaxRedirects` | `3` | Redirect hops. `0` makes any redirect throw (`RedirectLimitExceeded`) |
| `MaxResponseContentLength` | `null` | Byte limit, checked against `Content-Length` and while the body streams |
| `Timeout` | `null` | Total time for all hops plus reading the body |
| `AllowedNetworks`, `BlockedNetworks` | empty | `IPNetwork` exceptions, for example `IPNetwork.Parse("10.20.0.0/16")` |
| `AllowedHosts`, `BlockedHosts` | empty | Exact host or `*.example.com`. The wildcard matches subdomains only, not `example.com`. Case, a trailing dot and IDN form do not matter |
| `AddressPolicy` | `null` | `IOutboundAddressPolicy` for this client |
| `RequestPolicy` | `null` | `IOutboundRequestPolicy` for this client |

Precedence: block lists win over allow lists, and allow lists win over the address policy. An allowed host skips the address check for every address it resolves to, so use host entries (and wildcards above all) only for domains whose DNS you control.

### Blocked by default

IPv4: `0.0.0.0/8`, `10.0.0.0/8`, `100.64.0.0/10`, `127.0.0.0/8`, `169.254.0.0/16`, `172.16.0.0/12`, `192.0.0.0/24`, `192.0.2.0/24`, `192.88.99.0/24` (6to4 relay anycast), `192.168.0.0/16`, `198.18.0.0/15`, `198.51.100.0/24`, `203.0.113.0/24`, `224.0.0.0/4`, `240.0.0.0/4` (includes `255.255.255.255`).

IPv6: `::`, `::1`, `::ffff:0:0:0/96` (SIIT), `100::/64` (discard-only), `fc00::/7`, `fe80::/10`, `fec0::/10` (deprecated site-local), `ff00::/8`, `2001::/32` (Teredo), `2001:2::/48` (benchmarking), `2001:10::/28` (ORCHID), `2001:db8::/32` and `3fff::/20` (documentation), `5f00::/16` (SRv6 SIDs), `64:ff9b:1::/48` (local-use NAT64).

IPv6 forms that embed an IPv4 address are checked as that IPv4 address too: IPv4-mapped (`::ffff:a.b.c.d`), IPv4-compatible (`::a.b.c.d`), SIIT, NAT64 (`64:ff9b::/96`) and 6to4 (`2002::/16`). `::` and `::1` count as IPv6 addresses, not IPv4-compatible ones. `BlockedNetworks` matches the raw or the embedded address; `AllowedNetworks` matches only the raw address, so `0.0.0.0/0` does not allow `::1`. A custom address policy is asked about the embedded address and, when different, the raw one; both must pass.

### Custom rules

Implement `IOutboundAddressPolicy.IsAllowed(IPAddress address, Uri requestUri)` to decide which addresses may be reached, or `IOutboundRequestPolicy.IsAllowed(Uri requestUri)` to decide which URIs may be requested. The guard takes a policy from the options first, then from the container, then uses `DefaultOutboundAddressPolicy.Instance` or `new DefaultOutboundRequestPolicy(options)`.

```csharp
// One client.
services.AddHttpClient("webhooks").AddSsrfGuard(o => o.AddressPolicy = new MyAddressPolicy());

// Every guarded client in the container that sets no policy of its own.
services.AddSingleton<IOutboundAddressPolicy, MyAddressPolicy>();
```

Keep policies pure: the request policy runs twice per hop, and the address policy can run twice per address.

### Limits of the guard

- Not available on netstandard2.1 (it needs `SocketsHttpHandler.ConnectCallback`).
- Cannot be combined with an outbound proxy. Enforce the rules on the proxy instead.
- Do not replace the primary handler, or turn on a proxy or cookies, after `AddSsrfGuard`: the first request throws `InvalidOperationException`. To tune the socket handler, use `ConfigurePrimaryHttpMessageHandler((handler, _) => ...)` and cast `((DelegatingHandler)handler).InnerHandler` to `SocketsHttpHandler`.
- Only new connections are checked. Set `PooledConnectionLifetime` on that handler if DNS answers must be checked again over time.
- Synchronous `HttpClient.Send` throws `NotSupportedException`.
- A limit or timeout hit while a body is buffered (for example `LoadIntoBufferAsync`) can arrive as an `HttpRequestException` whose `InnerException` is the `SsrfBlockedException`. Walk the `InnerException` chain; the `*AsResultAsync` methods already do.
- 307 and 308 send the body again, also to another origin. Do not put secrets in the body of a request whose redirects you do not trust.
- Do not let a handler added after the guard send the request through another `HttpClient`.
- Do not retry `SsrfBlockedException`; the same request is blocked again. `IdempotentRetry` already skips it.

## Upgrade notes

- 9.0: a 422 response maps to `ErrorType.Domain` instead of `Validation`, and the retry helpers no longer retry it. See [v8 to v9](https://github.com/senrecep/CSharpEssentials/blob/main/docs/migration/v8-to-v9.md).
- 8.0: redirects follow only 301, 302, 303, 307 and 308, switch to GET as described above and strip credentials across origins. `HttpRequestBuilder` no longer disposes content passed to `WithContent`. An empty or malformed body, or a request value that cannot be serialized, fails with `Validation` instead of `Unexpected`. A literal `null` options argument is ambiguous because of the `JsonTypeInfo<T>` overloads.

## See also

- [Agent skill](https://github.com/senrecep/CSharpEssentials/blob/main/.well-known/agent-skills/csharpessentials-http/SKILL.md)
- [Example project](https://github.com/senrecep/CSharpEssentials/tree/main/examples/Examples.Http)
- [CSharpEssentials.Resilience](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Resilience/Readme.MD) for the policies behind the retry helpers
- [CSharpEssentials.Idempotency](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Idempotency/Readme.MD) for the server side of `Idempotency-Key`
- [CSharpEssentials.Results](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD) and [CSharpEssentials.Errors](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Errors/Readme.MD)
