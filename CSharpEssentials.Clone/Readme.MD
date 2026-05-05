# CSharpEssentials.Clone

Type-safe generic cloning for .NET. Provides a strongly-typed `ICloneable<T>` alternative to the legacy `System.ICloneable`, plus extension methods for cloning collections.

## Features

- **Generic `ICloneable<T>`** — Type-safe cloning interface.
- **Collection Extensions** — Clone `IEnumerable<T>` and `IQueryable<T>` in one call.
- **Deep Copy Support** — Compose clones for nested objects.

## Installation

```bash
dotnet add package CSharpEssentials.Clone
```

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
