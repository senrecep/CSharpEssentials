namespace CSharpEssentials.Entity;

/// <summary>
/// Marks a domain event class with its publish timing relative to the database save.
/// When not applied, the default timing is <see cref="DomainEventTiming.AfterSave"/>.
/// </summary>
/// <example>
/// <code>
/// [DomainEventTiming(DomainEventTiming.BeforeSave)]
/// public sealed class OrderValidationEvent : IDomainEvent { }
///
/// // No attribute = AfterSave (default)
/// public sealed class OrderCreatedEvent : IDomainEvent { }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class DomainEventTimingAttribute(DomainEventTiming timing = DomainEventTiming.AfterSave) : Attribute
{
    public DomainEventTiming Timing { get; } = timing;
}
