using CSharpEssentials.EntityFrameworkCore.Transactions;
using CSharpEssentials.Transactions;

using FluentAssertions;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

internal sealed class TransactionNote
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
}

internal sealed class NotesDbContext(DbContextOptions<NotesDbContext> options) : DbContext(options)
{
    public DbSet<TransactionNote> Notes => Set<TransactionNote>();
}

public sealed class EfCoreTransactionRunnerTests : IDisposable
{
    private sealed class RetryOnTransientStrategy(ExecutionStrategyDependencies dependencies)
        : ExecutionStrategy(dependencies, maxRetryCount: 3, maxRetryDelay: TimeSpan.Zero)
    {
        protected override bool ShouldRetryOn(Exception exception) => exception is TransientTestException;

        protected override TimeSpan? GetNextDelay(Exception lastException) =>
            base.GetNextDelay(lastException) is null ? null : TimeSpan.Zero;
    }

    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<NotesDbContext> _options;
    private readonly DbContextOptions<NotesDbContext> _retryingOptions;

    public EfCoreTransactionRunnerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<NotesDbContext>().UseSqlite(_connection).Options;
        _retryingOptions = new DbContextOptionsBuilder<NotesDbContext>()
            .UseSqlite(_connection, sqlite => sqlite.ExecutionStrategy(dependencies => new RetryOnTransientStrategy(dependencies)))
            .Options;

        using NotesDbContext context = new(_options);
        context.Database.EnsureCreated();
    }

    public void Dispose() => _connection.Dispose();

    private List<string> StoredTexts()
    {
        using NotesDbContext context = new(_options);
        return [.. context.Notes.OrderBy(note => note.Id).Select(note => note.Text)];
    }

    private static async ValueTask<bool> AddAsync(NotesDbContext context, string text, CancellationToken cancellationToken)
    {
        context.Notes.Add(new TransactionNote { Text = text });
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    [Fact]
    public async Task ExecuteAsync_Should_Commit_When_ShouldCommit_Returns_True()
    {
        await using NotesDbContext context = new(_options);
        EfCoreTransactionRunner<NotesDbContext> runner = new(context);

        bool result = await runner.ExecuteAsync(token => AddAsync(context, "committed", token), _ => true);

        result.Should().BeTrue();
        context.Database.CurrentTransaction.Should().BeNull();
        StoredTexts().Should().Equal("committed");
    }

    [Fact]
    public async Task ExecuteAsync_Should_Roll_Back_When_ShouldCommit_Returns_False()
    {
        await using NotesDbContext context = new(_options);
        EfCoreTransactionRunner<NotesDbContext> runner = new(context);

        await runner.ExecuteAsync(token => AddAsync(context, "rolled back", token), _ => false);

        StoredTexts().Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_Should_Roll_Back_And_Rethrow_When_Work_Throws()
    {
        await using NotesDbContext context = new(_options);
        EfCoreTransactionRunner<NotesDbContext> runner = new(context);

        Func<Task> act = () => runner.ExecuteAsync<bool>(async token =>
        {
            await AddAsync(context, "rolled back", token);
            throw new InvalidOperationException("boom");
        }, _ => true).AsTask();

        await act.Should().ThrowAsync<InvalidOperationException>();
        StoredTexts().Should().BeEmpty();
    }

    [Fact]
    public async Task Nested_ExecuteAsync_Should_Join_Outer_Transaction()
    {
        await using NotesDbContext context = new(_options);
        EfCoreTransactionRunner<NotesDbContext> runner = new(context);
        IDbContextTransaction? outerTransaction = null;
        IDbContextTransaction? innerTransaction = null;

        await runner.ExecuteAsync(async token =>
        {
            outerTransaction = context.Database.CurrentTransaction;
            await AddAsync(context, "outer", token);
            await runner.ExecuteAsync(async innerToken =>
            {
                innerTransaction = context.Database.CurrentTransaction;
                return await AddAsync(context, "inner", innerToken);
            }, _ => true, token);
            return false;
        }, committed => committed);

        innerTransaction.Should().BeSameAs(outerTransaction);
        StoredTexts().Should().BeEmpty("the outer call decided to roll back, so the joined inner work is undone too");
    }

    [Fact]
    public async Task Nested_ExecuteAsync_Should_Commit_With_Outer_Transaction()
    {
        await using NotesDbContext context = new(_options);
        EfCoreTransactionRunner<NotesDbContext> runner = new(context);

        await runner.ExecuteAsync(async token =>
        {
            await AddAsync(context, "outer", token);
            return await runner.ExecuteAsync(innerToken => AddAsync(context, "inner", innerToken), _ => true, token);
        }, _ => true);

        StoredTexts().Should().Equal("outer", "inner");
    }

    [Fact]
    public async Task Nested_Failure_Should_Roll_Back_To_Savepoint_And_Keep_Outer_Work()
    {
        await using NotesDbContext context = new(_options);
        EfCoreTransactionRunner<NotesDbContext> runner = new(context);

        await runner.ExecuteAsync(async token =>
        {
            await AddAsync(context, "outer", token);
            await runner.ExecuteAsync(innerToken => AddAsync(context, "inner", innerToken), _ => false, token);
            return true;
        }, _ => true);

        StoredTexts().Should().Equal("outer");
    }

    [Fact]
    public async Task Nested_Exception_Should_Roll_Back_To_Savepoint_When_Outer_Handles_It()
    {
        await using NotesDbContext context = new(_options);
        EfCoreTransactionRunner<NotesDbContext> runner = new(context);

        await runner.ExecuteAsync(async token =>
        {
            await AddAsync(context, "outer", token);
            try
            {
                await runner.ExecuteAsync<bool>(async innerToken =>
                {
                    await AddAsync(context, "inner", innerToken);
                    throw new InvalidOperationException("inner failed");
                }, _ => true, token);
            }
            catch (InvalidOperationException)
            {
                // The outer request handles the inner failure and carries on.
            }
            return true;
        }, _ => true);

        StoredTexts().Should().Equal("outer");
    }

    [Fact]
    public async Task Retry_Should_Rerun_Whole_Unit_Including_Nested_Work()
    {
        await using NotesDbContext context = new(_retryingOptions);
        EfCoreTransactionRunner<NotesDbContext> runner = new(context);
        int outerRuns = 0;
        int innerRuns = 0;

        await runner.ExecuteAsync(async token =>
        {
            outerRuns++;
            await AddAsync(context, "outer", token);
            return await runner.ExecuteAsync(async innerToken =>
            {
                innerRuns++;
                await AddAsync(context, "inner", innerToken);
                if (innerRuns == 1)
                    throw new TransientTestException();
                return true;
            }, _ => true, token);
        }, _ => true);

        outerRuns.Should().Be(2);
        innerRuns.Should().Be(2);
        StoredTexts().Should().Equal("outer", "inner");
    }

    [Fact]
    public async Task ExecuteAsync_Should_Throw_When_Retrying_Strategy_Would_Drop_Pending_Changes()
    {
        await using NotesDbContext context = new(_retryingOptions);
        context.Notes.Add(new TransactionNote { Text = "pending" });
        EfCoreTransactionRunner<NotesDbContext> runner = new(context);

        Func<Task> act = () => runner.ExecuteAsync(token => AddAsync(context, "work", token), _ => true).AsTask();

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*pending changes*");
        StoredTexts().Should().BeEmpty();
    }

    [Fact]
    public async Task Nested_Failure_Should_Roll_Back_To_Savepoint_When_Token_Is_Cancelled_After_Work()
    {
        await using NotesDbContext context = new(_options);
        EfCoreTransactionRunner<NotesDbContext> runner = new(context);
        using CancellationTokenSource innerCts = new();

        await runner.ExecuteAsync(async token =>
        {
            await AddAsync(context, "outer", token);
            await runner.ExecuteAsync(async innerToken =>
            {
                await AddAsync(context, "inner", innerToken);
                await innerCts.CancelAsync();
                return false;
            }, committed => committed, innerCts.Token);
            return true;
        }, _ => true);

        StoredTexts().Should().Equal("outer");
    }

    [Fact]
    public async Task Nested_Success_Should_Not_Throw_When_Token_Is_Cancelled_After_Work()
    {
        await using NotesDbContext context = new(_options);
        EfCoreTransactionRunner<NotesDbContext> runner = new(context);
        using CancellationTokenSource innerCts = new();

        await runner.ExecuteAsync(async token =>
        {
            await runner.ExecuteAsync(async innerToken =>
            {
                await AddAsync(context, "inner", innerToken);
                await innerCts.CancelAsync();
                return true;
            }, committed => committed, innerCts.Token);
            return true;
        }, _ => true);

        StoredTexts().Should().Equal("inner");
    }

    [Fact]
    public async Task Runner_Resolved_From_DI_Should_Use_Scoped_DbContext()
    {
        var services = new ServiceCollection();
        services.AddDbContext<NotesDbContext>(options => options.UseSqlite(_connection));
        services.AddEfCoreTransactionRunner<NotesDbContext>();
        await using ServiceProvider provider = services.BuildServiceProvider(validateScopes: true);
        using IServiceScope scope = provider.CreateScope();
        NotesDbContext context = scope.ServiceProvider.GetRequiredService<NotesDbContext>();
        ITransactionRunner runner = scope.ServiceProvider.GetRequiredService<ITransactionRunner>();

        await runner.ExecuteAsync(token => AddAsync(context, "from di", token), _ => true);

        StoredTexts().Should().Equal("from di");
    }

    [Fact]
    public void AddEfCoreTransactionRunner_Should_Register_Scoped_Runner_Once()
    {
        var services = new ServiceCollection();

        services.AddEfCoreTransactionRunner<NotesDbContext>();
        services.AddEfCoreTransactionRunner<NotesDbContext>();

        ServiceDescriptor descriptor = services.Should().ContainSingle(sd => sd.ServiceType == typeof(ITransactionRunner)).Subject;
        descriptor.ImplementationType.Should().Be<EfCoreTransactionRunner<NotesDbContext>>();
        descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }
}
