using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.Maybe;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

/// <summary><c>Maybe&lt;T&gt;?</c> columns against a real SQLite database, configured per property and by the convention.</summary>
public sealed class MaybeColumnSqliteTests
{
    public const string PerProperty = "per-property";
    public const string Convention = "convention";

    private static readonly Guid SampleGuid = Guid.Parse("8f1c7f4e-52a4-4c55-9a59-0c1e8d8b7a11");
    private static readonly DateTime SampleDate = new(2026, 3, 14, 9, 30, 0, DateTimeKind.Utc);

    private sealed class MaybeRow
    {
        public int Id { get; set; }
        public Maybe<int>? Count { get; set; }
        public Maybe<string>? Name { get; set; }
        public Maybe<Guid>? ExternalId { get; set; }
        public Maybe<DateTime>? DueAt { get; set; }
        public Maybe<string>? Ignored { get; set; }
    }

    private abstract class MaybeContext(SqliteConnection connection) : DbContext
    {
        public DbSet<MaybeRow> Rows => Set<MaybeRow>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite(connection);
    }

    private sealed class PerPropertyContext(SqliteConnection connection) : MaybeContext(connection)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder.Entity<MaybeRow>(row =>
            {
                row.ToTable("rows");
                row.Property(x => x.Count).HasNullableMaybeConversion();
                row.Property(x => x.Name).HasNullableMaybeConversion();
                row.Property(x => x.ExternalId).HasNullableMaybeConversion();
                row.Property(x => x.DueAt).HasNullableMaybeConversion();
                row.Ignore(x => x.Ignored);
            });
    }

    private sealed class ConventionContext(SqliteConnection connection) : MaybeContext(connection)
    {
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
            configurationBuilder.ConfigureNullableMaybeConventions();

        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder.Entity<MaybeRow>(row =>
            {
                row.ToTable("rows");
                row.Ignore(x => x.Ignored);
            });
    }

    public static TheoryData<string, bool> AbsentRows => new()
    {
        { PerProperty, false },
        { PerProperty, true },
        { Convention, false },
        { Convention, true },
    };

    [Theory]
    [MemberData(nameof(AbsentRows))]
    public async Task SaveChanges_Should_StoreNull_When_ValueIsNullOrNone(string path, bool none)
    {
        await using SqliteConnection connection = await OpenAsync();
        await using MaybeContext context = await CreateDatabaseAsync(connection, path);
        context.Rows.Add(AbsentRow(1, none));
        await context.SaveChangesAsync();

        string?[] values = await context.Database
            .SqlQueryRaw<string?>("SELECT COALESCE(\"Count\", \"Name\", \"ExternalId\", \"DueAt\") AS \"Value\" FROM rows")
            .ToArrayAsync();

        values.Should().ContainSingle().Which.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(AbsentRows))]
    public async Task Query_Should_ReturnNull_When_NullOrNoneWasSaved(string path, bool none)
    {
        await using SqliteConnection connection = await OpenAsync();
        await using (MaybeContext writer = await CreateDatabaseAsync(connection, path))
        {
            writer.Rows.Add(AbsentRow(1, none));
            await writer.SaveChangesAsync();
        }

        await using MaybeContext reader = NewContext(connection, path);
        MaybeRow row = await reader.Rows.SingleAsync();

        row.Count.Should().BeNull();
        row.Name.Should().BeNull();
        row.ExternalId.Should().BeNull();
        row.DueAt.Should().BeNull();
        row.Count.Flatten().HasNoValue.Should().BeTrue();
        row.Name.Flatten().HasNoValue.Should().BeTrue();
    }

    [Theory]
    [InlineData(PerProperty, 0, "")]
    [InlineData(PerProperty, 5, "hello")]
    [InlineData(Convention, 0, "")]
    [InlineData(Convention, 5, "hello")]
    public async Task Query_Should_ReturnSome_When_SomeWasSaved(string path, int count, string name)
    {
        await using SqliteConnection connection = await OpenAsync();
        await using (MaybeContext writer = await CreateDatabaseAsync(connection, path))
        {
            writer.Rows.Add(new MaybeRow { Id = 1, Count = Maybe<int>.From(count), Name = Maybe<string>.From(name), ExternalId = Maybe<Guid>.From(SampleGuid), DueAt = Maybe<DateTime>.From(SampleDate) });
            await writer.SaveChangesAsync();
        }

        await using MaybeContext reader = NewContext(connection, path);
        MaybeRow row = await reader.Rows.SingleAsync();

        row.Count.Should().Be(Maybe<int>.From(count));
        row.Name.Should().Be(Maybe<string>.From(name));
        row.ExternalId.Should().Be(Maybe<Guid>.From(SampleGuid));
        row.DueAt.Should().Be(Maybe<DateTime>.From(SampleDate));
    }

    [Theory]
    [InlineData(PerProperty)]
    [InlineData(Convention)]
    public async Task Model_Should_MapMaybeColumnsAsNullable(string path)
    {
        await using SqliteConnection connection = await OpenAsync();
        await using MaybeContext context = NewContext(connection, path);

        IEntityType entityType = context.Model.FindEntityType(typeof(MaybeRow))!;
        string[] names = [nameof(MaybeRow.Count), nameof(MaybeRow.Name), nameof(MaybeRow.ExternalId), nameof(MaybeRow.DueAt)];

        names.Select(name => entityType.FindProperty(name)!)
            .Should().OnlyContain(property => property.IsNullable && property.IsColumnNullable());
        entityType.FindProperty(nameof(MaybeRow.Count))!.GetValueConverter()!.ProviderClrType.Should().Be<int?>();
        entityType.FindProperty(nameof(MaybeRow.Name))!.GetValueConverter()!.ProviderClrType.Should().Be<string>();
        entityType.FindProperty(nameof(MaybeRow.Ignored)).Should().BeNull();
    }

    [Theory]
    [InlineData(PerProperty)]
    [InlineData(Convention)]
    public async Task CreateScript_Should_DeclareNullableColumns(string path)
    {
        await using SqliteConnection connection = await OpenAsync();
        await using MaybeContext context = NewContext(connection, path);

        string script = context.Database.GenerateCreateScript();

        script.Should().Contain("\"Count\" INTEGER NULL")
            .And.Contain("\"Name\" TEXT NULL")
            .And.Contain("\"ExternalId\" TEXT NULL")
            .And.Contain("\"DueAt\" TEXT NULL");
    }

    [Theory]
    [InlineData(PerProperty)]
    [InlineData(Convention)]
    public async Task Migration_Should_CreateNullableColumns(string path)
    {
        await using SqliteConnection connection = await OpenAsync();
        await using MaybeContext context = NewContext(connection, path);

        IReadOnlyList<MigrationOperation> operations = context.GetService<IMigrationsModelDiffer>()
            .GetDifferences(null, context.GetService<IDesignTimeModel>().Model.GetRelationalModel());

        CreateTableOperation table = operations.OfType<CreateTableOperation>().Single();
        table.Columns.Where(column => column.Name != nameof(MaybeRow.Id))
            .Should().HaveCount(4)
            .And.OnlyContain(column => column.IsNullable);
    }

    [Theory]
    [InlineData(PerProperty)]
    [InlineData(Convention)]
    public async Task Where_Should_MatchAbsentRows_When_ComparedWithNull(string path)
    {
        await using SqliteConnection connection = await OpenAsync();
        await using MaybeContext context = await SeedAsync(connection, path);

        int[] count = await context.Rows.Where(row => row.Count == null).Select(row => row.Id).OrderBy(id => id).ToArrayAsync();
        int[] name = await context.Rows.Where(row => row.Name == null).Select(row => row.Id).OrderBy(id => id).ToArrayAsync();

        count.Should().Equal(3, 4);
        name.Should().Equal(3, 4);
    }

    [Theory]
    [InlineData(PerProperty)]
    [InlineData(Convention)]
    public async Task Where_Should_MatchPresentRows_When_ComparedNotEqualToNull(string path)
    {
        await using SqliteConnection connection = await OpenAsync();
        await using MaybeContext context = await SeedAsync(connection, path);

        int[] count = await context.Rows.Where(row => row.Count != null).Select(row => row.Id).OrderBy(id => id).ToArrayAsync();
        int[] hasValue = await context.Rows.Where(row => row.Name.HasValue).Select(row => row.Id).OrderBy(id => id).ToArrayAsync();

        count.Should().Equal(1, 2);
        hasValue.Should().Equal(1, 2);
    }

    [Theory]
    [InlineData(PerProperty)]
    [InlineData(Convention)]
    public async Task Where_Should_MatchValue_When_ComparedWithMaybe(string path)
    {
        await using SqliteConnection connection = await OpenAsync();
        await using MaybeContext context = await SeedAsync(connection, path);
        Maybe<int> five = 5;
        Maybe<string> hello = "hello";

        int[] count = await context.Rows.Where(row => row.Count == five).Select(row => row.Id).ToArrayAsync();
        int[] name = await context.Rows.Where(row => row.Name == hello).Select(row => row.Id).ToArrayAsync();

        count.Should().Equal(2);
        name.Should().Equal(2);
    }

    [Theory]
    [InlineData(PerProperty)]
    [InlineData(Convention)]
    public async Task Where_Should_NotMatchAbsentRows_When_ComparedWithNone(string path)
    {
        await using SqliteConnection connection = await OpenAsync();
        await using MaybeContext context = await SeedAsync(connection, path);
        Maybe<int> none = Maybe<int>.None;

        int[] count = await context.Rows.Where(row => row.Count == none).Select(row => row.Id).ToArrayAsync();

        count.Should().BeEmpty("None is sent as a non-null parameter, so the SQL is \"Count\" = NULL");
    }

    private static MaybeRow AbsentRow(int id, bool none) => none
        ? new MaybeRow { Id = id, Count = Maybe<int>.None, Name = Maybe<string>.None, ExternalId = Maybe<Guid>.None, DueAt = Maybe<DateTime>.None }
        : new MaybeRow { Id = id };

    private static async Task<MaybeContext> SeedAsync(SqliteConnection connection, string path)
    {
        MaybeContext context = await CreateDatabaseAsync(connection, path);
        context.Rows.AddRange(
            new MaybeRow { Id = 1, Count = Maybe<int>.From(0), Name = Maybe<string>.From("") },
            new MaybeRow { Id = 2, Count = Maybe<int>.From(5), Name = Maybe<string>.From("hello") },
            AbsentRow(3, none: false),
            AbsentRow(4, none: true));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        return context;
    }

    private static MaybeContext NewContext(SqliteConnection connection, string path) => path == Convention
        ? new ConventionContext(connection)
        : new PerPropertyContext(connection);

    private static async Task<MaybeContext> CreateDatabaseAsync(SqliteConnection connection, string path)
    {
        MaybeContext context = NewContext(connection, path);
        await context.Database.EnsureCreatedAsync();
        return context;
    }

    private static async Task<SqliteConnection> OpenAsync()
    {
        SqliteConnection connection = new("DataSource=:memory:");
        await connection.OpenAsync();
        return connection;
    }
}
