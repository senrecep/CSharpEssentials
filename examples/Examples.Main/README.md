# CSharpEssentials (Main Package) Example

This console application demonstrates how several `CSharpEssentials` packages work together, and the helpers that the `CSharpEssentials` meta-package adds itself.

## Features Demonstrated

| Feature | Packages Used | Description |
|---------|---------------|-------------|
| **Input Sanitization** | Core | `Trim()` and `ToPascalCase()` for cleaning user input |
| **Optional Lookup** | Maybe | `Maybe<T>` for safe nullable handling (`FindUserByEmail`) |
| **Business Logic** | Results | `Result<T>` for composable operations (`PlaceOrder`) and `Switch` on the outcome |
| **Error Reporting** | Errors | Structured errors with `Error.Validation`, `Error.NotFound` |
| **Time Handling** | Time | `IDateTimeProvider` / `DateTimeProvider` for testable date/time access |
| **Guid Generation** | Core | `Guider.NewGuid()` and `Guider.ToStringFromGuid` (URL-safe) |
| **String Extensions** | Results | `TrimStart(prefix)` / `TrimEnd(suffix)` returning `Result<string>` (`Program.cs` labels this section "Meta String Extensions", but the methods come from `CSharpEssentials.Results`) |
| **Json Extensions** | Meta | `JsonElement.TryGetProperty(params string[])` (first name that exists) and `JsonDocument.TryGetNestedProperty(params string[])` (names as a path), both returning `Result` |
| **Time Extensions** | Meta | `long?.MsToDateTime()` returning `Maybe<DateTime>` (`None` for `null`) |
| **Maybe to Result** | Maybe | `ToMaybeResult(error)` and `ToMaybeUnitResult(error)` |
| **Pipeline** | All | A parse and validate function that returns `Result<int>` |

## Running

```bash
cd examples/Examples.Main
dotnet run
```

The `TryGetProperty 'user.name'` line of the Json section prints the whole `user` object, not the name: `TryGetProperty("user", "name")` returns the first of the given names that exists in the root, and `user` does. `TryGetNestedProperty("user", "name")` reads `user.name` (the next line, `Alice`).
