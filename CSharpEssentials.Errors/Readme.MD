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

### Domain-Specific Error Hierarchies

`Error` is a value type (`readonly record struct`) and cannot be subclassed. The idiomatic way to build domain-specific, typed error hierarchies is to group static factory methods in a dedicated class per aggregate or bounded context:

```csharp
using CSharpEssentials.Errors;

// Organize errors by domain aggregate
public static class UserErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("User.NotFound", $"User '{id}' was not found.");

    public static readonly Error AlreadyExists =
        Error.Conflict("User.AlreadyExists", "A user with that email already exists.");

    public static Error InvalidAge(int age) =>
        Error.Validation("User.InvalidAge", $"Age {age} is not allowed; must be 18 or older.");

    public static readonly Error Unauthorized =
        Error.Unauthorized("User.Unauthorized", "You are not authorized to perform this action.");
}

public static class OrderErrors
{
    public static readonly Error EmptyCart =
        Error.Validation("Order.EmptyCart", "Cannot place an order with an empty cart.");

    public static Error InsufficientFunds(decimal required, decimal available) =>
        Error.Failure(
            "Order.InsufficientFunds",
            $"Payment requires {required:C} but only {available:C} is available.",
            new ErrorMetadata().AddMetadata("Required", required).AddMetadata("Available", available));
}
```

Usage with `Result`:

```csharp
using CSharpEssentials.ResultPattern;

public Result<User> FindUser(Guid id)
{
    User? user = _repo.Find(id);
    if (user is null)
        return UserErrors.NotFound(id);   // implicit Error → Result<User>

    return user;
}

public Result PlaceOrder(Cart cart, decimal balance)
{
    if (cart.IsEmpty)
        return OrderErrors.EmptyCart;

    if (balance < cart.Total)
        return OrderErrors.InsufficientFunds(cart.Total, balance);

    return Result.Success();
}
```

This pattern provides discoverability (IDE autocomplete per domain), type-safe error codes, and centralised descriptions — without inheritance.
