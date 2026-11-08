# CSharpEssentials.Core example

This console app shows the core utility extensions in `CSharpEssentials.Core`.

## Features demonstrated

| Feature | Description |
|---------|-------------|
| GUID creation and text | `Guider.NewGuid()`, `Guider.ToStringFromGuid()`, `Guider.ToGuidFromString()` and the `Guid` / `string` extension forms |
| String extensions | `ToPascalCase`, `ToCamelCase`, `ToSnakeCase`, `ToKebabCase`, `ToTrainCase`, `ToTitleCase`, `ToMacroCase`, `ToUnderscoreCamelCase` (with optional `CultureInfo`), `IsEmpty`, `IsNotEmpty` |
| Collection extensions | `ForEach`, `WhereIf`, `WithoutNulls`, `HasSameElements`, `AllTrue`, `AllFalse`, `IfAdd`, `IfAddRange` |
| General extensions | `IsNull`, `IsNotNull`, `IsTrue`, `IsFalse`, `IfTrue`, `IfFalse`, `IfNotNull`, `IfNull`, `ExplicitCast`, `MsToDateTime`, `AsTask`, `AsValueTask`, `WithCancellation`, `GetTypeGroup` |
| Random items | `GetRandomItem`, `GetRandomItems` |
| Exception extensions | `GetInnerExceptions`, `GetInnerExceptionsMessages` |
| HTTP codes | `HttpCodes` constants |

## Running

```bash
cd examples/Examples.Core
dotnet run
```
