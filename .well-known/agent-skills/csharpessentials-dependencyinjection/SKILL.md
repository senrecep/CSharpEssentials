---
name: csharpessentials-dependencyinjection
description: Use when registering services by attribute instead of manual AddScoped calls or Scrutor scanning. Covers [RegisterSingleton], [RegisterScoped], [RegisterTransient] (Key, As = ServiceAs.*, Strategy = RegistrationStrategy.*), [Decorates] and services.Decorate/TryDecorate, [ExcludeFromRegistration], the generated services.AddAllServices() and services.Add{Asm}Services() registry, AddServicesFromAssemblies, and diagnostics CSE2001 to CSE2009.
---

# CSharpEssentials.DependencyInjection

Attribute-based registration for `Microsoft.Extensions.DependencyInjection`, plus decorators. A source generator writes the registration code; nothing is scanned at startup, so it is trim and Native AOT safe.

## Install

```bash
dotnet add package CSharpEssentials.DependencyInjection
```

Targets `net11.0`, `net10.0`, `net9.0`, `net8.0`, `netstandard2.1`. Generic attributes (`[RegisterScoped<T>]`, `[Decorates<T>]`) need `net7.0`+; on `netstandard2.1` use `typeof(...)`. A console app also needs `Microsoft.Extensions.DependencyInjection` for `ServiceCollection`.

## Namespaces

```csharp
using CSharpEssentials.DependencyInjection;        // attributes, ServiceAs, RegistrationStrategy
using Microsoft.Extensions.DependencyInjection;    // Decorate, TryDecorate, AddServicesFromAssemblies, generated Add*Services
```

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials.DependencyInjection
// dotnet add package Microsoft.Extensions.DependencyInjection --version 10.0.1
using CSharpEssentials.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddAllServices();

using ServiceProvider provider = services.BuildServiceProvider();
using IServiceScope scope = provider.CreateScope();

IInvoiceService invoices = scope.ServiceProvider.GetRequiredService<IInvoiceService>();
Console.WriteLine(invoices.Create(100m));
Console.WriteLine(provider.GetRequiredKeyedService<IPaymentGateway>("card").Pay(5m));

public interface ITaxCalculator
{
    decimal Apply(decimal amount);
}

[RegisterSingleton]
public sealed class TaxCalculator : ITaxCalculator
{
    public decimal Apply(decimal amount) => amount * 1.2m;
}

public interface IInvoiceService
{
    string Create(decimal amount);
}

[RegisterScoped]
public sealed class InvoiceService(ITaxCalculator tax) : IInvoiceService
{
    public string Create(decimal amount) => $"invoice total {tax.Apply(amount)}";
}

[Decorates<IInvoiceService>]
public sealed class AuditedInvoiceService(IInvoiceService inner) : IInvoiceService
{
    public string Create(decimal amount) => "[audited] " + inner.Create(amount);
}

public interface IPaymentGateway
{
    string Pay(decimal amount);
}

[RegisterSingleton<IPaymentGateway>(Key = "card")]
public sealed class CardGateway : IPaymentGateway
{
    public string Pay(decimal amount) => $"card paid {amount}";
}
```

Prints `[audited] invoice total 120.0` and `card paid 5`.

## Registering a service

```csharp
[RegisterScoped]                                     // as IOrderService (the I{TypeName} rule)
public sealed class OrderService : IOrderService { }

[RegisterSingleton(typeof(ISystemClock))]            // explicit service type
public sealed class UtcClock : ISystemClock { }

[RegisterSingleton<IPaymentGateway>(Key = "card")]   // generic form, keyed
public sealed class CardGateway : IPaymentGateway { }

[RegisterTransient(typeof(IRepository<>))]           // open generic: pass the unbound type
public sealed class Repository<T> : IRepository<T> { }

[RegisterSingleton(As = ServiceAs.SelfWithInterfaces)]   // class + each interface share one instance
public sealed class MemoryCache : IReadCache, IWriteCache { }
```

- Lifetime comes from the attribute name. There is no `Lifetime` property.
- Default service type: the implemented interface named `I{TypeName}` with the same arity, otherwise the class itself (CSE2003 if it has other interfaces).
- `As`: `Self`, `SelfWithInterfaces` (interfaces forward to the class registration), `ImplementedInterfaces` (one registration per interface). With an explicit service type, `As` adds types. Expansion skips `IDisposable`, `IAsyncDisposable`, `IEquatable<T>`, `IComparable<T>`, `IEnumerable`.
- `Key`: any attribute constant; `null` (default) is non-keyed. Resolve with `GetRequiredKeyedService<T>(key)` or `[FromKeyedServices(key)]`.
- `Strategy`: `Add` (default, always appends), `TryAdd`, `TryAddEnumerable`, `Replace`, `Throw`. All compare service type and key.
- Attributes repeat: one class can have several registrations.
- `[ExcludeFromRegistration]` on a class or assembly opts out.

## Decorating a service

```csharp
[Decorates(typeof(IOrderService), Order = 1)]          // lower Order = applied first = innermost
public sealed class LoggingOrderService(IOrderService inner, ILogger<LoggingOrderService> logger) : IOrderService { }

[Decorates<IPaymentGateway>(Key = "card")]             // only registrations keyed "card"; no Key = non-keyed only
public sealed class RetryingGateway(IPaymentGateway inner) : IPaymentGateway { }
```

Runtime calls (namespace `Microsoft.Extensions.DependencyInjection`):

```csharp
services.Decorate<IOrderService, LoggingOrderService>();                 // throws if nothing matches
services.Decorate<IPaymentGateway, RetryingGateway>(serviceKey: "card");
services.Decorate<IClock>((inner, sp) => new CachedClock(inner));        // factory
bool done = services.TryDecorate<IOrderService, AuditingOrderService>(); // false if nothing matches
services.Decorate(typeof(IRepository<>), typeof(CachedRepository<>));    // closed registrations only
```

- Decorator: exactly one public constructor, exactly one parameter of the decorated type. Other parameters resolve from the container; `[ServiceKey]` gets the decorated key.
- Every matching registration is decorated and keeps its lifetime and key. The original is hidden and does not appear in `GetServices<T>()`.
- `Decorate` must run after the registration exists. `Decorate(Type, Type)` throws if the open generic itself is registered (`typeof(IRepository<>)`) or no closed registration matches; register closed types (`IRepository<Order>`) to decorate them. It is `[RequiresUnreferencedCode]` and `[RequiresDynamicCode]`, so keep it out of AOT apps; the generic `Decorate<TService, TDecorator>()` is trim safe.
- An original implementation that injects `[ServiceKey]` cannot be decorated.

## Calling the generated registry

```csharp
services.AddAllServices();                    // app projects: every referenced registry + own, then all decorators
services.AddSampleBillingServices();          // one assembly: assembly name, non-identifier chars removed, segments capitalized
services.AddSampleBillingServices(logger);    // optional ILogger logs duplicate (service, key) at Debug
```

- `AddAllServices(ILogger? logger = null)` is `internal`, generated only in `Exe`/`WinExe` non-test projects. `[assembly: GenerateServiceAggregate]` opts a library or test project in; `[assembly: DisableServiceAggregate]` opts out.
- `[assembly: ServiceRegistryName("Billing")]` renames the method to `AddBillingServices`.
- `Add{Asm}Services` applies its own decorators immediately. A decorator of a service from another assembly needs that assembly registered first; `AddAllServices` handles the order.
- Calls are idempotent per assembly; a repeat does nothing. The marker descriptor counts in `services.Count`.

## Plugins: reflection fallback

```csharp
services.AddServicesFromAssemblies(pluginAssembly);
services.AddServicesFromAssemblies(logger, pluginAssembly);   // warn and continue on unloadable types
```

`[RequiresUnreferencedCode]` and `[RequiresDynamicCode]`. Not for trimmed or AOT apps.

## Rules

- Call `AddAllServices()` in the application. In a library use `services.Add{Asm}Services()`; `AddAllServices` does not exist there without `[assembly: GenerateServiceAggregate]`.
- The method name comes from the assembly name: `My.App` gives `AddMyAppServices`, `my-app` gives `AddMyAppServices`.
- Do not write `[assembly: ServiceModule(...)]`; the generator emits it.
- Do not call the generated `RegisterServices`, `ApplyDecorators` or `ServiceRegistration`; they are plumbing. A `[Decorates]` class is not registered as a service.
- No open-generic `[Decorates]` (CSE2008). Use `services.Decorate(typeof(I<>), typeof(D<>))`.
- With the default `Add` strategy a manual `AddScoped<I, C>()` plus the attribute gives two registrations. Use `TryAdd` or `Throw` to prevent that.
- Remove Scrutor when migrating: both define `Decorate`/`TryDecorate` in the same namespace and calls become ambiguous.

## Diagnostics

| ID | Severity | Meaning | Fix |
|---|---|---|---|
| CSE2001 | Error | Class does not implement the service type | Implement it or change the type |
| CSE2002 | Error | `Strategy = Throw` registration duplicates an earlier one in the assembly (metadata name order) | Remove one or change the strategy |
| CSE2003 | Info | Has interfaces, none named `I{TypeName}`; registered as itself | Code fix adds `typeof(IFoo)` or `As = ServiceAs.Self` |
| CSE2004 | Error | Decorator has zero or several constructor parameters of the decorated type | Exactly one |
| CSE2005 | Error | Decorator does not have exactly one public constructor | Exactly one |
| CSE2006 | Info | Captive dependency (singleton takes scoped or transient, scoped takes transient) | Fix the lifetimes; treat as a bug |
| CSE2007 | Error | Class cannot be built by generated code (abstract, static, file-local, inaccessible, nested in a generic type) | Make it concrete and accessible |
| CSE2008 | Warning | Open-generic decorator not generated | `services.Decorate(typeof(I<>), typeof(D<>))` |
| CSE2009 | Warning | Two referenced assemblies give the same registry name; `AddAllServices` skips them | `[assembly: ServiceRegistryName("...")]` |
