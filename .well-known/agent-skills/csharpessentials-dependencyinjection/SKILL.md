---
name: csharpessentials-dependencyinjection
description: Use when registering services by attribute instead of manual AddScoped calls or Scrutor scanning — [RegisterScoped]/[RegisterSingleton]/[RegisterTransient] (Key, As = ServiceAs.*, Strategy = RegistrationStrategy.*), [Decorates] and services.Decorate/TryDecorate, [ExcludeFromRegistration], the source-generated services.Add{Assembly}Services()/AddAllServices() registry, the AddServicesFromAssemblies reflection fallback, and CSE2001–CSE2008 diagnostics.
---

# CSharpEssentials.DependencyInjection

Attribute-based service registration with a source-generated, reflection-free registry (trim and Native AOT safe), plus decorators.

## Installation

```bash
dotnet add package CSharpEssentials.DependencyInjection
```

Target frameworks: `net11.0`, `net10.0`, `net9.0`, `netstandard2.1`. The generic attribute forms (`[RegisterScoped<TService>]`, `[Decorates<TService>]`) need `net7.0` or later; on `netstandard2.1` use the `typeof(...)` forms. The generator, analyzer and code fix ship in the package.

## Namespace

```csharp
using CSharpEssentials.DependencyInjection;
```

---

## Registering Services

```csharp
[RegisterScoped]                                     // IOrderService, by the I{TypeName} rule
public sealed class OrderService : IOrderService { }

[RegisterSingleton(typeof(IClock))]                  // explicit service type
public sealed class SystemClock : IClock { }

[RegisterSingleton<IPaymentGateway>(Key = "stripe")] // generic form, keyed
public sealed class StripeGateway : IPaymentGateway { }

[RegisterTransient(typeof(IRepository<>))]           // open generic
public sealed class Repository<T> : IRepository<T> { }

[RegisterScoped(As = ServiceAs.SelfWithInterfaces, Strategy = RegistrationStrategy.TryAdd)]
public sealed class ReportBuilder : IDisposable { public void Dispose() { } }
```

Without a service type and without `As`, a class registers as its interface named `I{TypeName}` (same generic arity), otherwise as itself.

| Property | Values |
|---|---|
| `Key` | Any attribute constant. `null` = non-keyed. |
| `As` | `Self`, `SelfWithInterfaces` (interfaces forward to one shared instance), `ImplementedInterfaces` |
| `Strategy` | `Add` (default), `TryAdd`, `TryAddEnumerable`, `Replace`, `Throw`. All are key-aware. |

- `[ExcludeFromRegistration]` on a class or an assembly opts out.
- With `Add`, a generated registration and a manual `AddScoped<IOrderService, OrderService>()` both stay: `GetService<T>()` resolves the last one, `GetServices<T>()` returns both. Use `TryAdd` or `Throw` when that is not wanted.

---

## Decorators

```csharp
[Decorates(typeof(IOrderService), Order = 1)]
public sealed class LoggingOrderService(IOrderService inner, ILogger<LoggingOrderService> logger) : IOrderService { }
```

Or at runtime:

```csharp
services.Decorate<IOrderService, LoggingOrderService>();             // throws InvalidOperationException when nothing matches
services.Decorate<IPaymentGateway, RetryingGateway>(serviceKey: "stripe");
services.Decorate<IClock>((inner, sp) => new CachedClock(inner));
services.TryDecorate<IOrderService, AuditingOrderService>();         // returns false when nothing matches
services.Decorate(typeof(IRepository<>), typeof(CachedRepository<>)); // open generic: decorates closed registrations
```

- Attribute decorators apply in ascending `Order`. Every matching registration is decorated; lifetime and key are kept.
- The original moves to a hidden private key under a service type other than `T` and `object`, so it never shows up in `GetServices<T>()`, `GetKeyedServices<T>(KeyedService.AnyKey)` or `GetKeyedServices<object>(KeyedService.AnyKey)`. The container disposes both.
- A decorator needs exactly one public constructor with exactly one parameter of the decorated type.

---

## Generated Registry

Each assembly with marked classes gets `services.Add{Assembly}Services()` (dots removed: `Sample.Billing` → `AddSampleBillingServices()`), plus an overload taking an `ILogger` that logs duplicate (service, key) registrations at `Debug`. It registers the assembly's services, then applies its decorators.

Applications (`Exe`/`WinExe`, not test projects) also get an internal `AddAllServices()`, which registers every referenced assembly's registry and the app's own before applying any decorator, so a decorator can wrap a service from another assembly.

| Assembly attribute | Effect |
|---|---|
| `[assembly: ServiceRegistryName("Billing")]` | Names the method `AddBillingServices()` |
| `[assembly: GenerateServiceAggregate]` | Generates `AddAllServices()` in a library or test project |
| `[assembly: DisableServiceAggregate]` | Turns off `AddAllServices()` in an application |

### Reflection Fallback

For assemblies that cannot use the generator (plugins loaded at runtime):

```csharp
services.AddServicesFromAssemblies(typeof(OrderService).Assembly);
```

Marked `[RequiresUnreferencedCode]` and `[RequiresDynamicCode]`. An overload taking an `ILogger` first warns on unloadable types. Prefer the generated method in trimmed and AOT apps.

---

## Diagnostics

| ID | Severity | Rule |
|---|---|---|
| CSE2001 | Error | Class does not implement the service type |
| CSE2002 | Error | Two registrations of the same service and key, one with `RegistrationStrategy.Throw` |
| CSE2003 | Info | Class has interfaces but none matches `I{TypeName}`, so it registers as itself. Code fix adds `typeof(IFoo)` or `As = ServiceAs.Self` |
| CSE2004 | Error | Decorator has zero or several constructor parameters of the decorated type |
| CSE2005 | Error | Decorator does not have exactly one public constructor |
| CSE2006 | Info | Captive dependency: a singleton takes a scoped/transient service (or scoped takes transient) registered by attribute in the same project |
| CSE2007 | Error | Class cannot be constructed by generated code (abstract, static, file-local, inaccessible, nested in a generic type) |
| CSE2008 | Warning | Open-generic decorators are not generated; call `services.Decorate(typeof(IRepository<>), typeof(CachedRepository<>))` |

---

## Migrating from Scrutor

| Scrutor | CSharpEssentials.DependencyInjection |
|---|---|
| `services.Scan(s => s.FromAssemblyOf<T>().AddClasses(...))` | `[RegisterScoped]`/`[RegisterSingleton]`/`[RegisterTransient]` on each class, then `services.Add{Assembly}Services()` |
| `.AsMatchingInterface()` | Default `I{TypeName}` rule |
| `.As<IFoo>()` | `[RegisterScoped(typeof(IFoo))]` or `[RegisterScoped<IFoo>]` |
| `.AsSelf()` / `.AsSelfWithInterfaces()` / `.AsImplementedInterfaces()` | `As = ServiceAs.Self` / `SelfWithInterfaces` / `ImplementedInterfaces` |
| `.WithScopedLifetime()` | The attribute name carries the lifetime |
| `RegistrationStrategy.Append` / `Skip` / `Replace()` / `Throw` | `Strategy = RegistrationStrategy.Add` / `TryAdd` / `Replace` / `Throw` |
| `Decorate` / `TryDecorate` | Same calls (also keyed), or `[Decorates(typeof(IFoo))]` |
| `.FromApplicationDependencies()` | `AddAllServices()` |
| Namespace or type filters | `[ExcludeFromRegistration]` |

Build after adding attributes: CSE2003 flags classes whose interfaces do not follow `I{TypeName}`. Remove Scrutor afterwards; both define `Decorate`/`TryDecorate` in `Microsoft.Extensions.DependencyInjection`, so calls are ambiguous while both are referenced.

---

## Best Practices

- Prefer the generated `Add{Assembly}Services()` / `AddAllServices()` over `AddServicesFromAssemblies`.
- Use `Strategy = RegistrationStrategy.Throw` or `TryAdd` for services that must have one registration.
- Treat CSE2006 (captive dependency) as a bug even though it is Info.
- Add a unit test that groups `IServiceCollection` by `(ServiceType, ServiceKey)` to catch unintended duplicates.
