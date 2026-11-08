# CSharpEssentials.Maybe example

Console app that runs the `CSharpEssentials.Maybe` API.

## Features demonstrated

| Feature | Description |
|---------|-------------|
| Maybe creation | Implicit conversion, `Maybe<T>.From`, `Maybe<T>.None`, `AsMaybe` |
| Tap / TapIf | Run side effects for a present value, optionally gated by a condition |
| BindIf / MapIf | Bind or map only when a condition holds |
| ToMaybeResult / ToUnitResult | Bridge a `Maybe<T>` to `Result<T>` or `Result`, with a custom error for `None` |
| Select / SelectMany | LINQ query syntax over `Maybe<T>` |
| Map | Transform the value inside a Maybe |
| Match | Handle `Some` and `None` in a single expression |
| Or | Fall back to another value or Maybe when empty |
| Choose | Keep the present values of a sequence of Maybes |
| Bind | Chain operations that return Maybe |
| GetValueOrDefault / GetValueOrThrow | Extract the value with a fallback, or throw a custom exception |
| TryFirst / TryLast / TryFind | Find an element in a sequence or dictionary as a Maybe |
| AsNullable | Convert back to a nullable value |
| Execute / ExecuteNoValue | Run side effects for the `Some` and `None` cases |
| Flatten | Collapse `Maybe<Maybe<T>>` |
| Deconstruct | `(bool hasValue, int value) = maybe` |
| Collection extensions | `Sequence`, `Traverse` and `Partition` |

## Running

```bash
cd examples/Examples.Maybe
dotnet run
```
