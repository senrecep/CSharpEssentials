---
name: csharpessentials-maybe
description: Use when representing optional values explicitly. Covers Maybe<T> as a null-safe container, Maybe.From()/FromTry()/AsMaybe() for creation, HasValue/HasNoValue, Map/Bind chaining, TapNone for None-side effects, Match for consumption, and ToMaybeResult() to bridge into the Result pattern.
---

# CSharpEssentials.Maybe

`Maybe<T>` makes optionality explicit. No null reference exceptions: the absence of a value is a first-class concept.

## Installation

```bash
dotnet add package CSharpEssentials.Maybe
```

## Namespace

```csharp
using CSharpEssentials.Maybe;
```

## Creating Maybe

```csharp
Maybe<string> name    = user?.Name;               // implicit T? → Maybe<T>: null → None, value → Some
Maybe<string> none    = Maybe.None;               // implicit Maybe → Maybe<T>
Maybe<string> some    = "Alice";
var           viaFrom = Maybe.From(user?.Name);   // explicit factory where the type is not known (var, chains)
Maybe<User>   viaExt  = user.AsMaybe();           // extension on T?
```

Implicit conversion is the idiomatic style; `Maybe.From(null)` → `None`, `Maybe.From(value)` → `Some(value)`. `.AsMaybe()` does the same on any `T?` (and on `Result<T>`); there is no `.ToMaybe()`.

## Checking Value

```csharp
bool has    = maybe.HasValue;
bool empty  = maybe.HasNoValue;
string val  = maybe.GetValueOrDefault("fallback");
string val2 = maybe.GetValueOrThrow();             // throws InvalidOperationException if None
```

## Exception-safe Creation

```csharp
// Returns None if the factory throws; OperationCanceledException is rethrown, never swallowed
Maybe<int>  n = Maybe<int>.FromTry(() => int.Parse(input));
Maybe<User> u = Maybe<User>.FromTry(() => JsonSerializer.Deserialize<User>(json)!);
```

## Choosing between `T?`, `Maybe<T>` and `Result<T>`

- `T?` at boundaries: DTOs, entity columns, JSON.
- `Maybe<T>` inside the domain, when absence is an expected state composed with `Map`/`Bind`.
- `Result<T>` when the failure carries a reason the caller acts on.
- Exceptions only for bugs and invariant violations.

```csharp
Maybe<string> middleName = entity.MiddleName;   // boundary T? → domain Maybe<T>, implicit
Result<User>  found      = _cache.TryFind(id).ToMaybeResult(Error.NotFound("User.NotFound", "User does not exist"));
```

Equality: `Maybe<int>.None == 0` is `false`, `Maybe<int>.From(0) == 0` is `true`. `null` compares as absence (`Maybe<string>.None == null` is `true`), in either operand order, but `Equals(object?)` and `maybe == (object?)null` follow the BCL rule and return `false` for `null` (the static type of the operand picks the rule); prefer `HasNoValue` over `== null`/`== default`. `Maybe<int> == null` hits CS9342 and `maybe == default` means `default(T)` for value types, so use `HasNoValue`. `ToString()` returns `Some(value)` or `None` (invariant culture for `IFormattable` values). Use `HasNoValue`, not the obsolete `IsNone`.

## Pattern Match

```csharp
string result = maybe.Match(
    some: name => $"Hello, {name}",
    none: ()   => "Hello, stranger");
```

## Transforming

```csharp
// Map: transform the inner value if present
string display = Maybe.From(user?.Email)
    .Map(e => e.ToLowerInvariant())
    .GetValueOrDefault("no email");

// Bind: flatMap — when the transform itself returns Maybe<T>
Maybe<Address> address = Maybe.From(user)
    .Bind(u => Maybe.From(u.Address));

// Flatten: Maybe<Maybe<T>> → Maybe<T>, and Maybe<T>? → Maybe<T> (null becomes None)
Maybe<int> points = customer.LoyaltyPoints.Flatten(); // Maybe<int>? from an EF nullable column
```

## None-side Effects

```csharp
// TapNone — runs only when None; returns the same Maybe unchanged
maybe.TapNone(() => logger.LogWarning("Value was absent"));

// Async — instance and extension variants
await maybe.TapNoneAsync(async () => await NotifyAsync());
await GetMaybeAsync().TapNoneAsync(() => fallback());

// GetValueOrElse — lazy factory, only called when None
int v = maybe.GetValueOrElse(() => ComputeExpensiveDefault());

// OrElse — lazy Maybe chain, alias for Or(Func<Maybe<T>>)
Maybe<Config> cfg = GetCached().OrElse(() => LoadFromDisk());
await GetCached().OrElseAsync(() => Task.FromResult(LoadFromDisk()));
```

## Bridge to Result

```csharp
// Convert Maybe → Result, providing the error for the None case
Result<string> r = maybe.ToMaybeResult(
    Error.NotFound("user.email", "No email address on file."));
```

## Async Naming

Every `Maybe` member that returns `Task` or `ValueTask` ends in `Async`: `WhereAsync`, `ExecuteAsync`, `ExecuteNoValueAsync`, `OrAsync`, `MatchAsync`, `ToMaybeResultAsync`, `ToMaybeUnitResultAsync` and `Maybe.FromAsync` (`maybe.ExecuteAsync(async v => ...)`, `task.OrAsync(() => ...)`). The old unsuffixed overloads still compile as `[Obsolete]` forwarders and will be removed in 7.0. `OrElseAsync` keeps its name. An untyped async lambda on an instance `ExecuteAsync`, `ExecuteNoValueAsync`, `OrAsync` or `MatchAsync`, or on a key/value `MatchAsync`, binds to the `Task` overload through `OverloadResolutionPriority`, which needs C# 13 or later; on C# 12, type the lambda or pass a typed local such as `Func<T, Task>`.

## Best Practices

- Prefer implicit conversion (`Maybe<T> m = value;`, `return value;`, `Maybe.None`); use `Maybe.From()` or `.AsMaybe()` where the target type is not known; there is no `.ToMaybe()`
- Prefer `Match()` over `HasValue` + `GetValueOrThrow()` to avoid branches
- Use `Bind()` when the transform itself can be absent (returns `Maybe<T>`)
- Bridge to `Result` with `ToMaybeResult()` when the caller needs error information
