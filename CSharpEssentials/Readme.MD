# CSharpEssentials

One package reference for the core CSharpEssentials modules: results and errors, `Maybe`, unions, rules, entities, JSON, HTTP, time, cloning, enums and the Core helpers. It also adds three helpers that combine those packages.

## Install

```bash
dotnet add package CSharpEssentials
```

Targets `net11.0`, `net10.0`, `net9.0` and `netstandard2.1`.

## What it includes

| Package | For |
|---|---|
| [CSharpEssentials.Core](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Core/README.MD) | String case conversion, GUIDs, collection and null helpers |
| [CSharpEssentials.Errors](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Errors/Readme.MD) | `Error` and `ErrorType` |
| [CSharpEssentials.Results](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD) | `Result` and `Result<T>` instead of exceptions |
| [CSharpEssentials.Maybe](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Maybe/Readme.MD) | `Maybe<T>` for optional values |
| [CSharpEssentials.Any](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Any/Readme.MD) | `Any<T1, T2, ...>` unions |
| [CSharpEssentials.These](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.These/Readme.MD) | A value, errors, or both |
| [CSharpEssentials.Rules](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Rules/Readme.MD) | Composable business rules |
| [CSharpEssentials.Entity](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Entity/Readme.MD) | Entity base classes and domain events |
| [CSharpEssentials.Json](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Json/Readme.MD) | System.Text.Json presets, enums, polymorphism |
| [CSharpEssentials.Http](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Http/Readme.MD) | `HttpClient` calls that return `Result<T>` |
| [CSharpEssentials.Time](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Time/Readme.MD) | Testable clock and time zone helpers |
| [CSharpEssentials.Clone](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Clone/Readme.MD) | `ICloneable<T>` |
| [CSharpEssentials.Enums](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Enums/Readme.MD) | `[StringEnum]` wire names and generated helpers |

Installed separately: `CSharpEssentials.Validation`, `.Mediator`, `.Resilience`, `.Endpoints`, `.DependencyInjection`, `.EntityFrameworkCore` (and its `.HealthChecks`, `.Hosting`, `.Locking` packages), `.AspNetCore` (with `.AspNetCore.OpenApi` or `.AspNetCore.Swashbuckle`), `.Idempotency`, `.RequestResponseLogging`, `.GcpSecretManager`, and the test helpers `.Results.Testing`, `.Mediator.Testing` and `.Idempotency.Testing`.

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials
using CSharpEssentials.Core;
using CSharpEssentials.Errors;
using CSharpEssentials.Maybe;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Time;

IDateTimeProvider clock = new FakeDateTimeProvider(new DateTimeOffset(2025, 1, 1, 9, 0, 0, TimeSpan.Zero));
var users = new Dictionary<string, string> { ["ada@example.com"] = "Ada" };

Result<string> Greet(string email) =>
    Find(email).ToResult(Error.NotFound("User.NotFound", $"No user with email {email}."));

Maybe<string> Find(string email) =>
    users.TryGetValue(email, out string? name) ? name : Maybe<string>.None;

foreach (string email in new[] { "ada@example.com", "bob@example.com" })
{
    Greet(email).Switch(
        onSuccess: name => Console.WriteLine($"{clock.UtcNow:HH:mm} hello {name}"),
        onError: errors => Console.WriteLine($"{errors[0].Code}: {errors[0].Description}"));
}
// 09:00 hello Ada
// User.NotFound: No user with email bob@example.com.

Console.WriteLine("UserRegistered".ToSnakeCase()); // user_registered
```

## Helpers in the meta-package

The `CSharpEssentials` assembly itself adds three extension methods that need more than one bundled package.

| Method | Namespace | Returns |
|---|---|---|
| `JsonElement.TryGetProperty(params string[] names)` | `CSharpEssentials.Json` | `Result<JsonElement>` for the first name that exists on the element |
| `JsonDocument?.TryGetNestedProperty(params string[] path)` | `CSharpEssentials.Json` | `Result<JsonElement?>` for the value at the path of names |
| `long?.MsToDateTime(DateTime? defaultValue = null)` | `CSharpEssentials.Time` | `Maybe<DateTime>`: the UTC time of the Unix milliseconds, `defaultValue` for `null`, or `None` |

```csharp
using System.Text.Json;
using CSharpEssentials.Json;
using CSharpEssentials.Maybe;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Time;

using JsonDocument doc = JsonDocument.Parse("""{"user":{"name":"Ada"},"userName":"ada1"}""");

Result<JsonElement> handle = doc.RootElement.TryGetProperty("user_name", "userName"); // "ada1": the first name that exists
Result<JsonElement?> name = doc.TryGetNestedProperty("user", "name");                // "Ada": names read as a path
Result<JsonElement> missing = doc.RootElement.TryGetProperty("email");               // failure, code PropertyNotFound

long? timestamp = 1_700_000_000_000;
Maybe<DateTime> created = timestamp.MsToDateTime();                                  // 2023-11-14T22:13:20Z
long? none = null;
Maybe<DateTime> fallback = none.MsToDateTime(DateTime.UnixEpoch);                    // the default you pass
```

- `TryGetProperty` with several names checks each on the same element; it does not walk into nested objects. Use `TryGetNestedProperty` for a path.
- Both return a failed `Result` instead of throwing: `NoPropertyNames` (validation) when you pass no names, `PropertyNotFound` (not found) when a name is missing, and `DocumentIsNull` (validation) for a `null` document or a JSON `null` root.
- The [Examples.Main](https://github.com/senrecep/CSharpEssentials/blob/main/examples/Examples.Main) project runs these calls.

## Upgrading to 8.0

`MsToDateTime` returns a `DateTime` with `Kind = Utc` (it was `Unspecified`, with the same ticks). Wrap the value in `DateTime.SpecifyKind(value, DateTimeKind.Unspecified)` to keep the old behavior. The other 8.0 changes are in the [CSharpEssentials.Core upgrade notes](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Core/README.MD#upgrading-to-80).

## See also

- [Package index skill](https://github.com/senrecep/CSharpEssentials/blob/main/.well-known/agent-skills/csharpessentials-meta/SKILL.md): every package and the problem it solves.
