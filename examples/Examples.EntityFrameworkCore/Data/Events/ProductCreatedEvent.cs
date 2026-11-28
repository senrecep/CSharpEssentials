using CSharpEssentials.Entity;
using CSharpEssentials.Entity.Interfaces;

namespace Examples.EntityFrameworkCore.Data.Events;

[DomainEventTiming(DomainEventTiming.BeforeSave)]
public sealed record ProductCreatedEvent(string ProductName) : IDomainEvent;
