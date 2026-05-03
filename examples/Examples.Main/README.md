# CSharpEssentials (Main Package) Example

This console application demonstrates how multiple `CSharpEssentials` packages work together in a cohesive workflow.

## Features Demonstrated

| Feature | Packages Used | Description |
|---------|---------------|-------------|
| **Input Sanitization** | Core | String extensions for cleaning user input |
| **Optional Lookup** | Maybe | `Maybe<T>` for safe nullable handling |
| **Business Logic** | Results | `Result<T>` for composable operations |
| **Error Reporting** | Errors | Structured errors with `Error.Validation`, `Error.NotFound` |
| **Time Handling** | Time | `IDateTimeProvider` for testable date/time access |
| **Guid Generation** | Core | `Guider.NewGuid()` for unique identifiers |
| **Pipeline** | All | End-to-end workflow combining all packages |

## Running

```bash
cd examples/Examples.Main
dotnet run
```
