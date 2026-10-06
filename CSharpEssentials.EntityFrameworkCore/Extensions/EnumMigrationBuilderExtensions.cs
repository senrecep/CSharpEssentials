using CSharpEssentials.Enums;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace CSharpEssentials.EntityFrameworkCore;

/// <summary>
/// Converts existing enum data when its storage changes (design section 12). PostgreSQL and SQLite are supported.
/// </summary>
/// <remarks>
/// <para>
/// A conversion replaces the <c>AlterColumn</c> that <c>dotnet ef migrations add</c> generates for the column: delete that call
/// (analyzer CSE0014 reports a migration that keeps both) and run the operations in this order: <c>DropCheckConstraint</c>,
/// the conversion, <c>AddCheckConstraint</c>.
/// </para>
/// <para>
/// The SQL is generated from the enum metadata when the migration is built and holds only literals. No conversion writes
/// <c>NULL</c>: text targets keep a value that matches no spelling, which then fails the check constraint added after the
/// conversion; integer targets keep integer text and abort the migration on any other text, naming the column, the enum and the
/// value. Run <see cref="EnumDataAudit"/> first to list such values.
/// </para>
/// </remarks>
public static class EnumMigrationBuilderExtensions
{
    /// <summary>
    /// Converts <paramref name="table"/>.<paramref name="column"/> from <paramref name="from"/> to wire names
    /// (<see cref="EnumStorage.String"/>) or numbers (<see cref="EnumStorage.Integer"/>), changing the column type in the same step.
    /// </summary>
    /// <param name="migrationBuilder">The migration builder.</param>
    /// <param name="table">The table name.</param>
    /// <param name="column">The column name.</param>
    /// <param name="from">The format the column holds. <see cref="EnumStoredAs.Text"/> reads every spelling; <see cref="EnumStoredAs.FlagsText"/> reads comma separated flags.</param>
    /// <param name="to">The storage to convert to. Flags enums convert to <see cref="EnumStorage.Integer"/> only.</param>
    /// <param name="schema">The schema, or <see langword="null"/> for the default schema.</param>
    /// <param name="type">The store type of the converted column; defaults to <c>text</c> or the integer type of the enum.</param>
    /// <remarks>
    /// Flags text becomes a bitmask through a temporary column <c>{column}__cse</c> filled with <c>bit_or</c> (PostgreSQL rejects
    /// subqueries in <c>ALTER COLUMN ... USING</c>); the column keeps its nullability and indexes. A column default that does not
    /// cast to the new type must be dropped before the conversion. Text to wire names is idempotent; the other conversions are
    /// guarded by the migration history like any migration.
    /// </remarks>
    /// <returns>The same builder.</returns>
    public static MigrationBuilder ConvertEnumColumn<TEnum>(
        this MigrationBuilder migrationBuilder,
        string table,
        string column,
        EnumStoredAs from,
        EnumStorage to,
        string? schema = null,
        string? type = null)
        where TEnum : struct, Enum
    {
        if (to is not (EnumStorage.String or EnumStorage.Integer))
            throw new ArgumentOutOfRangeException(nameof(to), to, $"Convert to {nameof(EnumStorage)}.{nameof(EnumStorage.String)} or {nameof(EnumStorage)}.{nameof(EnumStorage.Integer)}.");

        return Convert<TEnum>(migrationBuilder, table, column, schema, type, from, to == EnumStorage.Integer, textFormat: null);
    }

    /// <summary>
    /// Converts <paramref name="table"/>.<paramref name="column"/> from <paramref name="from"/> to a legacy format, typically in
    /// <c>Down()</c>. Text spellings cannot be restored byte for byte, so <c>Down()</c> restores the format, not the original values.
    /// </summary>
    /// <param name="migrationBuilder">The migration builder.</param>
    /// <param name="table">The table name.</param>
    /// <param name="column">The column name.</param>
    /// <param name="from">The format the column holds.</param>
    /// <param name="to">The legacy format to write. <see cref="EnumStoredAs.Text"/> is not a write format.</param>
    /// <param name="schema">The schema, or <see langword="null"/> for the default schema.</param>
    /// <param name="type">The store type of the converted column, for example <c>character varying(32)</c>; defaults to <c>text</c> or the integer type of the enum.</param>
    /// <returns>The same builder.</returns>
    public static MigrationBuilder ConvertEnumColumn<TEnum>(
        this MigrationBuilder migrationBuilder,
        string table,
        string column,
        EnumStoredAs from,
        EnumStoredAs to,
        string? schema = null,
        string? type = null)
        where TEnum : struct, Enum
    {
        if (to == EnumStoredAs.Text)
            throw new ArgumentException($"{nameof(EnumStoredAs)}.{nameof(EnumStoredAs.Text)} is a conversion source, not a write format.", nameof(to));

        bool integer = to == EnumStoredAs.Integer;
        return Convert<TEnum>(migrationBuilder, table, column, schema, type, from, integer, integer ? null : to);
    }

    /// <summary>
    /// Converts the enum at <paramref name="path"/> inside the PostgreSQL <c>jsonb</c> column <paramref name="table"/>.<paramref name="column"/>
    /// to wire names or numbers. Values that match no spelling, JSON <c>null</c> and missing paths are left unchanged; the document
    /// is never set to <c>NULL</c>. Array paths (<c>items[*].status</c>) are not supported.
    /// </summary>
    /// <param name="migrationBuilder">The migration builder.</param>
    /// <param name="table">The table name.</param>
    /// <param name="column">The <c>jsonb</c> column name.</param>
    /// <param name="path">The property names from the document root to the enum value.</param>
    /// <param name="schema">The schema, or <see langword="null"/> for the default schema.</param>
    /// <param name="to">Wire names (default) or numbers, for example in <c>Down()</c>.</param>
    /// <returns>The same builder.</returns>
    public static MigrationBuilder ConvertEnumJsonPath<TEnum>(
        this MigrationBuilder migrationBuilder,
        string table,
        string column,
        IReadOnlyList<string> path,
        string? schema = null,
        EnumStorage to = EnumStorage.String)
        where TEnum : struct, Enum
    {
        EnumSqlDialect dialect = Dialect(migrationBuilder, table, column);
        if (path is null || path.Count == 0 || path.Any(string.IsNullOrEmpty))
            throw new ArgumentException("The path needs at least one property name, and no empty names.", nameof(path));
        if (to is not (EnumStorage.String or EnumStorage.Integer))
            throw new ArgumentOutOfRangeException(nameof(to), to, $"Convert to {nameof(EnumStorage)}.{nameof(EnumStorage.String)} or {nameof(EnumStorage)}.{nameof(EnumStorage.Integer)}.");
        if (!dialect.IsPostgres)
            throw new NotSupportedException($"{nameof(ConvertEnumJsonPath)} supports PostgreSQL jsonb columns only.");

        EnumInfo<TEnum> info = EnumMetadata.Get<TEnum>();
        if (info.IsFlags)
            throw new NotSupportedException($"{nameof(ConvertEnumJsonPath)} does not convert flags enums such as '{typeof(TEnum).Name}'.");

        EnumConversionSql<TEnum> sql = new(info, dialect, $"{table}.{column}");
        string tableSql = dialect.Table(table, schema);
        string columnSql = EnumSqlDialect.Identifier(column);
        string pathSql = $"ARRAY[{string.Join(", ", path.Select(EnumSqlDialect.Literal))}]::text[]";
        string text = $"({columnSql} #>> {pathSql})";
        string value = to == EnumStorage.Integer
            ? $"to_jsonb(CAST({sql.TextToKnownNumber(text)} AS {sql.IntegerType}))"
            : $"to_jsonb(CAST({sql.TextToKnownName(text)} AS text))";

        migrationBuilder.Sql(
            $"UPDATE {tableSql} SET {columnSql} = jsonb_set({columnSql}, {pathSql}, {value}) " +
            $"WHERE {columnSql} #> {pathSql} IS NOT NULL AND {value} IS NOT NULL AND {columnSql} #> {pathSql} IS DISTINCT FROM {value};");
        return migrationBuilder;
    }

    private static MigrationBuilder Convert<TEnum>(
        MigrationBuilder migrationBuilder,
        string table,
        string column,
        string? schema,
        string? type,
        EnumStoredAs from,
        bool integer,
        EnumStoredAs? textFormat)
        where TEnum : struct, Enum
    {
        EnumSqlDialect dialect = Dialect(migrationBuilder, table, column);
        if (!Enum.IsDefined(from))
            throw new ArgumentOutOfRangeException(nameof(from), from, null);

        EnumInfo<TEnum> info = EnumMetadata.Get<TEnum>();
        bool fromInteger = from == EnumStoredAs.Integer;
        if (fromInteger && integer)
            throw new ArgumentException("The column already holds numbers; there is nothing to convert.", nameof(from));
        if (!info.IsFlags && (from == EnumStoredAs.FlagsText || textFormat == EnumStoredAs.FlagsText))
            throw new ArgumentException($"{nameof(EnumStoredAs)}.{nameof(EnumStoredAs.FlagsText)} applies to [Flags] enums only; '{typeof(TEnum).Name}' is not one.", nameof(from));
        if (info.IsFlags && !(fromInteger ^ integer))
            throw new NotSupportedException($"Flags enum '{typeof(TEnum).Name}' converts between numbers and member name text only.");
        if (info.IsFlags && !integer && textFormat is null)
            throw new NotSupportedException($"Flags enum '{typeof(TEnum).Name}' converts to {nameof(EnumStorage)}.{nameof(EnumStorage.Integer)} or to a {nameof(EnumStoredAs)} text format, not to wire name storage.");
        if (info.IsFlags && dialect.IsPostgres && info.UnderlyingType == typeof(ulong))
            throw new NotSupportedException($"Flags enum '{typeof(TEnum).Name}' has a ulong underlying type, stored as numeric on PostgreSQL, which has no bitwise operators.");

        EnumConversionSql<TEnum> sql = new(info, dialect, $"{table}.{column}");
        string tableSql = dialect.Table(table, schema);
        string columnSql = EnumSqlDialect.Identifier(column);
        string qualifiedColumn = $"{EnumSqlDialect.Identifier(table)}.{columnSql}";
        string targetType = type ?? (integer ? sql.IntegerType : dialect.TextType);
        string alter = $"ALTER TABLE {tableSql} ALTER COLUMN {columnSql} TYPE";

        if (!fromInteger && !integer)
        {
            string name = sql.TextToName(columnSql, textFormat);
            if (dialect.IsPostgres)
                migrationBuilder.Sql($"{alter} text;");
            string update = $"UPDATE {tableSql} SET {columnSql} = {name} WHERE {columnSql} IS NOT NULL AND {columnSql} <> {name};";
            migrationBuilder.Sql(dialect.IsPostgres ? update : SqliteUpdate(update));
            if (dialect.IsPostgres && !string.Equals(targetType, "text", StringComparison.OrdinalIgnoreCase))
                migrationBuilder.Sql($"{alter} {targetType};");
            return migrationBuilder;
        }

        if (dialect.IsPostgres && info.IsFlags && integer)
        {
            string temporary = EnumSqlDialect.Identifier(column + "__cse");
            migrationBuilder.Sql($"ALTER TABLE {tableSql} ADD COLUMN {temporary} {targetType};");
            migrationBuilder.Sql($"UPDATE {tableSql} SET {temporary} = {sql.FlagsTextToNumber(qualifiedColumn)} WHERE {columnSql} IS NOT NULL;");
            migrationBuilder.Sql($"{alter} {targetType} USING {temporary};");
            migrationBuilder.Sql($"ALTER TABLE {tableSql} DROP COLUMN {temporary};");
            return migrationBuilder;
        }

        string converted = (integer, info.IsFlags) switch
        {
            (true, true) => sql.FlagsTextToNumber(qualifiedColumn),
            (true, false) => sql.TextToNumber(columnSql),
            _ => sql.NumberToName(columnSql, textFormat),
        };

        if (dialect.IsPostgres)
        {
            migrationBuilder.Sql($"{alter} {targetType} USING {converted};");
            return migrationBuilder;
        }

        // SQLite cannot change a column type: convert in place, then let EF rebuild the table with the target model's column.
        migrationBuilder.Sql(SqliteUpdate($"UPDATE {tableSql} SET {columnSql} = {converted} WHERE {columnSql} IS NOT NULL;"));
        Type clrType = integer ? info.UnderlyingType : typeof(string);
        Type oldClrType = integer ? typeof(string) : info.UnderlyingType;
        migrationBuilder.Operations.Add(new AlterColumnOperation
        {
            Table = table,
            Schema = schema,
            Name = column,
            ClrType = clrType,
            ColumnType = targetType,
            IsNullable = true,
            OldColumn = new AddColumnOperation
            {
                Table = table,
                Schema = schema,
                Name = column,
                ClrType = oldClrType,
                ColumnType = integer ? dialect.TextType : sql.IntegerType,
                IsNullable = true,
            },
        });
        return migrationBuilder;
    }

    // EF runs the SQLite table rebuilds of a migration after its SQL operations, so the old check constraint is still in place
    // during the update. The rebuild copies every row into a table with the target constraints, which then reject bad values.
    private static string SqliteUpdate(string update) =>
        $"PRAGMA ignore_check_constraints = ON; {update} PRAGMA ignore_check_constraints = OFF;";

    private static EnumSqlDialect Dialect(MigrationBuilder migrationBuilder, string table, string column)
    {
        _ = migrationBuilder ?? throw new ArgumentNullException(nameof(migrationBuilder));
        if (string.IsNullOrWhiteSpace(table))
            throw new ArgumentException("A table name is required.", nameof(table));
        if (string.IsNullOrWhiteSpace(column))
            throw new ArgumentException("A column name is required.", nameof(column));

        return EnumSqlDialect.For(migrationBuilder.ActiveProvider
            ?? throw new InvalidOperationException("The migration builder has no active provider; enum conversions are generated per provider."));
    }
}
