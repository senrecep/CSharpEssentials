---
name: csharpessentials-errors
description: Use when creating structured error values — Error factory methods (Failure/Validation/NotFound/Conflict/Unauthorized/Forbidden/Unexpected), ErrorMetadata for contextual data, HTTP status mapping, and domain-specific static error class hierarchies.
---

# CSharpEssentials.Errors

Errors are values, not exceptions. `Error` is a `readonly record struct` with a code, description, type, and optional metadata.

## Installation

```bash
dotnet add package CSharpEssentials.Errors
```

## Namespace

```csharp
using CSharpEssentials.Errors;
```

## Creating Errors

```csharp
// ErrorType: Failure | Unexpected | Validation | Conflict | NotFound | Unauthorized | Forbidden
Error.Failure("order.failed",    "Order could not be processed.")
Error.Validation("email.invalid","Email format is invalid.")
Error.NotFound("user.not_found", "User not found.")
Error.Conflict("email.taken",    "Email is already registered.")
Error.Unauthorized("token.expired", "Token has expired.")
Error.Forbidden("access.denied", "Insufficient permissions.")
Error.Unexpected("sys.error",    "An unexpected error occurred.")
Error.Exception(ex)              // wrap exception → Error
```

## Error Properties

```csharp
error.Code         // "email.invalid"
error.Description  // "Email format is invalid." — NOT .Message
error.Type         // ErrorType.Validation
error.Metadata     // ErrorMetadata? (nullable)
```

## ErrorMetadata

```csharp
Error withMeta = Error.NotFound(
    "User.NotFound",
    "User was not found.",
    new ErrorMetadata()
        .AddMetadata("TraceId", traceId)
        .AddMetadata("RequestPath", "/api/users/1"));
// .AddMetadata() — NOT .WithMetadata()
```

## Combining Errors

```csharp
Error[] merged = error1 + error2;              // operator +
Error[] many   = Error.CreateMany(e1, e2, e3);
```

## HTTP Status Mapping

```csharp
int status   = ErrorType.NotFound.ToHttpStatusCode();  // 404
int status2  = ErrorType.Validation.ToHttpStatusCode(); // 400
ErrorType et = 401.ToErrorType();                       // Unauthorized
```

## Domain-Specific Error Hierarchies

`Error` is a `readonly record struct` — it cannot be subclassed. Use static classes per aggregate:

```csharp
public static class UserErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("User.NotFound", $"User '{id}' was not found.");

    public static readonly Error AlreadyExists =
        Error.Conflict("User.AlreadyExists", "A user with that email already exists.");

    public static Error InvalidAge(int age) =>
        Error.Validation("User.InvalidAge", $"Age {age} is invalid; must be 18 or older.");

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
            $"Payment requires {required:C} but only {available:C} available.",
            new ErrorMetadata()
                .AddMetadata("Required", required)
                .AddMetadata("Available", available));
}

// Usage — implicit Error → Result<T>
public Result<User> FindUser(Guid id)
{
    User? user = _repo.Find(id);
    return user is null ? UserErrors.NotFound(id) : user;
}
```

## Domain Exceptions

```csharp
using CSharpEssentials.Exceptions;

throw new DomainException(Error.Validation("Order.Invalid", "Total must be greater than zero."));
```

## Best Practices

- Group errors in static classes per aggregate for IDE autocomplete + type-safe codes
- Use `error.Description` — the field is named `Description`, not `Message`
- `ErrorMetadata` uses `.AddMetadata()` — there is no `.WithMetadata()`
- Prefer factory methods (parameterized) over static readonly fields when the message includes runtime data
- `Error` is a value type — safe to use as dictionary key, in switch expressions, etc.
