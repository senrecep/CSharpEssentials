# CSharpEssentials.DependencyInjection — Design Document

> **Date:** 2026-10-06 | **Status:** Approved design (4.1)
> **Issues:** #48 (epic), #49 (docs), #50 (infra), #55 (attributes + runtime), #56 (generator + analyzer), #57 (docs/AOT example), #58 (P3)
> **Related:** [ADR-006](../adr/ADR-006-source-generators-in-separate-projects.md), [Endpoints design](CSharpEssentials.Endpoints-DESIGN.md)

---

## 1. Goals and Non-Goals

### Goals
- Replace Scrutor for attribute-based registration and decoration on top of `Microsoft.Extensions.DependencyInjection`.
- Compile-time registration through a generated `Add{Asm}Services()`. Trim/AOT clean, with no startup reflection.
- Deterministic service-type resolution, key-aware strategies and keyed decoration.
- Gradual migration: manual `services.Add*` calls and generated registrations coexist.

### Non-Goals
- No container replacement and no new lifetimes. The output is plain `ServiceDescriptor`s.
- No convention-based "register everything" scanning.
- No touching third-party registrations (source-generated Mediator, MassTransit consumers, and so on). Only marked types are registered.
- No interception/proxy generation.

## 2. Principles

| # | Principle | Consequence |
|---|---|---|
| 1 | Opt-in only | A type is registered only if it carries `[RegisterScoped]`, `[RegisterSingleton]` or `[RegisterTransient]`, and decorated only via `[Decorates]`. |
| 2 | No startup reflection on the generated path | Registrations are `typeof`-based descriptors. Decorators use generated factories with direct `new` calls (no `ActivatorUtilities`). |
| 3 | Reflection is explicit | `AddServicesFromAssemblies` and open-generic `Decorate(Type, Type)` carry `[RequiresUnreferencedCode]` / `[RequiresDynamicCode]`. |
| 4 | Coexistence | The default strategy is `Add`. Generated code never removes or rewrites registrations it did not create, except under an explicit `Replace` strategy or decoration. |
| 5 | Deterministic | The same source always yields the same registrations in the same order. |

## 3. Packages, TFMs and Dependencies

| Project | TFMs | Dependencies | Packable |
|---|---|---|---|
| `CSharpEssentials.DependencyInjection` | `net11.0;net10.0;net9.0;net8.0;netstandard2.1` (no `netstandard2.0`) | `Microsoft.Extensions.DependencyInjection.Abstractions`, `Microsoft.Extensions.Logging.Abstractions` (duplicate debug log) | yes |
| `CSharpEssentials.DependencyInjection.Generators` | `netstandard2.0` | `Microsoft.CodeAnalysis.CSharp` `VersionOverride="4.8.0"`, `Microsoft.CodeAnalysis.Analyzers` | no — packed into the runtime nupkg at `analyzers/dotnet/cs` (ADR-006) |

- Both dependencies already exist in `Directory.Packages.props` (`[9.0.4,)`). Keyed services are available on `netstandard2.1` through `Microsoft.Extensions.DependencyInjection.Abstractions` 8+.
- **netstandard2.1 polyfills** (`internal`, compiled only for `netstandard2.1`): `RequiresUnreferencedCodeAttribute`, `RequiresDynamicCodeAttribute`, `DynamicallyAccessedMembersAttribute`, `DynamicallyAccessedMemberTypes`. Trim annotations are meaningful only on `net8.0+`.
- Generic attribute forms are compiled only for `NET7_0_OR_GREATER` TFMs. The `netstandard2.1` asset has the `typeof` forms only.
- Namespace: `CSharpEssentials.DependencyInjection`. Generated registries live in `Microsoft.Extensions.DependencyInjection`, so `services.Add{Asm}Services()` is discoverable without a `using`.

## 4. Public Contracts (#55)

### 4.1 Registration attributes

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class RegisterScopedAttribute : Attribute
{
    public RegisterScopedAttribute();
    public RegisterScopedAttribute(Type serviceType);

    public Type? ServiceType { get; }
    public object? Key { get; set; }
    public ServiceAs As { get; set; }
    public RegistrationStrategy Strategy { get; set; }   // default Add
}

public sealed class RegisterScopedAttribute<TService> : Attribute { /* same named properties */ }
```

`RegisterSingletonAttribute` / `RegisterSingletonAttribute<TService>` and `RegisterTransientAttribute` / `RegisterTransientAttribute<TService>` have the same shape.

- The `typeof` form is primary. The generic form is secondary (C# 11+, `net8.0+` assets only).
- `Key`: any attribute-legal constant (`string`, numeric, `char`, `bool`, enum, `Type`). `null` = non-keyed. Generated code emits the constant with its original type (`global::Ns.Region.Eu`, `"primary"`, `42`).
- `AllowMultiple = true`: one class may register under several lifetimes, keys or service types.
- No `[Service(Lifetime)]` variant. The lifetime is in the attribute name, so it is visible at a glance and needs no extra argument.

### 4.2 `ServiceAs`

```csharp
public enum ServiceAs
{
    Self,
    SelfWithInterfaces,
    ImplementedInterfaces,
}
```

- `As` is honored only when it is set explicitly as a named argument. The generator and the fallback read the named arguments. An unset `As` means "use the default resolution" (§5.1).
- Interface expansion (`SelfWithInterfaces`, `ImplementedInterfaces`) uses all implemented interfaces except: `System.IDisposable`, `System.IAsyncDisposable`, `System.IEquatable<T>`, `System.IComparable<T>`, `System.Collections.IEnumerable`.
- `SelfWithInterfaces`: the concrete type is registered with the attribute's lifetime. Each interface is registered as a forwarding factory (`sp => sp.GetRequiredService<Impl>()`, or the keyed equivalent), so singleton and scoped instances are shared.
- `ImplementedInterfaces`: each interface gets its own descriptor with the implementation type, so there is one instance per service type for scoped and singleton lifetimes. Use `SelfWithInterfaces` to share the instance.
- An explicit service type combined with `As` registers the union of both (explicit first).

### 4.3 `RegistrationStrategy`

```csharp
public enum RegistrationStrategy
{
    Add,
    TryAdd,
    TryAddEnumerable,
    Replace,
    Throw,
}
```

All strategies are **key-aware**: a match means the same `ServiceType` **and** an equal `ServiceKey` (`null` matches only `null`).

| Strategy | Behavior |
|---|---|
| `Add` (default) | Always appends. |
| `TryAdd` | Appends if no descriptor matches (service, key). |
| `TryAddEnumerable` | Appends if no descriptor matches (service, key, implementation type). |
| `Replace` | Removes **all** descriptors matching (service, key), then appends. Unlike `ServiceCollectionDescriptorExtensions.Replace`, it is key-aware and removes every match, not only the first. |
| `Throw` | Throws `InvalidOperationException` (service, key, existing implementation) if a descriptor matches. A `Throw` registration that follows a registration of the same (service, key) within one compilation is reported at compile time (CSE2002); registration order is metadata type name, then attribute order. |

### 4.4 `DecoratesAttribute`

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class DecoratesAttribute(Type serviceType) : Attribute
{
    public Type ServiceType { get; } = serviceType;
    public int Order { get; set; }          // default 0, lower = applied first = innermost
    public object? Key { get; set; }        // decorate the keyed registration
}

public sealed class DecoratesAttribute<TService> : Attribute { /* Order, Key */ }
```

- The decorator must implement the service type (CSE2001). It must have exactly one public constructor (CSE2005) with exactly one parameter of the decorated service type (CSE2004).
- A `[Decorates]` class is constructed only by its generated factory and is not registered as a service.
- Ties in `Order` are broken by the decorator's fully qualified name.

### 4.5 `ExcludeFromRegistrationAttribute`

`AttributeTargets.Class | AttributeTargets.Assembly`. On a class, the class is neither registered nor applied as a decorator, whatever other attributes it has. On an assembly, no registry or `ServiceModule` attribute is generated, and the fallback skips the assembly.

### 4.6 Registry-level attributes (same pattern as Endpoints)

| Attribute | Purpose |
|---|---|
| `ServiceModuleAttribute(Type registryType)` | Generated, `[assembly:]`. Marks an assembly that contains a registry and is read by the aggregate generator. |
| `ServiceRegistryNameAttribute(string name)` | `[assembly:]`. Overrides `{Asm}`: `ServiceRegistryName("Billing")` → `AddBillingServices`. |
| `GenerateServiceAggregateAttribute` | `[assembly:]` opt-in for `AddAllServices` in libraries and test projects. |
| `DisableServiceAggregateAttribute` | `[assembly:]` opt-out of the automatic `AddAllServices` in `Exe`/`WinExe`. |

### 4.7 Runtime decoration API

```csharp
public static class ServiceCollectionDecorationExtensions
{
    public static IServiceCollection Decorate<TService, TDecorator>(this IServiceCollection services, object? serviceKey = null)
        where TService : class
        where TDecorator : class, TService;

    public static bool TryDecorate<TService, TDecorator>(this IServiceCollection services, object? serviceKey = null)
        where TService : class
        where TDecorator : class, TService;

    public static IServiceCollection Decorate<TService>(
        this IServiceCollection services,
        Func<TService, IServiceProvider, TService> decorator,
        object? serviceKey = null)
        where TService : class;

    [RequiresDynamicCode("Closes the open-generic decorator for each matching closed registration.")]
    [RequiresUnreferencedCode("Closes the open-generic decorator for each matching closed registration; its constructors may be trimmed.")]
    public static IServiceCollection Decorate(this IServiceCollection services, Type serviceType, Type decoratorType);
}
```

- **Hidden inner registration:** each matched descriptor is re-registered under a **private sentinel key**. Its service type is the implementation type for implementation-type originals, the instance's runtime type for instance originals, and a private marker class for factory originals, because MS.DI requires the implementation or instance to be assignable to the service type. The key is a fresh instance of a private sealed class per decorated descriptor, compared by reference. Implementation type, factory or instance and the lifetime are preserved. The decorator descriptor takes the original's place, with the same service type, key and lifetime.
- **No leak:** the hidden registration's service type is not `TService`, so it never appears in `GetServices<TService>()`, `GetKeyedServices<TService>(key)` or `GetKeyedServices<TService>(KeyedService.AnyKey)`. The sentinel type is private, so no user code can construct or obtain the key. `GetKeyedServices<object>(KeyedService.AnyKey)` does not return hidden inners either. Only `GetKeyedServices<TImplementation>(KeyedService.AnyKey)` on the concrete implementation type returns them, which code that resolves services by their contracts does not do. The marker type is private and no runtime `MakeGenericType` is needed, so the path stays AOT-safe.
- **Multiple registrations:** every descriptor matching (service, key) is decorated, so enumeration returns decorated instances in the original order.
- **Keyed services** are decoratable through `serviceKey`. Original implementations that inject `[ServiceKey]` cannot be decorated (they would receive the sentinel), so `Decorate` throws `InvalidOperationException`. This is detected through the trim-annotated `ImplementationType` constructors.
- **No partial mutation:** all matches are collected and validated first, and the collection is changed only after validation succeeds. On failure, `services` is unchanged.
- `Decorate*` throws `InvalidOperationException` when nothing matches. `TryDecorate` returns `false` instead.
- `Decorate<TService, TDecorator>` constructs `TDecorator` through its single public constructor, which is trim-annotated with `[DynamicallyAccessedMembers(PublicConstructors)]`. The generated path never uses it, because it emits direct `new` factories via the `Func<,>` overload.
- **Open generics:** `Decorate(Type, Type)` decorates every **closed** registration whose service type is constructed from `serviceType`. Open-generic registrations (`typeof(IRepo<>)` → `typeof(Repo<>)`) cannot be decorated by MS.DI without a constant key. They are reported in the exception (no partial mutation).
- Disposal: the container owns both the hidden inner and the decorator, and disposes both. `ValidateOnBuild` and `ValidateScopes` pass, because the hidden registration is a normal descriptor.

### 4.8 Reflection fallback

```csharp
[RequiresUnreferencedCode("Scans assemblies for registration attributes; use the generated Add{Asm}Services for trimmed/AOT apps.")]
[RequiresDynamicCode("Builds decorator factories and closes generic types at runtime.")]
public static IServiceCollection AddServicesFromAssemblies(this IServiceCollection services, params Assembly[] assemblies);

[RequiresUnreferencedCode("Scans assemblies for registration attributes; use the generated Add{Asm}Services for trimmed/AOT apps.")]
[RequiresDynamicCode("Builds decorator factories and closes generic types at runtime.")]
public static IServiceCollection AddServicesFromAssemblies(this IServiceCollection services, ILogger? logger, params Assembly[] assemblies);
```

It uses the same rules as the generator (§5): resolution, strategies, forwarding, decorators last ordered by `Order`. Attributes are read through `CustomAttributeData`, so an explicitly set `As` is detected. All assemblies' registrations are applied before any decorator, in the order the assemblies are passed. Decorators are sorted globally by `Order`, then by the position of their assembly, then by type name, which matches the aggregate. `ReflectionTypeLoadException` is handled and never swallowed: without a logger, an `InvalidOperationException` listing the loader errors is thrown before `services` changes; with a logger, a `Warning` (event 2002) lists the loader errors and the loadable types are processed. The logger also receives the duplicate `Debug` log (§5.5). An open-generic `[Decorates]` class is applied through `Decorate(Type, Type)` restricted to the attribute's `Key`. Invalid types that the analyzer would report as errors throw `InvalidOperationException` (fail fast, because this path has no compile-time check).

## 5. Semantics

### 5.1 Default service resolution

Applied when neither an explicit service type nor `As` is given:

1. **Explicit:** `typeof(T)` / `<T>` / `As` → exactly those.
2. **`I{TypeName}`, arity-aware:** an implemented interface whose name is `I` + the class name and whose generic arity equals the class's arity (`Repository<T>` → `IRepository<T>`, `Clock` → `IClock`). Namespace does not matter. If several match, all are registered, ordered by fully qualified name.
3. **Self:** otherwise the class registers as itself. If the class implements at least one interface outside the exclusion list and none matched step 2, the analyzer reports **CSE2003** (info).

### 5.2 Open generics

- `[RegisterScoped(typeof(IRepository<>))] sealed class Repository<T> : IRepository<T>` → `ServiceDescriptor.Scoped(typeof(IRepository<>), typeof(Repository<>))`. Keys are supported.
- The class's type parameters must map one-to-one, in order, onto the service's type parameters. Otherwise, the mapping is invalid for MS.DI and reported as CSE2001.
- Default resolution applies with arity matching. `As = ImplementedInterfaces` and `As = SelfWithInterfaces` consider only open interfaces satisfying the one-to-one rule. Forwarding is not possible for open generics, so each gets its own descriptor.
- Open-generic decorators on the generated path → CSE2008 (use runtime `Decorate(Type, Type)`).

### 5.3 Order of operations in `Add{Asm}Services`

1. Registrations in deterministic order (by implementation fully qualified name, then attribute order), each through its strategy.
2. Decorators, **last**, sorted by `Order` then fully qualified name, each through `Decorate<TService>(factory, key)` with a generated factory.

Decorators may target services registered manually or by other modules, as long as those registrations exist when the decorator is applied. Otherwise, `Decorate` throws with the decorator name. The aggregate (§6.3) applies all modules' registrations before any decorators, which covers cross-assembly decoration.

### 5.4 Coexistence with manual registrations

Under `Add` (default), a generated registration and a manual `services.AddScoped<IFoo, Foo>()` both stay in the collection. `GetService<IFoo>()` resolves the **last** one added, and `GetServices<IFoo>()` returns both. Call order therefore decides the winner. This is the documented behavior during gradual migration. To prevent it, use `TryAdd` or `Throw` on the attribute, or the "no duplicate (service, key)" test helper described in the README (#57).

### 5.5 Duplicate visibility (debug log)

`Add{Asm}Services(this IServiceCollection services, ILogger? logger = null)`: when `logger` is not null and `Debug` is enabled, every registration whose (service, key) already exists in the collection is logged at `Debug` (service, key, existing implementation, new implementation, strategy). The default strategy remains `Add`. Logging goes through a `LoggerMessage`-generated method in the runtime package (`ServiceRegistrationLog`), which is AOT-safe. When the logger is null or `Debug` is disabled, the duplicate scan does not run at all.

## 6. Generated Code (#56)

### 6.1 Discovery

- `ForAttributeWithMetadataName` per attribute metadata name (`RegisterScopedAttribute`, `` RegisterScopedAttribute`1 ``, and so on, and `DecoratesAttribute` / `` DecoratesAttribute`1 ``). Value-equatable models only (ADR-006).
- Types with analyzer **errors** are skipped by the generator. The error comes from the analyzer, not from generated code.
- If nothing is registered or decorated, no registry and no `ServiceModule` attribute are emitted.

### 6.2 Per-assembly registry

```csharp
namespace Microsoft.Extensions.DependencyInjection;

public static class {Asm}ServiceRegistry
{
    public static IServiceCollection Add{Asm}Services(this IServiceCollection services, ILogger? logger = null);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void RegisterServices(IServiceCollection services, ILogger? logger);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static IReadOnlyList<int> DecoratorOrders { get; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void ApplyDecorators(IServiceCollection services);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void ApplyDecorators(IServiceCollection services, int order);
}

[assembly: CSharpEssentials.DependencyInjection.ServiceModule(typeof(Microsoft.Extensions.DependencyInjection.{Asm}ServiceRegistry))]
```

`{Asm}` sanitization is the same as in Endpoints (§5.2 of the Endpoints design). `Add{Asm}Services` = `RegisterServices` + `ApplyDecorators`. `ApplyDecorators(services)` calls `ApplyDecorators(services, order)` for each value of `DecoratorOrders` (the distinct `Order` values of the assembly, ascending).

### 6.3 Aggregate `AddAllServices`

```csharp
internal static class {Asm}ServiceAggregate
{
    internal static IServiceCollection AddAllServices(this IServiceCollection services, ILogger? logger = null);
}
```

Same rules as `MapAllEndpoints`:
- `internal`.
- Auto-generated in `Exe`/`WinExe` when `IsTestProject` is not `true`. `[assembly: DisableServiceAggregate]` opts out, and `[assembly: GenerateServiceAggregate]` opts in libraries and test projects.
- Referenced assemblies are filtered by name (skip `System*`, `Microsoft*`, `mscorlib`, `netstandard`, and assemblies not referencing `CSharpEssentials.DependencyInjection`) before `ServiceModule` attributes are read.
- Modules are deduplicated, so there is no duplicate registration.
- Phases: **all** `RegisterServices` (referenced modules by assembly name, own module last), then decorators across all modules: for each distinct `Order` in the union of every module's `DecoratorOrders` (ascending), `ApplyDecorators(services, order)` of each module in the same module order. `Order` is global; within one `Order`, module order and then type name decide, so the host's decorators are outermost among equal orders.

### 6.4 Sample generated code

Input (assembly `Sample.Billing`):

```csharp
public interface IInvoiceService { Task<Invoice> GetAsync(Guid id); }

[RegisterScoped]
public sealed class InvoiceService(IInvoiceRepository repository) : IInvoiceService { /* ... */ }

[RegisterSingleton(As = ServiceAs.SelfWithInterfaces)]
public sealed class SystemClock : IClock, IDisposable { /* ... */ }

[RegisterScoped(typeof(IPaymentGateway), Key = "stripe", Strategy = RegistrationStrategy.TryAdd)]
public sealed class StripeGateway : IPaymentGateway { /* ... */ }

[Decorates(typeof(IInvoiceService), Order = 1)]
public sealed class CachedInvoiceService(IInvoiceService inner, IMemoryCache cache) : IInvoiceService { /* ... */ }
```

Output `SampleBillingServiceRegistry.g.cs`:

```csharp
// <auto-generated/>
#nullable enable
[assembly: global::CSharpEssentials.DependencyInjection.ServiceModule(typeof(global::Microsoft.Extensions.DependencyInjection.SampleBillingServiceRegistry))]

namespace Microsoft.Extensions.DependencyInjection
{
    [global::System.CodeDom.Compiler.GeneratedCode("CSharpEssentials.DependencyInjection.Generators", "4.1.0")]
    public static class SampleBillingServiceRegistry
    {
        public static global::Microsoft.Extensions.DependencyInjection.IServiceCollection AddSampleBillingServices(
            this global::Microsoft.Extensions.DependencyInjection.IServiceCollection services,
            global::Microsoft.Extensions.Logging.ILogger? logger = null)
        {
            RegisterServices(services, logger);
            ApplyDecorators(services);
            return services;
        }

        [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
        public static void RegisterServices(
            global::Microsoft.Extensions.DependencyInjection.IServiceCollection services,
            global::Microsoft.Extensions.Logging.ILogger? logger)
        {
            global::CSharpEssentials.DependencyInjection.ServiceRegistration.Apply(services, logger,
                global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor.Scoped(
                    typeof(global::Sample.Billing.IInvoiceService), typeof(global::Sample.Billing.InvoiceService)),
                global::CSharpEssentials.DependencyInjection.RegistrationStrategy.Add);

            global::CSharpEssentials.DependencyInjection.ServiceRegistration.Apply(services, logger,
                new global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor(
                    typeof(global::Sample.Billing.IPaymentGateway), "stripe", typeof(global::Sample.Billing.StripeGateway),
                    global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped),
                global::CSharpEssentials.DependencyInjection.RegistrationStrategy.TryAdd);

            global::CSharpEssentials.DependencyInjection.ServiceRegistration.Apply(services, logger,
                global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor.Singleton(
                    typeof(global::Sample.Billing.SystemClock), typeof(global::Sample.Billing.SystemClock)),
                global::CSharpEssentials.DependencyInjection.RegistrationStrategy.Add);
            global::CSharpEssentials.DependencyInjection.ServiceRegistration.Apply(services, logger,
                global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor.Singleton<global::Sample.Billing.IClock>(
                    static sp => global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions
                        .GetRequiredService<global::Sample.Billing.SystemClock>(sp)),
                global::CSharpEssentials.DependencyInjection.RegistrationStrategy.Add);
        }

        [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
        public static global::System.Collections.Generic.IReadOnlyList<int> DecoratorOrders { get; } = new int[] { 0 };

        [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
        public static void ApplyDecorators(global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)
        {
            foreach (int order in DecoratorOrders)
            {
                ApplyDecorators(services, order);
            }
        }

        [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
        public static void ApplyDecorators(global::Microsoft.Extensions.DependencyInjection.IServiceCollection services, int order)
        {
            switch (order)
            {
                case 0:
                    global::Microsoft.Extensions.DependencyInjection.ServiceCollectionDecorationExtensions.Decorate<global::Sample.Billing.IInvoiceService>(
                        services,
                        static (inner, sp) => new global::Sample.Billing.CachedInvoiceService(
                            inner,
                            global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions
                                .GetRequiredService<global::Microsoft.Extensions.Caching.Memory.IMemoryCache>(sp)),
                        serviceKey: null);
                    break;
            }
        }
    }
}
```

`ServiceRegistration.Apply(IServiceCollection, ILogger?, ServiceDescriptor, RegistrationStrategy)` is the public, `[EditorBrowsable(Never)]` runtime helper that implements the key-aware strategies and the duplicate log. The fallback uses it too. Decorator constructor parameters are resolved as follows: the decorated service → `inner`; `[FromKeyedServices(k)]` → `GetRequiredKeyedService`; `[ServiceKey]` → the decorated key; parameters with defaults → `GetService` with a fallback to the default; others → `GetRequiredService`. `IDisposable` is excluded from `SystemClock`'s interface expansion. Snapshot tests pin the exact text.

## 7. Diagnostics (#56)

Reported by `DependencyInjectionAnalyzer` (`DiagnosticAnalyzer`) in `CSharpEssentials.DependencyInjection.Generators`, never by the generator (ADR-006). Category `CSharpEssentials.DependencyInjection`.

| ID | Rule | Severity | Rationale |
|---|---|---|---|
| CSE2001 | Service type not implemented by the class (also covers `[Decorates]` targets and invalid open-generic mappings) | Error | MS.DI would throw at registration or resolution |
| CSE2002 | `Throw` registration whose (service, key) an earlier registration in the same compilation already registers | Error | Certain runtime failure |
| CSE2003 | Class implements interfaces but none matched the default rule, so it is registered as self | Info | Deliberate fallback, made visible |
| CSE2004 | Decorator has no usable constructor parameter of the decorated service | Error | Generated factory would not compile |
| CSE2005 | Decorator has multiple public constructors | Error | Generated factory needs exactly one |
| CSE2006 | Captive dependency: a singleton depends on a scoped/transient service, or a scoped service on a transient one. Single-constructor, non-generic classes only; matches attribute registrations in the same compilation by service type and `[FromKeyedServices]` key, including open-generic registrations for closed parameter types | Info | The dependency lives longer than its registration intends. Reported on the consumer's attribute at compilation end. |
| CSE2007 | Registration or decoration attribute on an abstract or static class | Error | Cannot be constructed |
| CSE2008 | Open-generic decorator on the generated path; use runtime `Decorate(Type, Type)` | Warning | Not generated. The user must register it at runtime. |

Every ID gets a positive and a negative test and an entry in `AnalyzerReleases.Unshipped.md`.

## 8. P3 Follow-ups (#58, optional)

- Shipped: CSE2006 captive dependency (info, single-constructor types, §7).
- Shipped: code fix for CSE2003 in `CSharpEssentials.DependencyInjection.CodeFixes` (netstandard2.0, `Microsoft.CodeAnalysis.CSharp.Workspaces` 4.8.0, packed into `analyzers/dotnet/cs`). One action per implemented service interface adds `typeof(IFoo)` (`typeof(IFoo<>)` for generic classes), and one adds `As = ServiceAs.Self`. The analyzer assembly does not reference Workspaces (RS1038).

## 9. Explicitly Skipped Scrutor Features

| Scrutor feature | Reason skipped |
|---|---|
| `Scan(s => s.FromAssemblyOf<T>().AddClasses(...))` fluent scanning | Runtime reflection. Replaced by attribute opt-in + generator. Reflection exists only as `AddServicesFromAssemblies`. |
| Convention-based "register all classes" | Violates opt-in. Third-party types would be touched. |
| `FromApplicationDependencies` / `DependencyContext` scanning | Replaced by the `ServiceModule` aggregate, which reads compile-time references only. |
| `AsMatchingInterface(Action<...>)` with arbitrary naming | One deterministic rule: `I{TypeName}`, arity-aware. Anything else is explicit `typeof`. |
| `AsImplementedInterfaces(predicate)` | Fixed exclusion list. Use explicit `typeof` for finer control. |
| `[ServiceDescriptor]` / `[Service(Lifetime)]` single attribute | Lifetime-named attributes are clearer and need no enum argument. |
| `RegistrationStrategy.Replace(ReplacementBehavior.ImplementationType)` | `Replace` matches (service, key) only. Implementation-based replacement is rarely correct and is not key-aware in Scrutor. |
| `WithAttributes` / namespace filters (`InNamespaceOf`) | `[ExcludeFromRegistration]` on class or assembly. |
| Decoration via `ActivatorUtilities` | Generated factories (`new`) on the generated path, which are AOT-safe. |
| Decorating open-generic registrations | MS.DI cannot inject the hidden inner without a constant key. Only closed registrations are decorated (§4.7). |

## 10. Testing Strategy

| Layer | Tests |
|---|---|
| Strategies (#55) | Every strategy with and without keys; `Replace` removes all matches; `Throw` message |
| Decoration (#55) | Order; multiple registrations; keyed decoration; disposal of decorator + inner; `ValidateOnBuild` + `ValidateScopes`; **sentinel does not leak** via `GetKeyedServices<T>(KeyedService.AnyKey)`, `GetKeyedServices<object>(KeyedService.AnyKey)` or `GetServices<T>()`; no partial mutation on failure; `TryDecorate` returns `false`; `[ServiceKey]` original rejected; open-generic `Decorate(Type, Type)` on closed registrations |
| Coexistence (#55) | Manual `AddScoped` + generated `Add`: last wins, both enumerated |
| Fallback (#55) | `AddServicesFromAssemblies` parity with generated registry |
| Generator snapshots (#56) | Lifetimes, keys (string/enum/int/Type), `As` variants, forwarding, open generics, decorators ordering, empty assembly, `ServiceRegistryName`, aggregate on/off (Exe, library, test project, opt-in, opt-out) |
| Incremental caching | Second run → tracked steps `Cached`/`Unchanged` |
| Aggregate | Two fixture assemblies (`CSharpEssentials.Tests.Fixtures.DependencyInjectionA/B`); cross-assembly decorator; no duplicate registration |
| Debug log (#56) | Duplicate (service, key) logged at `Debug`; nothing logged when the logger is null |
| Analyzer (#56, #58) | Positive and negative per ID (CSE2001–2008) |
| AOT (#57) | `examples/Examples.Endpoints` uses `Add{Asm}Services` and publishes with zero trim/AOT warnings |

Test location: `CSharpEssentials.Tests/DependencyInjection/` and `CSharpEssentials.Tests/Generators/`. xUnit + FluentAssertions, `Method_Should_Behavior`.

## 11. Implementation Deviations

| Deviation | Reason |
|---|---|
| `Decorate(Type, Type)` also carries `[RequiresUnreferencedCode]` | Closing the decorator with `MakeGenericType` and reading its constructors cannot be statically annotated for trimming, so the trim analyzer warns unless the caller is told. |
| `AddServicesFromAssemblies(ILogger?, params Assembly[])` overload | §4.8 asks for loader failures to be "thrown or logged". The overload makes the choice explicit: no logger throws, a logger warns and continues. It also carries the duplicate debug log of §5.5 to the fallback. |
| `SelfWithInterfaces` with an explicit service type forwards the explicit type too | §4.2 defines the union but not whether the explicit type shares the instance. Forwarding keeps one instance per scope for every service type of the registration, which is what `SelfWithInterfaces` promises. |
| Open-generic `Decorate(Type, Type)` decorates closed registrations of every key | The API has no key parameter. Each registration keeps its own key, and the decorator receives that key through `[ServiceKey]`. The fallback restricts it to the attribute's `Key`. |
| CSE2007 also covers file-local types, types nested in a `private`/`protected` type, and types nested in a generic type | Generated code lives in another file and namespace and cannot name these types, so the generator would emit code that does not compile. Reporting them keeps the rule "the error comes from the analyzer, not from generated code" (§6.1). |
| CSE2005 also reports a decorator with zero public constructors | The generated factory needs exactly one public constructor, and the runtime activator rejects zero as well. |
| CSE2006 also reports a scoped service that depends on a transient one | The transient instance is held for the whole scope, which is the same capture problem one level down. Registrations made outside attributes (manual `services.Add*`, other assemblies) are not visible to the analyzer, so the rule stays best effort and `Info`. |
| A generated decorator with nothing to decorate does not name the decorator type in its exception | Generated code uses the `Decorate<TService>(Func<TService, IServiceProvider, TService>, serviceKey)` overload so it needs no reflection; that overload has no decorator type to report. The service type and key are still named. |
| The `[ServiceKey]` argument is emitted as `(T)(object)key`, or `default(T)!` for a non-keyed decorator | The key constant and the parameter type can differ (for example an `object` parameter or a boxed enum). The double cast compiles for every combination the runtime activator accepts. |
| The analyzer reports nothing in an assembly marked `[ExcludeFromRegistration]` | The generator emits no registry for such an assembly, so its declarations cannot fail at registration. |
| Fallback parity is tested on descriptor sets (service type, key, lifetime, implementation type), not on order | The fallback scans types in metadata order and the generator in ordinal name order. Registration order differs only between unrelated service types, which the container does not observe. |
