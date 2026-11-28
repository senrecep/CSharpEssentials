# CSharpEssentials.Clone

`ICloneable<out T>` is a typed copy contract: `T Clone()` instead of the `object Clone()` of `System.ICloneable`. Two extension methods clone every item of an `IEnumerable<T>` or `IQueryable<T>`. The library only calls your `Clone()`; your implementation decides how deep the copy goes.

Benchmarks of `Clone().ToList()`, the array and lazy sources and the `IQueryable<T>` overload against hand-written loops, `List<T>.ConvertAll` and `Array.ConvertAll` at 10 and 1,000 items are in [BENCHMARKS.md](./BENCHMARKS.md).

## Install

```bash
dotnet add package CSharpEssentials.Clone
```

Targets `net11.0`, `net10.0`, `net9.0`, `netstandard2.1` and `netstandard2.0`. The meta-package `CSharpEssentials` includes it.

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials.Clone
using CSharpEssentials.Clone;

var original = new User { Name = "Alice", Address = new Address { City = "NYC" } };
User copy = original.Clone();
copy.Address.City = "LA";
Console.WriteLine($"{original.Address.City} {copy.Address.City}"); // NYC LA

List<User> users = [original, new User { Name = "Bob" }];
List<User> copies = users.Clone().ToList();
copies[1].Name = "Robert";
Console.WriteLine($"{users[1].Name} {copies[1].Name}");           // Bob Robert

public sealed class User : ICloneable<User>
{
    public string Name { get; set; } = "";
    public Address Address { get; set; } = new();

    public User Clone() => new() { Name = Name, Address = Address.Clone() };
}

public sealed class Address : ICloneable<Address>
{
    public string City { get; set; } = "";

    public Address Clone() => new() { City = City };
}
```

## Implementing ICloneable\<T\>

`ICloneable<out T>` has one member, `T Clone()`. Copy every field, and call `Clone()` on child objects and on the items of child collections that must not be shared. The interface is covariant, so a `User : ICloneable<User>` is also an `ICloneable<object>`.

A single object has no extension method: `original.Clone()` is the instance method you write.

## Cloning collections

```csharp Program.cs
// dotnet add package CSharpEssentials.Clone
using CSharpEssentials.Clone;

Tag[] tags = [new() { Value = "a" }, new() { Value = "b" }];

IEnumerable<Tag> lazy = ((IEnumerable<Tag>)tags).Clone(); // nothing is copied yet
Tag[] copies = lazy.ToArray();                            // Clone() runs here, once per item
copies[0].Value = "changed";
Console.WriteLine($"{tags[0].Value} {copies[0].Value}");  // a changed

IQueryable<Tag> query = tags.AsQueryable().Where(t => t.Value == "b");
List<Tag> fromQuery = query.Clone().ToList();             // read, then cloned in memory
Console.WriteLine(fromQuery.Single().Value);              // b

public sealed class Tag : ICloneable<Tag>
{
    public string Value { get; set; } = "";
    public Tag Clone() => new() { Value = Value };
}
```

- Both extensions require `T : ICloneable<T>`.
- They are lazy. `Clone()` runs for each item every time the result is enumerated, so call `ToList()` or `ToArray()` once.
- There is no array overload. `array.Clone()` on a variable typed `T[]` binds to `Array.Clone()`, a shallow copy that returns `object`. Cast to `IEnumerable<T>` first, as above.
- The `IQueryable<T>` overload is not translated by the query provider. It reads the query as usual and wraps the cloned items in an in-memory `EnumerableQuery<T>`. That type compiles expression trees at run time, so the overload carries `[RequiresUnreferencedCode]` and `[RequiresDynamicCode]`. Use the `IEnumerable<T>` overload in trimmed or Native AOT apps.

## When to use it

- Snapshot entities loaded by EF Core before changing them in memory, so tracked instances stay untouched.
- Hand a copy of mutable state to code that must not change the original.

For immutable values, a `record` with a `with` expression is simpler.

## See also

- [Agent skill](https://github.com/senrecep/CSharpEssentials/blob/main/.well-known/agent-skills/csharpessentials-clone/SKILL.md) and [example project](https://github.com/senrecep/CSharpEssentials/blob/main/examples/Examples.Clone).
