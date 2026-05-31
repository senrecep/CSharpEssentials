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

### Collection Extensions

```csharp
// Require ALL lookups to succeed — None if any is missing
Maybe<User[]> allUsers = userIds.Traverse(id => FindUser(id));

// Collect present values and count absences
var (values, missingCount) = maybes.Partition();

// Sequence: None if any element is None
Maybe<Config[]> configs = configMaybes.Sequence();
```

| Method | Returns | Behavior |
|--------|---------|----------|
| `Sequence()` | `Maybe<T[]>` | `None` if any element is `None` |
| `Traverse(selector)` | `Maybe<TOut[]>` | Applies selector then sequences |
| `Partition()` | `(T[] Values, int NoneCount)` | Never returns `None` — always splits |

### Exception-safe Creation

```csharp
// Returns None if the factory throws
Maybe<int> result = Maybe<int>.FromTry(() => int.Parse(userInput));
Maybe<User> user  = Maybe<User>.FromTry(() => JsonSerializer.Deserialize<User>(json));
```

### TapNone — Side Effect on Absence

```csharp
Maybe<int> maybe = Maybe<int>.None;

maybe.TapNone(() => logger.LogWarning("Value missing"));  // called when None

// Async
await maybe.TapNoneAsync(async () => await NotifyAsync("Missing"));

// Via Task<Maybe<T>> / ValueTask<Maybe<T>>
await GetMaybeAsync().TapNoneAsync(() => fallback());
```

### GetValueOrElse — Lazy Fallback

```csharp
// Factory only called when None (unlike GetValueOrDefault which always evaluates)
int value = maybe.GetValueOrElse(() => ComputeExpensiveDefault());
```

### OrElse — Lazy Maybe Chain

```csharp
// Alias for Or(Func<Maybe<T>>) — returns self if Some, calls factory if None
Maybe<Config> config = GetFromCache().OrElse(() => GetFromDatabase());

// Async
Maybe<Config> config = await GetFromCache().OrElseAsync(() => Task.FromResult(GetFromDatabase()));
```
