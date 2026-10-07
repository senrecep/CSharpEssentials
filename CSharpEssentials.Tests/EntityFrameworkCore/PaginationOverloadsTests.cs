using CSharpEssentials.EntityFrameworkCore.Pagination;
using CSharpEssentials.EntityFrameworkCore.Pagination.Requests;
using CSharpEssentials.EntityFrameworkCore.Pagination.Responses;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

public class PaginationOverloadsTests
{
    private sealed class Item
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private sealed class ItemDbContext(DbContextOptions<ItemDbContext> options) : DbContext(options)
    {
        public DbSet<Item> Items => Set<Item>();
    }

    private static async Task<ItemDbContext> CreateSeededContextAsync(int count = 10)
    {
        DbContextOptions<ItemDbContext> options = new DbContextOptionsBuilder<ItemDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ItemDbContext(options);
        context.Items.AddRange(Enumerable.Range(1, count).Select(i => new Item { Id = i, Name = $"Item{i:00}" }));
        await context.SaveChangesAsync();
        return context;
    }

    private static IQueryable<Item> InMemoryItems(int count = 10) =>
        Enumerable.Range(1, count).Select(i => new Item { Id = i, Name = $"Item{i:00}" }).AsQueryable();

    [Fact]
    public async Task PaginateAsync_WithPageNumberAndSize_ShouldReturnRequestedPage()
    {
        using ItemDbContext context = await CreateSeededContextAsync();

        PaginationResponse<Item> result = await context.Items.OrderBy(i => i.Id).PaginateAsync(2, 4);

        result.Items.Select(i => i.Id).Should().Equal(5, 6, 7, 8);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(4);
        result.TotalCount.Should().Be(10);
        result.TotalPages.Should().Be(3);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task PaginateAsync_WithPageNumberAndSize_ShouldNormalizeInvalidValues()
    {
        using ItemDbContext context = await CreateSeededContextAsync();

        PaginationResponse<Item> result = await context.Items.OrderBy(i => i.Id).PaginateAsync(0, -5);

        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(1);
        result.Items.Should().ContainSingle().Which.Id.Should().Be(1);
    }

    [Fact]
    public async Task PaginateAsync_WithPageNumberAndSize_ShouldSkipCount_WhenTotalCountDisabled()
    {
        using ItemDbContext context = await CreateSeededContextAsync();

        PaginationResponse<Item> result = await context.Items.OrderBy(i => i.Id)
            .PaginateAsync(1, 3, includeTotalCount: false);

        result.TotalCount.Should().Be(-1);
        result.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task PaginateAsync_Should_ReturnEmptyPage_When_PageNumberIsIntMaxValue()
    {
        using ItemDbContext context = await CreateSeededContextAsync();

        PaginationResponse<Item> result = await context.Items.OrderBy(i => i.Id).PaginateAsync(int.MaxValue, 100);

        result.Items.Should().BeEmpty();
        result.PageNumber.Should().Be(int.MaxValue);
        result.TotalCount.Should().Be(10);
    }

    [Fact]
    public async Task PaginateAsync_WithRequest_Should_CapPageSizeAtDefaultMax_When_MaxPageSizeIsNotGiven()
    {
        using ItemDbContext context = await CreateSeededContextAsync(150);
        var request = new PaginationRequest { PageNumber = 1, PageSize = 500 };

        PaginationResponse<Item> result = await context.Items.OrderBy(i => i.Id).PaginateAsync(request);

        result.PageSize.Should().Be(PaginationDefaults.MaxPageSize);
        result.Items.Should().HaveCount(PaginationDefaults.MaxPageSize);
    }

    [Fact]
    public async Task PaginateAsync_WithRequest_Should_HonorMaxPageSize_When_MaxPageSizeIsGiven()
    {
        using ItemDbContext context = await CreateSeededContextAsync(150);
        var request = new PaginationRequest { PageNumber = 1, PageSize = 500 };

        PaginationResponse<Item> result = await context.Items.OrderBy(i => i.Id)
            .PaginateAsync(request, maxPageSize: 120, cancellationToken: CancellationToken.None);

        result.PageSize.Should().Be(120);
        result.Items.Should().HaveCount(120);
    }

    [Fact]
    public async Task PaginateAsync_WithRequest_Should_LowerPageSizeBelowDefault_When_MaxPageSizeIsSmaller()
    {
        using ItemDbContext context = await CreateSeededContextAsync();
        var request = new PaginationRequest { PageNumber = 2, PageSize = 50 };

        PaginationResponse<Item> result = await context.Items.OrderBy(i => i.Id).PaginateAsync(request, maxPageSize: 4);

        result.PageSize.Should().Be(4);
        result.Items.Select(i => i.Id).Should().Equal(5, 6, 7, 8);
    }

    [Fact]
    public async Task PaginateAsync_WithRequest_Should_CapAtDefaultMax_When_CalledWithPositionalCancellationToken()
    {
        using ItemDbContext context = await CreateSeededContextAsync(150);
        var request = new PaginationRequest { PageNumber = 1, PageSize = 500 };

        PaginationResponse<Item> result = await context.Items.OrderBy(i => i.Id)
            .PaginateAsync(request, null, true, CancellationToken.None);

        result.PageSize.Should().Be(PaginationDefaults.MaxPageSize);
        result.TotalCount.Should().Be(150);
    }

    [Fact]
    public async Task PaginateAsync_WithRequest_Should_CapAtDefaultMax_When_LastPositionalArgumentIsDefaultLiteral()
    {
        using ItemDbContext context = await CreateSeededContextAsync(150);
        var request = new PaginationRequest { PageNumber = 1, PageSize = 500 };

        PaginationResponse<Item> result = await context.Items.OrderBy(i => i.Id)
            .PaginateAsync(request, null, true, default);

        result.PageSize.Should().Be(PaginationDefaults.MaxPageSize);
        result.Items.Should().HaveCount(PaginationDefaults.MaxPageSize);
    }

    [Fact]
    public async Task PaginateAsync_WithRequest_Should_CapAtDefaultMax_When_AllArgumentsAreNamed()
    {
        using ItemDbContext context = await CreateSeededContextAsync(150);
        var request = new PaginationRequest { PageNumber = 1, PageSize = 500 };

        PaginationResponse<Item> result = await context.Items.OrderBy(i => i.Id)
            .PaginateAsync(
                paginationRequest: request,
                search: null,
                includeTotalCount: true,
                cancellationToken: CancellationToken.None);

        result.PageSize.Should().Be(PaginationDefaults.MaxPageSize);
        result.Items.Should().HaveCount(PaginationDefaults.MaxPageSize);
    }

    [Fact]
    public async Task PaginateAsync_WithPageNumberAndSize_Should_CapAtDefaultMax_When_LastPositionalArgumentIsDefaultLiteral()
    {
        using ItemDbContext context = await CreateSeededContextAsync(150);

        PaginationResponse<Item> result = await context.Items.OrderBy(i => i.Id)
            .PaginateAsync(1, 500, true, default);

        result.PageSize.Should().Be(PaginationDefaults.MaxPageSize);
        result.Items.Should().HaveCount(PaginationDefaults.MaxPageSize);
    }

    [Fact]
    public async Task PaginateAsync_WithPageNumberAndSize_Should_CapAtDefaultMax_When_AllArgumentsAreNamed()
    {
        using ItemDbContext context = await CreateSeededContextAsync(150);

        PaginationResponse<Item> result = await context.Items.OrderBy(i => i.Id)
            .PaginateAsync(
                pageNumber: 1,
                pageSize: 500,
                includeTotalCount: true,
                cancellationToken: CancellationToken.None);

        result.PageSize.Should().Be(PaginationDefaults.MaxPageSize);
        result.Items.Should().HaveCount(PaginationDefaults.MaxPageSize);
    }

    [Fact]
    public async Task PaginateAsync_WithPageNumberAndSize_Should_CapPageSizeAtDefaultMax_When_MaxPageSizeIsNotGiven()
    {
        using ItemDbContext context = await CreateSeededContextAsync(150);

        PaginationResponse<Item> result = await context.Items.OrderBy(i => i.Id).PaginateAsync(1, 500);

        result.PageSize.Should().Be(PaginationDefaults.MaxPageSize);
        result.Items.Should().HaveCount(PaginationDefaults.MaxPageSize);
    }

    [Fact]
    public async Task PaginateAsync_WithPageNumberAndSize_Should_HonorMaxPageSize_When_MaxPageSizeIsGiven()
    {
        using ItemDbContext context = await CreateSeededContextAsync(150);

        PaginationResponse<Item> result = await context.Items.OrderBy(i => i.Id).PaginateAsync(1, 500, maxPageSize: 150);

        result.PageSize.Should().Be(150);
        result.Items.Should().HaveCount(150);
    }

    [Fact]
    public async Task PaginateAsync_WithPageNumberAndSize_Should_CapAtDefaultMax_When_CalledWithPositionalCancellationToken()
    {
        using ItemDbContext context = await CreateSeededContextAsync(150);

        PaginationResponse<Item> result = await context.Items.OrderBy(i => i.Id)
            .PaginateAsync(1, 500, false, CancellationToken.None);

        result.PageSize.Should().Be(PaginationDefaults.MaxPageSize);
        result.TotalCount.Should().Be(-1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public async Task PaginateAsync_Should_Throw_When_MaxPageSizeIsLessThanOne(int maxPageSize)
    {
        using ItemDbContext context = await CreateSeededContextAsync();

        Func<Task> act = () => context.Items.OrderBy(i => i.Id).PaginateAsync(1, 10, maxPageSize: maxPageSize);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Paginate_WithRequest_Should_CapPageSizeAtDefaultMax_When_MaxPageSizeIsNotGiven()
    {
        var request = new PaginationRequest { PageNumber = 1, PageSize = 500 };

        PaginationResponse<Item> result = InMemoryItems(150).Paginate(request);

        result.PageSize.Should().Be(PaginationDefaults.MaxPageSize);
        result.Items.Should().HaveCount(PaginationDefaults.MaxPageSize);
    }

    [Fact]
    public void Paginate_WithRequest_Should_HonorMaxPageSize_When_MaxPageSizeIsGiven()
    {
        var request = new PaginationRequest { PageNumber = 1, PageSize = 500 };

        PaginationResponse<Item> result = InMemoryItems(150).Paginate(request, maxPageSize: int.MaxValue);

        result.PageSize.Should().Be(500);
        result.Items.Should().HaveCount(150);
    }

    [Fact]
    public void Paginate_WithPageNumberAndSize_Should_CapPageSizeAtDefaultMax_When_MaxPageSizeIsNotGiven()
    {
        PaginationResponse<Item> result = InMemoryItems(150).Paginate(2, 500);

        result.PageSize.Should().Be(PaginationDefaults.MaxPageSize);
        result.Items.Select(i => i.Id).Should().Equal(Enumerable.Range(101, 50));
    }

    [Fact]
    public void Paginate_WithPageNumberAndSize_Should_HonorMaxPageSize_When_MaxPageSizeIsGiven()
    {
        PaginationResponse<Item> result = InMemoryItems(150).Paginate(1, 500, maxPageSize: 130);

        result.PageSize.Should().Be(130);
        result.Items.Should().HaveCount(130);
    }

    [Fact]
    public void Paginate_Should_NormalizeValuesBelowOne_When_MaxPageSizeIsGiven()
    {
        PaginationResponse<Item> result = InMemoryItems().Paginate(-3, 0, maxPageSize: 5);

        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(1);
        result.Items.Should().ContainSingle().Which.Id.Should().Be(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Paginate_Should_Throw_When_MaxPageSizeIsLessThanOne(int maxPageSize)
    {
        var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

        Action act = () => InMemoryItems().Paginate(request, maxPageSize: maxPageSize);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Paginate_Should_ReturnEmptyPage_When_PageNumberIsIntMaxValue()
    {
        PaginationResponse<Item> result = InMemoryItems().Paginate(int.MaxValue, 100);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(10);
    }

    [Fact]
    public void Paginate_WithRequest_ShouldWorkOnNonEfQueryable()
    {
        var request = new PaginationRequest { PageNumber = 3, PageSize = 4 };

        PaginationResponse<Item> result = InMemoryItems().Paginate(request);

        result.Items.Select(i => i.Id).Should().Equal(9, 10);
        result.TotalCount.Should().Be(10);
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void Paginate_WithRequest_ShouldApplySearch()
    {
        var request = new PaginationRequest { PageNumber = 1, PageSize = 10, Search = "  Item07 " };

        PaginationResponse<Item> result = InMemoryItems().Paginate(request, term => i => i.Name == term);

        result.Items.Should().ContainSingle().Which.Id.Should().Be(7);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public void Paginate_WithRequest_ShouldIgnoreSearch_WhenSearchIsEmpty()
    {
        var request = new PaginationRequest { PageNumber = 1, PageSize = 5, Search = "   " };

        PaginationResponse<Item> result = InMemoryItems().Paginate(request, term => i => i.Name == term);

        result.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(10);
    }

    [Fact]
    public void Paginate_WithPageNumberAndSize_ShouldSkipCount_WhenTotalCountDisabled()
    {
        PaginationResponse<Item> result = InMemoryItems().Paginate(2, 5, includeTotalCount: false);

        result.Items.Select(i => i.Id).Should().Equal(6, 7, 8, 9, 10);
        result.TotalCount.Should().Be(-1);
    }

    [Fact]
    public async Task Paginate_WithPageNumberAndSize_ShouldMatchAsyncResult()
    {
        using ItemDbContext context = await CreateSeededContextAsync();

        PaginationResponse<Item> syncResult = PaginateSynchronously(context.Items.OrderBy(i => i.Id));
        PaginationResponse<Item> asyncResult = await context.Items.OrderBy(i => i.Id).PaginateAsync(2, 3);

        syncResult.Items.Select(i => i.Id).Should().Equal(asyncResult.Items.Select(i => i.Id));
        syncResult.TotalCount.Should().Be(asyncResult.TotalCount);

        // The synchronous overload is the subject under test, so it runs outside the async method.
        static PaginationResponse<Item> PaginateSynchronously(IQueryable<Item> items) => items.Paginate(2, 3);
    }
}
