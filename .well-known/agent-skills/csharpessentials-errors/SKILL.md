---
name: csharpessentials-errors
description: Use when creating structured error values. Covers Error factory methods (Failure/Validation/NotFound/Conflict/Unauthorized/Forbidden/Unexpected), ErrorMetadata for contextual data, HTTP status mapping, and domain-specific static error class hierarchies.
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

## Creating errors

```csharp
// ErrorType: Failure | Unexpected | Validation | Conflict | NotFound | Unauthorized | Forbidden | Unknown (no factory)
Error failure      = Error.Failure("order.failed", "Order could not be processed.");
Error validation   = Error.Validation("email.invalid", "Email format is invalid.");
Error notFound     = Error.NotFound("user.not_found", "User not found.");
Error conflict     = Error.Conflict("email.taken", "Email is already registered.");
Error unauthorized = Error.Unauthorized("token.expired", "Token has expired.");
Error forbidden    = Error.Forbidden("access.denied", "Insufficient permissions.");
Error unexpected   = Error.Unexpected("sys.error", "An unexpected error occurred.");
Error fromEx       = Error.Exception(ex); // code = exception type name, description = message, metadata = exception details
```

## Error properties

```csharp
string code          = error.Code;        // "email.invalid"
string description   = error.Description; // "Email format is invalid." (there is no .Message)
ErrorType type       = error.Type;        // ErrorType.Validation
int numeric          = error.NumericType; // int value of Type
ErrorMetadata? meta  = error.Metadata;    // nullable
```

Every factory has defaults for `code` and `description` (`Error.NotFound()` has the code `NotFound`). Two errors are equal when type, code, description and metadata entries are equal; metadata order does not matter.

## ErrorMetadata

```csharp
Error withMeta = Error.NotFound(
    "User.NotFound",
    "User was not found.",
    new ErrorMetadata()
        .AddMetadata("TraceId", traceId)
        .AddMetadata("RequestPath", "/api/users/1"));
// .AddMetadata(): NOT .WithMetadata(); an existing key is kept, not overwritten
```

## Combining errors

```csharp
Error[] merged = error1 + error2;              // operator +
Error[] many   = Error.CreateMany(error1, error2, error3);
```

## HTTP status mapping

```csharp
int status   = ErrorType.NotFound.ToHttpStatusCode();  // 404
int status2  = ErrorType.Validation.ToHttpStatusCode(); // 400 (Failure/Unexpected/Unknown map to 500)
ErrorType et = 401.ToErrorType();                       // Unauthorized
```

`Validation` 400, `Unauthorized` 401, `Forbidden` 403, `NotFound` 404, `Conflict` 409. `ToErrorType` reverses 400, 401, 403, 404, 409 and 500 (`Failure`); any other status gives `Unexpected`.

## Domain-specific error classes

`Error` is a `readonly record struct`, so it cannot be subclassed. Use static classes per aggregate:

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

// Usage: implicit Error → Result<T>
public Result<User> FindUser(Guid id)
{
    User? user = _repo.Find(id);
    return user is null ? UserErrors.NotFound(id) : user;
}
```

## Domain exceptions

```csharp
using CSharpEssentials.Exceptions;

throw new DomainException(Error.Validation("Order.Invalid", "Total must be greater than zero."));  // .Error holds the Error

throw new EnhancedValidationException(new[] { emailError, passwordError });                         // .Errors holds the Error[]
```

## Rules

- Group errors in static classes per aggregate.
- Use a factory method instead of a `static readonly` field when the description includes runtime data.
- `Error` is a value type with value equality, usable as a dictionary key or in switch expressions. Do not add an `ErrorMetadata` to itself: hashing or comparing the cycle overflows the stack.
