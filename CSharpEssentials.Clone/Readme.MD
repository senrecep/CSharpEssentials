# CSharpEssentials.Clone

`ICloneable<out T>`, a typed alternative to `System.ICloneable` with a single `T Clone()` method, plus extensions that clone every item of an `IEnumerable<T>` or `IQueryable<T>`. The library only calls `Clone()`; your implementation decides how deep the copy goes.

Benchmarks of `Clone().ToList()`, the array and lazy sources and the `IQueryable<T>` overload against hand-written loops, `List<T>.ConvertAll` and `Array.ConvertAll` at 10 and 1,000 items are in [BENCHMARKS.md](./BENCHMARKS.md).

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

```csharp
User original = new() { Name = "Alice", Address = new Address { City = "NYC" } };
User clone = original.Clone();

clone.Address.City = "LA";   // original.Address.City is still "NYC"
```

A single object has no extension method: `Clone()` is the instance method you implement.

### Cloning collections

```csharp
List<User> users = [new() { Name = "Alice" }, new() { Name = "Bob" }];

List<User> cloned = users.Clone().ToList();

IQueryable<User> query = users.AsQueryable();
IQueryable<User> clonedQuery = query.Clone();
```

Both extensions require `T : ICloneable<T>` and are lazy: `Clone()` runs for each element as the result is enumerated, so call `ToList()` to materialize the copies once. There is no array overload; `Clone()` on a variable typed `T[]` binds to `Array.Clone()`, a shallow copy that returns `object`. Use `((IEnumerable<T>)array).Clone().ToArray()` to clone the items.

The `IQueryable<T>` overload returns an in-memory query (`EnumerableQuery<T>`). It does not translate the cloning into the provider's query: a provider-backed query is read as usual and then cloned in memory. Because `EnumerableQuery<T>` compiles expression trees at run time, the overload is marked `[RequiresUnreferencedCode]` and `[RequiresDynamicCode]`; use the `IEnumerable<T>` overload in trimmed or Native AOT apps.
