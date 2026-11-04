using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.Tests.EntityFrameworkCore.EnumColumns;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CSharpEssentials.Tests.EntityFrameworkCore.OrderedGuids;

/// <summary><c>UseOrderedGuidKeys</c> on SQLite, PostgreSQL and the in-memory provider.</summary>
public sealed class OrderedGuidKeyConventionTests(PostgresEnumFixture postgres) : IClassFixture<PostgresEnumFixture>
{
    private static readonly Guid _defaultId = new("00000000-0000-0000-0000-000000000001");

    public sealed class Order
    {
        public Guid Id { get; set; }
        public List<Line> Lines { get; set; } = [];
    }

    public sealed class Line
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
    }

    public sealed class Pinned
    {
        public Guid Id { get; set; }
    }

    public sealed class Defaulted
    {
        public Guid Id { get; set; }
    }

    public sealed class Assigned
    {
        public Guid Id { get; set; }
    }

    private sealed class ShopContext(DbContextOptions options, bool ordered, OrderedGuidLayout? layout = null) : DbContext(options)
    {
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<Line> Lines => Set<Line>();
        public DbSet<Pinned> Pinned => Set<Pinned>();
        public DbSet<Defaulted> Defaulted => Set<Defaulted>();
        public DbSet<Assigned> Assigned => Set<Assigned>();

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            if (ordered)
                configurationBuilder.UseOrderedGuidKeys(layout);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Pinned>().Property(x => x.Id).HasValueGeneratorFactory<SqlServerGuidValueGeneratorFactory>();
            modelBuilder.Entity<Assigned>().Property(x => x.Id).ValueGeneratedNever();
            if (Database.IsRelational())
                modelBuilder.Entity<Defaulted>().Property(x => x.Id).HasDefaultValueSql($"'{_defaultId}'");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.ReplaceService<Microsoft.EntityFrameworkCore.Infrastructure.IModelCacheKeyFactory, PerInstanceModelCacheKeyFactory>();
    }

    private sealed class PerInstanceModelCacheKeyFactory : Microsoft.EntityFrameworkCore.Infrastructure.IModelCacheKeyFactory
    {
        public object Create(DbContext context, bool designTime) => (context.GetType(), Guid.NewGuid(), designTime);
    }

    private static async Task<(ShopContext Context, SqliteConnection Connection)> SqliteAsync(bool ordered, OrderedGuidLayout? layout = null)
    {
        SqliteConnection connection = new("DataSource=:memory:");
        await connection.OpenAsync();
        ShopContext context = new(new DbContextOptionsBuilder<ShopContext>().UseSqlite(connection).Options, ordered, layout);
        await context.Database.EnsureCreatedAsync();
        return (context, connection);
    }

    private static async Task<List<Guid>> InsertOneByOneAsync(ShopContext context, int count)
    {
        List<Guid> ids = [];
        for (int i = 0; i < count; i++)
        {
            Order order = new() { Lines = [new Line()] };
            context.Orders.Add(order);
            await context.SaveChangesAsync();
            ids.Add(order.Id);
        }
        context.ChangeTracker.Clear();
        return ids;
    }

    [Fact]
    public async Task Sqlite_ShouldStoreVersion7AsUppercaseText_InInsertOrder()
    {
        (ShopContext context, SqliteConnection connection) = await SqliteAsync(ordered: true);
        await using (connection)
        await using (context)
        {
            List<Guid> ids = await InsertOneByOneAsync(context, 300);

            ids.Should().OnlyContain(id => id.Version == 7);
            List<string> stored = await context.Database.SqlQueryRaw<string>("SELECT \"Id\" AS \"Value\" FROM \"Orders\" ORDER BY \"Id\"").ToListAsync();
            stored.Should().Equal(ids.Select(id => id.ToString().ToUpperInvariant()));
            (await context.Database.SqlQueryRaw<string>("SELECT DISTINCT typeof(\"Id\") AS \"Value\" FROM \"Orders\"").ToListAsync())
                .Should().Equal("text");
            (await context.Orders.OrderBy(x => x.Id).Select(x => x.Id).ToListAsync()).Should().Equal(ids);
        }
    }

    [Fact]
    public async Task Sqlite_ShouldKeepUserConfiguration_AndSkipForeignKeys()
    {
        (ShopContext context, SqliteConnection connection) = await SqliteAsync(ordered: true);
        await using (connection)
        await using (context)
        {
            Pinned pinned = new();
            Defaulted defaulted = new();
            Assigned assigned = new() { Id = _defaultId };
            Order order = new() { Lines = [new Line()] };
            context.AddRange(pinned, defaulted, assigned, order);
            await context.SaveChangesAsync();

            pinned.Id.Version.Should().Be(8);
            defaulted.Id.Should().Be(_defaultId);
            assigned.Id.Should().Be(_defaultId);
            order.Lines[0].Id.Version.Should().Be(7);
            order.Lines[0].OrderId.Should().Be(order.Id);
        }
    }

    [Fact]
    public async Task Sqlite_ShouldLeaveKeyGenerationAlone_WithoutOptIn()
    {
        (ShopContext context, SqliteConnection connection) = await SqliteAsync(ordered: false);
        await using (connection)
        await using (context)
        {
            List<Guid> ids = await InsertOneByOneAsync(context, 20);

            ids.Should().OnlyContain(id => id.Version == 4);
        }
    }

    [Fact]
    public async Task Sqlite_ShouldUseExplicitLayout()
    {
        (ShopContext context, SqliteConnection connection) = await SqliteAsync(ordered: true, OrderedGuidLayout.SqlServer);
        await using (connection)
        await using (context)
        {
            List<Guid> ids = await InsertOneByOneAsync(context, 5);

            ids.Should().OnlyContain(id => id.Version == 8);
        }
    }

    [Theory]
    [InlineData(null, 4)]
    [InlineData(OrderedGuidLayout.Version7, 7)]
    public async Task InMemory_ShouldChangeKeysOnlyWithExplicitLayout(OrderedGuidLayout? layout, int version)
    {
        await using ShopContext context = new(
            new DbContextOptionsBuilder<ShopContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options, ordered: true, layout);
        Order order = new();
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        order.Id.Version.Should().Be(version);
    }

    [Fact]
    public async Task Postgres_ShouldStoreVersion7Uuid_InInsertOrder()
    {
        await using ShopContext context = new(
            new DbContextOptionsBuilder<ShopContext>().UseNpgsql(postgres.ConnectionString("ordered_guid_keys")).Options, ordered: true);
        await context.Database.EnsureCreatedAsync();

        List<Guid> ids = await InsertOneByOneAsync(context, 300);

        ids.Should().OnlyContain(id => id.Version == 7);
        (await context.Database.SqlQueryRaw<Guid>("SELECT \"Id\" AS \"Value\" FROM \"Orders\" ORDER BY \"Id\"").ToListAsync())
            .Should().Equal(ids);
    }

    [Fact]
    public async Task Postgres_ShouldAlreadyGenerateVersion7_WithoutOptIn()
    {
        await using ShopContext context = new(
            new DbContextOptionsBuilder<ShopContext>().UseNpgsql(postgres.ConnectionString("npgsql_guid_keys")).Options, ordered: false);
        await context.Database.EnsureCreatedAsync();

        List<Guid> ids = await InsertOneByOneAsync(context, 5);

        ids.Should().OnlyContain(id => id.Version == 7);
    }
}
