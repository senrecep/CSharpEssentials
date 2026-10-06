# Migrating from 4.x to 5.0

5.0 applies one set of enum conventions (`EnumConventions` from `CSharpEssentials.Enums`) to JSON, ASP.NET Core binding and output. The full outline is in section 19 of the [enum conventions design](../design/CSharpEssentials.Enums-DESIGN.md).

## ASP.NET Core enum binding (`CSharpEssentials.AspNetCore`)

### Registration

`AddEnumBinding` and `EnumBindingOptions` are obsolete forwarders. `UseEnumBinding()` stays and now requires `AddEnumConventions()`; without it the app throws at startup.

```csharp
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
- Leading and trailing whitespace of route, query, header and form values is trimmed before parsing, as in 4.x: `?status=%20pending` binds to `Pending`. A value that is still invalid after trimming returns 400. JSON bodies are not trimmed.
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

The obsolete 4.x `ConfigureEnumConventions(params Assembly[])` and `ConfigureEnumConventions(Action<EnumConventionOptions>, ...)` forward to the 5.0 convention, with these changes:

- They now add the check constraint like the new API, and `UseLegacySnakeCase = true` no longer applies a max length. The next migration adds the constraints; pass `existingStorage: EnumStoredAs.LegacySnakeCase` or call `HasLegacyEnumStorage` to keep the old column shape.
- A `CanConvert` predicate that selects an enum without generated metadata now throws `InvalidOperationException` when the model is built. In 4.x such an enum was stored through reflection as a string. Silently falling back to `int` would change the column type in the next migration and lose the stored names, so add `[StringEnum]` to the enum (keep it public or internal), or call `ConfigureEnumConventionsWithReflection`.
- The new `ConfigureEnumConventions(EnumConventions, EnumStoredAs?)` leaves enums without generated metadata to EF Core.
- Compiled models (`dbcontext optimize`) are not supported for properties with enum conventions in 5.0.

## EF Core enum columns: converting existing data (`CSharpEssentials.EntityFrameworkCore`)

5.0 stores `[StringEnum]` enums by wire name with a check constraint. Existing columns keep their format with `existingStorage` or `HasLegacyEnumStorage` until you convert them. Convert one column at a time:

1. **Audit.** Run `EnumDataAudit.Sql<OrderStatus>("orders", "Status", storedAs: EnumStoredAs.Integer)` in production (pass `provider: "Microsoft.EntityFrameworkCore.Sqlite"` for SQLite). Fix or map every value it returns.
2. **Change the model.** Remove `HasLegacyEnumStorage` (or opt the property in with `HasEnumStorage(EnumStorage.String)`) and run `dotnet ef migrations add`.
3. **Edit the migration.** Delete the generated `AlterColumn` for the column and put `ConvertEnumColumn` in its place, after `DropCheckConstraint` and before `AddCheckConstraint`. Build errors with CSE0014 if you forget to delete the `AlterColumn`.

```csharp
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
| `HasConversion<string>()` member names, 3.x `ToSnakeCase` names, camelCase, mixed spellings | `EnumStoredAs.Text` (accepts every known spelling) |
| `[Flags]` stored as `"Read, Write"` | `EnumStoredAs.FlagsText` with `to: EnumStorage.Integer` |
| Enum inside a PostgreSQL `jsonb` document | `ConvertEnumJsonPath<TEnum>("orders", "payload", ["status"])` |

4. **Deploy** after every pre-5.0 instance is drained; older instances cannot read the new format.

`Down()` restores the format, not the original bytes: a column that held mixed spellings comes back in the one format you name. On PostgreSQL drop a column default that cannot be cast to the new type before the conversion and recreate it after. SQL is generated for PostgreSQL and SQLite only.

## OpenAPI (`CSharpEssentials.AspNetCore.Swashbuckle`, `CSharpEssentials.AspNetCore.OpenApi`)

`CSharpEssentials.AspNetCore` no longer depends on Swashbuckle. Pick one OpenAPI package per host, never both (Microsoft.OpenApi 2.x would replace the 1.x that Swashbuckle 8/9 needs):

- Staying on Swashbuckle: add `CSharpEssentials.AspNetCore.Swashbuckle`. `AddSwagger`, `UseVersionableSwagger`, `ConfigureSwaggerOptions`, `SecuritySchemes` and the filters keep their names and namespaces. `AddSwagger` adds the enum filters; with a plain `AddSwaggerGen` call `o.AddEnumConventions()`. `EnumSchemaFilter` has no parameterless constructor any more; a `SchemaFilter<EnumSchemaFilter>()` registration still works but misses the operation filter, so replace it with `AddEnumConventions()`.
- Moving to `Microsoft.AspNetCore.OpenApi` (net10.0+): add `CSharpEssentials.AspNetCore.OpenApi` and call `services.AddOpenApi(o => o.AddEnumConventions())` per document.

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
