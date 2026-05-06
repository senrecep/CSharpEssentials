---
name: csharpessentials-maybe
description: Use when representing optional values explicitly — Maybe<T> as a null-safe container, Maybe.From() for creation, HasValue/HasNoValue, Map/Bind chaining, Match for consumption, and ToMaybeResult() to bridge into the Result pattern.
---

# CSharpEssentials.Maybe

`Maybe<T>` makes optionality explicit. No null reference exceptions — the absence of a value is a first-class concept.

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
Maybe<string> name    = Maybe.From(user?.Name);   // null → None, value → Some
Maybe<string> none    = Maybe<string>.None;
Maybe<string> some    = Maybe.From("Alice");
Maybe<string> implicit = user.Name;               // implicit T? → Maybe<T>
```

`Maybe.From(null)` → `None`. `Maybe.From(value)` → `Some(value)`. Never use `.ToMaybe()` — that method does not exist.

## Checking Value

```csharp
bool has    = maybe.HasValue;
bool empty  = maybe.HasNoValue;
string val  = maybe.GetValueOrDefault("fallback");
string val  = maybe.GetValueOrThrow();             // throws if None
```

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
    .Bind(u => Maybe.From(u?.Address));
```

## Bridge to Result

```csharp
// Convert Maybe → Result, providing the error for the None case
Result<string> r = maybe.ToMaybeResult(
    Error.NotFound("user.email", "No email address on file."));
```

## Best Practices

- Use `Maybe.From()` — not `.ToMaybe()` (doesn't exist)
- Prefer `Match()` over `HasValue` + `GetValueOrThrow()` to avoid branches
- Use `Bind()` when the transform itself can be absent (returns `Maybe<T>`)
- Bridge to `Result` with `ToMaybeResult()` when the caller needs error information
