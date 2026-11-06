# CSharpEssentials.Core example

This console application shows the core utility extensions in `CSharpEssentials.Core`.

## Features demonstrated

| Feature | Description |
|---------|-------------|
| **Guid Generation** | `Guider.NewGuid()`, `Guider.ToStringFromGuid()`, `Guider.ToGuidFromString()` and the `Guid` / `string` extension forms |
| **String Extensions** | `ToPascalCase`, `ToCamelCase`, `ToSnakeCase`, `ToKebabCase`, `ToTrainCase`, `ToTitleCase`, `ToMacroCase`, `ToUnderscoreCamelCase` (with optional `CultureInfo`), `IsEmpty`, `IsNotEmpty` |
| **Collection Extensions** | `ForEach`, `WhereIf`, `WithoutNulls`, `HasSameElements`, `AllTrue`, `AllFalse`, `IfAdd`, `IfAddRange` |
| **General Extensions** | `IsNull`, `IsNotNull`, `IsTrue`, `IsFalse`, `IfTrue`, `IfFalse`, `IfNotNull`, `IfNull`, `ExplicitCast`, `MsToDateTime`, `AsTask`, `AsValueTask`, `WithCancellation`, `GetTypeGroup` |
| **Random Items** | `GetRandomItem`, `GetRandomItems` from collections |
| **Exception Extensions** | `GetInnerExceptions`, `GetInnerExceptionsMessages` |
| **HTTP Codes** | `HttpCodes` constants |

## Running

```bash
cd examples/Examples.Core
dotnet run
```
