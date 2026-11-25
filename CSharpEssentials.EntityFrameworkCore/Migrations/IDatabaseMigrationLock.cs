using Microsoft.EntityFrameworkCore;

namespace CSharpEssentials.EntityFrameworkCore.Migrations;

/// <summary>
/// Serializes <see cref="DatabaseMigrationExtensions.MigrateDatabaseAsync{TContext}"/> across application instances.
/// Register an implementation over your infrastructure (a PostgreSQL advisory lock, <c>sp_getapplock</c>,
/// a Redis lock) so only one instance applies migrations at a time.
/// </summary>
public interface IDatabaseMigrationLock
{
    /// <summary>
    /// Waits until the lock is held and returns a handle that releases it when disposed.
    /// <paramref name="context"/> is the context about to migrate; its connection may be used to take the lock.
    /// </summary>
    ValueTask<IAsyncDisposable> AcquireAsync(DbContext context, CancellationToken cancellationToken);
}
