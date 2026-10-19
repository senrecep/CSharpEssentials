# CSharpEssentials.Idempotency

Idempotency building blocks that do not depend on a transport: a record store, scoped record keys, scope resolvers and
deterministic keys built from business ids. `CSharpEssentials.AspNetCore` uses them for its HTTP middleware; message
consumers and background jobs can use them directly.

## Features

- **`IIdempotencyRecordStore`**: `TryReserveAsync`, `CompleteAsync` and `ReleaseAsync` with a reservation token, so a request that outlived its lease cannot overwrite or remove the entry of a later retry. Payloads are opaque bytes.
- **Stores**: `InMemoryIdempotencyRecordStore` (atomic within one process) and `DistributedCacheIdempotencyRecordStore` (any `IDistributedCache`; best-effort, not atomic).
- **`IdempotencyRecordKey`**: namespace, scope and key. `ToStorageKey()` gives `idem:v3:{n}:{namespace}|{n}:{scope}|{n}:{key}` with UTF-8 byte lengths, and `global` as the scope of a global key, so two different keys never share a storage key. The constructor takes an `IdempotencyScope?`; `null` or `IdempotencyScope.Global` gives a global key.
- **Scopes**: `IdempotencyScopes.User`, `Tenant(claim)`, `UserAndTenant(claim)`, `Client`, `Custom(kind, resolve)` and `Global`. The scopes are `user:{id}`, `tenant:{id}`, `client:{id}`, `{kind}:{value}`, `{n}:tenant:{tenantId}/user:{userId}` (`{n}` is the UTF-8 byte length of the tenant part) and `global`. A resolver returns `null` when it cannot tell who the caller is; `IdempotencyScope.Global` is a separate, explicit answer.
- **`IdempotencyScope`**: built only by its factories, `User(id)`, `Tenant(id)`, `Client(id)`, `UserAndTenant(userId, tenantId)`, `Custom(kind, value)` and `Global`. Every owner scope starts with its kind, so `Tenant("user:42")` (`tenant:user:42`) never matches `User("42")` (`user:42`) and no owner scope can be the `global` part of a global key. `Custom` rejects a `kind` that is empty, contains `:` or is all digits.
- **`IdempotencyKeys`**: `For(operation, ids)`, `FromMessage(messageId, consumer)`, `ToUuidV5(operation, ids)` (RFC 9562, namespace `9c8fbd17-da5f-4f32-a4bf-651721368d5e`) and `IsValid(key)`.
- **`IdempotencyErrorCodes`**: stable codes such as `Idempotency.KeyReused` and `Idempotency.InFlight`.
- Targets `net11.0`, `net10.0`, `net9.0` and `netstandard2.1`. No reflection.

## Installation

```bash
dotnet add package CSharpEssentials.Idempotency
```

## Usage

### Run a message handler once

```csharp
using CSharpEssentials.Idempotency;

IIdempotencyRecordStore store = new InMemoryIdempotencyRecordStore();

string key = IdempotencyKeys.FromMessage(message.Id, consumer: "billing");
IdempotencyRecordKey recordKey = new("messages", IdempotencyScope.Global, key);

IdempotencyRecordReservation reservation =
    await store.TryReserveAsync(recordKey, fingerprint: message.Hash, lease: TimeSpan.FromMinutes(1));

switch (reservation.Status)
{
    case IdempotencyReservationStatus.Reserved:
        try
        {
            byte[] result = await HandleAsync(message);
            await store.CompleteAsync(recordKey, reservation.Token!, message.Hash, result, TimeSpan.FromDays(1));
        }
        catch
        {
            await store.ReleaseAsync(recordKey, reservation.Token!);
            throw;
        }
        break;
    case IdempotencyReservationStatus.Completed:
        ReadOnlyMemory<byte> stored = reservation.Payload!.Value;
        break;
    case IdempotencyReservationStatus.InFlight:
    case IdempotencyReservationStatus.FingerprintMismatch:
        break;
}
```

The fingerprint identifies the request content: the same key with another fingerprint returns `FingerprintMismatch`
instead of the stored payload.

### Deterministic keys

```csharp
string key = IdempotencyKeys.For("refund", orderId, refundId);
Guid uuid = IdempotencyKeys.ToUuidV5("refund", orderId, refundId);
```

Each part is written as `{utf8ByteLength}:{value}` and the parts are joined with `|`, so `("a", "bc")` and `("ab", "c")`
give different keys. When that string contains a character outside printable ASCII (or `"` or `\`) or is longer than
255 characters, the key is `h:` followed by the unpadded base64url SHA-256 of it. The format is pinned by
[`tests/vectors/idempotency-key-vectors.json`](https://github.com/senrecep/CSharpEssentials/blob/main/tests/vectors/idempotency-key-vectors.json).
A part with a lone surrogate throws `ArgumentException`.

### Scopes

```csharp
IIdempotencyScopeResolver resolver = IdempotencyScopes.UserAndTenant("tenant_id");
IdempotencyScope? scope = resolver.Resolve(user);

if (scope is null)
{
    return;
}

IdempotencyRecordKey recordKey = new("http", scope.Value, key);
```

`User` reads `ClaimTypes.NameIdentifier`, then `sub`. `Client` reads `client_id`, then `azp`. The claim-based resolvers
return `null` for an anonymous principal or a missing claim, and the caller decides what to do with it.

## Stores

| Store | Guarantee |
|---|---|
| `InMemoryIdempotencyRecordStore` | Reservation is atomic within one process. Expired entries are removed on access and by a sweep that runs at most once a minute. |
| `DistributedCacheIdempotencyRecordStore` | For a single instance or development. `IDistributedCache` has no set-if-absent, so two concurrent requests with the same key can both be reserved. Entries are versioned; an entry it cannot read is treated as absent. |

For strict guarantees across instances, implement `IIdempotencyRecordStore` with one atomic operation for
`TryReserveAsync` (Redis `SET NX`, a unique index) and token-checked `CompleteAsync` and `ReleaseAsync`, then run the
`CSharpEssentials.Idempotency.Testing` contract tests against it.
