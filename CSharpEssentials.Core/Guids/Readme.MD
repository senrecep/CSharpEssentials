# Guider - Enhanced GUID Operations for .NET

The `Guider` class provides methods for working with GUIDs (Globally Unique Identifiers) in .NET applications. It uses modern C# features like `Span<T>` and stack allocation to reduce allocations.

## Features

### 1. NewGuid()

Creates an [RFC 9562](https://www.rfc-editor.org/rfc/rfc9562#name-uuid-version-7) version 7 GUID on every target. The first 48 bits are the Unix time in milliseconds, big-endian, followed by the version nibble `7`, the variant bits `10` and 74 random bits. IDs from different milliseconds sort by creation time. IDs created in the same millisecond have no set order.

On .NET 9 and later it calls `Guid.CreateVersion7()`. On `netstandard2.0` and `netstandard2.1` the package builds the same bit layout itself, with the random bits from `RandomNumberGenerator`. A .NET 8 app uses the `netstandard2.1` build, so it gets version 7 too.

```csharp
Guid newId = Guider.NewGuid();
```

The `netstandard` builds read random bytes from a per-thread 4 KiB buffer that is refilled from `RandomNumberGenerator`. Reading the generator once per ID cost about 1 µs on the test machine. With the buffer it costs about 60 ns. The call does not allocate after the buffer exists.

### SQL Server ordering

Version 7 GUIDs are ordered when compared byte by byte from the first byte, which is how `Guid.CompareTo`, PostgreSQL `uuid` and string sorting compare them. SQL Server's `uniqueidentifier` compares the last 6 bytes first ([Comparing GUID and uniqueidentifier values](https://learn.microsoft.com/sql/connect/ado-net/sql/compare-guid-uniqueidentifier-values)). Those bytes are random in a version 7 GUID, so `NewGuid()` does not give sequential index order in a SQL Server `uniqueidentifier` column.

### Upgrading to 8.0

On `netstandard2.0` and `netstandard2.1`, and so on .NET 8 and .NET Framework, `NewGuid()` now returns version 7 GUIDs instead of random version 4 GUIDs. The first 6 bytes show when the ID was created. If an ID must not reveal its creation time, use `Guid.NewGuid()`.

### 2. ToStringFromGuid(Guid)

Converts a GUID to a URL-safe string format. The output is:

- Shorter than standard GUID string (22 characters vs 36)
- URL-safe: RFC 4648 Base64url, so standard Base64 with '+' replaced by '-' and '/' replaced by '\_'. Any Base64url decoder, such as `System.Buffers.Text.Base64Url`, returns the same GUID
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

1. Use `NewGuid()` for time-ordered database keys on every target
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
