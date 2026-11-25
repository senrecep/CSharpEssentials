using System.Data.Common;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CSharpEssentials.EntityFrameworkCore.Sql;

public static class SqlSessionServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="DbContextSqlSession{TDbContext}"/> as the scoped <see cref="ISqlSession"/> and
    /// <see cref="ISqlConnectionFactory"/>, replacing earlier registrations of both.
    /// </summary>
    public static IServiceCollection AddDbContextSqlSession<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);

        services.Replace(ServiceDescriptor.Scoped<ISqlSession, DbContextSqlSession<TDbContext>>());
        services.Replace(ServiceDescriptor.Scoped<ISqlConnectionFactory, DbContextSqlSession<TDbContext>>());
        return services;
    }

    /// <summary>
    /// Registers <see cref="DbDataSourceSqlConnectionFactory"/> as the singleton <see cref="ISqlReadOnlyConnectionFactory"/>,
    /// replacing an earlier registration. The data source comes from <paramref name="dataSource"/>, or from the
    /// <see cref="DbDataSource"/> registered in the container when it is <see langword="null"/>.
    /// </summary>
    public static IServiceCollection AddDbDataSourceReadOnlyConnectionFactory(
        this IServiceCollection services, Func<IServiceProvider, DbDataSource>? dataSource = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        Func<IServiceProvider, DbDataSource> resolve = dataSource ?? (static provider => provider.GetRequiredService<DbDataSource>());
        services.Replace(ServiceDescriptor.Singleton<ISqlReadOnlyConnectionFactory>(
            provider => new DbDataSourceSqlConnectionFactory(resolve(provider))));
        return services;
    }
}
