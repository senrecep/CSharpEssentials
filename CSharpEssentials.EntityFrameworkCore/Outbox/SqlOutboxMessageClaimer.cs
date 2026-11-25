using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CSharpEssentials.EntityFrameworkCore.Outbox;

/// <summary>
/// <see cref="IOutboxMessageClaimer"/> that claims with one SQL statement through <c>FromSqlRaw</c>, so concurrent
/// processors skip each other's rows instead of competing for them (PostgreSQL <c>FOR UPDATE SKIP LOCKED</c>, SQL
/// Server <c>READPAST</c>). No provider package is referenced: the statement is plain text, written by
/// <see cref="OutboxClaimSql.Build"/> for a <see cref="OutboxClaimDialect"/> or supplied by you. Set it on
/// <see cref="OutboxProcessorOptions.Claimer"/>.
/// <para>
/// A custom statement must follow the <see cref="IOutboxMessageClaimer"/> contract, use the placeholders <c>{0}</c>
/// claim token, <c>{1}</c> lease end, <c>{2}</c> now and <c>{3}</c> batch size, and return every column of the claimed rows.
/// </para>
/// </summary>
public sealed class SqlOutboxMessageClaimer : IOutboxMessageClaimer
{
    private readonly ConditionalWeakTable<IModel, string> _sqlByModel = [];
    private readonly Func<DbContext, string> _buildSql;

    /// <summary>Claims with the statement <see cref="OutboxClaimSql.Build"/> writes for <paramref name="dialect"/>.</summary>
    /// <param name="dialect">The SQL dialect of the database.</param>
    public SqlOutboxMessageClaimer(OutboxClaimDialect dialect)
    {
        if (!Enum.IsDefined(dialect))
            throw new ArgumentOutOfRangeException(nameof(dialect), dialect, "Unknown outbox claim dialect.");
        _buildSql = context => OutboxClaimSql.Build(context, dialect);
    }

    /// <summary>Claims with your own statement.</summary>
    /// <param name="sql">The claim statement; see the class remarks for its placeholders.</param>
    /// <exception cref="ArgumentException"><paramref name="sql"/> is empty.</exception>
    public SqlOutboxMessageClaimer(string sql)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        _buildSql = _ => sql;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<OutboxMessage>> ClaimAsync(DbContext context, OutboxClaimRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(request);

        string sql = _sqlByModel.GetValue(context.Model, _ => _buildSql(context));
        return await context.Set<OutboxMessage>()
            .FromSqlRaw(sql, request.ClaimToken, request.LeaseUntil, request.Now, request.BatchSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
