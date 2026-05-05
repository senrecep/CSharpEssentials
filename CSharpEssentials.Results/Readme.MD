# CSharpEssentials.Results

Functional Result pattern for explicit success/failure handling in .NET. No exceptions for control flow.

## Features

- **Result & Result<T>** — Value-type results with implicit conversions from values and errors.
- **Railway Oriented Programming** — Chain operations with `Then`, `Bind`, `Map`, `Ensure`.
- **Error Aggregation** — Collect multiple validation errors in one result.
- **Async Support** — Full async/await across all extension methods.
- **Safe Execution** — `TryCatch` to wrap exceptions into results.

## Installation

```bash
dotnet add package CSharpEssentials.Results
```

## Usage

### Creating Results

```csharp
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Errors;

public Result<int> GetUser(int id)
{
    if (id <= 0)
        return Error.Validation("Id.Invalid", "ID must be positive.");

    return 42; // implicit conversion to Result<int>
}

public Result DeleteUser(int id)
{
    if (id <= 0)
        return Error.Validation("Id.Invalid", "ID must be positive.");

    return Result.Success();
}
```

### Chaining

```csharp
Result<int> Parse(string input)
{
    if (int.TryParse(input, out int n))
        return n;
    return Error.Validation("Parse", "Not a number.");
}

Result<int> result = Parse("5")
    .Then(n => n * 2)
    .Then(n => n + 10);
```

### Ensuring

```csharp
Result<int> ensured = Result.Success(50)
    .Ensure(v => v > 0, Error.Validation("Range", "Must be positive."))
    .Ensure(v => v < 100, Error.Validation("Range", "Must be less than 100."));
```

### Matching

```csharp
string message = GetUser(10).Match(
    onSuccess: user => $"User: {user}",
    onError: errors => $"Failed: {errors[0].Description}");
```

### Combining Results

```csharp
var r1 = Result.Success();
var r2 = Result.Success();
var r3 = Result.Failure(Error.NotFound());

Result combined = Result.And(r1, r2, r3); // Failure
```

### Safe Execution

```csharp
Result<int> result = Result.Try(() => 10 / 0, ex => Error.Exception(ex));
```

### Async

```csharp
Result<int> result = await FetchUserAsync(id)
    .ThenAsync(user => EnrichAsync(user))
    .EnsureAsync(v => IsValidAsync(v), Error.Validation("Invalid", "Not valid."));
```
