# CSharpEssentials.Clone

Type-safe generic cloning for .NET. The package provides a strongly typed `ICloneable<T>` alternative to the legacy `System.ICloneable`, plus extension methods for cloning collections.

> **Benchmarks:** `Clone().ToList()`, the array and lazy sources and the `IQueryable<T>` overload against hand-written loops, `List<T>.ConvertAll` and `Array.ConvertAll` at 10 and 1,000 items are described in [BENCHMARKS.md](./BENCHMARKS.md). Results are pending the full run.

## Features

- `ICloneable<out T>`, a type-safe, covariant cloning interface with a single `T Clone()` method.
- Extensions that clone an `IEnumerable<T>` or `IQueryable<T>` in one call.
- Deep copies on your terms: the library only calls `Clone()`, and you decide how deep it copies by composing clones of nested objects in your implementation.

## Installation

```bash
dotnet add package CSharpEssentials.Clone
```

Targets `net11.0`, `net10.0`, `net9.0`, `netstandard2.1` and `netstandard2.0`.

## Usage

### Implementing `ICloneable<T>`

```csharp
using CSharpEssentials.Clone;

public class User : ICloneable<User>
{
    public string Name { get; set; } = string.Empty;
    public Address Address { get; set; } = new();

    public User Clone() => new()
    {
        Name = Name,
        Address = Address.Clone()
    };
}

public class Address : ICloneable<Address>
{
    public string City { get; set; } = string.Empty;

    public Address Clone() => new() { City = City };
}
```

### Cloning a single object

```csharp
User original = new() { Name = "Alice", Address = new Address { City = "NYC" } };
User clone = original.Clone();

clone.Name = "Bob";
clone.Address.City = "LA";

// original is unaffected
```

A single object has no extension method: `Clone()` is the instance method you implement.

### Cloning collections

```csharp
var users = new List<User>
{
    new() { Name = "Alice" },
    new() { Name = "Bob" }
};

// Clone all items
var cloned = users.Clone().ToList();

// Clone from IQueryable
IQueryable<User> query = users.AsQueryable();
var clonedQuery = query.Clone();
```

Both extensions require `T : ICloneable<T>` and are lazy: `Clone()` runs for each element as the result is enumerated, so call `ToList()` to materialize the copies once. The `IQueryable<T>` overload returns an in-memory query (`EnumerableQuery<T>`); it does not translate the cloning into the provider's query, so a provider-backed query is read as usual and then cloned in memory.
