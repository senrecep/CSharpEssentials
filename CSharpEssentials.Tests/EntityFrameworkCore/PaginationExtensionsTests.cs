using CSharpEssentials.EntityFrameworkCore.Pagination;
using CSharpEssentials.EntityFrameworkCore.Pagination.Requests;
using CSharpEssentials.EntityFrameworkCore.Pagination.Responses;
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
        PaginationResponse<PaginatedEntity> result = await context.PaginatedEntities.PaginateAsync(request, null, includeTotalCount: true);

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
        PaginationResponse<PaginatedEntity> result = await context.PaginatedEntities.PaginateAsync(
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
        PaginationResponse<PaginatedEntity> result = await context.PaginatedEntities.PaginateAsync(
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
        PaginationResponse<PaginatedEntity> result = await context.PaginatedEntities.PaginateAsync(request, null, includeTotalCount: false);

        result.TotalCount.Should().Be(-1);
    }

    #endregion
}
