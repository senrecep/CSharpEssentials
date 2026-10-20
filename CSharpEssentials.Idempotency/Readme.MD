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
- **Stores**: `InMemoryIdempotencyRecordStore` (atomic within one process) and `DistributedCacheIdempotencyRecordStore` (any `IDistributedCache`; best-effort, not atomic). `CSharpEssentials.EntityFrameworkCore` adds a transactional store on your `DbContext`.
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

### Guarantee modes

| | Lease mode | Transactional mode |
|---|---|---|
| How | A store with a reservation token and a lease: in memory, the [Redis recipe](#external-stores), any store that passes the contract tests | The record is written in the same database transaction as the business data (`EfCoreIdempotencyRecordStore`, Mediator `InsideTransaction`); the primary key settles the race |
| Duplicates | A run that outlives its lease, or a crash between the side effect and `CompleteAsync`, lets a retry run the operation again after the lease | None for writes in that database: the record and the data commit or roll back together |
| External side effects | Pass the key on to the downstream system too | Same: a payment provider call is not in your transaction, so send it the deterministic key ([Deterministic keys](#deterministic-keys)) |
| Use it for | Most operations, and everywhere without a shared database | Money and credit movement, and anything whose duplicate cannot be repaired |

The `Idempotency-Key` middleware of `CSharpEssentials.AspNetCore` is always lease mode: it reserves before and completes
after the endpoint, outside your handler's transaction, whatever store it uses. For a transactional guarantee over HTTP,
parse the header with `IdempotencyKeyHeader.TryParse` (the middleware's own normalization, so a quoted `"abc"` is
`abc` in both layers), pass that key to a Mediator command and run the command `InsideTransaction`; see
[Transactional store for money movement](#transactional-store-for-money-movement). The middleware (namespace `http`)
and the behavior (namespace: the request type's full name, `Type.FullName`) never share a record, so you can keep both.

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

### Rollout: Observe → Enforce

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
services.Configure<IdempotencyExecutorOptions>(configuration.GetSection("Idempotency:Executor")); // "Mode": "Enforce"
```

- The executor reads `IOptionsMonitor<IdempotencyExecutorOptions>` on every call, so a reload of a bound configuration section changes the mode, globally or per profile, without a restart. A reload that makes the options invalid throws `OptionsValidationException` from every call until it is fixed. The store is the one registered at startup.
- The mode comes from the operation (`IdempotencyExecutionOptions.Mode`), else its profile, else `IdempotencyExecutorOptions.Mode`.
- After a switch from `Observe` to `Enforce`, a result stored in `Observe` is replayed and a key still in flight is answered `InFlight`. With a store that writes in the business transaction (`ITransactionalIdempotencyRecordStore`, such as the EF Core store) only results whose transaction committed are stored, so a retry of work that rolled back runs again.
- `Observe` measures duplicates; it does not prevent them. Keep it short for operations that move money.
- In `Observe` a store that throws while reserving is logged (`Idempotency.Observe.ReserveFailed`) and the operation runs once without idempotency, unless the caller cancelled. In `Enforce` the exception propagates.
- The `Idempotency-Key` middleware of `CSharpEssentials.AspNetCore` has its own switch, `IdempotencyOptions.Mode`. The Mediator behavior and `IdempotencyInbox` use this one.

To add idempotency to an operation that already has callers:

1. Ship it in `Observe`. Nothing changes for callers; every decision is counted and logged.
2. Read the [telemetry](#telemetry). `key_reused` shows callers that reuse a key for different content, `in_flight` shows concurrent duplicates, `replay` shows how often a retry would have been answered from the store, and `unscoped` shows callers that send a key without a resolvable scope (the log events name the endpoint or request type, never the key).
3. Fix what the numbers show (a client that reuses keys, a machine caller that needs `IdempotencyScopes.Client`), then switch to `Enforce` through configuration.

**Transactions in `Observe`.** `Observe` records are real records. With an `ITransactionalIdempotencyRecordStore` (the
EF Core store) the executor and the HTTP middleware reserve the key through the `Detached` view, so the reservation never
waits for or fails the business transaction. When the operation returns a result to store, they release that reservation
and write the result through the store itself, in the business transaction (the Mediator transaction with
`IdempotencyPlacement.InsideTransaction`, or the one open around `UseIdempotency`). The record then commits and rolls back
with the business data, as in `Enforce`: when the commit fails, nothing is recorded and a retry after the switch to
`Enforce` runs the operation again. What remains:

- A store that cannot join the business transaction (in-memory, distributed cache, Redis) stores the result before the
  commit when the transaction wraps the idempotency layer, in `Observe` and `Enforce` alike. A commit that then fails
  leaves a record, and a retry replays a result for work that never committed. Use the EF Core store, or put the
  idempotency layer outside the transaction, for work whose commit can fail after the operation returned.
- Between the release of the detached reservation and the write in the business transaction the key is absent for one
  round trip. A concurrent duplicate that arrives then is reserved: in `Observe` it is counted `reserved` instead of
  `in_flight`, and on an instance that already runs `Enforce` (a rolling switch) it runs.
- From that write until the commit, a reservation of the same key waits for the business transaction, as in `Enforce`.
  An operation that reuses the key of an operation it already completed in the same transaction waits until its command
  times out, then runs without idempotency (`Idempotency.Observe.ReserveFailed`).
- A database error on that write (a lock or statement timeout, for example) is rolled back to a savepoint, so the
  business data still commits, without the record, and a retry after the switch to `Enforce` runs again. The EF Core
  store takes the savepoint in the context's transaction, and on PostgreSQL also in an ambient `TransactionScope`; on
  other providers an error that aborts an ambient transaction takes the business data with it. Under an isolation level
  stricter than `READ COMMITTED` (`TransactionScope` defaults to `Serializable`) the write always fails this way,
  because the release changed the row after the transaction's snapshot, so `Observe` records nothing: run the business
  transaction at `READ COMMITTED`, which the store needs anyway.

Keep `Observe` short for operations that move money.

### Lease and retry budget

A lease is how long a key stays in flight when nobody settles it: after a crash, a client abort, an exception or a
503/504, a retry gets `InFlight` until the lease ends and then runs the operation once more. Pick it by one rule:

> lease ≥ max(the server's request timeout, the client's total retry budget)

A lease shorter than the server's request timeout lets a retry start a second run while the first is still working. A
lease shorter than the client's total retry budget (for `AddStandardResilienceHandler`, `TotalRequestTimeout`) lets the
client's last attempt run a second time. The defaults are 1 minute (`IdempotencyExecutorOptions.Lease`,
`IdempotencyOptions.InFlightTimeout`) and at most 15 minutes (`MaxLease`, `MaxInFlightTimeout`); a lease must not exceed
its retention either. The options and profiles are validated at startup, and the values of an operation
(`IdempotencyExecutionOptions`) on each call. The client retries an `InFlight` answer after its
`Retry-After` (at most `IdempotentRetryOptions.MaxRetryAfter`, 30 seconds by default), so a long operation needs a
client that keeps retrying for as long as the lease, or one that polls the result.

Set a longer lease per operation or profile (`IdempotencyExecutionOptions.Lease`, `p.Lease`), not globally. The token a
reservation carries keeps a request that outlived its lease from overwriting or removing the entry of the retry that took
over, but its side effects have already happened.

### Telemetry

Every entry point counts its decisions on one counter, so an `Observe` rollout can be measured before it is enforced:

```csharp
builder.Services.AddOpenTelemetry().WithMetrics(m => m.AddMeter(IdempotencyTelemetry.MeterName));
```

| | |
|---|---|
| Meter | `CSharpEssentials.Idempotency` (`IdempotencyTelemetry.MeterName`) |
| Counter | `idempotency.decisions` (`IdempotencyTelemetry.DecisionsInstrumentName`) |
| `decision` | `reserved`, `replay`, `key_reused`, `in_flight`, `unscoped`, `released`, `stored` |
| `mode` | `observe` or `enforce`. Nothing is recorded in `Off` |
| `source` | `http` (middleware), `mediator` (behavior), `messaging` (`IdempotencyInbox`), `executor` (direct calls) |
| `target` | The route template, the request type, `messaging:{consumer}`, or the record namespace |

`replay`, `key_reused` and `in_flight` are counted in `Observe` too, as what `Enforce` would have done. Keys and scopes are never tags, and logs never contain them: a log that names a record carries a key hash, the first 16 hex characters of the SHA-256 of its namespace, scope and key, so the log lines of one key can be correlated.

Log events:

| EventId | Name | Level | Logged by |
|---|---|---|---|
| 7001 | `Idempotency.UnscopedKeySkipped` | Warning | HTTP, Mediator |
| 7002 | `Idempotency.StoredResponseUnreadable` | Error | HTTP |
| 7003 | `Idempotency.KeptInFlight` | Warning | HTTP, executor |
| 7004 | `Idempotency.ResponseNotStorable` | Warning | HTTP |
| 7005 | `Idempotency.StoreFailed` | Error | HTTP, executor |
| 7006 | `Idempotency.StaleReservation` | Warning | HTTP, executor |
| 7007 | `Idempotency.ReleaseFailed` | Warning | HTTP, executor |
| 7008 | `Idempotency.ExecutionFilterMissing` | Warning | HTTP |
| 7009 | `Idempotency.RequestTimeoutExceedsLease` | Warning | HTTP |
| 7010 | `Idempotency.Observe.WouldReplay` | Information | HTTP, executor |
| 7011 | `Idempotency.Observe.WouldRejectInFlight` | Information | HTTP, executor |
| 7012 | `Idempotency.Observe.WouldRejectKeyReused` | Information | HTTP, executor |
| 7013 | `Idempotency.Observe.WouldRejectScopeRequired` | Information | HTTP, Mediator |
| 7014 | `Idempotency.Observe.ResolutionFailed` | Warning | HTTP, Mediator |
| 7015 | `Idempotency.Observe.ReserveFailed` | Warning | HTTP, executor |
| 7016 | `Idempotency.Observe.WouldRejectKeyMissing` | Information | HTTP, Mediator |
| 7017 | `Idempotency.Observe.WouldRejectKeyInvalid` | Information | HTTP, Mediator |

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

A caller that is a message handler, not a person, has no random key to retry with: a random key means a new key on every
redelivery. Derive the key from the business ids, and send the same one downstream:

```csharp
// consumer: IdempotencyInbox keys the message as IdempotencyKeys.FromMessage(messageId, consumer)
// command:  one capture per order, however often the event arrives
string key = IdempotencyKeys.For("capture", orderId.ToString());

// payment provider: the same key, so the provider does not charge twice either
using HttpRequestMessage request = new(HttpMethod.Post, "/charges") { Content = JsonContent.Create(charge) };
request.SetIdempotencyKey(key);
```

The key then makes the side effect single end to end: the inbox drops the duplicate message, the command drops a second
event for the same order, and the provider drops a repeated call. `ToUuidV5` gives the same key as a UUID for providers
that insist on one.

### Calling an idempotent endpoint

The client side lives in `CSharpEssentials.Http`: `request.SetIdempotencyKey(key)` or
`HttpRequestBuilder.WithIdempotencyKey(key)` attaches the key once per logical call, and
`RetryUnsafeMethodsOnlyWithIdempotencyKey()` retries a keyed POST or PATCH with that same key on a missing response,
408, 429, 503, 504 and `409 Idempotency.InFlight` (a 409 InFlight, 429 or 503 after its `Retry-After`), but never a replayed response or a
`KeyReused`, `KeyInvalid`, `KeyMissing` or `ScopeRequired` error. See
[Idempotency-Key and safe retries](../CSharpEssentials.Http/Readme.MD#idempotency-key-and-safe-retries).

### Scopes and machine callers

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

A scope is part of the key, so a key means something only to its owner: `consume:42` from two tenants is two records.
Scopes are built only by the factories, so a raw id such as `"user:42"` cannot collide with the output of another
factory. `User` needs a user claim, so a machine caller that authenticates with client credentials has none; pick
`IdempotencyScopes.Client` (`client_id`, then `azp`) for it, or `UserAndTenant` and `Custom` when the owner is the pair.
`IdempotencyScope.Global` is a deliberate choice to share a key between all callers and never matches a scoped key.

When no scope resolves, `UnscopedKeyBehavior` decides, and the two layers differ on purpose:

| Layer | Default | A keyed request without a scope |
|---|---|---|
| HTTP middleware | `Skip` | Runs without idempotency; logs `Idempotency.UnscopedKeySkipped` |
| Mediator behavior | `Reject` | `Idempotency.ScopeRequired` |

`Reject` over HTTP answers 400; `Idempotency.KeyReused` is 422 and `Idempotency.InFlight` is 409 with `Retry-After`. Use the
same resolver in both layers, so an HTTP request that reaches an idempotent command resolves the same scope. See
[Scopes: HTTP and Mediator defaults](../CSharpEssentials.Mediator/Readme.MD#scopes-http-and-mediator-defaults) and the
[HTTP side](../CSharpEssentials.AspNetCore/Readme.MD#scopes-and-machine-callers).

### Transactional store for money movement

When the side effect is a write to your own database, give the record the same transaction: the EF Core store with the
Mediator behavior `InsideTransaction`, shown in [EF Core store](#ef-core-store). The command runs in one transaction that
reserves the key, changes the data and completes the record, so a crash or a rollback leaves neither, and two concurrent
requests for one key serialize on the primary key. The cost is that a duplicate waits for the first commit.

```csharp
public sealed record CapturePaymentCommand(Guid OrderId, string IdempotencyKey, decimal Amount)
    : ICommand<Result<Receipt>>, ITransactionalRequest, IIdempotentRequest;
```

- A failed `Result` of an `ITransactionalRequest` rolls back, and an exception does too, so in both cases the record rolls back with the data and the retry runs again.
- Send the provider the deterministic key (`IdempotencyKeys.For("capture", orderId.ToString())`), because its call is outside your transaction.
- Use the Redis or distributed cache store for operations that have no database transaction to join, and a hybrid for the rest: `AddProfile("payments", p => p.StoreKey = "payments")` with `AddEfCoreIdempotencyRecordStore<AppDbContext>("payments")` sends only that profile to the database.
- Over HTTP the middleware can sit in front for the `Idempotency-Key` contract (lease mode); the command's record is what makes the data single.

### Webhook dedupe

Providers deliver at least once and send no `Idempotency-Key`; their event id is the identity. Dedupe it with the Mediator
behavior and the EF Core store: verify the signature over the raw body, claim the event, do the work, commit.

```csharp
public sealed record PaymentEventCommand(string AccountId, string EventId, string Payload)
    : ICommand<Result>, ITransactionalRequest, IIdempotentRequest
{
    public string IdempotencyKey => IdempotencyKeys.FromMessage(EventId, "payments-webhook");

    IdempotencyScope? IIdempotentRequest.IdempotencyScope => IdempotencyScope.Custom("account", AccountId);

    string? IIdempotentRequest.IdempotencyFingerprint => "event";
}

byte[] webhookSecret = Convert.FromBase64String(builder.Configuration["PaymentProvider:WebhookSecret"]
    ?? throw new InvalidOperationException("PaymentProvider:WebhookSecret is not configured."));

app.MapPost("/webhooks/payments", async (HttpRequest request, ISender sender, CancellationToken cancellationToken) =>
{
    using MemoryStream body = new();
    await request.Body.CopyToAsync(body, cancellationToken);
    if (!WebhookSignature.IsValid(webhookSecret, body.GetBuffer().AsSpan(0, (int)body.Length), request.Headers[WebhookSignature.HeaderName]))
        return Results.Unauthorized();

    PaymentEvent? paymentEvent = JsonSerializer.Deserialize<PaymentEvent>(body.ToArray(), JsonSerializerOptions.Web);
    if (paymentEvent is null)
        return Results.BadRequest();

    Result result = await sender.Send(
        new PaymentEventCommand(paymentEvent.AccountId, paymentEvent.Id, paymentEvent.Payload), cancellationToken);
    return result.IsSuccess ? Results.Ok() : result.ToProblemResult(statusCode: StatusCodes.Status500InternalServerError);
});
```

- The signature is an HMAC over the exact bytes the provider sent, so read the raw body and verify it before you deserialize; a bound model has lost those bytes. `WebhookSignature` is the HMAC-SHA256 check of the [end-to-end example](https://github.com/senrecep/CSharpEssentials/blob/main/examples/Examples.Idempotency.EndToEnd/App/WebhookSignature.cs): it decodes the hex header and compares the bytes with `CryptographicOperations.FixedTimeEquals`. An unverified event id must never be claimed, or a forged request turns the real delivery into a duplicate.
- The scope comes from the command: a webhook has no principal, and the behavior rejects a request without a scope. `Custom("account", ...)` is a legal kind.
- The fingerprint is constant, so the same event serialized differently is still a duplicate and never `KeyReused`.
- A duplicate returns the stored success (200), so the provider stops sending it.
- A failed transaction rolls back and releases the key, and the failure goes back as a 5xx (`ToProblemResult(statusCode: 500)`, or an exception that propagates), so the provider redelivers it and the record is gone. The default `ToProblemResult()` maps a validation or not-found error to a 4xx, which many providers treat as final and never redeliver.
- Doing the same in your own table (a unique constraint, claim first, then work) is the same pattern as the EF Core store's reserve step; there is no separate API for it.
- Keep retention longer than the provider's redelivery window.

## Stores

| Store | Guarantee |
|---|---|
| `InMemoryIdempotencyRecordStore` | Reservation is atomic within one process. Expired entries are removed on access and by a sweep that runs at most once a minute. |
| `DistributedCacheIdempotencyRecordStore` | For a single instance or development. `IDistributedCache` has no set-if-absent, so two concurrent requests with the same key can both be reserved. Entries are versioned; an entry it cannot read is treated as absent. |
| `EfCoreIdempotencyRecordStore<TContext>` (`CSharpEssentials.EntityFrameworkCore`) | Atomic across instances through the primary key of a table in your `DbContext`, a fixed-length hash of the storage key. Records are written inside the context's current transaction, so they commit and roll back with the business data. See [EF Core store](#ef-core-store). |

For strict guarantees across instances, use the EF Core store or implement `IIdempotencyRecordStore` with one atomic operation for
`TryReserveAsync` (Redis `SET NX`, a unique index) and token-checked `CompleteAsync` and `ReleaseAsync`, then run the
`CSharpEssentials.Idempotency.Testing` contract tests against it. See [External stores](#external-stores).

**Store requirement:** a store must compare `Namespace`, `Scope` and `Key` ordinally: case-sensitively and with
trailing spaces significant. A SQL unique index on the text columns gets this wrong in two ways: a case-insensitive
collation makes `Tenant("ABC")` and `Tenant("abc")` one record, and a PAD SPACE collation, which ignores trailing
spaces (every SQL Server collation, `_BIN2` included, and MySQL's `utf8mb4_bin`), makes `Tenant("abc")` and `Tenant("abc ")` one
record, so one scope can replay another's response. Key a SQL store by a fixed-length hash of `ToStorageKey()` in a
binary column instead, as the EF Core store does. That is the only safe way on SQL Server, which ignores trailing
spaces in `=` and unique comparisons under every collation and has no NO PAD one. Elsewhere a binary NO PAD collation
also works: PostgreSQL `text` or `varchar` (never `char(n)`, which pads), MySQL `utf8mb4_0900_bin`, MariaDB
`utf8mb4_nopad_bin`. The contract tests check both.

### Best-effort stores across multiple instances

`DistributedCacheIdempotencyRecordStore` reads the key and then writes it; `IDistributedCache` has no set-if-absent, so
two instances that see a free key at the same moment both write a reservation and both return `Reserved`. Both run the
operation. The later write wins, so only its token is valid and the other instance's `CompleteAsync` returns `false`,
after its side effect already happened. A completed entry is not affected: once it exists, every reader replays it.

Use it for one instance and for development, or where a rare duplicate is harmless. With several instances behind a load
balancer, or for anything that must not run twice, use the EF Core store or a store with an atomic reserve (Redis `SET NX`
through the [Redis recipe](#external-stores)), and run the contract tests against it. Override
`IsReservationAtomic` to `false` in the contract tests of a store you accept as best-effort, so the suite says so.

### EF Core store

`CSharpEssentials.EntityFrameworkCore` has a transactional store. Map the table, register the store and put the Mediator
idempotency behavior inside the transaction:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder) =>
    modelBuilder.AddIdempotencyRecords(Database.ProviderName);

services.AddEfCoreTransactionRunner<AppDbContext>();
services.AddMediatorTransactionRunnerBehavior();
services.AddMediatorIdempotencyBehavior(o => o.Placement = IdempotencyPlacement.InsideTransaction);
services.AddEfCoreIdempotencyRecordStore<AppDbContext>();
```

- A request whose transaction rolls back leaves no record, so a webhook or message that failed is processed again
  when it is redelivered. A duplicate that arrives while the first one is still running waits for its commit and
  replays its response.
- Records are keyed by `KeyHash`, the SHA-256 of the storage key in a `binary(32)` column, so the store requirement
  above holds under any collation. The readable columns get the binary collation of a known provider (`C`,
  `Latin1_General_100_BIN2`, `utf8mb4_0900_bin`, `BINARY`); `KeyCollation` overrides it.
- The store needs the `READ COMMITTED` isolation level. Expired rows count as absent; `PurgeExpiredAsync` deletes
  them, for example from a hosted service.
- `AddEfCoreIdempotencyRecordStore<AppDbContext>("payments")` registers it as a keyed store, so only the profiles with
  `StoreKey = "payments"` use the database and the others keep the default store.
- In `Observe` the executor and the HTTP middleware reserve and release through its `Detached` view, in a context and
  transaction of their own, so a reservation does not wait for or fail the business transaction. A result to store is
  written in the business transaction, so it commits and rolls back with the business data as in `Enforce` (see
  [Transactions in `Observe`](#rollout-observe--enforce)).

The [EF Core Readme](../CSharpEssentials.EntityFrameworkCore/Readme.MD#idempotency-records) has the details and a purge
service.

## External stores

There is no Redis or DynamoDB package. The library gives the `IIdempotencyRecordStore` contract, the requirements
below and the contract tests in `CSharpEssentials.Idempotency.Testing`; the store itself lives in your application.

**Atomicity requirements**

- `TryReserveAsync` is one atomic operation: of concurrent calls for a free or expired key, exactly one returns
  `Reserved`. A live entry with another fingerprint returns `FingerprintMismatch`, whether it is in flight or completed.
- `CompleteAsync` and `ReleaseAsync` are compare-and-set on the reservation token. They change the entry only while it
  carries the caller's token and is not completed. `CompleteAsync` also succeeds when the entry is gone (its lease
  expired and nobody took the key over); `ReleaseAsync` never removes a completed entry.
- The lease applies to the in-flight reservation, the retention to the completed entry. The store applies the duration
  it is given; the caller picks it (`Retention`, `FailureRetention`, `ServerErrorRetention`).
- Build the storage key with `key.ToStorageKey(prefix)` and compare it ordinally and case-sensitively (see the store
  requirement above).
- The payload is opaque bytes and may be empty, so a store marks completion separately from the payload content.

### Redis

A reference implementation on `StackExchange.Redis`. Each operation is one Lua script on one key, so it is atomic and
works on Redis Cluster. An entry is a hash with `t` (token), `f` (fingerprint) and, once completed, `p` (payload);
leases and retention are `PEXPIRE` TTLs, so an expired entry is absent.

- Reserve: when the key is absent, write `t` and `f` and set `PEXPIRE lease`; otherwise return `FingerprintMismatch`
  for another `f`, `Completed` with `p` when `p` exists, else `InFlight`.
- Complete: when the key is absent, or `t` matches and `p` does not exist, write `t`, `f` and `p` and set
  `PEXPIRE retention`.
- Release: when `t` matches and `p` does not exist, `DEL`.

```csharp
using CSharpEssentials.Idempotency;
using StackExchange.Redis;

namespace Examples.Idempotency.Redis;

/// <summary>
/// <see cref="IIdempotencyRecordStore"/> on Redis. Every operation is one Lua script on one key, so it is atomic and
/// works on Redis Cluster. An entry is a hash: <c>t</c> (token), <c>f</c> (fingerprint) and, once completed,
/// <c>p</c> (payload). Leases and retention are Redis TTLs (<c>PEXPIRE</c>), so an expired entry is simply absent.
/// </summary>
public sealed class RedisIdempotencyRecordStore : IIdempotencyRecordStore
{
    private const string ReserveScript = """
        local entry = redis.call('HMGET', KEYS[1], 't', 'f', 'p')
        if not entry[1] then
          redis.call('HSET', KEYS[1], 't', ARGV[1], 'f', ARGV[2])
          redis.call('PEXPIRE', KEYS[1], ARGV[3])
          return { 0 }
        end
        if entry[2] ~= ARGV[2] then
          return { 3 }
        end
        if not entry[3] then
          return { 1 }
        end
        return { 2, entry[3] }
        """;

    private const string CompleteScript = """
        local entry = redis.call('HMGET', KEYS[1], 't', 'p')
        if entry[1] and (entry[2] or entry[1] ~= ARGV[1]) then
          return 0
        end
        redis.call('HSET', KEYS[1], 't', ARGV[1], 'f', ARGV[2], 'p', ARGV[3])
        redis.call('PEXPIRE', KEYS[1], ARGV[4])
        return 1
        """;

    private const string ReleaseScript = """
        local entry = redis.call('HMGET', KEYS[1], 't', 'p')
        if entry[1] == ARGV[1] and not entry[2] then
          redis.call('DEL', KEYS[1])
          return 1
        end
        return 0
        """;

    private readonly IConnectionMultiplexer _connection;

    /// <summary>Uses <see cref="IdempotencyRecordKey.DefaultPrefix"/>.</summary>
    public RedisIdempotencyRecordStore(IConnectionMultiplexer connection)
        : this(connection, IdempotencyRecordKey.DefaultPrefix)
    {
    }

    /// <param name="connection">The application's Redis connection.</param>
    /// <param name="keyPrefix">Prefix of the Redis keys, passed to <see cref="IdempotencyRecordKey.ToStorageKey"/>.</param>
    public RedisIdempotencyRecordStore(IConnectionMultiplexer connection, string keyPrefix)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(keyPrefix);
        _connection = connection;
        KeyPrefix = keyPrefix;
    }

    /// <summary>Prefix of the Redis keys written by this store.</summary>
    public string KeyPrefix { get; }

    /// <inheritdoc />
    public async ValueTask<IdempotencyRecordReservation> TryReserveAsync(
        IdempotencyRecordKey key,
        string fingerprint,
        TimeSpan lease,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentException.ThrowIfNullOrEmpty(fingerprint);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(lease, TimeSpan.Zero);
        cancellationToken.ThrowIfCancellationRequested();

        string token = Guid.NewGuid().ToString("N");
        RedisResult result = await _connection.GetDatabase().ScriptEvaluateAsync(
            ReserveScript,
            [key.ToStorageKey(KeyPrefix)],
            [token, fingerprint, Milliseconds(lease)]).ConfigureAwait(false);

        return (int)result[0] switch
        {
            0 => IdempotencyRecordReservation.Reserved(token),
            1 => IdempotencyRecordReservation.InFlight,
            2 => IdempotencyRecordReservation.Completed((byte[]?)result[1] ?? []),
            _ => IdempotencyRecordReservation.FingerprintMismatch,
        };
    }

    /// <inheritdoc />
    public async ValueTask<bool> CompleteAsync(
        IdempotencyRecordKey key,
        string token,
        string fingerprint,
        ReadOnlyMemory<byte> payload,
        TimeSpan retention,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentException.ThrowIfNullOrEmpty(token);
        ArgumentException.ThrowIfNullOrEmpty(fingerprint);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(retention, TimeSpan.Zero);
        cancellationToken.ThrowIfCancellationRequested();

        RedisResult result = await _connection.GetDatabase().ScriptEvaluateAsync(
            CompleteScript,
            [key.ToStorageKey(KeyPrefix)],
            [token, fingerprint, payload.ToArray(), Milliseconds(retention)]).ConfigureAwait(false);
        return (int)result == 1;
    }

    /// <inheritdoc />
    public async ValueTask<bool> ReleaseAsync(IdempotencyRecordKey key, string token, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentException.ThrowIfNullOrEmpty(token);
        cancellationToken.ThrowIfCancellationRequested();

        RedisResult result = await _connection.GetDatabase().ScriptEvaluateAsync(
            ReleaseScript,
            [key.ToStorageKey(KeyPrefix)],
            [token]).ConfigureAwait(false);
        return (int)result == 1;
    }

    /// <summary>Whole milliseconds for <c>PEXPIRE</c>, rounded up so a positive span never becomes 0.</summary>
    private static long Milliseconds(TimeSpan duration) => Math.Max(1L, (long)Math.Ceiling(duration.TotalMilliseconds));
}
```

Register your `IConnectionMultiplexer` and select the store. For HTTP endpoints:

```csharp
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));
builder.Services.AddIdempotency(o => o.UseStore<RedisIdempotencyRecordStore>(ServiceLifetime.Singleton));
```

For the executor, `IdempotencyInbox` or the Mediator behavior, register it before `AddIdempotencyExecutor`, which keeps
an existing store:

```csharp
builder.Services.AddSingleton<IIdempotencyRecordStore, RedisIdempotencyRecordStore>();
builder.Services.AddIdempotencyExecutor();
```

For one profile only, register it as a keyed `IIdempotencyRecordStore` and name the key in the profile's `StoreKey`.
`StackExchange.Redis` commands take no `CancellationToken`, so the store checks it before the call only. Multi-field
`HSET` needs Redis 4.0 or later.

This code is kept in [`examples/Examples.Idempotency.Redis`](https://github.com/senrecep/CSharpEssentials/tree/main/examples/Examples.Idempotency.Redis),
where CI runs every contract scenario against a Redis container:

```csharp
public sealed class RedisIdempotencyRecordStoreTests(RedisFixture redis)
    : IdempotencyRecordStoreContractTests, IClassFixture<RedisFixture>
{
    protected override bool UseRealClock => true;

    protected override IIdempotencyRecordStore CreateStore(TimeProvider timeProvider) =>
        new RedisIdempotencyRecordStore(redis.Connection);
}
```

### DynamoDB

Use one item per `ToStorageKey()` partition key with `token`, `fingerprint`, `payload` and `expiresAt`, and conditional
writes. DynamoDB TTL deletes expired items late, so expiry is part of every condition and TTL only cleans up.

- Reserve: `PutItem` with `attribute_not_exists(pk) OR expiresAt <= :now`. On `ConditionalCheckFailedException`, read
  the existing item (`ReturnValuesOnConditionCheckFailure = ALL_OLD`) and return `FingerprintMismatch`, `Completed` or
  `InFlight`.
- Complete: `PutItem` with `attribute_not_exists(pk) OR (#token = :token AND attribute_not_exists(payload))`, with
  `expiresAt` set to now plus the retention.
- Release: `DeleteItem` with `#token = :token AND attribute_not_exists(payload)`.
