# CSharpEssentials.Clone Example

This console application demonstrates deep cloning with `CSharpEssentials.Clone`.

## Features Demonstrated

| Feature | Description |
|---------|-------------|
| **ICloneable<T>** | Explicit deep-clone contract for domain models (`Person`, `Address`, `Product`) |
| **Clone** | The `Clone()` method each model implements; the example does not clone collections |
| **Deep Copy Verification** | Modify clone without affecting original, and compare references |

## Running

```bash
cd examples/Examples.Clone
dotnet run
```
