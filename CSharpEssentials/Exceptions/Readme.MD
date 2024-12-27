# Exception Types in CSharpEssentials

CSharpEssentials provides custom exception types that enhance error handling capabilities in your applications. These exceptions are designed to work seamlessly with the functional programming paradigm and the Error management system.

## Available Exception Types

### 1. DomainException

`DomainException` is designed for domain-specific business rule violations. It takes an `Error` object as a parameter and exposes it through the `Error` property.

```csharp
public class DomainException : Exception
{
    public DomainException(Error error) : base(error.Description)
    {
        Error = error;
    }

    public Error Error { get; }

    // Usage example:
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
        // Access the original error
        var originalError = ex.Error;
    }
}
```

### 2. EnhancedValidationException

`EnhancedValidationException` is designed for handling multiple validation errors. It takes an array of `Error` objects and provides access to them through the `Errors` property.

```csharp
public class EnhancedValidationException : Exception
{
    public EnhancedValidationException(Error[] errors)
        : base($"Validation failed with {errors.Length} errors")
    {
        Errors = errors;
    }

    public Error[] Errors { get; }

    // Usage example:
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
}
```

## Integration with Error System

These exceptions are designed to work directly with the Error system:

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
        return Result.Failure<User>(ex.Error);
    }
    catch (EnhancedValidationException ex)
    {
        return Result.Failure<User>(ex.Errors);
    }
}
```

## Best Practices

1. **Use DomainException for Single Errors**: When you have a single business rule violation or domain error.
2. **Use EnhancedValidationException for Multiple Errors**: When you need to collect and report multiple validation errors at once.
3. **Error Creation**: Create appropriate Error objects using the Error factory methods (Error.Validation, Error.Failure, etc.).
4. **Error Properties**: Make use of the Error properties (Code, Description, Type) when creating errors.
5. **Result Integration**: Use the exceptions with the Result pattern for functional error handling.

## Example Scenarios

### Domain Exception Example

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

### Enhanced Validation Example

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
