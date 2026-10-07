using System.Linq.Expressions;

using CSharpEssentials.EntityFrameworkCore.Pagination;
using CSharpEssentials.EntityFrameworkCore.Pagination.Keyset;
using CSharpEssentials.EntityFrameworkCore.Pagination.Requests;
using CSharpEssentials.EntityFrameworkCore.Pagination.Responses;
using CSharpEssentials.ResultPattern;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CSharpEssentials.Tests.EntityFrameworkCore.Keyset;

public sealed class KeysetPaginationSqliteTests : IDisposable
{
    private const int RowCount = 25;
    private static readonly DateTime Start = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<KeysetDbContext> _options;

    public KeysetPaginationSqliteTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<KeysetDbContext>().UseSqlite(_connection).Options;

        using var context = new KeysetDbContext(_options);
        context.Database.EnsureCreated();
        context.Rows.AddRange(Enumerable.Range(0, RowCount).Select(CreateRow));
        context.SaveChanges();
    }

    public void Dispose() => _connection.Dispose();

    // Ids are unique but not in insertion order; rows 5..11 share one CreatedAt so the tie-breaker decides their order.
    private static KeysetRow CreateRow(int i) => new()
    {
        Id = (i * 37 % 101) + 1,
        CreatedAt = i is >= 5 and <= 11 ? Start.AddHours(5) : Start.AddHours(i),
        Name = $"n{i % 6}",
        Token = new Guid((uint)(i * 7919 % 25), 0, 0, 0, 0, 0, 0, 0, 0, 0, (byte)i),
        Sequence = i % 5 * 1000L,
        Price = i % 4 * 1.5m,
        Score = i % 3 * 0.25d,
        At = new DateTimeOffset(2024, 1, 1 + (i % 5), 0, 0, 0, TimeSpan.FromHours(i % 3)),
        Status = (KeysetRowStatus)(i % 3),
        Day = new DateOnly(2024, 1, 1).AddDays(i % 4),
        Time = new TimeOnly(i % 6, 30),
        Duration = TimeSpan.FromMinutes(i % 5),
        Note = i % 2 == 0 ? null : "note",
        Rank = i % 2 == 0 ? null : i,
    };

    private static KeysetOrdering<KeysetRow> NewestFirst =>
        new KeysetOrdering<KeysetRow>().Descending(x => x.CreatedAt).Ascending(x => x.Id);

    private KeysetDbContext CreateContext() => new(_options);

    private async Task<List<int>> CanonicalNewestFirstAsync()
    {
        using KeysetDbContext context = CreateContext();
        List<KeysetRow> rows = await context.Rows.ToListAsync();
        return [.. rows.OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Id).Select(x => x.Id)];
    }

    private async Task<KeysetPaginationResponse<KeysetRow>> PageAsync(
        KeysetOrdering<KeysetRow> ordering,
        int limit,
        string? after = null,
        string? before = null,
        KeysetPaginationOptions? options = null)
    {
        using KeysetDbContext context = CreateContext();
        Result<KeysetPaginationResponse<KeysetRow>> result = await context.Rows.KeysetPaginateAsync(
            new KeysetPaginationRequest { Limit = limit, After = after, Before = before },
            ordering,
            options ?? KeysetPaginationOptions.Default);

        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }

    private async Task<(List<int> Forward, List<int> Backward)> WalkAsync(
        KeysetOrdering<KeysetRow> ordering,
        int limit,
        KeysetPaginationOptions? options = null)
    {
        List<KeysetPaginationResponse<KeysetRow>> forwardPages = [await PageAsync(ordering, limit, options: options)];
        while (forwardPages[^1].NextCursor is { } next)
            forwardPages.Add(await PageAsync(ordering, limit, after: next, options: options));

        List<KeysetPaginationResponse<KeysetRow>> backwardPages = [forwardPages[^1]];
        while (backwardPages[0].PreviousCursor is { } previous)
            backwardPages.Insert(0, await PageAsync(ordering, limit, before: previous, options: options));

        forwardPages.Should().OnlyContain(page => page.Items.Count > 0 && page.Items.Count <= limit);
        backwardPages.Should().OnlyContain(page => page.Items.Count > 0 && page.Items.Count <= limit);
        return (
            [.. forwardPages.SelectMany(page => page.Items).Select(x => x.Id)],
            [.. backwardPages.SelectMany(page => page.Items).Select(x => x.Id)]);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(7)]
    [InlineData(24)]
    [InlineData(25)]
    public async Task KeysetPaginateAsync_Should_WalkCanonicalOrderBothWays_When_KeysHaveMixedDirections(int limit)
    {
        List<int> expected = await CanonicalNewestFirstAsync();

        (List<int> forward, List<int> backward) = await WalkAsync(NewestFirst, limit);

        forward.Should().Equal(expected);
        backward.Should().Equal(expected);
    }

    [Fact]
    public async Task KeysetPaginateAsync_Should_NotSkipOrRepeatRows_When_SevenRowsShareTheLeadingKeyAndLimitIsThree()
    {
        KeysetOrdering<KeysetRow> ordering = new KeysetOrdering<KeysetRow>().Descending(x => x.CreatedAt).Descending(x => x.Id);
        using KeysetDbContext context = CreateContext();
        List<KeysetRow> rows = await context.Rows.ToListAsync();
        List<int> expected = [.. rows.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id).Select(x => x.Id)];

        (List<int> forward, List<int> backward) = await WalkAsync(ordering, 3);

        rows.Count(x => x.CreatedAt == Start.AddHours(5)).Should().Be(7);
        forward.Should().Equal(expected).And.OnlyHaveUniqueItems();
        backward.Should().Equal(expected);
    }

    [Fact]
    public async Task KeysetPaginateAsync_Should_ReportNoPreviousPage_When_OnFirstPage()
    {
        KeysetPaginationResponse<KeysetRow> page = await PageAsync(NewestFirst, 10);

        page.HasPrevious.Should().BeFalse();
        page.PreviousCursor.Should().BeNull();
        page.HasNext.Should().BeTrue();
        page.Items.Should().HaveCount(10);
    }

    [Fact]
    public async Task KeysetPaginateAsync_Should_ReportNoNextPage_When_OnLastPage()
    {
        KeysetPaginationResponse<KeysetRow> first = await PageAsync(NewestFirst, 10);
        KeysetPaginationResponse<KeysetRow> second = await PageAsync(NewestFirst, 10, after: first.NextCursor);
        KeysetPaginationResponse<KeysetRow> last = await PageAsync(NewestFirst, 10, after: second.NextCursor);

        last.Items.Should().HaveCount(5);
        last.HasNext.Should().BeFalse();
        last.HasPrevious.Should().BeTrue();
    }

    [Fact]
    public async Task KeysetPaginateAsync_Should_ReportNoPreviousPage_When_BackwardWalkReachesStart()
    {
        List<int> expected = await CanonicalNewestFirstAsync();
        KeysetPaginationResponse<KeysetRow> first = await PageAsync(NewestFirst, 10);
        KeysetPaginationResponse<KeysetRow> second = await PageAsync(NewestFirst, 10, after: first.NextCursor);

        KeysetPaginationResponse<KeysetRow> back = await PageAsync(NewestFirst, 10, before: second.PreviousCursor);

        back.Items.Select(x => x.Id).Should().Equal(expected.Take(10));
        back.HasPrevious.Should().BeFalse();
        back.HasNext.Should().BeTrue();
    }

    [Fact]
    public async Task KeysetPaginateAsync_Should_ReturnSinglePageWithoutCursors_When_AllRowsFit()
    {
        KeysetPaginationResponse<KeysetRow> page = await PageAsync(NewestFirst, 100);

        page.Items.Should().HaveCount(RowCount);
        page.HasNext.Should().BeFalse();
        page.HasPrevious.Should().BeFalse();
    }

    [Fact]
    public async Task KeysetPaginateAsync_Should_ReturnEmptyPageWithoutCursors_When_QueryHasNoRows()
    {
        using KeysetDbContext context = CreateContext();

        Result<KeysetPaginationResponse<KeysetRow>> result = await context.Rows.Where(x => x.Id < 0)
            .KeysetPaginateAsync(new KeysetPaginationRequest { Limit = 5 }, k => k.Ascending(x => x.Id));

        result.Value.Items.Should().BeEmpty();
        result.Value.NextCursor.Should().BeNull();
        result.Value.PreviousCursor.Should().BeNull();
    }

    [Fact]
    public async Task KeysetPaginateAsync_Should_WalkIdOrder_When_SingleKeyIsUsed()
    {
        KeysetOrdering<KeysetRow> ordering = new KeysetOrdering<KeysetRow>().Ascending(x => x.Id);
        using KeysetDbContext context = CreateContext();
        List<int> expected = await context.Rows.OrderBy(x => x.Id).Select(x => x.Id).ToListAsync();

        (List<int> forward, List<int> backward) = await WalkAsync(ordering, 4);

        forward.Should().Equal(expected);
        backward.Should().Equal(expected);
    }

    [Fact]
    public async Task KeysetPaginateAsync_Should_ApplyKeysToFilteredQuery_When_QueryHasWhereClause()
    {
        using KeysetDbContext context = CreateContext();

        Result<KeysetPaginationResponse<KeysetRow>> first = await context.Rows.Where(x => x.Status == KeysetRowStatus.Active)
            .KeysetPaginateAsync(new KeysetPaginationRequest { Limit = 5 }, k => k.Descending(x => x.Id));
        Result<KeysetPaginationResponse<KeysetRow>> second = await context.Rows.Where(x => x.Status == KeysetRowStatus.Active)
            .KeysetPaginateAsync(new KeysetPaginationRequest { Limit = 5, After = first.Value.NextCursor }, k => k.Descending(x => x.Id));

        first.Value.Items.Concat(second.Value.Items).Should()
            .OnlyContain(x => x.Status == KeysetRowStatus.Active)
            .And.HaveCount(8)
            .And.BeInDescendingOrder(x => x.Id);
        second.Value.HasNext.Should().BeFalse();
    }

    [Fact]
    public async Task KeysetPaginateAsync_Should_ClampLimitToMaxLimit_When_LimitIsTooLarge()
    {
        var options = new KeysetPaginationOptions { MaxLimit = 5 };

        KeysetPaginationResponse<KeysetRow> page = await PageAsync(NewestFirst, 1000, options: options);

        page.Items.Should().HaveCount(5);
        page.HasNext.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public async Task KeysetPaginateAsync_Should_ReturnOneRow_When_LimitIsLessThanOne(int limit)
    {
        KeysetPaginationResponse<KeysetRow> page = await PageAsync(NewestFirst, limit);

        page.Items.Should().ContainSingle();
    }

    [Fact]
    public void CreatePageQuery_Should_TakeDefaultMaxLimitPlusOne_When_LimitExceedsDefaultMax()
    {
        using KeysetDbContext context = CreateContext();

        Result<IQueryable<KeysetRow>> query = KeysetPaginationExtensions.CreatePageQuery(
            context.Rows, new KeysetPaginationRequest { Limit = 1000 }, NewestFirst, KeysetPaginationOptions.Default);

        query.Value.ToQueryString().Should().Contain($"{KeysetPaginationOptions.DefaultMaxLimit + 1}");
    }

    [Fact]
    public async Task KeysetPaginateAsync_Should_ReturnValidationError_When_AfterAndBeforeAreBothSet()
    {
        KeysetPaginationResponse<KeysetRow> first = await PageAsync(NewestFirst, 5);
        KeysetPaginationResponse<KeysetRow> second = await PageAsync(NewestFirst, 5, after: first.NextCursor);
        using KeysetDbContext context = CreateContext();

        Result<KeysetPaginationResponse<KeysetRow>> result = await context.Rows.KeysetPaginateAsync(
            new KeysetPaginationRequest { Limit = 5, After = second.NextCursor, Before = second.PreviousCursor },
            NewestFirst);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Type.Should().Be(CSharpEssentials.Errors.ErrorType.Validation);
        result.FirstError.Code.Should().Be(KeysetCursorErrors.AfterAndBeforeCode);
    }

    [Theory]
    [InlineData("garbage")]
    [InlineData("eyJ2IjoxfQ")]
    public async Task KeysetPaginateAsync_Should_ReturnValidationError_When_CursorIsInvalid(string cursor)
    {
        using KeysetDbContext context = CreateContext();

        Result<KeysetPaginationResponse<KeysetRow>> result = await context.Rows.KeysetPaginateAsync(
            new KeysetPaginationRequest { Limit = 5, Before = cursor }, NewestFirst);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be(KeysetCursorErrors.InvalidCode);
    }

    [Fact]
    public async Task KeysetPaginateAsync_Should_ReturnInvalidCursor_When_CursorExceedsMaxCursorLength()
    {
        KeysetPaginationResponse<KeysetRow> page = await PageAsync(NewestFirst, 5);
        var options = new KeysetPaginationOptions { MaxCursorLength = page.NextCursor!.Length - 1 };
        using KeysetDbContext context = CreateContext();

        Result<KeysetPaginationResponse<KeysetRow>> tooLong = await context.Rows.KeysetPaginateAsync(
            new KeysetPaginationRequest { Limit = 5, After = page.NextCursor }, NewestFirst, options);
        Result<KeysetPaginationResponse<KeysetRow>> defaultCap = await context.Rows.KeysetPaginateAsync(
            new KeysetPaginationRequest { Limit = 5, Before = new string('A', KeysetPaginationOptions.DefaultMaxCursorLength + 1) },
            NewestFirst);

        tooLong.FirstError.Code.Should().Be(KeysetCursorErrors.InvalidCode);
        defaultCap.FirstError.Code.Should().Be(KeysetCursorErrors.InvalidCode);
        defaultCap.FirstError.Metadata.Should().ContainKey(KeysetCursorErrors.ParameterKey).WhoseValue.Should().Be("before");
    }

    [Fact]
    public async Task KeysetPaginateAsync_Should_WalkPastInfiniteValues_When_DoubleKeyIsNotFinite()
    {
        using (KeysetDbContext seed = CreateContext())
        {
            seed.Rows.AddRange(
                WithScore(CreateRow(0), 1001, double.PositiveInfinity),
                WithScore(CreateRow(1), 1002, double.NegativeInfinity),
                WithScore(CreateRow(2), 1003, double.PositiveInfinity));
            await seed.SaveChangesAsync();
        }

        await AssertWalkMatchesDatabaseOrderAsync(x => x.Score, descending: false);
        await AssertWalkMatchesDatabaseOrderAsync(x => x.Score, descending: true);
    }

    private static KeysetRow WithScore(KeysetRow row, int id, double score)
    {
        row.Id = id;
        row.Score = score;
        return row;
    }

    [Fact]
    public async Task KeysetPaginateAsync_Should_ReturnKeyMismatch_When_CursorComesFromAnotherOrdering()
    {
        KeysetPaginationResponse<KeysetRow> page = await PageAsync(new KeysetOrdering<KeysetRow>().Ascending(x => x.Id), 5);
        using KeysetDbContext context = CreateContext();

        Result<KeysetPaginationResponse<KeysetRow>> result = await context.Rows.KeysetPaginateAsync(
            new KeysetPaginationRequest { Limit = 5, After = page.NextCursor }, NewestFirst);

        result.FirstError.Code.Should().Be(KeysetCursorErrors.KeyMismatchCode);
    }

    [Fact]
    public async Task KeysetPaginateAsync_Should_ReturnDirectionMismatch_When_NextCursorIsSentAsBefore()
    {
        KeysetPaginationResponse<KeysetRow> page = await PageAsync(NewestFirst, 5);
        using KeysetDbContext context = CreateContext();

        Result<KeysetPaginationResponse<KeysetRow>> result = await context.Rows.KeysetPaginateAsync(
            new KeysetPaginationRequest { Limit = 5, Before = page.NextCursor }, NewestFirst);

        result.FirstError.Code.Should().Be(KeysetCursorErrors.DirectionMismatchCode);
    }

    [Fact]
    public async Task CreatePageQuery_Should_SendCursorValuesAsParameters_When_CursorIsApplied()
    {
        KeysetOrdering<KeysetRow> ordering = new KeysetOrdering<KeysetRow>()
            .Descending(x => x.CreatedAt).Ascending(x => x.Name).Ascending(x => x.Id);
        KeysetPaginationResponse<KeysetRow> first = await PageAsync(ordering, 3);
        KeysetPaginationResponse<KeysetRow> second = await PageAsync(ordering, 3, after: first.NextCursor);
        using KeysetDbContext context = CreateContext();

        string firstSql = QueryString(context, ordering, first.NextCursor!);
        string secondSql = QueryString(context, ordering, second.NextCursor!);

        string firstCommand = StripParameters(firstSql);
        firstCommand.Should().Contain("<= @").And.Contain("> @");
        firstCommand.Should().NotContain($"'{first.Items[^1].Name}'");
        firstCommand.Should().NotContain($"{first.Items[^1].CreatedAt:yyyy-MM-dd}");
        firstCommand.Should().Be(StripParameters(secondSql));
        firstSql.Should().NotBe(secondSql);
    }

    [Fact]
    public async Task KeysetPaginateAsync_Should_UseCustomPredicateBuilder_When_Configured()
    {
        var builder = new RecordingPredicateBuilder();
        var options = new KeysetPaginationOptions { PredicateBuilder = builder };
        List<int> expected = await CanonicalNewestFirstAsync();

        (List<int> forward, List<int> backward) = await WalkAsync(NewestFirst, 6, options);

        forward.Should().Equal(expected);
        backward.Should().Equal(expected);
        builder.Calls.Should().Contain(columns =>
            columns[0].Direction == KeysetDirection.Descending && columns[1].Direction == KeysetDirection.Ascending);
        builder.Calls.Should().Contain(columns =>
            columns[0].Direction == KeysetDirection.Ascending && columns[1].Direction == KeysetDirection.Descending);
        builder.Calls.Should().OnlyContain(columns => columns[0].Type == typeof(DateTime) && columns[1].Type == typeof(int));
    }

    [Fact]
    public async Task KeysetPaginateAsync_Should_Throw_When_PredicateBuilderReturnsNonBooleanExpression()
    {
        KeysetPaginationResponse<KeysetRow> page = await PageAsync(NewestFirst, 5);
        var options = new KeysetPaginationOptions { PredicateBuilder = new ConstantPredicateBuilder() };
        using KeysetDbContext context = CreateContext();

        Func<Task> act = () => context.Rows.KeysetPaginateAsync(
            new KeysetPaginationRequest { Limit = 5, After = page.NextCursor }, NewestFirst, options);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task KeysetPaginateAsync_Should_ProtectCursors_When_CustomProtectorIsConfigured()
    {
        var options = new KeysetPaginationOptions { Protector = new PrefixCursorProtector() };
        List<int> expected = await CanonicalNewestFirstAsync();
        KeysetPaginationResponse<KeysetRow> first = await PageAsync(NewestFirst, 5, options: options);
        using KeysetDbContext context = CreateContext();

        Result<KeysetPaginationResponse<KeysetRow>> unprotected = await context.Rows.KeysetPaginateAsync(
            new KeysetPaginationRequest { Limit = 5, After = first.NextCursor![PrefixCursorProtector.Prefix.Length..] }, NewestFirst, options);
        (List<int> forward, List<int> backward) = await WalkAsync(NewestFirst, 5, options);

        first.NextCursor.Should().StartWith(PrefixCursorProtector.Prefix);
        unprotected.FirstError.Code.Should().Be(KeysetCursorErrors.InvalidCode);
        forward.Should().Equal(expected);
        backward.Should().Equal(expected);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task KeysetPaginateAsync_Should_MatchDatabaseOrder_When_KeyIsString(bool descending) =>
        AssertWalkMatchesDatabaseOrderAsync(x => x.Name, descending);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task KeysetPaginateAsync_Should_MatchDatabaseOrder_When_KeyIsGuid(bool descending) =>
        AssertWalkMatchesDatabaseOrderAsync(x => x.Token, descending);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task KeysetPaginateAsync_Should_MatchDatabaseOrder_When_KeyIsLong(bool descending) =>
        AssertWalkMatchesDatabaseOrderAsync(x => x.Sequence, descending);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task KeysetPaginateAsync_Should_MatchDatabaseOrder_When_KeyIsDouble(bool descending) =>
        AssertWalkMatchesDatabaseOrderAsync(x => x.Score, descending);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task KeysetPaginateAsync_Should_MatchDatabaseOrder_When_KeyIsEnum(bool descending) =>
        AssertWalkMatchesDatabaseOrderAsync(x => x.Status, descending);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task KeysetPaginateAsync_Should_MatchDatabaseOrder_When_KeyIsDateTime(bool descending) =>
        AssertWalkMatchesDatabaseOrderAsync(x => x.CreatedAt, descending);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task KeysetPaginateAsync_Should_MatchDatabaseOrder_When_KeyIsDateOnly(bool descending) =>
        AssertWalkMatchesDatabaseOrderAsync(x => x.Day, descending);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task KeysetPaginateAsync_Should_MatchDatabaseOrder_When_KeyIsTimeOnly(bool descending) =>
        AssertWalkMatchesDatabaseOrderAsync(x => x.Time, descending);

    private async Task AssertWalkMatchesDatabaseOrderAsync<TKey>(Expression<Func<KeysetRow, TKey>> key, bool descending)
    {
        var empty = new KeysetOrdering<KeysetRow>();
        KeysetOrdering<KeysetRow> ordering = (descending ? empty.Descending(key) : empty.Ascending(key)).Ascending(x => x.Id);
        List<int> expected;
        using (KeysetDbContext context = CreateContext())
        {
            IOrderedQueryable<KeysetRow> ordered = descending ? context.Rows.OrderByDescending(key) : context.Rows.OrderBy(key);
            expected = await ordered.ThenBy(x => x.Id).Select(x => x.Id).ToListAsync();
        }

        (List<int> forward, List<int> backward) = await WalkAsync(ordering, 4);

        forward.Should().Equal(expected);
        backward.Should().Equal(expected);
    }

    private static string QueryString(KeysetDbContext context, KeysetOrdering<KeysetRow> ordering, string after) =>
        KeysetPaginationExtensions.CreatePageQuery(
            context.Rows, new KeysetPaginationRequest { Limit = 3, After = after }, ordering, KeysetPaginationOptions.Default)
            .Value.ToQueryString();

    private static string StripParameters(string sql) =>
        string.Join('\n', sql.Split('\n').Where(line => !line.StartsWith(".param set", StringComparison.Ordinal)));

    private sealed class RecordingPredicateBuilder : IKeysetPredicateBuilder
    {
        public List<IReadOnlyList<KeysetColumn>> Calls { get; } = [];

        public Expression BuildPredicate(IReadOnlyList<KeysetColumn> columns)
        {
            Calls.Add(columns);
            return KeysetPredicateBuilder.Instance.BuildPredicate(columns);
        }
    }

    private sealed class ConstantPredicateBuilder : IKeysetPredicateBuilder
    {
        public Expression BuildPredicate(IReadOnlyList<KeysetColumn> columns) => Expression.Constant(1);
    }
}
