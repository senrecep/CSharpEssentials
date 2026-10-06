# CSharpEssentials.Mediator Example

This console application demonstrates the pipeline behaviors from `CSharpEssentials.Mediator`.

## Features Demonstrated

| Feature | Description |
|---------|-------------|
| **ValidationBehavior** | Runs the request's `CSharpEssentials.Validation` validators and returns a failed `Result` without calling the handler |
| **LoggingBehavior** | Logs the request (`IRequestLoggable`), the response (`IResponseLoggable`) or both (`IRequestResponseLoggable`) |
| **CachingBehavior** | Caches the response of `ICacheable` requests in `IDistributedCache` |
| **TransactionScopeBehavior** | Runs the handler of an `ITransactionalRequest` inside a `TransactionScope` |
| **DI Registration** | `AddMediatorBehaviors()` in the default order, or one `AddMediator*Behavior()` call per behavior |

## Running

```bash
cd examples/Examples.Mediator
dotnet run
```
