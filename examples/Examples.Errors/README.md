# CSharpEssentials.Errors Example

This console application demonstrates the error handling system from `CSharpEssentials.Errors`.

## Features Demonstrated

| Feature | Description |
|---------|-------------|
| **Error Creation** | `Error.NotFound`, `Error.Validation`, `Error.Conflict`, `Error.Unauthorized`, `Error.Forbidden`, `Error.Unexpected`, `Error.Failure` |
| **Error[] Conversion** | Implicit `Error` to `Error[]` conversion |
| **Combining Errors** | The `+` operator and `Error.CreateMany` |
| **HTTP Status Mapping** | Convert `ErrorType` to HTTP status codes and status codes back with `ToErrorType` |
| **Error Metadata** | Attach custom key-value metadata to errors with `ErrorMetadata` |
| **Multiple Errors** | Collect and report several validation errors in an array |
| **DomainException** | Throw and catch a single `Error` wrapped in an exception |
| **EnhancedValidationException** | Throw and catch an `Error[]` wrapped in an exception |
| **Exception Bridge** | `Error.Exception(ex)` and `Error.Exception(code, ex)` |
| **Sentinel Values** | `Error.NoFirstError`, `Error.NoErrors`, `Error.False` |
| **ErrorType Enum** | `ToIntType` and the `ErrorType` members |
| **IError Interface** | Read `Code`, `Description`, `Type`, `NumericType` and `Metadata` through `IError` |

## Running

```bash
cd examples/Examples.Errors
dotnet run
```
