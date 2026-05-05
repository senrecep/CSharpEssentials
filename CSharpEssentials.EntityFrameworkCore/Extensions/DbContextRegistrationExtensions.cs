using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.EntityFrameworkCore.Extensions;

/// <summary>
/// Extension methods for registering DbContext variants (pooled, factory, write/read separation).
/// </summary>
public static class DbContextRegistrationExtensions
{
    #region — Pooled DbContext —

    /// <summary>
    /// Registers a pooled <see cref="DbContext"/>.
    /// The caller must configure the database provider (e.g. <c>UseNpgsql</c> or <c>UseSqlServer</c>) inside the <paramref name="configure"/> callback.
    /// </summary>
    public static IServiceCollection AddPooledDbContext<TContext>(
        this IServiceCollection services,
        Action<IServiceProvider, DbContextOptionsBuilder> configure)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddDbContextPool<TContext>(configure);
        return services;
    }

    /// <summary>
    /// Registers a pooled <see cref="DbContext"/> with <see cref="DbContextRegistrationOptions"/>.
    /// </summary>
    public static IServiceCollection AddPooledDbContext<TContext>(
        this IServiceCollection services,
        Action<DbContextRegistrationOptions> configureOptions,
        Action<IServiceProvider, DbContextOptionsBuilder>? configure = null)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        var options = new DbContextRegistrationOptions();
        configureOptions(options);

        services.AddDbContextPool<TContext>((sp, builder) =>
        {
            options.Apply(sp, builder);
            configure?.Invoke(sp, builder);
        });

        return services;
    }

    #endregion

    #region — DbContext Factory —

    /// <summary>
    /// Registers a <see cref="DbContext"/> factory.
    /// The caller must configure the database provider inside the <paramref name="configure"/> callback.
    /// </summary>
    public static IServiceCollection RegisterDbContextFactory<TContext>(
        this IServiceCollection services,
        Action<IServiceProvider, DbContextOptionsBuilder> configure)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        EntityFrameworkServiceCollectionExtensions.AddDbContextFactory<TContext>(services, configure);
        return services;
    }

    /// <summary>
    /// Registers a <see cref="DbContext"/> factory with <see cref="DbContextRegistrationOptions"/>.
    /// </summary>
    public static IServiceCollection RegisterDbContextFactory<TContext>(
        this IServiceCollection services,
        Action<DbContextRegistrationOptions> configureOptions,
        Action<IServiceProvider, DbContextOptionsBuilder>? configure = null)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        var options = new DbContextRegistrationOptions();
        configureOptions(options);

        EntityFrameworkServiceCollectionExtensions.AddDbContextFactory<TContext>(services, (sp, builder) =>
        {
            options.Apply(sp, builder);
            configure?.Invoke(sp, builder);
        });

        return services;
    }

    #endregion

    #region — Write DbContext —

    /// <summary>
    /// Registers a pooled write <see cref="DbContext"/> with change tracking enabled.
    /// The caller must configure the database provider inside the <paramref name="configure"/> callback.
    /// </summary>
    public static IServiceCollection AddWriteDbContext<TContext>(
        this IServiceCollection services,
        Action<IServiceProvider, DbContextOptionsBuilder> configure)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddDbContextPool<TContext>((sp, options) =>
        {
            options.UseAsWriteContext();
            configure(sp, options);
        });

        return services;
    }

    /// <summary>
    /// Registers a pooled write <see cref="DbContext"/> with <see cref="DbContextRegistrationOptions"/>.
    /// </summary>
    public static IServiceCollection AddWriteDbContext<TContext>(
        this IServiceCollection services,
        Action<DbContextRegistrationOptions> configureOptions,
        Action<IServiceProvider, DbContextOptionsBuilder>? configure = null)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        var options = new DbContextRegistrationOptions();
        configureOptions(options);

        services.AddDbContextPool<TContext>((sp, builder) =>
        {
            builder.UseAsWriteContext();
            options.Apply(sp, builder);
            configure?.Invoke(sp, builder);
        });

        return services;
    }

    #endregion

    #region — Read DbContext —

    /// <summary>
    /// Registers a pooled read-only <see cref="DbContext"/> with no change tracking.
    /// The caller must configure the database provider inside the <paramref name="configure"/> callback.
    /// </summary>
    public static IServiceCollection AddReadDbContext<TContext>(
        this IServiceCollection services,
        Action<IServiceProvider, DbContextOptionsBuilder> configure)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddDbContextPool<TContext>((sp, options) =>
        {
            options.UseAsReadContext();
            configure(sp, options);
        });

        return services;
    }

    /// <summary>
    /// Registers a pooled read-only <see cref="DbContext"/> with <see cref="DbContextRegistrationOptions"/>.
    /// </summary>
    public static IServiceCollection AddReadDbContext<TContext>(
        this IServiceCollection services,
        Action<DbContextRegistrationOptions> configureOptions,
        Action<IServiceProvider, DbContextOptionsBuilder>? configure = null)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        var options = new DbContextRegistrationOptions();
        configureOptions(options);

        services.AddDbContextPool<TContext>((sp, builder) =>
        {
            builder.UseAsReadContext();
            options.Apply(sp, builder);
            configure?.Invoke(sp, builder);
        });

        return services;
    }

    #endregion

    #region — CQRS (Write + Read) —

    /// <summary>
    /// Registers both a write <see cref="DbContext"/> and a read-only <see cref="DbContext"/> for CQRS scenarios.
    /// </summary>
    public static IServiceCollection AddCqrsDbContexts<TWriteContext, TReadContext>(
        this IServiceCollection services,
        Action<IServiceProvider, DbContextOptionsBuilder> configureWrite,
        Action<IServiceProvider, DbContextOptionsBuilder>? configureRead = null)
        where TWriteContext : DbContext
        where TReadContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureWrite);

        services.AddWriteDbContext<TWriteContext>(configureWrite);
        services.AddReadDbContext<TReadContext>(configureRead ?? configureWrite);

        return services;
    }

    /// <summary>
    /// Registers both a write <see cref="DbContext"/> and a read-only <see cref="DbContext"/> for CQRS scenarios
    /// with shared <see cref="DbContextRegistrationOptions"/>.
    /// </summary>
    public static IServiceCollection AddCqrsDbContexts<TWriteContext, TReadContext>(
        this IServiceCollection services,
        Action<DbContextRegistrationOptions> configureOptions,
        Action<IServiceProvider, DbContextOptionsBuilder>? configureWrite = null,
        Action<IServiceProvider, DbContextOptionsBuilder>? configureRead = null)
        where TWriteContext : DbContext
        where TReadContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.AddWriteDbContext<TWriteContext>(configureOptions, configureWrite);
        services.AddReadDbContext<TReadContext>(configureOptions, configureRead ?? configureWrite);

        return services;
    }

    #endregion
}
