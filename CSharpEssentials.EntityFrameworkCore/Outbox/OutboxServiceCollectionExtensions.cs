using CSharpEssentials.EntityFrameworkCore.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CSharpEssentials.EntityFrameworkCore.Outbox;

public static class OutboxServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="EfCoreDomainEventOutbox"/> as the scoped <see cref="IDomainEventOutbox"/>, an
    /// <see cref="OutboxEventTypeRegistry"/> filled by <paramref name="registerTypes"/>, and
    /// <see cref="JsonOutboxMessageSerializer"/> as the <see cref="IOutboxMessageSerializer"/> unless one is already
    /// registered. After-save domain events of <c>BaseDbContext</c> (with <c>DispatchDomainEventsOnSaveChanges</c>) or
    /// of <c>DomainEventInterceptor</c> are then stored as <see cref="OutboxMessage"/> rows in the saving context.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="registerTypes">Registers every event type the outbox stores, each under a stable name.</param>
    /// <returns><paramref name="services"/>.</returns>
    public static IServiceCollection AddEfCoreOutbox(this IServiceCollection services, Action<OutboxEventTypeRegistry> registerTypes)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(registerTypes);

        OutboxEventTypeRegistry registry = new();
        registerTypes(registry);
        services.AddSingleton(registry);
        services.TryAddSingleton<IOutboxMessageSerializer, JsonOutboxMessageSerializer>();
        services.Replace(ServiceDescriptor.Scoped<IDomainEventOutbox, EfCoreDomainEventOutbox>());
        return services;
    }

    /// <summary>
    /// Registers <see cref="OutboxProcessor{TContext}"/> as the singleton <see cref="IOutboxProcessor"/> and the
    /// <see cref="OutboxProcessorOptions"/> configured by <paramref name="configure"/>. Register an
    /// <see cref="IOutboxMessageHandler"/> (it is resolved in a scope per message) and call
    /// <see cref="AddEfCoreOutbox"/> for the serializer. <typeparamref name="TContext"/> must be registered and map
    /// <see cref="OutboxMessage"/>. A registered <see cref="TimeProvider"/> is used for leases and retries.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="configure">Sets batch size, attempts, lease, retry delays and the claimer.</param>
    /// <returns><paramref name="services"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">An option is out of range.</exception>
    public static IServiceCollection AddOutboxProcessor<TContext>(this IServiceCollection services, Action<OutboxProcessorOptions>? configure = null)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);

        OutboxProcessorOptions options = new();
        configure?.Invoke(options);
        options.Validate();
        services.Replace(ServiceDescriptor.Singleton(options));
        services.Replace(ServiceDescriptor.Singleton<IOutboxProcessor, OutboxProcessor<TContext>>());
        return services;
    }
}
