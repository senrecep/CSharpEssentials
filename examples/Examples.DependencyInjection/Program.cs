using CSharpEssentials.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Examples.DependencyInjection;

[assembly: ServiceRegistryName("Shop")]

var services = new ServiceCollection();
services.AddSingleton<IDiscountPolicy, NoDiscountPolicy>();

services.AddShopServices();
int count = services.Count;
services.AddShopServices();
Console.WriteLine($"Registry: second AddShopServices call added {services.Count - count} descriptors");

services.Decorate<IClock>((inner, _) => new OffsetClock(inner, TimeSpan.FromHours(3)));
bool decorated = services.TryDecorate<IAuditLog, BufferedAuditLog>();
services.Decorate(typeof(IRepository<>), typeof(CountingRepository<>));

using ServiceProvider provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
using IServiceScope scope = provider.CreateScope();
IServiceProvider sp = scope.ServiceProvider;

Console.WriteLine($"Convention: IOrderService -> {sp.GetRequiredService<IOrderService>().GetType().Name}");
Console.WriteLine($"Explicit type: IClock now = {sp.GetRequiredService<IClock>().UtcNow:HH:mm}");

var cache = sp.GetRequiredService<PriceCache>();
Console.WriteLine($"SelfWithInterfaces: same instance = {ReferenceEquals(cache, sp.GetRequiredService<IPriceReader>()) && ReferenceEquals(cache, sp.GetRequiredService<IPriceWriter>())}");

Console.WriteLine($"Keyed: card -> {sp.GetRequiredKeyedService<IPaymentGateway>("card").Pay(10m)}");
Console.WriteLine($"Keyed: cash -> {sp.GetRequiredKeyedService<IPaymentGateway>("cash").Pay(10m)}");

Console.WriteLine($"TryAdd: IDiscountPolicy -> {sp.GetRequiredService<IDiscountPolicy>().GetType().Name}");
Console.WriteLine($"TryAddEnumerable: {sp.GetServices<INotifier>().Count()} notifiers");

Console.WriteLine($"Decorators: {sp.GetRequiredService<IOrderService>().Place("book")}");
Console.WriteLine($"TryDecorate with nothing registered: {decorated}");
Console.WriteLine($"Open generic: {sp.GetRequiredService<IRepository<Product>>().Describe()}");
