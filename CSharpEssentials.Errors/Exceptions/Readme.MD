# Exception types in CSharpEssentials

CSharpEssentials has two exception types that carry `Error` values, so code that throws can still hand typed errors to the `Result` pattern. Both live in the `CSharpEssentials.Exceptions` namespace of the `CSharpEssentials.Errors` package.

```csharp
using CSharpEssentials.Errors;
using CSharpEssentials.Exceptions;
```

## Available exception types

### 1. DomainException

`DomainException` is for business rule violations in the domain. It takes an `Error` and exposes it through the `Error` property. The exception `Message` is the `Description` of the error.

```csharp
public class DomainException : Exception
{
    public DomainException(Error error)
        : base(error.Description) => Error = error;

    public Error Error { get; }
}
```

```csharp
try
{
    var error = Error.Validation(
        code: "INVALID_AGE",
        description: "User must be at least 18 years old"
    );
    throw new DomainException(error);
}
catch (DomainException ex)
{
    // ex.Message is "User must be at least 18 years old"
    var originalError = ex.Error;
}
```

### 2. EnhancedValidationException

`EnhancedValidationException` carries several validation errors. It takes an `Error[]` and exposes it through the `Errors` property. The exception `Message` is `Validation failed with {n} errors`.

```csharp
public class EnhancedValidationException : Exception
{
    public EnhancedValidationException(Error[] errors)
        : base($"Validation failed with {errors.Length} errors") => Errors = errors ?? throw new ArgumentNullException(nameof(errors));

    public Error[] Errors { get; }
}
```

```csharp
try
{
    var errors = new[]
    {
        Error.Validation("EMAIL_INVALID", "Invalid email format"),
        Error.Validation("PASSWORD_TOO_SHORT", "Password must be at least 8 characters")
    };

    throw new EnhancedValidationException(errors);
}
catch (EnhancedValidationException ex)
{
    // Access all validation errors
    foreach (var error in ex.Errors)
    {
        Console.WriteLine($"Code: {error.Code}, Description: {error.Description}");
    }
}
```

## Integration with the error system

Catch either exception and return its errors as a result. The `Result` types come from the `CSharpEssentials.Results` package (`using CSharpEssentials.ResultPattern;`), and an `Error` or an `Error[]` converts implicitly to `Result<T>`:

```csharp
public Result<User> CreateUser(UserDto dto)
{
    try
    {
        if (!IsValidEmail(dto.Email))
        {
            var error = Error.Validation(
                code: "INVALID_EMAIL",
                description: "Email format is not valid"
            );
            throw new DomainException(error);
        }

        if (!IsValidPassword(dto.Password))
        {
            var errors = new[]
            {
                Error.Validation("PASSWORD_LENGTH", "Password must be at least 8 characters"),
                Error.Validation("PASSWORD_COMPLEXITY", "Password must contain special characters")
            };
            throw new EnhancedValidationException(errors);
        }

        return new User(dto);
    }
    catch (DomainException ex)
    {
        return ex.Error;
    }
    catch (EnhancedValidationException ex)
    {
        return ex.Errors;
    }
}
```

## ASP.NET Core

With `CSharpEssentials.AspNetCore`, its global exception handler turns a `DomainException` into a ProblemDetails response for its `Error`, using the HTTP status of the error's `ErrorType`, and an `EnhancedValidationException` into a 400 response that carries its errors.

## Best practices

Throw `DomainException` for a single business rule violation or domain error, and `EnhancedValidationException` when you collect several validation errors and report them at once. Create the errors with the `Error` factory methods (`Error.Validation`, `Error.Failure` and so on) and use their `Code`, `Description` and `Type` properties. Where the caller works with `Result`, catch the exception and return its errors, as in the example above.

## Example scenarios

### Domain exception example

```csharp
public void ProcessOrder(Order order)
{
    if (order.Items.Count == 0)
    {
        var error = Error.Validation(
            code: "ORDER_EMPTY",
            description: "Order must contain at least one item"
        );
        throw new DomainException(error);
    }

    if (order.TotalAmount <= 0)
    {
        var error = Error.Validation(
            code: "INVALID_AMOUNT",
            description: "Order total amount must be greater than zero"
        );
        throw new DomainException(error);
    }
}
```

### Enhanced validation example

```csharp
public void ValidateUser(UserDto user)
{
    var errors = new List<Error>();

    if (string.IsNullOrEmpty(user.Email))
    {
        errors.Add(Error.Validation(
            code: "EMAIL_REQUIRED",
            description: "Email is required"
        ));
    }

    if (user.Password?.Length < 8)
    {
        errors.Add(Error.Validation(
            code: "PASSWORD_TOO_SHORT",
            description: "Password must be at least 8 characters"
        ));
    }

    if (errors.Any())
    {
        throw new EnhancedValidationException(errors.ToArray());
    }
}
```
