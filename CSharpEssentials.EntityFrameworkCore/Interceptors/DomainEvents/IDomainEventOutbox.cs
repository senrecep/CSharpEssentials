using CSharpEssentials.Entity.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CSharpEssentials.EntityFrameworkCore.Interceptors;

/// <summary>
/// Stores domain events for transactional delivery (outbox pattern).
/// When it is registered in DI, <see cref="DomainEventInterceptor"/> and <c>BaseDbContext</c> send after-save events
/// to the outbox instead of publishing them directly. Both call it before the changes are written, with the context
/// that is saving, so rows the outbox adds to <c>context</c> are written by the same <c>SaveChanges</c> as the
/// entities, in one transaction. <c>EfCoreDomainEventOutbox</c> is the built-in implementation, and an outbox
/// processor relays the stored rows to a message handler.
/// </summary>
public interface IDomainEventOutbox
{
    /// <summary>
    /// Stores <paramref name="domainEvents"/>. An implementation that adds entities to <paramref name="context"/>
    /// must not call <c>SaveChanges</c>: the save in progress writes them.
    /// </summary>
    /// <param name="context">The context whose <c>SaveChanges</c> raised the events.</param>
    /// <param name="domainEvents">The after-save events, in the order they were raised.</param>
    /// <param name="cancellationToken">Cancels the store.</param>
    Task StoreAsync(DbContext context, IReadOnlyList<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
