using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace CSharpEssentials.Tests.EntityFrameworkCore.EnumColumns;

/// <summary>Argument checks and generated operations of the enum migration helpers, without a database.</summary>
public sealed class EnumMigrationBuilderExtensionsTests
{
    private const string Postgres = "Npgsql.EntityFrameworkCore.PostgreSQL";
    private const string Sqlite = "Microsoft.EntityFrameworkCore.Sqlite";

    [Fact]
    public void ConvertEnumColumn_Should_AlterTypeInOneStatement_When_Postgres()
    {
        MigrationBuilder builder = new(Postgres);

        builder.ConvertEnumColumn<StoredOrderStatus>("orders", "Status", from: EnumStoredAs.Integer, to: EnumStorage.String);

        builder.Operations.Should().ContainSingle().Which.Should().BeOfType<SqlOperation>()
            .Which.Sql.Should().StartWith("ALTER TABLE \"orders\" ALTER COLUMN \"Status\" TYPE text USING CASE \"Status\" WHEN 0 THEN 'pending'")
            .And.NotContain("NULL");
    }

    [Fact]
    public void ConvertEnumColumn_Should_AddRebuildOperation_When_Sqlite()
    {
        MigrationBuilder builder = new(Sqlite);

        builder.ConvertEnumColumn<StoredOrderStatus>("orders", "Status", from: EnumStoredAs.Integer, to: EnumStorage.String);

        builder.Operations.Should().HaveCount(2);
        builder.Operations[0].Should().BeOfType<SqlOperation>().Which.Sql.Should().StartWith("PRAGMA ignore_check_constraints = ON; UPDATE \"orders\" SET \"Status\" = ")
            .And.EndWith("PRAGMA ignore_check_constraints = OFF;");
        builder.Operations[1].Should().BeOfType<AlterColumnOperation>().Which.ColumnType.Should().Be("TEXT");
    }

    [Fact]
    public void ConvertEnumColumn_Should_QuoteSchemaAndNames()
    {
        MigrationBuilder builder = new(Postgres);

        builder.ConvertEnumColumn<StoredOrderStatus>("or\"ders", "Sta'tus", from: EnumStoredAs.Integer, to: EnumStorage.String, schema: "sales");

        builder.Operations.OfType<SqlOperation>().Single().Sql.Should().StartWith("ALTER TABLE \"sales\".\"or\"\"ders\" ALTER COLUMN \"Sta'tus\"");
    }

    [Fact]
    public void ConvertEnumColumn_Should_Throw_When_BothSidesAreNumbers()
    {
        Action convert = () => new MigrationBuilder(Postgres).ConvertEnumColumn<StoredOrderStatus>("orders", "Status", from: EnumStoredAs.Integer, to: EnumStorage.Integer);

        convert.Should().Throw<ArgumentException>().WithParameterName("from");
    }

    [Fact]
    public void ConvertEnumColumn_Should_Throw_When_TargetIsText()
    {
        Action convert = () => new MigrationBuilder(Postgres).ConvertEnumColumn<StoredOrderStatus>("orders", "Status", from: EnumStoredAs.Integer, to: EnumStoredAs.Text);

        convert.Should().Throw<ArgumentException>().WithParameterName("to");
    }

    [Fact]
    public void ConvertEnumColumn_Should_Throw_When_TargetStorageIsDefault()
    {
        Action convert = () => new MigrationBuilder(Postgres).ConvertEnumColumn<StoredOrderStatus>("orders", "Status", from: EnumStoredAs.Integer, to: EnumStorage.Default);

        convert.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("to");
    }

    [Fact]
    public void ConvertEnumColumn_Should_Throw_When_FlagsTextIsUsedForAPlainEnum()
    {
        Action convert = () => new MigrationBuilder(Postgres).ConvertEnumColumn<StoredOrderStatus>("orders", "Status", from: EnumStoredAs.FlagsText, to: EnumStorage.Integer);

        convert.Should().Throw<ArgumentException>().WithParameterName("from");
    }

    [Fact]
    public void ConvertEnumColumn_Should_Throw_When_FlagsGoToWireNames()
    {
        Action convert = () => new MigrationBuilder(Postgres).ConvertEnumColumn<StoredPermissions>("orders", "Permissions", from: EnumStoredAs.Integer, to: EnumStorage.String);

        convert.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void ConvertEnumColumn_Should_Throw_When_ProviderIsUnknown()
    {
        Action convert = () => new MigrationBuilder("Microsoft.EntityFrameworkCore.SqlServer").ConvertEnumColumn<StoredOrderStatus>("orders", "Status", from: EnumStoredAs.Integer, to: EnumStorage.String);

        convert.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void ConvertEnumColumn_Should_Throw_When_ProviderIsMissing()
    {
        Action convert = () => new MigrationBuilder(null).ConvertEnumColumn<StoredOrderStatus>("orders", "Status", from: EnumStoredAs.Integer, to: EnumStorage.String);

        convert.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ConvertEnumColumn_Should_Throw_When_ColumnIsMissing()
    {
        Action convert = () => new MigrationBuilder(Postgres).ConvertEnumColumn<StoredOrderStatus>("orders", " ", from: EnumStoredAs.Integer, to: EnumStorage.String);

        convert.Should().Throw<ArgumentException>().WithParameterName("column");
    }

    [Fact]
    public void ConvertEnumJsonPath_Should_Throw_When_ProviderIsSqlite()
    {
        Action convert = () => new MigrationBuilder(Sqlite).ConvertEnumJsonPath<StoredOrderStatus>("documents", "data", ["status"]);

        convert.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void ConvertEnumJsonPath_Should_Throw_When_EnumIsFlags()
    {
        Action convert = () => new MigrationBuilder(Postgres).ConvertEnumJsonPath<StoredPermissions>("documents", "data", ["permissions"]);

        convert.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void ConvertEnumJsonPath_Should_Throw_When_PathIsEmpty()
    {
        Action convert = () => new MigrationBuilder(Postgres).ConvertEnumJsonPath<StoredOrderStatus>("documents", "data", []);

        convert.Should().Throw<ArgumentException>().WithParameterName("path");
    }

    [Fact]
    public void Sql_Should_ListUnknownSpellings_When_ProviderIsOmitted()
    {
        string sql = EnumDataAudit.Sql<StoredOrderStatus>("orders", "Status");

        sql.Should().StartWith("SELECT \"Status\" AS \"Value\", count(*) AS \"Count\" FROM \"orders\" WHERE \"Status\" IS NOT NULL AND lower(trim(\"Status\")) NOT IN ('pending', 'pending_approval', 'shipped', ")
            .And.EndWith(") GROUP BY \"Status\" ORDER BY \"Status\";");
    }
}
