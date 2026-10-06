# CSharpEssentials.DependencyInjection

Attribute-based service registration and decoration for `Microsoft.Extensions.DependencyInjection`. Mark a class, and it is registered with key-aware strategies. Decorate any registration, keyed or not, without touching its consumers.

## Installation

```bash
dotnet add package CSharpEssentials.DependencyInjection
```

Target frameworks: `net11.0`, `net10.0`, `net9.0`, `netstandard2.1`. The generic attribute forms (`[RegisterScoped<TService>]`, `[Decorates<TService>]`) need a `net7.0` or later target; on `netstandard2.1` use the `typeof(...)` forms. The source generator, analyzer and code fix are bundled in the package.

## Registering services

```csharp
using CSharpEssentials.DependencyInjection;

[RegisterScoped]                                   // IOrderService, by the I{TypeName} rule
public sealed class OrderService : IOrderService { }

[RegisterSingleton(typeof(IClock))]                // explicit service type
public sealed class SystemClock : IClock { }

[RegisterSingleton<IPaymentGateway>(Key = "stripe")] // generic form (C# 11+), keyed
public sealed class StripeGateway : IPaymentGateway { }

[RegisterTransient(typeof(IRepository<>))]         // open generics
public sealed class Repository<T> : IRepository<T> { }
```

Without a service type and without `As`, a class registers as the implemented interface named `I{TypeName}` (same generic arity), otherwise as itself.

| Property | Values |
|---|---|
| `Key` | Any attribute constant. `null` registers a non-keyed service. |
| `As` | `Self`, `SelfWithInterfaces` (interfaces forward to one shared instance), `ImplementedInterfaces`. |
| `Strategy` | `Add` (default), `TryAdd`, `TryAddEnumerable`, `Replace`, `Throw`. All are key-aware. |

`[ExcludeFromRegistration]` on a class or an assembly opts out.

With `Add`, a generated registration and a manual `services.AddScoped<IOrderService, OrderService>()` both stay in the collection: `GetService<T>()` resolves the last one added and `GetServices<T>()` returns both. Use `TryAdd` or `Throw` on the attribute, or the test below, when that is not what you want.

## Decorating services

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
```

Attribute decorators are applied in ascending `Order`. Every matching registration is decorated, and its lifetime and key are kept. The original is moved to a hidden registration under a private key and a service type other than `T` and `object`, so it never shows up in `GetServices<T>()`, `GetKeyedServices<T>(KeyedService.AnyKey)` or `GetKeyedServices<object>(KeyedService.AnyKey)`. The container disposes both the decorator and the original. If decoration fails, the collection is left unchanged.

## Generated registry

The package ships a source generator. Each assembly with marked classes gets an `Add{Assembly}Services` method (dots removed, so `Sample.Billing` becomes `AddSampleBillingServices`). It registers the assembly's services, then applies its decorators. It uses no reflection, so it is trim and Native AOT safe.

```csharp
builder.Services.AddSampleBillingServices();
builder.Services.AddSampleBillingServices(logger); // logs duplicate (service, key) registrations at Debug
```

Applications (`Exe`/`WinExe`, not test projects) also get an internal `AddAllServices()`. It registers the services of every referenced assembly that has a generated registry, and of the application itself, before it applies any decorator, so a decorator can wrap a service from another assembly.

| Assembly attribute | Effect |
|---|---|
| `[assembly: ServiceRegistryName("Billing")]` | Renames the method to `AddBillingServices`. |
| `[assembly: GenerateServiceAggregate]` | Generates `AddAllServices()` in a library or test project. |
| `[assembly: DisableServiceAggregate]` | Turns off `AddAllServices()` in an application. |

The analyzer reports invalid declarations at compile time:

| ID | Severity | Rule |
|---|---|---|
| CSE2001 | Error | The class does not implement the service type. |
| CSE2002 | Error | Two registrations of the same service type and key, one with `RegistrationStrategy.Throw`. |
| CSE2003 | Info | The class has interfaces, but none matches `I{TypeName}`, so it is registered as itself. |
| CSE2004 | Error | The decorator has no constructor parameter of the decorated type, or more than one. |
| CSE2005 | Error | The decorator does not have exactly one public constructor. |
| CSE2006 | Info | Captive dependency: a singleton takes a scoped or transient service (or a scoped one takes a transient) registered by attribute in the same project. Checked for single-constructor classes only. |
| CSE2007 | Error | The class cannot be constructed by generated code (abstract, static, file-local, inaccessible, or nested in a generic type). |
| CSE2008 | Warning | Open-generic decorators are not generated; call `services.Decorate(typeof(IRepository<>), typeof(CachedRepository<>))`, which decorates the closed registrations. |

The bundled code fix for CSE2003 offers one action per implemented interface, which adds `typeof(IFoo)` (`typeof(IFoo<>)` for a generic class) to the attribute, and one action that adds `As = ServiceAs.Self` to keep the registration as is and silence the diagnostic.

## Reflection fallback

```csharp
services.AddServicesFromAssemblies(typeof(Program).Assembly);
services.AddServicesFromAssemblies(logger, typeof(Program).Assembly); // warns on unloadable types, logs duplicates at Debug
```

The fallback uses reflection, so it is marked `[RequiresUnreferencedCode]` and `[RequiresDynamicCode]`. Use the generated `Add{Assembly}Services()` method for trimmed and Native AOT applications.

## Testing for duplicate registrations

The default `Add` strategy allows two registrations of the same service and key. A unit test catches unintended duplicates. Only the registry under test goes into the collection; decorated originals are kept under private keys, so they never count as duplicates.

```csharp
public static class ServiceCollectionAssertions
{
    public static void ShouldHaveNoDuplicateRegistrations(this IServiceCollection services, params Type[] allowMultiple)
    {
        List<string> duplicates = services
            .Where(descriptor => !allowMultiple.Contains(descriptor.ServiceType))
            .GroupBy(descriptor => (descriptor.ServiceType, descriptor.ServiceKey))
            .Where(group => group.Count() > 1)
            .Select(group => $"{group.Key.ServiceType} (key: {group.Key.ServiceKey ?? "none"}) registered {group.Count()} times")
            .ToList();

        if (duplicates.Count > 0)
        {
            throw new InvalidOperationException("Duplicate registrations:" + Environment.NewLine + string.Join(Environment.NewLine, duplicates));
        }
    }
}

// In a test ([Fact] for xUnit, [Test] for NUnit):
public void Services_Should_NotRegisterTheSameServiceTwice()
{
    var services = new ServiceCollection();
    services.AddSampleBillingServices();

    services.ShouldHaveNoDuplicateRegistrations(allowMultiple: typeof(INotificationChannel));
}
```

Pass service types that are meant to have several implementations (`TryAddEnumerable` registrations, for example) as `allowMultiple`.

## Migrating from Scrutor

Scrutor scans assemblies at runtime. This package replaces scanning with an attribute on each class and a generated, reflection-free registry.

| Scrutor | CSharpEssentials.DependencyInjection |
|---|---|
| `services.Scan(s => s.FromAssemblyOf<T>().AddClasses(...)...)` | `[RegisterScoped]`/`[RegisterSingleton]`/`[RegisterTransient]` on each class, then `services.Add{Assembly}Services()` |
| `.AsMatchingInterface()` | Default rule: the interface named `I{TypeName}` |
| `.As<IFoo>()` | `[RegisterScoped(typeof(IFoo))]` or `[RegisterScoped<IFoo>]` |
| `.AsSelf()` / `.AsSelfWithInterfaces()` / `.AsImplementedInterfaces()` | `As = ServiceAs.Self` / `ServiceAs.SelfWithInterfaces` / `ServiceAs.ImplementedInterfaces` |
| `.WithScopedLifetime()` | The attribute name carries the lifetime |
| `RegistrationStrategy.Append` / `Skip` / `Replace()` / `Throw` | `Strategy = RegistrationStrategy.Add` / `TryAdd` / `Replace` / `Throw` (all key-aware) |
| `[ServiceDescriptor(typeof(IFoo), ServiceLifetime.Scoped)]` with `.UsingAttributes()` | `[RegisterScoped(typeof(IFoo))]` |
| `services.Decorate<IFoo, FooDecorator>()` | Same call, or `[Decorates(typeof(IFoo))]` on the decorator; both also work for keyed services |
| `services.TryDecorate<IFoo, FooDecorator>()` | Same call, returns `bool` |
| `.FromApplicationDependencies()` | `AddAllServices()`, generated in applications from compile-time references |
| Namespace or type filters | `[ExcludeFromRegistration]` on a class or an assembly |

Steps:

1. Add the attributes to the classes the `Scan` call registered. Build: CSE2003 points at classes whose interfaces do not follow `I{TypeName}`, and the code fix adds the explicit service type.
2. Replace the `Scan` call with `services.Add{Assembly}Services()`, or `AddAllServices()` in the application.
3. Move `Decorate` calls to `[Decorates]` attributes or keep them as runtime calls.
4. Remove the Scrutor package. Both libraries define `Decorate` and `TryDecorate` in the `Microsoft.Extensions.DependencyInjection` namespace, so the calls are ambiguous while both are referenced.

Not supported: convention-based "register every class", custom interface-matching predicates, and decorating open-generic registrations (closed registrations can be decorated with `Decorate(Type, Type)`).
