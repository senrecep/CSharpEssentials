# Guider - Enhanced GUID Operations for .NET

The `Guider` class provides efficient and optimized methods for working with GUIDs (Globally Unique Identifiers) in .NET applications. It offers high-performance operations using modern C# features like `Span<T>` and stack allocation.

## Features

### 1. NewGuid()

Creates a new GUID using .NET's Version 7 algorithm, which is optimized for database performance and sequential generation.

```csharp
Guid newId = Guider.NewGuid();
```

### 2. ToStringFromGuid(Guid)

Converts a GUID to a URL-safe string format. The output is:

- Shorter than standard GUID string (22 characters vs 36)
- URL-safe (uses '-' and '\_' instead of '/' and '+')
- No padding characters
- Memory efficient using stack allocation

```csharp
Guid id = Guider.NewGuid();
string urlSafeString = Guider.ToStringFromGuid(id);
// Example output: "Yx7A9bx0T0y2zKl3mN8pQw"
```

### 3. ToGuidFromString(string)

Converts a URL-safe string back to a GUID. This method:

- Handles the custom URL-safe format
- Uses stack allocation for better performance
- Efficiently processes the input without heap allocations

```csharp
string urlSafeString = "Yx7A9bx0T0y2zKl3mN8pQw";
Guid id = Guider.ToGuidFromString(urlSafeString);
```

## Performance Considerations

- Uses `stackalloc` for efficient memory usage
- Avoids heap allocations where possible
- Utilizes `Span<T>` for high-performance memory operations
- Implements modern Base64 encoding/decoding techniques

## Best Practices

1. Use `NewGuid()` when you need database-friendly sequential GUIDs
2. Use the string conversion methods when you need URL-safe GUID representations
3. The methods are thread-safe and can be used in concurrent operations

## Example Use Cases

```csharp
// Generate a new GUID
Guid newId = Guider.NewGuid();

// Convert to URL-safe string
string urlSafe = Guider.ToStringFromGuid(newId);

// Use in URLs
var url = $"https://api.example.com/resources/{urlSafe}";

// Convert back to GUID
Guid originalId = Guider.ToGuidFromString(urlSafe);
```
