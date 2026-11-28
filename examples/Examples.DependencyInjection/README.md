# CSharpEssentials.DependencyInjection example

A console app that registers and decorates services with attributes, then resolves each one and prints what it got. It needs no external services.

## Running

```bash
dotnet run --project examples/Examples.DependencyInjection
```

Expected output:

```text
Registry: second AddShopServices call added 0 descriptors
Convention: IOrderService -> LoggingOrderService
Explicit type: IClock now = 12:00
SelfWithInterfaces: same instance = True
Keyed: card -> card 10 (with retry, key card)
Keyed: cash -> cash 10
TryAdd: IDiscountPolicy -> NoDiscountPolicy
TryAddEnumerable: 2 notifiers
Decorators: logged valid book at 12:00
TryDecorate with nothing registered: False
Open generic: counted products
```

## What each part shows

All service types are in `Shop.cs`; `Program.cs` registers and resolves them.

| Output line | Feature | Code |
|---|---|---|
| Registry | `[assembly: ServiceRegistryName("Shop")]` names the generated method `AddShopServices`. A second call adds nothing. | `Program.cs` |
| Convention | `[RegisterScoped]` on `OrderService` registers it as `IOrderService` by the `I{TypeName}` rule. | `OrderService` |
| Explicit type | `[RegisterSingleton(typeof(IClock))]`, then `Decorate<IClock>(factory)` adds three hours. | `FixedClock`, `OffsetClock` |
| SelfWithInterfaces | `As = ServiceAs.SelfWithInterfaces`: the class and both interfaces resolve to one singleton. | `PriceCache` |
| Keyed | `[RegisterSingleton<IPaymentGateway>(Key = "card")]` and a keyed `[Decorates<IPaymentGateway>(Key = "card")]` whose `[ServiceKey]` parameter receives `"card"`. The `cash` gateway is not decorated. | `CardGateway`, `CashGateway`, `RetryingGateway` |
| TryAdd | `Strategy = RegistrationStrategy.TryAdd` keeps the manual registration made before `AddShopServices`. | `DiscountPolicy` |
| TryAddEnumerable | Two implementations of `INotifier`, both kept. | `EmailNotifier`, `SmsNotifier` |
| Decorators | Two `[Decorates<IOrderService>]` with `Order = 1` (inner) and `Order = 2` (outer). | `ValidatingOrderService`, `LoggingOrderService` |
| TryDecorate | `TryDecorate` returns `false` when nothing is registered. | `BufferedAuditLog` |
| Open generic | `Decorate(typeof(IRepository<>), typeof(CountingRepository<>))` wraps the closed `IRepository<Product>` registration. | `ProductRepository`, `CountingRepository<T>` |

`LegacyOrderService` has `[ExcludeFromRegistration]`, so its `[RegisterSingleton]` is ignored.

The provider is built with `ValidateScopes` and `ValidateOnBuild`, so a lifetime mistake fails at startup.

In an application project you would normally call the generated `AddAllServices()` instead of the per-assembly method; it registers every referenced assembly's services before applying decorators. See the [package Readme](../../CSharpEssentials.DependencyInjection/Readme.MD).
