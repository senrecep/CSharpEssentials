# CSharpEssentials.Maybe

Maybe (Option) monad for null-safe, expressive code. Avoid `NullReferenceException` by making missing values explicit.

## Features

- **Maybe<T>**: Explicit presence/absence of a value with implicit conversions.
- **Functional API**: `Map`, `Bind`, `Where`, `Match`, `Or`, `Tap`.
- **LINQ Support**: Query syntax with `Select`, `SelectMany`, `Where`.
- **Async Support**: `MapAsync`, `BindAsync`, and awaitable extensions.
- **Result Bridge**: Convert `Maybe<T>` to `Result<T>` and back.

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

Maybe<string> none = Maybe.None;
Maybe<int> value = 42;
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
Result<Order> result2 = new Order();
Maybe<Order> maybe = result2.AsMaybe();
```

### Async

```csharp
Maybe<User> user = await FindUserAsync(1);
Maybe<Order> order = await user
    .BindAsync(u => GetOrderAsync(u.Id))
    .MapAsync(o => EnrichAsync(o));
```

Every `Maybe` member that returns `Task` or `ValueTask` ends in `Async`: `WhereAsync`, `ExecuteAsync`, `ExecuteNoValueAsync`, `OrAsync`, `MatchAsync`, `ToMaybeResultAsync`, `ToMaybeUnitResultAsync` and `Maybe.FromAsync` (`maybe.ExecuteAsync(async v => ...)`, `task.OrAsync(() => ...)`). The old unsuffixed overloads still compile as `[Obsolete]` forwarders and will be removed in 7.0. `OrElseAsync` keeps its name. An untyped async lambda on an instance `ExecuteAsync`, `ExecuteNoValueAsync`, `OrAsync` or `MatchAsync`, or on a key/value `MatchAsync`, binds to the `Task` overload through `OverloadResolutionPriority`, which needs C# 13 or later; on C# 12, type the lambda or pass a typed local such as `Func<T, Task>`.

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
| `Partition()` | `(T[] Values, int NoneCount)` | Never returns `None`; always splits |

### Choosing between `T?`, `Maybe<T>` and `Result<T>`

| Use | When | Example |
|-----|------|---------|
| `T?` | At boundaries: DTOs, entity columns, JSON | `public string? MiddleName { get; set; }` |
| `Maybe<T>` | Inside the domain, when absence is an expected state composed with `Map`/`Bind` | `Maybe<User> user = _cache.TryFind(id);` |
| `Result<T>` | When the failure carries a reason the caller acts on | `Result<User> user = FindUser(id);` |
| Exceptions | Only for bugs and invariant violations | `ArgumentNullException`, broken invariants |

```csharp
// Boundary: a nullable column converts implicitly into the domain
Maybe<string> middleName = entity.MiddleName;

// Domain: absence is expected, compose it
string display = middleName.Map(n => n.ToUpperInvariant()).GetValueOrDefault("-");

// The caller needs to know why it is missing: upgrade to Result
Result<User> found = _cache.TryFind(id).ToMaybeResult(Error.NotFound("User.NotFound", "User does not exist"));
```

`Maybe<T>.None` never equals a non-null plain `T`: `Maybe<int>.None == 0` is `false`, while `Maybe<int>.From(0) == 0` is `true`. `null` compares as absence: `Maybe<string>.None == null` is `true` and `Maybe<string>.From("a") == null` is `false`, in either operand order. Prefer `HasNoValue` over `== null` or `== default` for clarity. For a value type `T`, `maybe == default` means `default(T)` (so `Maybe<int>.None == default` is `false`), and `Maybe<int> == null` does not compile cleanly (CS9342, a known overload ambiguity), so use `HasNoValue`. `ToString()` returns `Some(value)` or `None`; `IFormattable` values are formatted with the invariant culture.

### Exception-safe Creation

```csharp
using System.Text.Json;

// Returns None if the factory throws; OperationCanceledException is rethrown
Maybe<int> result = Maybe<int>.FromTry(() => int.Parse(userInput));
Maybe<User> user  = Maybe<User>.FromTry(() => JsonSerializer.Deserialize<User>(json));
```

### TapNone: Side Effect on Absence

```csharp
Maybe<int> maybe = Maybe.None;

maybe.TapNone(() => logger.LogWarning("Value missing"));  // called when None

// Async
await maybe.TapNoneAsync(async () => await NotifyAsync("Missing"));

// Via Task<Maybe<T>> / ValueTask<Maybe<T>>
await GetMaybeAsync().TapNoneAsync(() => logger.LogWarning("Value missing"));
```

### GetValueOrElse: Lazy Fallback

```csharp
// Factory only called when None (unlike GetValueOrDefault which always evaluates)
int value = maybe.GetValueOrElse(() => ComputeExpensiveDefault());
```

### Flatten

```csharp
Maybe<int> inner = Maybe<Maybe<int>>.From(Maybe<int>.From(5)).Flatten(); // Some(5)

Maybe<int>? stored = null;              // e.g. a Maybe<int>? property mapped to a nullable column
Maybe<int> points = stored.Flatten();   // None
```

### OrElse: Lazy Maybe Chain

```csharp
// Alias for Or(Func<Maybe<T>>) — returns self if Some, calls factory if None
Maybe<Config> config = GetFromCache().OrElse(() => GetFromDatabase());

// Async
Maybe<Config> loaded = await GetFromCache().OrElseAsync(() => Task.FromResult(GetFromDatabase()));
```
