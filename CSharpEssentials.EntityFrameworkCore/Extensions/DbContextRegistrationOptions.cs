using CSharpEssentials.EntityFrameworkCore.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.EntityFrameworkCore.Extensions;

/// <summary>
/// Retry policy options for database connections.
/// </summary>
public sealed class DbContextRetryOptions
{
    /// <summary>
    /// Maximum number of retry attempts. Default is 3.
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// Maximum delay between retries. Default is 5 seconds.
    /// </summary>
    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Additional error codes to consider as transient.
    /// </summary>
    public ICollection<int> AdditionalErrorCodes { get; set; } = [];
}

/// <summary>
/// Options for configuring DbContext registration behavior.
/// </summary>
public sealed class DbContextRegistrationOptions
{
    /// <summary>
    /// Enables detailed errors on the DbContext.
    /// </summary>
    public bool EnableDetailedErrors { get; set; }

    /// <summary>
    /// Enables sensitive data logging on the DbContext.
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; }

    /// <summary>
    /// Overrides the default query tracking behavior. When <c>null</c>, the provider default is used.
    /// </summary>
    public QueryTrackingBehavior? QueryTrackingBehavior { get; set; }

    /// <summary>
    /// Adds the <see cref="AuditInterceptor"/> to the DbContext if it is registered in DI.
    /// </summary>
    public bool EnableAuditInterceptor { get; set; }

    /// <summary>
    /// Adds the <see cref="DomainEventInterceptor"/> to the DbContext if it is registered in DI.
    /// </summary>
    public bool EnableDomainEventInterceptor { get; set; }

    /// <summary>
    /// Adds the <see cref="SlowQueryInterceptor"/> to the DbContext if it is registered in DI.
    /// </summary>
    public bool EnableSlowQueryInterceptor { get; set; }

    /// <summary>
    /// The assembly name containing migrations. Applied via the provider's migrations assembly option.
    /// </summary>
    public string? MigrationsAssembly { get; set; }

    /// <summary>
    /// Retry policy options. When <c>null</c>, no retry is configured.
    /// </summary>
    public DbContextRetryOptions? RetryOptions { get; set; }

    /// <summary>
    /// The query splitting behavior. When <c>null</c>, the provider default is used.
    /// </summary>
    public QuerySplittingBehavior? QuerySplittingBehavior { get; set; }

    internal void Apply(IServiceProvider serviceProvider, DbContextOptionsBuilder options)
    {
        if (EnableDetailedErrors)
            options.EnableDetailedErrors();

        if (EnableSensitiveDataLogging)
            options.EnableSensitiveDataLogging();

        if (QueryTrackingBehavior.HasValue)
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.Value);

        if (EnableAuditInterceptor)
        {
            AuditInterceptor? interceptor = serviceProvider.GetService<AuditInterceptor>();
            if (interceptor is not null)
                options.AddInterceptors(interceptor);
        }

        if (EnableDomainEventInterceptor)
        {
            DomainEventInterceptor? interceptor = serviceProvider.GetService<DomainEventInterceptor>();
            if (interceptor is not null)
                options.AddInterceptors(interceptor);
        }

        if (EnableSlowQueryInterceptor)
        {
            SlowQueryInterceptor? interceptor = serviceProvider.GetService<SlowQueryInterceptor>();
            if (interceptor is not null)
                options.AddInterceptors(interceptor);
        }
    }
}
