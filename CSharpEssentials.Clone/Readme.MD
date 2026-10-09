# CSharpEssentials.Clone

Type-safe generic cloning for .NET. Provides a strongly-typed `ICloneable<T>` alternative to the legacy `System.ICloneable`, plus extension methods for cloning collections.

## Features

- **Generic `ICloneable<out T>`**: Type-safe, covariant cloning interface with a single `T Clone()` method.
- **Collection Extensions**: Clone `IEnumerable<T>` and `IQueryable<T>` in one call.
- **Deep Copy Support**: The library only calls `Clone()`; you decide how deep it copies by composing clones of nested objects in your implementation.

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

### Cloning a Single Object

```csharp
User original = new() { Name = "Alice", Address = new Address { City = "NYC" } };
User clone = original.Clone();

clone.Name = "Bob";
clone.Address.City = "LA";

// original is unaffected
```

A single object has no extension method: `Clone()` is the instance method you implement.

### Cloning Collections

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
