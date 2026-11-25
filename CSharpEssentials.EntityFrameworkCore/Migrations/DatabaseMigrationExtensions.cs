using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.EntityFrameworkCore.Migrations;

public static class DatabaseMigrationExtensions
{
    /// <summary>
    /// Applies the pending migrations of <typeparamref name="TContext"/> from a new service scope. Call it at startup,
    /// before the host starts serving, for example <c>await app.Services.MigrateDatabaseAsync&lt;AppDbContext&gt;();</c>.
    /// When an <see cref="IDatabaseMigrationLock"/> is registered, the migration runs while that lock is held.
    /// </summary>
    public static async Task MigrateDatabaseAsync<TContext>(this IServiceProvider services, CancellationToken cancellationToken = default)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);

        AsyncServiceScope scope = services.CreateAsyncScope();
        await using ConfiguredAsyncDisposable scopeDisposal = scope.ConfigureAwait(false);
        TContext context = scope.ServiceProvider.GetRequiredService<TContext>();
        IDatabaseMigrationLock? migrationLock = scope.ServiceProvider.GetService<IDatabaseMigrationLock>();
        if (migrationLock is null)
        {
            await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        IAsyncDisposable handle = await migrationLock.AcquireAsync(context, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"{migrationLock.GetType().Name}.AcquireAsync returned null.");
        await using ConfiguredAsyncDisposable release = handle.ConfigureAwait(false);
        await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
    }
}
