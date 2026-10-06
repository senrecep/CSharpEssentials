using CSharpEssentials.Enums;

namespace CSharpEssentials.EntityFrameworkCore;

/// <summary>
/// Read-only SQL that lists the values of an enum column a conversion or check constraint would reject (design section 12.2).
/// Run it before deploying <see cref="EnumMigrationBuilderExtensions.ConvertEnumColumn{TEnum}(Microsoft.EntityFrameworkCore.Migrations.MigrationBuilder, string, string, EnumStoredAs, EnumStorage, string?, string?)"/>
/// or a new check constraint, and fix or map every value it returns.
/// </summary>
public static class EnumDataAudit
{
    /// <summary>
    /// Returns <c>SELECT column AS "Value", count(*) AS "Count" ... GROUP BY column</c> for the non-null values of
    /// <paramref name="table"/>.<paramref name="column"/> that match no spelling of <typeparamref name="TEnum"/>: no wire name, member
    /// name, alias, camelCase or legacy snake case name, or defined number. For flags every comma separated token is checked;
    /// for <see cref="EnumStoredAs.Integer"/> columns undefined numbers (or undefined bits) are listed.
    /// </summary>
    /// <param name="table">The table name.</param>
    /// <param name="column">The column name.</param>
    /// <param name="schema">The schema, or <see langword="null"/> for the default schema.</param>
    /// <param name="storedAs">What the column holds: <see cref="EnumStoredAs.Integer"/> for numbers, any other value for text.</param>
    /// <param name="provider">The EF Core provider name (<c>DbContext.Database.ProviderName</c>); PostgreSQL when <see langword="null"/>. PostgreSQL and SQLite are supported.</param>
    /// <returns>The SQL query; it changes nothing.</returns>
    public static string Sql<TEnum>(string table, string column, string? schema = null, EnumStoredAs storedAs = EnumStoredAs.Text, string? provider = null)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(table))
            throw new ArgumentException("A table name is required.", nameof(table));
        if (string.IsNullOrWhiteSpace(column))
            throw new ArgumentException("A column name is required.", nameof(column));

        var dialect = EnumSqlDialect.For(provider ?? EnumStorageConvention.PostgresProvider);
        EnumInfo<TEnum> info = EnumMetadata.Get<TEnum>();
        EnumConversionSql<TEnum> sql = new(info, dialect, $"{table}.{column}");
        string columnSql = EnumSqlDialect.Identifier(column);
        return $"SELECT {columnSql} AS {EnumSqlDialect.Identifier("Value")}, count(*) AS {EnumSqlDialect.Identifier("Count")} FROM {dialect.Table(table, schema)} " +
            $"WHERE {Condition(sql, info.IsFlags, storedAs, table, columnSql)} GROUP BY {columnSql} ORDER BY {columnSql};";
    }

    private static string Condition<TEnum>(EnumConversionSql<TEnum> sql, bool flags, EnumStoredAs storedAs, string table, string columnSql)
        where TEnum : struct, Enum
    {
        if (storedAs == EnumStoredAs.Integer)
            return sql.IsUnknownNumber(columnSql);
        if (flags)
            return $"{columnSql} IS NOT NULL AND {sql.HasUnknownToken($"{EnumSqlDialect.Identifier(table)}.{columnSql}")}";
        return sql.IsUnknownText(columnSql);
    }
}
