using System.Diagnostics.CodeAnalysis;
using CSharpEssentials.EntityFrameworkCore.Pagination;
using CSharpEssentials.EntityFrameworkCore.Pagination.Responses;
#if NET9_0_OR_GREATER
using X.PagedList;
using X.PagedList.EF;
#endif

namespace CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks;

/// <summary>
/// A 20-row offset page with a total count over the same 10,000 feed rows ordered by <c>Id</c>; page 1 and page 250, both
/// in range. Every row runs a COUNT and a Skip/Take query. CSE normalizes the request first; X.PagedList.EF (net9.0 and
/// later) skips the data query past the last page and throws for a page number below 1, which these inputs do not reach.
/// Parity: the same ids, total count and page count.
/// </summary>
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "BenchmarkDotNet owns the lifetime: the database and contexts are created in [GlobalSetup] and disposed in [GlobalCleanup].")]
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class OffsetPaginationComparison
{
    private const int PageSize = 20;

    private BenchDatabase _database = null!;
    private PlainContext _context = null!;

    [Params(1, 250)]
    public int PageNumber { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _database = BenchDatabase.Create();
        FeedSeed.Insert(_database);
        _context = _database.Plain();
        Verify().GetAwaiter().GetResult();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _context.Dispose();
        _database.Dispose();
    }

    private IQueryable<FeedItem> Feed => _context.Set<FeedItem>().AsNoTracking().OrderBy(x => x.Id);

    // https://learn.microsoft.com/ef/core/querying/pagination#offset-pagination
    [Benchmark(Baseline = true)]
    public async Task<OffsetPage> Ef_CountSkipTake()
    {
        IQueryable<FeedItem> feed = Feed;
        int total = await feed.CountAsync();
        List<FeedItem> items = await feed.Skip((PageNumber - 1) * PageSize).Take(PageSize).ToListAsync();
        return new OffsetPage(items, total);
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore/Readme.MD#pagination
    [Benchmark]
    public Task<PaginationResponse<FeedItem>> CSharpEssentials_PaginateAsync() => Feed.PaginateAsync(PageNumber, PageSize);

#if NET9_0_OR_GREATER
    // https://github.com/udndc/X.PagedList/blob/a3249425910a09eb80e038afe3daf2a032359a2b/src/X.PagedList.EF/PagedListExtensions.cs
    [Benchmark]
    public Task<IPagedList<FeedItem>> XPagedList_ToPagedListAsync() => Feed.ToPagedListAsync(PageNumber, PageSize);
#endif

    private async Task Verify()
    {
        OffsetPage ef = await Ef_CountSkipTake();
        PaginationResponse<FeedItem> cse = await CSharpEssentials_PaginateAsync();
        int pages = (ef.TotalCount + PageSize - 1) / PageSize;
        Parity.True($"OffsetPagination PageNumber={PageNumber}", ef.Items.Count == PageSize, "the page is not full");
        Parity.Same($"OffsetPagination PageNumber={PageNumber}",
            ("Ef", $"{Ids(ef.Items)}|{ef.TotalCount}|{pages}"),
            ("CSharpEssentials", $"{Ids(cse.Items)}|{cse.TotalCount}|{cse.TotalPages}")
#if NET9_0_OR_GREATER
            , ("X.PagedList", await XPagedList())
#endif
            );
    }

#if NET9_0_OR_GREATER
    private async Task<string> XPagedList()
    {
        IPagedList<FeedItem> page = await XPagedList_ToPagedListAsync();
        return $"{Ids([.. page])}|{page.TotalItemCount}|{page.PageCount}";
    }
#endif

    private static string Ids(IReadOnlyList<FeedItem> items) => string.Join(',', items.Select(static item => item.Id));
}
