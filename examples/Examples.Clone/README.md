# CSharpEssentials.Clone example

This console app shows deep cloning with `CSharpEssentials.Clone`.

## Features demonstrated

| Feature | Description |
|---------|-------------|
| `ICloneable<T>` | Explicit deep-clone contract for domain models (`Person`, `Address`, `Product`) |
| Clone | The `Clone()` method each model implements; the example does not clone collections |
| Deep copy check | Change the clone without affecting the original, and compare references |

## Running

```bash
cd examples/Examples.Clone
dotnet run
```
