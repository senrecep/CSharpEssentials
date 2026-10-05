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

### Nested Enums

Extension methods are only generated for top-level enums (enums declared directly in a namespace). A `[StringEnum]` enum nested inside a class or struct is skipped, and the analyzer reports `CSE0001` (Info severity) on it so the skip is not silent:

```csharp
public class Order
{
    [StringEnum] // CSE0001: no extension methods are generated for Order.State
    public enum State { Pending, Shipped }
}
```

Move the enum out of its containing type to get the generated extensions. `CSE0001` is informational, so it does not fail builds that use `TreatWarningsAsErrors`; set its severity in `.editorconfig` if you want it stricter (`dotnet_diagnostic.CSE0001.severity = warning`).

### Integration

`CSharpEssentials.Json` and `CSharpEssentials.EntityFrameworkCore` recognize `[StringEnum]` to automatically serialize enums as strings.
