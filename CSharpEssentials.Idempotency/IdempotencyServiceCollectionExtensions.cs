using CSharpEssentials.Idempotency;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Registers <see cref="IIdempotencyExecutor"/> and <see cref="IdempotencyInbox"/>.</summary>
public static class IdempotencyServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IdempotencyExecutorOptions"/> as options, a scoped <see cref="IIdempotencyExecutor"/>, a
    /// scoped <see cref="IdempotencyInbox"/> and, unless a store is already registered,
    /// <see cref="InMemoryIdempotencyRecordStore"/>. The executor reads the options through
    /// <see cref="IOptionsMonitor{TOptions}"/> on every operation, so a change made through configuration reload, for
    /// example of <see cref="IdempotencyExecutorOptions.Mode"/>, applies without a restart.
    /// With <paramref name="configure"/> it replaces the configuration given to an earlier call; without it, that
    /// configuration is kept. Other <c>Configure</c> and <c>Bind</c> calls on the options still apply. Invalid options
    /// throw here; options that a reload makes invalid throw <see cref="OptionsValidationException"/> when they are
    /// rebuilt and on every operation until they are fixed.
    /// </summary>
    /// <exception cref="InvalidOperationException">The options or a profile are invalid.</exception>
    public static IServiceCollection AddIdempotencyExecutor(this IServiceCollection services, Action<IdempotencyExecutorOptions>? configure = null)
    {
        Guard.NotNull(services);

        IdempotencyExecutorOptions options = new();
        configure?.Invoke(options);
        options.Validate();

        services.AddOptions();
        if (configure is not null)
        {
            for (int index = services.Count - 1; index >= 0; index--)
            {
                if (services[index] is { IsKeyedService: false, ImplementationInstance: IdempotencyExecutorOptionsSetup })
                    services.RemoveAt(index);
            }

            services.AddSingleton<IConfigureOptions<IdempotencyExecutorOptions>>(new IdempotencyExecutorOptionsSetup(configure));
        }

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<IdempotencyExecutorOptions>, IdempotencyExecutorOptionsValidator>());
        services.TryAddTransient(static provider => provider.GetRequiredService<IOptionsMonitor<IdempotencyExecutorOptions>>().CurrentValue);
        services.TryAddSingleton<IIdempotencyRecordStore, InMemoryIdempotencyRecordStore>();
        services.TryAddScoped<IIdempotencyExecutor, IdempotencyExecutor>();
        services.TryAddScoped<IdempotencyInbox>();
        return services;
    }
}
