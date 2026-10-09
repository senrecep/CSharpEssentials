# CSharpEssentials.Core Example

This console application demonstrates the core utility extensions provided by `CSharpEssentials.Core`.

## Features Demonstrated

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
