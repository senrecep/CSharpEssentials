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
tags.IfAddRange(includeExtra, "x", "y");

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

---

## Best Practices

- Use `Guider.NewGuid()` for database primary keys. On .NET 9+ it returns time-sortable version 7 GUIDs
- `WithoutNulls()` keeps nullable annotations correct, unlike `.Where(x => x != null)`
- `IfNotNull()` is a statement form; for transforms use `Maybe<T>.Map()` instead
- `WithCancellation` stops waiting; it does not cancel the underlying task
