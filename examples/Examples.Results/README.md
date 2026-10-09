# CSharpEssentials.Results Example

This console application demonstrates the functional Result pattern from `CSharpEssentials.Results`.

## Features Demonstrated

| Feature | Description |
|---------|-------------|
| **Result Creation** | `Result.Success()`, `Result.Failure()` and the implicit conversions from a value or an `Error` |
| **Ensure / EnsureNotNull** | Guard a value and fail with a given error |
| **MapError** | Rewrite the errors of a failed result |
| **Try / TryAsync** | Wrap code that can throw into a `Result` |
| **ElseDo** | Run a side effect for the errors of a failed result |
| **TapError / TapErrorIf** | Observe errors without changing the result |
| **Deconstruct** | `var (isSuccess, value, errors) = result` |
| **SuccessIf / FailureIf** | Build a result from a condition |
| **Compensate** | Turn a failure back into a success |
| **ThenEnsure** | Validate the value with a `Result`-returning validator |
| **Then Chaining** | Compose multiple operations that short-circuit on failure |
| **Match / Switch** | Handle both success and failure in a single expression, or with side effects |
| **Map** | Transform the value of a successful result |
| **Bind** | Chain operations that return a `Result` |
| **Tap** | Run a side effect on success |
| **Else** | Provide fallback values or errors when a result fails |
| **Finally** | Receive the whole result and return a value |
| **GetValueOrDefault / GetValueOrThrow** | Extract the value with a fallback, or throw |
| **Unwrap / UnwrapOrDefault** | Extract the value or throw `ResultUnwrapException` |
| **Recover / RecoverFirst** | Recover from errors of a given `ErrorType` |
| **BindIf / FailIf** | Conditional bind and the inverse of `Ensure` |
| **TryCatch** | Run a function on a successful result and turn exceptions into errors |
| **Collection Extensions** | `Sequence`, `Traverse`, `Partition` and `FirstFailureOrSuccesses` |

## Running

```bash
cd examples/Examples.Results
dotnet run
```
