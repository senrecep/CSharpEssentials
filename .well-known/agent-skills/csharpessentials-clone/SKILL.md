---
name: csharpessentials-clone
description: Use when entities need deep-copy semantics. Implement ICloneable<T> on domain objects, then call .Clone() on IEnumerable<T> or IQueryable<T> collections to produce independent deep copies of every element.
---

# CSharpEssentials.Clone

Typed copy contract for domain objects. `ICloneable<out T>.Clone()` returns `T`, unlike `System.ICloneable`, which returns `object`. The library only calls `Clone()`; your implementation decides how deep the copy goes.

```bash
dotnet add package CSharpEssentials.Clone
```

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

## Clone collections

```csharp
List<Product> products = [new() { Id = 1, Name = "Pen" }];

// IEnumerable<T> where T : ICloneable<T>: lazily clones each element
IEnumerable<Product> copies = products.Clone();

// IQueryable<T> where T : ICloneable<T>: clones in memory, not translated to SQL;
// marked [RequiresUnreferencedCode]/[RequiresDynamicCode], so use the IEnumerable<T> overload under AOT
IQueryable<Product> projected = products.AsQueryable().Clone();
```

---

## Typical use

Snapshot EF Core results before applying in-memory transformations, without mutating tracked entities:

```csharp
List<Product> snapshot = await db.Products
    .Where(p => p.Name.StartsWith("P"))
    .ToListAsync();

List<Product> working = snapshot.Clone().ToList();  // independent deep copies; mutations don't affect EF tracking
```

---

## Guidance

- Deep-copy nested collections inside `Clone()`; the extensions do not.
- `ICloneable<T>` is covariant (`out T`): a `Product : ICloneable<Product>` is also an `ICloneable<object>`.
- `Clone()` on a sequence is lazy; call `ToList()` to materialize the copies once. There is no array overload: `Clone()` on a `T[]` variable binds to `Array.Clone()` (shallow, returns `object`).
- For immutable value objects, `record` types with `with` expressions are simpler than `ICloneable<T>`.
