# CSharpEssentials.Clone

`CSharpEssentials.Clone` provides a type-safe generic cloning interface and extension methods. It addresses the limitations of the legacy `System.ICloneable` by providing a strongly-typed `ICloneable<T>`.

## 🚀 Features

- **Generic `ICloneable<T>`**: Type-safe interface for implementing clone behavior.
- **Collection Extensions**: Easily clone `IEnumerable<T>` and `IQueryable<T>` collections where items implement `ICloneable<T>`.

## 📦 Installation

```bash
dotnet add package CSharpEssentials.Clone
```

## 🛠 Usage

### 1. Implementing ICloneable\<T\>

Implement the interface in your classes to define how they should be copied.

```csharp
using CSharpEssentials.Clone;

public class User : ICloneable<User>
{
    public string Name { get; set; }
    public Address Address { get; set; }

    public User Clone()
    {
        return new User
        {
            Name = this.Name,
            Address = this.Address.Clone() // Assuming Address also implements ICloneable<Address>
        };
    }
}
```

### 2. Cloning Collections

Use the `Clone()` extension method to deeply copy collections of cloneable items.

```csharp
using CSharpEssentials.Clone;

var users = new List<User>
{
    new User { Name = "Alice" },
    new User { Name = "Bob" }
};

// Creates a new IEnumerable where each User is cloned
var clonedUsers = users.Clone();
```
