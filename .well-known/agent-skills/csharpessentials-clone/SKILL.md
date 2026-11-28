---
name: csharpessentials-clone
description: Use when domain objects need a typed copy method. Implement ICloneable<T> (T Clone()) from CSharpEssentials.Clone on a class, then call Clone() on an IEnumerable<T> or IQueryable<T> to copy every item. The copy depth is whatever your Clone() does.
---

# CSharpEssentials.Clone

```bash
dotnet add package CSharpEssentials.Clone
```

```csharp
using CSharpEssentials.Clone;   // ICloneable<T>, Extensions.Clone
```

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials.Clone
using CSharpEssentials.Clone;

var product = new Product { Id = 1, Name = "Pen", Tags = [new Tag { Value = "office" }] };
Product copy = product.Clone();
copy.Tags[0].Value = "school";
Console.WriteLine($"{product.Tags[0].Value} {copy.Tags[0].Value}"); // office school

List<Product> products = [product];
List<Product> copies = products.Clone().ToList();
Console.WriteLine(ReferenceEquals(products[0], copies[0]));         // False

public sealed class Product : ICloneable<Product>
{
    public int Id { get; init; }
    public string Name { get; set; } = "";
    public List<Tag> Tags { get; init; } = [];

    public Product Clone() => new()
    {
        Id = Id,
        Name = Name,
        Tags = Tags.Select(t => t.Clone()).ToList(),
    };
}

public sealed class Tag : ICloneable<Tag>
{
    public string Value { get; set; } = "";
    public Tag Clone() => new() { Value = Value };
}
```

## Implementing `Clone()`

- The interface is `ICloneable<T>` with one method `T Clone()`. It is not `System.ICloneable`.
- Copy child objects with their own `Clone()` and child lists with `list.Select(x => x.Clone()).ToList()`. The library never walks your object graph.

## Cloning a sequence

```csharp
List<Product> copies = products.Clone().ToList();       // IEnumerable<T>, lazy until ToList
List<Product> fromDb = (await db.Products.ToListAsync()).Clone().ToList(); // snapshot of tracked entities
```

## Rules

- `T` must implement `ICloneable<T>`; the extension does not exist otherwise.
- The sequence overloads are lazy. Materialize once with `ToList()`/`ToArray()`, or every enumeration clones again.
- On a `T[]` variable, `array.Clone()` is `Array.Clone()` (shallow, returns `object`). Write `((IEnumerable<T>)array).Clone().ToArray()`.
- `IQueryable<T>.Clone()` is not translated to SQL; it reads the rows and clones in memory. It is marked `[RequiresUnreferencedCode]` and `[RequiresDynamicCode]`, so in Native AOT apps call `.AsEnumerable().Clone()` or clone a materialized list.
- For immutable value objects use a `record` and `with` instead.
