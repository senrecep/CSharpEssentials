using CSharpEssentials.Entity.Interfaces;
using CSharpEssentials.EntityFrameworkCore.Interceptors;
using Examples.EntityFrameworkCore.Data.Events;

namespace Examples.EntityFrameworkCore.Services;

public sealed class ConsoleDomainEventPublisher : IDomainEventPublisher
{
    public Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        Console.WriteLine(domainEvent switch
        {
            ProductCreatedEvent e => $"   [BeforeSave] creating {e.ProductName}",
            ProductPriceChangedEvent e => $"   [AfterSave] price {e.OldPrice} -> {e.NewPrice}",
            _ => $"   {domainEvent.GetType().Name}",
        });
        return Task.CompletedTask;
    }
}
