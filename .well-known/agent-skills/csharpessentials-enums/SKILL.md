---
name: csharpessentials-enums
description: Use when you need reflection-free enum string conversion. The [StringEnum] source generator emits {Enum}Extensions with ToOptimizedString/ToSnakeCase/ToKebabCase, IsDefined/TryParse/Parse, GetNames/GetValues and name constants, NativeAOT-safe; analyzer CSE0001 flags nested [StringEnum] enums.
---

# CSharpEssentials.Enums

`[StringEnum]` is a source generator attribute that produces fast, reflection-free string conversion methods for enum types. Safe for NativeAOT and Blazor WASM.

## Installation

```bash
dotnet add package CSharpEssentials.Enums
```

## Namespace

```csharp
using CSharpEssentials.Enums;
using CSharpEssentials.Json;   // ConditionalStringEnumConverter
```

## Usage

```csharp
[StringEnum]
public enum OrderStatus { Pending, Processing, Shipped, Delivered, Cancelled }
```

The generator emits a static `OrderStatusExtensions` class in the enum's namespace:

```csharp
OrderStatus status = OrderStatus.Shipped;

string name  = status.ToOptimizedString();   // "Shipped"
string snake = status.ToSnakeCase();         // "shipped"
string kebab = status.ToKebabCase();         // "shipped"
string upper = status.ToUpperCase();         // "SHIPPED"
int raw      = status.AsUnderlyingType();    // 2

bool known   = OrderStatusExtensions.IsDefined("Shipped");
bool parsed  = OrderStatusExtensions.TryParse("Shipped", out OrderStatus value);
OrderStatus s = OrderStatusExtensions.Parse("Shipped");   // throws on unknown names
string[] names = OrderStatusExtensions.GetNames();
OrderStatus[] values = OrderStatusExtensions.GetValues();
const string Constant = OrderStatusExtensions.ShippedSnakeCase; // "shipped"
```

## Nested Enums (CSE0001)

Extensions are generated only for enums declared directly in a namespace. A `[StringEnum]` enum nested in a class or struct is skipped and the analyzer reports `CSE0001` (Info). Move the enum to namespace level, or raise the severity with `dotnet_diagnostic.CSE0001.severity = warning` in `.editorconfig`.

## JSON and EF Core Integration

`ConditionalStringEnumConverter` (in `CSharpEssentials.Json`) writes `[StringEnum]` enums as snake_case strings and other enums as numbers; EF Core enum storage in `CSharpEssentials.EntityFrameworkCore` uses the same naming.

```csharp
var options = new JsonSerializerOptions();
options.Converters.Add(new ConditionalStringEnumConverter());

// OrderStatus.Shipped ([StringEnum]) → "shipped"
// Plain enum value                  → 2
```

## Best Practices

- Apply `[StringEnum]` to enums that appear in API responses, logs, or database columns as text
- Keep `[StringEnum]` enums at namespace level (CSE0001)
- Generated methods use `switch` expressions: no reflection, NativeAOT-safe
