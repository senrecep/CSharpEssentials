# CSharpEssentials.Core Example

This console application demonstrates the core utility extensions provided by `CSharpEssentials.Core`.

## Features Demonstrated

| Feature | Description |
|---------|-------------|
| **Guid Generation** | `Guider.NewGuid()`, `Guider.ToStringFromGuid()`, `Guider.ToGuidFromString()` |
| **String Extensions** | `ToPascalCase`, `ToCamelCase`, `ToSnakeCase`, `ToKebabCase`, `ToTrainCase`, `ToTitleCase`, `IsNumeric` (with optional `CultureInfo`) |
| **Collection Extensions** | `IfAdd`, `IfAddRange`, `HasSameElements`, `AllTrue`, `AllFalse`, `WithoutNulls` |
| **General Extensions** | `IsNull`, `IsNotNull`, `AsValueTask`, `In` |
| **Random Items** | `GetRandomItem`, `GetRandomItems` from collections |

## Running

```bash
cd examples/Examples.Core
dotnet run
```
