using CSharpEssentials.Entity.Interfaces;

namespace CSharpEssentials.EntityFrameworkCore.Interceptors;

/// <summary>
/// Stores domain events for transactional delivery (outbox pattern).
/// When it is registered in DI, <see cref="DomainEventInterceptor"/> sends
/// after-save events to the outbox instead of publishing them directly.
/// A background worker can then relay outbox entries to the message broker.
/// </summary>
public interface IDomainEventOutbox
{
    Task StoreAsync(IReadOnlyList<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
