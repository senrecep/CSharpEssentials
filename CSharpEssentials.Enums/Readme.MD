# CSharpEssentials.Enums

`CSharpEssentials.Enums` provides enum metadata attributes and a built-in source generator for fast, compile-time enum operations. It is designed to work with **CSharpEssentials.Json** and **CSharpEssentials.EntityFrameworkCore** for serialization and storage scenarios.

## 🚀 Features

- **StringEnumAttribute**: A marker attribute to explicitly indicate that an `Enum` should be serialized as a string rather than an integer.
- **Built-in Source Generator**: Apply `[StringEnum]` to enums and the generator produces compile-time optimized extension methods and constants.

## 📦 Installation

```bash
dotnet add package CSharpEssentials.Enums
```

## 🛠 Usage

Apply the `[StringEnum]` attribute to your enum definitions.

```csharp
using CSharpEssentials.Enums;

[StringEnum]
public enum UserStatus
{
    Active,
    Inactive,
    Suspended
}
```

The source generator automatically creates `{Enum}Extensions` with:

| Member | Description |
|--------|-------------|
| `ToOptimizedString()` | Fast `switch`-based string conversion |
| `ToSnakeCase()` | Compile-time snake_case string |
| `ToKebabCase()` | Compile-time kebab-case string |
| `ToLowerCase()` | Compile-time lower-case string |
| `ToUpperCase()` | Compile-time upper-case string |
| `{Member}SnakeCase` | Public constant for snake_case |
| `{Member}KebabCase` | Public constant for kebab-case |
| `IsDefined(string)` | Fast name lookup |
| `TryParse(string, out Enum)` | Fast parsing with numeric fallback |
| `Parse(string)` | Fast parsing (throws on failure) |
| `GetNames()` | Array of enum member names |
| `GetValues()` | Array of enum values |
| `AsUnderlyingType()` | Cast to backing numeric type |

### Example

```csharp
UserStatus s = UserStatus.Active;

s.ToOptimizedString();      // "Active"
s.ToSnakeCase();            // "active"
s.ToKebabCase();            // "active"

UserStatusExtensions.ActiveSnakeCase;   // "active"
UserStatusExtensions.InactiveKebabCase; // "inactive"

UserStatusExtensions.IsDefined("Active");           // true
UserStatusExtensions.TryParse("Inactive", out var v); // true, v = Inactive
```

### Integration
This package is intended to be used with serialization libraries (like `CSharpEssentials.Json`) and ORMs (like `CSharpEssentials.EntityFrameworkCore`) that look for this marker to automatically apply string conversion logic.
