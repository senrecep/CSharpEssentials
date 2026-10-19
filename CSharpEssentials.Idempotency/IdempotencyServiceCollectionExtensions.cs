using CSharpEssentials.Idempotency;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Registers <see cref="IIdempotencyExecutor"/> and <see cref="IdempotencyInbox"/>.</summary>
public static class IdempotencyServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IdempotencyExecutorOptions"/>, a scoped <see cref="IIdempotencyExecutor"/>, a scoped
    /// <see cref="IdempotencyInbox"/> and, unless a store is already registered, <see cref="InMemoryIdempotencyRecordStore"/>.
    /// With <paramref name="configure"/> the options replace any registered before; without it, registered options are
    /// kept. Invalid options throw here, not on the first operation.
    /// </summary>
    /// <exception cref="InvalidOperationException">The options or a profile are invalid.</exception>
    public static IServiceCollection AddIdempotencyExecutor(this IServiceCollection services, Action<IdempotencyExecutorOptions>? configure = null)
    {
        Guard.NotNull(services);

        IdempotencyExecutorOptions options = new();
        configure?.Invoke(options);
        options.Validate();
        if (configure is null)
            services.TryAddSingleton(options);
        else
            services.Replace(ServiceDescriptor.Singleton(options));

        services.TryAddSingleton<IIdempotencyRecordStore, InMemoryIdempotencyRecordStore>();
        services.TryAddScoped<IIdempotencyExecutor, IdempotencyExecutor>();
        services.TryAddScoped<IdempotencyInbox>();
        return services;
    }
}
