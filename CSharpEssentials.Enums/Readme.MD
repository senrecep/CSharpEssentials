# CSharpEssentials.Enums

Gives each enum member one string spelling, its wire name, and uses it in JSON, ASP.NET Core binding, OpenAPI, EF Core and outgoing HTTP. Mark an enum with `[StringEnum]`; a bundled source generator writes its metadata and helper methods at compile time, with no reflection, so it is trim and Native AOT safe.

> Benchmarks against Enums.NET 5.0.0, NetEscapades.EnumGenerators 1.0.0-beta21 (a prerelease; no stable version
> exists) and the BCL are in [BENCHMARKS.md](./BENCHMARKS.md). On net10, `ToOptimizedString` ties NetEscapades (0.5418 ns),
> CSE is the fastest column in three of four parse categories (valid case-sensitive 8.6618 ns against Enums.NET 18.3746 ns)
> and formats flags fastest with the least allocation. CSE loses elsewhere: formatting a boxed value (15.566 ns against
> BCL 5.052 ns), `TryFormatMany`, reflection-built names and values, descriptions through the metadata (3.6495 ns against
> NetEscapades 0.9027 ns), a rejected case-sensitive parse with the error-reporting overload (22.5530 ns / 88 B against
> Enums.NET 13.2294 ns / 0 B), `ToString` of an undefined value, and `GetNames` (184 B where Enums.NET allocates nothing).
> All three libraries publish under Native AOT; CSE and NetEscapades do so with 0 IL warnings, and the CSE binary is
> 1.3 MB against 1.2 MB for NetEscapades. Design-time cost of the source generator is in
> [the generator BENCHMARKS.md](../CSharpEssentials.Enums.Generators/BENCHMARKS.md).

## Install

```bash
dotnet add package CSharpEssentials.Enums
```

The package holds the generator, the analyzers and the code fixes. Every CSharpEssentials package that depends on Enums passes them on, so a project that references `CSharpEssentials.Json`, `.AspNetCore`, `.EntityFrameworkCore`, `.Http` or the meta-package needs no direct reference. `.Core`, `.Clone`, `.Time`, `.DependencyInjection`, `.Endpoints` and `.RequestResponseLogging` do not depend on Enums.

The generator runs in the project that declares the enum. A shared domain library that declares `[StringEnum]` enums needs `CSharpEssentials.Enums` (or a package that depends on it); projects that only use those enums need it only if they declare enums of their own.

Targets `net11.0`, `net10.0`, `net9.0`, `netstandard2.1` and `netstandard2.0`. Metadata and the wire-name helpers need C# 9 or later.

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials.Enums
using System.ComponentModel;
using System.Text.Json.Serialization;
using CSharpEssentials.Enums;

OrderStatus status = OrderStatus.PendingApproval;
Console.WriteLine(status.ToWireName());                         // pending_approval
Console.WriteLine(OrderStatus.Shipped.ToWireName());            // sent
Console.WriteLine(status.ToDescription());                      // Waiting for a manager

Console.WriteLine(OrderStatusExtensions.TryParseWire("Approval", out OrderStatus parsed)); // True
Console.WriteLine(parsed);                                      // PendingApproval
Console.WriteLine(OrderStatusExtensions.ParseWire("SENT"));     // Shipped

try
{
    OrderStatusExtensions.ParseWire("gone");
}
catch (EnumValueException ex)
{
    Console.WriteLine(ex.Message); // 'gone' is not a valid OrderStatus. Allowed values: pending, pending_approval, sent, unknown.
}

[StringEnum]
public enum OrderStatus
{
    Pending,
    [EnumAlias("Approval")][Description("Waiting for a manager")] PendingApproval,
    [JsonStringEnumMemberName("sent")] Shipped,
    [EnumFallback] Unknown = 99,
}
```

## Declaring an enum

| Attribute | On | Effect |
|---|---|---|
| `[StringEnum]` | enum | Opts in: the generator writes metadata and `{Enum}Extensions` |
| `[StringEnum(Naming = EnumNaming.KebabCaseLower)]` | enum | Wire name style for this enum |
| `[StringEnum(Storage = EnumStorage.Integer)]` | enum | EF Core stores the number instead of the wire name |
| `[JsonStringEnumMemberName("x")]` or `[EnumMember(Value = "x")]` | member | Sets the wire name of one member |
| `[EnumAlias("a", "b")]` | member | Extra spellings that are read but never written |
| `[EnumFallback]` | member | The member that data reads map unknown values to. Request input never accepts it |
| `[Description("...")]` | member | Returned by `ToDescription()` and shown in the OpenAPI value table |

Nested enums work. The class for `Order.State` is `Order_StateExtensions`.

### Wire names

A member's wire name comes from the first of these that is set:

1. `[JsonStringEnumMemberName("...")]` on the member
2. `[EnumMember(Value = "...")]` on the member
3. `[StringEnum(Naming = ...)]` on the enum
4. The `CSharpEssentialsEnumNaming` MSBuild property of the project
5. `SnakeCaseLower`

| `EnumNaming` | `PendingApproval` becomes |
|---|---|
| `SnakeCaseLower` (default) | `pending_approval` |
| `SnakeCaseUpper` | `PENDING_APPROVAL` |
| `KebabCaseLower` | `pending-approval` |
| `KebabCaseUpper` | `PENDING-APPROVAL` |
| `CamelCase` | `pendingApproval` |
| `PascalCase` | `PendingApproval` |

`EnumNaming.Default` means "use the project setting". The separator styles follow `System.Text.Json.JsonNamingPolicy`, so `HTTPStatus` is `http_status`. To set the style for a whole project:

```xml
<PropertyGroup>
  <CSharpEssentialsEnumNaming>KebabCaseLower</CSharpEssentialsEnumNaming>
</PropertyGroup>
```

## Generated helpers

For `OrderStatus` the generator writes `public static class OrderStatusExtensions` in the enum's namespace.

```csharp Program.cs
// dotnet add package CSharpEssentials.Enums
using CSharpEssentials.Enums;

var tier = PlanTier.ProPlus;

Console.WriteLine(tier.ToWireName());                 // pro-plus
Console.WriteLine(PlanTierExtensions.ProPlusWireName); // pro-plus (const)
Console.WriteLine(tier.ToOptimizedString());          // ProPlus
Console.WriteLine(tier.ToKebabCase());                // pro-plus
Console.WriteLine(tier.ToLowerCase());                // proplus
Console.WriteLine(tier.ToUpperCase());                // PROPLUS
Console.WriteLine(tier.AsUnderlyingType());           // 2
Console.WriteLine(tier.IsDefined());                  // True
Console.WriteLine(((PlanTier)42).IsDefined());        // False
Console.WriteLine(tier.ToDescription() ?? "null");    // null: no [Description]

Console.WriteLine(PlanTierExtensions.IsDefined("Pro"));       // True: member names
Console.WriteLine(string.Join(",", PlanTierExtensions.Names)); // Free,Pro,ProPlus
Console.WriteLine(PlanTierExtensions.Values.Count);            // 3
Console.WriteLine(PlanTierExtensions.GetNames().Length);       // 3, a new array per call
Console.WriteLine(PlanTierExtensions.TryParseWire("2", out PlanTier byNumber) && byNumber == PlanTier.ProPlus); // True

[StringEnum(Naming = EnumNaming.KebabCaseLower)]
public enum PlanTier { Free, Pro, ProPlus }
```

| Member | Behavior |
|---|---|
| `value.ToWireName()` | The wire name. Throws `EnumValueException` for an undefined value. `[Flags]` values give comma-separated wire names of the single flags |
| `{Member}WireName` | `const string` of each wire name |
| `TryParseWire(text, out value)`, `ParseWire(text)` | Read one value in any known spelling: wire name, member name in any casing, alias, or the number of a defined member (for flags, any defined combination). Never map to the fallback member; `ParseWire` throws `EnumValueException` |
| `value.IsDefined()` | A declared member, or for `[Flags]` a combination of declared flags |
| `value.ToDescription()` | The member's `[Description]` text, or `null` (also for undefined values and flag combinations) |
| `value.ToOptimizedString()` | The member name, like `ToString()` without reflection |
| `value.ToKebabCase()`, `ToLowerCase()`, `ToUpperCase()` | The member name in that style |
| `value.AsUnderlyingType()` | The number, typed as the underlying type |
| `IsDefined(string name)` | Whether a member has this exact C# name |
| `Names`, `Values` | Cached `IReadOnlyList` of member names and values, in declaration order |
| `GetNames()`, `GetValues()` | The same as new arrays |
| `{Member}SnakeCase`, `{Member}KebabCase` | Legacy 4.x spelling constants |
| `ToSnakeCase()`, `TryParse`, `Parse` | Obsolete: use `ToWireName()`, `TryParseWire`, `ParseWire` |

- `ToSnakeCase()` keeps its 4.x output (`HTTPStatus` gives `httpstatus`), while `ToWireName()` gives `http_status`. The old spelling is read as an extra alias, so stored values still load.
- Because `ToDescription()` lives in the enum's namespace, it wins over a generic `ToDescription(this Enum)` helper there, and a `ToDescription(this YourEnum)` of your own in the same namespace becomes ambiguous (CS0121). Rename or remove your own extension.

## Runtime API

`EnumMetadata`, `EnumValueParser` and `EnumValueFormatter` work with any registered enum, which helps in generic code.

```csharp Program.cs
// dotnet add package CSharpEssentials.Enums
using CSharpEssentials.Enums;

EnumInfo<Priority> info = EnumMetadata.Get<Priority>();
Console.WriteLine(string.Join(",", info.WireNames));   // low,high,unknown
Console.WriteLine(info.Fallback!.MemberName);          // Unknown
Console.WriteLine(EnumMetadata.IsRegistered(typeof(Priority))); // True

bool ok = EnumValueParser.TryParse("HIGH", EnumReadMode.Input, EnumConventions.Default, out Priority p, out EnumValueError? error);
Console.WriteLine($"{ok} {p}");                         // True High

ok = EnumValueParser.TryParse("urgent", EnumReadMode.Input, EnumConventions.Default, out p, out error);
Console.WriteLine($"{ok} {error!.Message}");           // False 'urgent' is not a valid Priority. Allowed values: low, high.

ok = EnumValueParser.TryParse("urgent", EnumReadMode.Data, EnumConventions.Default, out p);
Console.WriteLine($"{ok} {p}");                         // True Unknown

EnumConventions strict = EnumConventions.Default with { AcceptNumbers = false };
Console.WriteLine(EnumValueParser.TryParse("1", EnumReadMode.Input, strict, out p));  // False

Console.WriteLine(EnumValueFormatter.Format(Priority.High, EnumWireFormat.String)); // high
Console.WriteLine(EnumValueFormatter.Format(Priority.High, EnumWireFormat.Number)); // 1

[StringEnum]
public enum Priority { Low, High, [EnumFallback] Unknown }
```

### Read modes

| Mode | For | Behavior |
|---|---|---|
| `EnumReadMode.Input` | Values a caller sends: body, route, query, header, form | Strict. The switches of `EnumConventions` apply, an unknown value fails, and the fallback member is rejected |
| `EnumReadMode.Data` | Values that were valid once: database rows, JSON columns, other services' responses, messages | Tolerant. Every known spelling is accepted, and an unknown value follows `UnknownValue` |

A failure gives an `EnumValueError` (enum type, value, allowed wire names, path). `EnumValueException` and, in CSharpEssentials.Json, `EnumValueJsonException` carry it. In `Input` errors, the allowed values leave out the fallback member.

### EnumConventions

One `EnumConventions` record drives every layer. Derive variants with `with`.

| Property | Default | Meaning |
|---|---|---|
| `AcceptNumbers` | `true` | Input accepts the number of a defined member |
| `AcceptMemberNames` | `true` | Input accepts the C# member name |
| `CaseInsensitive` | `true` | Input ignores case |
| `UnknownValue` | `UseFallback` | Data reads map unknown values to the `[EnumFallback]` member; `Reject` fails instead |
| `WriteAs` | `String` | Output format: `EnumWireFormat.String` or `Number` |
| `Storage` | `String` | EF Core column shape: `EnumStorage.String` or `Integer` |
| `FlagsStorage` | `Integer` | EF Core column shape of `[Flags]` enums |
| `CheckConstraints` | `true` | EF Core adds a check constraint per enum column |
| `CanHandle` | enums with metadata, and `[StringEnum]` enums without it | Which enums the conventions apply to. A `[StringEnum]` enum without metadata is included on purpose, so it fails loudly (CSE0015) instead of writing numbers |

Other members: `EnumInfo<TEnum>` has `TypedMembers`, `Members`, `Fallback`, `WireNames`, `IsFlags`, `DefinedMask`, `Storage`, `TryGetMember`, `IsDefined`, `TryParse`, `TryParseWire`, `TryParseNumber`, `Format`, `FormatFlags` and `CreateError`. Each `EnumMemberInfo<TEnum>` has `Value`, `MemberName`, `WireName`, `Aliases`, `Description`, `IsObsolete`, `IsFallback`, `RawValue` and `NumericText`. `EnumMetadata.TryGet` looks up metadata without throwing, and `EnumMetadata.GetOrCreateWithReflection(type, naming)` builds metadata for an enum without `[StringEnum]` (not trim or AOT safe). `EnumMetadata.Register(info)` registers metadata built in code; `RegisterRange` is what the generated module initializer calls.

For code that holds only an `object` or a `Type`:

- `EnumValueFormatter.Format(object value, conventions, format = null)` formats a boxed enum and throws `ArgumentException` when the type is not handled. `TryFormat(value, conventions, out text, format)` returns `false` instead. `TryFormatMany(value, conventions, list, format)` adds one entry per value of an enum, a `[Flags]` value or a collection of enums, for repeated query keys.
- `EnumValueParser.TryParseNumber<TEnum>(number, mode, conventions, out value, out error)` reads a `long` or `ulong`.
- `IEnumInfo.Accept(IEnumInfoVisitor<TResult>)` calls back with the typed `EnumInfo<TEnum>`, so generic code needs no `MakeGenericType`.

## Across layers

Register the same conventions in each layer and every layer reads and writes the same names.

| Layer | Package | Call |
|---|---|---|
| JSON | `CSharpEssentials.Json` | `options.AddEnumConventions(conventions, EnumReadMode.Data)` |
| ASP.NET Core | `CSharpEssentials.AspNetCore` | `services.AddEnumConventions()` and `app.UseEnumBinding()` |
| OpenAPI | `CSharpEssentials.AspNetCore.OpenApi` or `.Swashbuckle` (one per host) | `o.AddEnumConventions()` |
| EF Core | `CSharpEssentials.EntityFrameworkCore` | `configurationBuilder.ConfigureEnumConventions(conventions)` |
| Outgoing HTTP | `CSharpEssentials.Http` | `WithEnumConventions(conventions)` |

The [end-to-end example](https://github.com/senrecep/CSharpEssentials/blob/main/examples/Examples.Enums.EndToEnd) runs one `[StringEnum]` and one `[Flags]` enum through JSON bodies, route, query and header binding, two API versions, OpenAPI and a PostgreSQL column. The full rule matrix is in the [enum conventions design](https://github.com/senrecep/CSharpEssentials/blob/main/docs/design/CSharpEssentials.Enums-DESIGN.md).

## Flags

```csharp Program.cs
// dotnet add package CSharpEssentials.Enums
using CSharpEssentials.Enums;

Permissions granted = Permissions.Read | Permissions.Write;
Console.WriteLine(granted.ToWireName());                                      // read,write
Console.WriteLine(PermissionsExtensions.ParseWire("5"));                      // Read, Delete: a number covers a combination
Console.WriteLine(PermissionsExtensions.TryParseWire("read,write", out _));    // False: one name per call
Console.WriteLine(((Permissions)8).IsDefined());                              // False: no member has bit 8

var names = new List<string>();
EnumValueFormatter.FormatFlags(granted, names);
Console.WriteLine(names.Count);                                               // 2

[StringEnum]
[Flags]
public enum Permissions { None = 0, Read = 1, Write = 2, Delete = 4 }
```

- `ToWireName()` joins the single flags with commas, but `TryParseWire` and `ParseWire` read one name or a number per call. To read `"read,write"`, split it and combine the parsed values. JSON and query binding do this for you.
- A `[Flags]` enum is a JSON array (`["read","write"]`), repeated or comma-separated query values, and an integer bitmask in the database (`FlagsStorage = Integer`).
- Give every member an explicit value and add `None = 0`. CSE0005, CSE0006 and CSE0007 check this. `[EnumFallback]` is not allowed on flags (CSE0008), because unknown bits are always rejected.
- Use flags for a small fixed set of independent switches (permissions, feature toggles). Use `List<TEnum>` for a user-chosen multi-select, a set that grows often or past 64 members, or members that carry data.

## Renaming a member safely

A wire name is a contract. It is in JSON payloads, URLs, API clients and database rows. Changing it (renaming a member, changing `Naming` or `[JsonStringEnumMemberName]`) also changes the EF Core check constraint, so `dotnet ef migrations add` emits `DropCheckConstraint` and `AddCheckConstraint`. Rows that still hold the old name make `AddCheckConstraint` fail.

Option 1, for public APIs: rename only the C# member and pin the old wire name.

```csharp
[JsonStringEnumMemberName("shipped")] Dispatched, // was Shipped; the wire name stays "shipped"
```

Nothing changes on the wire or in the database, and the next migration is empty.

Option 2: change the wire name, keep the old one readable, and rewrite stored data.

```csharp
[EnumAlias("shipped")] Dispatched, // writes "dispatched", still reads "shipped"
```

Edit the generated migration so the data is rewritten between the two constraint operations:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropCheckConstraint(name: "ck_orders_Status_enum", table: "orders");
    migrationBuilder.Sql("""UPDATE orders SET "Status" = 'dispatched' WHERE "Status" = 'shipped';""");
    migrationBuilder.AddCheckConstraint(name: "ck_orders_Status_enum", table: "orders", sql: "..."); // as generated
}
```

`ConvertEnumColumn<OrderStatus>("orders", "Status", from: EnumStoredAs.Text, to: EnumStorage.String)` does this rewrite for every non-canonical spelling. Run `EnumDataAudit.Sql<OrderStatus>(...)` first to find values that would fail the constraint (see [EF Core](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore/Readme.MD#converting-the-data)). Keep the alias while old clients or stored payloads may still send `shipped`.

Other services that do not know `dispatched` yet reject it, unless their copy of the enum has an `[EnumFallback]` member and they read in `Data` mode. Ship the new name to consumers before producers write it.

## Diagnostics

| ID | Severity | Meaning | Fix |
|---|---|---|---|
| CSE0002 | Error | Two members produce the same wire name, ignoring case (`HTTPStatus` and `HttpStatus`) | Rename one or set `[JsonStringEnumMemberName]` |
| CSE0003 | Error | An alias equals the wire name, member name or alias of another member | Remove or change the alias |
| CSE0004 | Error | More than one `[EnumFallback]` member | Keep one |
| CSE0005 | Warning | Integer storage (`Storage = EnumStorage.Integer`, or `[Flags]` by default) and a member without an explicit value | Code fix adds explicit values |
| CSE0006 | Warning | `[Flags]` enum without a zero member | Code fix adds `None = 0` |
| CSE0007 | Warning | `[Flags]` member that is neither a single bit nor a combination of other members | Use a power of two or a combination |
| CSE0008 | Error | `[EnumFallback]` on a `[Flags]` enum | Remove it |
| CSE0009 | Error | Wire name or alias is empty, has whitespace or a comma, or starts with a digit, `-`, `+` or `.` | Choose another name |
| CSE0010 | Info, off by default | Enum without `[StringEnum]` used by a public property or a parameter of a public member | Add `[StringEnum]`; enable the rule in `.editorconfig` |
| CSE0012 | Warning | Invalid `CSharpEssentialsEnumNaming` value; `SnakeCaseLower` is used | Use an `EnumNaming` member name |
| CSE0013 | Warning | `[EnumAlias]` or `[EnumFallback]` on an enum without `[StringEnum]`, so it is ignored | Add `[StringEnum]` or remove the attribute |
| CSE0014 | Error | A migration has both `AlterColumn` and `ConvertEnumColumn` for one column | Delete the generated `AlterColumn` |
| CSE0015 | Warning | `[StringEnum]` enum gets no metadata: private or protected nested, nested in a generic type, or below C# 9. Its converter throws | Make it public or internal, not nested in a generic type; use C# 9+ |
| CSE0016 | Error | Two enums generate the same extensions class name (`Order.State` and `Order_State`) | Rename one |

With CSE0002, CSE0003, CSE0004, CSE0008 or CSE0009 the generator writes no metadata for that enum. CSE0001 was retired in 5.0 and CSE0011 is unused.

## AOT and trimming

- The `[StringEnum]` path uses no reflection. JSON and EF Core converters, model binders and OpenAPI schemas get typed instances from the generated metadata. `CSharpEssentials.Enums` and `CSharpEssentials.Json` set `IsAotCompatible` for net8.0 and later.
- The reflection opt-ins carry `[RequiresUnreferencedCode]` and `[RequiresDynamicCode]`: `EnumMetadata.GetOrCreateWithReflection`, `AddEnumConventionsWithReflection` (JSON and ASP.NET Core), `EnumConverterFactory.CreateWithReflectionFallback` and `ConfigureEnumConventionsWithReflection` (EF Core). The default path never calls them.
- A `[StringEnum]` enum without metadata fails at build time (CSE0015) and when its converter is created. It never falls back to numbers silently.
- `IEnumInfo.WireNames` and `EnumValueError.AllowedValues` are internal read-only list types, not `string[]`. A source-generated `JsonSerializerContext` cannot register them, so copy with `[.. error.AllowedValues]` before putting them in an `object` slot such as error metadata.

## Inside this repository

Samples and tests here use `ProjectReference`s, which do not carry a referenced project's generator. They reference `CSharpEssentials.Enums.Generators` with `OutputItemType="Analyzer" ReferenceOutputAssembly="false"` themselves, and the tests import `build/CSharpEssentials.Enums.props` for `CSharpEssentialsEnumNaming`. A project that uses the NuGet packages needs none of this.

## Upgrading

- From 4.x: see [Migrating from 4.x to 5.0](https://github.com/senrecep/CSharpEssentials/blob/main/docs/migration/v4-to-v5.md#naming-and-generated-helpers-csharpessentialsenums) for the obsolete helpers, `ToSnakeCase()` against `ToWireName()` and the database rollout.
- To 8.0: the legacy spelling (`{Member}SnakeCase`, `{Member}KebabCase`, `ToKebabCase()`, the obsolete `ToSnakeCase()` and the legacy alias that `TryParseWire` reads) keeps letters without case, titlecase letters and combining marks. A member named `用户Name` was `name` and is now `用户_name`. Latin-only names keep their values. Rewrite stored values of the old spelling for such members.

## See also

- [Agent skill](https://github.com/senrecep/CSharpEssentials/blob/main/.well-known/agent-skills/csharpessentials-enums/SKILL.md), [example project](https://github.com/senrecep/CSharpEssentials/blob/main/examples/Examples.Enums) and [end-to-end example](https://github.com/senrecep/CSharpEssentials/blob/main/examples/Examples.Enums.EndToEnd).
- [CSharpEssentials.Json](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Json/Readme.MD) for the JSON converter.
