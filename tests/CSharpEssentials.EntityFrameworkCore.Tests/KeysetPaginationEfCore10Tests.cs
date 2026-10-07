using System.Data.Common;

using CSharpEssentials.EntityFrameworkCore.Pagination;
using CSharpEssentials.EntityFrameworkCore.Pagination.Keyset;
using CSharpEssentials.EntityFrameworkCore.Pagination.Requests;
using CSharpEssentials.EntityFrameworkCore.Pagination.Responses;
using CSharpEssentials.ResultPattern;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CSharpEssentials.EntityFrameworkCore.Tests;

public sealed class KeysetPaginationEfCore10Tests : IDisposable
{
    private static readonly DateTime Start = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static readonly KeysetOrdering<Post> NewestFirst =
        new KeysetOrdering<Post>().Descending(x => x.CreatedAt).Ascending(x => x.Id);

    private sealed class Post
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    private sealed class PostDbContext(DbContextOptions<PostDbContext> options) : DbContext(options)
    {
        public DbSet<Post> Posts => Set<Post>();

        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder.Entity<Post>().Property(x => x.Id).ValueGeneratedNever();
    }

    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<PostDbContext> _options;

    public KeysetPaginationEfCore10Tests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<PostDbContext>().UseSqlite(_connection).Options;

        using var context = new PostDbContext(_options);
        context.Database.EnsureCreated();
        context.Posts.AddRange(Enumerable.Range(0, 20).Select(i => new Post
        {
            Id = (i * 7 % 23) + 1,
            CreatedAt = i is >= 4 and <= 10 ? Start.AddHours(4) : Start.AddHours(i),
            Title = $"post {i}",
        }));
        context.SaveChanges();
    }

    public void Dispose() => _connection.Dispose();

    private async Task<KeysetPaginationResponse<Post>> PageAsync(string? after = null, string? before = null)
    {
        using var context = new PostDbContext(_options);
        Result<KeysetPaginationResponse<Post>> result = await context.Posts.KeysetPaginateAsync(
            new KeysetPaginationRequest { Limit = 3, After = after, Before = before }, NewestFirst);

        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }

    [Fact]
    public async Task KeysetPaginateAsync_Should_WalkCanonicalOrderBothWays_When_RunOnEfCore10()
    {
        List<int> expected;
        using (var context = new PostDbContext(_options))
            expected = [.. (await context.Posts.ToListAsync()).OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Id).Select(x => x.Id)];

        List<KeysetPaginationResponse<Post>> forward = [await PageAsync()];
        while (forward[^1].NextCursor is { } next)
            forward.Add(await PageAsync(after: next));
        List<KeysetPaginationResponse<Post>> backward = [forward[^1]];
        while (backward[0].PreviousCursor is { } previous)
            backward.Insert(0, await PageAsync(before: previous));

        forward.SelectMany(page => page.Items).Select(x => x.Id).Should().Equal(expected);
        backward.SelectMany(page => page.Items).Select(x => x.Id).Should().Equal(expected);
        forward[0].HasPrevious.Should().BeFalse();
        forward[^1].HasNext.Should().BeFalse();
    }

    [Fact]
    public async Task KeysetPaginateAsync_Should_ReturnValidationError_When_CursorIsInvalidOnEfCore10()
    {
        using var context = new PostDbContext(_options);

        Result<KeysetPaginationResponse<Post>> result = await context.Posts.KeysetPaginateAsync(
            new KeysetPaginationRequest { Limit = 3, After = "not-a-cursor" }, NewestFirst);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be(KeysetCursorErrors.InvalidCode);
    }

    [Fact]
    public async Task KeysetPaginateAsync_Should_SendIdenticalParameterisedSql_When_CursorsDifferOnEfCore10()
    {
        KeysetPaginationResponse<Post> first = await PageAsync();
        KeysetPaginationResponse<Post> second = await PageAsync(after: first.NextCursor);
        var interceptor = new CommandCapturingInterceptor();
        DbContextOptions<PostDbContext> options = new DbContextOptionsBuilder<PostDbContext>()
            .UseSqlite(_connection)
            .AddInterceptors(interceptor)
            .Options;

        foreach (string cursor in new[] { first.NextCursor!, second.NextCursor! })
        {
            using var context = new PostDbContext(options);
            Result<KeysetPaginationResponse<Post>> result = await context.Posts.KeysetPaginateAsync(
                new KeysetPaginationRequest { Limit = 3, After = cursor }, NewestFirst);
            result.IsSuccess.Should().BeTrue();
        }

        interceptor.Commands.Should().HaveCount(2);
        interceptor.Commands[0].Should().Contain("@").And.NotContain("2024-");
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
