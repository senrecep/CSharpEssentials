# String Enum Support

[![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Enums.svg)](https://www.nuget.org/packages/CSharpEssentials.Enums) [![NuGet](https://img.shields.io/nuget/dt/CSharpEssentials.Enums.svg)](https://www.nuget.org/packages/CSharpEssentials.Enums) [![GitHub](https://img.shields.io/github/stars/senrecep/CSharpEssentials.svg)](https://github.com/senrecep/CSharpEssentials)

This module provides enhanced enum functionality for C# applications, focusing on string-based enum operations and conversions.

## Features

### StringEnumAttribute

A marker attribute that enables automatic string-based handling for enums:

```csharp
[StringEnum]
public enum UserType
{
    Admin,
    SystemAdmin,
    RegularUser
}
```

## Integration Points

### 1. JSON Serialization

- Automatic string serialization for marked enums
- Uses snake_case by default (e.g., `SystemAdmin` → `"system_admin"`)
- Configurable naming policy and integer value support

```csharp
// Example JSON output
{
    "userType": "system_admin"
}
```

### 2. Entity Framework Core

- Automatic string conversion for database storage
- Snake case formatting
- Auto-calculated column lengths
- Bidirectional conversion support

```csharp
// Database storage example
user_type (varchar(12)) = "system_admin"
```

## Usage

1. Mark your enum with the attribute:

```csharp
[StringEnum]
public enum Status
{
    Active,
    Inactive,
    Pending
}
```

2. For EF Core, configure in your DbContext:

```csharp
protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
{
    configurationBuilder.ConfigureEnumConventions();
}
```

3. For JSON serialization, add the converter:

```csharp
services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new ConditionalStringEnumConverter());
    });
```

## Benefits

- Consistent enum handling across different layers
- Human-readable values in database and JSON
- Automatic case formatting
- Type-safe enum operations
- Reduced boilerplate code
