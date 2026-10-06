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

## Reflection fallback

```csharp
services.AddServicesFromAssemblies(typeof(Program).Assembly);
services.AddServicesFromAssemblies(logger, typeof(Program).Assembly); // warns on unloadable types, logs duplicates at Debug
```

The fallback uses reflection, so it is marked `[RequiresUnreferencedCode]` and `[RequiresDynamicCode]`. Use the generated `Add{Assembly}Services()` method for trimmed and Native AOT applications.
