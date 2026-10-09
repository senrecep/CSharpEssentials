# CSharpEssentials.Enums

One spelling per enum member across JSON, ASP.NET Core binding, OpenAPI, EF Core and outgoing HTTP. Mark an enum with `[StringEnum]` and a bundled source generator writes its metadata and helpers at compile time: no reflection, trim and NativeAOT safe.

## Installation

```bash
dotnet add package CSharpEssentials.Enums
```

The package contains the source generator, the analyzers and the code fixes. Any CSharpEssentials package brings the generator and analyzers: every package that depends on Enums, directly or through another CSharpEssentials package, passes them on. A project that references one of them gets `[StringEnum]` metadata, the CSE analyzers and the `CSharpEssentialsEnumNaming` property without a direct `CSharpEssentials.Enums` reference. Reference `CSharpEssentials.Enums` directly only when the project uses no other CSharpEssentials package that depends on it (`.Core`, `.Clone`, `.Time`, `.DependencyInjection`, `.Endpoints` and `.RequestResponseLogging` do not depend on it).

| Scenario | Packages |
|---|---|
| Enums only (generated helpers, `EnumMetadata`) | `CSharpEssentials.Enums` |
| System.Text.Json (workers, message buses, clients) | `CSharpEssentials.Json` |
| ASP.NET Core API (JSON, binding, responses) | `CSharpEssentials.AspNetCore` |
| OpenAPI document | add `CSharpEssentials.AspNetCore.OpenApi` (net10.0) **or** `CSharpEssentials.AspNetCore.Swashbuckle`, never both |
| EF Core storage and migrations | `CSharpEssentials.EntityFrameworkCore` |
| Outgoing HTTP | `CSharpEssentials.Http` |
| Enums declared in a shared domain library | `CSharpEssentials.Enums` (or one of the packages above) in the library; consumers need it only if they declare enums too |

Inside this repository the samples and tests use `ProjectReference`s; a `ProjectReference` does not carry the referenced project's own generator output, but analyzers from that project's NuGet packages do flow. The samples and tests therefore reference `CSharpEssentials.Enums.Generators` with `OutputItemType="Analyzer"` themselves (the tests also import `build/CSharpEssentials.Enums.props` for `CSharpEssentialsEnumNaming`).

Targets `net11.0`, `net10.0`, `net9.0`, `netstandard2.1` and `netstandard2.0`. Registration needs C# 9 or later.

## Declare an enum

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
- `[EnumFallback]` marks the member that data reads (database rows, responses of other services, message payloads) map unknown values to. Request input never uses it: an unknown request value is a 400.
- `[Description]` texts appear in the OpenAPI value table.

### Wire names

The wire name is the one string every layer writes. It is resolved in this order:

1. `[JsonStringEnumMemberName("...")]` on the member
2. `[EnumMember(Value = "...")]` on the member
3. `[StringEnum(Naming = EnumNaming.KebabCaseLower)]` on the enum
4. the `CSharpEssentialsEnumNaming` MSBuild property of the project
5. `SnakeCaseLower`

`EnumNaming` values: `Default` (use the project setting), `SnakeCaseLower` (`pending_approval`), `SnakeCaseUpper` (`PENDING_APPROVAL`), `KebabCaseLower` (`pending-approval`), `KebabCaseUpper` (`PENDING-APPROVAL`), `CamelCase` (`pendingApproval`), `PascalCase` (`PendingApproval`). The separators follow `System.Text.Json.JsonNamingPolicy`, so `HTTPStatus` is `http_status`.

```xml
<PropertyGroup>
  <CSharpEssentialsEnumNaming>KebabCaseLower</CSharpEssentialsEnumNaming>
</PropertyGroup>
```

## Generated helpers

For `OrderStatus` the generator writes `OrderStatusExtensions`:

```csharp
OrderStatus status = OrderStatus.PendingApproval;

status.ToWireName();                                // "pending_approval"
OrderStatusExtensions.PendingApprovalWireName;      // "pending_approval" (const)
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

- `ToWireName()` throws `EnumValueException` for an undefined value. `[Flags]` values are formatted as comma separated wire names of single flags.
- `TryParseWire`/`ParseWire` accept every known spelling (wire name, member name in any casing, alias, the number of a defined member). They never map an unknown value to the fallback member: `ParseWire("gone")` throws, while `ParseWire("unknown")` returns `Unknown` because that is the member's own wire name.
- `{Member}SnakeCase` and `{Member}KebabCase` constants keep their 4.x values.
- `ToSnakeCase()`, `TryParse` and `Parse` are obsolete; use `ToWireName()`, `TryParseWire` and `ParseWire`. `ToSnakeCase()` keeps its 4.x output (`HTTPStatus` → `httpstatus`), while `ToWireName()` returns `http_status`. That old spelling is read as an extra alias, so stored values still load.

## Conventions and runtime API

One `EnumConventions` instance drives every layer. `EnumConventions.Default` is:

| Property | Default | Meaning |
|---|---|---|
| `AcceptNumbers` | `true` | Input accepts the number of a defined member |
| `AcceptMemberNames` | `true` | Input accepts the C# member name |
| `CaseInsensitive` | `true` | Input ignores case |
| `UnknownValue` | `UseFallback` | Data reads map unknown values to the `[EnumFallback]` member (`Reject` to fail instead) |
| `WriteAs` | `String` | Output format (`EnumWireFormat.String` or `Number`) |
| `Storage` | `String` | EF Core column shape (`EnumStorage.String` or `Integer`) |
| `FlagsStorage` | `Integer` | EF Core column shape of `[Flags]` enums |
| `CheckConstraints` | `true` | EF Core adds `ck_{table}_{column}_enum` |
| `CanHandle` | enums with generated metadata, and `[StringEnum]` enums without it | Which enums the conventions apply to. A `[StringEnum]` enum without metadata is selected on purpose, so it fails loudly (CSE0015) instead of silently writing numbers |

It is a record, so derive variants with `with`: `EnumConventions.Default with { AcceptNumbers = false }`.

```csharp
using CSharpEssentials.Enums;

// Parse and format without the generated class (for example in generic code)
bool ok = EnumValueParser.TryParse("approval", EnumReadMode.Input, EnumConventions.Default,
    out OrderStatus value, out EnumValueError? error);
string text = EnumValueFormatter.Format(OrderStatus.Shipped, EnumWireFormat.String); // "sent"

// Metadata
EnumInfo<OrderStatus> info = EnumMetadata.Get<OrderStatus>();
IReadOnlyList<string> wireNames = info.WireNames; // ["pending", "pending_approval", "sent", "unknown"]
bool registered = EnumMetadata.IsRegistered(typeof(OrderStatus));
```

`EnumReadMode.Input` is the strict mode for values a caller sends you. `EnumReadMode.Data` accepts every known spelling and applies `UnknownValue`. On failure `EnumValueError` carries the enum type, the value, the allowed wire names and the path; `EnumValueException` wraps it.

Wire it into each layer:

| Layer | Package | Call |
|---|---|---|
| JSON | `CSharpEssentials.Json` | `options.AddEnumConventions(conventions, EnumReadMode.Data)` |
| ASP.NET Core | `CSharpEssentials.AspNetCore` | `services.AddEnumConventions()` + `app.UseEnumBinding()` |
| OpenAPI | `CSharpEssentials.AspNetCore.OpenApi` / `.Swashbuckle` | `o.AddEnumConventions()` |
| EF Core | `CSharpEssentials.EntityFrameworkCore` | `configurationBuilder.ConfigureEnumConventions(conventions)` |
| Outgoing HTTP | `CSharpEssentials.Http` | `WithEnumConventions(conventions)` |

See [Enums end to end](../README.MD#enums-end-to-end) and the [end-to-end sample](../examples/Examples.Enums.EndToEnd/README.md).

## Flags

```csharp
[StringEnum]
[Flags]
public enum Permissions
{
    None = 0,
    Read = 1,
    Write = 2,
    Delete = 4,
}
```

`[Flags]` enums are JSON arrays (`["read","write"]`), repeated or comma separated query values, and an integer bitmask in the database (`FlagsStorage = Integer`). Give every member an explicit value and a `None = 0` member. The analyzers check this: CSE0005 (no explicit values with integer storage), CSE0006 (no zero member) and CSE0007 (a member that is neither a single bit nor a combination of other members). `[EnumFallback]` is not allowed on flags (CSE0008), because unknown bits are always rejected.

Use flags for a fixed, small set of independent on/off capabilities (permissions, feature toggles, days of week). Use `List<TEnum>` for a user-chosen multi-select, a set that grows often or past 64 members, or when a member needs data attached. Use a single enum for one value at a time (statuses, types).

## Renaming a member safely

A wire name is a contract: it is in JSON payloads, URLs, OpenAPI clients and database rows. Changing it (renaming a member, changing `Naming` or `[JsonStringEnumMemberName]`) also changes the check constraint, so `dotnet ef migrations add` emits `DropCheckConstraint` + `AddCheckConstraint` and nothing else. Rows that still hold the old name make `AddCheckConstraint` fail.

**Option 1: rename the C# member only (recommended for public APIs).** Pin the old wire name, which has the highest priority:

```csharp
[JsonStringEnumMemberName("shipped")] Dispatched, // was Shipped; the wire name stays "shipped"
```

Nothing changes on the wire or in the database, and the next migration is empty.

**Option 2: change the wire name.** Keep the old spelling readable and rewrite the stored data:

```csharp
[EnumAlias("shipped")] Dispatched, // writes "dispatched", still reads "shipped"
```

Every layer now writes `dispatched` and still reads `shipped` (input and data). Edit the generated migration so the data is rewritten between the two constraint operations:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropCheckConstraint(name: "ck_orders_Status_enum", table: "orders");
    migrationBuilder.Sql("""UPDATE orders SET "Status" = 'dispatched' WHERE "Status" = 'shipped';""");
    migrationBuilder.AddCheckConstraint(name: "ck_orders_Status_enum", table: "orders", sql: "..."); // as generated
}
```

`ConvertEnumColumn<OrderStatus>("orders", "Status", from: EnumStoredAs.Text, to: EnumStorage.String)` does the same rewrite for every non-canonical spelling (aliases, member names, other casings). Run `EnumDataAudit.Sql<OrderStatus>(...)` (see [EF Core](../CSharpEssentials.EntityFrameworkCore/Readme.MD#converting-the-data)) before deploying to check that no other value would fail the constraint. Keep the alias as long as old clients or stored payloads (JSON columns, queues) may still send `shipped`.

Other services that read your output and do not know `dispatched` yet reject it, unless their copy of the enum has an `[EnumFallback]` member and they read in `Data` mode: then they map it to the fallback member. Ship the new name to consumers before producers start writing it.

## Diagnostics

| ID | Severity | Meaning | Fix |
|---|---|---|---|
| CSE0001 | | Retired in 5.0 (nested enums are supported); the ID stays reserved | |
| CSE0002 | Error | Two members produce the same wire name, compared case-insensitively (`HTTPStatus` and `HttpStatus` → `http_status`) | Rename one or set `[JsonStringEnumMemberName]` |
| CSE0003 | Error | An alias equals the wire name, member name or alias of another member (case-insensitive) | Remove or change the alias |
| CSE0004 | Error | More than one `[EnumFallback]` member | Keep one |
| CSE0005 | Warning | Integer storage (`Storage = EnumStorage.Integer`, or `[Flags]` with default storage) and a member without an explicit value: reordering changes stored data | Code fix: add explicit values |
| CSE0006 | Warning | `[Flags]` enum without a zero member | Code fix: add `None = 0` |
| CSE0007 | Warning | `[Flags]` member that is neither a single bit nor a combination of other members | Use a power of two or a combination |
| CSE0008 | Error | `[EnumFallback]` on a `[Flags]` enum | Remove it; unknown bits are always rejected |
| CSE0009 | Error | Wire name or alias is empty, contains whitespace or a comma, or starts with a digit, `-`, `+` or `.` | Choose another name |
| CSE0010 | Info, off by default | Enum without `[StringEnum]` used as the type of a public property or of a parameter of a public member (takes the framework or reflection path) | Add `[StringEnum]`; enable the rule in `.editorconfig` |
| CSE0012 | Warning | Invalid `CSharpEssentialsEnumNaming` value; `SnakeCaseLower` is used | Use an `EnumNaming` member name |
| CSE0013 | Warning | `[EnumAlias]` or `[EnumFallback]` on an enum without `[StringEnum]` (ignored) | Add `[StringEnum]` or remove the attribute |
| CSE0014 | Error | A migration has both `AlterColumn` and `ConvertEnumColumn` for the same column | Delete the generated `AlterColumn` |
| CSE0015 | Warning | `[StringEnum]` enum without generated metadata: private or protected nested, nested in a generic type, or below C# 9. Its converter throws | Make it public or internal and not nested in a generic type; use C# 9+ |
| CSE0016 | Error | Two enums map to the same extensions class name (`Order.State` and `Order_State`) | Rename one |

CSE0011 is not used (folded into CSE0015). When CSE0002, CSE0003, CSE0004, CSE0008 or CSE0009 applies, the generator writes no metadata for that enum.

## AOT and trimming

- The `[StringEnum]` path uses no reflection. JSON converters, EF Core converters, model binders and OpenAPI schemas get typed instances from the generated metadata. `CSharpEssentials.Enums` and `CSharpEssentials.Json` set `IsAotCompatible` on net8.0 and later, so the trim analyzer checks them.
- The reflection opt-ins are annotated `[RequiresUnreferencedCode]` and `[RequiresDynamicCode]`: `EnumMetadata.GetOrCreateWithReflection`, `AddEnumConventionsWithReflection` (JSON and ASP.NET Core), `EnumConverterFactory.CreateWithReflectionFallback` and `ConfigureEnumConventionsWithReflection` (EF Core). The default path never calls them.
- A `[StringEnum]` enum without metadata fails loudly (CSE0015 at build time, an exception when its converter is created). It never falls back to numbers silently.
- In AOT apps serialize with a source-generated `JsonSerializerContext`; the `ConvertToJson`/`ConvertFromJson` helpers and `EnhancedJsonSerializerOptions` of `CSharpEssentials.Json` use reflection-based serialization.

## Glossary

| Term | Meaning |
|---|---|
| Wire name | The canonical string of a member, written by every layer |
| Alias | An extra spelling (`[EnumAlias]`), read but never written |
| Input | A value a caller sends: body, route, query, header, form. Strict |
| Data read | A value from something that was valid once: database rows, JSON columns, other services' responses, messages. Tolerant, may map to the fallback |
| Output | Anything this service writes. Always the wire name (or the number) |
| Defined | A declared member or, for `[Flags]`, a combination of declared flags |
| Drained | Every instance of the previous version stopped, so no old code reads or writes during a rollout step |

Full vocabulary and the normative matrix: [enum conventions design](../docs/design/CSharpEssentials.Enums-DESIGN.md#2-vocabulary).

## Migrating from 4.x

See [Migrating from 4.x to 5.0](../docs/migration/v4-to-v5.md#naming-and-generated-helpers-csharpessentialsenums): obsolete helpers, the `ToSnakeCase()` vs `ToWireName()` difference and the database rollout.
