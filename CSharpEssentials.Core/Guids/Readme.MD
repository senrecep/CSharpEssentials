# Guider - Enhanced GUID Operations for .NET

The `Guider` class provides methods for working with GUIDs (Globally Unique Identifiers) in .NET applications. It uses modern C# features like `Span<T>` and stack allocation to reduce allocations.

## Features

### 1. NewGuid()

Creates a new GUID. On .NET 9 and later it uses `Guid.CreateVersion7()`, a time-ordered Version 7 GUID that suits database keys. On `netstandard2.0` and `netstandard2.1` it falls back to `Guid.NewGuid()` (random, Version 4).

```csharp
Guid newId = Guider.NewGuid();
```

### 2. ToStringFromGuid(Guid)

Converts a GUID to a URL-safe string format. The output is:

- Shorter than standard GUID string (22 characters vs 36)
- URL-safe: standard Base64 with '/' replaced by '-' and '+' replaced by '\_' (the reverse of RFC 4648 Base64url, so decode it with `ToGuidFromString`, not a generic Base64url decoder)
- No padding characters
- Memory efficient using stack allocation

```csharp
Guid id = Guider.NewGuid();
string urlSafeString = Guider.ToStringFromGuid(id);
// Example output: "Yx7A9bx0T0y2zKl3mN8pQw"
string sameString = id.ToStringFromGuid();
```

### 3. ToGuidFromString(ReadOnlySpan<char>)

Converts a URL-safe string back to a GUID. This method:

- Handles the custom URL-safe format
- Uses stack allocation for better performance
- Efficiently processes the input without heap allocations
- Accepts a `string` or a `ReadOnlySpan<char>`; `string` and `Guid` also have the extension forms `ToGuidFromString()` and `ToStringFromGuid()`

```csharp
string urlSafeString = "Yx7A9bx0T0y2zKl3mN8pQw";
Guid id = Guider.ToGuidFromString(urlSafeString);
Guid sameId = urlSafeString.ToGuidFromString();
```

The input must be exactly 22 characters from `A-Z`, `a-z`, `0-9`, `-` and `_`. Any other length or character throws `FormatException`; there is no `TryParse` form, so catch `FormatException` when the input is untrusted.

## Performance Considerations

- Uses small fixed-size `stackalloc` buffers for efficient memory usage
- Avoids heap allocations where possible
- Utilizes `Span<T>` for high-performance memory operations

## Best Practices

1. Use `NewGuid()` when you need database-friendly sequential GUIDs (.NET 9+; earlier targets get random GUIDs)
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
