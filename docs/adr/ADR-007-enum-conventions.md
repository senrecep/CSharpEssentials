# ADR-007: One Enum Contract for Every Layer, Built at Compile Time

- **Status:** Proposed
- **Date:** 2026-10-06
- **Issues:** #59 (epic), #60 (this ADR and the design document), #61 to #68 (implementation)
- **Design:** [CSharpEssentials.Enums-DESIGN.md](../design/CSharpEssentials.Enums-DESIGN.md)
- **Supersedes:** ADR-004 for `CSharpEssentials.Enums` (the generator moves to the ADR-006 layout).

---

## Context

An enum value crosses up to eight boundaries in a typical service: the database column, JSON columns, request and response bodies, route/query/header/form values, outgoing HTTP clients, message bus payloads, validation and the OpenAPI document. In 4.1 each boundary has its own code and its own options:

| Layer | 4.1 code | Own settings |
|---|---|---|
| JSON | `ConditionalStringEnumConverter` on top of `JsonStringEnumConverter` | naming policy, `allowIntegerValues`, `AllowUndefinedValues`, predicate |
| Binding | `EnumBindingMiddleware` + `EnumBindingOptions` | naming policy, `AllowIntegerValues`, `CanBind`, `ErrorFactory` |
| EF Core | `EnumToFormattedStringConverter`, `LegacySnakeCaseEnumConverter`, `EnumConventionOptions` | `CanConvert`, `UseLegacySnakeCase`, max length |
| OpenAPI | `EnumSchemaFilter` (Swashbuckle only) | none, hardcoded default policy |
| Generated helpers | `ToSnakeCase()`, `TryParse` from `StringEnumGenerator` | none, a different snake case algorithm |

The 4.1 audit found eight defects that all come from this split (listed in #59): an undefined number accepted by JSON and then stored in a row that can no longer be loaded, `[Flags]` values that overflow the column, case handling that differs between query and body, a generated `ToSnakeCase()` that disagrees with the wire name, a schema filter that ignores the configured policy, and Swashbuckle code bound to Microsoft.OpenApi 1.x.

The first consumer (easyapp) adds hard constraints: every API sends enums as integers today, ten EF columns store PascalCase names, jsonb documents hold integers, 35 Refit clients use the Refit default serializer settings (camelCase enum strings), events go through Newtonsoft with integers, and mobile apps already in the stores cannot be forced to update. A new contract must read all of that and must be able to keep writing integers to old clients.

## Decision

### 1. One contract, one parser, one formatter

Every layer formats with the same function and parses with the same function. Both live in `CSharpEssentials.Enums` and depend only on generated metadata, so JSON, binding, EF Core, HTTP clients, validation and OpenAPI cannot drift. A layer may restrict what it accepts (a request never accepts the fallback member), but it never spells a value differently.

### 2. Metadata is generated, not reflected

The `[StringEnum]` source generator emits an `EnumInfo<TEnum>` per enum: values, C# names, wire names, aliases, descriptions, `[Obsolete]`, flags, the defined mask and the fallback member. A generated module initializer registers it in `EnumMetadata`. Lookups are a static generic field read (`EnumMetadata.Get<TEnum>()`) or a frozen dictionary lookup (`EnumMetadata.TryGet(Type)`). Enums without generated metadata are handled only through an explicit, opt-in reflection fallback annotated `[RequiresUnreferencedCode]` and `[RequiresDynamicCode]`. A `[StringEnum]` enum whose metadata is missing (unreachable nested type, C# below 9, a contracts assembly built with 4.x) fails loudly instead of silently becoming an integer: CSE0015 at build time, `InvalidOperationException` when a converter is created.

### 3. Naming is a build-time decision

Wire names are computed by the generator, so they are constants in the compiled assembly. The naming comes from, in priority order: `[JsonStringEnumMemberName]` or `[EnumMember(Value = ...)]` on the member, `[StringEnum(Naming = ...)]` on the enum, the MSBuild property `<CSharpEssentialsEnumNaming>` of the declaring project, and finally `SnakeCaseLower`. The generator ports the `System.Text.Json` separator algorithm, and a test pins it against `JsonNamingPolicy` for every built-in policy, so `HTTPStatus` is `http_status` in every layer.

There is no runtime naming policy on `EnumConventions`. A runtime policy would make generated constants and `ToWireName()` disagree with the wire, which is defect 5 again. An enum's wire names belong to the assembly that declares it, which is also the right owner when a shared contracts library is used by several services.

### 4. Runtime behavior is one options object

`EnumConventions` holds every runtime switch (accepted inputs, unknown value handling, output format, storage). It is immutable, has `EnumConventions.Default`, and is registered once with `services.AddEnumConventions(o => ...)`. JSON, ASP.NET Core, EF Core, HTTP clients and OpenAPI read that single instance. Per scope overrides exist where a real need exists (storage per enum and per property, output format per endpoint group), and each override is expressed against the same type.

### 5. Secure by default

- An undefined value never enters the system. Requests get a 400 that lists the allowed values. Data read from storage or from another service maps to the declared `[EnumFallback]` member (`UnknownValue = UseFallback`, the default) or fails with a descriptive exception when the enum has no fallback member.
- Writes are canonical. Output is always the wire name unless legacy numeric output is selected explicitly.
- The database enforces the set of values with check constraints generated by an EF Core model convention, so `dotnet ef migrations add` keeps them in sync when members change. Properties with a user value converter or `HasLegacyEnumStorage` are left alone, so upgrading never changes an existing column until the team opts in.

### 6. Reads are tolerant, inputs are bounded

Reads accept the wire name, the C# name, any casing, declared aliases and the number of a defined member. That covers every format easyapp has in production today (PascalCase columns, integer jsonb, integer request bodies, Refit camelCase). Tolerance stops at defined members: `99` and `"bogus"` are rejected everywhere.

### 7. Flags and collections are first class

`[Flags]` values are a JSON array of names on the wire, repeated or comma separated keys in the query, and an integer bitmask with a mask check constraint in the database by default. Column length is therefore never a problem for flags. `List<TEnum>` and other enum collections map to `text[]` on PostgreSQL with a containment check.

### 8. OpenAPI moves to its own packages

`CSharpEssentials.AspNetCore` stops depending on Swashbuckle. Two packages produce the same enum schema (no package is added for HTTP client libraries such as Refit; see the design, section 13):

- `CSharpEssentials.AspNetCore.OpenApi` for `Microsoft.AspNetCore.OpenApi` and Microsoft.OpenApi 2.x, net10.0+ only. Microsoft.OpenApi 1.x and 2.x differ in the schema type, enum values, extensions and schema model, so a net9.0 target would duplicate the package core; net8.0/net9.0 reach end of support on 2026-11-10. Adding net9.0 later is non-breaking.
- `CSharpEssentials.AspNetCore.Swashbuckle` for the existing `AddSwagger` code (Swashbuckle 8.x/9.x, Microsoft.OpenApi 1.x, net8.0+). It carries the net9.0 story.

A host references one of the two, never both: Microsoft.OpenApi 2.x would replace the 1.x that Swashbuckle 8/9 needs and break it at runtime. No other package depends on Microsoft.OpenApi, and a test pins that the two dependency closures stay apart.

Both describe an enum as its wire names plus `x-enum-varnames`, `x-enum-descriptions`, `x-enum-numeric-values` and a `value | number | description` table, and both follow the output format of the endpoint group.

### 9. The Enums generator moves to the ADR-006 layout

The generator grows from one file into metadata emission, wire naming and five or more analyzers. It moves to `CSharpEssentials.Enums.Generators` (netstandard2.0, Roslyn 4.8, not packable), packed into `CSharpEssentials.Enums` at `analyzers/dotnet/cs`. The `lib/netstandard2.0` asset of `CSharpEssentials.Enums` no longer contains Roslyn types. Package consumers see no change.

### 10. This is 5.0.0

The defaults change (undefined values rejected, flags as arrays, no max length on PostgreSQL, check constraints in the next migration) and Swashbuckle code moves to a new package. Both are breaking, so the epic ships as 5.0.0 with `docs/migration/v4-to-v5.md`.

## Breaking changes in 5.0

| Area | 4.x | 5.0 | Migration |
|---|---|---|---|
| Swashbuckle | in `CSharpEssentials.AspNetCore` | `CSharpEssentials.AspNetCore.Swashbuckle` (or `CSharpEssentials.AspNetCore.OpenApi` on net10.0+, never both) | add the package, same `AddSwagger` API and namespaces; `EnumSchemaFilter` takes an `IServiceProvider`, register it with `AddEnumConventions()` |
| OpenAPI enum schemas | `Possible values:` description, snake_case strings | wire names, value table, `x-enum-*` extensions, integers in number documents, flags arrays, nullable where used | regenerate clients |
| JSON undefined numbers | accepted (`AllowUndefinedValues = true`) | rejected; `ConditionalStringEnumConverter.AllowUndefinedValues` removed (compile break) | none for valid data; `[EnumFallback]` for consumers |
| `[StringEnum]` without metadata | snake_case string through reflection | CSE0015 warning, converter creation throws | make the enum internal/public, not nested in a generic type; rebuild contracts with 5.0 |
| Enums without `[StringEnum]` | handled through reflection when the predicate selected them | JSON: not handled unless `AddEnumConventionsWithReflection` / `CreateWithReflectionFallback` is used. EF conventions and enum binding: left to the framework default (EF int storage, stock MVC/minimal API binding and number output; nothing throws), reflection only through the same explicit opt-in (#62/#64) | none; add `[StringEnum]` to adopt the conventions |
| Json AOT annotations | none | `EnhancedJsonSerializerOptions`, `PolymorphicJsonConverterFactory`/`PolymorphicJsonConverter<T>` and `ConvertTo*`/`ConvertFrom*` carry `[RequiresUnreferencedCode]`/`[RequiresDynamicCode]` | trim/AOT consumers with warnings as errors see IL2026/IL3050 on these calls; pass a source-generated `JsonSerializerContext` or opt out explicitly |
| JSON flags | `"read, write"` | `["read","write"]` (comma string still read) | clients that parse flags strings must read arrays |
| JSON member names | case-sensitive policy name or C# name | any casing, aliases | none |
| EF manual `HasConversion` | untouched | untouched (skipped by the convention) | remove it when ready to convert |
| EF string column | `varchar(longest name)` | `text` on PostgreSQL, provider length elsewhere | next migration alters the column type |
| EF check constraint | none | `ck_{table}_{column}_enum` | audit data first (`EnumDataAudit`), convert with the migration helpers |
| EF flags | comma string | integer bitmask | `ConvertEnumColumn<TEnum>(from: EnumStoredAs.FlagsText)` |
| EF options | `EnumConventionOptions`, `UseLegacySnakeCase` | `EnumConventions`, `HasLegacyEnumStorage` | legacy snake case names are read tolerantly; `HasLegacyEnumStorage(EnumStoredAs.LegacySnakeCase)` keeps writing them until the column is converted; the obsolete `UseLegacySnakeCase = true` forwards to `existingStorage: EnumStoredAs.LegacySnakeCase` |
| EF obsolete forwarders | `ConfigureEnumConventions(Assembly...)` and `(Action<EnumConventionOptions>)` added no check constraint; `UseLegacySnakeCase` set a max length | the forwarders add check constraints like the new API; `UseLegacySnakeCase` no longer applies a max length | the next migration adds the constraints; pass `existingStorage: EnumStoredAs.LegacySnakeCase` or call `HasLegacyEnumStorage` to keep the old column shape |
| EF obsolete forwarders and `CanConvert` | a `CanConvert` predicate that selected an enum without metadata stored it as a string through reflection | the forwarder throws `InvalidOperationException` at model build for such an enum (a silent `int` fallback would change the column and lose data); the new `ConfigureEnumConventions(EnumConventions...)` leaves plain enums alone | add `[StringEnum]`, or use `ConfigureEnumConventionsWithReflection` |
| EF compiled models | `dbcontext optimize` works | not supported for properties with enum conventions: the converters capture the model's `EnumConventions` and metadata as instances | do not use compiled models for contexts with enum conventions in 5.0 (design section 11) |
| Binding whitespace | `?status=%20pending` bound | route, query, header and form values are trimmed before parsing; a value that is still invalid returns 400; JSON bodies are not trimmed | none; clients that sent padded values keep working |
| Validation error codes | none (new rules) | `{Prop}.IsDefinedEnum`, `{Prop}.IsOneOf`, `{Prop}.HasOnlyDefinedFlags`, the format of the other Validation rules | match on these codes; the message lists the allowed values like binding errors |
| Binding | `AddEnumBinding` + `UseEnumBinding` | `AddEnumConventions` + `UseEnumBinding` | `AddEnumBinding` is an obsolete forwarder; `UseEnumBinding` stays |
| Generated helpers | `ToSnakeCase()`, `TryParse(string)` | `ToWireName()`, `TryParseWire(string)` | obsolete with the new name in the message |
| `StringEnumNaming` | public static helper with runtime policy | obsolete facade over `EnumMetadata` | use `EnumMetadata` / generated helpers |

## Consequences

**Positive**
- One spelling per value across every layer, provable by one cross-layer golden test (#68).
- No startup reflection for `[StringEnum]` enums; AOT and trimming are clean on the generated path.
- Legacy data and legacy clients keep working: tolerant reads, aliases, per-group numeric output, legacy storage, migration helpers. Rollout order is consumers before producers for HTTP and the bus, and old write format until each column is converted.
- The database rejects bad values even when they come from outside the application (manual SQL, other services).

**Negative**
- A runtime naming policy is no longer possible. Teams that used a custom `JsonNamingPolicy` set the MSBuild property, the enum attribute, or `[JsonStringEnumMemberName]` per member.
- Enums that are not marked `[StringEnum]` are not handled unless the reflection fallback is opted in, and that path is not AOT safe. CSE0010 (info, opt-in) points to them.
- Check constraints must be dropped and recreated when members change. The convention does it automatically, but the migration is no longer empty for an enum change.
- New dependencies `Microsoft.AspNetCore.OpenApi` and `Microsoft.OpenApi` 2.x (approved 2026-10-06), confined to `CSharpEssentials.AspNetCore.OpenApi`. There is no Refit package: HTTP client libraries get a ten-line adapter over the public non-generic `EnumValueFormatter` (design section 13.1).

**Neutral**
- Newtonsoft.Json is not supported. Newtonsoft producers that write integers are read correctly by STJ consumers through tolerant reads; the migration guide covers the transition.

## Alternatives Considered

| Alternative | Rejected because |
|---|---|
| Keep `JsonStringEnumConverter` and fix the wrappers | It cannot read aliases, cannot reject undefined numbers without a second pass, writes flags as a comma string, and has its own naming tables. The audit defects come from wrapping it. |
| Runtime `JsonNamingPolicy` in `EnumConventions` | Generated constants and helpers would disagree with the wire. Naming would also differ between hosts that load the same contracts assembly. |
| Reflection metadata cached at startup (4.1 `StringEnumNaming`) | Not AOT safe, and the cache key includes the policy, which is how layers diverged. |
| Per-layer options with a shared default | That is the 4.1 design. A custom value on one layer silently diverges from the others (defect 3). |
| Reject numbers by default | Every easyapp client sends integers today and old app versions cannot be updated. Numbers of defined members stay accepted; `AcceptNumbers = false` turns it off. |
| Integers on the wire by default | Not self-describing, unsafe under member reordering, and unreadable in logs. Integer output stays available per endpoint group for legacy clients. |
| `varchar(n)` with the longest name | The source of the flags overflow, and a migration for every renamed member. A check constraint is the real guard; `text` costs nothing on PostgreSQL. |
| Database enum types (`CREATE TYPE ... AS ENUM`) | PostgreSQL only, values cannot be removed, and renames need DDL outside EF migrations. Check constraints work on every provider. |
