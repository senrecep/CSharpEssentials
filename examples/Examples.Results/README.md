# CSharpEssentials.Results Example

This console application demonstrates the functional Result pattern from `CSharpEssentials.Results`.

## Features Demonstrated

| Feature | Description |
|---------|-------------|
| **Result Creation** | `Result.Success()`, `Result.Failure()`, `Result<T>.Success()` |
| **Then Chaining** | Compose multiple operations that short-circuit on failure |
| **Match** | Handle both success and failure in a single expression |
| **Switch** | Execute side effects for success/failure |
| **Else** | Provide fallback values when a Result fails |
| **Bind** | Flatten nested Results |

## Running

```bash
cd examples/Examples.Results
dotnet run
```
