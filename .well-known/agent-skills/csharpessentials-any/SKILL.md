---
name: csharpessentials-any
description: Use when a method returns one of several distinct types (typed outcomes, polymorphic results without a base class). Any<T0,T1> to Any<T0,...,T7> is a discriminated union. Covers implicit creation, First/Second factories, Any.Create, Match (AnyActionResult) and Switch (AnyActionStatus), IsFirst/GetFirst, Is/As/TryAs, Deconstruct/ToTuple, Partition/Traverse and JSON.
---

# CSharpEssentials.Any

`Any<T0, T1, ...>` holds exactly one value from 2 to 8 fixed types. It is a `readonly struct`.

```bash
dotnet add package CSharpEssentials.Any
```

```csharp
using CSharpEssentials.Any;
```

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials.Any
using CSharpEssentials.Any;

Any<User, NotFound> found = FindUser(1);
Any<User, NotFound> missing = FindUser(2);

foreach (Any<User, NotFound> result in new[] { found, missing })
{
    string text = result.Match(
        first: user => $"200: {user.Name}",
        second: error => $"404: {error.Message}").Result!;
    Console.WriteLine(text);
}

if (found.IsFirst)
    Console.WriteLine($"GetFirst: {found.GetFirst().Name}");

bool isMissing = missing.TryAs<NotFound, User, NotFound>(out NotFound? notFound);
Console.WriteLine($"TryAs<NotFound>: {isMissing}, {notFound?.Message}");

(User[] users, NotFound[] errors) = new[] { 1, 2, 1 }.Traverse(FindUser);
Console.WriteLine($"{users.Length} users, {errors.Length} not found");

static Any<User, NotFound> FindUser(int id) =>
    id == 1 ? new User(1, "Ada") : new NotFound($"User {id} not found");

internal sealed record User(int Id, string Name);
internal sealed record NotFound(string Message);
```

## Creating a union

```csharp
Any<User, NotFound> a = new User(1, "Ada");                       // implicit from any branch type
Any<User, NotFound> b = Any<User, NotFound>.Second(new NotFound("x")); // First, Second, ... Eighth
Any<User, NotFound> c = Any.Create<User, NotFound>(new User(2, "Bo"));  // type arguments required
```

`Index` is the active branch (0 for `T0`). `Value` is `object?`.

## Handling every branch

```csharp
AnyActionResult<string> result = a.Match(
    first: u => u.Name,
    second: e => e.Message);
string text = result.Result!;          // Status is AnyActionStatus.Executed

AnyActionStatus status = a.Switch(
    first: u => Console.WriteLine(u.Name),
    second: e => Console.WriteLine(e.Message));
```

Handler names are `first`, `second`, `third`, `fourth`, `fifth`, `sixth`, `seventh`, `eighth`.

## Checking one branch

```csharp
if (a.IsFirst) { User u = a.GetFirst(); }    // GetX throws InvalidOperationException for another branch

bool isUser = a.Is<User, User, NotFound>();                       // <TTarget, T0, T1>
User? user = a.As<User, User, NotFound>();
bool ok = a.TryAs<NotFound, User, NotFound>(out NotFound? error);
var (maybeUser, maybeError) = a;                                  // inactive slot is default
(User? First, NotFound? Second) tuple = a.ToTuple();
```

## Sequences

```csharp
IEnumerable<Any<User, NotFound>> lookups = [a, b];
(User[] users, NotFound[] notFound) = lookups.Partition();
(User[] loaded, NotFound[] failed) = new[] { 1, 2 }.Traverse(id => id == 1 ? (Any<User, NotFound>)new User(id, "Ada") : new NotFound("x"));
```

## JSON

```csharp
string json = System.Text.Json.JsonSerializer.Serialize(a);   // {"Index":0,"Value":{"Id":1,"Name":"Ada"}}
```

Deserializing uses the caller's `JsonSerializerOptions` for the value. With a source-generated context, add `[JsonSerializable]` for every branch type.

## Rules

- `Match` returns `AnyActionResult<TResult>`, not `TResult`. Read `.Result` (and `.Status` if a handler may be missing).
- All handlers are optional parameters. A missing handler for the active branch returns `NotExecuted` and `Result = default`. It is not a compile error, so pass one per branch.
- `Match` and `Switch` throw `InvalidOperationException` on `default(Any<...>)` or when the stored value is `null`. `Partition` throws for the same reason.
- `Is`, `As` and `TryAs` need all type arguments: `value.Is<int, string, int>()`. `value.Is<int>()` does not compile.
- `As<int, ...>()` returns `0` on a mismatch, not `null`. Use `TryAs` to check.
- `Any<int, int>` (duplicate types) breaks implicit conversion (CS0457). Use `First(...)`/`Second(...)`.
- Prefer `Result<T>` from `CSharpEssentials.Results` when the alternative is just an `Error`. Use `Any` when each branch carries its own typed data.
- `ToString()` returns the value's `ToString()`, not JSON.

## Diagnostics

This package reports no analyzer diagnostics.
