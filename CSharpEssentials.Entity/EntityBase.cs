using System.Collections.ObjectModel;
using CSharpEssentials.Entity.Interfaces;

namespace CSharpEssentials.Entity;

/// <summary>
/// Represents an entity base.
/// </summary>
public abstract class EntityBase : IEntityBase
{
    private readonly List<IDomainEvent> _domainEvents = [];
    private ReadOnlyCollection<IDomainEvent>? _domainEventsView;

#if NET9_0_OR_GREATER
    public DateTimeOffset CreatedAt
    {
        get;
        private set => field = value > DateTimeOffset.MinValue ? value : throw new ArgumentOutOfRangeException(nameof(value));
    }
#else
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0032:Use auto property", Justification = "Property validation requires explicit backing field on targets prior to .NET 9")]
    private DateTimeOffset _createdAt;
    public DateTimeOffset CreatedAt
    {
        get => _createdAt;
        private set => _createdAt = value > DateTimeOffset.MinValue ? value : throw new ArgumentOutOfRangeException(nameof(value));
    }
#endif
    public string? CreatedBy { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public string? UpdatedBy { get; private set; }

    /// <summary>
    /// A read-only live view of the raised events: it reflects later <see cref="Raise"/> and
    /// <see cref="ClearDomainEvents"/> calls. Copy it (for example with <c>[.. entity.DomainEvents]</c>) before
    /// clearing or raising while you iterate.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEventsView ??= _domainEvents.AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();

    public void Raise(IDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent ?? throw new ArgumentNullException(nameof(domainEvent)));

    public void SetCreatedInfo(DateTimeOffset createdAt, string createdBy) =>
        (CreatedAt, CreatedBy) = (createdAt, createdBy);
    public void SetUpdatedInfo(DateTimeOffset updatedAt, string updatedBy) =>
        (UpdatedAt, UpdatedBy) = (updatedAt, updatedBy);
}
