using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CSharpEssentials.EntityFrameworkCore.HealthChecks;

public static class MigrationsHealthCheckBuilderExtensions
{
    /// <summary>
    /// The tag <see cref="AddMigrationsCheck{TContext}"/> uses when no tags are given.
    /// </summary>
    public const string ReadyTag = "ready";

    /// <summary>
    /// Adds <see cref="DbContextMigrationsHealthCheck{TContext}"/>. The check resolves <typeparamref name="TContext"/>
    /// from the health check scope, so register the context first. <paramref name="name"/> defaults to
    /// <c>"{TContext}-migrations"</c> and <paramref name="tags"/> to <see cref="ReadyTag"/>, so a readiness endpoint
    /// that filters on <c>"ready"</c> fails while migrations are pending.
    /// </summary>
    public static IHealthChecksBuilder AddMigrationsCheck<TContext>(
        this IHealthChecksBuilder builder,
        string? name = null,
        HealthStatus? failureStatus = null,
        IEnumerable<string>? tags = null,
        TimeSpan? timeout = null)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddCheck<DbContextMigrationsHealthCheck<TContext>>(
            name ?? $"{typeof(TContext).Name}-migrations",
            failureStatus,
            tags ?? [ReadyTag],
            timeout);
    }
}
