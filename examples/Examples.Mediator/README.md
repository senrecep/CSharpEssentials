# CSharpEssentials.Mediator example

A console app that sends requests through the generated mediator (`Mediator.SourceGenerator`) with every
`CSharpEssentials.Mediator` behavior registered. Each section of the output shows one behavior at work.

## Running

```bash
dotnet run --project examples/Examples.Mediator
```

No database, cache server or network is needed. The program exits with code 0.

## What each section shows

| Section | Requests | What happens |
|---------|----------|--------------|
| Validation (Enforce) | `CreateProduct` | The validator fails, the handler does not run, and the caller gets a failed `Result` with `Name.NotEmpty` and `Price.GreaterThan`. `ConsoleValidationObserver` prints the failure |
| Validation (LogOnly) | `ImportProduct` | `IValidationModeOverride` returns `LogOnly`, so the failure is logged and observed and the handler still runs |
| Exception handling | `DeleteProduct` | The handler throws; the caller gets a failed `Result` with an `Unexpected` error whose code is the exception type name |
| Caching and logging | `GetProduct` | The second query is a cache hit and the handler runs once. `BypassCache` skips the cache. `IResponseLoggable` logs the response JSON |
| Transactions | `RenameProduct` | `ConsoleTransactionRunner` stands in for a database transaction: a successful `Result` commits, a failed one rolls back |
| Lock | `RestockProduct` | Two concurrent requests for the same `LockKey` run one at a time. With the key held elsewhere, `LockTimeout` passes and the caller gets a `TimeoutException` error |
| Idempotency | `CapturePayment`, `RefundPayment`, `ChargeCard` | A retry with the same key replays the stored result. The same key with another amount is `Idempotency.KeyReused`, a missing key `Idempotency.KeyMissing`, a request with no scope `Idempotency.ScopeRequired`. After a handler throws, a retry gets `Idempotency.InFlight` until the lease ends |

## Files

| File | Content |
|------|---------|
| `Program.cs` | Registration and the requests sent in each section |
| `Products.cs` | Product requests, validators, handlers and the validation observer |
| `Payments.cs` | Idempotent payment requests |
| `ConsoleTransactionRunner.cs` | An `ITransactionRunner` that prints `BEGIN`, `COMMIT` and `ROLLBACK` |
| `Counters.cs` | Counts handler calls so the output shows when a behavior skipped a handler |

## Registration notes

- `AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped)`: the validation, transaction runner,
  lock and idempotency behaviors are scoped. A singleton mediator resolves them from the root provider, which fails
  with scope validation on (the ASP.NET Core default in Development).
- `AddMediatorTransactionRunnerBehavior()` replaces the `TransactionScope` behavior that `AddMediatorBehaviors()`
  registered.
- `AddMediatorLockBehavior()` and `AddMediatorIdempotencyBehavior()` come after `AddMediatorBehaviors()`, so
  validation runs before a key is reserved or a lock is taken.
- A console app has no caller, so `CapturePayment` scopes its key to the tenant on the request. In ASP.NET Core,
  `AddIdempotency()` from `CSharpEssentials.AspNetCore` gives the behavior the request user instead.

See the [Mediator Readme](../../CSharpEssentials.Mediator/Readme.MD) for the full reference.
