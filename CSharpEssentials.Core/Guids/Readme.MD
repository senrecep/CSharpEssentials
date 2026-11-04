# Guider - Enhanced GUID Operations for .NET

The `Guider` class provides methods for working with GUIDs (Globally Unique Identifiers) in .NET applications. It uses modern C# features like `Span<T>` and stack allocation to reduce allocations.

## Features

### 1. NewGuid()

Creates an [RFC 9562](https://www.rfc-editor.org/rfc/rfc9562#name-uuid-version-7) version 7 GUID on every target. The first 48 bits are the Unix time in milliseconds, big-endian, followed by the version nibble `7`, the variant bits `10` and 74 random bits. IDs from different milliseconds sort by creation time, as long as the system clock does not move backwards. IDs created in the same millisecond have no set order.

On .NET 9 and later it calls `Guid.CreateVersion7()`. On `netstandard2.0` and `netstandard2.1` the package builds the same bit layout itself, with the random bits from `Guid.NewGuid()`, as `Guid.CreateVersion7()` does. A .NET 8 app uses the `netstandard2.1` build, so it gets version 7 too.

```csharp
Guid newId = Guider.NewGuid();
```

Every call reads fresh random bits from the operating system and keeps none in the process, so a process cloned from a snapshot (a VM clone, a restored container or a restored function instance) does not repeat the random bits of its siblings. A call costs about as much as `Guid.NewGuid()`, about 0.8 µs on the test machine, and does not allocate.

### 2. NewMonotonicGuid()

Creates a version 7 GUID that is strictly greater than every GUID it returned before in the same process (per loaded copy of the assembly), compared as big-endian bytes, with `Guid.CompareTo` or as strings.

```csharp
Guid first = Guider.NewMonotonicGuid();
Guid second = Guider.NewMonotonicGuid();
// second.CompareTo(first) > 0, even inside the same millisecond
```

How it works: the 12 bits after the version (`rand_a` in RFC 9562) hold the time inside the millisecond in steps of 1/4096 ms (RFC 9562 section 6.2, method 3). The first 6 bits of `rand_b` hold a counter (method 1). The timestamp, the fraction and the counter are packed into one number. A new ID starts from the current time with the counter at 0. If that is not greater than the last packed value, because another ID took the same 1/4096 ms step or the clock stalled or went backwards, the ID takes the last value plus one. A counter overflow carries into the fraction, and a fraction overflow into the timestamp. The shared value is updated with `Interlocked.CompareExchange`, so there is no lock. Random bytes come from `Guid.NewGuid()` on every target, so the call does not allocate.

What it costs:

- Each ID has 56 random bits instead of 74: the counter takes 6 bits of `rand_b`.
- All threads update one shared value. In a quick benchmark with 6 threads it cost about 11% more per ID than `Guid.NewGuid()` on .NET 9, because the random bits cost far more than the shared update.
- The order holds within one process, and within one loaded copy of the assembly: two `AssemblyLoadContext`s or .NET Framework AppDomains each keep their own value. Two processes or machines can still produce IDs that interleave.
- The fraction and the counter allow 262,144 IDs per millisecond (64 in each 1/4096 ms step), about 262 per microsecond across all threads. That is far above what one process makes: in a test, 1,000,000 IDs from a tight loop kept the encoded timestamp within 1 ms of the clock. Above that rate the encoded timestamp runs ahead of the clock, by roughly (IDs / 262,144) ms minus the elapsed time, and catches up once the rate drops. A clock that goes backwards keeps the timestamp at the last value until the clock passes it.
- The counter starts at 0 in each step, so it shows how many IDs the process made in that 1/4096 ms.

Use `NewGuid()` when IDs only need to be unique and roughly time ordered. Use `NewMonotonicGuid()` when code relies on the creation order of IDs made by one process, for example a cursor or an append-only log.

### SQL Server ordering

Version 7 GUIDs are ordered when compared byte by byte from the first byte, which is how `Guid.CompareTo`, PostgreSQL `uuid` and string sorting compare them. SQL Server's `uniqueidentifier` compares the last 6 bytes first ([Comparing GUID and uniqueidentifier values](https://learn.microsoft.com/sql/connect/ado-net/sql/compare-guid-uniqueidentifier-values)). Those bytes are random in a version 7 GUID, so neither `NewGuid()` nor `NewMonotonicGuid()` gives sequential index order in a SQL Server `uniqueidentifier` column.

### Upgrading to 8.0

On `netstandard2.0` and `netstandard2.1`, and so on .NET 8 and .NET Framework, `NewGuid()` now returns version 7 GUIDs instead of random version 4 GUIDs. The first 6 bytes show when the ID was created. If an ID must not reveal its creation time, use `Guid.NewGuid()`.

### 3. ToStringFromGuid(Guid)

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

### 4. ToGuidFromString(ReadOnlySpan<char>)

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

1. Use `NewGuid()` for time-ordered database keys on every target, and `NewMonotonicGuid()` when IDs from one process must be strictly increasing
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
