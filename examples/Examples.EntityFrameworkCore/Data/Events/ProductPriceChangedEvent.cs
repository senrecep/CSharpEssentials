using CSharpEssentials.Entity.Interfaces;

namespace Examples.EntityFrameworkCore.Data.Events;

public sealed class ProductPriceChangedEvent(Guid productId, decimal oldPrice, decimal newPrice) : IDomainEvent
{
    public Guid ProductId { get; } = productId;
    public decimal OldPrice { get; } = oldPrice;
    public decimal NewPrice { get; } = newPrice;
}
