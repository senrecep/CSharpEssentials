# Result Pattern in CSharpEssentials

[![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Results.svg)](https://www.nuget.org/packages/CSharpEssentials.Results) [![NuGet](https://img.shields.io/nuget/dt/CSharpEssentials.Results.svg)](https://www.nuget.org/packages/CSharpEssentials.Results) [![GitHub](https://img.shields.io/github/stars/senrecep/CSharpEssentials.svg)](https://github.com/senrecep/CSharpEssentials)

CSharpEssentials provides a robust implementation of the Result pattern, inspired by functional programming principles and popular libraries like CSharpFunctionalExtensions and ErrorOr. This implementation helps you handle success and failure cases in a more functional and type-safe way.

## Core Features

- Immutable result types (`Result` and `Result<T>`)
- Support for multiple errors
- Rich error context with the `Error` type
- Functional composition through LINQ-like methods
- Thread-safe operations
- JSON serialization support
- Comprehensive type safety with nullable reference types

## Basic Usage

### Result Type (No Value)

```csharp
// Success case
Result success = Result.Success();

// Failure case with single error
Result failure = Result.Failure(
    Error.Validation("INVALID_INPUT", "The input is invalid")
);

// Failure case with multiple errors
Result multipleErrors = Result.Failure([
    Error.Validation("REQUIRED_FIELD", "Email is required"),
    Error.Validation("INVALID_FORMAT", "Invalid email format")
]);
```

### Result<T> Type (With Value)

```csharp
// Success case with value
Result<User> success = Result<User>.Success(new User { Id = 1, Name = "John" });

// Failure case
Result<User> failure = Result<User>.Failure(
    Error.NotFound("USER_NOT_FOUND", "User with specified ID was not found")
);
```

## Advanced Features

### Combining Results

```csharp
// Using the + operator
Result<int[]> combined = successResult1 + successResult2;

// Using And method
Result<(User, Order)> userWithOrder =
    await GetUserAsync(userId)
        .And(user => GetOrderAsync(user.LastOrderId));
```

### Result Transformation

```csharp
// Map successful results
Result<string> userNameResult = userResult.Map(user => user.Name);

// Handle both success and failure cases
string userName = userResult.Match(
    success: user => user.Name,
    failure: errors => "Unknown User"
);
```

### Validation Chains

```csharp
public Result<User> CreateUser(UserDto dto)
{
    return Result<UserDto>.Success(dto)
        .Ensure(d => !string.IsNullOrEmpty(d.Email),
            Error.Validation("EMAIL_REQUIRED", "Email is required"))
        .Ensure(d => d.Age >= 18,
            Error.Validation("INVALID_AGE", "User must be at least 18 years old"))
        .Map(d => new User(d));
}
```

### Error Handling

```csharp
Result<User> result = await GetUserAsync(userId);

if (result.IsFailure)
{
    // Access error information
    Error firstError = result.FirstError;
    Error lastError = result.LastError;
    Error[] allErrors = result.Errors;

    // Error details
    string errorCode = firstError.Code;
    string description = firstError.Description;
    ErrorType errorType = firstError.Type;
    ErrorMetadata metadata = firstError.Metadata;
}
```

### Type Safety

The implementation uses C# nullable reference types and attributes to provide compile-time type safety:

```csharp
Result<User> result = GetUser();

// Compiler ensures Value is only accessible on success
if (result.IsSuccess)
{
    User user = result.Value; // Safe access
}

// Compiler ensures Errors is only accessible on failure
if (result.IsFailure)
{
    Error[] errors = result.Errors; // Safe access
}
```

## Best Practices

1. **Prefer Early Returns**: Return failures as soon as validation fails

   ```csharp
   if (!IsValid(input))
       return Result.Failure(Error.Validation("INVALID_INPUT"));
   ```

2. **Use Meaningful Error Codes**: Make error codes descriptive and consistent

   ```csharp
   const string USER_NOT_FOUND = "USER_NOT_FOUND";
   const string INVALID_EMAIL = "INVALID_EMAIL";
   ```

3. **Leverage Type Safety**: Use the compiler to prevent null reference exceptions

   ```csharp
   // Compiler will warn if you try to access Value without checking IsSuccess
   var user = result.IsSuccess ? result.Value : null;
   ```

4. **Chain Operations**: Use functional composition for clean code
   ```csharp
   return await GetUserAsync(userId)
       .Ensure(user => user.IsActive, Error.Validation("USER_INACTIVE"))
       .Map(user => user.ToDto())
       .Tap(dto => _logger.LogInfo($"User {dto.Id} retrieved"));
   ```

## Integration with ASP.NET Core

```csharp
public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return Results.Ok(result.Value);

        var error = result.FirstError;
        return error.Type switch
        {
            ErrorType.NotFound => Results.NotFound(error),
            ErrorType.Validation => Results.BadRequest(error),
            ErrorType.Unauthorized => Results.Unauthorized(),
            ErrorType.Forbidden => Results.Forbidden(),
            _ => Results.StatusCode(500)
        };
    }
}
```

## Why Use This Pattern?

1. **Type Safety**: Compile-time guarantees for error handling
2. **Immutability**: Thread-safe and side-effect free
3. **Composability**: Easy to chain and combine operations
4. **Explicit Error Handling**: No hidden exceptions
5. **Rich Error Context**: Structured error information
6. **Functional Approach**: Better testability and maintainability

## Performance Considerations

- Uses `readonly record struct` for zero-allocation in success cases
- Efficient error array handling with array pooling
- Minimal boxing through careful generic type usage
- Thread-safe without locks through immutability

## Implicit Conversions and Error Handling

### Implicit Error to Result Conversions

```csharp
// Single Error to Result conversion
Result result1 = Error.Validation("INVALID_INPUT", "The input is invalid");
Result result2 = Error.NotFound("USER_NOT_FOUND", "User not found");
Result result3 = Error.Unauthorized("UNAUTHORIZED", "Access denied");

// Multiple Errors to Result conversion
Result result4 = Error.CreateMany([
    Error.Validation("REQUIRED_FIELD", "Name is required"),
    Error.Validation("INVALID_FORMAT", "Invalid email format")
]);

// Generic Result with Error
Result<User> userResult1 = Error.NotFound("USER_NOT_FOUND", "User not found");
Result<Order> orderResult = Error.Forbidden("ORDER_ACCESS_DENIED", "Cannot access this order");

// Multiple Errors to Generic Result
Result<Product> productResult = Error.CreateMany([
    Error.Validation("PRICE_REQUIRED", "Price is required"),
    Error.Validation("INVALID_CATEGORY", "Invalid product category")
]);
```

### Error Factory Methods

```csharp
// Validation errors
Result result1 = Error.Validation();
Result result2 = Error.Validation("INVALID_INPUT");
Result result3 = Error.Validation("INVALID_INPUT", "The input is invalid");
Result result4 = Error.Validation("INVALID_INPUT", "The input is invalid", metadata);

// Not found errors
Result result5 = Error.NotFound();
Result result6 = Error.NotFound("USER_NOT_FOUND");
Result result7 = Error.NotFound("USER_NOT_FOUND", "The user was not found");

// Conflict errors
Result result8 = Error.Conflict();
Result result9 = Error.Conflict("EMAIL_EXISTS");
Result result10 = Error.Conflict("EMAIL_EXISTS", "Email already exists");

// Unauthorized errors
Result result11 = Error.Unauthorized();
Result result12 = Error.Unauthorized("INVALID_TOKEN");
Result result13 = Error.Unauthorized("INVALID_TOKEN", "The token is invalid or expired");

// Forbidden errors
Result result14 = Error.Forbidden();
Result result15 = Error.Forbidden("ACCESS_DENIED");
Result result16 = Error.Forbidden("ACCESS_DENIED", "You don't have permission");

// Unexpected errors
Result result17 = Error.Unexpected();
Result result18 = Error.Unexpected("SYSTEM_ERROR");
Result result19 = Error.Unexpected("SYSTEM_ERROR", "An unexpected error occurred");
```

### Combining Multiple Errors

```csharp
// Using array initializer
Result result1 = new[]
{
    Error.Validation("FIELD1_REQUIRED", "Field 1 is required"),
    Error.Validation("FIELD2_INVALID", "Field 2 is invalid")
};

// Using Error.CreateMany
Result result2 = Error.CreateMany([
    Error.Validation("USERNAME_REQUIRED"),
    Error.Validation("PASSWORD_INVALID"),
    Error.Validation("EMAIL_FORMAT")
]);

// Combining different error types
Result result3 = Error.CreateMany([
    Error.Validation("INVALID_INPUT", "The input data is invalid"),
    Error.Unauthorized("TOKEN_EXPIRED", "The authentication token has expired"),
    Error.Forbidden("INSUFFICIENT_PERMISSIONS", "User lacks required permissions")
]);

// With metadata
var metadata = new ErrorMetadata()
    .Add("userId", 123)
    .Add("timestamp", DateTime.UtcNow);

Result result4 = Error.CreateMany([
    Error.Validation("FIELD_REQUIRED", "Required field is missing", metadata),
    Error.Validation("FIELD_INVALID", "Field format is invalid", metadata)
]);
```

### Real-World Example

```csharp
public Result<User> RegisterUser(RegisterUserDto dto)
{
    // Early validation using implicit conversion
    if (string.IsNullOrEmpty(dto.Email))
        return Error.Validation("EMAIL_REQUIRED", "Email is required");

    if (string.IsNullOrEmpty(dto.Password))
        return Error.Validation("PASSWORD_REQUIRED", "Password is required");

    // Multiple validation errors
    var validationErrors = new List<Error>();

    if (dto.Password.Length < 8)
        validationErrors.Add(Error.Validation("PASSWORD_TOO_SHORT", "Password must be at least 8 characters"));

    if (!dto.Email.Contains("@"))
        validationErrors.Add(Error.Validation("INVALID_EMAIL", "Invalid email format"));

    if (validationErrors.Any())
        return Error.CreateMany(validationErrors);

    // Check for existing user
    if (_userRepository.ExistsByEmail(dto.Email))
        return Error.Conflict("EMAIL_EXISTS", "A user with this email already exists");

    try
    {
        var user = new User(dto);
        _userRepository.Add(user);
        return user;
    }
    catch (Exception ex)
    {
        return Error.Unexpected("USER_CREATION_FAILED", "Failed to create user",
            ErrorMetadata.CreateWithException(ex));
    }
}
```

## Implicit Conversions for Result<T>

### Value to Result<T> Conversions

```csharp
// Direct value to Result<T> conversion
Result<int> numberResult = 42;
Result<string> stringResult = "Hello World";
Result<User> userResult = new User { Id = 1, Name = "John" };

// In method returns
public Result<decimal> CalculatePrice(Order order)
{
    if (order is null)
        return Error.Validation("ORDER_NULL", "Order cannot be null");

    decimal price = order.Items.Sum(x => x.Price);
    return price; // Implicitly converts to Result<decimal>
}

// In LINQ queries
Result<IEnumerable<User>> GetActiveUsers(IEnumerable<User> users)
{
    return users.Where(u => u.IsActive); // Implicitly converts IEnumerable<User> to Result<IEnumerable<User>>
}
```

### Complex Examples

```csharp
// Mixing value and error conversions
public Result<UserProfile> GetUserProfile(int userId)
{
    // Early validation
    if (userId <= 0)
        return Error.Validation("INVALID_ID", "User ID must be positive");

    // Direct value return
    var profile = new UserProfile { Id = userId, Name = "John" };
    return profile; // Implicitly converts to Result<UserProfile>
}

// With async operations
public async Task<Result<OrderSummary>> ProcessOrder(Order order)
{
    // Error conversion
    if (!order.IsValid)
        return Error.Validation("INVALID_ORDER", "Order validation failed");

    try
    {
        var summary = await _orderProcessor.Process(order);
        return summary; // Implicitly converts OrderSummary to Result<OrderSummary>
    }
    catch (Exception ex)
    {
        return Error.Unexpected("PROCESSING_FAILED", "Order processing failed",
            ErrorMetadata.CreateWithException(ex));
    }
}

// With collections
public Result<List<ProductDto>> GetProducts(Category category)
{
    if (!category.IsActive)
        return Error.Validation("CATEGORY_INACTIVE", "Category is not active");

    var products = _productRepository
        .GetByCategory(category)
        .Select(p => p.ToDto())
        .ToList();

    return products; // Implicitly converts List<ProductDto> to Result<List<ProductDto>>
}

// Combining multiple operations
public Result<OrderConfirmation> CreateOrder(OrderRequest request)
{
    // Validation error
    if (!request.IsValid)
        return Error.Validation("INVALID_REQUEST", "Order request is invalid");

    // Multiple errors
    var validationErrors = ValidateOrder(request);
    if (validationErrors.Any())
        return Error.CreateMany(validationErrors);

    try
    {
        // Success case - direct value conversion
        var order = new Order(request);
        var confirmation = _orderService.Confirm(order);
        return confirmation; // Implicitly converts OrderConfirmation to Result<OrderConfirmation>
    }
    catch (DomainException ex)
    {
        // Single error conversion
        return Error.Failure(ex.Code, ex.Message);
    }
    catch (Exception ex)
    {
        // Unexpected error with metadata
        return Error.Unexpected(
            "ORDER_CREATION_FAILED",
            "Failed to create order",
            ErrorMetadata.CreateWithExceptionDetailed(ex));
    }
}
```

### Value and Error Combinations

```csharp
public class UserService
{
    public Result<User> GetUser(int id)
    {
        // Error conversion
        if (id <= 0)
            return Error.Validation("INVALID_ID", "Invalid user ID");

        var user = _repository.GetById(id);

        // Value conversion
        if (user is not null)
            return user;

        // Error conversion
        return Error.NotFound("USER_NOT_FOUND", "User not found");
    }

    public Result<UserStats> GetUserStats(User user)
    {
        // Error conversion with multiple errors
        var validationErrors = new List<Error>();

        if (!user.IsActive)
            validationErrors.Add(Error.Validation("USER_INACTIVE", "User is not active"));

        if (!user.HasPermission("VIEW_STATS"))
            validationErrors.Add(Error.Forbidden("STATS_ACCESS_DENIED", "No permission to view stats"));

        if (validationErrors.Any())
            return Error.CreateMany(validationErrors);

        // Value conversion
        var stats = _statsCalculator.Calculate(user);
        return stats; // Implicitly converts UserStats to Result<UserStats>
    }
}
```
