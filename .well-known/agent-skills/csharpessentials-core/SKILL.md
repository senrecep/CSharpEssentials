---
name: csharpessentials-core
description: Use when you need low-level C# helpers. Covers string case conversions (ToPascalCase/ToSnakeCase/ToKebabCase), Guider.NewGuid and URL-safe GUID strings, IsBetween/IsBetweenExclusive range checks, collection helpers (IfAdd, WhereIf, WithoutNulls), IfNotNull/IfTrue statements and WithCancellation for tasks.
---

# CSharpEssentials.Core

Lightweight C# utility helpers. No functional patterns here; those live in the Results, Errors, Maybe, and Any skills.

## Installation

```bash
dotnet add package CSharpEssentials.Core
```

Or the meta-package, which includes Core:

```bash
dotnet add package CSharpEssentials
```

## Namespace

```csharp
using CSharpEssentials.Core;
```

---

## String Case Conversions

```csharp
string pascal = "helloWorld".ToPascalCase();   // "HelloWorld"
string snake  = "HelloWorld".ToSnakeCase();    // "hello_world"
string kebab  = "HelloWorld".ToKebabCase();    // "hello-world"
string camel  = "hello-world".ToCamelCase();   // "helloWorld"
string macro  = "HelloWorld".ToMacroCase();    // "HELLO_WORLD"
```

Also available: `ToTitleCase`, `ToTrainCase`, `ToUnderscoreCamelCase`. Each takes an optional `CultureInfo`.

---

## GUID Utilities

```csharp
// Version 7 (time-sortable) GUID on .NET 9+, Guid.NewGuid() on older targets
Guid id = Guider.NewGuid();

// Compact, URL-safe 22-character string and back
string shortId = id.ToStringFromGuid();
Guid parsed = shortId.ToGuidFromString();
```

---

## Range Checks

```csharp
bool inRange = 5.IsBetween(1, 10);            // inclusive: 1 <= 5 <= 10
bool strict  = 10.IsBetweenExclusive(1, 10);  // exclusive: false
```

Both work for any `IComparable<T>` (`DateTime`, `decimal`, `string`, ...).

---

## Collection Helpers

```csharp
List<string> tags = ["a"];
tags.IfAdd(includeBeta, "beta");                 // adds only when the condition is true
tags.IfAddRange(includeExtra, ["x", "y"]);       // items as a collection; separate arguments need C# 13

IEnumerable<User> active = users.WhereIf(onlyActive, u => u.IsActive);   // also on IQueryable<T>
IEnumerable<string> names = rawNames.WithoutNulls();
```

---

## Statement Helpers and Tasks

```csharp
// Run an action only when the value is not null; returns whether it ran
user.IfNotNull(u => Console.WriteLine(u.Name));

// Run an action only when the condition is true
isAdmin.IfTrue(() => Console.WriteLine("admin"));

// Stop awaiting when the token is cancelled (throws OperationCanceledException)
string data = await LoadAsync().WithCancellation(ct);
```

`IfNotNull` and `IfNull` also take an else action, and each overload returns a `bool`: whether the value was not null (`IfNotNull`) or null (`IfNull`).

---

## Also Available

```csharp
bool same = listA.HasSameElements(listB);        // set comparison, order and duplicates ignored
bool all  = flags.AllTrue();                     // also AllFalse()
bool blank = text.IsEmpty();                     // null, "" or whitespace; also IsNotEmpty()
int pick  = numbers.GetRandomItem();             // RandomNumberGenerator; T[], List<T>, Span<T>; also GetRandomItems(count)
IEnumerable<Exception> chain = ex.GetInnerExceptions();   // ex, then each inner exception; also GetInnerExceptionsMessages()
int status = HttpCodes.NotFound;                 // 404; HTTP status code constants
```

- `ForEach(action)` on `IEnumerable<T>` is lazy: the action runs while the result is enumerated. On a `List<T>` the built-in `List<T>.ForEach` wins.
- `ExplicitCast<T>()`, `MsToDateTime()` (Unix milliseconds) and `GetTypeGroup<TGroup, TType>(group = 100)` are small conversion helpers.
- `ITransactionRunner` (namespace `CSharpEssentials.Transactions`) is a dependency-free abstraction for running work in a transaction; `EfCoreTransactionRunner<TDbContext>` implements it and `TransactionBehavior` in `CSharpEssentials.Mediator` consumes it.
- On .NET 10+, `using CSharpEssentials;` adds the extension members `IEnumerable<T>.IsEmpty` and `string.IsPalindrome`.

---

## Best Practices

- Use `Guider.NewGuid()` for database primary keys. On .NET 9+ it returns time-sortable version 7 GUIDs
- `WithoutNulls()` keeps nullable annotations correct, unlike `.Where(x => x != null)`
- `IfNotNull()` is a statement form; for transforms use `Maybe<T>.Map()` instead
- `WithCancellation` stops waiting; it does not cancel the underlying task
- Case conversions use a `stackalloc` buffer sized from the input; keep them for identifiers and short text
- `Guider.ToGuidFromString` does not validate its input; only pass strings produced by `ToStringFromGuid`
