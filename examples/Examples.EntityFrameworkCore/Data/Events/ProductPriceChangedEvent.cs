using CSharpEssentials.Entity;

namespace Examples.EntityFrameworkCore.Data.Events;

public sealed record ProductPriceChangedEvent(Guid ProductId, decimal OldPrice, decimal NewPrice) : DomainEvent;
