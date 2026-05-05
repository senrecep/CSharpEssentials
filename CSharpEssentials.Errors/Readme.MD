# CSharpEssentials.Errors

Structured, type-safe error handling for .NET. Errors are data, not exceptions.

## Features

- **Error Struct** — Value-type error with code, description, type, and metadata.
- **Standard Error Types** — Failure, Validation, NotFound, Conflict, Unauthorized, Forbidden, Unexpected.
- **ErrorMetadata** — Attach contextual data to errors fluently.
- **Exception Bridge** — Convert exceptions to `Error` or throw `DomainException` / `EnhancedValidationException`.
- **HTTP Mapping** — Built-in conversion between `ErrorType` and HTTP status codes.

## Installation

```bash
dotnet add package CSharpEssentials.Errors
```

## Usage

### Creating Errors

```csharp
using CSharpEssentials.Errors;

Error notFound = Error.NotFound("User.NotFound", "User was not found.");
Error validation = Error.Validation("Email.Invalid", "Email format is invalid.");
Error conflict = Error.Conflict("User.Duplicate", "User already exists.");
Error failure = Error.Failure("Payment.Failed", "Payment was rejected.");
```

### Error Metadata

```csharp
Error withMeta = Error.NotFound(
    "User.NotFound",
    "User was not found.",
    new ErrorMetadata()
        .AddMetadata("TraceId", Guid.NewGuid().ToString())
        .AddMetadata("RequestPath", "/api/users/123"));
```

### Combining Errors

```csharp
Error a = Error.Validation("Name.Empty", "Name is required.");
Error b = Error.Validation("Email.Invalid", "Email is invalid.");

Error[] combined = a + b;
```

### Converting from Exceptions

```csharp
try { /* ... */ }
catch (Exception ex)
{
    Error error = Error.Exception(ex);
}
```

### Domain Exceptions

```csharp
using CSharpEssentials.Exceptions;

Error error = Error.Validation("Order.Invalid", "Total must be greater than zero.");
throw new DomainException(error);
```

### HTTP Status Mapping

```csharp
int status = ErrorType.NotFound.ToHttpStatusCode();     // 404
ErrorType type = 401.ToErrorType();                      // Unauthorized
```
