---
name: csharpessentials-core
description: Use for low-level C# utility helpers — string case conversions (ToPascalCase/ToSnakeCase/ToKebabCase), URL-safe and v7 GUID generation, null-safe collection helpers (WhereNotNull, AddIf), and async cancellation utilities.
---

# CSharpEssentials.Core

Lightweight C# utility helpers. No functional patterns here — those live in the Results, Errors, Maybe, and Any skills.

## Installation

```bash
dotnet add package CSharpEssentials.Core
```

Or the meta-package (includes Core + Results + Errors + Maybe + Any):

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
"helloWorld".ToPascalCase()     // "HelloWorld"
"HelloWorld".ToSnakeCase()      // "hello_world"
"HelloWorld".ToKebabCase()      // "hello-world"
"hello-world".ToCamelCase()     // "helloWorld"
```

---

## GUID Utilities

```csharp
// URL-safe Base64 GUID (compact, URL-safe, no padding)
string id = Guider.NewGuid();

// Version 7 GUID — time-sortable, database index-friendly (.NET 9+)
Guid id = Guider.NewGuidV7();
```

---

## Null-Safe / Conditional Helpers

```csharp
// Execute action only when value is non-null
value.IfNotNull(v => process(v));

// Add to list conditionally
list.AddIf(condition, item);

// Filter nulls from sequence
IEnumerable<string> names = rawList.WhereNotNull();

// Cancellation-aware task awaiting
await longRunningTask.WithCancellation(ct);
```

---

## Best Practices

- Use `Guider.NewGuidV7()` for database primary keys — time-sortable GUIDs reduce index fragmentation
- `WhereNotNull()` is safer than `.Where(x => x != null).Select(x => x!)` — handles nullable annotations correctly
- `IfNotNull()` is a statement form; for transforms use `Maybe<T>.Map()` instead
