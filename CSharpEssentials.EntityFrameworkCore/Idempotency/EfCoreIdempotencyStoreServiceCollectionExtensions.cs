using CSharpEssentials.Idempotency;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CSharpEssentials.EntityFrameworkCore.Idempotency;

public static class EfCoreIdempotencyStoreServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="EfCoreIdempotencyRecordStore{TContext}"/> as a scoped service and exposes it as an
    /// <see cref="IIdempotencyRecordStore"/>: as the default store (replacing the in-memory one) when
    /// <paramref name="serviceKey"/> is <see langword="null"/>, otherwise as the keyed store that profiles select with
    /// <c>StoreKey</c>, so one application can keep some profiles in the database and others in another store.
    /// <typeparamref name="TContext"/> must be registered and map <see cref="IdempotencyRecord"/>
    /// (<see cref="IdempotencyRecordModelBuilderExtensions.AddIdempotencyRecords"/>). A registered
    /// <see cref="TimeProvider"/> is used for leases and retention.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="serviceKey">The store key of the profiles that use this store; <see langword="null"/> for the default store.</param>
    /// <returns><paramref name="services"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="serviceKey"/> is empty.</exception>
    public static IServiceCollection AddEfCoreIdempotencyRecordStore<TContext>(this IServiceCollection services, string? serviceKey = null)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        if (serviceKey is { Length: 0 })
            throw new ArgumentException("The service key must not be empty; pass null to register the default store.", nameof(serviceKey));

        services.TryAddScoped<EfCoreIdempotencyRecordStore<TContext>>();
        if (serviceKey is null)
            services.Replace(ServiceDescriptor.Scoped<IIdempotencyRecordStore>(sp => sp.GetRequiredService<EfCoreIdempotencyRecordStore<TContext>>()));
        else
            services.AddKeyedScoped<IIdempotencyRecordStore>(serviceKey, (sp, _) => sp.GetRequiredService<EfCoreIdempotencyRecordStore<TContext>>());
        return services;
    }
}
