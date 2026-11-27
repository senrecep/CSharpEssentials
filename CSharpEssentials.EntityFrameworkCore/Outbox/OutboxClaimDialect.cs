namespace CSharpEssentials.EntityFrameworkCore.Outbox;

/// <summary>The SQL dialects <see cref="OutboxClaimSql.Build"/> can write a claim statement for.</summary>
public enum OutboxClaimDialect
{
    /// <summary>PostgreSQL: <c>UPDATE … WHERE id IN (SELECT … FOR UPDATE SKIP LOCKED) RETURNING *</c>.</summary>
    PostgreSql,

    /// <summary>SQL Server: a CTE over <c>WITH (UPDLOCK, READPAST, ROWLOCK)</c> updated with <c>OUTPUT inserted.*</c>.</summary>
    SqlServer,

    /// <summary>SQLite 3.35 or later: <c>UPDATE … RETURNING *</c>. SQLite serializes writers, so no row locking is needed.</summary>
    Sqlite,
}
