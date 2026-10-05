using CSharpEssentials.Entity;
using CSharpEssentials.Entity.Interfaces;
using CSharpEssentials.EntityFrameworkCore;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

public sealed class SoftDeleteBatchTests : IDisposable
{
    private static readonly DateTimeOffset CreatedAt = new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private sealed class Document : SoftDeletableEntityBase, ISoftDeletableEntityBase<Guid>
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
    }

    private sealed class DocumentDbContext(DbContextOptions<DocumentDbContext> options) : DbContext(options)
    {
        public DbSet<Document> Documents => Set<Document>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Document>().SoftDeletableEntityBaseMap<Document, Guid>();
            modelBuilder.ApplySoftDeleteQueryFilter();
        }
    }

    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<DocumentDbContext> _options;

    public SoftDeleteBatchTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<DocumentDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new DocumentDbContext(_options);
        context.Database.EnsureCreated();
    }

    public void Dispose() => _connection.Dispose();

    private async Task SeedAsync(params string[] names)
    {
        using var context = new DocumentDbContext(_options);
        foreach (string name in names)
        {
            var document = new Document { Name = name };
            document.SetCreatedInfo(CreatedAt, "seed");
            context.Documents.Add(document);
        }
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task SoftDeleteAsync_ShouldMarkMatchedRowsAndReturnCount()
    {
        await SeedAsync("a1", "a2", "b1");
        DateTimeOffset deletedAt = new(2025, 5, 5, 10, 0, 0, TimeSpan.Zero);

        // The Sqlite provider cannot translate the StartsWith(char) overload CA1866 suggests; a string variable keeps it in SQL.
        string prefix = "a";
        int affected;
        using (var context = new DocumentDbContext(_options))
        {
            affected = await context.Documents
                .Where(d => d.Name.StartsWith(prefix))
                .SoftDeleteAsync(deletedAt, "admin");
        }

        affected.Should().Be(2);
        using var verify = new DocumentDbContext(_options);
        List<Document> all = await verify.Documents.IgnoreQueryFilters().OrderBy(d => d.Name).ToListAsync();
        all.Should().HaveCount(3);
        all.Where(d => d.Name.StartsWith('a')).Should().AllSatisfy(d =>
        {
            d.IsDeleted.Should().BeTrue();
            d.DeletedAt.Should().Be(deletedAt);
            d.DeletedBy.Should().Be("admin");
        });
        Document untouched = all.Single(d => d.Name == "b1");
        untouched.IsDeleted.Should().BeFalse();
        untouched.DeletedAt.Should().BeNull();
        untouched.DeletedBy.Should().BeNull();

        (await verify.Documents.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task SoftDeleteAsync_ShouldNotOverwriteAlreadyDeletedRows()
    {
        await SeedAsync("a1", "a2");
        DateTimeOffset firstDeletion = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        using (var context = new DocumentDbContext(_options))
            await context.Documents.Where(d => d.Name == "a1").SoftDeleteAsync(firstDeletion, "first");

        int affected;
        using (var context = new DocumentDbContext(_options))
        {
            affected = await context.Documents
                .IgnoreQueryFilters()
                .SoftDeleteAsync(firstDeletion.AddDays(1), "second");
        }

        affected.Should().Be(1);
        using var verify = new DocumentDbContext(_options);
        Document first = await verify.Documents.IgnoreQueryFilters().SingleAsync(d => d.Name == "a1");
        first.DeletedBy.Should().Be("first");
        first.DeletedAt.Should().Be(firstDeletion);
        Document second = await verify.Documents.IgnoreQueryFilters().SingleAsync(d => d.Name == "a2");
        second.DeletedBy.Should().Be("second");
    }

    [Fact]
    public async Task SoftDeleteAsync_ShouldReturnZero_WhenNothingMatches()
    {
        await SeedAsync("a1");

        using var context = new DocumentDbContext(_options);
        int affected = await context.Documents
            .Where(d => d.Name == "missing")
            .SoftDeleteAsync(CreatedAt, "admin");

        affected.Should().Be(0);
    }

    [Fact]
    public async Task SoftDeleteAsync_WithTimeProvider_ShouldUseProviderTime()
    {
        await SeedAsync("a1");
        FakeTimeProvider timeProvider = new(new DateTimeOffset(2026, 2, 3, 4, 5, 6, TimeSpan.Zero));

        using (var context = new DocumentDbContext(_options))
            await context.Documents.SoftDeleteAsync("system", timeProvider);

        using var verify = new DocumentDbContext(_options);
        Document document = await verify.Documents.IgnoreQueryFilters().SingleAsync();
        document.IsDeleted.Should().BeTrue();
        document.DeletedAt.Should().Be(timeProvider.GetUtcNow());
        document.DeletedBy.Should().Be("system");
    }

    [Fact]
    public async Task SoftDeleteAsync_WithoutTimeProvider_ShouldUseSystemClock()
    {
        await SeedAsync("a1");
        DateTimeOffset before = DateTimeOffset.UtcNow;

        using (var context = new DocumentDbContext(_options))
            await context.Documents.SoftDeleteAsync("system");

        using var verify = new DocumentDbContext(_options);
        Document document = await verify.Documents.IgnoreQueryFilters().SingleAsync();
        document.DeletedAt.Should().NotBeNull();
        document.DeletedAt.Value.Should().BeOnOrAfter(before);
    }

    [Fact]
    public async Task SoftDeleteAsync_ShouldThrow_WhenDeletedByIsNull()
    {
        using var context = new DocumentDbContext(_options);

        Func<Task> act = () => context.Documents.SoftDeleteAsync(CreatedAt, null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SoftDeleteAsync_ShouldThrow_WhenSourceIsNull()
    {
        IQueryable<Document> source = null!;

        Func<Task> act = () => source.SoftDeleteAsync(CreatedAt, "admin");

        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
