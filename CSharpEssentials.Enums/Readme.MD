# CSharpEssentials.Enums

`CSharpEssentials.Enums` is a minimal utility package defining metadata attributes for Enum handling. It is primarily designed to work in conjunction with **CSharpEssentials.Json** to control serialization behaviors.

## 🚀 Features

- **StringEnumAttribute**: A marker attribute to explicitly indicate that an `Enum` should be serialized as a string rather than an integer.

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

### Integration
While this package only provides the attribute, it is intended to be used with serialization libraries (like `CSharpEssentials.Json`) that look for this marker to automatically apply string conversion logic.
