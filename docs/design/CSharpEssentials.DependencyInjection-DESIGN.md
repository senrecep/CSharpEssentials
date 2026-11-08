# CSharpEssentials.DependencyInjection: design document

> **Date:** 2026-10-06 | **Status:** Implemented in 4.1, kept current through 7.0
> **Issues:** #48 (epic), #49 (docs), #50 (infra), #55 (attributes + runtime), #56 (generator + analyzer), #57 (docs/AOT example), #58 (P3)
> **Related:** [ADR-006](../adr/ADR-006-source-generators-in-separate-projects.md), [Endpoints design](CSharpEssentials.Endpoints-DESIGN.md)

---

## 1. Goals and non-goals

### Goals
- Replace Scrutor for attribute-based registration and decoration on top of `Microsoft.Extensions.DependencyInjection`.
- Compile-time registration through a generated `Add{Asm}Services()`. Trim/AOT clean, with no startup scanning.
- Deterministic service-type resolution, key-aware strategies and keyed decoration.
- Gradual migration: manual `services.Add*` calls and generated registrations coexist.

### Non-goals
- No container replacement and no new lifetimes. The output is plain `ServiceDescriptor`s.
- No convention-based "register everything" scanning.
- No touching third-party registrations (source-generated Mediator, MassTransit consumers, and so on). Only marked types are registered.
- No interception/proxy generation.

## 2. Principles

| # | Principle | Consequence |
|---|---|---|
| 1 | Opt-in only | A type is registered only if it carries `[RegisterScoped]`, `[RegisterSingleton]` or `[RegisterTransient]`, and decorated only via `[Decorates]`. |
| 2 | No startup scanning on the generated path | Registrations are `typeof`-based descriptors. Decorators use generated factories with direct `new` calls (no `ActivatorUtilities`). The only reflection left is a cached constructor-metadata read before decorating (§4.7). |
| 3 | Reflection is explicit | `AddServicesFromAssemblies` and open-generic `Decorate(Type, Type)` carry `[RequiresUnreferencedCode]` / `[RequiresDynamicCode]`. |
| 4 | Coexistence | The default strategy is `Add`. Generated code never removes or rewrites registrations it did not create, except under an explicit `Replace` strategy or decoration. |
| 5 | Deterministic | The same source always yields the same registrations in the same order. |

## 3. Packages, TFMs and dependencies

| Project | TFMs | Dependencies | Packable |
|---|---|---|---|
| `CSharpEssentials.DependencyInjection` | `net11.0;net10.0;net9.0;net8.0;netstandard2.1` (no `netstandard2.0`) | `Microsoft.Extensions.DependencyInjection.Abstractions`, `Microsoft.Extensions.Logging.Abstractions` (duplicate debug log), both `[9.0.4,)` | yes |
| `CSharpEssentials.DependencyInjection.Generators` | `netstandard2.0` | `Microsoft.CodeAnalysis.CSharp` `VersionOverride="4.8.0"`, `Microsoft.CodeAnalysis.Analyzers` (`build/Generators.props`) | no, packed into the runtime nupkg at `analyzers/dotnet/cs` (ADR-006) |
| `CSharpEssentials.DependencyInjection.CodeFixes` | `netstandard2.0` | `Microsoft.CodeAnalysis.CSharp.Workspaces` `VersionOverride="4.8.0"` | no, packed at `analyzers/dotnet/cs` |

- Keyed services are available on `netstandard2.1` through `Microsoft.Extensions.DependencyInjection.Abstractions` 8+.
- netstandard2.1 polyfills (`internal`, compiled only for `netstandard2.1`): `RequiresUnreferencedCodeAttribute`, `RequiresDynamicCodeAttribute`, `DynamicallyAccessedMembersAttribute`, `DynamicallyAccessedMemberTypes`. Trim annotations are meaningful only on `net8.0+`, where `IsAotCompatible` is set.
- Generic attribute forms are compiled only for `NET7_0_OR_GREATER` TFMs. The `netstandard2.1` asset has the `typeof` forms only.
- Namespace: `CSharpEssentials.DependencyInjection`. Generated registries live in `Microsoft.Extensions.DependencyInjection`, so `services.Add{Asm}Services()` is discoverable without a `using`.

## 4. Public contracts (#55)

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

public sealed class RegisterScopedAttribute<TService> : Attribute { /* ServiceType => typeof(TService); Key, As, Strategy */ }
```

`RegisterSingletonAttribute` / `RegisterSingletonAttribute<TService>` and `RegisterTransientAttribute` / `RegisterTransientAttribute<TService>` have the same shape.

- The `typeof` form is primary. The generic form needs C# 11 and a `net7.0+` asset.
- `Key`: any attribute-legal constant (`string`, numeric, `char`, `bool`, enum, `Type`). `null` = non-keyed. Generated code emits the constant with its original type (`global::Ns.Region.Eu`, `"primary"`, `42`).
- `AllowMultiple = true`: one class may register under several lifetimes, keys or service types.
- No `[Service(Lifetime)]` variant: the lifetime is in the attribute name, visible at a glance.

### 4.2 `ServiceAs`

```csharp
public enum ServiceAs
{
    Self,
    SelfWithInterfaces,
    ImplementedInterfaces,
}
```

- `As` is honored only when it is set explicitly as a named argument. An unset `As` means the default resolution (§5.1).
- Interface expansion (`SelfWithInterfaces`, `ImplementedInterfaces`) uses all implemented interfaces except `System.IDisposable`, `System.IAsyncDisposable`, `System.IEquatable<T>`, `System.IComparable<T>` and `System.Collections.IEnumerable`.
- `SelfWithInterfaces`: the concrete type is registered with the attribute's lifetime, and each interface is a forwarding factory (`sp => sp.GetRequiredService<Impl>()`, or the keyed equivalent), so singleton and scoped instances are shared.
- `ImplementedInterfaces`: each interface gets its own descriptor with the implementation type, so scoped and singleton lifetimes get one instance per service type.
- An explicit service type combined with `As` registers the union of both (explicit first). With `SelfWithInterfaces`, the explicit type is forwarded too, so every service type of the registration shares the instance.

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

All strategies are key-aware: a match means the same `ServiceType` and an equal `ServiceKey` (`null` matches only `null`).

| Strategy | Behavior |
|---|---|
| `Add` (default) | Always appends. |
| `TryAdd` | Appends if no descriptor matches (service, key). |
| `TryAddEnumerable` | Appends if no descriptor matches (service, key, implementation). |
| `Replace` | Removes all descriptors matching (service, key), then appends. Unlike `ServiceCollectionDescriptorExtensions.Replace`, it is key-aware and removes every match, not only the first. |
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

public sealed class DecoratesAttribute<TService> : Attribute { /* ServiceType, Order, Key */ }
```

- The decorator must implement the service type (CSE2001). It must have exactly one public constructor (CSE2005, which also covers zero) with exactly one parameter of the decorated service type (CSE2004).
- A `[Decorates]` class is constructed only by its generated factory and is not registered as a service.
- Ties in `Order` are broken by the decorator's fully qualified name.

### 4.5 `ExcludeFromRegistrationAttribute`

`AttributeTargets.Class | AttributeTargets.Assembly`. On a class, the class is neither registered nor applied as a decorator, whatever other attributes it has. On an assembly, no registry or `ServiceModule` attribute is generated, the fallback skips the assembly, and the analyzer reports nothing (no declaration there can fail at registration).

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
    public static IServiceCollection Decorate<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TDecorator>(
        this IServiceCollection services, object? serviceKey = null)
        where TService : class
        where TDecorator : class, TService;

    public static bool TryDecorate<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TDecorator>(
        this IServiceCollection services, object? serviceKey = null)
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

- Hidden inner registration: each matched descriptor is re-registered under a private key, a fresh instance of a private sealed class per decorated descriptor, compared by reference. Its service type is the implementation type for implementation-type originals, the instance's runtime type for instance originals, and a private marker class for factory originals, because MS.DI requires the implementation or instance to be assignable to the service type. The implementation, factory or instance is preserved. The lifetime is preserved too, except that a scoped original gets a transient inner: the decorator factory runs once per scope and resolves the inner from that scope, so the scope still creates and disposes one inner per cached decorator without caching it a second time. The decorator descriptor takes the original's place, with the same service type, key and lifetime.
- No leak: the hidden registration's service type is neither `TService` nor `object`, so it never appears in `GetServices<TService>()`, `GetKeyedServices<TService>(key)`, `GetKeyedServices<TService>(KeyedService.AnyKey)` or `GetKeyedServices<object>(KeyedService.AnyKey)`. Only `GetKeyedServices<TImplementation>(KeyedService.AnyKey)` on the concrete type returns it. The key type is private, so no user code can obtain the key, and no runtime `MakeGenericType` is needed.
- Multiple registrations: every descriptor matching (service, key) is decorated, so enumeration returns decorated instances in the original order.
- Keyed services are decoratable through `serviceKey`. An original implementation that injects `[ServiceKey]` would receive the private key, so `Decorate` throws `InvalidOperationException`. The check reads the implementation type's public constructors once per type per process; the result is cached in a `ConditionalWeakTable`, which does not keep collectible types alive. Generated decorators pass the implementation types the generator already verified (`DecoratorRegistration.VerifiedImplementations`: non-generic, non-partial types registered by attribute in the same assembly under the same service type and key), and those are not read at all.
- No partial mutation: all matches are collected and validated first, and the collection changes only after validation succeeds.
- `Decorate*` throws `InvalidOperationException` when nothing matches. `TryDecorate` returns `false` instead.
- `Decorate<TService, TDecorator>` constructs `TDecorator` through its single public constructor (a `ConstructorInvoker` on net8+), kept by the `[DynamicallyAccessedMembers]` annotation. The generated path does not use it (§6.2).
- Open generics: `Decorate(Type, Type)` decorates every closed registration whose service type is constructed from `serviceType`. The API has no key parameter, so it decorates closed registrations of every key; each keeps its key and the decorator receives it through `[ServiceKey]`. It also carries `[RequiresUnreferencedCode]`, because closing the decorator with `MakeGenericType` and reading its constructors cannot be annotated statically. Open-generic registrations (`typeof(IRepo<>)` → `typeof(Repo<>)`) cannot be decorated by MS.DI without a constant key; they are reported in the exception, with no partial mutation.
- Disposal: the container owns and disposes both the hidden inner and the decorator. `ValidateOnBuild` and `ValidateScopes` pass, because the hidden registration is a normal descriptor.

### 4.8 Reflection fallback

```csharp
[RequiresUnreferencedCode("Scans assemblies for registration attributes; use the generated Add{Asm}Services for trimmed/AOT apps.")]
[RequiresDynamicCode("Builds decorator factories and closes generic types at runtime.")]
public static IServiceCollection AddServicesFromAssemblies(this IServiceCollection services, params Assembly[] assemblies);

[RequiresUnreferencedCode(...)]
[RequiresDynamicCode(...)]
public static IServiceCollection AddServicesFromAssemblies(this IServiceCollection services, ILogger? logger, params Assembly[] assemblies);
```

- Same rules as the generator (§5): resolution, strategies, forwarding, decorators last ordered by `Order`. Attributes are read through `CustomAttributeData`, so an explicitly set `As` is detected.
- All assemblies' registrations are applied before any decorator, in the order the assemblies are passed. Decorators are sorted globally by `Order`, then by the position of their assembly, then by type name, which matches the aggregate.
- `ReflectionTypeLoadException`: the `ILogger?` overload makes the choice explicit. Without a logger, an `InvalidOperationException` listing the loader errors is thrown before `services` changes. With a logger, a `Warning` (event 2002) lists the loader errors and the loadable types are processed. The logger also receives the duplicate `Debug` log (§5.5).
- An open-generic `[Decorates]` class is applied through `Decorate(Type, Type)` restricted to the attribute's `Key`.
- Invalid types that the analyzer would report as errors throw `InvalidOperationException`, because this path has no compile-time check. A decorator with zero public constructors gets a message that names trimming and the AOT-safe alternatives, since in a trimmed app the trimmer removes constructors that only the scan uses. Other callers of the activator get the usual message, because outside the scanner zero public constructors is a declaration error.

## 5. Semantics

### 5.1 Default service resolution

1. Explicit: `typeof(T)` / `<T>` / `As` → exactly those.
2. `I{TypeName}`, arity-aware: an implemented interface whose name is `I` + the class name and whose generic arity equals the class's arity (`Repository<T>` → `IRepository<T>`, `Clock` → `IClock`). Namespace does not matter. If several match, all are registered, ordered by fully qualified name.
3. Self: otherwise the class registers as itself. If the class implements at least one interface outside the exclusion list and none matched step 2, the analyzer reports CSE2003 (info).

### 5.2 Open generics

- `[RegisterScoped(typeof(IRepository<>))] sealed class Repository<T> : IRepository<T>` → `ServiceDescriptor.Scoped(typeof(IRepository<>), typeof(Repository<>))`. Keys are supported.
- The class's type parameters must map one-to-one, in order, onto the service's type parameters. Otherwise the mapping is invalid for MS.DI and reported as CSE2001.
- Default resolution applies with arity matching. `As = ImplementedInterfaces` and `As = SelfWithInterfaces` consider only open interfaces satisfying the one-to-one rule. Forwarding is not possible for open generics, so each gets its own descriptor.
- Open-generic decorators on the generated path → CSE2008 (use runtime `Decorate(Type, Type)`).

### 5.3 Order of operations in `Add{Asm}Services`

1. Registrations in deterministic order (by implementation metadata name, then attribute order), each through its strategy.
2. Decorators, last: for each distinct `Order` ascending, one `ServiceRegistration.Decorate(services, DecoratorRegistration[])` call with every decorator of that order, sorted by fully qualified name.

One pass over the collection finds the registrations of every decorator of that order. The result equals consecutive decorate calls in declaration order (a later decorator of the same service wraps an earlier one, and the first failing decorator throws the same exception), but every decorator of the order is validated before any registration is replaced, so a failure leaves the collection unchanged.

Decorators may target services registered manually or by other modules, as long as those registrations exist when the decorator is applied; otherwise `Decorate` throws. The generated exception names the service type and key but not the decorator type, because a `DecoratorRegistration` carries only the service type, key, typed factory and verified implementation types. Because `Add{Asm}Services` applies its decorators immediately, per-assembly calls are order-sensitive: `AddAuditServices()` decorating a service from `Billing` throws when it runs before `AddBillingServices()`. Callers register dependencies first or use the aggregate (§6.3). Decorators are not deferred, so a missing registration fails at startup instead of silently leaving a service undecorated.

### 5.4 Coexistence with manual registrations

Under `Add` (default), a generated registration and a manual `services.AddScoped<IFoo, Foo>()` both stay in the collection. `GetService<IFoo>()` resolves the last one added, and `GetServices<IFoo>()` returns both, so call order decides the winner. To prevent it, use `TryAdd` or `Throw` on the attribute, or the duplicate-registration test in the README (#57).

### 5.5 Duplicate visibility (debug log)

`Add{Asm}Services(this IServiceCollection services, ILogger? logger = null)`: when `logger` is not null and `Debug` is enabled, every registration whose (service, key) already exists is logged at `Debug` (service, key, existing implementation, new implementation, strategy) through a `LoggerMessage`-generated method (`ServiceRegistrationLog`). When the logger is null or `Debug` is disabled, the duplicate scan does not run.

## 6. Generated code (#56)

### 6.1 Discovery

- One `ForAttributeWithMetadataName` provider per attribute metadata name (`RegisterScopedAttribute`, `` RegisterScopedAttribute`1 ``, and so on, plus `DecoratesAttribute` / `` DecoratesAttribute`1 ``), eight in all. Value-equatable models only (ADR-006).
- Types with analyzer errors are skipped by the generator, so the error comes from the analyzer, not from generated code.
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

- `{Asm}` sanitization is the same as in Endpoints (§5.2 of the Endpoints design).
- `Add{Asm}Services` = `RegisterServices` + `ApplyDecorators`, guarded by `ServiceRegistration.IsRegistered(services, typeof({Asm}ServiceRegistry).Assembly)`. When the assembly is already marked, it returns without changes. Otherwise it registers, decorates and then calls `ServiceRegistration.MarkRegistered`, which adds a singleton marker descriptor of an internal type. The marker is written only after registrations and decorators succeed, so an exception (for example a `Throw` conflict) leaves the assembly unmarked and the next call retries. The marker counts in `services.Count` and appears when enumerating, but nothing resolves it.
- `RegisterServices` and `ApplyDecorators` are unguarded building blocks for the aggregate. `ApplyDecorators(services)` calls `ApplyDecorators(services, order)` for each value of `DecoratorOrders` (the distinct `Order` values of the assembly, ascending).
- Decorator entries are `static readonly` `DecoratorRegistration[]` fields, one per order, built once per registry type.

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
- Modules are deduplicated. Registries of different assemblies whose names collide after sanitizing (`Foo.Api`, `FooApi`) are skipped, because the aggregate cannot refer to an ambiguous type. A referenced registry with the same name as the own registry is skipped too (the compiler resolves the name to the own type). CSE2009 reports both cases. The analyzer counts the own registry only when the project has at least one registration or decorator the generator emits.
- Phases: all `RegisterServices` (referenced modules by assembly name, own module last), then, for each distinct `Order` in the union of every module's `DecoratorOrders` (ascending), `ApplyDecorators(services, order)` of each module in the same module order. `Order` is global; within one `Order`, module order and then type name decide, so the host's decorators are outermost among equal orders.
- Idempotent: modules already registered (by `Add{Asm}Services`, an earlier `AddAllServices`, or `AddServicesFromAssemblies`) take part in neither phase. The remaining modules are marked only after both phases succeed. Registration is not transactional, so descriptors added before an exception stay in the collection and a retry can add them again, and one failing registry leaves the modules that succeeded unmarked as well. Startup fails in that case anyway; the guidance is to fix the cause and build a new collection. The reflection fallback (§4.8) checks and sets the same marker per assembly; its scan validates every type before mutating the collection.

### 6.4 Sample generated code

Input (assembly `Sample.Billing`):

```csharp
public interface IInvoiceService { Task<Invoice> GetAsync(Guid id); }

[RegisterScoped]
public sealed class InvoiceService(IInvoiceRepository repository) : IInvoiceService { /* ... */ }

[RegisterSingleton(As = ServiceAs.SelfWithInterfaces)]
public sealed class SystemClock : IClock, IDisposable { /* ... */ }

[RegisterScoped(typeof(IPaymentGateway), Key = "card", Strategy = RegistrationStrategy.TryAdd)]
public sealed class CardGateway : IPaymentGateway { /* ... */ }

[Decorates(typeof(IInvoiceService), Order = 1)]
public sealed class CachedInvoiceService(IInvoiceService inner, IMemoryCache cache) : IInvoiceService { /* ... */ }
```

Output `SampleBillingServiceRegistry.g.cs` (XML doc comments omitted):

```csharp
// <auto-generated/>
#nullable enable
[assembly: global::CSharpEssentials.DependencyInjection.ServiceModule(typeof(global::Microsoft.Extensions.DependencyInjection.SampleBillingServiceRegistry))]

namespace Microsoft.Extensions.DependencyInjection
{
    [global::System.CodeDom.Compiler.GeneratedCode("CSharpEssentials.DependencyInjection.Generators", "7.0.0.0")]
    public static class SampleBillingServiceRegistry
    {
        public static global::Microsoft.Extensions.DependencyInjection.IServiceCollection AddSampleBillingServices(
            this global::Microsoft.Extensions.DependencyInjection.IServiceCollection services,
            global::Microsoft.Extensions.Logging.ILogger? logger = null)
        {
            if (global::CSharpEssentials.DependencyInjection.ServiceRegistration.IsRegistered(services, typeof(SampleBillingServiceRegistry).Assembly))
            {
                return services;
            }

            RegisterServices(services, logger);
            ApplyDecorators(services);
            global::CSharpEssentials.DependencyInjection.ServiceRegistration.MarkRegistered(services, typeof(SampleBillingServiceRegistry).Assembly);
            return services;
        }

        [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
        public static void RegisterServices(
            global::Microsoft.Extensions.DependencyInjection.IServiceCollection services,
            global::Microsoft.Extensions.Logging.ILogger? logger)
        {
            global::CSharpEssentials.DependencyInjection.ServiceRegistration.Apply(
                services,
                logger,
                new global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor(typeof(global::Sample.Billing.IPaymentGateway), "card", typeof(global::Sample.Billing.CardGateway), global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped),
                global::CSharpEssentials.DependencyInjection.RegistrationStrategy.TryAdd);
            global::CSharpEssentials.DependencyInjection.ServiceRegistration.Apply(
                services,
                logger,
                global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor.Scoped(typeof(global::Sample.Billing.IInvoiceService), typeof(global::Sample.Billing.InvoiceService)),
                global::CSharpEssentials.DependencyInjection.RegistrationStrategy.Add);
            global::CSharpEssentials.DependencyInjection.ServiceRegistration.Apply(
                services,
                logger,
                global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor.Singleton(typeof(global::Sample.Billing.SystemClock), typeof(global::Sample.Billing.SystemClock)),
                global::CSharpEssentials.DependencyInjection.RegistrationStrategy.Add);
            global::CSharpEssentials.DependencyInjection.ServiceRegistration.Apply(
                services,
                logger,
                global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor.Singleton<global::Sample.Billing.IClock>(sp => global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<global::Sample.Billing.SystemClock>(sp)),
                global::CSharpEssentials.DependencyInjection.RegistrationStrategy.Add);
        }

        [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
        public static global::System.Collections.Generic.IReadOnlyList<int> DecoratorOrders { get; } = new int[] { 1 };

        private static readonly global::System.Type[] VerifiedImplementations0 = new global::System.Type[] { typeof(global::Sample.Billing.InvoiceService) };

        private static readonly global::CSharpEssentials.DependencyInjection.DecoratorRegistration[] Decorators0 = new global::CSharpEssentials.DependencyInjection.DecoratorRegistration[]
        {
            new global::CSharpEssentials.DependencyInjection.DecoratorRegistration(
                typeof(global::Sample.Billing.IInvoiceService),
                null,
                (inner, sp) => new global::Sample.Billing.CachedInvoiceService(
                    (global::Sample.Billing.IInvoiceService)inner,
                    global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<global::Microsoft.Extensions.Caching.Memory.IMemoryCache>(sp)),
                VerifiedImplementations0),
        };

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
                case 1:
                    global::CSharpEssentials.DependencyInjection.ServiceRegistration.Decorate(services, Decorators0);
                    break;
            }
        }
    }
}
```

`ServiceRegistration.Apply(IServiceCollection, ILogger?, ServiceDescriptor, RegistrationStrategy)` is the public, `[EditorBrowsable(Never)]` runtime helper that implements the key-aware strategies and the duplicate log; the fallback uses it too. `IDisposable` is excluded from `SystemClock`'s interface expansion. Snapshot tests pin the exact text.

Decorator constructor parameters are emitted as follows:
- the decorated service → `(TService)inner`;
- `[FromKeyedServices(k)]` → `GetRequiredKeyedService`, or `GetRequiredService` when `k` is `null` (`ServiceKeyLookupMode.NullKey`);
- the parameterless `[FromKeyedServices]` (.NET 10 `ServiceKeyLookupMode.InheritKey`) → the decorated key, or `GetRequiredService` for a non-keyed decorator;
- `[ServiceKey]` → `(T)(object)key`, or `default(T)!` for a non-keyed decorator. The double cast compiles for every key/parameter type pair the runtime activator accepts (for example an `object` parameter or a boxed enum);
- parameters with defaults → `GetService` with a fallback to the default;
- others → `GetRequiredService`.

## 7. Diagnostics (#56)

Reported by `DependencyInjectionAnalyzer` (`DiagnosticAnalyzer`) in `CSharpEssentials.DependencyInjection.Generators`, never by the generator (ADR-006). Category `CSharpEssentials.DependencyInjection`.

| ID | Rule | Severity | Rationale |
|---|---|---|---|
| CSE2001 | Service type not implemented by the class (also covers `[Decorates]` targets and invalid open-generic mappings) | Error | MS.DI would throw at registration or resolution |
| CSE2002 | `Throw` registration whose (service, key) an earlier registration in the same compilation already registers | Error | Certain runtime failure |
| CSE2003 | Class implements interfaces but none matched the default rule, so it is registered as self | Info | Deliberate fallback, made visible |
| CSE2004 | Decorator has no constructor parameter of the decorated service, or more than one | Error | Generated factory would not compile |
| CSE2005 | Decorator does not have exactly one public constructor (zero or several) | Error | Generated factory needs exactly one; the runtime activator rejects zero as well |
| CSE2006 | Captive dependency: a singleton depends on a scoped/transient service, or a scoped service on a transient one. Single-constructor, non-generic classes only; matches attribute registrations in the same compilation by service type and `[FromKeyedServices]` key (a parameterless `[FromKeyedServices]` inherits the consumer's own key; `[FromKeyedServices(null)]` is keyless), including open-generic registrations for closed parameter types | Info | The dependency lives longer than its registration intends. Registrations outside attributes are not visible, so the rule is best effort. Reported on the consumer's attribute at compilation end. |
| CSE2007 | Type cannot be constructed by generated code: abstract, static, file-local, nested in a `private`/`protected` type, or nested in a generic type | Error | Generated code lives in another file and namespace and cannot name these types |
| CSE2008 | Open-generic decorator on the generated path; use runtime `Decorate(Type, Type)` | Warning | Not generated. The user must apply it at runtime. |
| CSE2009 | Two registries, referenced or the project's own, have the same fully qualified name after sanitizing (`Foo.Api`, `FooApi`) | Warning | Reported only when the aggregate is generated. The aggregate skips the colliding referenced registries so the project compiles. When the own registry is part of the collision, only the own registry is called. |

Every ID has a positive and a negative test and an entry in `AnalyzerReleases.*.md`.

## 8. P3 follow-ups (#58)

- Shipped: CSE2006 captive dependency (§7).
- Shipped: code fix for CSE2003 in `CSharpEssentials.DependencyInjection.CodeFixes`. One action per implemented service interface adds `typeof(IFoo)` (`typeof(IFoo<>)` for generic classes), and one adds `As = ServiceAs.Self`. The analyzer assembly does not reference Workspaces (RS1038).

## 9. Explicitly skipped Scrutor features

| Scrutor feature | Reason skipped |
|---|---|
| `Scan(s => s.FromAssemblyOf<T>().AddClasses(...))` fluent scanning | Runtime reflection. Replaced by attribute opt-in + generator; reflection exists only as `AddServicesFromAssemblies`. |
| Convention-based "register all classes" | Violates opt-in; third-party types would be touched. |
| `FromApplicationDependencies` / `DependencyContext` scanning | Replaced by the `ServiceModule` aggregate, which reads compile-time references only. |
| `AsMatchingInterface(Action<...>)` with arbitrary naming | One deterministic rule: `I{TypeName}`, arity-aware. Anything else is explicit `typeof`. |
| `AsImplementedInterfaces(predicate)` | Fixed exclusion list. Use explicit `typeof` for finer control. |
| `[ServiceDescriptor]` / `[Service(Lifetime)]` single attribute | Lifetime-named attributes need no enum argument. |
| `RegistrationStrategy.Replace(ReplacementBehavior.ImplementationType)` | `Replace` matches (service, key) only. Implementation-based replacement is rarely correct and is not key-aware in Scrutor. |
| `WithAttributes` / namespace filters (`InNamespaceOf`) | `[ExcludeFromRegistration]` on class or assembly. |
| Decoration via `ActivatorUtilities` | Generated factories (`new`) on the generated path. |
| Decorating open-generic registrations | MS.DI cannot inject the hidden inner without a constant key. Only closed registrations are decorated (§4.7). |

## 10. Testing strategy

| Layer | Tests |
|---|---|
| Strategies (#55) | Every strategy with and without keys; `Replace` removes all matches; `Throw` message |
| Decoration (#55) | Order; multiple registrations; keyed decoration; disposal of decorator + inner; `ValidateOnBuild` + `ValidateScopes`; hidden key does not leak via `GetKeyedServices<T>(KeyedService.AnyKey)`, `GetKeyedServices<object>(KeyedService.AnyKey)` or `GetServices<T>()`; no partial mutation on failure; `TryDecorate` returns `false`; `[ServiceKey]` original rejected; open-generic `Decorate(Type, Type)` on closed registrations |
| Coexistence (#55) | Manual `AddScoped` + generated `Add`: last wins, both enumerated |
| Fallback (#55) | `AddServicesFromAssemblies` parity with the generated registry, compared on descriptor sets (service type, key, lifetime, implementation type), not on order: the fallback scans in metadata order and the generator in ordinal name order, which differs only between unrelated service types |
| Generator snapshots (#56) | Lifetimes, keys (string/enum/int/Type), `As` variants, forwarding, open generics, decorator ordering, empty assembly, `ServiceRegistryName`, aggregate on/off (Exe, library, test project, opt-in, opt-out) |
| Incremental caching | Second run → tracked steps `Cached`/`Unchanged` |
| Aggregate | Two fixture assemblies (`tests/CSharpEssentials.Tests.Fixtures.DependencyInjectionA/B`); cross-assembly decorator; no duplicate registration |
| Debug log (#56) | Duplicate (service, key) logged at `Debug`; nothing logged when the logger is null |
| Analyzer (#56, #58) | Positive and negative per ID (CSE2001 to CSE2009) |
| AOT (#57) | `examples/Examples.Endpoints` uses `AddAllServices()` and publishes with zero trim/AOT warnings |

Test location: `CSharpEssentials.Tests/DependencyInjection/` and `CSharpEssentials.Tests/Generators/`. xUnit + FluentAssertions, `Method_Should_Behavior`.
