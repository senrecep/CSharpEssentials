# CSharpEssentials.DependencyInjection

Attribute-based service registration and decoration for `Microsoft.Extensions.DependencyInjection`. Mark a class, and it is registered with key-aware strategies. Decorate any registration, keyed or not, without touching its consumers.

## Installation

```bash
dotnet add package CSharpEssentials.DependencyInjection
```

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

## Decorating services

```csharp
[Decorates(typeof(IOrderService), Order = 1)]
public sealed class LoggingOrderService(IOrderService inner, ILogger<LoggingOrderService> logger) : IOrderService { }
```

Or at runtime:

```csharp
services.Decorate<IOrderService, LoggingOrderService>();
services.Decorate<IPaymentGateway, RetryingGateway>(serviceKey: "stripe");
services.Decorate<IClock>((inner, sp) => new CachedClock(inner));
services.TryDecorate<IOrderService, AuditingOrderService>();
```

Every matching registration is decorated, and its lifetime and key are kept. The original is moved to a hidden registration under a private key, so it never shows up in `GetServices<T>()` or `GetKeyedServices<T>(KeyedService.AnyKey)`. The container disposes both the decorator and the original. If decoration fails, the collection is left unchanged.

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
| CSE2008 | Warning | Open-generic decorators are not generated; call `services.Decorate(typeof(IRepository<>), typeof(CachedRepository<>))`. |

## Reflection fallback

```csharp
services.AddServicesFromAssemblies(typeof(Program).Assembly);
services.AddServicesFromAssemblies(logger, typeof(Program).Assembly); // warns on unloadable types, logs duplicates at Debug
```

The fallback uses reflection, so it is marked `[RequiresUnreferencedCode]` and `[RequiresDynamicCode]`. Use the generated `Add{Assembly}Services()` method for trimmed and Native AOT applications.
