# CSharpEssentials.Core example

A console app that calls every public helper of [CSharpEssentials.Core](../../CSharpEssentials.Core/README.MD) and prints the result.

## Run

```bash
dotnet run --project examples/Examples.Core
```

It needs no services and exits when done. The random items and new GUIDs differ on each run.

## What each section shows

| Section | APIs |
|---|---|
| String case conversion | `ToSnakeCase`, `ToKebabCase`, `ToPascalCase`, `ToCamelCase`, `ToMacroCase`, `ToTrainCase`, `ToTitleCase`, `ToUnderscoreCamelCase`, with a `tr-TR` culture |
| GUIDs | `Guider.NewGuid` (plain, `DateTimeOffset`, `TimeProvider`), `NewMonotonicGuid`, `NewSqlServerGuid`, `NewGuidV5`, `ToStringFromGuid`, `ToGuidFromString` and its `FormatException` |
| Collections | `IfAdd`, `IfAddRange`, the three `WhereIf` overloads, both `WithoutNulls`, `HasSameElements`, `AllTrue`, `AllFalse`, lazy `ForEach`, `GetRandomItem`, `GetRandomItems` |
| Null, empty and boolean checks | `IsEmpty`, `IsNotEmpty`, `IsNull`, `IsNotNull`, `IfNotNull`, `IfNull`, `IfTrue`, `IfFalse`, `IsTrue`, `IsFalse` |
| Tasks | `AsTask`, `AsValueTask`, `WithCancellation` (stops waiting, the task keeps running) |
| Other helpers | `IsBetween`, `IsBetweenExclusive`, `ExplicitCast`, `MsToDateTime`, `GetTypeGroup`, `GetInnerExceptions`, `GetInnerExceptionsMessages`, `HttpCodes`, and the C# 14 properties `IsEmpty` and `IsPalindrome` |
| Transactions | A small in-memory `ITransactionRunner` and `ITransactionCallbacks`: a callback runs after commit and is dropped on rollback |

`Members.cs` uses the C# 14 properties from namespace `CSharpEssentials`. It is a separate file because importing both `CSharpEssentials` and `CSharpEssentials.Core` makes `string.IsEmpty()` ambiguous (CS9339).
