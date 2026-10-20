using CSharpEssentials.EntityFrameworkCore.Idempotency;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CSharpEssentials.Tests.EntityFrameworkCore.Idempotency;

public sealed class IdempotencyRecordModelTests
{
    [Theory]
    [InlineData("Npgsql.EntityFrameworkCore.PostgreSQL", "C")]
    [InlineData("Microsoft.EntityFrameworkCore.SqlServer", "Latin1_General_100_BIN2")]
    [InlineData("Microsoft.EntityFrameworkCore.Sqlite", "BINARY")]
    [InlineData("Pomelo.EntityFrameworkCore.MySql", "utf8mb4_bin")]
    [InlineData("MySql.EntityFrameworkCore", "utf8mb4_bin")]
    public void UseBinaryKeyCollation_Should_Pick_The_Binary_Collation_Of_The_Provider(string provider, string collation)
    {
        IdempotencyRecordModelOptions options = new();

        options.UseBinaryKeyCollation(provider);

        options.KeyCollation.Should().Be(collation);
    }

    [Fact]
    public void UseBinaryKeyCollation_Should_Throw_For_An_Unknown_Provider()
    {
        IdempotencyRecordModelOptions options = new();

        Action unknown = () => options.UseBinaryKeyCollation("Microsoft.EntityFrameworkCore.InMemory");

        unknown.Should().Throw<NotSupportedException>().WithMessage("*KeyCollation*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void UseBinaryKeyCollation_Should_Throw_Without_A_Provider(string? provider)
    {
        IdempotencyRecordModelOptions options = new();

        Action missing = () => options.UseBinaryKeyCollation(provider);

        missing.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddIdempotencyRecords_Should_Throw_For_An_Empty_Table_Name()
    {
        ModelBuilder modelBuilder = new();

        Action empty = () => modelBuilder.AddIdempotencyRecords(options => options.TableName = string.Empty);

        empty.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddIdempotencyRecords_Should_Key_The_Table_By_Namespace_Scope_And_Key_With_The_Collation()
    {
        ModelBuilder modelBuilder = new();

        modelBuilder.AddIdempotencyRecords(options =>
        {
            options.TableName = "records";
            options.Schema = "idem";
            options.KeyCollation = "C";
        });
        IReadOnlyEntityType entity = modelBuilder.Model.FindEntityType(typeof(IdempotencyRecord))!;

        entity.GetTableName().Should().Be("records");
        entity.GetSchema().Should().Be("idem");
        entity.FindPrimaryKey()!.Properties.Select(property => property.Name).Should().Equal("Namespace", "Scope", "Key");
        entity.GetIndexes().Should().ContainSingle(index => index.Properties.Single().Name == nameof(IdempotencyRecord.ExpiresAt));
        entity.GetProperties().Where(property => property.GetCollation() == "C").Select(property => property.Name)
            .Should().BeEquivalentTo("Namespace", "Scope", "Key");
        entity.FindProperty(nameof(IdempotencyRecord.Payload))!.IsNullable.Should().BeTrue();
        entity.FindProperty(nameof(IdempotencyRecord.Token))!.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void AddIdempotencyRecords_Should_Keep_The_Database_Collation_By_Default()
    {
        ModelBuilder modelBuilder = new();

        modelBuilder.AddIdempotencyRecords();
        IReadOnlyEntityType entity = modelBuilder.Model.FindEntityType(typeof(IdempotencyRecord))!;

        entity.GetTableName().Should().Be(IdempotencyRecordModelOptions.DefaultTableName);
        entity.GetProperties().Should().OnlyContain(property => property.GetCollation() == null);
    }
}
