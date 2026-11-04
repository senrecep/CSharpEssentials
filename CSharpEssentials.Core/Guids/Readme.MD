# Guider - Enhanced GUID Operations for .NET

The `Guider` class provides methods for working with GUIDs (Globally Unique Identifiers) in .NET applications. It uses modern C# features like `Span<T>` and stack allocation to reduce allocations.

## Features

### 1. NewGuid()

Creates an [RFC 9562](https://www.rfc-editor.org/rfc/rfc9562#name-uuid-version-7) version 7 GUID on every target. The first 48 bits are the Unix time in milliseconds, big-endian, followed by the version nibble `7`, the variant bits `10` and 74 random bits. IDs from different milliseconds sort by creation time, as long as the system clock does not move backwards. IDs created in the same millisecond have no set order.

The package builds this layout itself on every target, .NET 9 and later included, instead of calling `Guid.CreateVersion7()`. A .NET 8 app uses the `netstandard2.1` build, so it gets version 7 too. The random bits come from a per-thread pool of 1024 bytes, so one refill serves 102 IDs. A refill takes 1024 bytes from `RandomNumberGenerator` (OpenSSL on Linux) plus one `Guid.NewGuid()` as a salt, and every ID XORs the salt into the bytes it takes. So every ID also carries fresh bits from the same operating system source that `Guid.NewGuid()` and `Guid.CreateVersion7()` read. `Guid.CreateVersion7()` reads the generator for every ID, which on Linux costs about 0.8 to 1 µs on .NET 9 and .NET 10.

```csharp
Guid newId = Guider.NewGuid();
```

A call does not allocate, apart from the 1024-byte pool on the first call of each thread. See [Cost](#cost) for measured numbers.

#### Cloned processes

Bytes kept in memory are copied when a process is cloned. A cloned virtual machine or a checkpointed and restored process starts with the same unused pool bytes as its siblings, and with the same state of any random generator that runs inside the process, such as OpenSSL's. Three guards keep the clones from returning the same IDs:

- Every refill takes a new salt from `Guid.NewGuid()`, which reads the operating system for each call. Two clones get different IDs from a refill as long as their `Guid.NewGuid()` values differ.
- A thread uses its pool only while it makes IDs at most 1 ms apart. The first ID after a longer pause, or after the clock steps back, takes its bits from `Guid.NewGuid()` and that GUID becomes the new salt for the rest of the pool. A snapshot taken while the thread is idle therefore leaves each clone with its own salt.
- The clock ticks inside the millisecond (100 ns steps) are XORed into the random bits. Two clones that use the same leftover bytes still differ unless they read the clock at the same 100 ns tick.

What remains: if a snapshot is taken less than 1 ms after a thread made an ID, and the restored clock still reads less than 1 ms after that ID, the clones keep the same salt and the same leftover bytes on that thread. Two of them then return the same GUID if they create an ID at the same pool position in the same 100 ns tick. At most 102 IDs per thread come from leftover bytes. After those, IDs are as safe between clones as `Guid.NewGuid()` itself, which depends on the operating system giving each clone fresh randomness.

How likely that is depends on the restored clock. If the clock is resynced before the clones run, it rarely reads within 1 ms of the last ID. A cloned virtual machine whose clock resumes from the snapshot time until it is resynced does read within 1 ms by default. Its clones then run the same code from the same state, so two of them reaching the same pool position in the same 100 ns tick is much likelier than chance. The number of pairs that can collide grows with the square of the number of clones. `Guid.CreateVersion7()` reads the operating system for every ID, so it does not have this pool-specific exposure. Turn the pool off with the switch below for any cloned virtual machine or checkpointed and restored process that serves work before its clock is resynced.

To turn the pool off, set the AppContext switch before the first ID is created, in the project file:

```xml
<ItemGroup>
  <RuntimeHostConfigurationOption Include="CSharpEssentials.Guider.DisableRandomPool" Value="true" />
</ItemGroup>
```

or with `AppContext.SetSwitch("CSharpEssentials.Guider.DisableRandomPool", true)` at startup. Every ID then takes its random bits from `Guid.NewGuid()`, as `Guid.CreateVersion7()` does, and costs about the same. The switch covers `NewMonotonicGuid()` too.

### 2. NewMonotonicGuid()

Creates a version 7 GUID that is strictly greater than every GUID it returned before in the same process (per loaded copy of the assembly), compared as big-endian bytes, with `Guid.CompareTo` or as strings.

```csharp
Guid first = Guider.NewMonotonicGuid();
Guid second = Guider.NewMonotonicGuid();
// second.CompareTo(first) > 0, even inside the same millisecond
```

How it works: the 12 bits after the version (`rand_a` in RFC 9562) hold the time inside the millisecond in steps of 1/4096 ms (RFC 9562 section 6.2, method 3). The first 6 bits of `rand_b` hold a counter (method 1). The timestamp, the fraction and the counter are packed into one number. A new ID starts from the current time with the counter at 0. If that is not greater than the last packed value, because another ID took the same 1/4096 ms step or the clock stalled or went backwards, the ID takes the last value plus one. A counter overflow carries into the fraction, and a fraction overflow into the timestamp. The shared value is updated with `Interlocked.CompareExchange`, so there is no lock. Random bytes come from the same per-thread pool as `NewGuid()`, with the same guards for cloned processes, so the call does not allocate.

What it costs:

- Each ID has 56 random bits instead of 74: the counter takes 6 bits of `rand_b`.
- All threads update one shared value. With 6 threads it cost 30 to 105 ns per ID in a quick benchmark, against 13 to 16 ns for `NewGuid()`, because the threads take turns on that value. The number moves a lot between runs. See [Cost](#cost).
- The order holds within one process, and within one loaded copy of the assembly: two `AssemblyLoadContext`s or .NET Framework AppDomains each keep their own value. Two processes or machines can still produce IDs that interleave.
- The fraction and the counter allow 262,144 IDs per millisecond (64 in each 1/4096 ms step), about 262 per microsecond across all threads. That is far above what one process makes: in a test, 1,000,000 IDs from a tight loop kept the encoded timestamp within 1 ms of the clock. Above that rate the encoded timestamp runs ahead of the clock, by roughly (IDs / 262,144) ms minus the elapsed time, and catches up once the rate drops. A clock that goes backwards keeps the timestamp at the last value until the clock passes it.
- The counter starts at 0 in each step, so it shows how many IDs the process made in that 1/4096 ms.

Use `NewGuid()` when IDs only need to be unique and roughly time ordered. Use `NewMonotonicGuid()` when code relies on the creation order of IDs made by one process, for example a cursor or an append-only log.

### Cost

Time per ID from a quick BenchmarkDotNet ShortRun on a 12-core Linux machine, 1-minute load average 1.8 to 4.4. Every row allocated 0 bytes. The .NET 8 column runs the `netstandard2.1` asset, and .NET 8 has no `Guid.CreateVersion7()`. The 6-thread rows run 6 threads that each make 20,000 IDs.

| Method | .NET 8 | .NET 9 | .NET 10 | .NET 11 |
|---|---:|---:|---:|---:|
| `Guid.NewGuid()` (version 4) | 748 ns | 764 ns | 762 ns | 58 ns |
| `Guid.CreateVersion7()` | n/a | 820 ns | 791 ns | 90 ns |
| `Guider.NewGuid()` | 72 ns | 70 ns | 60 ns | 52 ns |
| `Guider.NewMonotonicGuid()` | 85 ns | 82 ns | 76 ns | 70 ns |
| 6 threads: `Guid.NewGuid()` | 153 ns | 162 ns | 165 ns | 14.2 ns |
| 6 threads: `Guid.CreateVersion7()` | n/a | 164 ns | 158 ns | 18.6 ns |
| 6 threads: `Guider.NewGuid()` | 15.5 ns | 15.6 ns | 13.6 ns | 12.6 ns |
| 6 threads: `Guider.NewMonotonicGuid()` | 105 ns | 30.2 ns | 52.2 ns | 52.2 ns |

On .NET 8, 9 and 10 the BCL reads the operating system once per GUID, so the pool makes an ID 10 to 13 times cheaper on one thread and 10 to 12 times cheaper with 6 threads. On .NET 11 the BCL read is much cheaper, and `Guider.NewGuid()` is still faster than `Guid.CreateVersion7()`. These rows come from tight loops. The first ID after a pause of more than 1 ms takes the `Guid.NewGuid()` path, so a thread that makes a few IDs at a time pays about what `Guid.CreateVersion7()` costs for the first one. In a Stopwatch probe that paused 1.5 ms before each burst, `Guider.NewGuid()` was within about 15% of the BCL for bursts of one ID on .NET 8, 9 and 10 and faster from two IDs on. On .NET 11 it was 25 to 30 ns slower for bursts of one or three IDs, equal at two, and faster from five on.

### SQL Server ordering

Version 7 GUIDs are ordered when compared byte by byte from the first byte, which is how `Guid.CompareTo`, PostgreSQL `uuid` and string sorting compare them. SQL Server's `uniqueidentifier` compares the last 6 bytes first ([Comparing GUID and uniqueidentifier values](https://learn.microsoft.com/sql/connect/ado-net/sql/compare-guid-uniqueidentifier-values)). Those bytes are random in a version 7 GUID, so neither `NewGuid()` nor `NewMonotonicGuid()` gives sequential index order in a SQL Server `uniqueidentifier` column.

### Upgrading to 8.0

On `netstandard2.0` and `netstandard2.1`, and so on .NET 8 and .NET Framework, `NewGuid()` now returns version 7 GUIDs instead of random version 4 GUIDs. The first 6 bytes show when the ID was created. If an ID must not reveal its creation time, use `Guid.NewGuid()`.

On every target `NewGuid()` now builds the GUID from a per-thread pool of random bytes instead of calling `Guid.CreateVersion7()` on .NET 9 and later. If the app runs in a cloned virtual machine or a checkpointed and restored process, read [Cloned processes](#cloned-processes); the `CSharpEssentials.Guider.DisableRandomPool` switch restores one OS read per ID.

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
