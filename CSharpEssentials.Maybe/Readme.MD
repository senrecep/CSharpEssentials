# CSharpEssentials.Maybe

`CSharpEssentials.Maybe` implements the Maybe (Option) monad for .NET. It provides a type-safe way to handle the presence or absence of a value without relying on null references, making your code more expressive and less prone to `NullReferenceException`.

## 🚀 Features

- **Explicit Null Safety**: `Maybe<T>` represents a value that might be missing.
- **Functional API**: Chain operations with `Map`, `Bind`, `Where`, and `Match`.
- **LINQ Support**: Use `Select`, `SelectMany`, and `Where` in LINQ query syntax.
- **Async Support**: First-class support for async workflows (`MapAsync`, `BindAsync`).
- **JSON Integration**: Serialization support included.

## 📦 Installation

```bash
dotnet add package CSharpEssentials.Maybe
```

## 🛠 Usage

### 1. Creating Maybe

Wrap values to indicate they might be absent.

```csharp
using CSharpEssentials.Maybe;

public Maybe<User> GetUser(int id)
{
    User? user = _repo.Find(id);
    return user; // Implicit conversion from T? to Maybe<T>
}

// Explicit creation
var maybeValue = Maybe.From(someValue);
var none = Maybe.None;
```

### 2. Functional Chaining

Transform values safely. If the value is missing, the operations are skipped.

```csharp
string result = GetUser(1)
    .Where(u => u.IsActive)          // Filter
    .Map(u => u.Address)             // Transform
    .Map(a => a.City)
    .GetValueOrDefault("Unknown");   // Unwrap
```

### 3. LINQ Query Syntax

Use LINQ to compose multiple Maybe values.

```csharp
var result = 
    from user in GetUser(1)
    from order in GetLatestOrder(user.Id)
    where order.Amount > 100
    select order.TrackingNumber;

// result is Maybe<string>
```

### 4. Matching (Unwrapping)

Handle both cases (Value vs None) explicitly.

```csharp
GetUser(1).Match(
    some: user => Console.WriteLine($"Found: {user.Name}"),
    none: () => Console.WriteLine("User not found")
);
```

### 5. Async Workflow

Seamlessly work with async tasks.

```csharp
public async Task<Maybe<Order>> ProcessAsync(int userId)
{
    return await GetUserAsync(userId)
        .BindAsync(user => GetOrderAsync(user.Id))
        .MapAsync(order => EnrichOrderAsync(order));
}
```
