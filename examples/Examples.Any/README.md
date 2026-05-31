# CSharpEssentials.Any Example

This console application demonstrates the dynamic Any type from `CSharpEssentials.Any`.

## Features Demonstrated

| Feature | Description |
|---------|-------------|
| **Any Type** | Store any value in a single type with `Any.Create<T0,T1>(value)` or implicit operator |
| **Type Checking** | `Is<TTarget, T0, T1>()` to verify the underlying type |
| **Casting** | `As<TTarget, T0, T1>()` with safe fallback to default |
| **AnyActionResult** | Generic success/failure result with any payload |
| **Any&lt;T0,T1&gt; through Any&lt;T0,...,T7&gt;** | Typed unions for 2 to 8 different types |

## Running

```bash
cd examples/Examples.Any
dotnet run
```
