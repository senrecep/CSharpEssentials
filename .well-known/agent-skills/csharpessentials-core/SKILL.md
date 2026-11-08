---
name: csharpessentials-core
description: Use when you need low-level C# helpers. Covers string case conversions (ToPascalCase/ToSnakeCase/ToKebabCase), Guider.NewGuid and URL-safe GUID strings, IsBetween/IsBetweenExclusive range checks, collection helpers (IfAdd, WhereIf, WithoutNulls), IfNotNull/IfTrue statements and WithCancellation for tasks.
---

# CSharpEssentials.Core

Small C# utility helpers. Functional types live in the Results, Errors, Maybe and Any skills.

```bash
dotnet add package CSharpEssentials.Core   # or the CSharpEssentials meta-package, which includes it
```

```csharp
using CSharpEssentials.Core;
```

## String case conversions

```csharp
string pascal = "helloWorld".ToPascalCase();   // "HelloWorld"
string snake  = "HelloWorld".ToSnakeCase();    // "hello_world"
string kebab  = "HelloWorld".ToKebabCase();    // "hello-world"
string camel  = "hello-world".ToCamelCase();   // "helloWorld"
string macro  = "HelloWorld".ToMacroCase();    // "HELLO_WORLD"
```

Also `ToTitleCase`, `ToTrainCase`, `ToUnderscoreCamelCase`. Each takes an optional `CultureInfo` (invariant when omitted). Letters without case (CJK, Arabic, Hebrew, Thai) are kept and continue the current word.

## GUIDs

```csharp
Guid id = Guider.NewGuid();                 // version 7, time-sortable, every target
Guid ordered = Guider.NewMonotonicGuid();   // version 7, strictly increasing within the process
Guid sqlKey = Guider.NewSqlServerGuid();    // version 8, sequential in SQL Server uniqueidentifier order

string shortId = id.ToStringFromGuid();     // 22-char RFC 4648 base64url, no padding
Guid parsed = shortId.ToGuidFromString();
```

`ToGuidFromString` requires exactly 22 characters from `A-Z a-z 0-9 - _` and throws `FormatException` otherwise (no `TryParse` form). Version 7 GUIDs reveal their creation time; use `Guid.NewGuid()` where that matters. In a cloned VM or a checkpointed and restored process, set the AppContext switch `CSharpEssentials.Guider.DisableRandomPool` to `true`.

## Range checks

```csharp
bool inRange = 5.IsBetween(1, 10);            // inclusive
bool strict  = 10.IsBetweenExclusive(1, 10);  // exclusive: false
```

Both work for any `IComparable<T>`.

## Collection helpers

```csharp
List<string> tags = ["a"];
tags.IfAdd(includeBeta, "beta");                 // adds only when the condition is true
tags.IfAddRange(includeExtra, ["x", "y"]);       // params collection; separate arguments need C# 13

IEnumerable<User> active = users.WhereIf(onlyActive, u => u.IsActive);   // also on IQueryable<T>
IEnumerable<string> names = rawNames.WithoutNulls();
```

## Statement helpers and tasks

```csharp
user.IfNotNull(u => Console.WriteLine(u.Name));        // returns whether it ran
isAdmin.IfTrue(() => Console.WriteLine("admin"));
string data = await LoadAsync().WithCancellation(ct);  // throws OperationCanceledException on cancel
```

`IfNotNull` and `IfNull` also take an else action, and each overload returns a `bool`: whether the value was not null (`IfNotNull`) or null (`IfNull`). `WithCancellation` stops waiting; it does not cancel the underlying task. On .NET 9+ it throws the `TaskCanceledException` subclass.

## Also available

```csharp
bool same = listA.HasSameElements(listB);        // set comparison, order and duplicates ignored
bool all  = flags.AllTrue();                     // also AllFalse()
bool blank = text.IsEmpty();                     // null, "" or whitespace; also IsNotEmpty()
int pick  = numbers.GetRandomItem();             // RandomNumberGenerator; T[], List<T>, Span<T>; also GetRandomItems(count)
IEnumerable<Exception> chain = ex.GetInnerExceptions();   // ex, then each inner exception; also GetInnerExceptionsMessages()
int status = HttpCodes.NotFound;                 // 404
```

- `ForEach(action)` on `IEnumerable<T>` is lazy: the action runs while the result is enumerated. On a `List<T>` the built-in `List<T>.ForEach` wins.
- `ExplicitCast<T>()`, `MsToDateTime()` (Unix milliseconds, `Kind = Utc`) and `GetTypeGroup<TGroup, TType>(group = 100)` are small conversion helpers.
- `ITransactionRunner` (namespace `CSharpEssentials.Transactions`) runs work in a transaction and commits when `shouldCommit` returns `true`; `EfCoreTransactionRunner<TDbContext>` implements it and `TransactionBehavior` in `CSharpEssentials.Mediator` consumes it.
- On .NET 10+, `using CSharpEssentials;` adds the extension properties `IEnumerable<T>.IsEmpty` and `string.IsPalindrome`.

## Guidance

- Use `Guider.NewGuid()` for primary keys, or `NewSqlServerGuid()` for SQL Server `uniqueidentifier` columns.
- `WithoutNulls()` keeps nullable annotations correct, unlike `.Where(x => x != null)`.
- `IfNotNull()` is a statement form; for transforms use `Maybe<T>.Map()`.
