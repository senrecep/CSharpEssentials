using CSharpEssentials.EntityFrameworkCore.Pagination;
using CSharpEssentials.EntityFrameworkCore.Pagination.Requests;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

public class PaginationExtensionsTests
{
    private sealed class PaginatedEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    private sealed class PaginationDbContext : DbContext
    {
        public DbSet<PaginatedEntity> PaginatedEntities { get; set; } = null!;
        public PaginationDbContext(DbContextOptions<PaginationDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PaginatedEntity>().HasKey(x => x.Id);
        }
    }

    private static DbContextOptions<PaginationDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<PaginationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    private static async Task SeedAsync(PaginationDbContext context)
    {
        for (int i = 1; i <= 10; i++)
        {
            context.PaginatedEntities.Add(new PaginatedEntity
            {
                Id = i,
                Name = $"Item{i:00}",
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        await context.SaveChangesAsync();
    }

    #region PaginateAsync<T>

    [Fact]
    public async Task PaginateAsync_ShouldReturnCorrectPage()
    {
        using var context = new PaginationDbContext(CreateOptions());
        await SeedAsync(context);

        var request = new PaginationRequest { PageNumber = 2, PageSize = 3 };
        var result = await context.PaginatedEntities.PaginateAsync(request, null, includeTotalCount: true);

        result.Items.Should().HaveCount(3);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(3);
        result.TotalCount.Should().Be(10);
    }

    [Fact]
    public async Task PaginateAsync_ShouldApplySearch()
    {
        using var context = new PaginationDbContext(CreateOptions());
        await SeedAsync(context);

        var request = new PaginationRequest { PageNumber = 1, PageSize = 10, Search = "Item05" };
        var result = await context.PaginatedEntities.PaginateAsync(
            request,
            term => e => e.Name.Contains(term),
            includeTotalCount: true);

        result.Items.Should().ContainSingle();
        result.Items[0].Name.Should().Be("Item05");
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task PaginateAsync_ShouldNormalizeRequest()
    {
        using var context = new PaginationDbContext(CreateOptions());
        await SeedAsync(context);

        var request = new PaginationRequest { PageNumber = 0, PageSize = -1, Search = "  Item01  " };
        var result = await context.PaginatedEntities.PaginateAsync(
            request,
            term => e => e.Name.Contains(term),
            includeTotalCount: true);

        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(1);
        result.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task PaginateAsync_WithoutTotalCount_ShouldReturnNegativeOne()
    {
        using var context = new PaginationDbContext(CreateOptions());
        await SeedAsync(context);

        var request = new PaginationRequest { PageNumber = 1, PageSize = 5 };
        var result = await context.PaginatedEntities.PaginateAsync(request, null, includeTotalCount: false);

        result.TotalCount.Should().Be(-1);
    }

    #endregion

    #region PaginateAsync<T, TCursor>

    [Fact]
    public async Task PaginateAsync_Cursor_Ascending_ShouldReturnNextPage()
    {
        using var context = new PaginationDbContext(CreateOptions());
        await SeedAsync(context);

        var request = new CursorPaginationRequest<int> { Cursor = 3, Limit = 3 };
        var result = await context.PaginatedEntities
            .PaginateAsync<PaginatedEntity, int>(request, e => e.Id, isAscending: true);

        result.Items.Should().HaveCount(3);
        result.Items.Select(e => e.Id).Should().Equal(4, 5, 6);
        result.HasMore.Should().BeTrue();
        result.Next.Should().Be(6);
    }

    [Fact]
    public async Task PaginateAsync_Cursor_Descending_ShouldReturnPreviousPage()
    {
        using var context = new PaginationDbContext(CreateOptions());
        await SeedAsync(context);

        var request = new CursorPaginationRequest<int> { Cursor = 7, Limit = 3 };
        var result = await context.PaginatedEntities
            .PaginateAsync<PaginatedEntity, int>(request, e => e.Id, isAscending: false);

        result.Items.Should().HaveCount(3);
        result.Items.Select(e => e.Id).Should().Equal(6, 5, 4);
        result.HasMore.Should().BeTrue();
        result.Next.Should().Be(4);
    }

    [Fact]
    public async Task PaginateAsync_Cursor_NoMoreItems_ShouldReturnHasMoreFalse()
    {
        using var context = new PaginationDbContext(CreateOptions());
        await SeedAsync(context);

        var request = new CursorPaginationRequest<int> { Cursor = 8, Limit = 5 };
        var result = await context.PaginatedEntities
            .PaginateAsync<PaginatedEntity, int>(request, e => e.Id, isAscending: true);

        result.Items.Should().HaveCount(2);
        result.HasMore.Should().BeFalse();
        result.Next.Should().Be(default(int));
    }

    [Fact]
    public async Task PaginateAsync_Cursor_WithSearch_ShouldFilter()
    {
        using var context = new PaginationDbContext(CreateOptions());
        await SeedAsync(context);

        var request = new CursorPaginationRequest<int> { Cursor = 0, Limit = 10, Search = "Item09" };
        var result = await context.PaginatedEntities
            .PaginateAsync<PaginatedEntity, int>(
                request,
                e => e.Id,
                isAscending: true,
                search: term => e => e.Name.Contains(term));

        result.Items.Should().ContainSingle();
        result.Items[0].Id.Should().Be(9);
        result.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task PaginateAsync_Cursor_WithThenBy_ShouldApplySecondarySort()
    {
        using var context = new PaginationDbContext(CreateOptions());
        for (int i = 1; i <= 5; i++)
        {
            context.PaginatedEntities.Add(new PaginatedEntity
            {
                Id = i,
                Name = "Same",
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        await context.SaveChangesAsync();

        var request = new CursorPaginationRequest<int> { Cursor = 0, Limit = 3 };
        var result = await context.PaginatedEntities
            .PaginateAsync<PaginatedEntity, int>(
                request,
                e => e.Id,
                isAscending: true,
                thenBy: ordered => ordered.ThenByDescending(e => e.CreatedAt));

        result.Items.Should().HaveCount(3);
        result.HasMore.Should().BeTrue();
    }

    #endregion
}
