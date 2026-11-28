---
name: csharpessentials-errors
description: Use when creating error values for Result returns, error catalogs or HTTP status mapping. Covers Error factories (Validation, NotFound, Conflict, Unauthorized, Forbidden, Domain, Failure, Unexpected, Exception, Create), ErrorMetadata.AddMetadata, ErrorType.ToHttpStatusCode/ToErrorType, typed kinds (NotFoundError, ConflictError, ValidationError, ...) with ToError and [ErrorKind], and DomainException/EnhancedValidationException.
---

# CSharpEssentials.Errors

`Error` is a `readonly record struct` with `Code`, `Description`, `Type` (`ErrorType`), `NumericType` and `Metadata`. Typed kinds such as `NotFoundError` are records that put the error type in a signature for `Result<T, E1..E3>`.

## Install

```bash
dotnet add package CSharpEssentials.Errors
```

## Namespaces

```csharp
using CSharpEssentials.Errors;      // Error, ErrorType, ErrorMetadata, IError, TypedError and kinds, ErrorKindAttribute
using CSharpEssentials.Exceptions;  // DomainException, EnhancedValidationException
```

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials.Errors
// dotnet add package CSharpEssentials.Results
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

Result<User> found = UserRepository.Find(1);
Result<User> missing = UserRepository.Find(2);
Result<User> underage = UserRepository.Register("Bob", 15);

Console.WriteLine(found.Value.Name);                                         // Ada
Console.WriteLine($"{missing.FirstError.Code} {missing.FirstError.Type.ToHttpStatusCode()}");   // User.NotFound 404
Console.WriteLine($"{underage.FirstError.Code} {underage.FirstError.Metadata!["age"]}");         // User.Underage 15

public sealed record User(int Id, string Name);

public static class UserErrors
{
    public static Error NotFound(int id) =>
        Error.NotFound("User.NotFound", $"User {id} was not found.");

    public static readonly Error AlreadyExists =
        Error.Conflict("User.AlreadyExists", "A user with that email already exists.");

    public static Error Underage(int age) =>
        Error.Domain("User.Underage", $"Age {age} is below 18.", new ErrorMetadata().AddMetadata("age", age));
}

public static class UserRepository
{
    public static Result<User> Find(int id) => id == 1 ? new User(1, "Ada") : UserErrors.NotFound(id);

    public static Result<User> Register(string name, int age)
    {
        if (age < 18)
            return UserErrors.Underage(age);   // Error -> Result<User>
        return new User(2, name);
    }
}
```

## Creating errors

```csharp
Error.Validation("Email.Invalid", "Email format is invalid.");        // 400
Error.Unauthorized("Token.Expired", "Token has expired.");           // 401
Error.Forbidden("Access.Denied", "Insufficient permissions.");       // 403
Error.NotFound("User.NotFound", "User not found.");                  // 404
Error.Conflict("Email.Taken", "Email is already registered.");       // 409
Error.Domain("Payment.Declined", "The card was declined.");          // 422: business rule rejected a valid request
Error.Failure("Order.Failed", "Order could not be processed.");      // 500
Error.Unexpected("Sys.Error", "An unexpected error occurred.");      // 500
Error.Exception(ex);                                                 // code = type name, description = message, Type = Unexpected
Error.Exception("Orders.SaveFailed", "Could not save.", ex, ErrorType.Failure);
Error.Create("Code", "Description", ErrorType.Unknown, metadata);    // any ErrorType, also to bridge an IError
```

Every factory has optional `code`, `description` and `metadata` parameters (`Error.NotFound()` has the code `NotFound`). `ErrorType` members: `Failure`, `Unexpected`, `Validation`, `Conflict`, `NotFound`, `Unauthorized`, `Forbidden`, `Unknown` (no factory, fallback for unknown values in data), `Domain`.

## Metadata

```csharp
var metadata = new ErrorMetadata()
    .AddMetadata("traceId", traceId)        // keeps an existing key
    .Combine(other);                        // merges without overwriting
var single = new ErrorMetadata("orderId", 42);
Error error = Error.NotFound("Order.NotFound", "Order not found.", metadata);
string? field = error.Metadata?.GetField(); // "field" entry (ErrorMetadata.FieldKey)
```

Also: `AddStackTrace()`, `AddException(ex)`, `AddExceptionDetailed(ex)`, `CreateEmpty()`, `CreateWithStackTrace()`, `CreateWithException(ex)`, `CreateWithExceptionDetailed(ex)`.

## Combining and mapping

```csharp
Error[] pair = error1 + error2;
Error[] many = Error.CreateMany(error1, error2, error3);
Error[] one = error1;                                   // implicit
int status = ErrorType.NotFound.ToHttpStatusCode();     // 404
ErrorType type = 422.ToErrorType();                     // Domain (unknown codes -> Unexpected)
int number = ErrorType.Domain.ToIntType();              // 8
```

`Validation` 400, `Unauthorized` 401, `Forbidden` 403, `NotFound` 404, `Conflict` 409, `Domain` 422, `Failure`/`Unexpected`/`Unknown` 500. `ToErrorType` maps 500 to `Failure`.

## Typed kinds

```csharp Program.cs
// dotnet add package CSharpEssentials.Errors
// dotnet add package CSharpEssentials.Results
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

Result<int, NotFoundError, OrderErrors.Locked> result = new OrderErrors.Locked(7);
Console.WriteLine(result.Match(id => "ok", nf => nf.Code, locked => $"{locked.Code} {locked.OrderId}")); // Order.Locked 7

Error bridged = OrderErrors.NotFound.ToError();             // kind -> Error, for Result<T>
Result<int> classic = bridged;
Console.WriteLine($"{classic.FirstError.Code} {classic.FirstError.Type}"); // Order.NotFound NotFound

var invalid = new ValidationError("City.Empty", "City is required.", new ErrorMetadata(ErrorMetadata.FieldKey, "Address.City"));
Console.WriteLine(invalid.Field);                           // Address.City

public static class OrderErrors
{
    public static class Codes
    {
        public const string NotFound = "Order.NotFound";
        public const string Locked = "Order.Locked";
    }

    public static readonly NotFoundError NotFound = new(Codes.NotFound, "Order not found.");
    public sealed record Locked(int OrderId) : ConflictError(Codes.Locked, "Order is locked.");
}
```

Kinds: `ValidationError`, `UnauthorizedError`, `ForbiddenError`, `NotFoundError`, `ConflictError`, `DomainError`, `FailureError`, `UnexpectedError`, all derived from `TypedError` and constructed with `(code, description, metadata = null)`. A derived record keeps the base kind's `Type`. Match predefined instances on the constant code: `NotFoundError { Code: OrderErrors.Codes.NotFound }`.

`[ErrorKind(ErrorType.Conflict)]` declares the kind of a type (class or struct implementing `IError`). Built-in kinds already have it; never redeclare a different kind on a derived record (CSE5005).

## Exceptions

```csharp
throw new DomainException(Error.Domain("Order.Closed", "Order is closed."));     // ex.Error, Message = Description
throw new EnhancedValidationException([emailError, passwordError]);             // ex.Errors, Message = "Validation failed with 2 errors"
```

Catch at the boundary and return `ex.Error` or `ex.Errors` as a `Result`. `CSharpEssentials.AspNetCore` maps both to problem details.

## Rules

- There is no `.Message` on `Error`; use `Description`. There is no public `WithMetadata`; pass metadata to the factory or use `AddMetadata` on an `ErrorMetadata`.
- Use `Error.Domain` (422) for a business rule the client should read. `Failure`/`Unexpected` are 500 and their description is hidden by the AspNetCore problem details.
- `Error.Exception(ex)` gives `ErrorType.Unexpected` (9.0+). Pass `ErrorType.Failure` explicitly for the old behavior.
- `Error` is a struct: it cannot be subclassed. Group errors in static catalog classes; use a method when the description has runtime data.
- `Error` equality includes metadata. An expected `Error.Validation("Name.NotEmpty", "...")` without metadata does not equal one recorded by CSharpEssentials.Validation (it carries `field`). Compare `Code`.
- An `Error` does not convert to a typed kind or a typed `Result<T, E..>` slot (CS0029). Make catalog fields typed kinds; call `ToError()` to go from kind to `Error`. `kind == error` does not compile.
- A nested error record is a type: `return OrderErrors.Locked;` fails (CS0119); write `new OrderErrors.Locked(id)`.
- Do not add metadata to a shared `static readonly` error; its `ErrorMetadata` is shared and mutable. Never add an `ErrorMetadata` to itself.
- Typed errors with metadata are not equal after a JSON round trip (values become `JsonElement`). Compare `Code`.

Full reference: [Errors Readme](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Errors/Readme.MD).
