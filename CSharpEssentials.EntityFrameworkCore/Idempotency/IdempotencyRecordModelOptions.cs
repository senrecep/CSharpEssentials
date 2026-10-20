namespace CSharpEssentials.EntityFrameworkCore.Idempotency;

/// <summary>
/// How <see cref="IdempotencyRecordModelBuilderExtensions.AddIdempotencyRecords"/> maps <see cref="IdempotencyRecord"/>.
/// <para>
/// Idempotency keys and scopes compare byte for byte: <c>key-a</c> and <c>KEY-A</c> are two keys. The columns of the
/// primary key (<see cref="IdempotencyRecord.Namespace"/>, <see cref="IdempotencyRecord.Scope"/>,
/// <see cref="IdempotencyRecord.Key"/>) therefore need a binary collation. A case- or accent-insensitive default (SQL
/// Server's <c>SQL_Latin1_General_CP1_CI_AS</c>, MySQL's <c>utf8mb4_0900_ai_ci</c>) would make two clients' keys one
/// record and replay one client's response to the other. Set <see cref="KeyCollation"/>, or call
/// <see cref="UseBinaryKeyCollation"/> with the provider name.
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
    /// The collation of the namespace, scope and key columns; <see langword="null"/> (the default) keeps the database
    /// default. It must compare byte for byte; see the type remarks.
    /// </summary>
    public string? KeyCollation { get; set; }

    /// <summary>
    /// Sets <see cref="KeyCollation"/> to the binary collation of the provider: <c>C</c> on PostgreSQL,
    /// <c>Latin1_General_100_BIN2</c> on SQL Server, <c>utf8mb4_bin</c> on MySQL and <c>BINARY</c> on SQLite.
    /// Pass <c>Database.ProviderName</c> from <c>OnModelCreating</c>.
    /// </summary>
    /// <param name="providerName">The EF Core provider, as <c>DbContext.Database.ProviderName</c> reports it.</param>
    /// <returns>These options.</returns>
    /// <exception cref="ArgumentException"><paramref name="providerName"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="NotSupportedException">
    /// The provider has no known binary collation; set <see cref="KeyCollation"/> yourself.
    /// </exception>
    public IdempotencyRecordModelOptions UseBinaryKeyCollation(string? providerName)
    {
        ArgumentException.ThrowIfNullOrEmpty(providerName);

        KeyCollation = providerName switch
        {
            PostgreSqlProvider => "C",
            SqlServerProvider => "Latin1_General_100_BIN2",
            SqliteProvider => "BINARY",
            PomeloMySqlProvider or OracleMySqlProvider => "utf8mb4_bin",
            _ => throw new NotSupportedException(
                $"No binary collation is known for the provider '{providerName}'. Set {nameof(KeyCollation)} to a collation that compares byte for byte."),
        };
        return this;
    }
}
