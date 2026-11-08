# CSharpEssentials.Mediator example

Console application that calls the `CSharpEssentials.Mediator` pipeline behaviors directly, without a generated mediator.

| Feature | What the sample does |
|---------|----------------------|
| ValidationBehavior | Runs the request's `CSharpEssentials.Validation` validator and returns a failed `Result` without calling the handler |
| LoggingBehavior | Logs the handling and the JSON response of a request marked `IResponseLoggable` |
| CachingBehavior | Caches the response of an `ICacheable` request in an `IDistributedCache` (a dictionary-backed one in the sample) |
| TransactionScopeBehavior | Runs the handler of an `ITransactionalRequest` inside a `TransactionScope` |
| DI registration | `AddMediatorBehaviors()` registers the default behaviors |

## Running

```bash
cd examples/Examples.Mediator
dotnet run
```
