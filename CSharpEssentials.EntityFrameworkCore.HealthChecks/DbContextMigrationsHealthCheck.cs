using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CSharpEssentials.EntityFrameworkCore.HealthChecks;

/// <summary>
/// Reports the registration's failure status (<see cref="HealthStatus.Unhealthy"/> by default) while
/// <typeparamref name="TContext"/> has migrations that are not applied, and <see cref="HealthStatus.Healthy"/> once
/// they all are. A database that does not exist has every migration pending. When the database cannot be reached,
/// the query throws and the health check service reports the failure status with the exception.
/// </summary>
public sealed class DbContextMigrationsHealthCheck<TContext>(TContext dbContext) : IHealthCheck
    where TContext : DbContext
{
    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        string[] pending = [.. await dbContext.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false)];
        if (pending.Length == 0)
            return HealthCheckResult.Healthy();

        return new HealthCheckResult(
            context.Registration.FailureStatus,
            $"{typeof(TContext).Name} has {pending.Length} pending migration(s).",
            data: new Dictionary<string, object> { ["pendingMigrations"] = pending });
    }
}
