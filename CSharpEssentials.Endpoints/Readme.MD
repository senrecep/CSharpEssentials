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

## Mapping

The bundled source generator creates one registry per assembly in the `Microsoft.AspNetCore.Builder` namespace:

```csharp
var app = builder.Build();

app.MapAppsEndpoints();   // one assembly's registry
// or
app.MapAllEndpoints();    // own registry + every referenced assembly's registry
```

- `Map{Asm}Endpoints` maps the endpoints of one assembly. `{Asm}` is the sanitized assembly name (`MyCompany.Apps.Api` → `MyCompanyAppsApi`) or the `[assembly: EndpointRegistryName("Apps")]` value.
- `{Asm}EndpointRegistry.EndpointTypes` lists the endpoint types in mapping order.
- `MapAllEndpoints` is `internal` and generated automatically in `Exe`/`WinExe` projects that are not test projects. Use `[assembly: GenerateEndpointAggregate]` to opt in from a library or test project, `[assembly: DisableEndpointAggregate]` to opt out. Each registry is mapped once, own registry first, then referenced registries by assembly name.
- Each endpoint type is mapped inside its own `MapGroup("")`, so route patterns, OpenAPI metadata, filters and authorization are the same as direct mapping. Route calls such as `MapGet` stay in your `Map` method.

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

## Diagnostics

The bundled analyzer reports types the generator cannot map. Invalid types are skipped, so the generated registry always compiles. Types and assemblies marked `[ExcludeFromMapping]` are not analyzed.

| ID | Severity | Rule |
|---|---|---|
| CSE1001 | Error | Endpoint or group type is not accessible from generated code (private/protected nested, `file`-local, or nested in an inaccessible type). |
| CSE1002 | Error | Group nesting cycle: the `[EndpointGroup]` chain returns to the same group. Reported on each group in the cycle. |
| CSE1003 | Error | More than one group attribute on one type (`[EndpointGroup(typeof(X))]` and `[EndpointGroup<Y>]`). |
| CSE1004 | Warning | Endpoint declares instance fields, auto-properties or a constructor with parameters. Endpoint types are never instantiated; take dependencies as handler parameters. |
| CSE1006 | Info | Abstract or open-generic endpoint or group type is skipped. |
| CSE1007 | Error | `[EndpointGroup(typeof(X))]` target does not implement `IEndpointGroup`, or is abstract or open-generic. |
