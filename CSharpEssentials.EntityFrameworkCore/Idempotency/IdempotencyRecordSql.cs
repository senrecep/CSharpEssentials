using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace CSharpEssentials.EntityFrameworkCore.Idempotency;

/// <summary>
/// The <c>INSERT</c> statements of <see cref="EfCoreIdempotencyRecordStore{TContext}"/>, built once per model from the
/// mapped table and column names, so naming conventions and custom table names are followed. Parameters are the
/// <c>{0}</c> placeholders of <c>ExecuteSqlRaw</c>: key hash, namespace, scope, key, token, fingerprint, then the
/// payload (only in <see cref="InsertCompleted"/>) and the expiry. <c>ON CONFLICT</c> targets the primary key.
/// </summary>
internal sealed class IdempotencyRecordSql
{
    private const string PostgreSqlProvider = "Npgsql.EntityFrameworkCore.PostgreSQL";
    private const string SqliteProvider = "Microsoft.EntityFrameworkCore.Sqlite";

    private static readonly ConditionalWeakTable<IModel, IdempotencyRecordSql> Cache = [];

    private IdempotencyRecordSql(string insertInFlight, string insertCompleted, string? onConflictDoNothing, bool ambientSavepoints)
    {
        AmbientSavepoints = ambientSavepoints;
        InsertInFlight = insertInFlight;
        InsertCompleted = insertCompleted;
        if (onConflictDoNothing is not null)
        {
            InsertInFlightOrNothing = insertInFlight + onConflictDoNothing;
            InsertCompletedOrNothing = insertCompleted + onConflictDoNothing;
        }
    }

    /// <summary>Inserts an in-flight row; raises the provider's unique violation when the key exists.</summary>
    public string InsertInFlight { get; }

    /// <summary>Inserts a completed row; raises the provider's unique violation when the key exists.</summary>
    public string InsertCompleted { get; }

    /// <summary>
    /// <see cref="InsertInFlight"/> with <c>ON CONFLICT (…) DO NOTHING</c>, which affects no row when the key exists;
    /// <see langword="null"/> unless the provider is PostgreSQL or SQLite.
    /// </summary>
    public string? InsertInFlightOrNothing { get; }

    /// <summary><see cref="InsertCompleted"/> with <c>ON CONFLICT (…) DO NOTHING</c>; see <see cref="InsertInFlightOrNothing"/>.</summary>
    public string? InsertCompletedOrNothing { get; }

    /// <summary>
    /// Whether the store takes a <c>SAVEPOINT</c> with plain SQL inside an ambient <c>System.Transactions</c>
    /// transaction: only on PostgreSQL, whose failed statement aborts the whole transaction.
    /// </summary>
    public bool AmbientSavepoints { get; }

    public static IdempotencyRecordSql For(DbContext context)
    {
        IModel model = context.Model;
        return Cache.TryGetValue(model, out IdempotencyRecordSql? sql)
            ? sql
            : Cache.GetValue(model, _ => Create(context));
    }

    private static IdempotencyRecordSql Create(DbContext context)
    {
        IEntityType entity = context.Model.FindEntityType(typeof(IdempotencyRecord))
            ?? throw new InvalidOperationException(
                $"{context.GetType().Name} does not map {nameof(IdempotencyRecord)}. Call modelBuilder.{nameof(IdempotencyRecordModelBuilderExtensions.AddIdempotencyRecords)}() in OnModelCreating.");
        string table = entity.GetTableName()
            ?? throw new InvalidOperationException($"{nameof(IdempotencyRecord)} must be mapped to a table.");
        var storeObject = StoreObjectIdentifier.Table(table, entity.GetSchema());
        ISqlGenerationHelper helper = context.GetService<ISqlGenerationHelper>();

        string Column(string property) =>
            Escape(helper.DelimitIdentifier(entity.FindProperty(property)?.GetColumnName(storeObject)
                ?? throw new InvalidOperationException($"{nameof(IdempotencyRecord)}.{property} is not mapped to a column.")));

        string keyColumns = string.Join(", ", (entity.FindPrimaryKey()
            ?? throw new InvalidOperationException($"{nameof(IdempotencyRecord)} must have a primary key.")).Properties.Select(property => Column(property.Name)));
        string insert =
            $"INSERT INTO {Escape(helper.DelimitIdentifier(table, entity.GetSchema()))} ({Column(nameof(IdempotencyRecord.KeyHash))}, " +
            $"{Column(nameof(IdempotencyRecord.Namespace))}, {Column(nameof(IdempotencyRecord.Scope))}, {Column(nameof(IdempotencyRecord.Key))}, " +
            $"{Column(nameof(IdempotencyRecord.Token))}, {Column(nameof(IdempotencyRecord.Fingerprint))}, {Column(nameof(IdempotencyRecord.Payload))}, " +
            $"{Column(nameof(IdempotencyRecord.ExpiresAt))}) VALUES ({{0}}, {{1}}, {{2}}, {{3}}, {{4}}, {{5}}, ";
        string? onConflictDoNothing = context.Database.ProviderName is PostgreSqlProvider or SqliteProvider
            ? $" ON CONFLICT ({keyColumns}) DO NOTHING"
            : null;

        return new IdempotencyRecordSql(
            insert + "NULL, {6})", insert + "{6}, {7})", onConflictDoNothing, context.Database.ProviderName is PostgreSqlProvider);
    }

    /// <summary>Doubles the braces of an identifier so <c>ExecuteSqlRaw</c> does not read them as placeholders.</summary>
    private static string Escape(string identifier) =>
        identifier.Replace("{", "{{", StringComparison.Ordinal).Replace("}", "}}", StringComparison.Ordinal);
}
