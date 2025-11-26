# CSharpEssentials.Results

`CSharpEssentials.Results` implements the **Result Pattern** for functional error handling in .NET. It allows you to represent the success or failure of an operation explicitly, avoiding exceptions for control flow and making your code more robust and readable.

## 🚀 Features

- **Explicit Outcomes**: `Result` (for void operations) and `Result<T>` (for value-returning operations).
- **Immutable**: Thread-safe, readonly record structs.
- **Multiple Errors**: Support for aggregating multiple errors (e.g., for validation).
- **Functional Extensions**: Rich set of methods like `Map`, `Bind`, `Match`, `Tap`, `Ensure`, and `Then` for chaining operations.
- **Async Support**: Full support for async/await in extension methods.
- **JSON Support**: Built-in attributes for proper serialization.

## 📦 Installation

```bash
dotnet add package CSharpEssentials.Results
```

## 🛠 Usage

### 1. Creating Results

Return `Result.Success()` or `Result.Failure(...)`.

```csharp
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Errors;

public Result DoSomething(bool fail)
{
    if (fail)
    {
        return Result.Failure(Error.Failure("Operation.Failed", "Something went wrong."));
    }
    
    return Result.Success();
}

public Result<int> GetValue(int id)
{
    if (id < 0)
    {
        // Implicit conversion from Error to Result<T>
        return Error.Validation("Id.Invalid", "ID cannot be negative"); 
    }

    // Implicit conversion from Value to Result<T>
    return 42;
}
```

### 2. Functional Chaining (Railway Oriented Programming)

Chain operations so that subsequent steps only run if the previous ones succeeded.

```csharp
public Result<Order> ProcessOrder(Request request)
{
    return ValidateRequest(request)
        .Bind(CreateOrder)
        .Bind(ProcessPayment)
        .Tap(SendEmail) // Side effect (fire and forget, doesn't change result)
        .Map(order => order);
}
```

### 3. Handling the Result (Match)

At the end of the chain, unwrap the result to handle success and failure cases explicitly.

```csharp
var result = ProcessOrder(request);

string response = result.Match(
    onSuccess: order => $"Order {order.Id} processed!",
    onFailure: errors => $"Failed: {errors[0].Description}"
);
```

### 4. Combining Results

Combine multiple results; if any fail, the whole operation is a failure.

```csharp
var r1 = Result.Success();
var r2 = Result.Success();
var r3 = Result.Failure(Error.Unexpected());

var combined = Result.And(r1, r2, r3); // Returns Failure
```

### 5. Safe Execution (TryCatch)

Wrap risky code that might throw exceptions into a Result.

```csharp
var result = Result.TryCatch(() => 
{
    // Code that might throw
    return File.ReadAllText("config.json");
});

// result is Result<string>, capturing any exception as an Error
```
