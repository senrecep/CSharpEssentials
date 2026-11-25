namespace CSharpEssentials.EntityFrameworkCore.Locking;

/// <summary>
/// The SQL that <see cref="RelationalResourceLock{TContext}"/> runs on a dedicated connection to take and release a
/// session lock. Both statements get the lock key as the <c>FromSqlRaw</c> placeholder <c>{0}</c>.
/// <see cref="TryAcquireSql"/> must not wait and must return one row with an integer column named <c>Value</c> that is
/// 1 when the lock was taken (0 or no row otherwise). It runs through <c>SqlQueryRaw&lt;int&gt;</c> without composition.
/// Use <see cref="PostgreSql"/> or <see cref="SqlServer"/>, or build one for another database.
/// </summary>
public sealed class ResourceLockSqlDialect
{
    /// <summary>Creates a dialect.</summary>
    /// <param name="tryAcquireSql">Takes the lock for <c>{0}</c> without waiting; returns <c>Value</c> = 1 when taken.</param>
    /// <param name="releaseSql">Releases the lock for <c>{0}</c> held by the connection.</param>
    /// <exception cref="ArgumentException">A statement is empty.</exception>
    public ResourceLockSqlDialect(string tryAcquireSql, string releaseSql)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tryAcquireSql);
        ArgumentException.ThrowIfNullOrWhiteSpace(releaseSql);
        TryAcquireSql = tryAcquireSql;
        ReleaseSql = releaseSql;
    }

    /// <summary>
    /// PostgreSQL session advisory locks: <c>pg_try_advisory_lock</c> and <c>pg_advisory_unlock</c> on
    /// <c>hashtextextended(key, 0)</c> (PostgreSQL 11 or later), a stable 64-bit hash of the key. Two keys with the same
    /// hash share a lock.
    /// </summary>
    public static ResourceLockSqlDialect PostgreSql { get; } = new(
        "SELECT CASE WHEN pg_try_advisory_lock(hashtextextended({0}, 0)) THEN 1 ELSE 0 END AS \"Value\"",
        "SELECT pg_advisory_unlock(hashtextextended({0}, 0))");

    /// <summary>
    /// SQL Server application locks owned by the session: <c>sp_getapplock</c> with <c>@LockTimeout = 0</c> (a result of
    /// 0 or more means taken) and <c>sp_releaseapplock</c>. The key is the resource name, at most 255 characters.
    /// </summary>
    public static ResourceLockSqlDialect SqlServer { get; } = new(
        "DECLARE @result int; EXEC @result = sp_getapplock @Resource = {0}, @LockMode = 'Exclusive', @LockOwner = 'Session', @LockTimeout = 0; SELECT CASE WHEN @result >= 0 THEN 1 ELSE 0 END AS [Value];",
        "EXEC sp_releaseapplock @Resource = {0}, @LockOwner = 'Session';");

    /// <summary>Takes the lock for <c>{0}</c> without waiting and returns <c>Value</c> = 1 when taken.</summary>
    public string TryAcquireSql { get; }

    /// <summary>Releases the lock for <c>{0}</c>.</summary>
    public string ReleaseSql { get; }
}
