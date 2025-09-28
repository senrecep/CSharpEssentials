# CSharpEssentials.Clone

[![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Clone.svg)](https://www.nuget.org/packages/CSharpEssentials.Clone) [![NuGet](https://img.shields.io/nuget/dt/CSharpEssentials.Clone.svg)](https://www.nuget.org/packages/CSharpEssentials.Clone) [![GitHub](https://img.shields.io/github/stars/senrecep/CSharpEssentials.svg)](https://github.com/senrecep/CSharpEssentials)

This library provides functional programming approach to object cloning in C#. It offers type-safe cloning capabilities for both single objects and collections.

## Features

- Generic type-safe cloning with covariant type parameter
- Extension methods for easy usage
- Support for both single objects and collections (`IEnumerable<T>` and `IQueryable<T>`)
- LINQ-friendly design
- Functional programming approach

## Usage

### 1. Implement `ICloneable<T>` Interface

```csharp
public class Person : ICloneable<Person>
{
    public string Name { get; set; }
    public int Age { get; set; }

    public Person Clone()
    {
        return new Person
        {
            Name = this.Name,
            Age = this.Age
        };
    }
}
```

### 2. Use Extension Methods

```csharp
// Single object cloning
var person = new Person { Name = "John", Age = 30 };
var clonedPerson = person.Clone();

// List cloning
var people = new List<Person>
{
    new Person { Name = "John", Age = 30 },
    new Person { Name = "Jane", Age = 25 }
};
var clonedPeople = people.Clone();

// LINQ query cloning
var queryablePeople = people.AsQueryable();
var clonedQueryable = queryablePeople.Clone();
```

## Benefits

1. **Type Safety**: Generic type parameter ensures compile-time type checking
2. **Functional Approach**: Extension methods and LINQ support functional programming paradigms
3. **Collection Support**: Cloning support for both single objects and collections
4. **Chainable API**: Can be used with LINQ methods seamlessly
5. **Immutability Support**: Perfect for working with immutable data structures

## Best Practices

- Use for defensive copying when working with mutable objects
- Implement deep cloning when needed for complex objects
- Combine with LINQ for powerful data transformations
- Consider using with immutable objects for functional programming patterns

## Implementation Details

```csharp
public interface ICloneable<out T>
{
    T Clone();
}

public static class Extensions
{
    private static T Clone<T>(this T source)
        where T : ICloneable<T> => source.Clone();

    public static IEnumerable<T> Clone<T>(this IEnumerable<T> source)
        where T : ICloneable<T> => source.Select(Clone);

    public static IQueryable<T> Clone<T>(this IQueryable<T> source)
        where T : ICloneable<T> => source.Select(Clone).AsQueryable();
}
```
