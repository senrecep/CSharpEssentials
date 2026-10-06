# CSharpEssentials.Validation Example

This console application demonstrates model validation with `CSharpEssentials.Validation` and its `Result` integration.

## Features Demonstrated

| Feature | Description |
|---------|-------------|
| **String validators** | `NotEmpty`, length, `EmailAddress`, `Matches`, `StartsWith`, `EndsWith`, `NotNull` |
| **Comparable validators** | `GreaterThan`, `LessThan`, `InclusiveBetween` and related checks for `int`, `decimal`, `DateTime` |
| **Nullable structs** | Rules on `int?` and `DateTime?` are skipped when the value is `null` |
| **Collections** | Rules on `List<T>?`, `T[]?` and `IEnumerable<T>?` |
| **CascadeMode** | `CascadeMode.Continue` collects every error of a field |
| **Must / MustAsync** | Custom predicates |
| **Nested objects** | `SetValidatorAsync` on a nested property; a `null` nested object is skipped |
| **ForEach / ForEachAsync** | Validates each item of a collection |
| **Conditional rules** | Plain C# `if`/`switch` inside the validator |
| **Include** | Composes one validator from others |
| **ValidateWith** | Validates inside a `Result<T>` chain |

## Running

```bash
cd examples/Examples.Validation
dotnet run
```
