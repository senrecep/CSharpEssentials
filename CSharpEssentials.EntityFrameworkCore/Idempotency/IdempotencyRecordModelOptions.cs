namespace CSharpEssentials.EntityFrameworkCore.Idempotency;

/// <summary>
/// How <see cref="IdempotencyRecordModelBuilderExtensions.AddIdempotencyRecords"/> maps <see cref="IdempotencyRecord"/>.
/// <para>
/// Records are unique on <see cref="IdempotencyRecord.KeyHash"/>, a fixed-length binary hash, so no collation can merge
/// two keys. The text columns (<see cref="IdempotencyRecord.Namespace"/>, <see cref="IdempotencyRecord.Scope"/>,
/// <see cref="IdempotencyRecord.Key"/>) only narrow the store's updates and are compared ordinally after every read;
/// they get the binary collation of a known provider so that queries of your own compare them as the store does.
/// </para>
/// </summary>
public sealed class IdempotencyRecordModelOptions
{
    /// <summary>The default <see cref="TableName"/>.</summary>
    public const string DefaultTableName = "IdempotencyRecords";

    private const string PostgreSqlProvider = "Npgsql.EntityFrameworkCore.PostgreSQL";
    private const string SqlServerProvider = "Microsoft.EntityFrameworkCore.SqlServer";
    private const string SqliteProvider = "Microsoft.EntityFrameworkCore.Sqlite";
    private const string PomeloMySqlProvider = "Pomelo.EntityFrameworkCore.MySql";
    private const string OracleMySqlProvider = "MySql.EntityFrameworkCore";

    /// <summary>The table of the records. Default: <see cref="DefaultTableName"/>.</summary>
    public string TableName { get; set; } = DefaultTableName;

    /// <summary>The schema of the table; <see langword="null"/> (the default) uses the model's default schema.</summary>
    public string? Schema { get; set; }

    /// <summary>
    /// The collation of the namespace, scope and key columns. <see langword="null"/> (the default) picks the binary
    /// collation of the provider: <c>C</c> on PostgreSQL, <c>Latin1_General_100_BIN2</c> on SQL Server,
    /// <c>utf8mb4_0900_bin</c> on MySQL (Pomelo and Oracle; set <c>utf8mb4_nopad_bin</c> on MariaDB, which lacks it)
    /// and <c>BINARY</c> on SQLite, and keeps the database default for any other provider.
    /// </summary>
    public string? KeyCollation { get; set; }

    /// <summary><see cref="KeyCollation"/>, or the binary collation of <paramref name="providerName"/> when it is not set.</summary>
    /// <param name="providerName">The EF Core provider, as <c>DbContext.Database.ProviderName</c> reports it.</param>
    /// <returns>The collation, or <see langword="null"/> for the database default.</returns>
    internal string? ResolveKeyCollation(string? providerName) =>
        KeyCollation ?? providerName switch
        {
            PostgreSqlProvider => "C",
            SqlServerProvider => "Latin1_General_100_BIN2",
            SqliteProvider => "BINARY",
            PomeloMySqlProvider or OracleMySqlProvider => "utf8mb4_0900_bin",
            _ => null,
        };
}
