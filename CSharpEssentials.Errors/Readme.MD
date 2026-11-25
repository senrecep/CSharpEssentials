# CSharpEssentials.Errors

Errors as data instead of exceptions. `Error` is a value type with a code, description, `ErrorType` and optional `ErrorMetadata`. The package also converts exceptions to errors, maps `ErrorType` to HTTP status codes and back, and has two exception types that carry errors.

Targets `net11.0`, `net10.0`, `net9.0` and `netstandard2.1`. Depends on `CSharpEssentials.Core` and `CSharpEssentials.Enums`.

Benchmarks for creating errors, equality and hashing, errors from exceptions, metadata and record `ToString`, and JSON against ErrorOr and FluentResults are in [BENCHMARKS.md](./BENCHMARKS.md).

## Installation

```bash
dotnet add package CSharpEssentials.Errors
```

## Usage

### Creating errors

```csharp
using CSharpEssentials.Errors;

Error notFound = Error.NotFound("User.NotFound", "User was not found.");
Error validation = Error.Validation("Email.Invalid", "Email format is invalid.");
Error conflict = Error.Conflict("User.Duplicate", "User already exists.");
Error domain = Error.Domain("Payment.Declined", "The card was declined.");
Error failure = Error.Failure("Payment.GatewayDown", "The payment gateway did not respond.");
Error forbidden = Error.Forbidden("Order.Forbidden", "You cannot cancel this order.");
Error unauthorized = Error.Unauthorized("Token.Expired", "The token has expired.");
Error unexpected = Error.Unexpected("Db.Down", "The database is unreachable.");
```

`Failure`, `Unexpected` and `Unknown` map to HTTP 500. The AspNetCore problem details hide the description of a 5xx error unless `ExposeExceptionDetails` is on, so use `Domain` (422) for a business-rule rejection the client should read. The factories cover every `ErrorType` except `Unknown`; `Error.Create(code, description, type, metadata = null)` takes any `ErrorType`, so it can turn any `IError` into an `Error`. Each takes `code`, `description` and an optional `ErrorMetadata`, and all three have defaults: `Error.NotFound()` has the code `NotFound` and the description `A 'Not Found' error has occurred.` An `Error` exposes `Code`, `Description`, `Type`, `NumericType` (the `int` value of `Type`) and `Metadata` (`null` when none was given). It implements `IError`, and two errors are equal when type, code, description and metadata entries are equal. Metadata is compared by key, so entry order does not affect equality or `GetHashCode`.

### Error metadata

```csharp
Error withMeta = Error.NotFound(
    "User.NotFound",
    "User was not found.",
    new ErrorMetadata()
        .AddMetadata("TraceId", Guid.NewGuid().ToString())
        .AddMetadata("RequestPath", "/api/users/123"));
```

`ErrorMetadata` is a `Dictionary<string, object?>`. `AddMetadata` keeps the existing value when the key is already present. `AddStackTrace`, `AddException` and `AddExceptionDetailed` add well-known keys (`stackTrace`, `exception`, `exceptionType`, `exceptionStackTrace`, `exceptionMessage`, `innerException`), `Combine` merges another instance without overwriting, and `CreateEmpty`, `CreateWithStackTrace`, `CreateWithException` and `CreateWithExceptionDetailed` are the static counterparts. `exception` holds a nested `ErrorMetadata` with the `exceptionType`, `exceptionStackTrace`, `exceptionMessage` and `innerException` keys, so the metadata stays serializable with System.Text.Json.

### Combining errors

```csharp
Error a = Error.Validation("Name.Empty", "Name is required.");
Error b = Error.Validation("Email.Invalid", "Email is invalid.");

Error[] combined = a + b;

Error[] many = Error.CreateMany(a, b);                          // params of Error or of Error collections
Error[] single = a;                                             // implicit Error -> Error[]
```

### Converting from exceptions

```csharp
try { /* ... */ }
catch (Exception ex)
{
    Error error = Error.Exception(ex);
}
```

`Error.Exception(ex)` uses the exception type name as the code, the exception message as the description and `ErrorType.Unexpected` (before 9.0 it was `ErrorType.Failure`). The metadata carries `exceptionType`, `exceptionStackTrace` (`null` when the exception was never thrown), `exceptionMessage` and, when there is one, a nested `innerException`. Overloads take an explicit `code`, an explicit `code` and `description`, an `ErrorType` and extra `ErrorMetadata`:

```csharp
Error custom = Error.Exception("Orders.SaveFailed", "The order could not be saved.", ex, ErrorType.Unexpected);
```

### Domain exceptions

```csharp
using CSharpEssentials.Errors;
using CSharpEssentials.Exceptions;

Error error = Error.Validation("Order.Invalid", "Total must be greater than zero.");
throw new DomainException(error);
```

`DomainException` and `EnhancedValidationException` live in the `CSharpEssentials.Exceptions` namespace of this package; see the [exception types](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Errors/Exceptions/Readme.MD).

### HTTP status mapping

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
| `Domain` | 422 |
| `Failure`, `Unexpected`, `Unknown` | 500 |

`ToErrorType` is the reverse for 400, 401, 403, 404, 409, 422 (`Domain`) and 500 (`Failure`); every other status code gives `Unexpected`.

`Unknown` is the `[EnumFallback]` member of `ErrorType`: a data read (JSON with the `CSharpEssentials.Json` enum conventions, a cached or stored payload) maps a type name or number it does not know, such as a member added by a later version, to `Unknown`, which maps to 500. Request input through the AspNetCore enum conventions rejects `Unknown`. 8.x has no fallback member, so 8.x readers reject `Domain`; see the [8.x to 9.0 migration guide](../docs/migration/v8-to-v9.md).

### Domain-specific error hierarchies

`Error` is a `readonly record struct` and cannot be subclassed. Group static factories in one class per aggregate or bounded context instead (or use the typed kinds in the next section):

```csharp
using CSharpEssentials.Errors;

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
        Error.Domain(
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

### Typed error kinds

`TypedError` is an abstract record that implements `IError`. Each `ErrorType` except `Unknown` has a kind that derives from it:

| Kind | `Type` |
|---|---|
| `ValidationError` | `Validation` |
| `UnauthorizedError` | `Unauthorized` |
| `ForbiddenError` | `Forbidden` |
| `NotFoundError` | `NotFound` |
| `ConflictError` | `Conflict` |
| `DomainError` | `Domain` |
| `FailureError` | `Failure` |
| `UnexpectedError` | `Unexpected` |

The CLR type carries the kind, so code can match on the type instead of reading `Type`. The kinds are not sealed: derive a record from one to give an error its own type. The derived type keeps the kind's `Type` (the override is sealed). The constructor rejects a null `code` or `description`, and so does a `with` expression.

Write a catalog per aggregate with predefined instances, a `Codes` class of constants, and a nested type for any error that callers must tell apart by type:

```csharp
using CSharpEssentials.Errors;

public static class UserErrors
{
    public static class Codes
    {
        public const string NotFound = "User.NotFound";
        public const string Deleted = "User.Deleted";
        public const string Locked = "User.Locked";
    }

    public static readonly NotFoundError NotFound = new(Codes.NotFound, "User not found.");
    public static readonly NotFoundError Deleted = new(Codes.Deleted, "User was deleted.");

    public static NotFoundError NotFoundById(Guid id) => new(Codes.NotFound, $"User {id} not found.");

    public sealed record Locked(Guid Id) : ConflictError(Codes.Locked, "User is locked.");
}
```

SonarAnalyzer reports S3218 on `Codes.NotFound` because it shares a name with the `NotFound` field. The shadowing is harmless here; move the constants to a separate `UserErrorCodes` class if your build treats that rule as an error.

A static field cannot be a pattern, so match predefined instances on their constant code. Put the bare kind after the code arms:

```csharp
string message = error switch
{
    NotFoundError { Code: UserErrors.Codes.Deleted } => "User was deleted.",
    NotFoundError => "User not found.",
    UserErrors.Locked locked => $"User {locked.Id} is locked.",
    _ => error.Description,
};
```

`ValidationError.Field` reads the `field` metadata entry (`ErrorMetadata.FieldKey`), the property path that CSharpEssentials.Validation stores on each failure. Build the typed error from a validation failure with its metadata to keep it: `new ValidationError(error.Code, error.Description, error.Metadata).Field` is `Address.City`. `ErrorMetadata.GetField()` returns the same value from any metadata, including a string read back from JSON. `Field` is not serialized; the entry stays in `Metadata`.

`ToError()` turns a typed error into an `Error` with the same code, description and type, and the same `ErrorMetadata` instance, so it feeds today's `Result` and the AspNetCore problem details:

```csharp
Result<User> result = UserErrors.NotFoundById(id).ToError();
```

`[ErrorKind(ErrorType.X)]` declares the kind on the type itself, so tools can read it without an instance. Every built-in kind has it and derived types inherit it. Do not redeclare it with a different kind on a derived error: `Type` stays the base kind's, so the attribute would disagree with the runtime type. The CSE5005 analyzer reports that mismatch, and a `Type` property that returns another constant kind; it ships with the `CSharpEssentials.Results` package, so a project that references only `CSharpEssentials.Errors` does not get it. The attribute also works on a struct that implements `IError`:

```csharp
[ErrorKind(ErrorType.Conflict)]
public readonly record struct OrderLocked(int OrderId) : IError
{
    public string Code => "Order.Locked";
    public string Description => $"Order {OrderId} is locked.";
    public ErrorType Type => ErrorType.Conflict;
    public int NumericType => (int)Type;
    public ErrorMetadata? Metadata => null;
}
```

A struct error is boxed whenever it is used as `IError`, so prefer the class kinds. Bridge it with `Error.Create(e.Code, e.Description, e.Type, e.Metadata)`. Nothing stops `default(OrderLocked)`, and a struct that stores its code in a field returns a null `Code` from `default`, which breaks the `IError` contract.

Equality:

- Two typed errors are equal when the runtime type, code, description and type match and the metadata has the same content (compared like `Error` compares it).
- A derived error never equals its base kind, even with the same code and description.
- Equality does not survive serialization when metadata holds values: System.Text.Json reads them back as `JsonElement`. Code and type do survive.
- Tell errors apart by `Code`, not by comparing values across kinds or after a round trip.

Predefined instances share one `ErrorMetadata` dictionary, which is mutable. Do not add metadata to a shared instance; create a new error with its own metadata instead.

## Upgrading to 8.0

`ErrorMetadata.CreateWithException` and `AddException` no longer store the `Exception` object under `exception`. They store a nested `ErrorMetadata` with `exceptionType`, `exceptionStackTrace`, `exceptionMessage` and `innerException`, because System.Text.Json cannot serialize a thrown exception. Code that cast `metadata["exception"]` to `Exception` should read the nested keys instead, or keep the exception in its own variable.

The nested entry keeps only the short type name (`GetType().Name`), the message, the stack trace and the inner exception chain. The full type name, `Data`, `HResult` and the exception object itself are dropped, so code that rethrew `metadata["exception"]` or tested it with `is SqlException` has no key to migrate to. Keep the exception in its own variable if you need its identity.

`exceptionStackTrace` holds `exception.StackTrace` and nothing else. An exception that was never thrown has no stack trace, so the key is present with a `null` value. Earlier versions fell back to `Environment.StackTrace`, which recorded where the metadata was built rather than where the exception came from and cost a full stack walk on every call. Call `AddStackTrace()` if you want the current call stack. Because the key no longer depends on the call site, `Error.Exception` values built from the same unthrown exception now compare equal.

`Error` equality and `GetHashCode` now compare nested `ErrorMetadata` values by content. Do not add a metadata instance to itself (directly or through a chain): hashing or comparing such a cycle overflows the stack.

`Code` and `Description` reject `null` in `with` expressions too. `error with { Code = null! }` used to produce an `Error` with a null code; it now throws `ArgumentNullException`, as the factory methods already did.

`ErrorMetadata.ToString()` formats values with the invariant culture on every target. The netstandard2.1 build, which net8.0 apps load, used the current culture, so `2.5` came out as `2,5` under `tr-TR`. Code that parsed that text with the current culture should parse it as invariant.
