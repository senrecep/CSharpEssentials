# Migrating idempotency from 6.x to 7.0

7.0 moves the idempotency store out of `CSharpEssentials.AspNetCore` into the new `CSharpEssentials.Idempotency` package, so message consumers and jobs can use the same store as the `Idempotency-Key` middleware. `CSharpEssentials.AspNetCore` depends on it; you do not add a package reference for the middleware.

## Breaking changes

- `IIdempotencyStore`, `IdempotencyReservation`, `IdempotencyReservationStatus`, `InMemoryIdempotencyStore` and `DistributedCacheIdempotencyStore` are removed from `CSharpEssentials.AspNetCore`. There is no `[Obsolete]` bridge; use the types in the table below.
- `IdempotencyOptions.KeyScope` (`Func<HttpContext, string?>`) and `IdempotencyOptions.DefaultKeyScope` are removed. Use `IdempotencyOptions.ScopeResolver`.
- Stored records use a new key format (`idem:v3:` prefix) and a new payload format (version 3). Records written by 6.x are not read. See [Stored records](#stored-records).
- `IdempotencyOptions.ShouldStore` is a `Func<IdempotencyOutcome, bool>` instead of a `Func<int, bool>`. `IdempotencyOutcome` carries `StatusCode` and `HandlerStarted`.
- `IdempotencyOptions.AllowUnscopedKeys` is removed. Use `UnscopedKeyBehavior`.
- `IdempotencyOptions` resolved from DI is transient and reflects the current options, instead of a singleton instance. See [Options at runtime](#options-at-runtime).
- `IdempotencyOptions.RetryAfter` is a non-nullable `TimeSpan` (default 1 second, must be positive). The `Retry-After` header on the 409 can no longer be omitted.
- A key must be 1 to `MaxKeyLength` printable ASCII characters other than space, `"` and `\` (`IdempotencyKeys.IsValid`). Keys with other characters that 6.x accepted now return 400 `Idempotency.KeyInvalid`.
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
| A raw scope string, such as `"user:42"` or `IdempotencyScope.Of(...)` in a 7.0 pre-release, and `new IdempotencyRecordKey(ns, "user:42", key)` | A factory: `IdempotencyScope.User("42")`, `Tenant(id)`, `Client(id)`, `UserAndTenant(userId, tenantId)`, `Custom(kind, value)` or `Global`, and `new IdempotencyRecordKey(ns, IdempotencyScope.User("42"), key)`. The storage key is unchanged |
| `KeyScope = DefaultKeyScope` | `ScopeResolver = IdempotencyScopes.User` (the default) |
| `KeyScope = context => ...` | `ScopeResolver = IdempotencyScopes.Custom("kind", user => ...)`, or a ready resolver: `Tenant(claimType)`, `UserAndTenant(claimType)`, `Client`, `Global` |
| `ShouldStore: Func<int, bool>` (namespace `CSharpEssentials.AspNetCore`) | `ShouldStore: Func<IdempotencyOutcome, bool>` (`StatusCode`, `HandlerStarted`), same namespace. The 6.x behavior is `IdempotencyStorePolicies.SuccessOnly` |
| `AllowUnscopedKeys = true` | `UnscopedKeyBehavior = UnscopedKeyBehavior.Global` |
| `AllowUnscopedKeys = false` (the default) | `UnscopedKeyBehavior = UnscopedKeyBehavior.Skip` (the default), which now logs a warning |

`IdempotentResponse` stays in `CSharpEssentials.AspNetCore`. The middleware serializes it into the payload, so a store no longer sees it.

## Scopes

`ScopeResolver` receives `HttpContext.User`, not the whole `HttpContext`. A 6.x `KeyScope` that read a claim becomes `IdempotencyScopes.Custom("org", user => user?.FindFirst("org_id")?.Value)`. A scope that came from a header or a route value has to become a claim first, for example in an `IClaimsTransformation`.

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
| Handler ignores the abort and returns | Stored | Stored; if writing the response fails after the abort, the key stays in flight until `InFlightTimeout` |
| Response that cannot be stored (`Content-Encoding` set by the handler, `text/event-stream`, body larger than `MaxResponseBodySize`) | Released | Released on 4xx only; any other status stays in flight until `InFlightTimeout` |
| Request with a key but no scope | Ran without idempotency | Same, and a warning `Idempotency.UnscopedKeySkipped` is logged (the key itself is never logged) |

- **5xx is stored.** A handler that fails after its side effect would run twice if the 500 released the key. Applications that map business errors to 500 now replay that error for `ServerErrorRetention`. Set `ServerErrorRetention = RetentionPeriod` for the strictest behavior on payment endpoints, or keep the default so clients are not locked out of a key for a full day.
- **503, 504, exceptions and aborts stay in flight.** The handler may have done its work, so the key is neither stored nor released. A retry within `InFlightTimeout` gets 409 with `Retry-After`; after it, the handler runs exactly once more.
- **Responses that cannot be stored stay in flight** unless they are 4xx, for the same reason. Before 7.0 they released the key.
- **`IdempotencyStorePolicies.SuccessOnly`** restores the 6.x storing rule: it stores 2xx only and releases 3xx and 5xx, so a handler that fails after its side effect can run again on retry. `NeverStoreStatusCodes`, exceptions and aborts still keep the key in flight under this policy.
- **3xx is stored**, for example a `303 See Other` that points to the created resource.
- **`Retry-After` is replayed**: it is in the default `ReplayedHeaders`.
- **An idempotent endpoint without the filter** (for example a `RequestDelegate` endpoint given `IdempotentAttribute` metadata by hand, or an MVC action opted in through `WithIdempotency()` without `[Idempotent]`) cannot tell a handler failure from an earlier one. It keeps the 6.x storing rule: 2xx is stored, and every other outcome, including 3xx, 5xx, 503/504 and exceptions before the response started, releases the key. A 2xx that cannot be stored keeps the key in flight. A warning `Idempotency.ExecutionFilterMissing` is logged once per endpoint; set `RequireExecutionFilter = true` to throw instead. Add `.WithIdempotency()` or `[Idempotent]` to get the rest of the table above.
- **A request timeout longer than `InFlightTimeout`** logs `Idempotency.RequestTimeoutExceedsLease` once per endpoint: a retry after the lease could run while the first request is still running.

New startup validation in `AddIdempotency`:

- `InFlightTimeout` must not exceed `RetentionPeriod` or `MaxInFlightTimeout` (15 minutes).
- `ServerErrorRetention` must be greater than zero and not exceed `RetentionPeriod`.
- `NeverStoreStatusCodes` must not be empty, and every code must be between 100 and 599.

## Options at runtime

- `IdempotencyOptions` are read through `IOptionsMonitor<IdempotencyOptions>` once per request. `AddIdempotency(configure)` registers `configure` as an `IConfigureOptions<IdempotencyOptions>`, so `Configure<IdempotencyOptions>` and `Bind` calls apply after it, and a reload of a bound configuration section reaches the next request. Calling `AddIdempotency` again still replaces the delegate of the earlier call.
- `IdempotencyOptions` resolved from DI is now transient: each resolution returns the monitor's current value instead of one singleton instance. Changes made to a resolved instance are lost on the next reload and may not reach endpoints whose settings were already resolved; configure the options through `AddIdempotency` or `Configure<IdempotencyOptions>` instead.
- A reload that makes the options invalid throws `OptionsValidationException` on every request to an idempotent endpoint until it is fixed. The store is chosen in `AddIdempotency`; a reload does not change it.
- New `IdempotencyOptions.Mode` (`Enforce` by default, so nothing changes without it): `Observe` stores as `Enforce` does but never answers differently from `Off`, and `Off` ignores the header. Endpoints and profiles override it with `IdempotencyEndpointOptions.Mode` or `[Idempotent(Mode = ...)]`. It does not prevent duplicates in `Observe`; keep that phase short for payments.
- Every idempotency problem (`Idempotency.KeyMissing`, `KeyInvalid`, `InFlight`, `KeyReused`, `ScopeRequired`) is `application/problem+json` with a top-level `code` member and `errorCodes`. Clients can branch on `code`: `KeyReused` is 422, `InFlight` is 409 with `Retry-After`, the others are 400.
- A key sent as a structured-field string (`"abc"`) is accepted and names the same record as the bare `abc`. Malformed quoting, several headers or an empty value return 400 `Idempotency.KeyInvalid`.
- `AddSwagger` in `CSharpEssentials.AspNetCore.Swashbuckle` now calls `AddIdempotencyConventions()`, so generated documents describe the `Idempotency-Key` header and the problem responses of idempotent endpoints. Regenerate any checked-in OpenAPI snapshot.
- Start new endpoints in `Mode = Observe` and switch to `Enforce` after the decision counters look right; see the [rollout steps](../../CSharpEssentials.Idempotency/Readme.MD#rollout-observe--enforce).
- For operations where a duplicate must be impossible (payments), the EF Core store (`ITransactionalIdempotencyRecordStore`) commits the key with your data; see [Transactional store for money movement](../../CSharpEssentials.Idempotency/Readme.MD#transactional-store-for-money-movement).
- Decisions are counted on `idempotency.decisions` (meter `CSharpEssentials.Idempotency`), and the middleware's log messages that name a record now include a key hash, never the key or the scope. See the [Idempotency Readme](../../CSharpEssentials.Idempotency/Readme.MD#telemetry).

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

A Redis store written against the 7.0 interface (one Lua script per operation on a hash with token, fingerprint and payload, keyed by `key.ToStorageKey()`) is in the [External stores](../../CSharpEssentials.Idempotency/Readme.MD#external-stores) section of the Idempotency Readme, together with the atomicity requirements and a DynamoDB note. Start from it when you port a 6.x Redis store.

## Stored records

- Storage keys start with `idem:v3:` (`IdempotencyRecordKey.DefaultPrefix`) and contain the namespace, so 7.0 does not see the records 6.x wrote under `idempotency:`. Those expire with their 6.x retention.
- Scope strings carry their kind: `user:{id}`, `tenant:{id}`, `client:{id}` and, for `IdempotencyScopes.Custom(kind, resolve)`, `{kind}:{value}`. `UserAndTenant` gives `{n}:tenant:{tenantId}/user:{userId}`, where `{n}` is the UTF-8 byte length of the `tenant:{tenantId}` part. `Custom` throws `ArgumentException` for a `kind` that is null, empty, contains `:` or `/`, consists only of digits or is a reserved built-in kind (`user`, `tenant`, `client`, `global`, in any casing; use the matching factory). The scope factories and the `IdempotencyRecordKey` constructor throw `ArgumentException` for a value with an unpaired surrogate.
- Every part of the storage key is written as `{utf8ByteLength}:{value}` and the parts are joined with `|`. A global key writes its scope part as `6:global`, not `-`, for example `idem:v3:4:http|6:global|3:abc`. A scoped key looks like `idem:v3:7:capture|9:tenant:t1|3:abc`. Scopes are built only by the `IdempotencyScope` factories, which always write a kind and a `:`, so no owner scope can be `global`.
- 7.0 has not been released, so records written by a 7.0 pre-release under the earlier scope strings or the `-` global marker do not need a migration.
- The middleware writes its responses as payload version 3 and does not read version 2. If a custom store keeps its old keys and returns a 6.x record as a completed payload, the middleware answers that key with `500` and logs an error, without running the handler, until the record expires. Use a new key prefix, or clear the table, when you upgrade a custom store.
- A custom store on SQL Server must not rely on the column collation to tell `abc` from `abc ` apart: every SQL Server collation, `_BIN2` included, ignores trailing spaces in `=` and unique comparisons. Key the table on a hash of the storage key, as the EF Core store does (`KeyHash binary(32)`), and compare the full key in code.
- During a rolling deploy, 6.x and 7.0 instances run side by side without seeing each other's records. A retry that reaches a 7.0 instance after the first attempt ran on a 6.x instance runs again. For operations that must not run twice, such as payments, drain traffic before switching or deploy outside the retry window of your clients.
