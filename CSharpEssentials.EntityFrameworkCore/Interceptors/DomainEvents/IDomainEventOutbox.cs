using CSharpEssentials.Entity.Interfaces;

namespace CSharpEssentials.EntityFrameworkCore.Interceptors;

/// <summary>
/// Stores domain events for reliable, transactional delivery (outbox pattern).
/// When registered in DI, the <see cref="DomainEventInterceptor"/> will route
/// after-save events to the outbox instead of publishing them directly.
/// A background worker can then relay outbox entries to the message broker.
/// </summary>
public interface IDomainEventOutbox
{
    Task StoreAsync(IReadOnlyList<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
