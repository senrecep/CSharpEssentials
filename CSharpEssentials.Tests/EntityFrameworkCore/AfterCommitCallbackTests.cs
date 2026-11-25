using CSharpEssentials.Entity;
using CSharpEssentials.Entity.Interfaces;
using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.EntityFrameworkCore.Interceptors;
using CSharpEssentials.EntityFrameworkCore.Transactions;
using CSharpEssentials.Transactions;

using System.Transactions;

using FluentAssertions;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

public sealed class AfterCommitCallbackTests : IDisposable
{
    private sealed class RetryOnTransientStrategy(ExecutionStrategyDependencies dependencies)
        : ExecutionStrategy(dependencies, maxRetryCount: 3, maxRetryDelay: TimeSpan.Zero)
    {
        protected override bool ShouldRetryOn(Exception exception) => exception is TransientTestException;

        protected override TimeSpan? GetNextDelay(Exception lastException) =>
            base.GetNextDelay(lastException) is null ? null : TimeSpan.Zero;
    }

    private sealed class CancelOnCommitInterceptor(CancellationTokenSource source) : DbTransactionInterceptor
    {
        public override async Task TransactionCommittedAsync(
            System.Data.Common.DbTransaction transaction, TransactionEndEventData eventData, CancellationToken cancellationToken = default) =>
            await source.CancelAsync();
    }

    private readonly SqliteConnection _connection;
    private readonly SqliteConnection _eventConnection = new("DataSource=:memory:");
    private readonly DbContextOptions<NotesDbContext> _options;
    private readonly List<string> _log = [];

    public AfterCommitCallbackTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<NotesDbContext>().UseSqlite(_connection).Options;

        using NotesDbContext context = new(_options);
        context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _connection.Dispose();
        _eventConnection.Dispose();
    }

    private Func<CancellationToken, ValueTask> Record(string entry) => _ =>
    {
        _log.Add(entry);
        return ValueTask.CompletedTask;
    };

    private int StoredCount()
    {
        using NotesDbContext context = new(_options);
        return context.Notes.Count();
    }

    private static async Task SaveNoteAsync(NotesDbContext context, CancellationToken cancellationToken)
    {
        context.Notes.Add(new TransactionNote { Text = "note" });
        await context.SaveChangesAsync(cancellationToken);
    }

    [Fact]
    public async Task Callback_Runs_After_Commit_Not_Inside_The_Unit()
    {
        await using NotesDbContext context = new(_options);
        EfCoreTransactionRunner<NotesDbContext> runner = new(context);
        int storedWhenCallbackRan = -1;

        await runner.ExecuteAsync(
            async token =>
            {
                await SaveNoteAsync(context, token);
                await runner.OnCommittedAsync(_ =>
                {
                    storedWhenCallbackRan = StoredCount();
                    _log.Add("committed");
                    return ValueTask.CompletedTask;
                }, token);
                _log.Should().BeEmpty();
                return true;
            },
            _ => true);

        _log.Should().Equal("committed");
        storedWhenCallbackRan.Should().Be(1);
    }

    [Fact]
    public async Task Callbacks_Run_With_An_Uncancelled_Token_When_The_Caller_Cancels_After_Commit()
    {
        using CancellationTokenSource source = new();
        DbContextOptions<NotesDbContext> options = new DbContextOptionsBuilder<NotesDbContext>()
            .UseSqlite(_connection)
            .AddInterceptors(new CancelOnCommitInterceptor(source))
            .Options;
        await using NotesDbContext context = new(options);
        EfCoreTransactionRunner<NotesDbContext> runner = new(context);
        bool? callbackSawCancellation = null;

        await runner.ExecuteAsync(
            async token =>
            {
                await SaveNoteAsync(context, token);
                await runner.OnCommittedAsync(token =>
                {
                    callbackSawCancellation = token.IsCancellationRequested;
                    return ValueTask.CompletedTask;
                }, token);
                return true;
            },
            _ => true,
            source.Token);

        source.IsCancellationRequested.Should().BeTrue();
        callbackSawCancellation.Should().BeFalse();
        StoredCount().Should().Be(1);
    }

    [Fact]
    public async Task Callbacks_Are_Dropped_When_The_Unit_Does_Not_Commit()
    {
        await using NotesDbContext context = new(_options);
        EfCoreTransactionRunner<NotesDbContext> runner = new(context);

        await runner.ExecuteAsync(
            async token =>
            {
                await runner.OnCommittedAsync(Record("dropped"), token);
                return false;
            },
            committed => committed);

        _log.Should().BeEmpty();
        await runner.OnCommittedAsync(Record("later"));
        _log.Should().Equal("later");
    }

    [Fact]
    public async Task Callbacks_Are_Dropped_When_The_Work_Throws()
    {
        await using NotesDbContext context = new(_options);
        EfCoreTransactionRunner<NotesDbContext> runner = new(context);

        Func<Task> act = async () => await runner.ExecuteAsync<bool>(
            async token =>
            {
                await runner.OnCommittedAsync(Record("dropped"), token);
                throw new InvalidOperationException("boom");
            },
            _ => true);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
        _log.Should().BeEmpty();
    }

    [Fact]
    public async Task Retry_Drops_Callbacks_Of_The_Failed_Attempt()
    {
        DbContextOptions<NotesDbContext> retrying = new DbContextOptionsBuilder<NotesDbContext>()
            .UseSqlite(_connection, sqlite => sqlite.ExecutionStrategy(dependencies => new RetryOnTransientStrategy(dependencies)))
            .Options;
        await using NotesDbContext context = new(retrying);
        EfCoreTransactionRunner<NotesDbContext> runner = new(context);
        int attempt = 0;

        await runner.ExecuteAsync(
            async token =>
            {
                attempt++;
                await runner.OnCommittedAsync(Record($"attempt {attempt}"), token);
                if (attempt == 1)
                    throw new TransientTestException();
                return true;
            },
            _ => true);

        _log.Should().Equal("attempt 2");
    }

    [Fact]
    public async Task Nested_Rollback_Drops_Only_Nested_Callbacks()
    {
        await using NotesDbContext context = new(_options);
        EfCoreTransactionRunner<NotesDbContext> runner = new(context);

        await runner.ExecuteAsync(
            async token =>
            {
                await runner.OnCommittedAsync(Record("outer"), token);
                await runner.ExecuteAsync(
                    async innerToken =>
                    {
                        await runner.OnCommittedAsync(Record("nested rolled back"), innerToken);
                        return false;
                    },
                    committed => committed,
                    token);
                await runner.ExecuteAsync(
                    async innerToken =>
                    {
                        await runner.OnCommittedAsync(Record("nested committed"), innerToken);
                        return true;
                    },
                    committed => committed,
                    token);
                return true;
            },
            _ => true);

        _log.Should().Equal("outer", "nested committed");
    }

    [Fact]
    public async Task Nested_Exception_Drops_Nested_Callbacks()
    {
        await using NotesDbContext context = new(_options);
        EfCoreTransactionRunner<NotesDbContext> runner = new(context);

        await runner.ExecuteAsync(
            async token =>
            {
                await runner.OnCommittedAsync(Record("outer"), token);
                try
                {
                    await runner.ExecuteAsync<bool>(
                        async innerToken =>
                        {
                            await runner.OnCommittedAsync(Record("nested"), innerToken);
                            throw new InvalidOperationException("nested");
                        },
                        _ => true,
                        token);
                }
                catch (InvalidOperationException)
                {
                    _log.Add("handled");
                }
                return true;
            },
            _ => true);

        _log.Should().Equal("handled", "outer");
    }

    [Fact]
    public async Task Without_A_Transaction_The_Callback_Runs_Now()
    {
        await using NotesDbContext context = new(_options);
        EfCoreTransactionRunner<NotesDbContext> runner = new(context);

        await runner.OnCommittedAsync(Record("now"));

        _log.Should().Equal("now");
    }

    [Fact]
    public async Task Foreign_Transaction_Throws()
    {
        await using NotesDbContext context = new(_options);
        EfCoreTransactionRunner<NotesDbContext> runner = new(context);
        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync();

        Func<Task> act = async () => await runner.OnCommittedAsync(Record("never"));

        await act.Should().ThrowAsync<InvalidOperationException>();
        _log.Should().BeEmpty();
    }

    [Fact]
    public async Task Ambient_TransactionScope_Throws()
    {
        await using NotesDbContext context = new(_options);
        EfCoreTransactionRunner<NotesDbContext> runner = new(context);
        using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

        Func<Task> act = async () => await runner.OnCommittedAsync(Record("never"));

        await act.Should().ThrowAsync<InvalidOperationException>();
        _log.Should().BeEmpty();
    }

    [Fact]
    public async Task Failing_Callbacks_Do_Not_Stop_The_Others_And_Surface_After_Commit()
    {
        await using NotesDbContext context = new(_options);
        EfCoreTransactionRunner<NotesDbContext> runner = new(context);

        Func<Task> act = async () => await runner.ExecuteAsync(
            async token =>
            {
                await SaveNoteAsync(context, token);
                await runner.OnCommittedAsync(_ => throw new InvalidOperationException("first"), token);
                await runner.OnCommittedAsync(Record("second"), token);
                return true;
            },
            _ => true);

        (await act.Should().ThrowAsync<AggregateException>())
            .Which.InnerExceptions.Should().ContainSingle().Which.Message.Should().Be("first");
        _log.Should().Equal("second");
        StoredCount().Should().Be(1);
    }

    [Fact]
    public async Task Callback_Registered_By_A_Callback_Runs_Now()
    {
        await using NotesDbContext context = new(_options);
        EfCoreTransactionRunner<NotesDbContext> runner = new(context);

        await runner.ExecuteAsync(
            async token =>
            {
                await runner.OnCommittedAsync(async callbackToken =>
                {
                    _log.Add("first");
                    await runner.OnCommittedAsync(Record("chained"), callbackToken);
                }, token);
                return true;
            },
            _ => true);

        _log.Should().Equal("first", "chained");
    }

    [Fact]
    public void Registration_Resolves_Runner_And_Callbacks()
    {
        ServiceCollection services = new();
        services.AddScoped(_ => new NotesDbContext(_options));
        services.AddEfCoreTransactionRunner<NotesDbContext>();

        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredService<ITransactionRunner>().Should().BeOfType<EfCoreTransactionRunner<NotesDbContext>>();
        scope.ServiceProvider.GetRequiredService<ITransactionCallbacks>().Should().BeOfType<EfCoreTransactionRunner<NotesDbContext>>();
    }

    private sealed class EventNote : EntityBase<Guid>
    {
        public EventNote() => Id = Guid.NewGuid();
    }

    private sealed class NoteCreated : IDomainEvent;

    private sealed class RecordingPublisher(List<string> log) : IDomainEventPublisher
    {
        public Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            log.Add("published");
            return Task.CompletedTask;
        }
    }

    private sealed class DeferringDbContext(
        DbContextOptions<DeferringDbContext> options, IServiceScopeFactory scopeFactory, bool defer)
        : BaseDbContext<DeferringDbContext>(options, scopeFactory)
    {
        public DbSet<EventNote> Notes => Set<EventNote>();

        protected override bool DispatchDomainEventsOnSaveChanges => true;

        protected override bool DeferDomainEventPublishingUntilCommit => defer;
    }

    private DeferringDbContext CreateDeferringContext(ServiceProvider provider, bool defer)
    {
        DbContextOptions<DeferringDbContext> options = new DbContextOptionsBuilder<DeferringDbContext>().UseSqlite(_eventConnection).Options;
        _eventConnection.Open();
        DeferringDbContext context = new(options, provider.GetRequiredService<IServiceScopeFactory>(), defer);
        context.Database.EnsureCreated();
        return context;
    }

    private sealed class RecordingOutbox(List<string> log) : IDomainEventOutbox
    {
        public Task StoreAsync(IReadOnlyList<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
        {
            log.Add("stored");
            return Task.CompletedTask;
        }
    }

    private ServiceProvider BuildEventServices(bool withOutbox = false)
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddSingleton<IDomainEventPublisher>(new RecordingPublisher(_log));
        if (withOutbox)
            services.AddSingleton<IDomainEventOutbox>(new RecordingOutbox(_log));
        return services.BuildServiceProvider();
    }

    private static async Task<bool> SaveEventAsync(DeferringDbContext context, List<string> log, CancellationToken cancellationToken)
    {
        EventNote note = new();
        note.Raise(new NoteCreated());
        context.Notes.Add(note);
        await context.SaveChangesAsync(cancellationToken);
        log.Add("saved");
        return true;
    }

    [Theory]
    [InlineData(true, new[] { "saved", "published" })]
    [InlineData(false, new[] { "published", "saved" })]
    public async Task Domain_Events_Are_Published_After_Commit_Only_When_Deferred(bool defer, string[] expected)
    {
        await using ServiceProvider provider = BuildEventServices();
        await using DeferringDbContext context = CreateDeferringContext(provider, defer);
        EfCoreTransactionRunner<DeferringDbContext> runner = new(context);

        await runner.ExecuteAsync(token => new ValueTask<bool>(SaveEventAsync(context, _log, token)), _ => true);

        _log.Should().Equal(expected);
    }

    [Fact]
    public async Task Deferred_Domain_Events_Are_Dropped_On_Rollback()
    {
        await using ServiceProvider provider = BuildEventServices();
        await using DeferringDbContext context = CreateDeferringContext(provider, defer: true);
        EfCoreTransactionRunner<DeferringDbContext> runner = new(context);

        await runner.ExecuteAsync(token => new ValueTask<bool>(SaveEventAsync(context, _log, token)), _ => false);

        _log.Should().Equal("saved");
    }

    [Fact]
    public async Task Deferred_Domain_Events_Outside_A_Runner_Unit_Are_Published_After_The_Save()
    {
        await using ServiceProvider provider = BuildEventServices();
        await using DeferringDbContext context = CreateDeferringContext(provider, defer: true);

        await SaveEventAsync(context, _log, CancellationToken.None);

        _log.Should().Equal("published", "saved");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Outbox_Storage_Is_Never_Deferred(bool commit)
    {
        await using ServiceProvider provider = BuildEventServices(withOutbox: true);
        await using DeferringDbContext context = CreateDeferringContext(provider, defer: true);
        EfCoreTransactionRunner<DeferringDbContext> runner = new(context);

        await runner.ExecuteAsync(token => new ValueTask<bool>(SaveEventAsync(context, _log, token)), _ => commit);

        _log.Should().Equal("stored", "saved");
    }
}
