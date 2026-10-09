# CSharpEssentials.Http

`HttpClient` extensions that return `Result<T>` instead of throwing exceptions. Built-in status-code-to-error mapping, fluent request builder, Polly resilience, and redirect handling.

## Features

- **Result-based HTTP**: `GetFromJsonAsResultAsync`, `PostAsJsonAsResultAsync`, `PutAsJsonAsResultAsync`, `PatchAsJsonAsResultAsync`, `DeleteAsResultAsync`.
- **Automatic Error Mapping**: HTTP codes mapped to `ErrorType` (Validation, Unauthorized, NotFound, Conflict, etc.).
- **Fluent Request Builder**: Chain headers, query params, JSON content, and redirects.
- **Polly Integration**: Retry, timeout, and circuit breaker pipelines.
- **Query String Helpers**: Build URIs from dictionaries or anonymous objects.
- **SSRF Guard** (.NET 9+, not available on netstandard2.1): `AddSsrfGuard()` blocks requests to private, loopback, link-local and metadata addresses, re-validated on every redirect hop.

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

### Errors, timeouts and cancellation

The `HttpClient` extensions above, `PostAsResultAsync`, `PutAsResultAsync`, `SendAsResultAsync`, `SendWithRedirectsAsResultAsync`, `HttpRequestBuilder.AsResultAsync` and the `HttpContent` readers (`ReadAsStringAsResultAsync`, `ReadFromJsonAsResultAsync<T>`) follow the `Result.TryAsync` rule: only your own token propagates.

- Cancelling the `cancellationToken` you pass throws `OperationCanceledException`.
- An HTTP timeout, that is a `TaskCanceledException` whose `InnerException` is a `TimeoutException` (what `HttpClient.Timeout` throws), returns a failed `Result` with `ErrorType.Unexpected` and code `Http.Timeout`.
- A Polly attempt or total timeout from `Microsoft.Extensions.Http.Resilience` or another Polly handler surfaces as `TimeoutRejectedException`, so it keeps `ErrorType.Unexpected` with code `TimeoutRejectedException`, not `Http.Timeout`.
- Any other cancellation, for example a token a handler links or owns, returns a failed `Result` with `ErrorType.Unexpected` and the exception type name as code.
- Other transport exceptions return `ErrorType.Unexpected` with the exception type name as code; an SSRF guard block returns `Http.SsrfBlocked` (see below).

**Behaviour change in 6.5.0:** before, an `HttpClient.Timeout` or any other cancelled token threw `OperationCanceledException`. Handle the failed `Result` instead; check for the `Http.Timeout` code to tell a timeout apart.

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

### Enums in route, query and body

Enums with `[StringEnum]` are written as wire names; `WithEnumConventions(conventions, EnumWireFormat.Number)` writes numbers for servers that accept only integers. Flags and enum collections become repeated query keys, and an undefined value is never sent.

```csharp
Result<Order> result = await HttpRequestBuilder
    .Get("https://api.example.com/orders/{status}")
    .WithEnumConventions(EnumConventions.Default)
    .WithRoute("status", OrderStatus.PendingApproval)          // /orders/pending_approval
    .WithQuery("permissions", Permissions.Read | Permissions.Write) // ?permissions=read&permissions=write
    .AsResultAsync<Order>(client, options);
```

Build `options` once with `new JsonSerializerOptions(JsonSerializerDefaults.Web).AddEnumConventions(conventions, EnumReadMode.Data, writeAs)`. Responses are then read tolerantly: wire names, member names, aliases and numbers, with the `[EnumFallback]` member for values a newer server adds. Plain enums without `[StringEnum]` keep the framework behavior.

Roll out consumers before producers: a server moves its output from `Number` to `String` only after every client reads names.

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
    await client.GetFromJsonAsResultAsync<User>(new Uri("/users/1", UriKind.Relative), cancellationToken: token));
```

### Redirect Following

```csharp
Result<User> result = await HttpRequestBuilder
    .Get("/legacy-url")
    .FollowRedirects(maxRedirects: 3)
    .AsResultAsync<User>(client);
```

### SSRF Guard

Use it when the URL comes from a user, a tenant or a third party: webhooks, link previews, image fetchers, import-by-URL. Without a guard, a caller can point your server at `http://169.254.169.254/` (cloud metadata), `http://localhost:6379/` or an internal admin panel. The response then leaks back to them, or the request alone does damage.

```csharp
services.AddHttpClient("webhooks")
    .AddSsrfGuard(options =>
    {
        options.MaxResponseContentLength = 1_048_576; // 1 MB, enforced while streaming
        options.Timeout = TimeSpan.FromSeconds(10);
    });
```

What the guard does:

- **Checks resolved addresses, not host names.** A `ConnectCallback` on `SocketsHttpHandler` resolves DNS itself and checks every address the name returns. If any address is blocked, the request fails, so a name that resolves to `[8.8.8.8, 10.0.0.5]` is rejected. The socket connects to an address that was already checked, so DNS rebinding between the check and the connect does not work. IP literals (`http://127.0.0.1/`, `http://[::1]/`) go through the same check.
- **Follows redirects itself.** `AllowAutoRedirect` is off. The guard follows 301/302/303/307/308 itself (default 3 hops) and checks scheme, port and address again on every hop. 303, and 301/302 after a POST, become GET with no body. 307/308 keep the method and body. `Authorization`, `Proxy-Authorization` and `Cookie` are removed when a redirect changes scheme, host or port.
- **Disables proxies and the cookie container.** A proxy would resolve the name instead of the guard, so the guard turns off `UseProxy`. `UseCookies` is off, so cookies set by one host are never sent to another. If the primary handler is replaced or reconfigured with a proxy or cookies after `AddSsrfGuard`, the first request throws `InvalidOperationException`. The primary handler is an internal wrapper around that `SocketsHttpHandler`; to tune it, cast `((DelegatingHandler)handler).InnerHandler` to `SocketsHttpHandler`.
- **Never uses HTTP/3.** HTTP/3 runs over QUIC, which does not go through `ConnectCallback`, so it would skip the address check. On every hop the guard caps `request.Version` at HTTP/2 and turns `RequestVersionOrHigher` into `RequestVersionOrLower`, which also rules out an `Alt-Svc` upgrade.
- **Checks the final request again at the primary handler.** Handlers added after `AddSsrfGuard` run between the guard and the socket handler. The primary handler applies the HTTP/2 cap and the request policy (scheme, port, `RequestPolicy`) once more on the request it actually sends, so such a handler cannot switch to HTTP/3 or rewrite the URI to a blocked scheme or port.
- **Allows only https on port 443 by default.** Set `AllowHttp = true` to allow http (port 80 is then added). Adding entries to `AllowedPorts` replaces the default ports.

Blocked by default: `0.0.0.0/8`, `10.0.0.0/8`, `100.64.0.0/10`, `127.0.0.0/8`, `169.254.0.0/16`, `172.16.0.0/12`, `192.0.0.0/24`, `192.0.2.0/24`, `192.88.99.0/24` (6to4 relay anycast), `192.168.0.0/16`, `198.18.0.0/15`, `198.51.100.0/24`, `203.0.113.0/24`, `224.0.0.0/4`, `240.0.0.0/4`, `255.255.255.255`, `::`, `::1`, `::ffff:0:0:0/96` (SIIT IPv4-translated), `100::/64` (discard-only), `fc00::/7`, `fe80::/10`, `fec0::/10` (deprecated site-local), `ff00::/8`, `2001::/32` (Teredo), `2001:2::/48` (benchmarking), `2001:10::/28` (ORCHID), `2001:db8::/32`, `3fff::/20` (documentation), `5f00::/16` (SRv6 SIDs), `64:ff9b:1::/48` (local-use NAT64). IPv6 addresses that embed one of the IPv4 ranges are blocked too: IPv4-mapped (`::ffff:a.b.c.d`), IPv4-compatible (`::a.b.c.d`), SIIT (`::ffff:0:a.b.c.d`), NAT64 (`64:ff9b::/96`) and 6to4 (`2002::/16`). `::` and `::1` are IPv6 addresses in their own right and are not treated as IPv4-compatible. `BlockedNetworks` matches either the raw address or the embedded IPv4 address. `AllowedNetworks` matches only the raw address, so `0.0.0.0/0` does not allow `::1`. The address policy is asked about the embedded IPv4 address and, when it differs, the raw address too; both must be allowed.

| Option | Default | Meaning |
|--------|---------|---------|
| `AllowHttp` | `false` | Allow `http` in addition to `https` |
| `AllowedPorts` | empty (443, plus 80 with `AllowHttp`) | Ports a request or redirect may target |
| `MaxRedirects` | `3` | Redirect hops; `0` means any redirect throws `SsrfBlockedException` (the 3xx response is not returned) |
| `MaxResponseContentLength` | `null` | Byte limit, checked against `Content-Length` and while the body streams |
| `Timeout` | `null` | Total time for all hops plus reading the body |
| `AllowedNetworks` / `BlockedNetworks` | empty | `IPNetwork` exceptions, for example `IPNetwork.Parse("10.20.0.0/16")` |
| `AllowedHosts` / `BlockedHosts` | empty | Host exceptions, exact or `*.example.com`. `*.example.com` matches subdomains only, not `example.com` itself; add both if you need both. Case, a trailing dot and IDN (Unicode or `xn--` form) do not matter |
| `AddressPolicy` | `null` | `IOutboundAddressPolicy` for this client only |
| `RequestPolicy` | `null` | `IOutboundRequestPolicy` for this client only |

Block lists win over allow lists, and allow lists win over the address policy. A blocked request throws `SsrfBlockedException`, an `HttpRequestException` with a `Reason` of `RequestNotAllowed`, `HostNotAllowed`, `AddressNotAllowed`, `RedirectLimitExceeded`, `ResponseTooLarge` or `Timeout`. The message never contains the resolved address, the query string or user info; read `BlockedAddress` and the full `RequestUri` from the exception when you need them for logs. The `*AsResultAsync` extensions turn it into a failed `Result` with `ErrorType.Forbidden`, code `Http.SsrfBlocked` and the `Reason` under the `reason` metadata key. This applies to every reason, including `Timeout` and `ResponseTooLarge`, so check the `reason` metadata to tell a slow or oversized response from a blocked target.

A limit or timeout hit while the body is buffered (for example `LoadIntoBufferAsync`) can surface as an `HttpRequestException` whose `InnerException` is the `SsrfBlockedException`. Walk the `InnerException` chain instead of catching only `SsrfBlockedException`; the `*AsResultAsync` extensions already do this.

To replace the rules, give your own `IOutboundAddressPolicy` (which IP addresses may be reached) or `IOutboundRequestPolicy` (which URIs may be requested). The guard takes the policy from the options first, then from the container, then falls back to the built-in default:

```csharp
// One client only
services.AddHttpClient("webhooks")
    .AddSsrfGuard(options => options.AddressPolicy = new MyAddressPolicy());

// Every guarded client in the container that does not set its own policy
services.AddSingleton<IOutboundAddressPolicy, MyAddressPolicy>();
```

Limitations:

- The guard is not available on `netstandard2.1`, because it needs `SocketsHttpHandler.ConnectCallback`.
- It cannot be combined with an outbound proxy. If you have to use a proxy, enforce the rules on the proxy.
- A custom policy registered in the container applies to every guarded client in that container. Use `AddressPolicy`/`RequestPolicy` on the options to scope a policy to one client.
- Policies should be pure and idempotent: the request policy runs twice per hop (in the guard and again in the primary handler), and the address policy may run for both the raw and the normalized form of an address.
- HTTP/3 is never used, because QUIC connections bypass `ConnectCallback`. The cap is enforced at the primary handler, so it also holds when a handler added after `AddSsrfGuard` raises the version. Such a handler still sees the request before the final check, so do not let it send the request through another `HttpClient`.
- Only new connections are checked; a pooled connection was checked when it opened. Configure the primary handler with `ConnectTimeout` and `PooledConnectionLifetime` (for example through `ConfigurePrimaryHttpMessageHandler((handler, _) => ...)`) if DNS answers must be re-checked periodically.
- Synchronous `HttpClient.Send` is not supported. A synchronous `Read` on the response stream checks the timeout only before each read, and throws the same `SsrfBlockedException` as the asynchronous path.
- A 307/308 redirect sends the request body again, also to another origin, the same as the default .NET redirect handling. Bodies that can only be read once (forward-only streams) fail on that hop. Do not put secrets in the body of a request whose redirects you do not trust.
- A wildcard in `AllowedHosts` skips the address check for every name under it. Use it only for domains whose DNS you control; otherwise anyone who can add a record there can point it at an internal address.
- Retry and resilience handlers should not retry `SsrfBlockedException`: the same request is blocked again. Exclude it from the handled exceptions (for example `ShouldHandle` in Polly).
