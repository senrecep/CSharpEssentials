# CSharpEssentials (main package) example

A console app that combines several `CSharpEssentials` packages and uses the helpers the `CSharpEssentials` meta-package adds itself.

## Features demonstrated

| Feature | Package | Description |
|---------|---------------|-------------|
| Input sanitization | Core | `Trim()` and `ToPascalCase()` on user input |
| Optional lookup | Maybe | `Maybe<T>` instead of a nullable return (`FindUserByEmail`) |
| Business logic | Results | `Result<T>` for composable operations (`PlaceOrder`) and `Switch` on the outcome |
| Error reporting | Errors | Structured errors with `Error.Validation`, `Error.NotFound` |
| Time | Time | `IDateTimeProvider` / `DateTimeProvider` for testable date and time access |
| GUID generation | Core | `Guider.NewGuid()` and `Guider.ToStringFromGuid` (URL-safe) |
| String extensions | Results | `TrimStart(prefix)` / `TrimEnd(suffix)` returning `Result<string>` (`Program.cs` labels this section "Meta String Extensions", but the methods come from `CSharpEssentials.Results`) |
| JSON extensions | Meta | `JsonElement.TryGetProperty(params string[])` (first name that exists) and `JsonDocument.TryGetNestedProperty(params string[])` (names as a path), both returning `Result` |
| Time extensions | Meta | `long?.MsToDateTime()` returning `Maybe<DateTime>` (`None` for `null`) |
| Maybe to Result | Maybe | `ToMaybeResult(error)` and `ToMaybeUnitResult(error)` |
| Pipeline | All | A parse and validate function that returns `Result<int>` |

## Running

```bash
cd examples/Examples.Main
dotnet run
```

The `TryGetProperty 'user.name'` line of the Json section prints the whole `user` object, not the name: `TryGetProperty("user", "name")` returns the first of the given names that exists in the root, and `user` does. `TryGetNestedProperty("user", "name")` reads `user.name` (the next line, `Alice`).
