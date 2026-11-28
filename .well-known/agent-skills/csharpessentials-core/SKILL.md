---
name: csharpessentials-core
description: Use for small C# helpers from CSharpEssentials.Core. String case conversion (ToSnakeCase, ToPascalCase, ToKebabCase), GUIDs (Guider.NewGuid version 7, NewMonotonicGuid, NewSqlServerGuid, NewGuidV5, ToStringFromGuid/ToGuidFromString), collection helpers (IfAdd, WhereIf, WithoutNulls, HasSameElements, GetRandomItems), null and bool helpers (IsEmpty, IfNotNull, IfTrue), IsBetween, WithCancellation, HttpCodes and the ITransactionRunner/ITransactionCallbacks interfaces.
---

# CSharpEssentials.Core

```bash
dotnet add package CSharpEssentials.Core
```

```csharp
using CSharpEssentials.Core;          // all extension methods, Guider, GuidNamespaces, HttpCodes
using CSharpEssentials.Transactions;  // ITransactionRunner, ITransactionCallbacks
using CSharpEssentials;               // .NET 10+: IEnumerable<T>.IsEmpty and string.IsPalindrome properties
```

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials.Core
using CSharpEssentials.Core;

Console.WriteLine("OrderLineItem".ToSnakeCase());    // order_line_item
Console.WriteLine("order-line-item".ToCamelCase());  // orderLineItem

Guid id = Guider.NewGuid();                          // version 7
string shortId = id.ToStringFromGuid();              // 22 chars, URL-safe
Guid back = shortId.ToGuidFromString();
Console.WriteLine(back == id);                       // True

List<string> roles = ["user"];
roles.IfAdd(condition: true, "admin");
Console.WriteLine(string.Join(",", roles));          // user,admin

int[] scores = [40, 75, 90];
bool onlyPassing = true;
Console.WriteLine(string.Join(",", scores.WhereIf(onlyPassing, s => s >= 50))); // 75,90

string? name = null;
name.IfNotNull(n => Console.WriteLine(n), () => Console.WriteLine("no name"));   // no name
Console.WriteLine(75.IsBetween(50, 100));            // True
Console.WriteLine(HttpCodes.NotFound);               // 404
```

## Converting case

```csharp
string snake  = "HelloWorld".ToSnakeCase();      // hello_world
string kebab  = "HelloWorld".ToKebabCase();      // hello-world
string pascal = "hello_world".ToPascalCase();    // HelloWorld
string camel  = "hello world".ToCamelCase();     // helloWorld
string macro  = "helloWorld".ToMacroCase();      // HELLO_WORLD
string train  = "helloWorld".ToTrainCase();      // Hello-World
string title  = "helloWorld".ToTitleCase();      // Hello World
string field  = "helloWorld".ToUnderscoreCamelCase(); // _helloWorld
```

Each takes an optional `CultureInfo`. A run of capitals is one word: `"HTTPStatus".ToSnakeCase()` is `httpstatus`. For enum wire names use `[StringEnum]` and `ToWireName()` from CSharpEssentials.Enums (`http_status`).

## Creating GUIDs

```csharp Program.cs
// dotnet add package CSharpEssentials.Core
using CSharpEssentials.Core;

Guid key = Guider.NewGuid();                       // version 7 primary key
Guid ordered = Guider.NewMonotonicGuid();          // strictly increasing in this process
Guid sqlServerKey = Guider.NewSqlServerGuid();     // version 8, for SQL Server uniqueidentifier
Guid fromKey = Guider.NewGuidV5(GuidNamespaces.Url, "https://example.com/a"); // deterministic
Guid atTime = Guider.NewGuid(DateTimeOffset.UtcNow);
Guid fromClock = Guider.NewGuid(TimeProvider.System); // only in the .NET 9+ builds

Console.WriteLine($"{key.Version} {sqlServerKey.Version} {fromKey.Version}"); // 7 8 5

try
{
    Guider.ToGuidFromString("bad");
}
catch (FormatException)
{
    Console.WriteLine("rejected");
}
```

## Working with collections

```csharp
list.IfAdd(condition, item);                    // ICollection<T>, returns the collection
list.IfAddRange(condition, ["a", "b"]);
query.WhereIf(filter is not null, x => x.Name == filter);   // IQueryable<T> stays translatable
IEnumerable<string> names = rawNames.WithoutNulls();         // IEnumerable<string?> -> IEnumerable<string>
bool same = a.HasSameElements(b);               // set comparison
bool all = flags.AllTrue();                     // also AllFalse(); both true when empty
T one = array.GetRandomItem();                  // T[], List<T>, Span<T>
T[] some = array.GetRandomItems(3);             // distinct positions
```

## Waiting with a token

```csharp
string data = await LoadAsync().WithCancellation(cancellationToken);
```

`WithCancellation` stops waiting and throws `OperationCanceledException` (on .NET 9+ the subclass `TaskCanceledException`). It does not cancel `LoadAsync`; pass the token to the operation when it accepts one.

## Implementing ITransactionRunner

```csharp
public interface ITransactionRunner
{
    ValueTask<T> ExecuteAsync<T>(Func<CancellationToken, ValueTask<T>> work, Func<T, bool> shouldCommit, CancellationToken cancellationToken = default);
}

public interface ITransactionCallbacks
{
    ValueTask OnCommittedAsync(Func<CancellationToken, ValueTask> callback, CancellationToken cancellationToken = default);
}
```

Do not write your own for EF Core: `EfCoreTransactionRunner<TDbContext>` in CSharpEssentials.EntityFrameworkCore implements both. `OnCommittedAsync` queues work until the outermost commit, drops it on rollback, and runs it at once when no transaction is open.

## Rules

- The class is `Guider`, not `GuidHelper`. The string methods are `ToStringFromGuid` and `ToGuidFromString`; there is no `TryParse`, so catch `FormatException`.
- Version 7 GUIDs reveal their creation time. Use `Guid.NewGuid()` for tokens that must not.
- Use `NewSqlServerGuid()` only for SQL Server; it does not sort by time elsewhere.
- `IsEmpty()` is a method on `string?` and is `true` for whitespace. Do not import `CSharpEssentials` and `CSharpEssentials.Core` in the same file if it calls `IsEmpty` on a string: that is CS9339 on .NET 10.
- `ForEach` on `IEnumerable<T>` is lazy and returns the sequence; enumerate it. On a `List<T>` variable it binds to `List<T>.ForEach`.
- `IfNotNull` and `IfNull` are statements that return `bool`. For transforms use `Maybe<T>.Map` (CSharpEssentials.Maybe).
- In a cloned VM or restored process, set the AppContext switch `CSharpEssentials.Guider.DisableRandomPool` to `true`.
- XML docs: `<see cref="Guider.NewGuid()"/>`; a bare `Guider.NewGuid` is CS0419 since 9.0.
