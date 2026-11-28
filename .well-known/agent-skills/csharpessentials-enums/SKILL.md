---
name: csharpessentials-enums
description: Use when an enum crosses a boundary (JSON, ASP.NET Core binding, OpenAPI, EF Core, HTTP) and needs one string spelling, or for reflection-free enum helpers. Covers [StringEnum], [EnumAlias], [EnumFallback], EnumNaming, the generated {Enum}Extensions (ToWireName, TryParseWire, ParseWire, IsDefined, ToDescription, Names, Values), EnumConventions, EnumReadMode, EnumValueParser, EnumValueFormatter, EnumMetadata, flags, safe renames and analyzers CSE0002-CSE0016.
---

# CSharpEssentials.Enums

```bash
dotnet add package CSharpEssentials.Enums   # also comes with CSharpEssentials.Json, .AspNetCore, .EntityFrameworkCore, .Http
```

```csharp
using CSharpEssentials.Enums;                      // attributes, EnumConventions, EnumMetadata, parser, formatter
using System.Text.Json.Serialization;              // [JsonStringEnumMemberName]
using System.ComponentModel;                       // [Description]
```

The generated `{Enum}Extensions` class is in the enum's own namespace; no extra `using` is needed there.

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials.Enums
using CSharpEssentials.Enums;

PaymentState state = PaymentState.AwaitingCapture;
string wire = state.ToWireName();                                  // awaiting_capture
Console.WriteLine(wire);

if (PaymentStateExtensions.TryParseWire("AWAITING_CAPTURE", out PaymentState parsed))
    Console.WriteLine(parsed);                                     // AwaitingCapture

Console.WriteLine(PaymentStateExtensions.TryParseWire("auth", out parsed) ? parsed.ToString() : "no match"); // Authorized (alias)
Console.WriteLine(PaymentStateExtensions.TryParseWire("refunded", out _));                        // False

bool ok = EnumValueParser.TryParse("refunded", EnumReadMode.Data, EnumConventions.Default, out PaymentState fromDb);
Console.WriteLine($"{ok} {fromDb}");                               // True Unknown

[StringEnum]
public enum PaymentState
{
    AwaitingCapture,
    [EnumAlias("auth")] Authorized,
    Captured,
    [EnumFallback] Unknown,
}
```

## Declaring an enum

```csharp
[StringEnum]                                        // required: turns on the generator
public enum OrderStatus
{
    Pending,                                        // "pending"
    [EnumAlias("Approval")] PendingApproval,        // "pending_approval"; also reads "Approval"
    [JsonStringEnumMemberName("sent")] Shipped,     // "sent"
    [Description("Unknown or newer value")]
    [EnumFallback] Unknown = 99,                    // Data reads map unknown values here
}
```

- Wire name order: `[JsonStringEnumMemberName]`, `[EnumMember(Value = ...)]`, `[StringEnum(Naming = EnumNaming.X)]`, the `<CSharpEssentialsEnumNaming>` MSBuild property, then `SnakeCaseLower`.
- `EnumNaming`: `Default`, `SnakeCaseLower`, `SnakeCaseUpper`, `KebabCaseLower`, `KebabCaseUpper`, `CamelCase`, `PascalCase`. `HTTPStatus` becomes `http_status`.
- `[StringEnum(Storage = EnumStorage.Integer)]` stores the number in EF Core; give every member an explicit value then (CSE0005).

## Using the generated helpers

| Call | Result |
|---|---|
| `value.ToWireName()` | wire name; throws `EnumValueException` if undefined |
| `{Enum}Extensions.{Member}WireName` | `const string` wire name |
| `{Enum}Extensions.TryParseWire(text, out value)` | `bool`; wire name, member name (any case), alias or defined number |
| `{Enum}Extensions.ParseWire(text)` | value; throws `EnumValueException` |
| `value.IsDefined()` | declared member or valid flag combination |
| `value.ToDescription()` | `[Description]` text or `null` |
| `value.ToOptimizedString()` | member name without reflection |
| `value.AsUnderlyingType()` | the number |
| `{Enum}Extensions.Names`, `.Values` | cached lists in declaration order |
| `{Enum}Extensions.GetNames()`, `.GetValues()` | new arrays |

`TryParseWire`/`ParseWire` are static methods on the extensions class, not on the enum: write `OrderStatusExtensions.TryParseWire(...)`.

## Reading values in generic code

```csharp
bool ok = EnumValueParser.TryParse(text, EnumReadMode.Input, conventions, out TEnum value, out EnumValueError? error);
string name = EnumValueFormatter.Format(value, EnumWireFormat.String);
EnumInfo<TEnum> info = EnumMetadata.Get<TEnum>();          // throws if not registered; TryGet does not
IReadOnlyList<string> allowed = info.WireNames;
```

- `EnumReadMode.Input`: values from callers (body, route, query, header). Strict, rejects unknown values and the fallback member.
- `EnumReadMode.Data`: values from storage or other services. Tolerant; unknown values go to `[EnumFallback]` when `UnknownValue = UseFallback` (default).
- `EnumConventions.Default with { AcceptNumbers = false }` builds a variant. Other switches: `AcceptMemberNames`, `CaseInsensitive`, `UnknownValue`, `WriteAs`, `Storage`, `FlagsStorage`, `CheckConstraints`, `CanHandle`.

## Wiring the layers

| Layer | Call |
|---|---|
| System.Text.Json | `options.AddEnumConventions(EnumConventions.Default, EnumReadMode.Data)` (CSharpEssentials.Json) |
| ASP.NET Core | `services.AddEnumConventions()` and `app.UseEnumBinding()` (CSharpEssentials.AspNetCore) |
| OpenAPI | `o.AddEnumConventions()` (CSharpEssentials.AspNetCore.OpenApi or .Swashbuckle, one per host) |
| EF Core | `configurationBuilder.ConfigureEnumConventions(conventions)` in `ConfigureConventions` (CSharpEssentials.EntityFrameworkCore) |
| Outgoing HTTP | `WithEnumConventions(conventions)` (CSharpEssentials.Http) |

## Flags

```csharp
[StringEnum]
[Flags]
public enum Permissions { None = 0, Read = 1, Write = 2, Delete = 4 }   // explicit values and None = 0
```

`ToWireName()` gives `"read,write"`, but `ParseWire` reads one name or a number (`"3"`) per call. JSON writes flags as arrays; EF Core stores an integer.

## Renaming a member

- Keep the wire name: `[JsonStringEnumMemberName("shipped")] Dispatched,` (empty migration).
- New wire name, old still readable: `[EnumAlias("shipped")] Dispatched,` then rewrite stored rows between `DropCheckConstraint` and `AddCheckConstraint`, or use `ConvertEnumColumn<T>(...)`.

## Rules

- Every enum used in JSON, routes, queries, OpenAPI or EF Core gets `[StringEnum]`. Without it the enum takes the framework path (numbers in JSON).
- Use `ToWireName()`, `TryParseWire`, `ParseWire`. `ToSnakeCase()`, `TryParse` and `Parse` on the extensions class are obsolete and spell some names differently (`HTTPStatus`: `httpstatus` against `http_status`).
- Do not call `Enum.Parse` or `ToString()` for wire values; they ignore aliases and wire names.
- The enum must be public or internal and not nested in a generic or private type, or it gets no metadata (CSE0015) and its converter throws.
- One `[EnumFallback]` member at most, never on `[Flags]` enums.
- Do not write your own `ToDescription(this MyEnum)` in the enum's namespace (CS0121).
- Inside this repository, examples need a `ProjectReference` to `CSharpEssentials.Enums.Generators` with `OutputItemType="Analyzer" ReferenceOutputAssembly="false"`; NuGet consumers do not.

## Diagnostics

| ID | Severity | Rule |
|---|---|---|
| CSE0002 | Error | Two members produce the same wire name (ignoring case) |
| CSE0003 | Error | An alias collides with another member's wire name, member name or alias |
| CSE0004 | Error | More than one `[EnumFallback]` member |
| CSE0005 | Warning | Integer storage and a member without an explicit value (code fix) |
| CSE0006 | Warning | `[Flags]` enum without a zero member (code fix adds `None = 0`) |
| CSE0007 | Warning | `[Flags]` member is neither a single bit nor a combination |
| CSE0008 | Error | `[EnumFallback]` on a `[Flags]` enum |
| CSE0009 | Error | Wire name or alias is empty, has whitespace or a comma, or starts with a digit, `-`, `+` or `.` |
| CSE0010 | Info (off) | Enum without `[StringEnum]` in a public API |
| CSE0012 | Warning | Invalid `CSharpEssentialsEnumNaming` value |
| CSE0013 | Warning | `[EnumAlias]`/`[EnumFallback]` on an enum without `[StringEnum]` |
| CSE0014 | Error | Migration has both `AlterColumn` and `ConvertEnumColumn` for one column |
| CSE0015 | Warning | `[StringEnum]` enum gets no generated metadata |
| CSE0016 | Error | Two enums generate the same extensions class name |
