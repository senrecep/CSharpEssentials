using CSharpEssentials.Entity.Interfaces;
using CSharpEssentials.EntityFrameworkCore.Interceptors;
using Examples.EntityFrameworkCore.Data.Events;

namespace Examples.EntityFrameworkCore.Services;

public sealed class ConsoleDomainEventPublisher : IDomainEventPublisher
{
    public Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        string message = domainEvent switch
        {
            ProductCreatedEvent e => $"  [BeforeSave] Product creating: {e.ProductName}",
            ProductPriceChangedEvent e => $"  [AfterSave]  Price changed: ${e.OldPrice} -> ${e.NewPrice} (ProductId: {e.ProductId})",
            _ => $"  [Event] {domainEvent.GetType().Name}"
        };

        Console.WriteLine(message);
        return Task.CompletedTask;
    }
}
