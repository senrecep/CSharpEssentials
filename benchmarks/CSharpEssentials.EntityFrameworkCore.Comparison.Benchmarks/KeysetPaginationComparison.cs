using System.Diagnostics.CodeAnalysis;
using CSharpEssentials.EntityFrameworkCore.Pagination;
using CSharpEssentials.EntityFrameworkCore.Pagination.Keyset;
using CSharpEssentials.EntityFrameworkCore.Pagination.Requests;
using CSharpEssentials.EntityFrameworkCore.Pagination.Responses;
using CSharpEssentials.ResultPattern;
#if NET10_0_OR_GREATER
using MR.EntityFrameworkCore.KeysetPagination;
#endif

namespace CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks;

/// <summary>
/// A 20-row keyset page over 10,000 feed rows ordered by <c>PublishedAt</c> then <c>Id</c>, both descending.
/// First: no cursor. After: the cursor of the row at ordinal 5,000. Every row decodes its own cursor format and issues the
/// next cursor. The hand-written row's cursor has no version, direction or key checks (labelled). CSE also issues the
/// previous cursor. MR (net10.0 and later) takes the decoded hand-written cursor as its reference object;
/// <c>MR_KeysetPaginate_Prebuilt_HasNextAsync</c> follows MR's documented <c>HasNextAsync</c>, which runs one extra query
/// (labelled). CSE's codec is internal, so <c>After</c> minus <c>First</c> on the CSE rows is the only view of the cursor
/// cost. Parity: every row returns the same 20 ids and flags, and its next cursor leads to the same second page.
/// </summary>
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "BenchmarkDotNet owns the lifetime: the database and contexts are created in [GlobalSetup] and disposed in [GlobalCleanup].")]
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class KeysetPaginationComparison
{
    private const int Limit = 20;
    private const int StartOrdinal = 5_000;

    private static readonly KeysetOrdering<FeedItem> NewestFirst =
        new KeysetOrdering<FeedItem>().Descending(x => x.PublishedAt).Descending(x => x.Id);

#if NET10_0_OR_GREATER
    private static readonly KeysetQueryDefinition<FeedItem> Prebuilt =
        KeysetQuery.Build<FeedItem>(b => b.Descending(x => x.PublishedAt).Descending(x => x.Id));
#endif

    private BenchDatabase _database = null!;
    private PlainContext _context = null!;
    private string? _handWrittenCursor;
    private string? _cseCursor;

    [Params("First", "After")]
    public string Page { get; set; } = "First";

    [GlobalSetup]
    public void Setup()
    {
        _database = BenchDatabase.Create();
        FeedSeed.Insert(_database);
        _context = _database.Plain();
        if (Page == "After")
            (_handWrittenCursor, _cseCursor) = CursorsAt(StartOrdinal).GetAwaiter().GetResult();

        Verify().GetAwaiter().GetResult();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _context.Dispose();
        _database.Dispose();
    }

    // https://learn.microsoft.com/ef/core/querying/pagination#keyset-pagination
    [Benchmark(Baseline = true)]
    public Task<FeedPage> HandWritten_WhereOrderTake() => HandWritten(_handWrittenCursor);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore/Readme.MD#keyset-pagination
    [Benchmark]
    public Task<Result<KeysetPaginationResponse<FeedItem>>> CSharpEssentials_KeysetPaginateAsync_CachedOrdering() => CseCached(_cseCursor);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore/Readme.MD#keyset-pagination
    [Benchmark]
    public Task<Result<KeysetPaginationResponse<FeedItem>>> CSharpEssentials_KeysetPaginateAsync_Func() => CseFunc(_cseCursor);

#if NET10_0_OR_GREATER
    // https://github.com/mrahhal/MR.EntityFrameworkCore.KeysetPagination/blob/84fd37923e35db4a23244e40189fd42efb60caf6/README.md#prebuilt-keyset-query-definition
    [Benchmark]
    public Task<FeedPage> MR_KeysetPaginate_Prebuilt_TakePlusOne() => MrPrebuilt(_handWrittenCursor);

    // https://github.com/mrahhal/MR.EntityFrameworkCore.KeysetPagination/blob/84fd37923e35db4a23244e40189fd42efb60caf6/README.md#usage
    [Benchmark]
    public Task<FeedPage> MR_KeysetPaginate_Inline_TakePlusOne() => MrInline(_handWrittenCursor);

    // https://github.com/mrahhal/MR.EntityFrameworkCore.KeysetPagination/blob/84fd37923e35db4a23244e40189fd42efb60caf6/README.md#getting-the-data
    [Benchmark]
    public Task<FeedPage> MR_KeysetPaginate_Prebuilt_HasNextAsync() => MrHasNext(_handWrittenCursor);
#endif

    private IQueryable<FeedItem> Feed => _context.Set<FeedItem>().AsNoTracking();

    private async Task<FeedPage> HandWritten(string? cursor)
    {
        IQueryable<FeedItem> query = Feed;
        if (cursor is not null)
        {
            var reference = HandWrittenCursor.Decode(cursor);
            DateTime at = reference.PublishedAt;
            int id = reference.Id;
            query = query.Where(x => x.PublishedAt < at || x.PublishedAt == at && x.Id < id);
        }

        List<FeedItem> items = await query.OrderByDescending(x => x.PublishedAt).ThenByDescending(x => x.Id).Take(Limit + 1).ToListAsync();
        return FeedPage.FromLimitPlusOne(items, Limit, hasPrevious: cursor is not null);
    }

    private Task<Result<KeysetPaginationResponse<FeedItem>>> CseCached(string? cursor) =>
        Feed.KeysetPaginateAsync(new KeysetPaginationRequest { Limit = Limit, After = cursor }, NewestFirst);

    private Task<Result<KeysetPaginationResponse<FeedItem>>> CseFunc(string? cursor) =>
        Feed.KeysetPaginateAsync(new KeysetPaginationRequest { Limit = Limit, After = cursor }, k => k.Descending(x => x.PublishedAt).Descending(x => x.Id));

#if NET10_0_OR_GREATER
    private async Task<FeedPage> MrPrebuilt(string? cursor)
    {
        KeysetPaginationContext<FeedItem> keyset = Feed.KeysetPaginate(Prebuilt, KeysetPaginationDirection.Forward, Reference(cursor));
        List<FeedItem> items = await keyset.Query.Take(Limit + 1).ToListAsync();
        keyset.EnsureCorrectOrder(items);
        return FeedPage.FromLimitPlusOne(items, Limit, hasPrevious: cursor is not null);
    }

    private async Task<FeedPage> MrInline(string? cursor)
    {
        KeysetPaginationContext<FeedItem> keyset = Feed.KeysetPaginate(
            b => b.Descending(x => x.PublishedAt).Descending(x => x.Id), KeysetPaginationDirection.Forward, Reference(cursor));
        List<FeedItem> items = await keyset.Query.Take(Limit + 1).ToListAsync();
        keyset.EnsureCorrectOrder(items);
        return FeedPage.FromLimitPlusOne(items, Limit, hasPrevious: cursor is not null);
    }

    private async Task<FeedPage> MrHasNext(string? cursor)
    {
        KeysetPaginationContext<FeedItem> keyset = Feed.KeysetPaginate(Prebuilt, KeysetPaginationDirection.Forward, Reference(cursor));
        List<FeedItem> items = await keyset.Query.Take(Limit).ToListAsync();
        keyset.EnsureCorrectOrder(items);
        bool hasNext = await keyset.HasNextAsync(items);
        return new FeedPage(items, hasNext, cursor is not null, hasNext ? HandWrittenCursor.Encode(items[^1]) : null);
    }

    private static HandWrittenCursor? Reference(string? cursor) => cursor is null ? null : HandWrittenCursor.Decode(cursor);
#endif

    private async Task<(string HandWritten, string Cse)> CursorsAt(int ordinal)
    {
        FeedItem row = await Feed.OrderByDescending(x => x.PublishedAt).ThenByDescending(x => x.Id).Skip(ordinal - 1).FirstAsync();
        string? cursor = null;
        for (int read = 0; read < ordinal; read += KeysetPaginationOptions.DefaultMaxLimit)
        {
            Result<KeysetPaginationResponse<FeedItem>> page = await Feed.KeysetPaginateAsync(
                new KeysetPaginationRequest { Limit = KeysetPaginationOptions.DefaultMaxLimit, After = cursor }, NewestFirst);
            cursor = page.Value.NextCursor;
        }

        return (HandWrittenCursor.Encode(row), cursor ?? throw new InvalidOperationException("The feed is shorter than the start ordinal."));
    }

    private async Task Verify()
    {
        string scenario = $"KeysetPagination Page={Page}";
        FeedPage handWritten = await HandWritten(_handWrittenCursor);
        Result<KeysetPaginationResponse<FeedItem>> cached = await CseCached(_cseCursor);
        Result<KeysetPaginationResponse<FeedItem>> func = await CseFunc(_cseCursor);
        List<(string, string)> pages =
        [
            ("HandWritten", Describe(handWritten)),
            ("CSharpEssentials cached", Describe(cached)),
            ("CSharpEssentials Func", Describe(func)),
        ];
        List<(string, string)> nextPages =
        [
            ("HandWritten", Ids((await HandWritten(handWritten.NextCursor)).Items)),
            ("CSharpEssentials cached", Ids((await CseCached(cached.Value.NextCursor)).Value.Items)),
            ("CSharpEssentials Func", Ids((await CseFunc(func.Value.NextCursor)).Value.Items)),
        ];
#if NET10_0_OR_GREATER
        foreach ((string name, Func<string?, Task<FeedPage>> row) in new (string, Func<string?, Task<FeedPage>>)[]
                 { ("MR prebuilt", MrPrebuilt), ("MR inline", MrInline), ("MR HasNextAsync", MrHasNext) })
        {
            FeedPage page = await row(_handWrittenCursor);
            pages.Add((name, Describe(page)));
            nextPages.Add((name, Ids((await row(page.NextCursor)).Items)));
        }
#endif
        Parity.True(scenario, handWritten.Items.Count == Limit, "the page is not full");
        Parity.Same(scenario, [.. pages]);
        Parity.Same(scenario + " next page", [.. nextPages]);
    }

    private static string Describe(FeedPage page) => $"{Ids(page.Items)}|next={page.HasNext}|previous={page.HasPrevious}";

    private static string Describe(Result<KeysetPaginationResponse<FeedItem>> page) =>
        $"{Ids(page.Value.Items)}|next={page.Value.HasNext}|previous={page.Value.HasPrevious}";

    private static string Ids(IReadOnlyList<FeedItem> items) => string.Join(',', items.Select(static item => item.Id));
}
