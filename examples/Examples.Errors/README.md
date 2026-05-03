# CSharpEssentials.Errors Example

This console application demonstrates the comprehensive error handling system from `CSharpEssentials.Errors`.

## Features Demonstrated

| Feature | Description |
|---------|-------------|
| **Error Creation** | `Error.NotFound`, `Error.Validation`, `Error.Conflict`, `Error.Unauthorized`, `Error.Forbidden`, `Error.Unexpected` |
| **HTTP Status Mapping** | Convert `ErrorType` to HTTP status codes |
| **Error Metadata** | Attach custom key-value metadata to errors |
| **Multiple Errors** | Collect and report multiple validation errors |
| **Custom Errors** | Create errors with custom codes and types |

## Running

```bash
cd examples/Examples.Errors
dotnet run
```
