# Migrating from 4.x to 5.0

5.0 applies one set of enum conventions (`EnumConventions` from `CSharpEssentials.Enums`) to JSON, ASP.NET Core binding and output, OpenAPI and EF Core. The full outline is in section 19 of the [enum conventions design](../design/CSharpEssentials.Enums-DESIGN.md); every breaking change is listed in [ADR-007](../adr/ADR-007-enum-conventions.md#breaking-changes-in-50). The [end-to-end sample](../../examples/Examples.Enums.EndToEnd/README.md) shows the finished setup.

Suggested order:

1. Packages: add `CSharpEssentials.AspNetCore.Swashbuckle` or `CSharpEssentials.AspNetCore.OpenApi` ([OpenAPI](#openapi-csharpessentialsaspnetcoreswashbuckle-csharpessentialsaspnetcoreopenapi)). A direct `CSharpEssentials.Enums` reference is not required: its source generator, analyzers and code fixes flow through any CSharpEssentials package that depends on Enums, directly or through another CSharpEssentials package (`.Core`, `.Clone`, `.Time`, `.DependencyInjection`, `.Endpoints` and `.RequestResponseLogging` do not depend on it). The CSE enum analyzers (CSE0002 to CSE0016) therefore run in every project that references one of these packages, so new CSE warnings and errors (e.g. CSE0014 in migration projects) can appear in projects that never referenced `CSharpEssentials.Enums`. CSE0002 to CSE0004, CSE0008, CSE0009, CSE0014 and CSE0016 are errors by default, so a migration project that calls `AlterColumn` and `ConvertEnumColumn` on the same column stops building, and the warnings become errors under `TreatWarningsAsErrors`; fix them or set their severity in `.editorconfig` (`dotnet_diagnostic.CSE0013.severity = none`).
2. Registration: `AddEnumBinding` → `AddEnumConventions`, `ConditionalStringEnumConverter` → `AddEnumConventions`, `EnumConventionOptions` → `EnumConventions`.
3. Build and fix the obsolete warnings (generated helpers, `StringEnumNaming`).
4. Keep old clients on numbers where needed ([Legacy numeric output](#legacy-numeric-output)).
5. Deploy with tolerant reads and no storage change, then convert the database columns one at a time ([Database rollout](#ef-core-enum-columns-database-rollout-csharpessentialsentityframeworkcore)).

## New defaults

`EnumConventions.Default` is what every layer uses unless you pass your own instance (`AddEnumConventions(c => c with { ... })`, `ConfigureEnumConventions(conventions)`):

| Setting | Default | Meaning |
|---|---|---|
| Wire name | snake_case lower (`PendingApproval` → `pending_approval`) | decided at build time: `[JsonStringEnumMemberName]` > `[EnumMember]` > `[StringEnum(Naming = ...)]` > `<CSharpEssentialsEnumNaming>` > snake_case lower |
| `AcceptNumbers` | `true` | defined numbers (`1`, `"1"`) are read; undefined numbers never are |
| `AcceptMemberNames` | `true` | `PendingApproval` is read next to `pending_approval`; aliases (`[EnumAlias]`) always are |
| `CaseInsensitive` | `true` | `PENDING_APPROVAL` is read |
| `UnknownValue` | `UseFallback` | tolerant reads (EF, client responses, consumers) map unknown values to the `[EnumFallback]` member; request input always rejects them |
| `WriteAs` | `String` | JSON and responses write wire names; `Number` per group or globally |
| `Storage` | `String` | EF stores the wire name (`text` on PostgreSQL) |
| `FlagsStorage` | `Integer` | EF stores `[Flags]` as an integer bitmask |
| `CheckConstraints` | `true` | EF adds `ck_{table}_{column}_enum` |
| `CanHandle` | enums with generated metadata (`[StringEnum]`) | everything else keeps the framework default |

## Enums without `[StringEnum]`

Plain enums fall through to the framework defaults in every layer; nothing throws:

- JSON: not handled by `AddEnumConventions`. A `JsonStringEnumConverter` the host registered still applies to them.
- Binding and output: stock Minimal API and MVC binding, number output.
- EF Core: EF's integer column, no converter, no check constraint.
- OpenAPI: the generator's default schema.

4.x handled them through reflection when a predicate (`CanBind`, `canConvert`, `CanConvert`) selected them. Add `[StringEnum]` to adopt the conventions. The reflection opt-ins (`AddEnumConventionsWithReflection`, `EnumConverterFactory.CreateWithReflectionFallback`, `ConfigureEnumConventionsWithReflection`) are not trimming or AOT safe. A `[StringEnum]` enum the generator cannot emit metadata for (private, or nested in a generic type) reports CSE0015, and its converter throws instead of falling back to numbers.

## Naming and generated helpers (`CSharpEssentials.Enums`)

| 4.x | 5.0 |
|---|---|
| `value.ToSnakeCase()` | `value.ToWireName()` |
| `OrderStatusExtensions.TryParse(text, out status)`, `Parse(text)` | `OrderStatusExtensions.TryParseWire(text, out status)`, `ParseWire(text)` |
| `StringEnumNaming` (runtime policy, reflection cache) | `EnumMetadata` and the generated helpers |
| a custom `JsonNamingPolicy` for enums | `[StringEnum(Naming = EnumNaming.KebabCaseLower)]`, `<CSharpEssentialsEnumNaming>KebabCaseLower</CSharpEssentialsEnumNaming>` in `Directory.Build.props`, or `[JsonStringEnumMemberName]` per member |

The old helpers stay as obsolete members whose message names the replacement. `ToSnakeCase()` keeps its 4.x output, which can differ from the wire name for acronyms and digits: `ToWireName()` matches `JsonNamingPolicy.SnakeCaseLower` (`HTTPStatus` → `http_status`). Check values you stored or compared from `ToSnakeCase()` before switching; reads accept the old spelling.

Changing a wire name later (renaming a member, changing `Naming`) changes the EF check constraint too. See [Renaming a member safely](../../CSharpEssentials.Enums/Readme.MD#renaming-a-member-safely).

## JSON (`CSharpEssentials.Json`)

```csharp
using CSharpEssentials.Enums; // EnumConventions
using CSharpEssentials.Json;  // AddEnumConventions

// 4.x
options.Converters.Add(new ConditionalStringEnumConverter(JsonNamingPolicy.SnakeCaseLower, allowIntegerValues: true));

// 5.0 (ASP.NET Core hosts: AddEnumConventions() does this for the Minimal API and MVC JsonOptions)
options.AddEnumConventions(EnumConventions.Default);
```

- `ConditionalStringEnumConverter` is obsolete; `AllowUndefinedValues` is removed (compile break). Undefined numbers are rejected. Consumers that must survive newer producers add an `[EnumFallback]` member and read in `EnumReadMode.Data` (the default of `AddEnumConventions` on `JsonSerializerOptions`).
- `[Flags]` values are written as arrays (`["read","write"]`); the comma string `"read, write"` is still read. Clients that parse the string must read arrays.
- Member names are read in any casing, and aliases are read.
- `EnhancedJsonSerializerOptions`, `PolymorphicJsonConverterFactory`/`PolymorphicJsonConverter<T>` and the `ConvertTo*`/`ConvertFrom*` helpers carry `[RequiresUnreferencedCode]`/`[RequiresDynamicCode]`. Trim and AOT builds with warnings as errors see IL2026/IL3050 there; pass a source-generated `JsonSerializerContext`.

## ASP.NET Core enum binding (`CSharpEssentials.AspNetCore`)

### Registration

`AddEnumBinding` and `EnumBindingOptions` are obsolete forwarders. `UseEnumBinding()` stays and now requires `AddEnumConventions()`; without it the app throws at startup.

```csharp
using CSharpEssentials.AspNetCore; // AddEnumConventions, UseEnumBinding
using CSharpEssentials.Enums;      // EnumConventions
using CSharpEssentials.Errors;     // Error

// 4.x
builder.Services.AddEnumBinding(o =>
{
    o.AllowIntegerValues = false;
    o.CanBind = t => t == typeof(OrderStatus);
    o.ErrorFactory = (key, enumType, names) => Error.Validation($"validation.{key}", string.Join(", ", names));
});
app.UseEnumBinding();

// 5.0
builder.Services.AddEnumConventions(c => c with
    {
        AcceptNumbers = false,
        CanHandle = t => t == typeof(OrderStatus),
    })
    .ConfigureErrors((error, key) => Error.Validation($"validation.{key}", string.Join(", ", error.AllowedValues)));
app.UseEnumBinding();
```

| 4.x | 5.0 |
|---|---|
| `EnumBindingOptions.AllowIntegerValues` | `EnumConventions.AcceptNumbers` |
| `EnumBindingOptions.CanBind` | `EnumConventions.CanHandle` (defaults to enums with generated metadata) |
| `EnumBindingOptions.ErrorFactory(key, enumType, names)` | `EnumConventionsBuilder.ConfigureErrors((error, key) => ...)`, also used for JSON body errors |
| `EnumBindingOptions.NamingPolicy` | `[StringEnum(Naming = ...)]` or the `CSharpEssentialsEnumNaming` MSBuild property; a runtime policy other than snake_case throws `NotSupportedException` |

### Behavior changes

- Route, query, header and form values follow the accept rules of a JSON body. Header (`[FromHeader]`) and form (`[FromForm]`) values are new sources.
- Leading and trailing whitespace of route, query, header and form values is trimmed before parsing, as in 4.x: `?status=%20pending` binds to `Pending`. A value that is still invalid after trimming returns 400. An empty or whitespace-only value (`?status=%20`) binds `null` for a nullable parameter and returns 400 for a non-nullable one. JSON bodies are not trimmed.
- The default error message lists the allowed values: `'99' is not a valid OrderStatus. Allowed values: pending, pending_approval.`
- Arrays accept repeated keys and comma-separated values (`?s=a&s=b`, `?s=a,b`).
- `AddEnumConventions` also applies the conventions to the Minimal API and MVC `JsonOptions` (in 4.x a separate converter registration). Enums without generated metadata keep the framework's behavior; `AddEnumConventionsWithReflection` opts them in.

### `EnumData<T>`-style holders become typed parameters

The pattern "string parameter + action filter + scoped holder" existed because the framework could not bind the wire names. `UseEnumBinding()` binds them, and a rejected value returns the 400 problem before the action runs, so the parameter can be the enum itself.

```csharp
// 4.x
public sealed class EnumData<TEnum> where TEnum : struct, Enum
{
    public TEnum Value { get; set; }
}

public sealed class OrderStatusFilter(EnumData<OrderStatus> holder) : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        string? text = context.ActionArguments["status"] as string;
        if (!OrderStatusExtensions.TryParse(text, out OrderStatus status))
        {
            context.Result = new BadRequestResult();
            return;
        }
        holder.Value = status;
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}

[HttpGet("orders/{status}")]
[ServiceFilter<OrderStatusFilter>]
public IActionResult List(string status, [FromServices] EnumData<OrderStatus> holder) => Ok(service.List(holder.Value));

// 5.0
[HttpGet("orders/{status}")]
public IActionResult List(OrderStatus status) => Ok(service.List(status));
```

Remove the holder registration (`AddScoped<EnumData<T>>()`) and the filter. The same works for Minimal API handlers (`(OrderStatus status) => ...`), nullable and array parameters, and query DTO properties.

### Legacy numeric output

Clients that still expect numbers keep them per group, controller or action; the host's `JsonOptions` are not changed:

```csharp
using CSharpEssentials.AspNetCore; // WithEnumWireFormat, [EnumWireFormat]
using CSharpEssentials.Enums;      // EnumWireFormat

app.MapGroup("/api/v1").WithEnumWireFormat(EnumWireFormat.Number);

[EnumWireFormat(EnumWireFormat.Number)]
public sealed class LegacyOrdersController : ControllerBase { ... }
```

Routes shared by old and new clients can select the format per request; the response gets `Vary: X-Enum-Format`:

```csharp
group.WithEnumWireFormat("X-Enum-Format",
    ctx => ctx.Request.Headers["X-Enum-Format"] == "string" ? EnumWireFormat.String : EnumWireFormat.Number);
```

To keep numbers everywhere and opt new APIs into strings, set `AddEnumConventions(c => c with { WriteAs = EnumWireFormat.Number })` and use `WithEnumWireFormat(EnumWireFormat.String)` on the new groups. Precedence: action attribute > controller attribute > endpoint or group > `WriteAs`.

## EF Core enum storage (`CSharpEssentials.EntityFrameworkCore`)

| 4.x | 5.0 |
|---|---|
| `ConfigureEnumConventions(params Assembly[])` | `ConfigureEnumConventions(conventions)`, no assembly scan |
| `EnumConventionOptions.CanConvert` | `EnumConventions.CanHandle` |
| `EnumConventionOptions.UseLegacySnakeCase = true` | `existingStorage: EnumStoredAs.LegacySnakeCase` or `HasLegacyEnumStorage(EnumStoredAs.LegacySnakeCase)` until the column is converted |

The obsolete 4.x `ConfigureEnumConventions(params Assembly[])` and `ConfigureEnumConventions(Action<EnumConventionOptions>, ...)` forward to the 5.0 convention, with these changes:

- They now add the check constraint like the new API, and `UseLegacySnakeCase = true` no longer applies a max length. The next migration adds the constraints; pass `existingStorage: EnumStoredAs.LegacySnakeCase` or call `HasLegacyEnumStorage` to keep the old column shape.
- A `CanConvert` predicate that selects an enum without generated metadata now throws `InvalidOperationException` when the model is built. In 4.x such an enum was stored through reflection as a string. Silently falling back to `int` would change the column type in the next migration and lose the stored names, so add `[StringEnum]` to the enum (keep it public or internal), or call `ConfigureEnumConventionsWithReflection`.
- The new `ConfigureEnumConventions(EnumConventions, EnumStoredAs?)` leaves enums without generated metadata to EF Core.
- Compiled models (`dbcontext optimize`) are not supported for properties with enum conventions in 5.0.

## EF Core enum columns: database rollout (`CSharpEssentials.EntityFrameworkCore`)

5.0 stores `[StringEnum]` enums by wire name in a `text` column (provider length elsewhere) with a check constraint, and `[Flags]` as an integer bitmask. Columns with a manual `HasConversion` are skipped until you remove it. Roll out one column at a time:

1. **Audit.** Run the read-only query of `EnumDataAudit.Sql<OrderStatus>("orders", "Status", storedAs: EnumStoredAs.Integer)` in production (pass `provider: "Microsoft.EntityFrameworkCore.Sqlite"` for SQLite) with psql or `Database.SqlQueryRaw` ([how](../../CSharpEssentials.EntityFrameworkCore/Readme.MD#auditing-the-data)). Fix or map every value it returns. Do this before the first 5.0 migration if the column was managed by the 4.x `ConfigureEnumConventions`: that migration alters `varchar(n)` to `text` on PostgreSQL and adds the constraint.
2. **Deploy tolerant reads.** Upgrade to 5.0 without changing what is written: `ConfigureEnumConventions(conventions, existingStorage: EnumStoredAs.Integer)` for plain integer columns, or `HasLegacyEnumStorage(EnumStoredAs.MemberName)` (`Integer`, `CamelCase`, `LegacySnakeCase`, `FlagsText`) per property. Reads accept every known spelling from now on; writes and query parameters keep the old format, so filters still match and older instances still read new rows. `dotnet ef migrations add Upgrade5` should produce an empty migration.
3. **Convert.** Remove `HasLegacyEnumStorage` from the property, or, if the column follows the global `existingStorage` setting, opt it in with `HasEnumStorage(EnumStorage.String)`: `existingStorage` applies to every column without an explicit `HasEnumStorage`, so each column you convert needs its own `HasEnumStorage(EnumStorage.String)`. Then run `dotnet ef migrations add`. In the generated migration, delete the `AlterColumn` for the column and put `ConvertEnumColumn` in its place, after `DropCheckConstraint` when the column already has one. CSE0014 fails the build if the `AlterColumn` is left in.
4. **Add the constraint.** Keep the generated `AddCheckConstraint` after `ConvertEnumColumn`, so every converted row is checked. Deploy after every pre-5.0 instance is drained; older instances cannot read the new format.

```csharp
using CSharpEssentials.Enums;               // EnumStorage
using CSharpEssentials.EntityFrameworkCore; // ConvertEnumColumn, EnumStoredAs

protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.ConvertEnumColumn<OrderStatus>("orders", "Status", from: EnumStoredAs.Integer, to: EnumStorage.String);
    migrationBuilder.AddCheckConstraint(name: "ck_orders_Status_enum", table: "orders", sql: "...");   // as generated
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropCheckConstraint(name: "ck_orders_Status_enum", table: "orders");
    migrationBuilder.ConvertEnumColumn<OrderStatus>("orders", "Status", from: EnumStoredAs.Text, to: EnumStorage.Integer);
}
```

| Old column | `from:` |
|---|---|
| EF default integers | `EnumStoredAs.Integer` |
| `HasConversion<string>()` member names, 3.x/4.x `ToSnakeCase` names, camelCase, mixed spellings | `EnumStoredAs.Text` (accepts every known spelling) |
| `[Flags]` stored as `"Read, Write"` | `EnumStoredAs.FlagsText` with `to: EnumStorage.Integer` |
| Enum inside a PostgreSQL `jsonb` document | `ConvertEnumJsonPath<TEnum>("orders", "payload", ["status"])` (optional: reads are tolerant) |

A value that matches no spelling is never written as `NULL`: text targets keep it and the constraint of step 4 rejects it, integer targets abort the migration with an error naming the value. `Down()` restores the format, not the original bytes: a column that held mixed spellings comes back in the one format you name. On PostgreSQL drop a column default that cannot be cast to the new type before the conversion and recreate it after. SQL is generated for PostgreSQL and SQLite only.

## Validation (`CSharpEssentials.Validation`)

New rules `IsDefinedEnum()`, `IsOneOf(...)` and `HasOnlyDefinedFlags()` report the codes `{Prop}.IsDefinedEnum`, `{Prop}.IsOneOf` and `{Prop}.HasOnlyDefinedFlags`; the message lists the allowed values like binding errors.

## Outgoing HTTP clients (`CSharpEssentials.Http`)

- `ToQueryString(object)`, `ToQueryString(object, EnumConventions, EnumWireFormat?)`, `WithQueryString(Uri, object)` and `WithQueryString(Uri, object, EnumConventions, EnumWireFormat?)` read public properties through reflection and carry `[RequiresUnreferencedCode]`/`[RequiresDynamicCode]` on net8.0+. Trim and AOT builds see IL2026/IL3050; pass a `Dictionary<string, string?>` or use `WithQueryString(name, value)`.
- `HttpRequestBuilder.WithEnumConventions(conventions)` formats enums in routes and queries; flags and collections become repeated keys. Use `WithEnumConventions(conventions, EnumWireFormat.Number)` for servers that accept only integers.
- Refit, RestEase and Flurl are not referenced; put `AddEnumConventions(conventions)` on the client's `JsonSerializerOptions` and format route and query values with `EnumValueFormatter.TryFormat` (recipes in section 13.1 of the design).

## Message bus

There is no bus package. A bus that serializes with System.Text.Json takes the same JSON options, for example MassTransit: `cfg.ConfigureJsonSerializerOptions(o => o.AddEnumConventions(conventions, EnumReadMode.Data, writeAs: EnumWireFormat.Number))`. Upgrade consumers first (they read numbers and names), then switch producers from `Number` to `String`.

## OpenAPI (`CSharpEssentials.AspNetCore.Swashbuckle`, `CSharpEssentials.AspNetCore.OpenApi`)

`CSharpEssentials.AspNetCore` no longer depends on Swashbuckle. Pick one OpenAPI package per host, never both (Microsoft.OpenApi 2.x would replace the 1.x that Swashbuckle 8/9 needs):

- Staying on Swashbuckle: add `CSharpEssentials.AspNetCore.Swashbuckle`. `AddSwagger`, `UseVersionableSwagger`, `ConfigureSwaggerOptions`, `SecuritySchemes` and the filters keep their names and namespaces. `AddSwagger` adds the enum filters; with a plain `AddSwaggerGen` call `o.AddEnumConventions()`. `EnumSchemaFilter` has no parameterless constructor any more; a `SchemaFilter<EnumSchemaFilter>()` registration still works but misses the operation filter, so replace it with `AddEnumConventions()`.
- Moving to `Microsoft.AspNetCore.OpenApi` (net10.0+): add `CSharpEssentials.AspNetCore.OpenApi` and call `services.AddOpenApi(o => o.AddEnumConventions())` per document. The package targets net10.0 only, with `Microsoft.AspNetCore.OpenApi` 10.x and Microsoft.OpenApi 2.x; `Microsoft.AspNetCore.OpenApi` 11.x needs Microsoft.OpenApi 3.x, so net11.0 support will come later as a non-breaking addition.

```bash
dotnet add package CSharpEssentials.AspNetCore.Swashbuckle   # or CSharpEssentials.AspNetCore.OpenApi
```

The enum schemas change in both packages:

| 4.x (Swashbuckle filter) | 5.0 |
|---|---|
| `description` replaced by `Possible values: a, b` | your description kept, value table appended (number, description, deprecated, `Response only:`) |
| names from the snake_case policy | wire names from the metadata (`[JsonStringEnumMemberName]`, aliases not listed) |
| always strings | integers in a document whose operations all write numbers; mixed documents mark the number operations (`x-enum-wire-format: number`) |
| flags: one string schema | arrays with `uniqueItems: true` |
| nullable: Swashbuckle default | nullable where used (`allOf` + `nullable` in 3.0, `oneOf` with `type: null` in 3.1) |
| no extensions | `x-enum-varnames`, `x-enum-descriptions`, `x-enum-numeric-values` |

Client generators that read `x-enum-varnames` (NSwag, openapi-generator, Kiota) keep the C# member names. Regenerate clients after the upgrade.
