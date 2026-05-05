# CSharpEssentials.Enums

Source generator for fast, compile-time enum operations. Mark enums with `[StringEnum]` and get optimized extension methods generated automatically.

## Features

- **StringEnumAttribute** — Marker to opt enums into source generation.
- **Source Generator** — Compile-time optimized `ToString`, parsing, and formatting with zero reflection.

## Installation

```bash
dotnet add package CSharpEssentials.Enums
```

## Usage

### Mark Your Enum

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

### Generated Extensions

```csharp
UserStatus s = UserStatus.Active;

s.ToOptimizedString();      // "Active"
s.ToSnakeCase();            // "active"
s.ToKebabCase();            // "active"
s.ToLowerCase();            // "active"
s.ToUpperCase();            // "ACTIVE"
s.AsUnderlyingType();       // 0

// Constants
UserStatusExtensions.ActiveSnakeCase;    // "active"
UserStatusExtensions.ActiveKebabCase;    // "active"

// Lookup
UserStatusExtensions.IsDefined("Active");              // true
UserStatusExtensions.TryParse("Inactive", out var v);  // true, v = Inactive
UserStatusExtensions.Parse("Suspended");               // UserStatus.Suspended

// Metadata
UserStatusExtensions.GetNames();   // ["Active", "Inactive", "Suspended"]
UserStatusExtensions.GetValues();  // [Active, Inactive, Suspended]
```

### Integration

`CSharpEssentials.Json` and `CSharpEssentials.EntityFrameworkCore` recognize `[StringEnum]` to automatically serialize enums as strings.
