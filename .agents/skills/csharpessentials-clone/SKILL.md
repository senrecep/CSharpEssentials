---
name: csharpessentials-clone
description: Use when entities need deep-copy semantics — implement ICloneable<T> on domain objects, then call .Clone() on IEnumerable<T> or IQueryable<T> collections to produce independent deep copies of every element.
---

# CSharpEssentials.Clone

Typed deep-copy contract for domain objects. `ICloneable<T>` is covariant and type-safe — unlike `System.ICloneable` which returns `object`.

## Installation

```bash
dotnet add package CSharpEssentials.Clone
```

## Namespace

```csharp
using CSharpEssentials.Clone;
```

---

## Implement ICloneable\<T\>

```csharp
public class Product : ICloneable<Product>
{
    public int Id       { get; init; }
    public string Name  { get; init; } = "";
    public List<Tag> Tags { get; init; } = new();

    public Product Clone() => new()
    {
        Id   = Id,
        Name = Name,
        Tags = Tags.Select(t => t.Clone()).ToList()  // deep-copy child collections too
    };
}

public class Tag : ICloneable<Tag>
{
    public string Value { get; init; } = "";
    public Tag Clone() => new() { Value = Value };
}
```

---

## Clone Collections

Extension methods call `Clone()` on every element:

```csharp
// IEnumerable<T> where T : ICloneable<T>
IEnumerable<Product> copies = products.Clone();

// IQueryable<T> where T : ICloneable<T>
IQueryable<Product> projected = dbSet.Clone();
```

---

## Typical Use Case

Snapshot EF Core results before applying in-memory transformations, without mutating tracked entities:

```csharp
var snapshot = await _db.Products
    .Where(p => p.CategoryId == id)
    .ToListAsync();

var working = snapshot.Clone();  // independent deep copies — mutations don't affect EF tracking
ApplyDiscounts(working);
```

---

## Best Practices

- Always deep-copy nested collections inside `Clone()` — a shallow copy defeats the purpose
- `ICloneable<T>` is covariant (`out T`) — a `Product : ICloneable<Product>` satisfies `ICloneable<object>` if needed
- Consider using `record` types with `with` expressions for immutable value objects instead of `ICloneable<T>`
- `ICloneable<T>` is most valuable for mutable domain objects that are tracked by EF Core
