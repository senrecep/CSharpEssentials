---
name: csharpessentials-maybe
description: Use when a value may be missing and callers must handle it (lookups, optional fields, first-or-none). Maybe<T> is an option type. Covers implicit creation, Maybe.None/From/FromTry, AsMaybe, HasValue, GetValueOrDefault, Map/Bind/Where/Or, LINQ syntax, Match, Tap/TapNone, ToResult(error)/ToMaybeResult, TryFirst/TryFind/Choose/Sequence/Traverse/Partition and the *Async chain.
---

# CSharpEssentials.Maybe

`Maybe<T>` is a `readonly struct` that holds a value or is `None`.

```bash
dotnet add package CSharpEssentials.Maybe
```

```csharp
using CSharpEssentials.Maybe;          // Maybe<T>, Maybe, all extension methods
using CSharpEssentials.ResultPattern;  // Result<T>, for ToResult / AsMaybe
using CSharpEssentials.Errors;         // Error, for ToResult(error)
```

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials.Maybe
using CSharpEssentials.Errors;
using CSharpEssentials.Maybe;
using CSharpEssentials.ResultPattern;

Dictionary<int, User> users = new() { [1] = new User(1, "Ada", "ada@example.com"), [2] = new User(2, "Bo", null) };

Maybe<User> FindUser(int id) => users.TryGetValue(id, out User? user) ? user : Maybe.None;

foreach (int id in new[] { 1, 2, 3 })
{
    string email = FindUser(id)
        .Bind(u => Maybe.From(u.Email))   // Email is string?, null becomes None
        .Map(e => e.ToUpperInvariant())
        .GetValueOrDefault("no email");
    Console.WriteLine($"{id}: {email}");
}

string label = FindUser(3).Match(some: u => u.Name, none: () => "stranger");
Console.WriteLine(label);

Result<User> result = FindUser(3).ToResult(Error.NotFound("User.NotFound", "User not found."));
Console.WriteLine(result.FirstError.Code);

Maybe<User> loaded = await Maybe.FromAsync(LoadAsync(1));
Maybe<string> name = await loaded.MapAsync(u => Task.FromResult(u.Name));
Console.WriteLine(name);

static async Task<User?> LoadAsync(int id)
{
    await Task.Yield();
    return id == 1 ? new User(1, "Ada", null) : null;
}

internal sealed record User(int Id, string Name, string? Email);
```

## Creating

```csharp
Maybe<string> some = "Alice";                          // implicit from T
Maybe<string> fromNullable = (string?)null;            // null -> None
Maybe<string> none = Maybe.None;                       // non-generic None converts to Maybe<T>
var inferred = Maybe.From("Alice");                    // Maybe<string>
Maybe<int> ext = 5.AsMaybe();                          // extension on T?
Maybe<int> safe = Maybe.FromTry(() => int.Parse("x")); // None on exception (OperationCanceledException rethrown)
Maybe<string> lazy = Maybe.From(() => Environment.GetEnvironmentVariable("HOME"));
```

There is no `ToMaybe()`, `Some()` or `Maybe.Some(...)`. Use the implicit conversion, `Maybe.From` or `AsMaybe()`.

## Reading

```csharp
Maybe<int> maybe = 5;

if (maybe.HasValue) { int v = maybe.Value; }     // Value throws InvalidOperationException on None
bool missing = maybe.HasNoValue;
int a = maybe.GetValueOrDefault(0);
int b = maybe.GetValueOrDefault(() => 0);        // lazy; same as GetValueOrElse(() => 0)
int c = maybe.GetValueOrDefault(v => v * 2, 0);  // map or fallback
int d = maybe.GetValueOrThrow("missing");        // InvalidOperationException; or pass an Exception
bool ok = maybe.TryGetValue(out int e);
var (hasValue, value) = maybe;
int? f = maybe.AsNullable();
List<int> list = maybe.ToList();
```

## Chaining

```csharp
Maybe<User> user = new User(1, "Ada", "ada@example.com");

Maybe<string> name = user.Map(u => u.Name);                 // Func<T, TOut>
Maybe<string> email = user.Bind(u => Maybe.From(u.Email));  // Func<T, Maybe<TOut>>
Maybe<User> active = user.Where(u => u.Id > 0);
Maybe<User> fallback = Maybe<User>.None.Or(new User(0, "Guest", null)); // also Or(Func<T>), Or(Func<Maybe<T>>), OrElse
Maybe<User> changed = user.MapIf(u => u.Id == 1, u => u with { Name = "Admin" }); // MapIf, BindIf, TapIf take bool, Func<bool> or Func<T, bool>

Maybe<string> line =
    from u in user
    from mail in Maybe.From(u.Email)
    select $"{u.Name} <{mail}>";
```

## Handling both cases

```csharp
string text = user.Match(some: u => u.Name, none: () => "none");
user.Match(some: u => Console.WriteLine(u.Name), none: () => Console.WriteLine("none"));

user.Tap(u => Console.WriteLine(u.Name));      // Some only, returns the Maybe
user.TapNone(() => Console.WriteLine("none")); // None only, returns the Maybe
user.Execute(u => Console.WriteLine(u.Name));  // Some only, returns void
user.ExecuteNoValue(() => Console.WriteLine("none"));
```

## Result bridge

```csharp
Result<User> r1 = user.ToResult(Error.NotFound("User.NotFound", "User not found."));
Result<User> r2 = user.ToMaybeResult();       // default error: NotFound, code "Maybe.Result"
Result r3 = user.ToUnitResult();              // no value
Maybe<int> fromResult = Result<int>.Success(5).AsMaybe(); // failure -> None, errors dropped
```

`AsMaybe()` also works on `Result<T, E1>`, `Result<T, E1, E2>` and `Result<T, E1, E2, E3>`. `maybe.ToResult(typedError)` returns `Result<T, TError>`.

## Collections

```csharp
int[] numbers = [3, 8, 11];
Maybe<int> first = numbers.TryFirst(n => n % 2 == 0);   // also TryFirst(), TryLast(), TryLast(predicate)
IReadOnlyDictionary<string, int> stock = new Dictionary<string, int> { ["apple"] = 3 };
Maybe<int> apples = stock.TryFind("apple");

Maybe<int>[] maybes = [1, Maybe.None, 3];
IEnumerable<int> present = maybes.Choose();             // 1, 3; Choose(selector) maps too
var (values, noneCount) = maybes.Partition();           // ([1, 3], 1)
Maybe<int[]> all = maybes.Sequence();                   // None if any item is None
Maybe<int[]> parsed = new[] { "1", "2" }.Traverse(s => Maybe.FromTry(() => int.Parse(s)));
Maybe<int> flat = Maybe<Maybe<int>>.From(Maybe<int>.From(5)).Flatten();
```

## Async

```csharp
Maybe<User> u1 = await Maybe.FromAsync(LoadAsync(1));          // Task<T?> -> Maybe<T>
Maybe<string> n1 = await u1.MapAsync(u => Task.FromResult(u.Name));
Maybe<User> u2 = await Maybe.FromAsync(LoadAsync(9)).OrAsync(() => new User(0, "Guest", null));
string t1 = await u1.MatchAsync(
    some: (u, ct) => Task.FromResult(u.Name),   // async handlers receive a CancellationToken
    none: ct => Task.FromResult("none"));
Result<User> r4 = await Maybe.FromAsync(LoadAsync(9)).ToResultAsync(Error.NotFound("User.NotFound", "Missing."));
```

Async names: `FromAsync`, `MapAsync`, `BindAsync`, `WhereAsync`, `OrAsync`, `OrElseAsync`, `MatchAsync`, `TapAsync`, `TapIfAsync`, `TapNoneAsync`, `MapIfAsync`, `BindIfAsync`, `ExecuteAsync`, `ExecuteNoValueAsync`, `GetValueOrDefaultAsync`, `GetValueOrThrowAsync`, `ToResultAsync`, `ToUnitResultAsync`, `ToMaybeResultAsync`, `ToMaybeUnitResultAsync`, `AsMaybeAsync`. The extension forms continue a `Task<Maybe<T>>` or `ValueTask<Maybe<T>>`.

## Rules

- Never call `maybe.ToResult()` with no argument. It binds the generic `ToResult<TValue>(this TValue)` from Results and returns a successful `Result<Maybe<T>>`. Pass an `Error`, or call `ToMaybeResult()`.
- Every async method ends in `Async`. The unsuffixed `Execute`, `ExecuteNoValue`, `Or`, `Where` and `Match` overloads that return a `Task`/`ValueTask` (async arguments or a `Task<Maybe<T>>` receiver), `Maybe.From(Task)` and `ToMaybeResult`/`ToMaybeUnitResult` on a `Task<Maybe<T>>` are `[Obsolete]`. `IsNone` is obsolete too: use `HasNoValue`.
- `maybe.MapAsync(async v => ...)` fails with CS0121 (Task and ValueTask overloads). Write `maybe.MapAsync(v => LoadAsync(v))` or type the delegate.
- Async `MatchAsync` handlers take a token: `some: (v, ct) => ...`, `none: ct => ...`.
- `Maybe.From(null)` does not compile (no type to infer). Use `Maybe<string>.None` or `(string?)null`.
- `Maybe<int>.None == 0` is `false`. `Maybe<int> == null` does not compile cleanly (CS9342). Test absence with `HasNoValue`.
- `Bind` takes a function that returns `Maybe<TOut>`. Use `Map` for a plain value.
- `ChooseAsync` yields in completion order on .NET 9 and later.
- Use `T?` in DTOs and entities, `Maybe<T>` inside the domain, and `Result<T>` when the caller needs a reason.

## Diagnostics

This package reports no analyzer diagnostics.
