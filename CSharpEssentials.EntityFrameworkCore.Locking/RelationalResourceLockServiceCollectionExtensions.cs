using CSharpEssentials.Locking;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CSharpEssentials.EntityFrameworkCore.Locking;

public static class RelationalResourceLockServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="RelationalResourceLock{TContext}"/> as the singleton <see cref="IResourceLock"/>, replacing
    /// an earlier registration such as the <c>InProcessResourceLock</c> of <c>AddMediatorLockBehavior</c>.
    /// <typeparamref name="TContext"/> must be registered. A registered <see cref="TimeProvider"/> drives timeouts.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="dialect">The SQL of the database, for example <see cref="ResourceLockSqlDialect.PostgreSql"/>.</param>
    /// <param name="configure">Sets the poll interval.</param>
    /// <returns><paramref name="services"/>.</returns>
    /// <exception cref="ArgumentException">The poll interval is not positive.</exception>
    public static IServiceCollection AddRelationalResourceLock<TContext>(
        this IServiceCollection services, ResourceLockSqlDialect dialect, Action<RelationalResourceLockOptions>? configure = null)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(dialect);

        RelationalResourceLockOptions options = new() { Dialect = dialect };
        configure?.Invoke(options);
        if (options.PollInterval <= TimeSpan.Zero)
            throw new ArgumentException($"{nameof(RelationalResourceLockOptions)}.{nameof(RelationalResourceLockOptions.PollInterval)} must be positive.", nameof(configure));

        services.Replace(ServiceDescriptor.Singleton<IResourceLock>(sp => new RelationalResourceLock<TContext>(
            sp.GetRequiredService<IServiceScopeFactory>(), options, sp.GetService<TimeProvider>())));
        return services;
    }
}
