using CSharpEssentials.Transactions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CSharpEssentials.EntityFrameworkCore.Transactions;

public static class TransactionRunnerServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="EfCoreTransactionRunner{TDbContext}"/> as the scoped <see cref="ITransactionRunner"/> and
    /// <see cref="ITransactionCallbacks"/>, replacing any registered before.
    /// </summary>
    public static IServiceCollection AddEfCoreTransactionRunner<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);

        services.Replace(ServiceDescriptor.Scoped<ITransactionRunner, EfCoreTransactionRunner<TDbContext>>());
        services.Replace(ServiceDescriptor.Scoped<ITransactionCallbacks, EfCoreTransactionRunner<TDbContext>>());
        return services;
    }
}
