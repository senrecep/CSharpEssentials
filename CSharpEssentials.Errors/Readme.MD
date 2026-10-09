# CSharpEssentials.Errors

Structured, type-safe error handling for .NET. Errors are data, not exceptions.

## Features

- **Error Struct**: Value-type error with code, description, type, and metadata.
- **Standard Error Types**: Failure, Validation, NotFound, Conflict, Unauthorized, Forbidden, Unexpected. `ErrorType` also has an `Unknown` member, which has no factory method.
- **ErrorMetadata**: Attach contextual data to errors fluently.
- **Exception Bridge**: Convert exceptions to `Error` or throw `DomainException` / `EnhancedValidationException`.
- **HTTP Mapping**: Built-in conversion between `ErrorType` and HTTP status codes.

Targets `net11.0`, `net10.0`, `net9.0` and `netstandard2.1`. Depends on `CSharpEssentials.Core` and `CSharpEssentials.Enums`.

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
Error forbidden = Error.Forbidden("Order.Forbidden", "You cannot cancel this order.");
Error unauthorized = Error.Unauthorized("Token.Expired", "The token has expired.");
Error unexpected = Error.Unexpected("Db.Down", "The database is unreachable.");
```

Every factory takes `code`, `description` and an optional `ErrorMetadata`, and all three have defaults: `Error.NotFound()` has the code `NotFound` and the description `A 'Not Found' error has occurred.` An `Error` exposes `Code`, `Description`, `Type`, `NumericType` (the `int` value of `Type`) and `Metadata` (`null` when none was given). It implements `IError`, and two errors are equal when type, code, description and metadata entries are equal.

### Error Metadata

```csharp
Error withMeta = Error.NotFound(
    "User.NotFound",
    "User was not found.",
    new ErrorMetadata()
        .AddMetadata("TraceId", Guid.NewGuid().ToString())
        .AddMetadata("RequestPath", "/api/users/123"));
```

`ErrorMetadata` is a `Dictionary<string, object?>`. `AddMetadata` keeps the existing value when the key is already present. `AddStackTrace`, `AddException` and `AddExceptionDetailed` add well-known keys (`stackTrace`, `exception`, `exceptionType`, `exceptionStackTrace`, `exceptionMessage`, `innerException`), `Combine` merges another instance without overwriting, and `CreateEmpty`, `CreateWithStackTrace`, `CreateWithException` and `CreateWithExceptionDetailed` are the static counterparts.

### Combining Errors

```csharp
Error a = Error.Validation("Name.Empty", "Name is required.");
Error b = Error.Validation("Email.Invalid", "Email is invalid.");

Error[] combined = a + b;

Error[] many = Error.CreateMany(a, b);                          // params of Error or of Error collections
Error[] single = a;                                             // implicit Error -> Error[]
```

### Converting from Exceptions

```csharp
try { /* ... */ }
catch (Exception ex)
{
    Error error = Error.Exception(ex);
}
```

`Error.Exception(ex)` uses the exception type name as the code, the exception message as the description and `ErrorType.Failure`. The metadata carries `exceptionType`, `exceptionStackTrace`, `exceptionMessage` and, when there is one, a nested `innerException`. Overloads take an explicit `code`, an explicit `code` and `description`, an `ErrorType` and extra `ErrorMetadata`:

```csharp
Error custom = Error.Exception("Orders.SaveFailed", "The order could not be saved.", ex, ErrorType.Unexpected);
```

### Domain Exceptions

```csharp
using CSharpEssentials.Errors;
using CSharpEssentials.Exceptions;

Error error = Error.Validation("Order.Invalid", "Total must be greater than zero.");
throw new DomainException(error);
```

`DomainException` and `EnhancedValidationException` live in the `CSharpEssentials.Exceptions` namespace of this package; see the [exception types](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Errors/Exceptions/Readme.MD).

### HTTP Status Mapping

```csharp
int status = ErrorType.NotFound.ToHttpStatusCode();     // 404
ErrorType type = 401.ToErrorType();                      // Unauthorized
```

| `ErrorType` | HTTP status |
|---|---|
| `Validation` | 400 |
| `Unauthorized` | 401 |
| `Forbidden` | 403 |
| `NotFound` | 404 |
| `Conflict` | 409 |
| `Failure`, `Unexpected`, `Unknown` | 500 |

`ToErrorType` is the reverse for 400, 401, 403, 404, 409 and 500 (`Failure`); every other status code gives `Unexpected`.

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
using CSharpEssentials.ResultPattern; // CSharpEssentials.Results package

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

This pattern provides discoverability (IDE autocomplete per domain), type-safe error codes, and centralised descriptions, all without inheritance.
