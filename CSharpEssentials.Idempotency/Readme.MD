# CSharpEssentials.Idempotency

Idempotency building blocks that do not depend on a transport: a record store, scoped record keys, scope resolvers and
deterministic keys built from business ids, plus an executor that runs an operation at most once on top of them.
`CSharpEssentials.AspNetCore` uses them for its HTTP middleware, `CSharpEssentials.Mediator` for its idempotency
behavior; message consumers, background jobs and other pipelines can use them directly.

## Features

- **`IIdempotencyExecutor`**: reserves a key, runs the operation once, stores its result and replays it to a retry; answers `Executed`, `Replayed`, `InFlight` or `KeyReused`. A codec (`IIdempotencyResultCodec<T>`, or `IdempotencyResultCodec.Create`) serializes the result and classifies it as `Store`, `StoreAsFailure` (kept for the shorter `FailureRetention`) or `Release`. Registered by `AddIdempotencyExecutor`.
- **`IdempotencyInbox`**: processes each message at most once per consumer over the executor, with a 7-day `messaging` profile.
- **Modes**: `Off`, `Observe` and `Enforce`, globally, per profile or per operation, so a new idempotent operation can be rolled out in `Observe` first.
- **`IIdempotencyRecordStore`**: `TryReserveAsync`, `CompleteAsync` and `ReleaseAsync` with a reservation token, so a request that outlived its lease cannot overwrite or remove the entry of a later retry. Payloads are opaque bytes.
- **Stores**: `InMemoryIdempotencyRecordStore` (atomic within one process) and `DistributedCacheIdempotencyRecordStore` (any `IDistributedCache`; best-effort, not atomic).
- **`IdempotencyRecordKey`**: namespace, scope and key. `ToStorageKey()` gives `idem:v3:{n}:{namespace}|{n}:{scope}|{n}:{key}` with UTF-8 byte lengths, and `global` as the scope of a global key, so two different keys never share a storage key. The constructor takes an `IdempotencyScope?`; `null` or `IdempotencyScope.Global` gives a global key.
- **Scopes**: `IdempotencyScopes.User`, `Tenant(claim)`, `UserAndTenant(claim)`, `Client`, `Custom(kind, resolve)` and `Global`. The scopes are `user:{id}`, `tenant:{id}`, `client:{id}`, `{kind}:{value}`, `{n}:tenant:{tenantId}/user:{userId}` (`{n}` is the UTF-8 byte length of the tenant part) and `global`. A resolver returns `null` when it cannot tell who the caller is; `IdempotencyScope.Global` is a separate, explicit answer.
- **`IdempotencyScope`**: built only by its factories, `User(id)`, `Tenant(id)`, `Client(id)`, `UserAndTenant(userId, tenantId)`, `Custom(kind, value)` and `Global`. Every owner scope starts with its kind, so `Tenant("user:42")` (`tenant:user:42`) never matches `User("42")` (`user:42`) and no owner scope can be the `global` part of a global key. `Custom` rejects a `kind` that is empty, contains `:` or `/`, is all digits or has an unpaired surrogate; the built-in kinds `user`, `tenant`, `client` and `global` are reserved in any casing.
- **`IdempotencyKeys`**: `For(operation, ids)`, `FromMessage(messageId, consumer)`, `ToUuidV5(operation, ids)` (RFC 9562, namespace `9c8fbd17-da5f-4f32-a4bf-651721368d5e`) and `IsValid(key)`.
- **`IdempotencyErrorCodes`**: stable codes such as `Idempotency.KeyReused` and `Idempotency.InFlight`.
- **`IIdempotencyScopeAccessor`**: gives a pipeline without a request context of its own the current principal. `CSharpEssentials.AspNetCore` registers one that reads `HttpContext.User`.
- Targets `net11.0`, `net10.0`, `net9.0` and `netstandard2.1`. No reflection.

## Installation

```bash
dotnet add package CSharpEssentials.Idempotency
```

## Usage

### Run an operation once

`IIdempotencyExecutor` wraps any pipeline: a third-party mediator, a job runner or your own dispatcher.

```csharp
services.AddIdempotencyExecutor(o => o.AddProfile("payments", p => p.Retention = TimeSpan.FromDays(7)));

IIdempotencyResultCodec<Receipt> codec = IdempotencyResultCodec.Create<Receipt>(
    receipt => JsonSerializer.SerializeToUtf8Bytes(receipt),
    payload => JsonSerializer.Deserialize<Receipt>(payload.Span)!);

IdempotencyRecordKey key = new("payments.capture", IdempotencyScope.Tenant(command.TenantId), command.IdempotencyKey);
IdempotencyExecution<Receipt> execution = await executor.ExecuteAsync(
    key, fingerprint: command.Hash, new IdempotencyExecutionOptions { Profile = "payments" },
    token => handler.HandleAsync(command, token), codec, cancellationToken);
```

| Status | Meaning |
|---|---|
| `Executed` | The operation ran; `Value` is its result |
| `Replayed` | A result stored for the same key and fingerprint; the operation did not run |
| `InFlight` | Another run holds the key; retry later with the same key |
| `KeyReused` | The key was used with another fingerprint; do not retry |

- The codec's `Classify` decides what happens to a result: `Store` keeps it for `Retention` (24 hours), `StoreAsFailure` for `FailureRetention` (10 minutes), and `Release` frees the key, for a failure whose side effects were undone, such as a rolled-back transaction. Without `classify` every result is stored.
- A cancellation before the operation starts releases the key. An exception thrown by the operation is rethrown and keeps the key in flight until `Lease` (1 minute) expires, because its side effects are unknown.
- Settings resolve from the operation, then its profile, then `IdempotencyExecutorOptions`. `StoreKey` selects a keyed `IIdempotencyRecordStore`. `AddIdempotencyExecutor` validates the options and every profile at startup.
- `ResolveMode(options)` returns the mode an operation runs in, so a caller can skip computing its fingerprint in `Off`.

### Process a message once

`IdempotencyInbox` is for brokers and consumers that have no inbox of their own; it does not replace the inbox or outbox
of a messaging framework.

```csharp
IdempotencyExecutionStatus status = await inbox.ProcessAsync(
    consumer: "billing.invoice-created",
    messageId: message.Id,
    scope: IdempotencyScope.Tenant(message.TenantId),
    handler: token => HandleAsync(message, token),
    cancellationToken: cancellationToken);
```

- The record namespace is `messaging:{consumer}` and the key is `IdempotencyKeys.FromMessage(messageId, consumer)`, so two consumers of the same message do not deduplicate each other.
- The fingerprint is constant: the message id is authoritative, so a redelivery that serializes the message differently is still a duplicate.
- `Replayed` means a duplicate: acknowledge it. `InFlight` means another delivery is processing it: redeliver it later.
- The built-in `messaging` profile keeps records for 7 days, so a message redelivered from a dead-letter queue days later is still recognized. Pass `IdempotencyExecutionOptions` to override it, or register a profile named `messaging` to replace it.
- A generic overload takes a handler with a result and a codec, and returns the `IdempotencyExecution<TResult>`.

### Rollout modes

| Mode | Behavior |
|---|---|
| `Enforce` (default) | A stored result is replayed; an in-flight or reused key stops the operation |
| `Observe` | Records are reserved and stored as in `Enforce`, but nothing is blocked: a replay, an in-flight key or a reused key is logged and the operation runs. It measures duplicates; it does not prevent them |
| `Off` | The operation runs without touching the store |

```csharp
services.AddIdempotencyExecutor(o =>
{
    o.Mode = IdempotencyMode.Observe;
    o.AddProfile("payments", p => p.Mode = IdempotencyMode.Enforce);
});
```

### Use the store directly

```csharp
IdempotencyRecordKey recordKey = new("imports", IdempotencyScope.Global, IdempotencyKeys.For("import", fileId));
IdempotencyRecordReservation reservation =
    await store.TryReserveAsync(recordKey, fingerprint: fileHash, lease: TimeSpan.FromMinutes(1));

if (reservation.Status == IdempotencyReservationStatus.Reserved)
{
    byte[] result = await ImportAsync(fileId);
    await store.CompleteAsync(recordKey, reservation.Token!, fileHash, result, TimeSpan.FromDays(1));
}
```

`Completed` carries the stored `Payload`; `InFlight` means another run holds the key; `FingerprintMismatch` means the
key was used for other content. Release a reservation with `ReleaseAsync(recordKey, token)` when its work was undone.

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

The scope factories and the `IdempotencyRecordKey` constructor throw `ArgumentException` for a value with an unpaired
surrogate, which has no UTF-8 encoding. A `Custom` kind cannot contain `:` or `/`, and `user`, `tenant`, `client` and
`global` are reserved for the built-in factories in any casing: `Custom("user", id)` throws, use `User(id)`.

## Stores

| Store | Guarantee |
|---|---|
| `InMemoryIdempotencyRecordStore` | Reservation is atomic within one process. Expired entries are removed on access and by a sweep that runs at most once a minute. |
| `DistributedCacheIdempotencyRecordStore` | For a single instance or development. `IDistributedCache` has no set-if-absent, so two concurrent requests with the same key can both be reserved. Entries are versioned; an entry it cannot read is treated as absent. |

For strict guarantees across instances, implement `IIdempotencyRecordStore` with one atomic operation for
`TryReserveAsync` (Redis `SET NX`, a unique index) and token-checked `CompleteAsync` and `ReleaseAsync`, then run the
`CSharpEssentials.Idempotency.Testing` contract tests against it.

**Store requirement:** a store must compare `Namespace`, `Scope` and `Key` ordinally and case-sensitively. For a SQL
store that means a binary or case-sensitive collation on those columns; with a case-insensitive one,
`Tenant("ABC")` and `Tenant("abc")` share a record. The contract tests check this.
