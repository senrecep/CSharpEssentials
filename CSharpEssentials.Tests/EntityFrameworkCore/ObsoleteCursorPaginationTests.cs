using System.Data.Common;
using CSharpEssentials.EntityFrameworkCore.Pagination;
using CSharpEssentials.EntityFrameworkCore.Pagination.Requests;
using CSharpEssentials.EntityFrameworkCore.Pagination.Responses;
using CSharpEssentials.Tests.EntityFrameworkCore.Keyset;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

[Obsolete("Covers the obsolete single-column cursor PaginateAsync<T, TCursor> on purpose; new code uses KeysetPaginateAsync.")]
public sealed class ObsoleteCursorPaginationTests
{
    private sealed class PaginatedEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    private sealed class PaginationDbContext(DbContextOptions<PaginationDbContext> options) : DbContext(options)
    {
        public DbSet<PaginatedEntity> PaginatedEntities => Set<PaginatedEntity>();
    }

    private static PaginationDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<PaginationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static async Task<PaginationDbContext> CreateSeededContextAsync(int count = 10, string? name = null)
    {
        PaginationDbContext context = CreateContext();
        context.PaginatedEntities.AddRange(Enumerable.Range(1, count).Select(i => new PaginatedEntity
        {
            Id = i,
            Name = name ?? $"Item{i:00}",
            CreatedAt = DateTime.UtcNow.AddMinutes(-i)
        }));
        await context.SaveChangesAsync();
        return context;
    }

    [Fact]
    public async Task PaginateAsync_Cursor_Ascending_ShouldReturnNextPage()
    {
        using PaginationDbContext context = await CreateSeededContextAsync();

        var request = new CursorPaginationRequest<int> { Cursor = 3, Limit = 3 };
        CursorPaginationResponse<PaginatedEntity, int> result = await context.PaginatedEntities
            .PaginateAsync<PaginatedEntity, int>(request, e => e.Id, isAscending: true);

        result.Items.Select(e => e.Id).Should().Equal(4, 5, 6);
        result.HasMore.Should().BeTrue();
        result.Next.Should().Be(6);
    }

    [Fact]
    public async Task PaginateAsync_Cursor_Descending_ShouldReturnPreviousPage()
    {
        using PaginationDbContext context = await CreateSeededContextAsync();

        var request = new CursorPaginationRequest<int> { Cursor = 7, Limit = 3 };
        CursorPaginationResponse<PaginatedEntity, int> result = await context.PaginatedEntities
            .PaginateAsync<PaginatedEntity, int>(request, e => e.Id, isAscending: false);

        result.Items.Select(e => e.Id).Should().Equal(6, 5, 4);
        result.HasMore.Should().BeTrue();
        result.Next.Should().Be(4);
    }

    [Fact]
    public async Task PaginateAsync_Cursor_NoMoreItems_ShouldReturnHasMoreFalse()
    {
        using PaginationDbContext context = await CreateSeededContextAsync();

        var request = new CursorPaginationRequest<int> { Cursor = 8, Limit = 5 };
        CursorPaginationResponse<PaginatedEntity, int> result = await context.PaginatedEntities
            .PaginateAsync<PaginatedEntity, int>(request, e => e.Id, isAscending: true);

        result.Items.Should().HaveCount(2);
        result.HasMore.Should().BeFalse();
        result.Next.Should().Be(default);
    }

    [Fact]
    public async Task PaginateAsync_Cursor_WithSearch_ShouldFilter()
    {
        using PaginationDbContext context = await CreateSeededContextAsync();

        var request = new CursorPaginationRequest<int> { Cursor = 0, Limit = 10, Search = "Item09" };
        CursorPaginationResponse<PaginatedEntity, int> result = await context.PaginatedEntities
            .PaginateAsync<PaginatedEntity, int>(
                request,
                e => e.Id,
                isAscending: true,
                search: term => e => e.Name.Contains(term));

        result.Items.Should().ContainSingle().Which.Id.Should().Be(9);
        result.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task PaginateAsync_Cursor_WithThenBy_ShouldApplySecondarySort()
    {
        using PaginationDbContext context = await CreateSeededContextAsync(count: 5, name: "Same");

        var request = new CursorPaginationRequest<int> { Cursor = 0, Limit = 3 };
        CursorPaginationResponse<PaginatedEntity, int> result = await context.PaginatedEntities
            .PaginateAsync<PaginatedEntity, int>(
                request,
                e => e.Id,
                isAscending: true,
                thenBy: ordered => ordered.ThenByDescending(e => e.CreatedAt));

        result.Items.Should().HaveCount(3);
        result.HasMore.Should().BeTrue();
    }

    [Fact]
    public async Task PaginateAsync_Cursor_Should_ReturnAllItems_When_LimitIsIntMaxValue()
    {
        using PaginationDbContext context = await CreateSeededContextAsync();

        CursorPaginationResponse<PaginatedEntity, int> result = await context.PaginatedEntities.PaginateAsync(
            new CursorPaginationRequest<int> { Limit = int.MaxValue }, e => e.Id);

        result.Items.Should().HaveCount(10);
    }

    [Fact]
    public async Task PaginateAsync_Cursor_Should_SendCursorAsParameter_When_CursorIsSet()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var interceptor = new CommandCapturingInterceptor();
        DbContextOptions<KeysetDbContext> options = new DbContextOptionsBuilder<KeysetDbContext>()
            .UseSqlite(connection)
            .AddInterceptors(interceptor)
            .Options;
        using (var context = new KeysetDbContext(options))
            await context.Database.EnsureCreatedAsync();

        using (var context = new KeysetDbContext(options))
            await context.Rows.PaginateAsync(new CursorPaginationRequest<int> { Cursor = 40, Limit = 3 }, x => x.Id);
        using (var context = new KeysetDbContext(options))
            await context.Rows.PaginateAsync(new CursorPaginationRequest<int> { Cursor = 60, Limit = 3 }, x => x.Id);

        interceptor.Commands.Should().HaveCount(2);
        interceptor.Commands[0].Should().Contain("@").And.NotContain("40").And.NotContain("60");
        interceptor.Commands[1].Should().Be(interceptor.Commands[0]);
    }

    private sealed class CommandCapturingInterceptor : DbCommandInterceptor
    {
        public List<string> Commands { get; } = [];

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            Commands.Add(command.CommandText);
            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }
    }
}
