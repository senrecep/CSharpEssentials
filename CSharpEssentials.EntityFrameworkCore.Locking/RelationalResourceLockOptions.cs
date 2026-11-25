namespace CSharpEssentials.EntityFrameworkCore.Locking;

/// <summary>Options of <see cref="RelationalResourceLock{TContext}"/>.</summary>
public sealed class RelationalResourceLockOptions
{
    /// <summary>The SQL of the database. Required.</summary>
    public ResourceLockSqlDialect? Dialect { get; set; }

    /// <summary>How long <c>AcquireAsync</c> waits between attempts while the lock is held elsewhere. Default: 100 ms.</summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromMilliseconds(100);
}
