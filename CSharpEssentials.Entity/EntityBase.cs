
using CSharpEssentials.Entity.Interfaces;

namespace CSharpEssentials.Entity;

/// <summary>
/// Represents an entity base.
/// </summary>
public abstract class EntityBase : IEntityBase
{
    private readonly List<IDomainEvent> _domainEvents = [];


    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public string? UpdatedBy { get; private set; }


    public IReadOnlyList<IDomainEvent> DomainEvents => [.. _domainEvents];

    public void ClearDomainEvents() => _domainEvents.Clear();

    public void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void SetCreatedInfo(DateTimeOffset createdAt, string createdBy) =>
        (CreatedAt, CreatedBy) = (createdAt, createdBy);
    public void SetUpdatedInfo(DateTimeOffset updatedAt, string updatedBy) =>
        (UpdatedAt, UpdatedBy) = (updatedAt, updatedBy);
}
