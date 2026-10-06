# CSharpEssentials.Endpoints

Source-generated endpoint organization for ASP.NET Core Minimal APIs. A thin layer over Minimal APIs: route calls stay in your code, so binding, filters, `IResult`, OpenAPI and the Request Delegate Generator work unchanged.

## Installation

```bash
dotnet add package CSharpEssentials.Endpoints
```

Target frameworks: `net11.0`, `net10.0`, `net9.0`, `net8.0`.

## Endpoints and groups

```csharp
using CSharpEssentials.Endpoints;

public sealed class AppsGroup : IEndpointGroup
{
    public static string Prefix => "apps";

    public static void Configure(RouteGroupBuilder group) => group.WithTags("Apps").RequireAuthorization();
}

[EndpointGroup(typeof(AppsGroup))]
public sealed class CreateApp : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/", (CreateAppRequest request, IAppService service) => service.CreateAsync(request));
}
```

- `IEndpoint.Map` is static. Endpoint types are never instantiated; dependencies come from handler parameters.
- `IEndpointGroup.Prefix` is passed to `MapGroup`. `Configure` applies group-wide conventions.
- `[EndpointGroup(typeof(TGroup))]` or `[EndpointGroup<TGroup>]` places an endpoint or a group under a group. Groups can nest.
- `[ExcludeFromMapping]` excludes an endpoint, a group (with everything under it) or an assembly.

## Mapping options

```csharp
app.MapAppsEndpoints(options =>
{
    options.Filter(type => type.Namespace != "Sample.Internal");
    options.OperationNaming = OperationNaming.TypeName;
    options.AutoTagFromGroup = true;
    options.LogDiscovered = true;
    options.ConfigureEach((builder, type) => builder.WithMetadata(new MyMetadata(type)));
});
```

| Option | Behavior |
|---|---|
| `Filter` | Skips endpoint types for which a predicate returns `false`. Multiple predicates are AND-combined. Groups whose endpoints are all filtered out are not created. |
| `ConfigureEach` | Runs once per endpoint type after group `Configure` and per-type metadata, in registration order. |
| `OperationNaming` | `None` (default), `TypeName` or `Custom(...)`. An explicit `WithName(...)` always wins. |
| `AutoTagFromGroup` | Tags endpoints without explicit tags with the innermost group name (`UsersGroup` → `Users`). |
| `LogDiscovered` | Logs mapped and filtered endpoint types at `Debug`, category `CSharpEssentials.Endpoints`. |

Every endpoint gets `EndpointTypeMetadata`, which identifies the endpoint type at runtime.
