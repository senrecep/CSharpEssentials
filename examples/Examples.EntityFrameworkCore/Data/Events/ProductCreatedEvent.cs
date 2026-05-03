using CSharpEssentials.Entity;
using CSharpEssentials.Entity.Interfaces;

namespace Examples.EntityFrameworkCore.Data.Events;

[DomainEventTiming(DomainEventTiming.BeforeSave)]
public sealed class ProductCreatedEvent(string productName) : IDomainEvent
{
    public string ProductName { get; } = productName;
}
