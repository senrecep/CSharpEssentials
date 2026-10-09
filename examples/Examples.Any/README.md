# CSharpEssentials.Any Example

This console application demonstrates the typed discriminated unions from `CSharpEssentials.Any`.

## Features Demonstrated

| Feature | Description |
|---------|-------------|
| **Creation** | `Any.Create<T0, T1>(value)` or the implicit conversion from a branch type |
| **Deconstruct and ToTuple** | Split a union into one value per branch; inactive branches are `default` |
| **Type Helpers** | `Is<TTarget, T0, T1>()`, `As<TTarget, T0, T1>()` and `TryAs<TTarget, T0, T1>(out value)` |
| **Type Checking** | `IsFirst`, `IsSecond` and `Index` |
| **Get Values** | `GetFirst()` and `GetSecond()` |
| **Factory Methods** | `Any<T0, T1>.First(value)` and `Second(value)` |
| **Switch** | Run a side effect for the active branch and read the returned `AnyActionStatus` |
| **Match** | Map the active branch to a value; the `AnyActionResult<TResult>` carries `Status` and `Result` |
| **Larger Unions** | `Any<T0, T1, T2>` and `Any<T0, T1, T2, T3>`; unions of 2 to 8 types exist |

## Running

```bash
cd examples/Examples.Any
dotnet run
```
