# Migrating idempotency from 6.x to 7.0

7.0 moves the idempotency store out of `CSharpEssentials.AspNetCore` into the new `CSharpEssentials.Idempotency` package, so message consumers and jobs can use the same store as the `Idempotency-Key` middleware. `CSharpEssentials.AspNetCore` depends on it; you do not add a package reference for the middleware.

## Breaking changes

- `IIdempotencyStore`, `IdempotencyReservation`, `IdempotencyReservationStatus`, `InMemoryIdempotencyStore` and `DistributedCacheIdempotencyStore` are removed from `CSharpEssentials.AspNetCore`. There is no `[Obsolete]` bridge; use the types in the table below.
- `IdempotencyOptions.KeyScope` (`Func<HttpContext, string?>`) and `IdempotencyOptions.DefaultKeyScope` are removed. Use `IdempotencyOptions.ScopeResolver`.
- Stored records use a new key format (`idem:v3:` prefix) and a new payload format (version 3). Records written by 6.x are not read. See [Stored records](#stored-records).
- `IdempotencyOptions.ShouldStore` is a `Func<IdempotencyOutcome, bool>` instead of a `Func<int, bool>`. `IdempotencyOutcome` carries `StatusCode` and `HandlerStarted`.
- `IdempotencyOptions.AllowUnscopedKeys` is removed. Use `UnscopedKeyBehavior`.
- What happens to a key after the request depends on whether the handler started, and the default policy stores more statuses. See [Behavior changes](#behavior-changes).

`UseInMemoryStore()`, `UseDistributedCacheStore()`, `UseStore<TStore>(lifetime)` and the other options keep their names and behavior.

## Type mapping

Old types are in namespace `CSharpEssentials.AspNetCore`, new types in `CSharpEssentials.Idempotency`.

| 6.x | 7.0 |
|---|---|
| `IIdempotencyStore` | `IIdempotencyRecordStore` |
| `ValueTask<IdempotencyReservation> TryReserveAsync(string key, string fingerprint, TimeSpan inFlightTimeout, CancellationToken)` | `ValueTask<IdempotencyRecordReservation> TryReserveAsync(IdempotencyRecordKey key, string fingerprint, TimeSpan lease, CancellationToken)` |
| `ValueTask<bool> CompleteAsync(string key, string token, string fingerprint, IdempotentResponse response, TimeSpan retention, CancellationToken)` | `ValueTask<bool> CompleteAsync(IdempotencyRecordKey key, string token, string fingerprint, ReadOnlyMemory<byte> payload, TimeSpan retention, CancellationToken)` |
| `ValueTask<bool> ReleaseAsync(string key, string token, CancellationToken)` | `ValueTask<bool> ReleaseAsync(IdempotencyRecordKey key, string token, CancellationToken)` |
| `IdempotencyReservation` (`Status`, `Token`, `IdempotentResponse? Response`) | `IdempotencyRecordReservation` (`Status`, `Token`, `ReadOnlyMemory<byte>? Payload`); same factories, `Completed(payload)` takes the bytes |
| `IdempotencyReservationStatus` | `IdempotencyReservationStatus`, same values, new namespace |
| `InMemoryIdempotencyStore` | `InMemoryIdempotencyRecordStore` |
| `DistributedCacheIdempotencyStore` (`KeyPrefix = "idempotency:"`) | `DistributedCacheIdempotencyRecordStore` (prefix `idem:v3:`, configurable through the constructor) |
| The string key built by the middleware, `"{scope.Length}:{scope}:{key}"` or `"-:{key}"` | `IdempotencyRecordKey` (`Namespace`, `Scope`, `Key`); `ToStorageKey(prefix)` gives the length-prefixed storage key |
| `KeyScope = DefaultKeyScope` | `ScopeResolver = IdempotencyScopes.User` (the default) |
| `KeyScope = context => ...` | `ScopeResolver = IdempotencyScopes.Custom(user => ...)`, or a ready resolver: `Tenant(claimType)`, `UserAndTenant(claimType)`, `Client`, `Global` |
| `ShouldStore: Func<int, bool>` (namespace `CSharpEssentials.AspNetCore`) | `ShouldStore: Func<IdempotencyOutcome, bool>` (`StatusCode`, `HandlerStarted`), same namespace. The 6.x behavior is `IdempotencyStorePolicies.SuccessOnly` |
| `AllowUnscopedKeys = true` | `UnscopedKeyBehavior = UnscopedKeyBehavior.Global` |
| `AllowUnscopedKeys = false` (the default) | `UnscopedKeyBehavior = UnscopedKeyBehavior.Skip` (the default), which now logs a warning |

`IdempotentResponse` stays in `CSharpEssentials.AspNetCore`. The middleware serializes it into the payload, so a store no longer sees it.

## Scopes

`ScopeResolver` receives `HttpContext.User`, not the whole `HttpContext`. A 6.x `KeyScope` that read a claim becomes `IdempotencyScopes.Custom(user => user?.FindFirst("org_id")?.Value)`. A scope that came from a header or a route value has to become a claim first, for example in an `IClaimsTransformation`.

A resolver returns `null` when it cannot resolve a scope. What happens then is set by `UnscopedKeyBehavior`: `Skip` (the default) runs the request without idempotency, as in 6.x, and logs a warning; `Reject` returns 400 with code `Idempotency.ScopeRequired`; `Global` uses a global key, like 6.x `AllowUnscopedKeys = true`. Machine callers that authenticate with client credentials have no user claim; use `IdempotencyScopes.Client` for them. `IdempotencyScopes.Global` is a different answer: it shares the key between all callers on purpose. A global key never matches a scoped one.

The new `Namespace` option (default `http`) separates the middleware's records from records written by other code that shares the store.

## Behavior changes

The middleware now knows whether the endpoint's handler started. `.WithIdempotency()` adds an endpoint filter on Minimal API endpoints and groups, and `[Idempotent]` is an action filter on MVC actions; each marks the handler as started right before it runs. Anything that answers before that point, such as a rate limiter, load shedding, authorization, a binding failure or MVC model validation, leaves the handler not started.

| Outcome | 6.x | 7.0 |
|---|---|---|
| 2xx | Stored for `RetentionPeriod` | Same |
| 3xx from the handler | Released | Stored for `RetentionPeriod` |
| 4xx | Released | Same |
| 5xx from the handler (other than 503 and 504) | Released | Stored for `ServerErrorRetention` (10 minutes), then a retry runs the handler again |
| 503 or 504 from the handler (`NeverStoreStatusCodes`) | Released | Stays in flight until `InFlightTimeout`; a retry gets 409, then runs the handler once more |
| Any status before the handler started (429 from a rate limiter, 503 from load shedding) | Released (unless 2xx) | Released |
| Exception or abort after the handler started | Released, unless the response had started | Stays in flight until `InFlightTimeout` |
| Exception before the handler started | Released | Same |
| Handler ignores the abort and returns | Stored | Same |
| Response that cannot be stored (`Content-Encoding` set by the handler, `text/event-stream`, body larger than `MaxResponseBodySize`) | Released | Released on 4xx only; any other status stays in flight until `InFlightTimeout` |
| Request with a key but no scope | Ran without idempotency | Same, and a warning `Idempotency.UnscopedKeySkipped` is logged (the key itself is never logged) |

- **5xx is stored.** A handler that fails after its side effect would run twice if the 500 released the key. Applications that map business errors to 500 now replay that error for `ServerErrorRetention`. Set `ServerErrorRetention = RetentionPeriod` for the strictest behavior on payment endpoints, or keep the default so clients are not locked out of a key for a full day.
- **503, 504, exceptions and aborts stay in flight.** The handler may have done its work, so the key is neither stored nor released. A retry within `InFlightTimeout` gets 409 with `Retry-After`; after it, the handler runs exactly once more.
- **Responses that cannot be stored stay in flight** unless they are 4xx, for the same reason. Before 7.0 they released the key.
- **`IdempotencyStorePolicies.SuccessOnly`** restores the 6.x storing rule: it stores 2xx only and releases 3xx and 5xx, so a handler that fails after its side effect can run again on retry. `NeverStoreStatusCodes`, exceptions and aborts still keep the key in flight under this policy.
- **3xx is stored**, for example a `303 See Other` that points to the created resource.
- **`Retry-After` is replayed**: it is in the default `ReplayedHeaders`.
- **An idempotent endpoint without the filter** (for example a `RequestDelegate` endpoint given `IdempotentAttribute` metadata by hand, or an MVC action opted in through `WithIdempotency()` without `[Idempotent]`) cannot tell a handler failure from an earlier one, so every outcome releases the key and a warning `Idempotency.ExecutionFilterMissing` is logged once per endpoint. Set `RequireExecutionFilter = true` to throw instead.
- **A request timeout longer than `InFlightTimeout`** logs `Idempotency.RequestTimeoutExceedsLease` once per endpoint: a retry after the lease could run while the first request is still running.

New startup validation in `AddIdempotency`:

- `InFlightTimeout` must not exceed `RetentionPeriod` or `MaxInFlightTimeout` (15 minutes).
- `ServerErrorRetention` must be greater than zero and not exceed `RetentionPeriod`.
- `NeverStoreStatusCodes` must not be empty, and every code must be between 100 and 599.

## Custom stores

A store keeps its atomicity and token rules. Two things change:

1. Build the storage key with `key.ToStorageKey()` (or `key.ToStorageKey(prefix)`) instead of using the string the middleware passed.
2. Store the payload bytes as they are, instead of an `IdempotentResponse`, and return them with `IdempotencyRecordReservation.Completed(payload)`.

```csharp
// 6.x
public async ValueTask<bool> ReleaseAsync(string key, string token, CancellationToken cancellationToken = default) =>
    await _db.ExecuteAsync("DELETE FROM idempotency WHERE key = @key AND token = @token AND state = 'in-flight'",
        new { key, token }, cancellationToken) == 1;

// 7.0
public async ValueTask<bool> ReleaseAsync(IdempotencyRecordKey key, string token, CancellationToken cancellationToken = default) =>
    await _db.ExecuteAsync("DELETE FROM idempotency WHERE key = @key AND token = @token AND state = 'in-flight'",
        new { key = key.ToStorageKey(), token }, cancellationToken) == 1;
```

Then run the contract tests from `CSharpEssentials.Idempotency.Testing` against it:

```csharp
public sealed class SqlIdempotencyRecordStoreTests : IdempotencyRecordStoreContractTests
{
    protected override IIdempotencyRecordStore CreateStore(TimeProvider timeProvider) =>
        new SqlIdempotencyRecordStore(ConnectionString, timeProvider);
}
```

Set `UseRealClock => true` for a store whose expiry runs on its own clock (Redis TTL, a database `now()`), and `IsReservationAtomic => false` for a best-effort store. See the [Testing package Readme](../../CSharpEssentials.Idempotency.Testing/Readme.MD).

## Stored records

- Storage keys start with `idem:v3:` (`IdempotencyRecordKey.DefaultPrefix`) and contain the namespace, so 7.0 does not see the records 6.x wrote under `idempotency:`. Those expire with their 6.x retention.
- The middleware writes its responses as payload version 3 and does not read version 2. If a custom store keeps its old keys and returns a 6.x record as a completed payload, the middleware answers that key with `500` and logs an error, without running the handler, until the record expires. Use a new key prefix, or clear the table, when you upgrade a custom store.
- During a rolling deploy, 6.x and 7.0 instances run side by side without seeing each other's records. A retry that reaches a 7.0 instance after the first attempt ran on a 6.x instance runs again. For operations that must not run twice, such as payments, drain traffic before switching or deploy outside the retry window of your clients.
