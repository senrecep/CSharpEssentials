# Migrating idempotency from 6.x to 7.0

7.0 moves the idempotency store out of `CSharpEssentials.AspNetCore` into the new `CSharpEssentials.Idempotency` package, so message consumers and jobs can use the same store as the `Idempotency-Key` middleware. `CSharpEssentials.AspNetCore` depends on it; you do not add a package reference for the middleware.

## Breaking changes

- `IIdempotencyStore`, `IdempotencyReservation`, `IdempotencyReservationStatus`, `InMemoryIdempotencyStore` and `DistributedCacheIdempotencyStore` are removed from `CSharpEssentials.AspNetCore`. There is no `[Obsolete]` bridge; use the types in the table below.
- `IdempotencyOptions.KeyScope` (`Func<HttpContext, string?>`) and `IdempotencyOptions.DefaultKeyScope` are removed. Use `IdempotencyOptions.ScopeResolver`.
- Stored records use a new key format (`idem:v3:` prefix) and a new payload format (version 3). Records written by 6.x are not read. See [Stored records](#stored-records).

`UseInMemoryStore()`, `UseDistributedCacheStore()`, `UseStore<TStore>(lifetime)`, `AllowUnscopedKeys` and the other options keep their names and behavior.

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

`IdempotentResponse` stays in `CSharpEssentials.AspNetCore`. The middleware serializes it into the payload, so a store no longer sees it.

## Scopes

`ScopeResolver` receives `HttpContext.User`, not the whole `HttpContext`. A 6.x `KeyScope` that read a claim becomes `IdempotencyScopes.Custom(user => user?.FindFirst("org_id")?.Value)`. A scope that came from a header or a route value has to become a claim first, for example in an `IClaimsTransformation`.

A resolver returns `null` when it cannot resolve a scope, and the request runs without idempotency unless `AllowUnscopedKeys` is `true`, as in 6.x. `IdempotencyScopes.Global` is a different answer: it shares the key between all callers on purpose. A global key never matches a scoped one.

The new `Namespace` option (default `http`) separates the middleware's records from records written by other code that shares the store.

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
