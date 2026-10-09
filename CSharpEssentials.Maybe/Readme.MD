# CSharpEssentials.Maybe

Maybe (Option) monad for null-safe, expressive code. Avoid `NullReferenceException` by making missing values explicit.

## Features

- **Maybe<T>**: Explicit presence/absence of a value with implicit conversions.
- **Functional API**: `Map`, `Bind`, `Where`, `Match`, `Or`, `Tap`.
- **LINQ Support**: Query syntax with `Select`, `SelectMany`, `Where`.
- **Async Support**: `MapAsync`, `BindAsync`, and awaitable extensions.
- **Result Bridge**: Convert `Maybe<T>` to `Result<T>` and back.
- **Sequence Helpers**: `TryFirst`, `TryLast`, `TryFind`, `Choose`, `Sequence`, `Traverse`, `Partition`.

Targets `net11.0`, `net10.0`, `net9.0` and `netstandard2.1`. Depends on `CSharpEssentials.Results` and `CSharpEssentials.Errors`. `Maybe<T>` and the non-generic `Maybe` (for `Maybe.None`, `Maybe.From`, `Maybe.FromAsync` and `Maybe.FromTry`) live in the `CSharpEssentials.Maybe` namespace.

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

Maybe<string> name = Maybe.From("Ann");                       // the non-generic Maybe infers T
Maybe<int> five = 5.AsMaybe();
Maybe<int> parsed = Maybe.FromTry(() => int.Parse("42"));
Maybe<string> lazy = Maybe.From(() => Environment.GetEnvironmentVariable("HOME"));
```

`From` gives `None` for `null`. `Maybe<T>` serializes with `System.Text.Json` as `{"HasValue":true,"value":5}` and deserializes back from that shape.

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

string label = FindUser(1).Match(some: u => u.Name, none: () => "stranger");

string loaded = await FindUser(1).MatchAsync(
    some: async (u, ct) => await LoadProfileAsync(u, ct),   // async handlers receive a CancellationToken
    none: async ct => await LoadAnonymousAsync(ct),
    cancellationToken: token);
```

### Reading the Value

```csharp
Maybe<int> maybe = 5;

if (maybe.HasValue) { int v = maybe.Value; }          // Value throws InvalidOperationException on None
bool found = maybe.TryGetValue(out int value);
var (hasValue, inner) = maybe;                          // Deconstruct

int a = maybe.GetValueOrDefault(0);                    // fallback value
int b = maybe.GetValueOrDefault(() => 0);              // lazy fallback
int c = maybe.GetValueOrDefault(v => v * 2, -1);       // map when Some, else the fallback
int d = maybe.GetValueOrThrow("missing");              // or GetValueOrThrow(new MyException())
int? e = maybe.AsNullable();                           // T? for reference types; default(T) for value types
List<int> list = maybe.ToList();                       // one element, or empty
```

### Filtering, Fallbacks and Side Effects

```csharp
Maybe<int> big = maybe.Where(v => v > 3);               // None when the predicate fails
Maybe<int> alt = Maybe<int>.None.Or(7);                 // Or(Maybe<T>) (a plain T converts), Or(Func<T>), Or(Func<Maybe<T>>)

maybe.Tap(v => Console.WriteLine(v));                   // only when Some; returns the same Maybe
maybe.TapIf(v => v > 3, v => Console.WriteLine(v));     // predicate-gated tap
maybe.Execute(v => Console.WriteLine(v));               // Action<T> on Some
maybe.ExecuteNoValue(() => Console.WriteLine("none"));  // Action on None

Maybe<int> doubled = maybe.MapIf(v => v > 3, v => v * 2);
Maybe<int> kept = maybe.BindIf(v => v > 3, v => Maybe<int>.From(v + 1));
```

`MapIf`, `BindIf` and `TapIf` also take a `bool` or a `Func<bool>` condition. `MapIfAsync` and `BindIfAsync` exist on `Maybe<T>`, `Task<Maybe<T>>` and `ValueTask<Maybe<T>>`; `TapIfAsync` exists on the `Task` and `ValueTask` sources.

### Converting to Result

```csharp
Maybe<Order> order = FindOrder(1);
Result<Order> result = order.ToResult(Error.NotFound("Order.Missing", "Order not found."));

// Result -> Maybe
Result<Order> result2 = new Order();
Maybe<Order> maybe = result2.AsMaybe();
```

`ToResult` and `ToUnitResult` are aliases of `ToMaybeResult` and `ToMaybeUnitResult`. Without an error argument a `None` becomes the `Maybe.Result` `NotFound` error. Always pass the error to `ToResult(error)`, or call `ToMaybeResult()`: `maybe.ToResult()` with no argument binds the generic `ToResult<TValue>(this TValue value)` of `CSharpEssentials.Results` and returns a successful `Result<Maybe<T>>`. `AsMaybe` on a failed `Result<T>` gives `None` and drops the errors. The async forms are `ToResultAsync`, `ToUnitResultAsync`, `ToMaybeResultAsync`, `ToMaybeUnitResultAsync` and `AsMaybeAsync`.

### Async

```csharp
Maybe<User> user = await FindUserAsync(1);
Maybe<Order> order = await user
    .BindAsync(u => GetOrderAsync(u.Id))
    .MapAsync(o => EnrichAsync(o));
```

Every `Maybe` member that returns `Task` or `ValueTask` ends in `Async`: `WhereAsync`, `ExecuteAsync`, `ExecuteNoValueAsync`, `OrAsync`, `MatchAsync`, `ToMaybeResultAsync`, `ToMaybeUnitResultAsync` and `Maybe.FromAsync` (`maybe.ExecuteAsync(async v => ...)`, `task.OrAsync(() => ...)`). The old unsuffixed overloads still compile as `[Obsolete]` forwarders and will be removed in 7.0. `OrElseAsync` keeps its name. An untyped async lambda on an instance `ExecuteAsync`, `ExecuteNoValueAsync`, `OrAsync`, `WhereAsync` or `MatchAsync`, or on a key/value `MatchAsync`, binds to the `Task` overload through `OverloadResolutionPriority`, which needs C# 13 or later; on C# 12, type the lambda or pass a typed local such as `Func<T, Task>`. The instance `MapAsync` has a `Task` and a `ValueTask` twin without that priority, so `maybe.MapAsync(async v => ...)` is ambiguous (CS0121) on every language version; pass a lambda that returns a `Task` instead (`maybe.MapAsync(v => LoadAsync(v))`) or type the delegate.

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
| `Choose()` | `IEnumerable<T>` | Keeps the present values; `Choose(selector)` maps them too, `ChooseAsync` streams a sequence of `Task<Maybe<T>>` |
| `TryFirst()`, `TryFirst(predicate)`, `TryLast()`, `TryLast(predicate)` | `Maybe<T>` | First or last element, `None` when there is none |
| `dictionary.TryFind(key)` | `Maybe<TValue>` | Lookup on an `IReadOnlyDictionary` |

`TryFirst` is built on `FirstOrDefault`: for a sequence of value types such as `int[]`, an empty sequence or a predicate without a match gives `Some(default)` instead of `None`. `TryLast` and the other helpers do not have this limitation.

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

`Maybe<T>.None` never equals a non-null plain `T`: `Maybe<int>.None == 0` is `false`, while `Maybe<int>.From(0) == 0` is `true`. `null` compares as absence: `Maybe<string>.None == null` is `true` and `Maybe<string>.From("a") == null` is `false`, in either operand order. A typed `null` means absence, but `Equals(object?)` and `maybe == (object?)null` follow the BCL rule that `x.Equals(null)` is `false`: `Maybe<string>.None.Equals((object?)null)` and `Maybe<string>.None == (object?)null` are both `false`. The static type of the operand picks the rule, so cast to `T?` or use `HasNoValue` to test for absence. Prefer `HasNoValue` over `== null` or `== default` for clarity. For a value type `T`, `maybe == default` means `default(T)` (so `Maybe<int>.None == default` is `false`), and `Maybe<int> == null` does not compile cleanly (CS9342, a known overload ambiguity), so use `HasNoValue`. `ToString()` returns `Some(value)` or `None`; `IFormattable` values are formatted with the invariant culture.

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
