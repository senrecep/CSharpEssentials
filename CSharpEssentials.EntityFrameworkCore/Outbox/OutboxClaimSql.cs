using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace CSharpEssentials.EntityFrameworkCore.Outbox;

/// <summary>
/// Writes the claim statement of <see cref="SqlOutboxMessageClaimer"/> for a dialect, from the table and column names
/// the model maps <see cref="OutboxMessage"/> to, so naming conventions, <c>TableName</c> and <c>Schema</c> are followed.
/// The statement uses the <c>FromSqlRaw</c> placeholders <c>{0}</c> claim token, <c>{1}</c> lease end, <c>{2}</c> now and
/// <c>{3}</c> batch size, and returns every column of the claimed rows. Print it to review it or to write your own.
/// </summary>
public static class OutboxClaimSql
{
    /// <summary>Builds the claim statement of <paramref name="dialect"/> for the <see cref="OutboxMessage"/> mapping of <paramref name="context"/>.</summary>
    /// <param name="context">A context that maps <see cref="OutboxMessage"/>; only its model and SQL generation helper are used.</param>
    /// <param name="dialect">The SQL dialect.</param>
    /// <returns>The SQL text.</returns>
    /// <exception cref="InvalidOperationException">The context does not map <see cref="OutboxMessage"/> to a table.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="dialect"/> is not defined.</exception>
    public static string Build(DbContext context, OutboxClaimDialect dialect)
    {
        ArgumentNullException.ThrowIfNull(context);

        IEntityType entity = context.Model.FindEntityType(typeof(OutboxMessage))
            ?? throw new InvalidOperationException(
                $"{context.GetType().Name} does not map {nameof(OutboxMessage)}. Call modelBuilder.{nameof(OutboxModelBuilderExtensions.AddOutboxMessages)}() in OnModelCreating.");
        string tableName = entity.GetTableName()
            ?? throw new InvalidOperationException($"{nameof(OutboxMessage)} must be mapped to a table.");
        var storeObject = StoreObjectIdentifier.Table(tableName, entity.GetSchema());
        ISqlGenerationHelper helper = context.GetService<ISqlGenerationHelper>();

        string Column(string property) =>
            Escape(helper.DelimitIdentifier(entity.FindProperty(property)?.GetColumnName(storeObject)
                ?? throw new InvalidOperationException($"{nameof(OutboxMessage)}.{property} is not mapped to a column.")));

        string table = Escape(helper.DelimitIdentifier(tableName, entity.GetSchema()));
        string id = Column(nameof(OutboxMessage.Id));
        string token = Column(nameof(OutboxMessage.ClaimToken));
        string next = Column(nameof(OutboxMessage.NextAttemptAt));
        string attempts = Column(nameof(OutboxMessage.Attempts));
        string due =
            $"{Column(nameof(OutboxMessage.ProcessedAt))} IS NULL AND {Column(nameof(OutboxMessage.DeadLetteredAt))} IS NULL AND {next} <= {{2}}";
        string order = $"{Column(nameof(OutboxMessage.OccurredAt))}, {id}";
        string set = $"{token} = {{0}}, {next} = {{1}}, {attempts} = {attempts} + 1";

        return dialect switch
        {
            OutboxClaimDialect.PostgreSql =>
                $"UPDATE {table} SET {set} WHERE {id} IN (SELECT {id} FROM {table} WHERE {due} ORDER BY {order} LIMIT {{3}} FOR UPDATE SKIP LOCKED) RETURNING *",
            OutboxClaimDialect.SqlServer =>
                $"WITH claimed AS (SELECT TOP ({{3}}) * FROM {table} WITH (UPDLOCK, READPAST, ROWLOCK) WHERE {due} ORDER BY {order}) UPDATE claimed SET {set} OUTPUT inserted.*",
            OutboxClaimDialect.Sqlite =>
                $"UPDATE {table} SET {set} WHERE {id} IN (SELECT {id} FROM {table} WHERE {due} ORDER BY {order} LIMIT {{3}}) RETURNING *",
            _ => throw new ArgumentOutOfRangeException(nameof(dialect), dialect, "Unknown outbox claim dialect."),
        };
    }

    private static string Escape(string identifier) =>
        identifier.Replace("{", "{{", StringComparison.Ordinal).Replace("}", "}}", StringComparison.Ordinal);
}
