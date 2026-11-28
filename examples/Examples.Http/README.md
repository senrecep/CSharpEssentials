# Examples.Http

Console app that runs every feature of [CSharpEssentials.Http](../../CSharpEssentials.Http/Readme.MD). A fake `HttpMessageHandler` (`FakeApi` at the bottom of `Program.cs`) answers in memory, and the SSRF section only uses targets the guard refuses before it opens a socket, so the app needs no network.

## Run

```bash
cd examples/Examples.Http
dotnet run
```

## What each section shows

1. The `Result` calls on `HttpClient`: `GetFromJsonAsResultAsync`, `PostAsJsonAsResultAsync`, `PutAsJsonAsResultAsync`, `PatchAsJsonAsResultAsync`, `DeleteAsResultAsync`, `PostAsResultAsync`, `PutAsResultAsync` and both `SendAsResultAsync` forms.
2. How failures come back: status codes mapped by `HttpStatusCodeMapper` (422 is `Domain`), a `null` body, a broken body, a typed call on 204, a refused connection, `HttpClient.Timeout` as `Http.Timeout`, and a cancelled caller token that throws.
3. `ReadFromJsonAsResultAsync` and `ReadAsStringAsResultAsync` on an `HttpContent`.
4. `HttpRequestBuilder`: route values, query values (single, collection, dictionary, null skipped), headers, JSON body, a `Patch` call, your own `HttpContent` sent twice, `Build()` and the validation errors.
5. `[StringEnum]` and `[Flags]` enums in route and query, numbers with `EnumWireFormat.Number`, and an undefined value rejected.
6. Query string helpers: `WithQueryString` with a pair, an enum, a dictionary and an object, plus `ToQueryString`.
7. The `JsonTypeInfo<T>` overloads for trimming and Native AOT.
8. `SendWithRedirectsAsResultAsync` and `FollowRedirects`, the redirect limit, and a cross-origin redirect refused because `DefaultRequestHeaders` holds `Authorization`.
9. `HttpClientResilienceExtensions`: a policy and a pipeline retrying 503 responses, a 404 that is not retried, the timeout policy, and the circuit breaker, timeout and combined pipelines.
10. Idempotency: a Polly retry with `RetryUnsafeMethodsOnlyWithIdempotencyKey` that retries a keyed POST with the same key and leaves an unkeyed POST alone, a connection reset retried through `IdempotentRetry.RequestMessageKey`, `IdempotencyKeyHandler`, `AddIdempotencyKeys`, `SetIdempotencyKey`, `IdempotentRetry.ShouldRetryAsync` and `GetRetryAfter`.
11. SSRF guard: `AddSsrfGuard` blocking loopback, cloud metadata, an IPv4-mapped private address, plain http and a blocked host; a custom `IOutboundRequestPolicy`; a client with `AllowedPorts`, `AllowedHosts`, `AllowedNetworks`, `BlockedNetworks` and a custom `IOutboundAddressPolicy`; `SsrfBlockedException` from a plain `GetAsync`; and the default policies called directly.

The fake handler does not follow redirects by itself. A real `HttpClientHandler` or `SocketsHttpHandler` does, so set `AllowAutoRedirect = false` on it before relying on section 8.
