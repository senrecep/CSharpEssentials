---
name: csharpessentials-enums
description: Use when you need enum-to-string serialization without reflection — [StringEnum] source generator produces compile-time ToString(), Parse(), and TryParse() methods that are NativeAOT-safe and zero-allocation.
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
```

## Usage

```csharp
[StringEnum]
public enum OrderStatus { Pending, Processing, Shipped, Delivered, Cancelled }

[StringEnum]
public enum UserRole { Admin, Editor, Viewer }
```

The source generator emits at compile time:

```csharp
// Generated methods (no reflection, no allocations)
string s    = OrderStatus.Shipped.ToStringFast();   // "Shipped"
bool parsed = OrderStatus.TryParse("Shipped", out OrderStatus status);
OrderStatus s = OrderStatus.Parse("Shipped");       // throws on unknown value
```

## JSON Integration

`ConditionalStringEnumConverter` (in `CSharpEssentials.Json`) serializes `[StringEnum]`-decorated enums as strings and all others as integers:

```csharp
// In JsonOptions setup
options.Converters.Add(new ConditionalStringEnumConverter());

// OrderStatus (has [StringEnum]) → "Shipped" in JSON
// HttpMethod (no [StringEnum])   → 2 in JSON
```

## Best Practices

- Apply `[StringEnum]` to any enum that appears in API responses, logs, or database columns as text
- Combine with `ConditionalStringEnumConverter` so string enums serialize naturally in ASP.NET Core
- Generated methods are NativeAOT-safe — no `RuntimeReflectionExtensions` involved
