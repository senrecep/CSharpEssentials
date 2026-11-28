using CSharpEssentials.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Examples.DependencyInjection;

public interface IClock
{
    DateTime UtcNow { get; }
}

[RegisterSingleton(typeof(IClock))]
public sealed class FixedClock : IClock
{
    public DateTime UtcNow => new(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);
}

public sealed class OffsetClock(IClock inner, TimeSpan offset) : IClock
{
    public DateTime UtcNow => inner.UtcNow + offset;
}

public interface IOrderService
{
    string Place(string item);
}

[RegisterScoped]
public sealed class OrderService(IClock clock) : IOrderService
{
    public string Place(string item) => $"{item} at {clock.UtcNow:HH:mm}";
}

[Decorates<IOrderService>(Order = 1)]
public sealed class ValidatingOrderService(IOrderService inner) : IOrderService
{
    public string Place(string item) => string.IsNullOrWhiteSpace(item) ? "rejected" : "valid " + inner.Place(item);
}

[Decorates<IOrderService>(Order = 2)]
public sealed class LoggingOrderService(IOrderService inner) : IOrderService
{
    public string Place(string item) => "logged " + inner.Place(item);
}

public interface IPriceReader;

public interface IPriceWriter;

[RegisterSingleton(As = ServiceAs.SelfWithInterfaces)]
public sealed class PriceCache : IPriceReader, IPriceWriter;

public interface IPaymentGateway
{
    string Pay(decimal amount);
}

[RegisterSingleton<IPaymentGateway>(Key = "card")]
public sealed class CardGateway : IPaymentGateway
{
    public string Pay(decimal amount) => $"card {amount}";
}

[RegisterSingleton<IPaymentGateway>(Key = "cash")]
public sealed class CashGateway : IPaymentGateway
{
    public string Pay(decimal amount) => $"cash {amount}";
}

[Decorates<IPaymentGateway>(Key = "card")]
public sealed class RetryingGateway(IPaymentGateway inner, [ServiceKey] string key) : IPaymentGateway
{
    public string Pay(decimal amount) => $"{inner.Pay(amount)} (with retry, key {key})";
}

public interface IDiscountPolicy;

[RegisterSingleton(Strategy = RegistrationStrategy.TryAdd)]
public sealed class DiscountPolicy : IDiscountPolicy;

public sealed class NoDiscountPolicy : IDiscountPolicy;

public interface INotifier;

[RegisterTransient<INotifier>(Strategy = RegistrationStrategy.TryAddEnumerable)]
public sealed class EmailNotifier : INotifier;

[RegisterTransient<INotifier>(Strategy = RegistrationStrategy.TryAddEnumerable)]
public sealed class SmsNotifier : INotifier;

public interface IAuditLog;

public sealed class BufferedAuditLog(IAuditLog inner) : IAuditLog
{
    public IAuditLog Inner { get; } = inner;
}

public sealed record Product(int Id);

public interface IRepository<T>
{
    T? Find(int id);

    void Save(T item);

    string Describe();
}

[RegisterScoped<IRepository<Product>>]
public sealed class ProductRepository : IRepository<Product>
{
    public Product? Find(int id) => new(id);

    public void Save(Product item)
    {
    }

    public string Describe() => "products";
}

public sealed class CountingRepository<T>(IRepository<T> inner) : IRepository<T>
{
    public T? Find(int id) => inner.Find(id);

    public void Save(T item) => inner.Save(item);

    public string Describe() => $"counted {inner.Describe()}";
}

[ExcludeFromRegistration]
[RegisterSingleton]
public sealed class LegacyOrderService : IOrderService
{
    public string Place(string item) => "legacy";
}
