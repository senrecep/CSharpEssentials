# Enums End to End Example

One `[StringEnum]` enum and one `[Flags]` enum travel through every layer with a single `EnumConventions` instance: JSON bodies, route/query/header binding, response output, OpenAPI and an EF Core (PostgreSQL) column. See [Enums end to end](../../README.MD#enums-end-to-end) and the [design document](../../docs/design/CSharpEssentials.Enums-DESIGN.md).

## What it shows

| Feature | File | Description |
|---------|------|-------------|
| Enum metadata | `OrderStatus.cs` | `[StringEnum]`, `[Description]`, an `[EnumAlias]` and an `[EnumFallback]` member |
| Flags | `Permissions.cs` | `[Flags]`: a JSON array on the wire, an integer bitmask in the database |
| One registration | `Program.cs` | `AddEnumConventions()` + `UseEnumBinding()` |
| EF Core storage | `ShopDbContext.cs` | `ConfigureEnumConventions(conventions)`: wire names in a `text` column plus check constraints |
| Body, query, route, header | `OrderEndpoints.cs` | the same accept rules in every source; a rejected value returns a 400 problem |
| Results | `OrderService.cs` | `Result<Order>` with implicit conversions from `Order` and `Error` |
| Two API versions | `Program.cs` | `v1` writes numbers (`WithEnumWireFormat(EnumWireFormat.Number)`), `v2` writes wire names |
| OpenAPI | `Program.cs` | `AddOpenApi("v1"/"v2", o => o.AddEnumConventions())`: an integer schema in `v1`, a string schema in `v2` |

The sample uses `CSharpEssentials.AspNetCore.OpenApi` (net10.0). A Swashbuckle host references `CSharpEssentials.AspNetCore.Swashbuckle` instead and calls `o.AddEnumConventions()` on `SwaggerGenOptions` (`AddSwagger` does it for you). Never reference both packages in one host: they need different Microsoft.OpenApi major versions.

## Package references

The project uses `ProjectReference`s, including the generator project with `OutputItemType="Analyzer"`. That form only works inside this repository. The same app outside the repository references the packages:

```xml
<ItemGroup>
  <PackageReference Include="CSharpEssentials.Enums" Version="5.0.0" />           <!-- generator, analyzers, code fixes -->
  <PackageReference Include="CSharpEssentials.AspNetCore" Version="5.0.0" />
  <PackageReference Include="CSharpEssentials.AspNetCore.OpenApi" Version="5.0.0" />
  <PackageReference Include="CSharpEssentials.EntityFrameworkCore" Version="5.0.0" />
</ItemGroup>
```

Keep the direct `CSharpEssentials.Enums` reference: the other packages bring the library but not its generator, so without it `[StringEnum]` enums get no metadata.

## Running

```bash
export PGPASSWORD='<choose-a-local-password>'
docker run -d --name shop-postgres -e POSTGRES_PASSWORD="$PGPASSWORD" -p 5432:5432 postgres:17
ConnectionStrings__Shop="Host=localhost;Port=5432;Database=shop;Username=postgres;Password=$PGPASSWORD" \
  dotnet run --project examples/Examples.Enums.EndToEnd
```

`appsettings.json` holds the connection string without a password; supply the full string through the `ConnectionStrings__Shop` environment variable or `dotnet user-secrets`. The app creates the table on startup.

## Endpoints

Each route exists under `/api/v1` (numbers out) and `/api/v2` (wire names out). Both accept wire names, member names in any casing, aliases and numbers.

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/orders/status/{status}` | Route value: `pending_approval`, `PendingApproval`, `approval` (alias) or `1` |
| GET | `/orders?status=&permissions=` | Query: `status` is nullable, so `?status=` binds `null`; `permissions=read,write` or repeated keys |
| GET | `/orders/{id}` | `Result<Order>` to 200 or a 404 problem |
| POST | `/orders` | Body `{ "customer": "ada", "status": "pending_approval", "permissions": ["read", "write"] }`, optional `X-Source-Status` header |
| GET | `/openapi/v1.json`, `/openapi/v2.json` | The two OpenAPI documents |

```bash
curl -i http://localhost:5000/api/v2/orders/status/xyz
# 400: 'xyz' is not a valid OrderStatus. Allowed values: pending, pending_approval, shipped.
```
