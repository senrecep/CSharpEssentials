# CSharpEssentials.Maybe

Maybe (Option) monad for null-safe, expressive code. Avoid `NullReferenceException` by making missing values explicit.

## Features

- **Maybe<T>** — Explicit presence/absence of a value with implicit conversions.
- **Functional API** — `Map`, `Bind`, `Where`, `Match`, `Or`, `Tap`.
- **LINQ Support** — Query syntax with `Select`, `SelectMany`, `Where`.
- **Async Support** — `MapAsync`, `BindAsync`, and awaitable extensions.
- **Result Bridge** — Convert `Maybe<T>` to `Result<T>` and back.

## Installation

```bash
dotnet add package CSharpEssentials.Maybe
```

## Usage

### Creating Maybe

```csharp
using CSharpEssentials.Maybe;

public Maybe<User> FindUser(int id)
{
    User? user = _repo.Find(id);
    return user; // implicit from T? to Maybe<T>
}

Maybe<string> none = Maybe<string>.None;
Maybe<int> value = Maybe<int>.From(42);
```

### Functional Chaining

```csharp
string city = FindUser(1)
    .Where(u => u.IsActive)
    .Map(u => u.Address)
    .Map(a => a.City)
    .GetValueOrDefault("Unknown");
```

### LINQ Queries

```csharp
Maybe<string> tracking =
    from user in FindUser(1)
    from order in FindOrder(user.Id)
    where order.Amount > 100
    select order.TrackingNumber;
```

### Matching

```csharp
FindUser(1).Match(
    some: user => Console.WriteLine($"Found: {user.Name}"),
    none: () => Console.WriteLine("Not found"));
```

### Converting to Result

```csharp
Maybe<Order> order = FindOrder(1);
Result<Order> result = order.ToResult(Error.NotFound("Order.Missing", "Order not found."));

// Result -> Maybe
Result<Order> result2 = Result.Success(new Order());
Maybe<Order> maybe = result2.AsMaybe();
```

### Async

```csharp
Maybe<Order> order = await FindUserAsync(1)
    .BindAsync(user => GetOrderAsync(user.Id))
    .MapAsync(order => EnrichAsync(order));
```
