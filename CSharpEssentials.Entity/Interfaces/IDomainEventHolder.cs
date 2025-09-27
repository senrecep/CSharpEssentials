
namespace CSharpEssentials.Entity.Interfaces;

public interface IDomainEventHolder
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();

    void Raise(IDomainEvent domainEvent);
}
