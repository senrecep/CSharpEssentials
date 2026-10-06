namespace CSharpEssentials.EntityFrameworkCore;

/// <summary>
/// The SQL spelling of the enum migration helpers for one provider: identifier and literal quoting, integer store types and the
/// expressions that make a conversion fail loudly.
/// </summary>
/// <remarks>
/// Migrations get only the provider name, not its services, so quoting follows the provider's documented rules here instead of
/// its <c>ISqlGenerationHelper</c> (whose construction outside a context is an EF Core internal API).
/// </remarks>
internal sealed class EnumSqlDialect
{
    private static readonly EnumSqlDialect PostgresDialect = new(isPostgres: true);
    private static readonly EnumSqlDialect SqliteDialect = new(isPostgres: false);

    private EnumSqlDialect(bool isPostgres) => IsPostgres = isPostgres;

    public bool IsPostgres { get; }

    public string TextType => IsPostgres ? "text" : "TEXT";

    public static EnumSqlDialect For(string provider) => provider switch
    {
        EnumStorageConvention.PostgresProvider => PostgresDialect,
        EnumStorageConvention.SqliteProvider => SqliteDialect,
        _ => throw new NotSupportedException(
            $"The enum migration helpers support PostgreSQL ({EnumStorageConvention.PostgresProvider}) and SQLite ({EnumStorageConvention.SqliteProvider}); '{provider}' is not supported. Write the conversion SQL with migrationBuilder.Sql."),
    };

    public static string Identifier(string name) => "\"" + name.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";

    public static string Literal(string text) => "'" + text.Replace("'", "''", StringComparison.Ordinal) + "'";

    public string Table(string table, string? schema) =>
        IsPostgres && !string.IsNullOrEmpty(schema) ? $"{Identifier(schema)}.{Identifier(table)}" : Identifier(table);

    public string IntegerType(Type underlyingType)
    {
        if (!IsPostgres)
            return "INTEGER";
        if (underlyingType == typeof(sbyte) || underlyingType == typeof(byte) || underlyingType == typeof(short))
            return "smallint";
        if (underlyingType == typeof(uint) || underlyingType == typeof(long))
            return "bigint";
        return underlyingType == typeof(ulong) ? "numeric(20,0)" : "integer";
    }

    public string Lower(string sql) => $"lower(trim({sql}))";

    public string ToText(string sql) => IsPostgres ? $"CAST({sql} AS text)" : $"CAST({sql} AS TEXT)";

    /// <summary>True when the text <paramref name="sql"/> is an optionally negative decimal integer.</summary>
    public string IsIntegerText(string sql)
    {
        if (IsPostgres)
            return $"{sql} ~ {Literal("^[[:space:]]*-?[0-9]+[[:space:]]*$")}";

        string digits = $"(CASE WHEN trim({sql}) LIKE {Literal("-%")} THEN substr(trim({sql}), 2) ELSE trim({sql}) END)";
        return $"({digits} <> {Literal(string.Empty)} AND {digits} NOT GLOB {Literal("*[^0-9]*")})";
    }

    public string CastInteger(string sql, string integerType) => $"CAST(trim({sql}) AS {integerType})";

    /// <summary>
    /// An expression that aborts the statement with <paramref name="message"/> followed by the offending <paramref name="valueSql"/>.
    /// PostgreSQL reports it as an invalid integer input, SQLite as a bad JSON path; both name the column, enum and value.
    /// </summary>
    public string Fail(string message, string valueSql, string integerType) =>
        IsPostgres
            ? $"CAST({Literal(message)} || {valueSql} AS {integerType})"
            : $"json_extract({Literal("{}")}, {Literal(message)} || {valueSql})";
}
