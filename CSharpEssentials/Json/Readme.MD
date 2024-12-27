# JSON Utilities for C#

This package provides enhanced JSON serialization and deserialization capabilities for C# applications, focusing on functional programming principles and type safety.

## Features

### 1. Enhanced JSON Serializer Options

The `EnhancedJsonSerializerOptions` class provides pre-configured JSON serialization options with sensible defaults:

```csharp
// Default options with all converters
var options = EnhancedJsonSerializerOptions.DefaultOptions;

// Default options without converters
var basicOptions = EnhancedJsonSerializerOptions.DefaultOptionsWithoutConverters;

// Options with only DateTime converter
var dateTimeOptions = EnhancedJsonSerializerOptions.DefaultOptionsWithDateTimeConverter;
```

### 2. Multi-Format DateTime Converter

The `MultiFormatDateTimeConverter` supports parsing multiple date formats:

```csharp
// Using with custom formats
var options = EnhancedJsonSerializerOptions.CreateOptionsWithConverters(
    new MultiFormatDateTimeConverterFactory(
        "dd/MM/yyyy",
        "yyyy-MM-dd"
    )
);

// Example JSON
var json = @"{
    ""date"": ""2024-03-14"",
    ""alternateDate"": ""14/03/2024""
}";

var result = JsonSerializer.Deserialize<DateContainer>(json, options);
```

Supports numerous date formats including:

- ISO 8601
- Common regional formats
- Unix timestamps
- SQL datetime formats
- Week-based formats
- And more...

### 3. Polymorphic JSON Serialization

The `PolymorphicJsonConverter` enables type-safe serialization of abstract classes and interfaces:

```csharp
public interface IAnimal { string Name { get; } }
public class Dog : IAnimal { public string Name { get; set; } }
public class Cat : IAnimal { public string Name { get; set; } }

// Serialization
var animal = new Dog { Name = "Rex" };
string json = JsonSerializer.Serialize<IAnimal>(animal, options);

// Deserialization
IAnimal deserializedAnimal = JsonSerializer.Deserialize<IAnimal>(json, options);
```

### 4. Conditional String Enum Converter

Converts enums to strings based on attributes:

```csharp
[StringEnum]
public enum Status
{
    Active,
    Inactive,
    Pending
}

// Will be serialized as strings: "active", "inactive", "pending"
var data = new { Status = Status.Active };
string json = JsonSerializer.Serialize(data, options);
```

### 5. JSON Extension Methods

Utility methods for common JSON operations:

```csharp
// Convert object to JSON
var myObject = new { Name = "Test" };
string json = myObject.ConvertToJson();

// Convert JSON to object
var obj = json.ConvertFromJson<MyClass>();

// Convert to JsonDocument
var doc = myObject.ConvertToJsonDocument();

// Try get nested properties
var result = doc.TryGetNestedProperty("user", "address", "city");
```

## Best Practices

1. **Default Options**: Use `EnhancedJsonSerializerOptions.DefaultOptions` for most scenarios as it includes all converters.

2. **DateTime Handling**: Use `MultiFormatDateTimeConverter` when dealing with dates from various sources or formats.

3. **Polymorphic Types**: Implement `PolymorphicJsonConverter` when working with inheritance hierarchies or interfaces.

4. **Enum Serialization**: Apply `[StringEnum]` attribute to enums that should be serialized as strings.

5. **Error Handling**: Use the `TryGetProperty` and `TryGetNestedProperty` methods for safe property access.

## Configuration

The default configuration includes:

```csharp
new JsonSerializerOptions(JsonSerializerDefaults.Web)
{
    ReferenceHandler = ReferenceHandler.IgnoreCycles,
    WriteIndented = false,
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
}
```

## Extension Points

You can create custom configurations using:

```csharp
var customOptions = EnhancedJsonSerializerOptions.CreateOptionsWithConverters(
    new YourCustomConverter(),
    new AnotherCustomConverter()
);
```

Or modify existing options:

```csharp
var options = EnhancedJsonSerializerOptions.DefaultOptions.Create(opt => {
    opt.WriteIndented = true;
    opt.PropertyNamingPolicy = null;
});
```
