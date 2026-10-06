using System.Diagnostics.CodeAnalysis;
using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.Enums;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace CSharpEssentials.Tests.EntityFrameworkCore.EnumColumns;

/// <summary>
/// The SQL of the SQLite conversions run directly on a pooled file database: quoted names and values, and the
/// <c>ignore_check_constraints</c> pragma, which must never stay on for the next user of a pooled connection.
/// </summary>
public sealed class EnumConversionSqliteSqlTests : IDisposable
{
    private const string Table = "or\"ders";
    private const string Column = "Sta\"tus";

    private readonly string _file = Path.Combine(Path.GetTempPath(), $"cse-enum-{Guid.NewGuid():N}.db");

    private string ConnectionString => $"Data Source={_file};Pooling=True";

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        File.Delete(_file);
    }

    [Fact]
    public void ConvertEnumColumn_Should_ConvertQuotedNamesAndValues()
    {
        Create("('o''pen'), ('Closed'), ('OPEN')");

        Execute(Conversion(EnumStoredAs.Text, EnumStorage.Integer));

        Values().Should().Equal("0", "1", "0");
        IgnoresCheckConstraints().Should().BeFalse();
    }

    [Fact]
    public void ConvertEnumColumn_Should_KeepCheckConstraintsEnforced_When_ConversionFails()
    {
        Create("('o''pen'), ('it''s bogus')");

        Action execute = () => Execute(Conversion(EnumStoredAs.Text, EnumStorage.Integer));

        execute.Should().Throw<SqliteException>().Which.Message.Should()
            .Contain("cannot convert or\"ders.Sta\"tus to QuotedOrderStatus, unknown value: it");
        IgnoresCheckConstraints().Should().BeFalse();
        Values().Should().Equal("o'pen", "it's bogus");
    }

    [Fact]
    public void ConvertEnumColumn_Should_KeepCheckConstraintsEnforced_When_FlagsTokenIsUnknown()
    {
        Create("('Read, Write'), ('Read, Bogus')");

        Action execute = () => Execute(new MigrationBuilder("Microsoft.EntityFrameworkCore.Sqlite")
            .ConvertEnumColumn<StoredPermissions>(Table, Column, from: EnumStoredAs.FlagsText, to: EnumStorage.Integer));

        execute.Should().Throw<SqliteException>().Which.Message.Should().Contain("unknown value:");
        IgnoresCheckConstraints().Should().BeFalse();
    }

    [Fact]
    public void ConvertEnumColumn_Should_ApplyOldCheckConstraintAgain_When_ConversionFails()
    {
        Create("('Closed'), ('bogus')");
        Action execute = () => Execute(Conversion(EnumStoredAs.Text, EnumStorage.Integer));
        execute.Should().Throw<SqliteException>();

        Action insert = () => Run($"INSERT INTO {Quote(Table)} ({Quote(Column)}) VALUES ('not allowed')");

        insert.Should().Throw<SqliteException>().Which.Message.Should().Contain("CHECK constraint failed");
    }

    [Fact]
    public void ConvertEnumColumn_Should_ConvertFlagsText_When_ValuesHaveNoUnknownTokens()
    {
        Create("('Read, Write'), ('write'), (''), ('Read,Delete')");

        Execute(new MigrationBuilder("Microsoft.EntityFrameworkCore.Sqlite")
            .ConvertEnumColumn<StoredPermissions>(Table, Column, from: EnumStoredAs.FlagsText, to: EnumStorage.Integer));

        Values().Should().Equal("3", "2", "0", "5");
    }

    [Theory]
    [InlineData("char(13)")]
    [InlineData("char(10)")]
    [InlineData("char(9)")]
    public void ConvertEnumColumn_Should_ReportUnknownValue_When_AFlagsTokenHasAControlCharacter(string control)
    {
        Create($"('Read, Write'), ('Read, Write' || {control})");

        Action execute = () => Execute(new MigrationBuilder("Microsoft.EntityFrameworkCore.Sqlite")
            .ConvertEnumColumn<StoredPermissions>(Table, Column, from: EnumStoredAs.FlagsText, to: EnumStorage.Integer));

        execute.Should().Throw<SqliteException>().Which.Message.Should().Contain("unknown value:").And.NotContain("malformed");
        IgnoresCheckConstraints().Should().BeFalse();
    }

    // SQLite before 3.45 rejects control characters inside JSON text, so the tokens must not go through json_each.
    [Fact]
    public void ConvertEnumColumn_Should_SplitFlagsTextWithoutJson()
    {
        MigrationBuilder builder = new MigrationBuilder("Microsoft.EntityFrameworkCore.Sqlite")
            .ConvertEnumColumn<StoredPermissions>(Table, Column, from: EnumStoredAs.FlagsText, to: EnumStorage.Integer);
        string audit = EnumDataAudit.Sql<StoredPermissions>(Table, Column, storedAs: EnumStoredAs.FlagsText, provider: "Microsoft.EntityFrameworkCore.Sqlite");

        builder.Operations.OfType<SqlOperation>().Should().AllSatisfy(static operation => operation.Sql.Should().NotContain("json_each"));
        audit.Should().NotContain("json_each");
    }

    [Fact]
    public void Audit_Should_ListFlagsValues_When_TokensHaveControlCharacters()
    {
        Create("('Read, Write'), ('Read' || char(13) || char(10) || ', Write'), ('Read,' || char(9) || 'Write'), ('Read, Write')");

        List<string> unknown = Values(EnumDataAudit.Sql<StoredPermissions>(Table, Column, storedAs: EnumStoredAs.FlagsText, provider: "Microsoft.EntityFrameworkCore.Sqlite"));

        unknown.Should().BeEquivalentTo(@"Read\r\n, Write", @"Read,\tWrite");
    }

    private static MigrationBuilder Conversion(EnumStoredAs from, EnumStorage to) =>
        new MigrationBuilder("Microsoft.EntityFrameworkCore.Sqlite").ConvertEnumColumn<QuotedOrderStatus>(Table, Column, from, to);

    private static string Quote(string name) => "\"" + name.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";

    private void Create(string rows)
    {
        Run($"CREATE TABLE {Quote(Table)} (\"Id\" INTEGER PRIMARY KEY, {Quote(Column)} TEXT " +
            $"CHECK ({Quote(Column)} IS NULL OR {Quote(Column)} <> 'not allowed'))");
        Run($"INSERT INTO {Quote(Table)} ({Quote(Column)}) VALUES {rows}");
    }

    // Only the SQL operations: the rebuild of the AlterColumn operation needs a model.
    private void Execute(MigrationBuilder builder)
    {
        foreach (SqlOperation operation in builder.Operations.OfType<SqlOperation>())
            Run(operation.Sql);
    }

    [SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Test-only helper; the SQL is generated by the migration helpers from test constants, not user input.")]
    private void Run(string sql)
    {
        using SqliteConnection connection = new(ConnectionString);
        connection.Open();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private bool IgnoresCheckConstraints()
    {
        using SqliteConnection connection = new(ConnectionString);
        connection.Open();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "PRAGMA ignore_check_constraints";
        return Convert.ToInt64(command.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture) != 0;
    }

    private List<string> Values() => Values($"SELECT CAST({Quote(Column)} AS TEXT) FROM {Quote(Table)} ORDER BY \"Id\"");

    // The first column of every row, control characters shown as \r, \n and \t.
    [SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Test-only helper; the SQL is generated by the migration helpers from test constants, not user input.")]
    private List<string> Values(string sql)
    {
        using SqliteConnection connection = new(ConnectionString);
        connection.Open();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = sql;
        using SqliteDataReader reader = command.ExecuteReader();
        List<string> values = [];
        while (reader.Read())
        {
            values.Add(reader.GetString(0)
                .Replace("\r", "\\r", StringComparison.Ordinal)
                .Replace("\n", "\\n", StringComparison.Ordinal)
                .Replace("\t", "\\t", StringComparison.Ordinal));
        }

        return values;
    }
}
