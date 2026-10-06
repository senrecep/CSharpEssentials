# CSharpEssentials.Endpoints — Design Document

> **Date:** 2026-10-06 | **Status:** Approved design (4.1)
> **Issues:** #48 (epic), #49 (docs), #50 (infra), #51 (contracts), #52 (generator), #53 (analyzer), #54 (fallback + versioned group), #57 (docs/AOT example), #58 (P3)
> **Related:** [ADR-006](../adr/ADR-006-source-generators-in-separate-projects.md), [DependencyInjection design](CSharpEssentials.DependencyInjection-DESIGN.md)

---

## 1. Goals and Non-Goals

### Goals
- Replace Carter for Minimal API endpoint organization: one type per endpoint (or a small set of related routes), discovered at compile time, mapped with one call.
- Endpoint groups that map to real `MapGroup` route groups, including nesting.
- Trim/AOT clean on the generated path, with no startup reflection.
- Per-assembly registries plus an opt-in cross-assembly aggregate for the host.

### Non-Goals
- No custom request pipeline. Binding, validation, filters, `IResult`, ProblemDetails, rate limiting, output cache, authorization and OpenAPI stay ASP.NET Core's.
- No route DSL. The generator never emits `MapGet`/`MapPost`/`MapMethods`.
- No endpoint instances, constructor injection or endpoint base classes.
- No command/event bus, background jobs or response negotiation.

## 2. Principles

| # | Principle | Consequence |
|---|---|---|
| 1 | Thin layer over Minimal APIs | User code calls `MapGet`/`MapPost` inside `static Map`. The Request Delegate Generator (RDG) interceptors still apply because the call sites are in user code. |
| 2 | No startup reflection on the generated path | Generated registries call `T.Map(...)` through static abstract members. Reflection exists only in `MapEndpointsFromAssemblies`, annotated `[RequiresUnreferencedCode]` + `[RequiresDynamicCode]`. |
| 3 | Opt-in | Only types implementing `IEndpoint` are mapped, and only when the user calls a registry or aggregate method. Nothing is mapped implicitly. |
| 4 | Coexistence | MVC controllers, manual `app.MapGet(...)` calls and other libraries' endpoints work alongside generated mapping in the same host. |
| 5 | Static and stateless | Endpoints and groups are static members on types. Dependencies come from handler parameters (DI injection into delegates), not constructors. |

## 3. Packages, TFMs and Dependencies

| Project | TFMs | Dependencies | Packable |
|---|---|---|---|
| `CSharpEssentials.Endpoints` | `net11.0;net10.0;net9.0;net8.0` (same as AspNetCore) | `FrameworkReference Microsoft.AspNetCore.App` | yes |
| `CSharpEssentials.Endpoints.Generators` | `netstandard2.0` | `Microsoft.CodeAnalysis.CSharp` `VersionOverride="4.8.0"`, `Microsoft.CodeAnalysis.Analyzers` | no — packed into `CSharpEssentials.Endpoints` at `analyzers/dotnet/cs` (ADR-006) |
| `CSharpEssentials.AspNetCore` (existing) | unchanged | **no** reference to Endpoints | yes |

Packing: `build/PackGenerator.targets` (imported by the runtime csproj) adds a `ReferenceOutputAssembly=false` project reference to `$(MSBuildProjectName).Generators` and packs its `netstandard2.0` dll at `analyzers/dotnet/cs`. `build/CSharpEssentials.Endpoints.props` is packed at both `build/` and `buildTransitive/`.

Namespace: `CSharpEssentials.Endpoints`. Generated registries live in `Microsoft.AspNetCore.Builder`, so `app.Map{Asm}Endpoints()` is discoverable without a `using`.

## 4. Public Contracts (#51)

All public types are `sealed` unless they are interfaces or static classes, carry XML docs, and follow one type per file.

### 4.1 `IEndpoint`

```csharp
public interface IEndpoint
{
    static abstract void Map(IEndpointRouteBuilder app);
}
```

- Implemented by a non-abstract, non-generic `class`, `record` or `struct`. A `static class` cannot implement an interface, and instances are never created.
- `app` is the builder the endpoint is mapped on: the group builder when the type has `[EndpointGroup]`, otherwise a per-type empty-prefix group under the root builder (§5.4).
- A type may map more than one route in `Map`. All of them receive the type's metadata.

### 4.2 `IEndpointGroup`

```csharp
public interface IEndpointGroup
{
    static abstract string Prefix { get; }
    static virtual void Configure(RouteGroupBuilder group) { }
}
```

- `Prefix` is passed to `MapGroup(Prefix)`. An empty string is allowed.
- `Configure` applies group-wide conventions (`WithTags`, `RequireAuthorization`, filters, `WithOpenApi`, and so on). It is called once per created group builder.
- A group may itself carry `[EndpointGroup]` to nest under a parent group.
- A group with no endpoints (directly or through nested groups) is not created.

### 4.3 Attributes

| Attribute | Targets | Constructor / properties | Purpose |
|---|---|---|---|
| `EndpointGroupAttribute` | Class, Struct | `(Type groupType)` | Places an endpoint or group under `groupType`. `typeof` form is primary. |
| `EndpointGroupAttribute<TGroup>` | Class, Struct | `where TGroup : IEndpointGroup` | Generic form (C# 11+). It is equivalent to the `typeof` form and only one of the two may be present (CSE1003). |
| `ExcludeFromMappingAttribute` | Class, Struct, Assembly | — | Excludes a type from discovery (generated and fallback paths). On a group, it excludes the group and everything under it. On an assembly, no registry or `EndpointModule` attribute is generated. |
| `EndpointRegistryNameAttribute` | Assembly | `(string name)` | Overrides the registry name. `[assembly: EndpointRegistryName("Apps")]` → `AppsEndpointRegistry.MapAppsEndpoints`. The value is sanitized like an assembly name (§5.2). |
| `EndpointModuleAttribute` | Assembly, `AllowMultiple=false` | `(Type registryType)`, `RegistryType { get; }` | Generated. It marks an assembly that contains a registry and is read by the aggregate generator in referencing assemblies. |
| `GenerateEndpointAggregateAttribute` | Assembly | — | Opt-in: generate `MapAllEndpoints` in a library or test project (§5.6). |
| `DisableEndpointAggregateAttribute` | Assembly | — | Opt-out: suppress the automatic `MapAllEndpoints` in an `Exe`/`WinExe` (§5.6). |

All attributes are `sealed`, with `Inherited = false`.

### 4.4 `EndpointMappingOptions`

```csharp
public sealed class EndpointMappingOptions
{
    public EndpointMappingOptions ConfigureEach(Action<IEndpointConventionBuilder, Type> configure);
    public EndpointMappingOptions Filter(Func<Type, bool> predicate);
    public bool LogDiscovered { get; set; }                       // default false
    public OperationNaming OperationNaming { get; set; }          // default OperationNaming.None
    public bool AutoTagFromGroup { get; set; }                    // default false
}
```

| Member | Semantics |
|---|---|
| `ConfigureEach` | Callbacks run once per endpoint type after group `Configure` and per-type metadata (§5.5). The `Type` argument is the endpoint type. Multiple calls accumulate in registration order. |
| `Filter` | Called once per endpoint type before mapping. If it returns `false`, the type is skipped. Multiple calls are AND-combined. Groups whose endpoints are all filtered out are not created. |
| `LogDiscovered` | Logs each mapped endpoint type (and each filtered type) at `Debug` through `ILoggerFactory` from `app.ServiceProvider`, category `CSharpEssentials.Endpoints`. If no `ILoggerFactory` is registered, nothing is logged. |
| `OperationNaming` | Endpoint name (operationId) policy, see 4.5. |
| `AutoTagFromGroup` | Adds a tag with the innermost group's type name, with a trailing `Group` suffix removed (`UsersGroup` → `Users`). The tag is applied only when the endpoint has no explicit `ITagsMetadata`. |

### 4.5 `OperationNaming`

```csharp
public sealed class OperationNaming
{
    public static OperationNaming None { get; }
    public static OperationNaming TypeName { get; }
    public static OperationNaming Custom(Func<Type, EndpointBuilder, string?> nameFactory);
}
```

- `None`: names are not changed.
- `TypeName`: endpoint name = endpoint type name (`CreateApp`). Generic arity suffixes do not apply because generic endpoints are skipped (CSE1006).
- `Custom`: the delegate receives the endpoint type and the endpoint builder (HTTP methods and route available). Returning `null` leaves the endpoint unnamed.
- Naming is applied through `IEndpointConventionBuilder.Finally`, so an explicit `WithName(...)` in the user's `Map` body always wins. A type that maps several routes under `TypeName` naming must name them explicitly. Otherwise, ASP.NET Core rejects the duplicate endpoint names at startup.

### 4.6 `EndpointTypeMetadata`

```csharp
public sealed class EndpointTypeMetadata(Type endpointType)
{
    public Type EndpointType { get; } = endpointType;
}
```

It is added to every endpoint produced by an `IEndpoint` type. It identifies the source type at runtime (diagnostics, tests, the `RouteOf<T>` helper of §8) without reflection over handlers.

### 4.7 `EndpointMapper` (runtime helper, used by generated code and the fallback)

```csharp
[EditorBrowsable(EditorBrowsableState.Never)]
public static class EndpointMapper
{
    public static EndpointMappingOptions CreateOptions(Action<EndpointMappingOptions>? configure);
    public static bool ShouldMapAny(IEndpointRouteBuilder app, EndpointMappingOptions options, params Type[] endpointTypes);
    public static RouteGroupBuilder MapGroup<TGroup>(IEndpointRouteBuilder parent) where TGroup : IEndpointGroup;
    public static void MapEndpoint<TEndpoint>(IEndpointRouteBuilder parent, Type? innermostGroup, EndpointMappingOptions options)
        where TEndpoint : IEndpoint;
}
```

This is the only place that implements wrapping, ordering, naming, tagging, filtering and logging. Generated code stays small, and the reflection fallback reuses exactly the same logic (via `MakeGenericMethod`). This gives parity by construction.

`ShouldMapAny` (added during implementation of #51) returns `true` when at least one of the given endpoint types passes `options.Filter`. Generated code and the fallback wrap each group in it (passing every endpoint type under that group, directly or through nested groups), so a group whose endpoints are all filtered out is never created and its `Configure` never runs. Filter results are memoized per type inside the options instance, so each filter predicate runs once per type and a filtered type is logged once, even though both `ShouldMapAny` and `MapEndpoint` consult the filter.

## 5. Generated Code (#52)

### 5.1 Discovery

- `context.SyntaxProvider.CreateSyntaxProvider`: the predicate keeps `TypeDeclarationSyntax` (class/record/struct) that has a base list. The transform resolves the symbol and checks `AllInterfaces` for `CSharpEssentials.Endpoints.IEndpoint` / `IEndpointGroup` by metadata name. `ForAttributeWithMetadataName` is not used because discovery is interface-based.
- The transform outputs value-equatable models (fully qualified name, accessibility, group type name, flags), with no symbols. Partial types are deduplicated by fully qualified name.
- Skipped (no code, analyzer reports): abstract and open-generic types (CSE1006, info), inaccessible types (CSE1001), types in group cycles (CSE1002), types with conflicting group attributes (CSE1003), types whose group target is invalid (CSE1007), `[ExcludeFromMapping]` types, and everything when the assembly has `[ExcludeFromMapping]`.
- If no endpoint survives, no registry and no `EndpointModule` attribute are emitted.

### 5.2 Names

- `{Asm}` = sanitized assembly name, or the `EndpointRegistryName` value if present.
- Sanitization: split on every character that is not `[A-Za-z0-9_]`, upper-case the first character of each segment, then concatenate. If the result starts with a digit, prefix it with `_`. Examples: `MyCompany.Apps.Api` → `MyCompanyAppsApi`, `web-api` → `WebApi`.
- Two referenced assemblies that sanitize to the same name produce ambiguous registry types in an aggregate host. Fix this with `EndpointRegistryName` in one of them.

### 5.3 Per-assembly registry

```csharp
namespace Microsoft.AspNetCore.Builder;

public static class {Asm}EndpointRegistry
{
    public static IReadOnlyList<Type> EndpointTypes { get; }

    public static IEndpointRouteBuilder Map{Asm}Endpoints(
        this IEndpointRouteBuilder app,
        Action<EndpointMappingOptions>? configure = null);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void MapEndpoints(IEndpointRouteBuilder app, EndpointMappingOptions options);
}

[assembly: CSharpEssentials.Endpoints.EndpointModule(typeof(Microsoft.AspNetCore.Builder.{Asm}EndpointRegistry))]
```

- `EndpointTypes` lists the mapped endpoint types in mapping order (before runtime `Filter`).
- `Map{Asm}Endpoints` returns `app` for chaining. It works on any `IEndpointRouteBuilder`, including a `RouteGroupBuilder` (`app.MapVersionedGroup(2).MapAppsEndpoints()`).
- Mapping order is deterministic: groups are created depth-first in order of fully qualified name, endpoints sorted by fully qualified name, and root-level endpoints first.

### 5.4 Wrapping and ordering

Each endpoint type is wrapped in its own empty-prefix group:

```csharp
var g = parent.MapGroup("");
T.Map(g);
g.WithMetadata(new EndpointTypeMetadata(typeof(T)));
```

- `MapGroup("")` adds no route segment. The resulting `RoutePattern`, ApiExplorer group name, tags and operationId are identical to direct mapping. This is verified with `TestServer` (#52 acceptance).
- The generator never emits `MapGet`/`MapPost`.

### 5.5 Convention order

Conventions are applied in this fixed order:

1. Group `Configure` (outer group before inner group), applied when `MapGroup<TGroup>` creates the group.
2. Per-type metadata: `EndpointTypeMetadata`, `AutoTagFromGroup` tag, `OperationNaming` (the last two through `Finally`, so explicit metadata wins).
3. `ConfigureEach` callbacks, in registration order.

Conventions set inside the user's `Map` body (`.RequireAuthorization()`, `.AddEndpointFilter(...)`, `.WithName(...)`) are endpoint-level and apply after all group-level conventions, per ASP.NET Core semantics. They are never overridden.

### 5.6 Aggregate `MapAllEndpoints`

```csharp
namespace Microsoft.AspNetCore.Builder;

internal static class {Asm}EndpointAggregate
{
    internal static IEndpointRouteBuilder MapAllEndpoints(
        this IEndpointRouteBuilder app,
        Action<EndpointMappingOptions>? configure = null);
}
```

| Rule | Behavior |
|---|---|
| Visibility | Always `internal`. It is never part of a library's public API. |
| Auto-generation | Emitted automatically when `OutputKind` is `ConsoleApplication` or `WindowsApplication` (`Exe`/`WinExe`) **and** `build_property.IsTestProject` is not `true`. The package ships `build/CSharpEssentials.Endpoints.props` with `<CompilerVisibleProperty Include="IsTestProject" />`. |
| Opt-out | `[assembly: DisableEndpointAggregate]` suppresses auto-generation. |
| Opt-in | Libraries and test projects get it only with `[assembly: GenerateEndpointAggregate]`. |
| Contents | Own registry (if any) plus every referenced assembly's `EndpointModule` registry. Options are created once and passed to each `MapEndpoints(app, options)`. |
| Reference filtering | Referenced assemblies are filtered **by name** before their attributes are read: skip `System*`, `Microsoft*`, `mscorlib`, `netstandard`, and any assembly whose identity does not reference `CSharpEssentials.Endpoints`. Only the survivors have `GetAttributes()` evaluated. |
| No duplicate mapping | Registries are deduplicated by registry type. An assembly that has both its own module and the aggregate maps its endpoints exactly once. |
| Order | Own registry first, then referenced registries by assembly name (ordinal). |
| Exclusions | Runtime selection uses `options.Filter` (for example, by `type.Assembly`). |
| Empty aggregate | An eligible assembly gets `MapAllEndpoints` even when it has no own registry and no referenced module. The method is then a no-op, so `app.MapAllEndpoints()` compiles before the first endpoint exists. *(Added during #52: emitting it conditionally would break the host build whenever all endpoints are removed or excluded.)* |

An assembly that sees another assembly's internals (`InternalsVisibleTo`) and also has its own aggregate gets an ambiguous `MapAllEndpoints` call. In that case, call the registries explicitly.

### 5.7 Sample generated code

Input (assembly `Sample.Api`, an `Exe`):

```csharp
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

public sealed class Health : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app.MapGet("/health", () => TypedResults.Ok());
}
```

Output `SampleApiEndpointRegistry.g.cs`:

```csharp
// <auto-generated/>
#nullable enable
[assembly: global::CSharpEssentials.Endpoints.EndpointModule(typeof(global::Microsoft.AspNetCore.Builder.SampleApiEndpointRegistry))]

namespace Microsoft.AspNetCore.Builder
{
    [global::System.CodeDom.Compiler.GeneratedCode("CSharpEssentials.Endpoints.Generators", "4.1.0")]
    public static class SampleApiEndpointRegistry
    {
        public static global::System.Collections.Generic.IReadOnlyList<global::System.Type> EndpointTypes { get; } = new global::System.Type[]
        {
            typeof(global::Sample.Api.Health),
            typeof(global::Sample.Api.CreateApp),
        };

        public static global::Microsoft.AspNetCore.Routing.IEndpointRouteBuilder MapSampleApiEndpoints(
            this global::Microsoft.AspNetCore.Routing.IEndpointRouteBuilder app,
            global::System.Action<global::CSharpEssentials.Endpoints.EndpointMappingOptions>? configure = null)
        {
            MapEndpoints(app, global::CSharpEssentials.Endpoints.EndpointMapper.CreateOptions(configure));
            return app;
        }

        [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
        public static void MapEndpoints(
            global::Microsoft.AspNetCore.Routing.IEndpointRouteBuilder app,
            global::CSharpEssentials.Endpoints.EndpointMappingOptions options)
        {
            global::CSharpEssentials.Endpoints.EndpointMapper.MapEndpoint<global::Sample.Api.Health>(app, null, options);

            if (global::CSharpEssentials.Endpoints.EndpointMapper.ShouldMapAny(app, options, typeof(global::Sample.Api.CreateApp)))
            {
                var appsGroup = global::CSharpEssentials.Endpoints.EndpointMapper.MapGroup<global::Sample.Api.AppsGroup>(app);
                global::CSharpEssentials.Endpoints.EndpointMapper.MapEndpoint<global::Sample.Api.CreateApp>(appsGroup, typeof(global::Sample.Api.AppsGroup), options);
            }
        }
    }
}
```

Output `SampleApiEndpointAggregate.g.cs` (`Exe`, no opt-out, references `Sample.Modules.Billing` which has a module):

```csharp
// <auto-generated/>
#nullable enable
namespace Microsoft.AspNetCore.Builder
{
    internal static class SampleApiEndpointAggregate
    {
        internal static global::Microsoft.AspNetCore.Routing.IEndpointRouteBuilder MapAllEndpoints(
            this global::Microsoft.AspNetCore.Routing.IEndpointRouteBuilder app,
            global::System.Action<global::CSharpEssentials.Endpoints.EndpointMappingOptions>? configure = null)
        {
            var options = global::CSharpEssentials.Endpoints.EndpointMapper.CreateOptions(configure);
            global::Microsoft.AspNetCore.Builder.SampleApiEndpointRegistry.MapEndpoints(app, options);
            global::Microsoft.AspNetCore.Builder.SampleModulesBillingEndpointRegistry.MapEndpoints(app, options);
            return app;
        }
    }
}
```

The generated registry carries `<summary>`/`<param>` XML docs on its public members (omitted above for brevity), so consumers with `GenerateDocumentationFile` and warnings-as-errors build clean (CS1591). *(Added during #52.)* The `GeneratedCode` version is the generator assembly version.

The runtime `EndpointMapper.MapEndpoint<T>` performs the §5.4 wrapping and the §5.5 ordering. Snapshot tests pin the exact text, and the listing above shows the shape.

## 6. Reflection Fallback and Versioned Group (#54)

### 6.1 `MapEndpointsFromAssemblies`

```csharp
public static class EndpointRouteBuilderExtensions
{
    [RequiresUnreferencedCode("Scans assemblies for IEndpoint types; use the generated Map{Asm}Endpoints for trimmed/AOT apps.")]
    [RequiresDynamicCode("Closes EndpointMapper generic methods at runtime.")]
    public static IEndpointRouteBuilder MapEndpointsFromAssemblies(this IEndpointRouteBuilder app, params Assembly[] assemblies);

    [RequiresUnreferencedCode("...")]
    [RequiresDynamicCode("...")]
    public static IEndpointRouteBuilder MapEndpointsFromAssemblies(
        this IEndpointRouteBuilder app, Action<EndpointMappingOptions>? configure, params Assembly[] assemblies);
}
```

- Discovery uses the same rules as the generator (§5.1): it skips abstract, open-generic, inaccessible (the CSE1001 set), cyclic, conflicting and excluded types, and skips assemblies with `[ExcludeFromMapping]`. It uses the same ordering and the same `EndpointMapper`.
- On `ReflectionTypeLoadException`, it maps the loadable types and **logs** every loader exception (with the type name when available) at `Warning`, category `CSharpEssentials.Endpoints`. Nothing is silently swallowed.
- Invalid types that the analyzer would report as errors are skipped and logged at `Warning`, because there is no compile-time check on this path.
- Implementation note: abstract, open-generic and `[ExcludeFromMapping]` types (and endpoints under an excluded group) are skipped without a log entry, matching the generator, where they produce no error. Each assembly is scanned once even when it is passed more than once, and gets its own group tree, like one generated registry per assembly.
- Parity test: the fallback and the generated registry produce the same endpoint set (route patterns, metadata, order).

### 6.2 `MapVersionedGroup` (CSharpEssentials.AspNetCore)

```csharp
public static RouteGroupBuilder MapVersionedGroup(this IEndpointRouteBuilder app, int version);
```

- Built on the existing `CreateVersionSet` / `CreateVersionedGroup` logic with an empty route: prefix `v{version:apiVersion}` and `WithApiVersionSet(...)`. It does not add a trailing empty segment.
- It lives in `CSharpEssentials.AspNetCore` and has **no** reference to `CSharpEssentials.Endpoints`. Composition happens in user code: `app.MapVersionedGroup(2).MapAppsEndpoints()`.
- Tests cover version routing and the ApiExplorer group name. A test asserts that `CSharpEssentials.AspNetCore` does not reference `CSharpEssentials.Endpoints`.

## 7. Diagnostics (#53)

Reported by `EndpointsAnalyzer` (`DiagnosticAnalyzer`) in `CSharpEssentials.Endpoints.Generators`, never by the generator (ADR-006). Category `CSharpEssentials.Endpoints`. Help links point to the README diagnostics table.

| ID | Rule | Severity | Rationale |
|---|---|---|---|
| CSE1001 | Endpoint or group type is not accessible from generated code (private/protected nested, `file`-local, or nested in an inaccessible type) | Error | Generated code would not compile |
| CSE1002 | Group nesting cycle (`[EndpointGroup]` chain returns to a type already in the chain) | Error | Mapping cannot terminate |
| CSE1003 | More than one group attribute on one type (`typeof` form and generic form both present) | Error | Ambiguous parent |
| CSE1004 | Endpoint declares instance state (instance fields or auto-properties) or a constructor with parameters | Warning | It is never instantiated, so the state and dependencies are dead code |
| CSE1005 | Duplicate HTTP method + route literal in the same group (best effort, constant strings only) | Warning | Ambiguous match at runtime. Compares `MapGet/Post/Put/Delete/Patch` and `MapMethods` with a constant pattern and a constant method array, called directly on the `Map` parameter. Patterns compare case-insensitively without leading/trailing `/`. Ungrouped endpoints form one group. Reported on each pattern argument at compilation end. |
| CSE1006 | Abstract or open-generic `IEndpoint`/`IEndpointGroup` type is skipped | Info | Deliberate skip, made visible |
| CSE1007 | `[EndpointGroup(typeof(X))]` target does not implement `IEndpointGroup`, or is abstract or open-generic | Error | Generated `MapGroup<X>` would not compile |

CSE1007 is added by this design. The `typeof` form cannot carry the `IEndpointGroup` constraint that the generic form has, so this case would otherwise surface as a compile error in generated code. It ships with #53.

Every ID gets a positive and a negative test and an entry in `AnalyzerReleases.Unshipped.md`.

## 8. P3 Follow-ups (#58, optional)

- Shipped: CSE1005 duplicate HTTP method + route literal (warning, §7).
- Shipped: typed route helper `app.RouteOf<CreateApp>(new { id })` and `app.RouteOf<CreateApp>(nameOrMethod, values)` in `EndpointRouteLookupExtensions`. It selects `RouteEndpoint`s by `EndpointTypeMetadata` (optionally by `WithName` or HTTP method), binds values with `TemplateBinderFactory` and checks route constraints. No route, an ambiguous route or a missing/invalid value throws `InvalidOperationException`. It is `[RequiresUnreferencedCode]` because `RouteValueDictionary(object)` reads properties by reflection.
- Shipped: validation endpoint filter `ValidationEndpointFilter<T>` + `RouteHandlerBuilder.WithValidation<T>()` in **`CSharpEssentials.AspNetCore`**, not in Endpoints. Endpoints stays dependency-light, and AspNetCore must not reference Endpoints. `ToProblemResult` already lives in AspNetCore, so the filter needs one new edge, AspNetCore → Validation (Validation depends only on Results and DI abstractions, so no cycle). It works for any Minimal API handler, inside or outside `IEndpoint.Map`. Missing validators throw `InvalidOperationException`; a handler without a `T` parameter fails when the endpoint is built.
- Shipped: security shortcuts `RequireRoles`, `RequirePolicies` and `RequireAuthSchemes` (`params string[]`, any `IEndpointConventionBuilder`) in `EndpointAuthorizationExtensions`. Roles and schemes are joined into one `AuthorizeAttribute` (any of them), policies add one `AuthorizeAttribute` each (all of them). The names do not collide with ASP.NET Core 8 to 11, which only ship `RequireAuthorization` and `AllowAnonymous`.
- Code fix for CSE1004 (needs `Microsoft.CodeAnalysis.CSharp.Workspaces` in a separate `*.CodeFixes` project; owner approval required).

## 9. Explicitly Skipped Features

### Carter
| Feature | Reason skipped |
|---|---|
| `ICarterModule` instances with constructor injection | Static abstract `Map` gives no instances and AOT-safe dispatch. Dependencies belong in handler parameters. |
| `CarterModule` base class with fluent config in the constructor | `IEndpointGroup.Configure` is the single place for group conventions. Avoids base classes. |
| Built-in FluentValidation (`IValidatorLocator`, `ValidateAsync`) | Not our validation stack. `WithValidation<T>()` in `CSharpEssentials.AspNetCore` targets `CSharpEssentials.Validation` (§8). |
| Response negotiation (`IResponseNegotiator`) | ASP.NET Core `IResult`/`TypedResults` already cover it (thin-layer principle). |
| Runtime assembly scanning (`DependencyContextAssemblyCatalog`) | Replaced by the generator. Reflection exists only as an explicit fallback. |
| `CarterConfigurator` explicit module/validator lists | `EndpointMappingOptions.Filter` and `[ExcludeFromMapping]`. |

### FastEndpoints
| Feature | Reason skipped |
|---|---|
| REPR base classes (`Endpoint<TReq,TRes>`) with their own binding and serialization pipeline | Replaces Minimal APIs instead of layering on them. |
| Pre/post processors and the built-in validation pipeline | Endpoint filters already provide this. Validation is an endpoint filter in `CSharpEssentials.AspNetCore` (§8). |
| Command bus, event bus, job queues | Out of scope. `CSharpEssentials.Mediator` covers in-process messaging. |
| Permission code generation and security DSL | Native `RequireAuthorization`. Only the thin shortcuts in `EndpointAuthorizationExtensions` ship. |
| NSwag-based Swagger generation | Native ApiExplorer/OpenAPI, verified identical with wrapping. |
| Route and verb declared in a `Configure()` override | Routes stay in `Map` bodies so RDG interceptors apply. |
| Test fixtures (`AppFixture`) | `WebApplicationFactory`/`TestServer` are sufficient. |

### Immediate.Apis
| Feature | Reason skipped |
|---|---|
| Generated `MapGet`/`MapPost` from `[MapGet("/route")]` attributes on handlers | A parallel routing DSL. Generated calls would bypass user-code RDG interception, so the generator never emits verbs. |
| Dependency on Immediate.Handlers behaviors pipeline | Coupling to a handler framework. Use Mediator or endpoint filters. |
| Generated validation (Immediate.Validations) | Endpoint filter on `CSharpEssentials.Validation` in `CSharpEssentials.AspNetCore` (§8). |
| `CustomizeEndpoint` static hook | `Map` already holds the endpoint's conventions. |

## 10. Testing Strategy

| Layer | Tests |
|---|---|
| Options (#51) | `Filter` AND-combination, `OperationNaming` (`None`/`TypeName`/`Custom`, explicit `WithName` wins), `AutoTagFromGroup` (suffix strip, explicit tags win), `ConfigureEach` order, `LogDiscovered` |
| Generator snapshots (#52) | Registry, nested groups, empty assembly (no output), `EndpointRegistryName`, sanitized names, aggregate on/off (Exe, library, test project, opt-in, opt-out), Verify.SourceGenerators |
| Incremental caching | Second run with an unrelated edit → tracked steps `Cached`/`Unchanged` |
| Behavior (`TestServer`) | `MapGroup("")` yields an identical `RoutePattern`, ApiExplorer group, tags and operationId vs. direct mapping; convention order (§5.5); filters and `RequireAuthorization` in `Map` still apply; nested groups; aggregate across two fixture assemblies (`CSharpEssentials.Tests.Fixtures.EndpointsA/B`); no duplicate mapping with module + aggregate; MVC controllers coexist |
| Analyzer (#53, #58) | Positive and negative per ID (CSE1001–1007) |
| Fallback (#54) | Parity with generated registry; `ReflectionTypeLoadException` logging; versioned group routing + ApiExplorer; AspNetCore has no Endpoints reference |
| Pack | Generator dll is under `analyzers/dotnet/cs` in `CSharpEssentials.Endpoints.nupkg`, with no `*.Generators` package |
| AOT (#57) | `examples/Examples.Endpoints` publishes with `PublishAot=true` and zero trim/AOT warnings (CI `aot` job) |

Test location: `CSharpEssentials.Tests/Endpoints/` and `CSharpEssentials.Tests/Generators/`. xUnit + FluentAssertions, `Method_Should_Behavior`.
