# Error Types in CSharpEssentials

CSharpEssentials provides a robust error management system designed according to functional programming principles. This system treats errors as values and aims to avoid side effects.

## Error Structure

`Error` is designed as an immutable `readonly record struct` with the following properties:

- `Code`: Unique error code
- `Description`: Error description
- `Type`: Error type (`ErrorType` enum)
- `NumericType`: Numeric value of the error type
- `Metadata`: Additional information about the error (optional)

## Error Types

The `ErrorType` enum includes the following error types:

- `Failure`: General failure cases
- `Unexpected`: Unexpected errors
- `Validation`: Validation errors
- `Conflict`: Conflict situations
- `NotFound`: Not found situations
- `Unauthorized`: Unauthorized access
- `Forbidden`: Forbidden access
- `Unknown`: Unknown errors

## Usage Examples

### 1. Simple Error Creation

```csharp
// Creating a validation error
var validationError = Error.Validation(
    code: "USER_INVALID_AGE",
    description: "User age must be greater than 18."
);

// Creating a not found error
var notFoundError = Error.NotFound(
    code: "USER_NOT_FOUND",
    description: "User with specified ID was not found."
);
```

### 2. Error with Metadata

```csharp
var metadata = new ErrorMetadata()
    .Add("userId", 123)
    .Add("attemptCount", 3);

var error = Error.Unauthorized(
    code: "INVALID_LOGIN",
    description: "Invalid login attempt",
    metadata: metadata
);
```

### ErrorMetadata Factory Methods

ErrorMetadata provides several factory methods and fluent extension methods for creating and manipulating metadata:

#### Static Factory Methods

```csharp
// Create an empty metadata
var emptyMetadata = ErrorMetadata.CreateEmpty();

// Create metadata with stack trace
var stackTraceMetadata = ErrorMetadata.CreateWithStackTrace();

// Create metadata with exception
var exceptionMetadata = ErrorMetadata.CreateWithException(exception);

// Create detailed exception metadata (includes type, stack trace, message, and inner exceptions)
var detailedExceptionMetadata = ErrorMetadata.CreateWithExceptionDetailed(exception);
```

#### Fluent Extension Methods

```csharp
// Add stack trace to existing metadata
var metadata = new ErrorMetadata()
    .Add("userId", 123)
    .AddStackTrace();

// Add exception to existing metadata
metadata.AddException(exception);

// Add detailed exception information
metadata.AddExceptionDetailed(exception);

// Combine two metadata objects
var combinedMetadata = metadata.Combine(otherMetadata);
```

The `CreateWithExceptionDetailed` method creates a rich metadata structure:

- `exceptionType`: The type name of the exception
- `exceptionStackTrace`: The stack trace of the exception
- `exceptionMessage`: The exception message
- `innerException`: Nested metadata for inner exceptions (if any)

### 3. Creating Error from Exception

```csharp
try
{
    // ... some code that might throw
}
catch (Exception ex)
{
    var error = Error.Exception(
        code: "DB_CONNECTION_ERROR",
        exception: ex,
        type: ErrorType.Failure
    );
}
```

### 4. Creating Multiple Errors

```csharp
var errors = Error.CreateMany([
    Error.Validation("FIELD_REQUIRED", "Email field is required."),
    Error.Validation("INVALID_FORMAT", "Invalid email format.")
]);
```

## Best Practices

1. **Unique Codes**: Use meaningful and unique codes for each error.
2. **Descriptive Messages**: Write clear and understandable error messages.
3. **Proper Type Selection**: Choose the most appropriate `ErrorType` for the error's nature.
4. **Metadata Usage**: Store necessary debugging and logging information in metadata.
5. **Immutability**: Don't try to modify error objects, create new instances instead.

## Integration with Result Pattern

The Error type works most effectively when used with the Result pattern:

```csharp
public Result<User> CreateUser(UserDto dto)
{
    if (!dto.IsValid)
        return Error.Validation("INVALID_USER", "Invalid user information");

    if (UserExists(dto.Email))
        return Error.Conflict("USER_EXISTS", "This email is already in use");

    // ... in case of success
    return new User(dto);
}
```

## Why Use This Error System?

1. **Functional Approach**: Treats errors as values rather than using exceptions for control flow.
2. **Type Safety**: Provides compile-time safety through strong typing.
3. **Immutability**: Prevents accidental modifications and ensures thread safety.
4. **Rich Error Context**: Allows including detailed metadata without breaking encapsulation.
5. **HTTP Integration**: Error types naturally map to HTTP status codes.
6. **Predictable Error Handling**: Makes error handling more predictable and easier to test.
