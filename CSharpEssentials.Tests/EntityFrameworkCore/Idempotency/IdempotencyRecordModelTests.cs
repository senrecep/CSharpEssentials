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
    [InlineData("Pomelo.EntityFrameworkCore.MySql", "utf8mb4_0900_bin")]
    [InlineData("MySql.EntityFrameworkCore", "utf8mb4_0900_bin")]
    public void AddIdempotencyRecords_Should_Pick_The_Binary_Collation_Of_The_Provider(string provider, string collation)
    {
        ModelBuilder modelBuilder = new();

        modelBuilder.AddIdempotencyRecords(provider);

        TextColumnCollations(modelBuilder).Should().OnlyContain(found => found == collation).And.HaveCount(3);
    }

    [Theory]
    [InlineData("Microsoft.EntityFrameworkCore.InMemory")]
    [InlineData(null)]
    public void AddIdempotencyRecords_Should_Keep_The_Database_Collation_For_An_Unknown_Provider(string? provider)
    {
        ModelBuilder modelBuilder = new();

        modelBuilder.AddIdempotencyRecords(provider);

        TextColumnCollations(modelBuilder).Should().OnlyContain(found => found == null);
    }

    [Fact]
    public void AddIdempotencyRecords_Should_Prefer_The_Configured_Collation()
    {
        ModelBuilder modelBuilder = new();

        modelBuilder.AddIdempotencyRecords("Npgsql.EntityFrameworkCore.PostgreSQL", options => options.KeyCollation = "ucs_basic");

        TextColumnCollations(modelBuilder).Should().OnlyContain(found => found == "ucs_basic");
    }

    [Fact]
    public void AddIdempotencyRecords_Should_Throw_For_An_Empty_Table_Name()
    {
        ModelBuilder modelBuilder = new();

        Action empty = () => modelBuilder.AddIdempotencyRecords("Npgsql.EntityFrameworkCore.PostgreSQL", options => options.TableName = string.Empty);

        empty.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddIdempotencyRecords_Should_Key_The_Table_By_The_Fixed_Length_Key_Hash()
    {
        ModelBuilder modelBuilder = new();

        modelBuilder.AddIdempotencyRecords(null, options =>
        {
            options.TableName = "records";
            options.Schema = "idem";
        });
        IReadOnlyEntityType entity = modelBuilder.Model.FindEntityType(typeof(IdempotencyRecord))!;
        IReadOnlyProperty keyHash = entity.FindProperty(nameof(IdempotencyRecord.KeyHash))!;

        entity.GetTableName().Should().Be("records");
        entity.GetSchema().Should().Be("idem");
        entity.FindPrimaryKey()!.Properties.Should().ContainSingle().Which.Name.Should().Be(nameof(IdempotencyRecord.KeyHash));
        keyHash.GetMaxLength().Should().Be(32);
        keyHash.IsFixedLength().Should().BeTrue();
        entity.GetIndexes().Should().ContainSingle(index => index.Properties.Single().Name == nameof(IdempotencyRecord.ExpiresAt));
        entity.FindProperty(nameof(IdempotencyRecord.Payload))!.IsNullable.Should().BeTrue();
        entity.FindProperty(nameof(IdempotencyRecord.Key))!.IsNullable.Should().BeFalse();
    }

    private static IEnumerable<string?> TextColumnCollations(ModelBuilder modelBuilder)
    {
        IReadOnlyEntityType entity = modelBuilder.Model.FindEntityType(typeof(IdempotencyRecord))!;
        return new[] { nameof(IdempotencyRecord.Namespace), nameof(IdempotencyRecord.Scope), nameof(IdempotencyRecord.Key) }
            .Select(name => entity.FindProperty(name)!.GetCollation());
    }
}
