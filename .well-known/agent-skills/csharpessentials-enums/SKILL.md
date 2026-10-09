---
name: csharpessentials-enums
description: Use when an enum must have one string spelling across JSON, ASP.NET Core binding, OpenAPI, EF Core and outgoing HTTP, or when you need reflection-free enum helpers. Covers [StringEnum], [EnumAlias], [EnumFallback], wire-name naming, the generated ToWireName/TryParseWire/ParseWire/IsDefined/GetNames/GetValues helpers, EnumConventions, EnumValueParser/EnumValueFormatter/EnumMetadata, flags, renaming members safely, AOT/trimming and the CSE0002-CSE0016 analyzers.
---

# CSharpEssentials.Enums

One spelling per enum member across JSON, ASP.NET Core binding, OpenAPI, EF Core and outgoing HTTP. Mark an enum with `[StringEnum]`; a bundled source generator writes its metadata and helpers at compile time: no reflection, trim and NativeAOT safe.

## Installation

```bash
dotnet add package CSharpEssentials.Enums
```

The package contains the source generator, the analyzers and the code fixes. Any CSharpEssentials package that depends on Enums (directly or through another CSharpEssentials package) passes them on, so a project that references one of them gets `[StringEnum]` metadata and the CSE analyzers without a direct `CSharpEssentials.Enums` reference. `.Core`, `.Clone`, `.Time`, `.DependencyInjection`, `.Endpoints` and `.RequestResponseLogging` do not depend on Enums.

| Scenario | Packages |
|---|---|
| Enums only (generated helpers, `EnumMetadata`) | `CSharpEssentials.Enums` |
| System.Text.Json | `CSharpEssentials.Json` |
| ASP.NET Core API (JSON, binding, responses) | `CSharpEssentials.AspNetCore` |
| OpenAPI document | add `CSharpEssentials.AspNetCore.OpenApi` (net10.0) **or** `CSharpEssentials.AspNetCore.Swashbuckle`, never both |
| EF Core storage and migrations | `CSharpEssentials.EntityFrameworkCore` |
| Outgoing HTTP | `CSharpEssentials.Http` |

Targets `net11.0`, `net10.0`, `net9.0`, `netstandard2.1` and `netstandard2.0`. Registration needs C# 9 or later.

## Namespace

```csharp
using CSharpEssentials.Enums;
```

---

## Declare an Enum

```csharp
using System.ComponentModel;
using System.Text.Json.Serialization;
using CSharpEssentials.Enums;

[StringEnum]
[Description("Order lifecycle state.")]
public enum OrderStatus
{
    Pending,                                   // "pending"
    [EnumAlias("Approval")] PendingApproval,   // "pending_approval", also reads "Approval"
    [JsonStringEnumMemberName("sent")] Shipped, // "sent"
    [EnumFallback] Unknown = 99,               // what tolerant readers map unknown values to
}
```

- `[StringEnum]` opts the enum in. Nested enums are supported (the class for `Order.State` is `Order_StateExtensions`).
- `[EnumAlias(...)]` adds spellings that are read everywhere but never written.
- `[EnumFallback]` marks the member that data reads (database rows, other services' responses, messages) map unknown values to. Request input never uses it: an unknown request value is a 400.
- `[Description]` texts appear in the OpenAPI value table.

### Wire Names

Resolved in this order: `[JsonStringEnumMemberName]` on the member, `[EnumMember(Value = "...")]` on the member, `[StringEnum(Naming = ...)]` on the enum, the `CSharpEssentialsEnumNaming` MSBuild property, `SnakeCaseLower`.

`EnumNaming`: `Default` (use the project setting), `SnakeCaseLower` (`pending_approval`), `SnakeCaseUpper`, `KebabCaseLower` (`pending-approval`), `KebabCaseUpper`, `CamelCase` (`pendingApproval`), `PascalCase`. Separators follow `JsonNamingPolicy`, so `HTTPStatus` is `http_status`.

```xml
<PropertyGroup>
  <CSharpEssentialsEnumNaming>KebabCaseLower</CSharpEssentialsEnumNaming>
</PropertyGroup>
```

---

## Generated Helpers

For `OrderStatus` the generator writes `OrderStatusExtensions`:

```csharp
OrderStatus status = OrderStatus.PendingApproval;

status.ToWireName();                                // "pending_approval"
OrderStatusExtensions.PendingApprovalWireName;      // const "pending_approval"
status.IsDefined();                                 // true (flags: a combination of defined flags)

OrderStatusExtensions.TryParseWire("Approval", out OrderStatus parsed); // true, PendingApproval
OrderStatusExtensions.ParseWire("pending_approval");                     // PendingApproval
OrderStatusExtensions.ParseWire("gone");                                 // throws EnumValueException

status.ToOptimizedString();                         // "PendingApproval"
status.ToKebabCase();                               // "pending-approval"
status.ToLowerCase();                               // "pendingapproval"
status.ToUpperCase();                               // "PENDINGAPPROVAL"
status.AsUnderlyingType();                          // 1
OrderStatusExtensions.IsDefined("PendingApproval"); // true (member names)
OrderStatusExtensions.GetNames();                   // member names
OrderStatusExtensions.GetValues();                  // all members
```

- `ToWireName()` throws `EnumValueException` for an undefined value. `[Flags]` values are written as comma separated wire names of single flags.
- `TryParseWire`/`ParseWire` accept every known spelling (wire name, member name in any casing, alias, the number of a defined member). They never map an unknown value to the fallback member.
- `ToSnakeCase()`, `TryParse` and `Parse` are `[Obsolete]`: use `ToWireName()`, `TryParseWire` and `ParseWire`. `ToSnakeCase()` keeps its 4.x output (`HTTPStatus` → `httpstatus`); `ToWireName()` returns `http_status`.
- The `{Member}SnakeCase` and `{Member}KebabCase` constants keep their 4.x values.

---

## Conventions and Runtime API

One `EnumConventions` record drives every layer; `EnumConventions.Default` accepts numbers, member names and any casing on input, maps unknown data to the `[EnumFallback]` member (`UnknownValue = UseFallback`), writes strings (`WriteAs = String`), stores strings in EF Core (`Storage = String`, flags `Integer`) and adds check constraints. Derive variants with `with`:

```csharp
EnumConventions strict = EnumConventions.Default with { AcceptNumbers = false };

bool ok = EnumValueParser.TryParse("approval", EnumReadMode.Input, EnumConventions.Default,
    out OrderStatus value, out EnumValueError? error);
string text = EnumValueFormatter.Format(OrderStatus.Shipped, EnumWireFormat.String); // "sent"

EnumInfo<OrderStatus> info = EnumMetadata.Get<OrderStatus>();
IReadOnlyList<string> wireNames = info.WireNames;
```

`EnumReadMode.Input` is strict (values a caller sends). `EnumReadMode.Data` accepts every known spelling and applies `UnknownValue`.

| Layer | Package | Call |
|---|---|---|
| JSON | `CSharpEssentials.Json` | `options.AddEnumConventions(conventions, EnumReadMode.Data)` |
| ASP.NET Core | `CSharpEssentials.AspNetCore` | `services.AddEnumConventions()` + `app.UseEnumBinding()` |
| OpenAPI | `CSharpEssentials.AspNetCore.OpenApi` / `.Swashbuckle` | `o.AddEnumConventions()` |
| EF Core | `CSharpEssentials.EntityFrameworkCore` | `configurationBuilder.ConfigureEnumConventions(conventions)` |
| Outgoing HTTP | `CSharpEssentials.Http` | `WithEnumConventions(conventions)` |

---

## Flags

```csharp
[StringEnum]
[Flags]
public enum Permissions { None = 0, Read = 1, Write = 2, Delete = 4 }
```

Flags are JSON arrays (`["read","write"]`), repeated or comma separated query values, and an integer bitmask in the database. Give every member an explicit value and a `None = 0` member (CSE0005, CSE0006, CSE0007). `[EnumFallback]` is not allowed on flags (CSE0008).

---

## Renaming a Member Safely

A wire name is a contract (JSON, URLs, clients, database rows); changing it also changes the EF Core check constraint.

```csharp
// Option 1: rename the C# member (was Shipped), keep the wire name "shipped"
[JsonStringEnumMemberName("shipped")] Dispatched,

// Option 2: new wire name "dispatched", old spelling "shipped" still read
[EnumAlias("shipped")] Dispatched,
```

For a changed wire name, edit the generated migration so the stored data is rewritten between `DropCheckConstraint` and `AddCheckConstraint`, or use `ConvertEnumColumn<OrderStatus>("orders", "Status", from: EnumStoredAs.Text, to: EnumStorage.String)`, and run `EnumDataAudit.Sql<OrderStatus>(...)` before deploying. Ship the new name to consumers before producers write it.

---

## Diagnostics

| ID | Severity | Rule |
|---|---|---|
| CSE0001 | | Retired in 5.0 (nested enums are supported); the ID stays reserved |
| CSE0002 | Error | Two members produce the same wire name (case-insensitive) |
| CSE0003 | Error | An alias equals the wire name, member name or alias of another member |
| CSE0004 | Error | More than one `[EnumFallback]` member |
| CSE0005 | Warning | Integer storage (or `[Flags]` with default storage) and a member without an explicit value. Code fix adds values |
| CSE0006 | Warning | `[Flags]` enum without a zero member. Code fix adds `None = 0` |
| CSE0007 | Warning | `[Flags]` member that is neither a single bit nor a combination of other members |
| CSE0008 | Error | `[EnumFallback]` on a `[Flags]` enum |
| CSE0009 | Error | Wire name or alias is empty, contains whitespace or a comma, or starts with a digit, `-`, `+` or `.` |
| CSE0010 | Info, off by default | Enum without `[StringEnum]` used as a public property type or public member parameter type |
| CSE0012 | Warning | Invalid `CSharpEssentialsEnumNaming` value; `SnakeCaseLower` is used |
| CSE0013 | Warning | `[EnumAlias]` or `[EnumFallback]` on an enum without `[StringEnum]` (ignored) |
| CSE0014 | Error | A migration has both `AlterColumn` and `ConvertEnumColumn` for the same column |
| CSE0015 | Warning | `[StringEnum]` enum without generated metadata (private/protected nested, nested in a generic type, below C# 9); its converter throws |
| CSE0016 | Error | Two enums map to the same extensions class name (`Order.State` and `Order_State`) |

CSE0011 is not used. When CSE0002, CSE0003, CSE0004, CSE0008 or CSE0009 applies, the generator writes no metadata for that enum.

---

## AOT and Trimming

- The `[StringEnum]` path uses no reflection. `CSharpEssentials.Enums` and `CSharpEssentials.Json` set `IsAotCompatible` on net8.0 and later.
- The reflection opt-ins carry `[RequiresUnreferencedCode]` and `[RequiresDynamicCode]`: `EnumMetadata.GetOrCreateWithReflection`, `AddEnumConventionsWithReflection` (JSON and ASP.NET Core), `EnumConverterFactory.CreateWithReflectionFallback` and `ConfigureEnumConventionsWithReflection` (EF Core).
- A `[StringEnum]` enum without metadata fails loudly (CSE0015, an exception when its converter is created); it never falls back to numbers silently.

---

## Migrating from 4.x

`ConditionalStringEnumConverter` is `[Obsolete]`: use `JsonSerializerOptions.AddEnumConventions(...)`. Undefined numbers are now rejected, flags are JSON arrays, and wire names are fixed at build time (no runtime naming policy). See `docs/migration/v4-to-v5.md`.

---

## Best Practices

- Apply `[StringEnum]` to every enum that crosses a boundary (API, database, messages).
- Pin a wire name with `[JsonStringEnumMemberName]` before renaming a member of a public enum.
- Add an `[EnumFallback]` member to enums that newer producers may extend.
- Give `[Flags]` members explicit values and a `None = 0` member.
