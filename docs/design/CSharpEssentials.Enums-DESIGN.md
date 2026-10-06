# CSharpEssentials Enum Conventions: Design Document

> **Date:** 2026-10-06 | **Status:** Proposed design (5.0)
> **Issues:** #59 (epic), #60 (this document), #61 (metadata, conventions, JSON), #62 (ASP.NET Core), #63 (OpenAPI), #64 (EF Core storage), #65 (EF Core migrations), #66 (HTTP clients), #67 (validation, analyzers), #68 (docs, examples, golden tests)
> **Related:** [ADR-007](../adr/ADR-007-enum-conventions.md), [ADR-006](../adr/ADR-006-source-generators-in-separate-projects.md)

---

## 1. Goals and Non-Goals

### Goals
- One enum contract, configured once, applied identically to the database, JSON columns, request and response bodies, route/query/header/form values, outgoing HTTP clients, message bus payloads, validation and the OpenAPI document.
- `snake_case` wire names by default, configurable per project, per enum and per member.
- Undefined values never enter the system. The database enforces the value set with generated check constraints.
- Legacy data (PascalCase, camelCase, integers, numeric text, 3.x snake case, comma separated flags) is readable in every layer, and existing rows can be converted with migration helpers.
- Legacy clients can keep receiving integers per endpoint group.
- Flags and enum collections never hit a column length limit.
- Everything on the generated path is decided at build time: no startup reflection, trim and AOT clean.

### Non-Goals
- Newtonsoft.Json converters. Newtonsoft producers are supported only through tolerant reads on the STJ side (section 14).
- Database enum types (`CREATE TYPE ... AS ENUM`). Check constraints are portable and migration friendly (ADR-007).
- Localized display names. `x-enum-descriptions` carries documentation text, not UI labels.
- Runtime naming policies (ADR-007, decision 3).

## 2. Vocabulary

| Term | Meaning |
|---|---|
| **Wire name** | The canonical string of a member. Written by every layer when the output format is `String`. |
| **Member name** | The C# identifier (`PendingApproval`). |
| **Alias** | A declared extra spelling (`[EnumAlias("Started")]`). Read, never written. |
| **Legacy name** | The 3.x Core `ToSnakeCase()` spelling (`HTTPStatus` → `httpstatus`), emitted automatically when it differs from the wire name. Read, never written. |
| **Input** | A value sent to this service by a caller: request body, route, query, header, form. The strictest read mode. |
| **Data read** | A value this service reads from something it trusts to have been valid once: database rows, JSON columns, responses of other services, message bus payloads. Tolerant, may map to the fallback member. |
| **Output** | Any value this service writes: response bodies, outgoing request bodies and query strings, message payloads, database writes. Always canonical. |
| **Defined** | A value equal to a declared member or, for `[Flags]`, a combination of declared flags (`(value & ~DefinedMask) == 0`). |

## 3. Packages, TFMs and Dependencies

| Project | TFMs | New dependencies | Contents |
|---|---|---|---|
| `CSharpEssentials.Enums` | `net11.0;net10.0;net9.0;netstandard2.1;netstandard2.0` (unchanged) | none | attributes, `EnumInfo<TEnum>`, `EnumMetadata`, `EnumConventions`, parser, formatter, `EnumValueError` |
| `CSharpEssentials.Enums.Generators` (new, not packable) | `netstandard2.0` | Roslyn 4.8 (ADR-006) | `StringEnumGenerator`, analyzers CSE0002 to CSE0014, packed into `CSharpEssentials.Enums` |
| `CSharpEssentials.Enums.CodeFixes` (new, not packable) | `netstandard2.0` | Roslyn 4.8 | code fixes for CSE0005 and CSE0006 |
| `CSharpEssentials.Json` | unchanged | none | `EnumConverterFactory`, `JsonSerializerOptions.AddEnumConventions` |
| `CSharpEssentials.AspNetCore` | unchanged | **removes** `Swashbuckle.AspNetCore` | binding, errors, per-group output format, `AddEnumConventions` |
| `CSharpEssentials.AspNetCore.OpenApi` (new) | `net11.0;net10.0;net9.0` | `Microsoft.AspNetCore.OpenApi` (approval needed) | schema and operation transformers |
| `CSharpEssentials.AspNetCore.Swashbuckle` (new) | `net11.0;net10.0;net9.0;net8.0` | `Swashbuckle.AspNetCore` (moved) | `AddSwagger`, filters, schema id factory, security schemes |
| `CSharpEssentials.EntityFrameworkCore` | unchanged | none | storage conventions, check constraint convention, migration helpers, `EnumDataAudit` |
| `CSharpEssentials.Http` | unchanged | none | query formatting, `AddEnumConventions` for typed clients |
| `CSharpEssentials.Validation` | unchanged | **adds** `CSharpEssentials.Enums` | `IsDefinedEnum`, `IsOneOf`, `HasOnlyDefinedFlags` |

The parser and the formatter live in `CSharpEssentials.Enums` because every other package already depends on it (directly or through `CSharpEssentials.Json`), and because it has no `System.Text.Json` dependency. The generator reads `[JsonStringEnumMemberName]` and `[EnumMember]` by metadata name, so no package reference is needed for that either.

## 4. Configuration Model

### 4.1 Build time: naming

Wire names are decided by the generator. Priority, first match wins:

| # | Source | Scope | Example |
|---|---|---|---|
| 1 | `[JsonStringEnumMemberName("...")]` | member | `[JsonStringEnumMemberName("approved_by_admin")]` |
| 2 | `[EnumMember(Value = "...")]` | member | for contracts shared with WCF/Refit-era code |
| 3 | `[StringEnum(Naming = EnumNaming.X)]` | enum | `[StringEnum(Naming = EnumNaming.KebabCaseLower)]` |
| 4 | `<CSharpEssentialsEnumNaming>X</CSharpEssentialsEnumNaming>` | declaring project | in `Directory.Build.props` |
| 5 | `SnakeCaseLower` | default | |

```csharp
public enum EnumNaming
{
    Default = 0,        // use the project setting (4) or SnakeCaseLower (5)
    SnakeCaseLower,     // pending_approval
    SnakeCaseUpper,     // PENDING_APPROVAL
    KebabCaseLower,     // pending-approval
    KebabCaseUpper,     // PENDING-APPROVAL
    CamelCase,          // pendingApproval
    PascalCase,         // PendingApproval (the member name)
}
```

The MSBuild property reaches the generator through `CompilerVisibleProperty` declared in `buildTransitive/CSharpEssentials.Enums.props` of the package. An invalid value is reported by CSE0012 and falls back to `SnakeCaseLower`.

The separator algorithm is a port of the `System.Text.Json` `JsonSeparatorNamingPolicy` (the same word boundaries for acronyms and digits). A test runs every member name of a fixed corpus through the generator and through `JsonNamingPolicy.SnakeCaseLower/SnakeCaseUpper/KebabCaseLower/KebabCaseUpper/CamelCase` and requires equal output.

### 4.2 Build time: per enum and per member attributes

```csharp
[AttributeUsage(AttributeTargets.Enum)]
public sealed class StringEnumAttribute : Attribute
{
    public EnumNaming Naming { get; set; }            // Default
    public EnumStorage Storage { get; set; }          // Default: EnumConventions decides
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class EnumAliasAttribute(params string[] aliases) : Attribute
{
    public IReadOnlyList<string> Aliases { get; } = aliases;
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class EnumFallbackAttribute : Attribute;
```

`[Description("...")]` (System.ComponentModel) or the XML `<summary>` of the member becomes the description. XML summaries are read only when the declaring project generates documentation (`GenerateDocumentationFile`); otherwise `[Description]` is the only source. `[Obsolete]` members are marked in metadata, still readable, and flagged `deprecated` in OpenAPI.

### 4.3 Runtime: `EnumConventions`

One immutable object, registered once:

```csharp
public sealed record EnumConventions
{
    public static EnumConventions Default { get; } = new();

    // Input (requests): what a caller may send.
    public bool AcceptNumbers { get; init; } = true;        // numbers and numeric strings of defined members
    public bool AcceptMemberNames { get; init; } = true;    // C# member names
    public bool CaseInsensitive { get; init; } = true;      // wire names, member names and aliases

    // Data reads: what to do with a value that is not defined.
    public UnknownEnumValueHandling UnknownValue { get; init; } = UnknownEnumValueHandling.UseFallback;

    // Output.
    public EnumWireFormat WriteAs { get; init; } = EnumWireFormat.String;

    // Storage (EF Core).
    public EnumStorage Storage { get; init; } = EnumStorage.String;
    public EnumStorage FlagsStorage { get; init; } = EnumStorage.Integer;
    public bool CheckConstraints { get; init; } = true;

    // Which enums the conventions apply to. Default: [StringEnum] enums with generated metadata.
    public Func<Type, bool> CanHandle { get; init; } = EnumMetadata.IsRegistered;
}

public enum UnknownEnumValueHandling { Reject, UseFallback }
public enum EnumWireFormat { String, Number }
public enum EnumStorage { Default, String, Integer }
```

Rules for the switches:

- `UseFallback` maps an undefined value to the `[EnumFallback]` member **only when the enum declares one**. Without a fallback member it behaves like `Reject`. `Reject` ignores fallback members everywhere.
- `UnknownValue` applies to **data reads only**. Input is always strict: undefined values and the fallback member itself are rejected (section 5).
- `AcceptNumbers`, `AcceptMemberNames` and `CaseInsensitive` apply to **input only**. Data reads always accept every known spelling of a defined member, because that is how legacy rows and legacy producers stay readable.
- `WriteAs = Number` writes the underlying number. It exists for legacy consumers; it is selected per endpoint group (section 9.4), per HTTP client (section 13), globally for ASP.NET Core output (section 9.4) or per serializer options instance (message producers).

Registration:

```csharp
// Any host
services.AddEnumConventions(o => o with { AcceptNumbers = false });

// Non-DI hosts (MassTransit configuration lambdas, tests, console tools)
JsonSerializerOptions json = new JsonSerializerOptions().AddEnumConventions(EnumConventions.Default);
```

`AddEnumConventions` registers `EnumConventions` as a singleton. Every package resolves it from DI; a package that runs without DI takes an explicit instance. There is no mutable static.

### 4.4 Precedence summary

| Setting | Member | Property | Enum | Endpoint group / client | Project (MSBuild) | `EnumConventions` | Built-in default |
|---|---|---|---|---|---|---|---|
| Wire name | `[JsonStringEnumMemberName]`, `[EnumMember]` | | `[StringEnum(Naming)]` | | `CSharpEssentialsEnumNaming` | | `SnakeCaseLower` |
| Aliases | `[EnumAlias]` | | | | | | legacy 3.x name |
| Fallback | `[EnumFallback]` | | | | | `UnknownValue` | `UseFallback` |
| Output format | | | | action/controller `[EnumWireFormat]` > group `WithEnumWireFormat`, client options | | `WriteAs` | `String` |
| Storage | | `.HasEnumStorage()`, `.HasLegacyEnumStorage()` | `[StringEnum(Storage)]` | | `ConfigureEnumConventions(existingStorage)` (model) | `Storage`, `FlagsStorage` | `String`, flags `Integer` |
| Check constraint | | `.HasEnumCheckConstraint(false)` | | | | `CheckConstraints` | on |

## 5. Normative Matrix

Enum used in every row:

```csharp
[StringEnum]
public enum OrderStatus
{
    Pending = 0,
    [EnumAlias("Approval")]
    PendingApproval = 1,
    [EnumFallback]
    Unknown = 99_999,
}
```

Default conventions. "data reads" covers EF reads, JSON column reads, HTTP client response reads and message consumer reads.

| Input | Input: JSON body | Input: route/query/header/form | Data reads | Result |
|---|---|---|---|---|
| `"pending_approval"` | ok | ok | ok | `PendingApproval` |
| `"PendingApproval"`, `"PENDING_APPROVAL"`, `"pendingApproval"` | ok | ok | ok | `PendingApproval` |
| `"Approval"`, `"approval"` (alias) | ok | ok | ok | `PendingApproval` |
| `1`, `"1"` | ok | ok | ok | `PendingApproval` |
| `1` with `AcceptNumbers = false` | 400 | 400 | ok | data reads always accept defined numbers |
| `"PendingApproval"` with `AcceptMemberNames = false` | 400 | 400 | ok | |
| `"PENDING_APPROVAL"` with `CaseInsensitive = false` | 400 | 400 | ok | |
| `"unknown"`, `99999` (the fallback member) | 400 | 400 | ok | `Unknown` |
| `99`, `"99"`, `"bogus"` | 400 | 400 | `Unknown` (fallback) | without a fallback member: exception, never stored |
| `99` with `UnknownValue = Reject` | 400 | 400 | exception | |
| `""`, `" pending"` | 400 | 400 | exception | no trimming, no empty |
| JSON `null` / missing value | nullable: `null`; non-nullable: STJ rules | nullable: `null`; required: ASP.NET Core required handling | nullable: `null` | |
| `"1.0"`, `"0x1"`, `"+1"`, `"-0"` | 400 | 400 | exception | numbers are `[-]digits` in the invariant culture only |

Output:

| Layer | `WriteAs = String` (default) | `WriteAs = Number` |
|---|---|---|
| JSON | `"pending_approval"` | `1` |
| Dictionary key | `"pending_approval"` | `"1"` |
| Outgoing route/query | `pending_approval` | `1` |
| EF column (`Storage = String`) | `pending_approval` | not affected (storage is separate) |
| EF column (`Storage = Integer`) | `1` | `1` |
| Undefined value | `EnumValueException` before anything is written | same |

The exception rows matter: an undefined value created in code (`(OrderStatus)42`) is never written to a response, a database or a message. Writing it is a bug and fails loudly at the boundary.

### 5.1 Flags

```csharp
[StringEnum, Flags]
public enum Permissions { None = 0, Read = 1, Write = 2, Delete = 4, ReadWrite = Read | Write }
```

| Layer | Wire | Accepted on read |
|---|---|---|
| JSON | `["read","write"]`; `None` → `[]` | arrays of any accepted spelling or number; legacy comma string `"read, write"`; a single number `3` |
| Route/query/form | `?p=read&p=write` | repeated keys, comma separated `?p=read,write`, a single number |
| Header | `read,write` | comma separated |
| EF `Integer` (default) | `3` | integer |
| EF `String` | PostgreSQL `text[]` `{read,write}`; other providers JSON array text `["read","write"]` | also legacy comma text |

Output decomposition: named composite members are **not** used on output (`ReadWrite` is written as `["read","write"]`), so the output does not depend on member declaration order and consumers never have to know composites. Composite names are accepted on input. `WriteAs = Number` writes the integer.

### 5.2 Enum collections

`List<TEnum>`, `TEnum[]`, `IReadOnlyList<TEnum>`, `HashSet<TEnum>` and their nullable element variants (`List<TEnum?>`; `null` elements are kept as JSON `null` and SQL `NULL`):

| Layer | Wire |
|---|---|
| JSON | array of wire names |
| Query/form | repeated keys or comma separated |
| EF PostgreSQL | `text[]` (or `integer[]` for `Integer` storage) with containment check |
| EF other providers | JSON array text (EF Core primitive collection) without a check constraint |

## 6. Generated Code

### 6.1 Metadata

```csharp
public interface IEnumInfo
{
    Type EnumType { get; }
    Type UnderlyingType { get; }
    bool IsFlags { get; }
    ulong DefinedMask { get; }
    EnumStorage Storage { get; }                      // from [StringEnum(Storage)], Default when unset
    IReadOnlyList<IEnumMemberInfo> Members { get; }   // declaration order
    IEnumMemberInfo? Fallback { get; }
    IReadOnlyList<string> WireNames { get; }          // declaration order, without aliases

    // Typed dispatch without reflection: JSON, EF Core and binders get EnumInfo<TEnum> from a Type.
    TResult Accept<TResult>(IEnumInfoVisitor<TResult> visitor);
}

public interface IEnumInfoVisitor<out TResult>
{
    TResult Visit<TEnum>(EnumInfo<TEnum> info) where TEnum : struct, Enum;
}

public interface IEnumMemberInfo
{
    string MemberName { get; }
    string WireName { get; }
    IReadOnlyList<string> Aliases { get; }            // declared aliases, then the legacy name when it differs
    string? Description { get; }
    bool IsObsolete { get; }
    bool IsFallback { get; }
    ulong RawValue { get; }                           // two's complement bits, sign-extended
    string NumericText { get; }                       // derived from RawValue and the underlying type: "-1", "18446744073709551615"
}

public sealed class EnumInfo<TEnum> : IEnumInfo where TEnum : struct, Enum
{
    public IReadOnlyList<EnumMemberInfo<TEnum>> TypedMembers { get; }
    public bool TryGetMember(TEnum value, out EnumMemberInfo<TEnum> member);
    public bool IsDefined(TEnum value);
}

public static class EnumMetadata
{
    public static EnumInfo<TEnum> Get<TEnum>() where TEnum : struct, Enum;   // throws for unregistered enums
    public static bool TryGet<TEnum>(out EnumInfo<TEnum>? info) where TEnum : struct, Enum;
    public static bool TryGet(Type enumType, out IEnumInfo? info);
    public static bool IsRegistered(Type type);

    [RequiresUnreferencedCode("..."), RequiresDynamicCode("...")]
    public static IEnumInfo GetOrCreateWithReflection(Type enumType, EnumNaming naming = EnumNaming.SnakeCaseLower);

    public static void Register<TEnum>(EnumInfo<TEnum> info) where TEnum : struct, Enum;   // called by generated code
}
```

Generic access is a static field read (`EnumInfoCache<TEnum>.Value`). Non-generic access is a dictionary lookup in a copy-on-write dictionary that becomes a `FrozenDictionary` snapshot on net9.0+ after the first read (`CSharpEssentials.Enums` has no net8.0 TFM; net8.0 hosts use the netstandard2.1 build with a plain dictionary).

`IEnumInfo.Accept` is how every package outside `CSharpEssentials.Enums` reaches typed code from a `Type` without `MakeGenericType`: the JSON factory, EF Core converters and the binding plan each implement one visitor. `CSharpEssentials.Enums` therefore stays free of `System.Text.Json`, EF Core and ASP.NET Core references, and a contracts assembly needs only `CSharpEssentials.Enums`.

### 6.2 Registration

The generator emits, per assembly, one `internal static class __CSharpEssentialsEnumRegistry` with a `[ModuleInitializer]` that calls `EnumMetadata.Register(...)` for every `[StringEnum]` enum of the assembly. On `netstandard2.0`/`netstandard2.1` consumers the generator emits an internal `ModuleInitializerAttribute` polyfill when the compilation does not have one. C# 9 is required for registration.

A module initializer runs only when code of the declaring assembly runs; `typeof(OrderStatus)` alone does not trigger it. A contracts assembly that contains only types would otherwise report `IsRegistered = false` and the enum would silently fall back to default handling. Therefore `EnumMetadata.TryGet(Type)` and `EnumInfoCache<TEnum>` call `RuntimeHelpers.RunModuleConstructor(type.Module.ModuleHandle)` on the first miss for a type and retry once (the runtime runs a module constructor at most once, so the call is cheap and AOT safe). A test covers an enum in a separate type-only assembly. On older language versions the generator emits the extension methods without registration and CSE0011 (info) explains that the enum takes the reflection path.

Nested enums are supported in 5.0 (the generated extension class is emitted at namespace level with the containing type names joined, `Order_StateExtensions`), so CSE0001 is retired as obsolete and kept reserved.

### 6.3 Extension methods

| Method | 4.x | 5.0 |
|---|---|---|
| `ToWireName()` | | new: the canonical wire name, throws `EnumValueException` for undefined values |
| `TryParseWire(string?, out T)` | | new: data read rules without fallback (every known spelling, defined numbers) |
| `ParseWire(string?)` | | new |
| `IsDefined(this T)` | | new: flags aware |
| `ToSnakeCase()` | 3.x algorithm | `[Obsolete("Use ToWireName()")]`, unchanged output |
| `ToKebabCase()`, `ToLowerCase()`, `ToUpperCase()`, `ToOptimizedString()` | | unchanged |
| `TryParse(string?, out T)`, `Parse(string?)` | C# names and any int | `[Obsolete("Use TryParseWire/ParseWire")]`, unchanged behavior |
| `IsDefined(string)` | | unchanged |
| `{Member}SnakeCase`, `{Member}KebabCase` constants | | unchanged; new `{Member}WireName` constants |

The obsolete members keep their exact 4.x behavior so upgrading does not silently change results; the compiler warning points to the replacement.

### 6.4 Parser and formatter

```csharp
public enum EnumReadMode { Input, Data }

public static class EnumValueParser
{
    public static bool TryParse<TEnum>(ReadOnlySpan<char> text, EnumReadMode mode, EnumConventions conventions,
        out TEnum value, out EnumValueError? error) where TEnum : struct, Enum;

    public static bool TryParseNumber<TEnum>(long number, EnumReadMode mode, EnumConventions conventions,
        out TEnum value, out EnumValueError? error) where TEnum : struct, Enum;

    public static bool TryParseNumber<TEnum>(ulong number, EnumReadMode mode, EnumConventions conventions,
        out TEnum value, out EnumValueError? error) where TEnum : struct, Enum;
}

public static class EnumValueFormatter
{
    public static string Format<TEnum>(TEnum value, EnumWireFormat format) where TEnum : struct, Enum;
    public static void FormatFlags<TEnum>(TEnum value, IList<string> names) where TEnum : struct, Enum;

    // Non-generic entry points for HTTP client adapters (section 13.1) and other code that only has object + Type.
    // Stable public API. Flags and collections are not handled here: adapters expand them through FormatFlags/TryFormatMany.
    public static string Format(object value, EnumConventions conventions, EnumWireFormat? format = null);
    public static bool TryFormat(object? value, EnumConventions conventions, [NotNullWhen(true)] out string? text, EnumWireFormat? format = null);
    public static bool TryFormatMany(object? value, EnumConventions conventions, IList<string> values, EnumWireFormat? format = null); // flags value or IEnumerable of enums
}

public sealed record EnumValueError(Type EnumType, string? Value, IReadOnlyList<string> AllowedValues, string? Path)
{
    // "'bogus' is not a valid OrderStatus. Allowed values: pending, pending_approval."
    public string Message { get; }
}

public sealed class EnumValueException(EnumValueError error) : Exception(error.Message);
```

Lookup order for a single token (first match wins, so an alias can never shadow a wire name):

1. Exact wire name (ordinal).
2. Wire name, case-insensitive (input: only when `CaseInsensitive`).
3. Member name (input: only when `AcceptMemberNames`), case-insensitive when allowed.
4. Declared alias, then legacy name, with the same case rule.
5. Number: `-?[0-9]+` within the underlying type's range (input: only when `AcceptNumbers`), then the defined check.

Then: fallback member rejected in `Input` mode; undefined values go to the fallback member in `Data` mode when `UnknownValue = UseFallback` and a fallback exists; otherwise the read fails with `EnumValueError` listing the wire names (fallback member excluded from the list for input).

The lookup tables are built once per enum from metadata: a `FrozenDictionary<string, ulong>` with `StringComparer.Ordinal` and one with `StringComparer.OrdinalIgnoreCase` (net9.0+), plain `Dictionary` on netstandard. On net9.0+ the JSON converter copies the token into a stack buffer of chars (`Utf8JsonReader.CopyString(Span<char>)`, which also unescapes) and looks it up through `GetAlternateLookup<ReadOnlySpan<char>>`, so a valid value costs no string allocation. Longer tokens than the longest accepted spelling are rejected before the copy.

## 7. JSON (`CSharpEssentials.Json`, #61)

- `EnumConverterFactory` replaces the `JsonStringEnumConverter` wrapper. `ConditionalStringEnumConverter` stays as an obsolete subclass that maps its constructor arguments to `EnumConventions` (`allowIntegerValues` → `AcceptNumbers`, `canConvert` → `CanHandle`; `namingPolicy` other than null or `SnakeCaseLower` throws `NotSupportedException` with a pointer to section 4.1).
- Handles `TEnum`, `TEnum?`, dictionary keys (`ReadAsPropertyName`/`WriteAsPropertyName`), flags as arrays, enum collections through the normal collection converters.
- `JsonSerializerOptions AddEnumConventions(this JsonSerializerOptions options, EnumConventions conventions, EnumReadMode mode = EnumReadMode.Data, EnumWireFormat? writeAs = null)`. It inserts the factory at position 0 and removes `JsonStringEnumConverter`/`ConditionalStringEnumConverter` instances, so a Refit or MassTransit default cannot win by order.
- Source generated contexts: the factory works with `JsonSerializerContext` because it is added to `JsonSerializerOptions.Converters` (converters take precedence over generated metadata). Contract: no `MakeGenericType` on the generated path. The factory creates `EnumConverter<TEnum>` through an `IEnumInfoVisitor<JsonConverter>` (section 6.1), so no reflection is needed even for the non-generic `CreateConverter(Type)` call. A test pins that a converter in `Options.Converters` wins over source generated metadata for enum properties, including contexts with `UseStringEnumConverter = true`.
- Errors: `EnumValueJsonException : JsonException` carrying `EnumValueError`. ASP.NET Core maps it to the same validation problem as binding errors (section 9.3).
- `EnhancedJsonSerializerOptions.DefaultOptions` uses `AddEnumConventions(EnumConventions.Default)`.

## 8. Shared Error Shape

Every input rejection produces one `EnumValueError`. Every layer turns it into its own error type without changing the content:

| Layer | Error |
|---|---|
| JSON body | `EnumValueJsonException` → 400 validation problem, key = JSON path (`$.items[0].status` normalized to `items[0].status`) |
| Route/query/header/form | 400 validation problem, key = parameter name |
| Validation rules | `Error.Validation(code: "enum.invalid", description: error.Message)` |
| Data reads | `EnumValueException` with table/column or JSON path added to the message by EF Core |

The 400 body is the existing ProblemDetails format of `CSharpEssentials.AspNetCore` with one error per key:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "status": [ "'bogus' is not a valid OrderStatus. Allowed values: pending, pending_approval." ]
  }
}
```

`EnumBindingOptions.ErrorFactory` moves to `services.AddEnumConventions(...).ConfigureErrors(Func<EnumValueError, string, Error>)` (the string is the key) so the code and message can be localized once for body and binding.

## 9. ASP.NET Core (`CSharpEssentials.AspNetCore` + `CSharpEssentials.Endpoints`, #62)

### 9.1 Registration

```csharp
builder.Services.AddEnumConventions();   // EnumConventions, JSON options (Input mode), errors, numeric output options
app.UseEnumBinding();                     // route/query/header/form normalization, after routing
```

`AddEnumConventions` configures `Microsoft.AspNetCore.Http.Json.JsonOptions` and `Microsoft.AspNetCore.Mvc.JsonOptions` with `AddEnumConventions(conventions, EnumReadMode.Input)` and registers the error mapping and the numeric output options of section 9.4.

### 9.2 Binding

Minimal APIs bind enums with `Enum.TryParse` inside `RequestDelegateFactory` and offer no global binder hook, and a startup filter runs before routing, so it cannot see the endpoint. 5.0 keeps the 4.1 mechanism, which already works for minimal APIs (including `[AsParameters]`) and MVC on every TFM: the `UseEnumBinding()` middleware. It stays public and is **not** obsolete; only `AddEnumBinding`/`EnumBindingOptions` are obsolete forwarders to `AddEnumConventions`.

Changes in 5.0:
- Reads `EnumConventions` (Input mode) and metadata instead of its own options; the per-endpoint plan is built once and cached per endpoint as in 4.1.
- Adds header and form sources to the 4.1 query/route plan. Flags and collections accept repeated keys and comma separated values.
- Rewrites accepted spellings to the member name before binding and short-circuits with the 400 of section 8 for rejected values, so `Enum.TryParse` never sees an undefined number.
- `UseEnumBinding()` without `AddEnumConventions()` throws at startup with a message naming the missing call.

`CSharpEssentials.Endpoints` does not get its own mechanism. A per-group `Finally` convention was considered (it does run after the request delegate is created on net8.0+), but two mechanisms would double the test matrix for no behavior difference.

### 9.3 Errors

The `GlobalExceptionHandler` maps `EnumValueJsonException` (and `BadHttpRequestException` whose inner exception is one) to the problem of section 8, so a body error and a query error for the same value look identical.

### 9.4 Legacy numeric output per endpoint group

```csharp
var v1 = app.MapGroup("/api/v1").WithEnumWireFormat(EnumWireFormat.Number);
var v2 = app.MapGroup("/api/v2");                                        // String
```

```csharp
[EnumWireFormat(EnumWireFormat.Number)]       // MVC controller or action
public sealed class LegacyOrdersController : ControllerBase { ... }
```

Global default and overrides (both directions):

```csharp
builder.Services.AddEnumConventions(o => o with { WriteAs = EnumWireFormat.Number });   // every API answers with numbers
var v2 = app.MapGroup("/api/v2").WithEnumWireFormat(EnumWireFormat.String);         // except v2

[EnumWireFormat(EnumWireFormat.String)]                                               // MVC controller or action
public sealed class OrdersV2Controller : ControllerBase { ... }
```

The default output format of ASP.NET Core is `EnumConventions.WriteAs`. Precedence: action attribute > controller attribute > endpoint group > `EnumConventions.WriteAs`. No model convention is needed to apply a format to every controller.

Contract:

- Global `JsonOptions` are never mutated. `AddEnumConventions` builds a second `JsonSerializerOptions` instance once at startup: a copy of the configured options with the format that is not the global default.
- Minimal APIs: an endpoint filter replaces the returned value or `IValueHttpResult` with a JSON result that uses the second options instance and keeps the status code and content type. `Result`/`Result<T>` from `CSharpEssentials.Results` go through the same path after their normal mapping.
- MVC: a result filter assigns a `SystemTextJsonOutputFormatter` built from the second options instance to `ObjectResult.Formatters`.
- Reading is unaffected: a v1 group still accepts names, so new clients can send names to old endpoints.
- Optional selector for apps that cannot version routes (easyapp mobile clients share routes across app versions): `group.WithEnumWireFormat(ctx => ctx.Request.Headers["X-Enum-Format"] == "string" ? EnumWireFormat.String : EnumWireFormat.Number)`. The OpenAPI document of such a group shows the default branch (the value returned for a request without the header) and a description note. A selector group adds `Vary: X-Enum-Format` (the configured header name) to every response so caches keep the two formats apart.
- The endpoint filter unwraps `Results<T1, ...>` through `INestedHttpResult` before looking for `IValueHttpResult`.

### 9.5 Replacing `EnumData<T>`-style holders

The pattern "string route parameter + action filter + scoped holder" is replaced by a typed parameter (`OrderStatus status`). The migration guide shows the before/after.

## 10. OpenAPI (`CSharpEssentials.AspNetCore.OpenApi` and `.Swashbuckle`, #63)

### 10.1 Enum schema

Both packages produce semantically equal JSON (golden test, normalized diff). For `OrderStatus` with `[Description]` texts:

```json
"OrderStatus": {
  "type": "string",
  "description": "Order lifecycle state.\n\n| value | number | description |\n|---|---|---|\n| `pending` | 0 | Created, waiting for payment. |\n| `pending_approval` | 1 | Paid, waiting for manual approval. |\n| `unknown` | 99999 | Response only: a value this API version does not know. |",
  "enum": [ "pending", "pending_approval", "unknown" ],
  "x-enum-varnames": [ "Pending", "PendingApproval", "Unknown" ],
  "x-enum-descriptions": [ "Created, waiting for payment.", "Paid, waiting for manual approval.", "Response only: a value this API version does not know." ],
  "x-enum-numeric-values": [ 0, 1, 99999 ]
}
```

Rules:

- One component schema per enum. `description` keeps the user's text (XML `<summary>` of the enum or `[Description]`) and appends the table. It is never overwritten.
- Aliases are not listed (they are accepted, not advertised). Obsolete members get `(deprecated)` in the table and stay in `enum`.
- The fallback member stays in `enum` (it can appear in responses) and its table row and `x-enum-descriptions` entry start with `Response only:`. Separate request schemas were considered and rejected: they double the component count for a rule that input validation already enforces.
- `default` is the wire name when the property or parameter has a default value.
- Nullable is expressed where it is used, never on the shared component (a nullable component would make every use nullable). The transformer branches on the **document's** OpenAPI version, not on the package version (net10.0+ can still emit 3.0):
  - OpenAPI 3.0: `{ "allOf": [ { "$ref": "#/components/schemas/OrderStatus" } ], "nullable": true }`.
  - OpenAPI 3.1: `{ "oneOf": [ { "$ref": "#/components/schemas/OrderStatus" }, { "type": "null" } ] }`.
- Flags and collections: `type: array`, `items: { $ref: OrderStatus }`, `uniqueItems: true` for flags; query parameters get `style: form`, `explode: true`.
- `EnumWireFormat.Number`: `type: integer`, `format: int32/int64` by underlying type, `enum: [0, 1, 99999]`, `x-enum-varnames`, `x-enum-descriptions`, the same table. The format is decided per document: a document whose endpoints all belong to Number groups describes the enum as integer. A document that mixes formats for the same enum describes the String form and marks every operation of a Number group with `x-enum-wire-format: number` plus a description note. Versioned APIs (one document per version) never hit the mixed case.
- Deferred to 5.1: `EnumSchemaStyle.StringOrNumber` (a `oneOf` string/integer request schema).

### 10.2 Package split

- `CSharpEssentials.AspNetCore.Swashbuckle`: `AddSwagger`, `ConfigureSwaggerOptions`, `EnumSchemaFilter`, `ReApplyOptionalRouteParameterOperationFilter`, `SwashbuckleSchemaIdFactory`, `SecuritySchemes`. Namespaces stay `CSharpEssentials.AspNetCore.Swagger*` so only a package reference changes. Swashbuckle 8.x/9.x (Microsoft.OpenApi 1.x) on net8.0/net9.0, 10.x (Microsoft.OpenApi 2.x) on net10.0+.
- Fixes moved with the code: XML comments are loaded per assembly that declares the type (`type.Assembly.Location` with `File.Exists`), not `GetCallingAssembly`; descriptions are appended, not replaced; the schema follows `EnumConventions` and metadata, no hardcoded policy.
- `CSharpEssentials.AspNetCore.OpenApi`: `services.AddOpenApi(o => o.AddEnumConventions())` registers an `IOpenApiSchemaTransformer` and an `IOpenApiOperationTransformer` (parameters, group format). net9.0 compiles against Microsoft.OpenApi 1.x, net10.0+ against 2.x, with `#if` only inside the two transformer files.
- Scalar UI: no helper in 5.0 (no new dependency). The README shows the two lines for versioned documents.

## 11. EF Core Storage (`CSharpEssentials.EntityFrameworkCore`, #64)

### 11.1 Registration

```csharp
protected override void ConfigureConventions(ModelConfigurationBuilder builder) =>
    builder.ConfigureEnumConventions(EnumConventions.Default);   // or the DI instance

modelBuilder.Entity<Order>().Property(o => o.Status).HasEnumStorage(EnumStorage.Integer);
modelBuilder.Entity<Order>().Property(o => o.Status).HasEnumCheckConstraint(false);
```

```csharp
// Transition: keep writing the format the column holds today (section 12, step 1).
modelBuilder.Entity<Order>().Property(o => o.Status).HasLegacyEnumStorage(EnumStoredAs.MemberName);
```

Upgrading a model whose enum columns are plain integers (no `HasConversion`, EF's default):

```csharp
builder.ConfigureEnumConventions(conventions, existingStorage: EnumStoredAs.Integer);
modelBuilder.Entity<Order>().Property(o => o.Status).HasEnumStorage(EnumStorage.String);   // opt in, one column at a time
```

`existingStorage` applies `HasLegacyEnumStorage(existingStorage)` to every enum property the convention handles that has no property-level `HasEnumStorage`/`HasLegacyEnumStorage`; it wins over `[StringEnum(Storage)]` and `EnumConventions.Storage`. Without it, a `[StringEnum]` enum stored as an integer would get `Storage = String` and EF would generate an `int → text` `AlterColumn` (PostgreSQL's implicit cast writes `'1'`). With it, the first 5.0 migration is empty (section 12.3). New projects omit the argument.

`HasLegacyEnumStorage(EnumStoredAs format)` writes the old format (`Integer`, `MemberName`, `CamelCase`, `LegacySnakeCase`, `FlagsText`), reads every spelling tolerantly, adds **no** check constraint and keeps the current column type. It affects the database only; JSON, OpenAPI and binding still use the wire name. Server-side filters keep matching existing rows because writes and query parameters use the same old format.

Properties that already have a value converter configured by the user (`HasConversion<string>()`, `HasConversion<int>()`, a custom converter) are **skipped** by the convention: no converter replacement, no check constraint, no column change. Upgrading to 5.0 therefore never changes such a column on its own; the team opts in by removing the manual conversion (or replacing it with `HasLegacyEnumStorage`) when it is ready.

`ConfigureEnumConventions` no longer scans assemblies. It adds a model convention (`IPropertyAddedConvention`, `IModelFinalizingConvention`) that recognizes enum properties through `EnumConventions.CanHandle` and metadata, so only enums that are actually mapped are configured. The 4.1 overload with `params Assembly[]` stays as an obsolete forwarder that ignores the assemblies.

### 11.2 Column types

| Storage | PostgreSQL | SQL Server | SQLite | MySQL | Other |
|---|---|---|---|---|---|
| `String` | `text` | `nvarchar(n)` | `TEXT` | `varchar(n)` | `n` |
| `Integer` | underlying integer type | same | `INTEGER` | same | same |
| Flags `Integer` | underlying integer type | same | `INTEGER` | same | same |
| Flags `String` | `text[]` | `nvarchar(max)` JSON array | `TEXT` JSON array | `json` | JSON text |
| Collection | `text[]` / `integer[]` | `nvarchar(max)` JSON | `TEXT` JSON | `json` | JSON text |

`n` is the longest wire name rounded up to the next multiple of 16 (so adding a slightly longer member usually does not alter the column). The provider is detected through `Database.ProviderName` known names; unknown providers get `n`. `.HasMaxLength(...)` set by the user always wins.

### 11.3 Converters

- String storage: `EnumWireNameConverter<TEnum>` (write: `ToWireName`; read: `EnumValueParser` in `Data` mode with the conventions of the model). Unknown value: fallback member, otherwise `EnumValueException` whose message names the entity, the property, the value and the allowed values.
- Integer storage: `EnumIntegerConverter<TEnum>` (write: underlying value after the defined check; read: defined check, fallback, exception).
- Writes of undefined values fail in the converter with `EnumValueException`, before the database reports a constraint violation.
- JSON columns mapped by EF Core (`ToJson()` on owned types, EF 8+, and on complex types, EF 10+) use their own writer, not STJ converters. The convention sets a `JsonValueReaderWriter` (`EnumJsonValueReaderWriter<TEnum>`) on enum properties inside JSON-mapped types. Reads are tolerant (names and the integers EF wrote before). Writes follow the property's storage: `Integer` storage writes numbers, `String` writes wire names, and `HasLegacyEnumStorage` keeps the old format. Because EF Core writes enums in JSON columns as numbers by default, an existing JSON column must be configured with `HasLegacyEnumStorage(EnumStoredAs.Integer)` until `ConvertEnumJsonPath` has run; otherwise new rows get strings next to legacy integers and JSON predicates miss rows. The convention sets the instance, not the type (`HasJsonValueReaderWriterType` would create it through reflection).
- Properties mapped to `jsonb` by a user value converter through `JsonSerializer` use the user's options; the guide shows `AddEnumConventions` there.
- `UseFallback` on reads: an entity loaded with a fallback member and saved again writes the fallback member, overwriting the unknown stored value. The XML doc and the guide state it; teams that cannot accept it set `UnknownValue = Reject` for the model.

### 11.4 Check constraints

An `IModelFinalizingConvention` adds one constraint per enum column, named `ck_{table}_{column}_enum` (truncated with a stable hash suffix to the provider identifier limit, 63 on PostgreSQL). SQL literals go through `ISqlGenerationHelper` and the column type mapping, never string concatenation.

| Case | SQL (PostgreSQL shown) |
|---|---|
| String | `"status" IN ('pending', 'pending_approval', 'unknown')` |
| Integer | `"status" IN (0, 1, 99999)` |
| Flags integer | `("permissions" & ~7) = 0` (7 = defined mask) |
| Flags `text[]`, collection `text[]` | `"tags" <@ ARRAY['a', 'b']::text[]` |
| Collection `integer[]` | `"tags" <@ ARRAY[0, 1]::integer[]` |
| Collection with nullable elements | `array_remove("tags", NULL) <@ ARRAY['a', 'b']::text[]` (`NULL` elements are not contained in any array) |
| Nullable column | the same expression; SQL `NULL IN (...)` is `NULL`, which a check constraint treats as passing |
| JSON column, JSON text collections | no constraint (provider specific JSON validation is out of scope); the converter guards writes |

SQL Server uses `[status] IN (N'pending', ...)` and `([permissions] & ~7) = 0`; SQLite and MySQL use the same `IN` form. Table splitting and TPH: the constraint is added once per table and column; for TPH columns shared by several enum properties (rare), no constraint is added and CSE0xxx is not involved (EF diagnostic log message instead).

Properties with a user value converter or with `HasLegacyEnumStorage` get no constraint (section 11.1).

Adding, removing or renaming a member changes the constraint SQL, so `dotnet ef migrations add` emits `DropCheckConstraint` + `AddCheckConstraint`. Nothing else changes in that migration (golden test).

Opt-out: `HasEnumCheckConstraint(false)` per property, `CheckConstraints = false` globally.

## 12. EF Core Migration Helpers (#65)

```csharp
public enum EnumStoredAs
{
    Integer,          // 0, 1
    MemberName,       // PendingApproval
    CamelCase,        // pendingApproval
    LegacySnakeCase,  // 3.x Core ToSnakeCase: httpstatus
    Text,             // any known spelling: wire name, member name, any case, alias, legacy name, numeric text
    FlagsText,        // "Read, Write", "read,write"
}

migrationBuilder.ConvertEnumColumn<OrderStatus>("orders", "status", schema: null,
    from: EnumStoredAs.Integer, to: EnumStorage.String);
migrationBuilder.ConvertEnumJsonPath<OrderStatus>("orders", "payload", ["status"]);   // PostgreSQL jsonb, path segments
string sql = EnumDataAudit.Sql<OrderStatus>("orders", "status");                      // read only
```

`from` values other than `Text` and `FlagsText` are what `HasLegacyEnumStorage` writes; for reading old data `Text` accepts all of them, so `MemberName`, `CamelCase` and `LegacySnakeCase` matter only as `to` targets of `Down()`.

### 12.1 Operation order

When storage changes (removing `HasLegacyEnumStorage` or a manual `HasConversion`), `dotnet ef migrations add` generates its own `AlterColumn` (for integer to text an implicit cast that produces `'1'`) and an `AddCheckConstraint`, in an order EF chooses. The helper **replaces** EF's `AlterColumn` for that column, and the migration must run in this order:

1. `DropCheckConstraint` (when one exists).
2. `ConvertEnumColumn` (type change and data conversion in one step).
3. `AddCheckConstraint`.

The guide shows the edit: delete the generated `AlterColumn` for the column, insert the helper call, keep the constraint operations around it. Forgetting to delete the generated `AlterColumn` does not corrupt data: if EF's `AlterColumn` runs first, the helper's `CASE status WHEN 0` compares text with integer, PostgreSQL raises a type error and the migration transaction rolls back; if the helper runs first, EF's `AlterColumn` is a no-op. CSE0014 (error) catches it at build time: a `Migration` whose `Up` or `Down` contains both `AlterColumn` and `ConvertEnumColumn` for the same table and column. An automatic `IMigrationsModelDiffer` replacement was rejected because it depends on EF Core `.Internal` APIs.

### 12.2 SQL

Every CASE has an `ELSE` that keeps the original value, so no conversion ever writes `NULL` or loses data. Values that match no spelling stay as they are and fail the constraint added in step 3, which is intended: `EnumDataAudit` lists them before the migration runs.

| Conversion | SQL shape (PostgreSQL) |
|---|---|
| `Integer` → `String` | `ALTER TABLE ... ALTER COLUMN status TYPE text USING CASE status WHEN 0 THEN 'pending' WHEN 1 THEN 'pending_approval' ... ELSE status::text END` |
| `Text` → `String` | `UPDATE ... SET status = CASE lower(status) WHEN 'pendingapproval' THEN 'pending_approval' WHEN 'pending_approval' THEN 'pending_approval' WHEN 'approval' THEN 'pending_approval' WHEN '1' THEN 'pending_approval' ... ELSE status END WHERE status IS NOT NULL AND status IS DISTINCT FROM <same CASE>` |
| `String`/`Text` → `Integer` | text values are first normalized as above, then `ALTER COLUMN ... TYPE integer USING CASE status WHEN 'pending' THEN 0 ... END` (no `ELSE`: an unmapped value must fail the type change; the audit lists it first) |
| `FlagsText` → flags `Integer` | PostgreSQL rejects subqueries in `USING`, so: `ADD COLUMN status__cse integer`; `UPDATE ... SET status__cse = (SELECT coalesce(bit_or(CASE lower(trim(p)) WHEN 'read' THEN 1 WHEN 'readwrite' THEN 3 ... END), 0) FROM unnest(string_to_array(permissions, ',')) AS p)`; `DROP COLUMN permissions`; `RENAME COLUMN status__cse TO permissions`. `bit_or` handles composites that overlap. Indexes on the column are listed by the helper's XML doc as a manual step. |
| jsonb path | `UPDATE ... SET payload = jsonb_set(payload, '{status}', to_jsonb(<CASE ... END>)) WHERE payload #> '{status}' IS NOT NULL AND <CASE ... END> IS NOT NULL AND payload #>> '{status}' IS DISTINCT FROM <CASE ... END>`, where the CASE has no `ELSE` and the `IS NOT NULL` guard prevents `jsonb_set` from receiving `NULL` (which would null the whole document). Integers and strings in the document are both matched through `payload #>> '{status}'`. Array paths (`items[*].status`) are not supported in 5.0. |

- The CASE expressions are generated from metadata when the migration is authored and written into the migration as literals, so the migration does not change when the enum changes later.
- `Down()`: the mirrored call (`from: EnumStoredAs.Text, to: EnumStorage.Integer` or `to: EnumStoredAs.MemberName`). Text spellings cannot be restored byte for byte (PascalCase vs camelCase is lost), so `Down()` restores the format, not the original bytes; the XML doc states it.
- Idempotent: running a conversion twice does not change data (`Text` → `String` and jsonb updates skip already canonical values; `Integer` → `String` is guarded by the migration history like any migration).
- Other providers: `Integer` ↔ `String` and `Text` → `String` for SQL Server and SQLite through standard `CASE`; `FlagsText` and jsonb paths are PostgreSQL only in 5.0.

### 12.3 Rollout order

1. **Deploy 5.0 without changing storage.** Columns with a manual `HasConversion` are skipped (section 11.1); plain integer columns are covered by `existingStorage: EnumStoredAs.Integer`. Columns managed by 4.x `ConfigureEnumConventions` already hold 4.x wire names; the first migration alters `varchar(n)` to `text` on PostgreSQL and adds the constraint, so run `EnumDataAudit` before it. JSON columns get `HasLegacyEnumStorage(EnumStoredAs.Integer)`. Reads are tolerant from now on; writes are unchanged, so queries keep matching rows and older instances can still read new rows.
2. **Audit** every column that will be converted: run `EnumDataAudit` in production, fix or map unknown values.
   Acceptance criterion: `dotnet ef migrations add Upgrade5` right after the upgrade produces an **empty** migration (no `AlterColumn`, no `AddCheckConstraint`) for a model that only used manual conversions and plain integer columns. #64 pins it with a test; consumers use the same check.
3. **Convert**: replace the manual conversion with nothing (or remove `HasLegacyEnumStorage`), add the migration, apply section 12.1, deploy. Pre-5.0 instances must be drained before this step, because they cannot read the new format.

## 13. Outgoing HTTP Clients (`CSharpEssentials.Http`, #66)

There is no Refit (or RestEase, Flurl) package and no new dependency. Bodies are library-agnostic through `JsonSerializerOptions`; only route and query formatting is library-specific, and a `DelegatingHandler` cannot do it because the enum type is gone by then. The library therefore ships the stable non-generic `EnumValueFormatter.Format/TryFormat/TryFormatMany` (section 6.4) and the guide shows a short adapter per library.

- Request bodies: `AddEnumConventions(conventions, EnumReadMode.Data, writeAs)` on the client's `JsonSerializerOptions`.
- Responses: `Data` mode. The client keeps working while the server moves from integer to string output, and maps newer server values to the fallback member.
- Route/query in `CSharpEssentials.Http`: `HttpRequestBuilder` and `QueryStringExtensions` format enums with `EnumValueFormatter` (wire name or number by `writeAs`); flags and collections become repeated keys.
- `WriteAs = Number` per client for servers that accept only integers.

### 13.1 HTTP client recipes (guide, #68)

Refit:

```csharp
public sealed class EnumUrlParameterFormatter(EnumConventions conventions, EnumWireFormat? format = null)
    : DefaultUrlParameterFormatter
{
    public override string? Format(object? value, ICustomAttributeProvider attributeProvider, Type type) =>
        EnumValueFormatter.TryFormat(value, conventions, out string? text, format)
            ? text
            : base.Format(value, attributeProvider, type);
}

JsonSerializerOptions json = new JsonSerializerOptions(JsonSerializerDefaults.Web).AddEnumConventions(conventions);
services.AddRefitClient<IOrdersApi>(new RefitSettings
{
    ContentSerializer = new SystemTextJsonContentSerializer(json),
    UrlParameterFormatter = new EnumUrlParameterFormatter(conventions),
});
```

The guide also shows RestEase (`IRequestQueryParamSerializer`/`RequestPathParamSerializer` delegating to `TryFormat`/`TryFormatMany`), Flurl (format before `SetQueryParam`, `ISerializer` over the options), and `HttpClient` with `CSharpEssentials.Http`. Generated clients (Kiota, NSwag) need nothing: they send the wire names from the OpenAPI `enum` list, and their response readers accept names.

Rollout rule for HTTP, the same as for the bus: **every consumer before any producer**. A server switches its output from `Number` to `String` only after all of its clients run 5.0 (tolerant reads) or another reader that accepts names. A pre-5.0 Refit client fails on `pending_approval`, because the Refit default converter knows only its own naming. Mobile apps that cannot be updated keep the `Number` group or the header selector (section 9.4) for as long as they are supported.

## 14. Message Bus

No bus package. The contract is the JSON one:

```csharp
// MassTransit (STJ)
cfg.ConfigureJsonSerializerOptions(o => o.AddEnumConventions(conventions, EnumReadMode.Data, writeAs: EnumWireFormat.Number));
```

- Consumers use `Data` mode: integers from Newtonsoft producers and names from STJ producers are both read; unknown values map to the fallback member.
- Producers write `Number` until every consumer runs 5.0, then switch to `String`. The guide gives the order: consumers first, producers second.

## 15. Validation and Diagnostics (#67)

### 15.1 Validation rules (`CSharpEssentials.Validation`)

| Rule | Use |
|---|---|
| `.IsDefinedEnum()` | values created in code (casts, mapping from other enums, arithmetic) |
| `.IsOneOf(params TEnum[])` | subset of members allowed for an operation (`status` may only move to `Approved` or `Rejected`) |
| `.HasOnlyDefinedFlags()` | flags values built with bitwise operations |

Error: `Error.Validation(code: "enum.invalid" | "enum.not_allowed", description: EnumValueError.Message)`, the same text as binding errors.

### 15.2 Diagnostics (block `CSE0001` to `CSE0999`, ADR-006)

| ID | Severity | Rule |
|---|---|---|
| CSE0001 | | retired in 5.0 (nested enums are supported); ID stays reserved |
| CSE0002 | Error | Two members produce the same wire name (`HTTPStatus` and `HttpStatus` → `http_status`) |
| CSE0003 | Error | An alias equals a wire name, member name or alias of another member (case-insensitive) |
| CSE0004 | Error | More than one `[EnumFallback]` member |
| CSE0005 | Warning | Effective storage is `Integer` and a member has no explicit value (reordering changes stored data). Code fix: add explicit values |
| CSE0006 | Warning | `[Flags]` enum without a zero member. Code fix: add `None = 0` |
| CSE0007 | Warning | `[Flags]` member that is neither a power of two nor a combination of other members |
| CSE0008 | Error | `[EnumFallback]` on a `[Flags]` enum (unknown bits are rejected, a fallback is meaningless) |
| CSE0009 | Error | Wire name or alias is empty, contains whitespace or a comma, or is a valid number (it would collide with numeric input or with comma separated flags) |
| CSE0010 | Info, disabled by default | Enum without `[StringEnum]` used as a property or parameter type of a public type (takes the reflection path); enable in `.editorconfig` |
| CSE0011 | Info | `[StringEnum]` enum in a compilation below C# 9: no metadata registration, reflection path |
| CSE0012 | Warning | Invalid `CSharpEssentialsEnumNaming` MSBuild value |
| CSE0013 | Warning | `[EnumAlias]`, `[EnumFallback]` or `[StringEnum(Naming)]`-dependent attributes on an enum without `[StringEnum]` (no effect) |
| CSE0014 | Error | A migration's `Up` or `Down` contains both EF's `AlterColumn` and `ConvertEnumColumn` for the same table and column (section 12.1). Symbols are matched by metadata name, so the analyzer is inert without `CSharpEssentials.EntityFrameworkCore` |

When an error diagnostic applies, the generator skips metadata for that enum (ADR-006 rule), so the only error the user sees is the analyzer's.

## 16. Flags Guidance

| Use | When | Example |
|---|---|---|
| `[Flags]` enum | A fixed, small set of independent on/off capabilities that are checked with bit operations and stored together | permissions, feature toggles, days of week, notification channels |
| Collection of enums (`List<TEnum>`) | A user-chosen multi-select where order or duplicates may matter, or the set can grow past 64 members, or members are added often | interests, selected categories, enabled integrations |
| Single enum, never flags | One value at a time | statuses, lifecycle states, types, priorities |

Signals that a flags enum should be a collection: the API filters "contains any of", the UI shows a checklist that grows every release, or a member needs data attached to it.

## 17. AOT and Trimming

- `[StringEnum]` path: no reflection, no `MakeGenericType`, no `Activator`. The JSON factory, EF converters and model binders obtain typed instances through generated `IEnumInfo` factory methods.
- Reflection fallback (`EnumMetadata.GetOrCreateWithReflection`) is annotated; any public API that may reach it carries the same annotations or a `[StringEnum]`-only overload.
- AOT smoke test: a `PublishAot=true` console sample in `examples/` that serializes, parses and formats every matrix row with `TrimmerSingleWarn=false` and warnings as errors.

## 18. Testing Strategy

| Area | Tests |
|---|---|
| Naming | generator vs `JsonNamingPolicy` corpus test for every built-in policy |
| Generator | snapshot per feature (aliases, fallback, flags, nested, descriptions, obsolete, every underlying type including `ulong` and negative `long`), incremental caching test, CSE0002 to CSE0014 positive and negative |
| Parser | every matrix row in `Input` and `Data` mode, each `EnumConventions` switch; registration from a separate type-only assembly |
| JSON | every matrix row, flags, nullable, dictionary key, collection, alias, fallback, source generated context |
| ASP.NET Core | TestServer: matrix rows for route, query, header, form, body; minimal API and MVC in one host; v1 Number + v2 String groups |
| OpenAPI | golden files for both packages, normalized diff equality (generated client round trip deferred to 5.1) |
| EF Core | Testcontainers PostgreSQL + SQLite: column types, constraints, invalid insert rejected, migration diff after adding a member, JSON columns, collections, skipped user converters, legacy storage; migration helper conversions with an undefined value in the data (kept, never `NULL`), `Down()`, idempotency, operation order of 12.1 |
| HTTP | TestServer peer: String client ↔ Number server and Number client ↔ tolerant server (`CSharpEssentials.Http`); `EnumValueFormatter` non-generic overloads for every underlying type, nullable, flags, collections |
| Cross-layer | one table-driven golden test (#68) executed against JSON, binding, EF, `CSharpEssentials.Http` and both OpenAPI outputs |

## 19. Migration Guide Outline (`docs/migration/v4-to-v5.md`, #68)

1. Package changes: add `CSharpEssentials.AspNetCore.Swashbuckle` (or move to `CSharpEssentials.AspNetCore.OpenApi`).
2. Registration: `AddEnumBinding` → `AddEnumConventions` (`UseEnumBinding` stays); `EnumConventionOptions` → `EnumConventions`; `ConditionalStringEnumConverter` → `AddEnumConventions`.
3. Naming: custom `JsonNamingPolicy` → MSBuild property, `[StringEnum(Naming)]` or per-member names.
4. Generated helpers: `ToSnakeCase()` → `ToWireName()`, `TryParse` → `TryParseWire`; output differences (`HTTPStatus`).
5. JSON: undefined numbers are rejected; flags are arrays; add `[EnumFallback]` where newer producers exist.
6. Database:
   - Columns with a manual `HasConversion` are untouched until you remove it.
   - Integer storage → string storage (audit, convert, constraint, operation order of 12.1).
   - PascalCase/camelCase text → snake_case (audit, `Text` conversion, constraint).
   - 4.x `varchar(n)` → `text` on PostgreSQL (automatic ALTER in the next migration; review it).
   - Flags comma text → integer bitmask.
   - jsonb documents with integers: no change needed (tolerant read); optional `ConvertEnumJsonPath`.
7. Legacy clients: `WithEnumWireFormat(EnumWireFormat.Number)` per group, or the header selector for shared routes.
8. Outgoing clients: `AddEnumConventions` on the client options plus the recipe of section 13.1 (Refit, RestEase, Flurl).
9. Message bus: consumers first, producers second.
10. `EnumData<T>`-style holders → typed parameters.

## 20. Easyapp Rollout (first consumer)

| Today | Step 1 (deploy 5.0) | Target |
|---|---|---|
| APIs send enums as integers, mobile apps in stores | global `WriteAs = Number`, new v2 groups/controllers override to `String` (or the header selector); input accepts integers and names | new app versions send the header or call new groups and get names |
| plain integer enum columns | `existingStorage: EnumStoredAs.Integer`; first migration empty | `HasEnumStorage(String)` per column, audit, convert |
| 10 EF columns `HasConversion<string>()` PascalCase | unchanged: the convention skips them, tolerant reads only where `HasLegacyEnumStorage(EnumStoredAs.MemberName)` replaces the manual conversion | audit, `ConvertEnumColumn(from: Text)`, constraint (section 12.3) |
| jsonb with integers | `HasLegacyEnumStorage(EnumStoredAs.Integer)` on JSON-mapped enum properties (writes stay integers, reads accept names) | `ConvertEnumJsonPath`, then remove the legacy storage |
| 35 Refit clients with the default converter | own adapter in BuildingBlocks (section 13.1) with `writeAs: Number` where the server reads only integers | names once every server runs 5.0 (servers accept names from step 1) |
| Newtonsoft events with integers | STJ consumers on `Data` mode read integers | MassTransit STJ producers, `Number` until every consumer runs 5.0 |

Open item for easyapp: confirm whether its jsonb enums are EF `ToJson()` owned/complex types or value-converted `JsonSerializer` properties, and which naming the Refit clients actually send (the Refit default `SystemTextJsonContentSerializer` options use a camelCase `JsonStringEnumConverter`).

## 21. Open Points Resolved During Implementation

| Point | Issue | Decision rule |
|---|---|---|
| Converter precedence over `JsonSerializerContext` metadata (including `UseStringEnumConverter`) on net9.0 to net11.0 | #61 | pinned by a test; if a TFM differs, `AddEnumConventions` also sets the context's `TypeInfoResolver` modifier |
| EF Core `JsonValueReaderWriter` on enum properties of `ToJson()` owned (EF 8+) and complex (EF 10+) types | #64 | covered by tests per EF major; a provider that bypasses it is documented |

Deferred to 5.1: `EnumSchemaStyle.StringOrNumber`, a Scalar helper, a migration checklist test helper, a generated-client round trip test, JSON array paths in `ConvertEnumJsonPath`, `FlagsText` conversion for non-PostgreSQL providers.

## 22. Issue Map

| Section | Issue |
|---|---|
| 3, 4, 5, 6, 7, 8 | #61 |
| 8, 9 | #62 |
| 10 | #63 |
| 11 | #64 |
| 12 | #65 |
| 13, 14 | #66 |
| 15 | #67 |
| 16, 17, 18, 19, 20 | #68 |
