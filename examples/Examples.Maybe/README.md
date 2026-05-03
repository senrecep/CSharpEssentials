# CSharpEssentials.Maybe Example

This console application demonstrates the Maybe monad from `CSharpEssentials.Maybe` for handling nullable values functionally.

## Features Demonstrated

| Feature | Description |
|---------|-------------|
| **Maybe Creation** | `Maybe.From`, `AsMaybe`, `Maybe.None` |
| **Map** | Transform the value inside a Maybe |
| **Bind** | Chain operations that return Maybe |
| **Where** | Filter Maybe values with predicates |
| **GetValueOrDefault** | Extract value with fallback |
| **GetValueOrThrow** | Extract value or throw custom exception |
| **Execute** | Run side effects for Some/None cases |

## Running

```bash
cd examples/Examples.Maybe
dotnet run
```
