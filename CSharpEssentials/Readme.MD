# CSharpEssentials

Meta-package that bundles the core functional modules. It targets net11.0, net10.0, net9.0 and netstandard2.1.

## Included packages

- `CSharpEssentials.Core`
- `CSharpEssentials.Errors`
- `CSharpEssentials.Results`
- `CSharpEssentials.Maybe`
- `CSharpEssentials.Json`
- `CSharpEssentials.Any`
- `CSharpEssentials.These`
- `CSharpEssentials.Rules`
- `CSharpEssentials.Entity`
- `CSharpEssentials.Http`
- `CSharpEssentials.Time`
- `CSharpEssentials.Clone`
- `CSharpEssentials.Enums`

## Installation

```bash
dotnet add package CSharpEssentials
```

## Usage

The other packages are installed separately: `CSharpEssentials.Validation`, `CSharpEssentials.Mediator`, `CSharpEssentials.Resilience`, `CSharpEssentials.Endpoints`, `CSharpEssentials.DependencyInjection`, `CSharpEssentials.EntityFrameworkCore`, `CSharpEssentials.AspNetCore` (with `CSharpEssentials.AspNetCore.OpenApi` or `CSharpEssentials.AspNetCore.Swashbuckle`), `CSharpEssentials.Idempotency` (with `CSharpEssentials.Idempotency.Testing` for store contract tests), `CSharpEssentials.RequestResponseLogging` and `CSharpEssentials.GcpSecretManager`.

```csharp
using CSharpEssentials.Errors;
using CSharpEssentials.Maybe;
using CSharpEssentials.ResultPattern;

Maybe<User> maybe = FindUser(1);
Result<User> result = maybe.ToResult(Error.NotFound("User.Missing", "User not found."));
```

## Extensions in the meta-package

The assembly adds a few helpers that combine the bundled packages:

```csharp
using System.Text.Json;
using CSharpEssentials.Json;          // JsonExtensions
using CSharpEssentials.Maybe;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Time;          // TimeExtensions

using JsonDocument doc = JsonDocument.Parse("""{"a":{"b":5},"userName":"x"}""");

Result<JsonElement> name = doc.RootElement.TryGetProperty("user_name", "userName"); // the first name that exists
Result<JsonElement?> five = doc.TryGetNestedProperty("a", "b");                     // walks the names as a path

long? timestamp = 1_700_000_000_000;
Maybe<DateTime> date = timestamp.MsToDateTime();                                    // None for null, or the default you pass
```

`TryGetProperty` and `TryGetNestedProperty` return a failed `Result` (`PropertyNotFound` as the error code when no name matches, `NoPropertyNames` when you pass none, `DocumentIsNull` for a null document) instead of throwing.

## Upgrading to 8.0

- `MsToDateTime` returns a `DateTime` with `Kind = Utc` (it was `Unspecified`, with the same ticks). Wrap the value in `DateTime.SpecifyKind(value, DateTimeKind.Unspecified)` to keep the old behaviour. The other 8.0 changes are in the [CSharpEssentials.Core upgrade notes](../CSharpEssentials.Core/README.MD#upgrading-to-80).
