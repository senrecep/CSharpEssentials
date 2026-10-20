---
name: csharpessentials-idempotency
description: Use when an operation must run once per key and caller, or a retry or redelivery must not repeat a side effect. Covers IIdempotencyRecordStore (in-memory, distributed cache, EF Core, contract-test kit), IIdempotencyExecutor with profiles and codecs, IdempotencyInbox for message consumers, IdempotencyScope factories, IdempotencyKeys.For/FromMessage/ToUuidV5 deterministic keys, lease versus transactional guarantee modes, Observe to Enforce rollout and the v6 to v7 migration.
---

# CSharpEssentials.Idempotency

Store, executor and inbox that the `Idempotency-Key` middleware (`CSharpEssentials.AspNetCore`), the Mediator `IdempotencyBehavior` and message consumers share. No reflection.

## Installation

```bash
dotnet add package CSharpEssentials.Idempotency
```

## Guarantee modes

| | Lease mode | Transactional mode |
|---|---|---|
| Store | In memory, a Redis recipe, any store that passes the contract tests | `EfCoreIdempotencyRecordStore` (`CSharpEssentials.EntityFrameworkCore`), Mediator `InsideTransaction` |
| Duplicates | A run that outlives its lease, or a crash before `CompleteAsync`, can repeat the operation after the lease | None for writes in that database |
| Use it for | Most operations | Money and credit movement |

The HTTP middleware is always lease mode. For a transactional guarantee over HTTP, parse the header with `IdempotencyKeyHeader.TryParse` (`CSharpEssentials.AspNetCore`; the middleware's normalization, so a quoted `"abc"` is `abc` in both layers) and pass that key to a Mediator command that is `ITransactionalRequest` and `IIdempotentRequest`. A call to an external system is outside your transaction: send it the same deterministic key.

## Run an operation once

```csharp
services.AddIdempotencyExecutor(o => o.AddProfile("payments", p => p.Retention = TimeSpan.FromDays(7)));

IdempotencyRecordKey key = new("payments.capture", IdempotencyScope.Tenant(command.TenantId), command.IdempotencyKey);
IdempotencyExecution<Receipt> execution = await executor.ExecuteAsync(
    key, fingerprint: command.Hash, new IdempotencyExecutionOptions { Profile = "payments" },
    token => handler.HandleAsync(command, token), codec, cancellationToken);
```

Statuses: `Executed`, `Replayed`, `InFlight` (retry later), `KeyReused` (different fingerprint; do not retry). The codec's `Classify` returns `Store` (`Retention`, 24 h), `StoreAsFailure` (`FailureRetention`, 10 min) or `Release`. An exception keeps the key in flight until `Lease` (1 min). `Lease` must not exceed `Retention` or `MaxLease` (15 min), and must cover the longest server timeout and the client's total retry budget.

## Process a message once

```csharp
IdempotencyExecutionStatus status = await inbox.ProcessAsync(
    consumer: "billing.invoice-created", messageId: message.Id,
    scope: IdempotencyScope.Tenant(message.TenantId),
    handler: token => HandleAsync(message, token), cancellationToken: cancellationToken);
```

Namespace `messaging:{consumer}`, key `IdempotencyKeys.FromMessage(messageId, consumer)`, constant fingerprint, 7-day `messaging` profile. `Replayed` means a duplicate: acknowledge it. `InFlight`: redeliver later.

## Deterministic keys

```csharp
string key = IdempotencyKeys.For("capture", orderId.ToString());   // {utf8ByteLength}:{value} parts joined with |
Guid uuid = IdempotencyKeys.ToUuidV5("capture", orderId.ToString());
```

A machine caller has no random key to retry with, so derive the key from business ids (event -> command -> provider) and send the same one downstream. Keys longer than 255 characters or outside printable ASCII become `h:` plus a SHA-256.

## Scopes

Build scopes only with the factories: `IdempotencyScope.User(id)`, `Tenant`, `Client`, `UserAndTenant(userId, tenantId)`, `Custom(kind, value)` (kinds `user`, `tenant`, `client`, `global` are reserved in any casing) and `Global`. A raw string does not compile. A key means something only to its owner.

| Layer | `UnscopedKeyBehavior` default | Without a scope |
|---|---|---|
| HTTP middleware | `Skip` | Runs without idempotency, logs `Idempotency.UnscopedKeySkipped` |
| Mediator behavior | `Reject` | `Idempotency.ScopeRequired` |

Machine callers have no user claim: use `IdempotencyScopes.Client`. Problem codes over HTTP: `KeyReused` 422, `InFlight` 409 with `Retry-After`, `ScopeRequired`/`KeyMissing`/`KeyInvalid` 400.

## Webhook dedupe

Key the command with `IdempotencyKeys.FromMessage(EventId, "payments-webhook")`, scope it with `IdempotencyScope.Custom("account", AccountId)` and use a constant fingerprint, so a provider redelivery with a different body is still a duplicate.

## Stores

- `AddIdempotency` (ASP.NET Core) defaults to `InMemoryIdempotencyRecordStore`: single instance only.
- `DistributedCacheIdempotencyRecordStore` is best effort: two instances can both get `Reserved` and the first `CompleteAsync` returns false. Use it only for operations that tolerate a rare repeat, or use the EF Core store.
- A custom store implements `IIdempotencyRecordStore`, builds keys with `key.ToStorageKey()` and must pass `IdempotencyRecordStoreContractTests` from `CSharpEssentials.Idempotency.Testing` (`Key(string, IdempotencyScope?)`, `UseRealClock`, `IsReservationAtomic`).
- SQL Server ignores trailing spaces in `=` and unique comparisons under every collation, `_BIN2` included. Key tables on a hash (`KeyHash binary(32)`) and compare the full key in code. Elsewhere use a binary no-pad collation or `text`/`varchar`, never `char(n)`.

## Rollout

`Mode` is `Enforce` by default. Start in `Observe` (answers as `Off`, stores as `Enforce`, counts what `Enforce` would have done on `idempotency.decisions`), check the counters, then switch to `Enforce`. `Observe` does not prevent duplicates; keep it short for payments.

## Migrating from 6.x

See `docs/migration/v6-to-v7-idempotency.md`: type mapping, `idem:v3:` record format, rolling-deploy advice and behavior changes.
