using CSharpEssentials.Entity.Interfaces;
using CSharpEssentials.EntityFrameworkCore.Outbox;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace CSharpEssentials.Tests.EntityFrameworkCore.Outbox;

public sealed class OutboxProcessorTests : IDisposable
{
    private readonly SqliteConnection _connection = OutboxTestTypes.OpenConnection();
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 5, 6, 7, 8, 9, TimeSpan.Zero));
    private readonly RecordingHandler _handler = new();
    private ServiceProvider? _provider;

    public void Dispose()
    {
        _provider?.Dispose();
        _connection.Dispose();
    }

    private sealed class RecordingHandler : IOutboxMessageHandler
    {
        public List<(OutboxMessage Message, IDomainEvent Event)> Handled { get; } = [];

        public Func<OutboxMessage, Task>? OnHandle { get; set; }

        public async Task HandleAsync(OutboxMessage message, IDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            Handled.Add((message, domainEvent));
            if (OnHandle is not null)
                await OnHandle(message);
        }
    }

    private IOutboxProcessor CreateProcessor(Action<OutboxProcessorOptions>? configure = null)
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddSingleton<TimeProvider>(_clock);
        services.AddDbContext<PlainOutboxDbContext>(options => options.UseSqlite(_connection));
        services.AddEfCoreOutbox(OutboxTestTypes.Register);
        services.AddSingleton<IOutboxMessageHandler>(_handler);
        services.AddOutboxProcessor<PlainOutboxDbContext>(options =>
        {
            options.RetryDelay = _ => TimeSpan.FromMinutes(1);
            configure?.Invoke(options);
        });
        _provider = services.BuildServiceProvider();
        using (IServiceScope scope = _provider.CreateScope())
            scope.ServiceProvider.GetRequiredService<PlainOutboxDbContext>().Database.EnsureCreated();
        return _provider.GetRequiredService<IOutboxProcessor>();
    }

    private PlainOutboxDbContext NewContext() =>
        new(new DbContextOptionsBuilder<PlainOutboxDbContext>().UseSqlite(_connection).Options);

    private async Task<OutboxMessage> SeedAsync(IDomainEvent domainEvent, DateTime? occurredAt = null, string? type = null)
    {
        await using PlainOutboxDbContext context = NewContext();
        OutboxPayload payload = _provider!.GetRequiredService<IOutboxMessageSerializer>().Serialize(domainEvent);
        OutboxMessage message = new()
        {
            Id = Guid.NewGuid(),
            Type = type ?? payload.Type,
            Content = payload.Content,
            OccurredAt = occurredAt ?? _clock.GetUtcNow().UtcDateTime,
            NextAttemptAt = _clock.GetUtcNow().UtcDateTime,
        };
        context.OutboxMessages.Add(message);
        await context.SaveChangesAsync();
        return message;
    }

    private async Task<OutboxMessage> ReadAsync(Guid id)
    {
        await using PlainOutboxDbContext context = NewContext();
        return await context.OutboxMessages.AsNoTracking().SingleAsync(m => m.Id == id);
    }

    [Fact]
    public async Task ProcessBatchAsync_Should_DeliverInOccurrenceOrderAndMarkProcessed()
    {
        IOutboxProcessor processor = CreateProcessor();
        DateTime t0 = _clock.GetUtcNow().UtcDateTime;
        OutboxMessage later = await SeedAsync(new LegacyEvent("later"), t0.AddSeconds(-1));
        OutboxMessage first = await SeedAsync(new LegacyEvent("first"), t0.AddSeconds(-2));

        int claimed = await processor.ProcessBatchAsync();

        claimed.Should().Be(2);
        _handler.Handled.Select(h => h.Event).Should().Equal(new LegacyEvent("first"), new LegacyEvent("later"));
        _handler.Handled[0].Message.Attempts.Should().Be(1);
        OutboxMessage stored = await ReadAsync(first.Id);
        stored.ProcessedAt.Should().Be(t0);
        stored.ClaimToken.Should().BeNull();
        (await ReadAsync(later.Id)).ProcessedAt.Should().NotBeNull();
        (await processor.ProcessBatchAsync()).Should().Be(0);
    }

    [Fact]
    public async Task ProcessBatchAsync_Should_ClaimAtMostBatchSize()
    {
        IOutboxProcessor processor = CreateProcessor(options => options.BatchSize = 2);
        for (int i = 0; i < 3; i++)
            await SeedAsync(new LegacyEvent($"e{i}"));

        (await processor.ProcessBatchAsync()).Should().Be(2);
        (await processor.ProcessBatchAsync()).Should().Be(1);
        _handler.Handled.Should().HaveCount(3);
    }

    [Fact]
    public async Task ProcessBatchAsync_Should_ScheduleRetryWithError_When_HandlerThrows()
    {
        IOutboxProcessor processor = CreateProcessor();
        OutboxMessage seeded = await SeedAsync(new LegacyEvent("x"));
        _handler.OnHandle = message => message.Attempts == 1
            ? throw new InvalidOperationException("broker down")
            : Task.CompletedTask;

        await processor.ProcessBatchAsync();

        OutboxMessage failed = await ReadAsync(seeded.Id);
        failed.Attempts.Should().Be(1);
        failed.Error.Should().Be("System.InvalidOperationException: broker down");
        failed.NextAttemptAt.Should().Be(_clock.GetUtcNow().UtcDateTime.AddMinutes(1));
        failed.ClaimToken.Should().BeNull();
        failed.ProcessedAt.Should().BeNull();
        (await processor.ProcessBatchAsync()).Should().Be(0);

        _clock.Advance(TimeSpan.FromMinutes(1));
        await processor.ProcessBatchAsync();

        OutboxMessage retried = await ReadAsync(seeded.Id);
        retried.Attempts.Should().Be(2);
        retried.ProcessedAt.Should().NotBeNull();
        retried.Error.Should().BeNull();
    }

    [Fact]
    public async Task ProcessBatchAsync_Should_DeadLetter_When_LastAttemptFails()
    {
        IOutboxProcessor processor = CreateProcessor(options => options.MaxAttempts = 2);
        OutboxMessage seeded = await SeedAsync(new LegacyEvent("x"));
        _handler.OnHandle = _ => throw new InvalidOperationException("poison");

        await processor.ProcessBatchAsync();
        _clock.Advance(TimeSpan.FromMinutes(1));
        await processor.ProcessBatchAsync();
        _clock.Advance(TimeSpan.FromHours(1));
        int afterDeadLetter = await processor.ProcessBatchAsync();

        OutboxMessage dead = await ReadAsync(seeded.Id);
        dead.Attempts.Should().Be(2);
        dead.DeadLetteredAt.Should().NotBeNull();
        dead.Error.Should().Contain("poison");
        afterDeadLetter.Should().Be(0);
        _handler.Handled.Should().HaveCount(2);
    }

    [Fact]
    public async Task ProcessBatchAsync_Should_RecordFailure_When_TypeIsNotRegistered()
    {
        IOutboxProcessor processor = CreateProcessor(options => options.MaxAttempts = 1);
        OutboxMessage seeded = await SeedAsync(new LegacyEvent("x"), type: "renamed.event");

        await processor.ProcessBatchAsync();

        OutboxMessage dead = await ReadAsync(seeded.Id);
        dead.DeadLetteredAt.Should().NotBeNull();
        dead.Error.Should().Contain("'renamed.event'");
        _handler.Handled.Should().BeEmpty();
    }

    [Fact]
    public async Task ProcessBatchAsync_Should_ReclaimAfterLease_When_EarlierClaimWasAbandoned()
    {
        IOutboxProcessor processor = CreateProcessor(options => options.LeaseDuration = TimeSpan.FromMinutes(2));
        OutboxMessage seeded = await SeedAsync(new LegacyEvent("x"));
        DateTime now = _clock.GetUtcNow().UtcDateTime;
        await using (PlainOutboxDbContext context = NewContext())
            await ConditionalUpdateOutboxMessageClaimer.Instance.ClaimAsync(context, new OutboxClaimRequest(Guid.NewGuid(), 10, now, now.AddMinutes(2)), default);

        (await processor.ProcessBatchAsync()).Should().Be(0);
        _clock.Advance(TimeSpan.FromMinutes(2));
        (await processor.ProcessBatchAsync()).Should().Be(1);

        OutboxMessage done = await ReadAsync(seeded.Id);
        done.Attempts.Should().Be(2);
        done.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ProcessBatchAsync_Should_DeadLetterWithoutHandling_When_LeaseOfLastAttemptEnded()
    {
        IOutboxProcessor processor = CreateProcessor(options => options.MaxAttempts = 1);
        OutboxMessage seeded = await SeedAsync(new LegacyEvent("x"));
        DateTime now = _clock.GetUtcNow().UtcDateTime;
        await using (PlainOutboxDbContext context = NewContext())
            await ConditionalUpdateOutboxMessageClaimer.Instance.ClaimAsync(context, new OutboxClaimRequest(Guid.NewGuid(), 10, now, now), default);

        await processor.ProcessBatchAsync();

        OutboxMessage dead = await ReadAsync(seeded.Id);
        dead.DeadLetteredAt.Should().NotBeNull();
        dead.Error.Should().Contain("lease");
        _handler.Handled.Should().BeEmpty();
    }

    [Fact]
    public async Task ProcessBatchAsync_Should_NotOverwriteNewerClaim_When_LeaseEndedDuringHandling()
    {
        IOutboxProcessor processor = CreateProcessor(options => options.LeaseDuration = TimeSpan.FromMinutes(1));
        OutboxMessage seeded = await SeedAsync(new LegacyEvent("x"));
        Guid thief = Guid.NewGuid();
        _handler.OnHandle = async _ =>
        {
            _clock.Advance(TimeSpan.FromMinutes(1));
            DateTime now = _clock.GetUtcNow().UtcDateTime;
            await using PlainOutboxDbContext context = NewContext();
            await ConditionalUpdateOutboxMessageClaimer.Instance.ClaimAsync(context, new OutboxClaimRequest(thief, 10, now, now.AddMinutes(1)), default);
        };

        await processor.ProcessBatchAsync();

        OutboxMessage stored = await ReadAsync(seeded.Id);
        stored.ClaimToken.Should().Be(thief);
        stored.ProcessedAt.Should().BeNull();
    }

    [Fact]
    public async Task ProcessBatchAsync_Should_Throw_When_Cancelled()
    {
        IOutboxProcessor processor = CreateProcessor();
        await SeedAsync(new LegacyEvent("x"));
        using CancellationTokenSource cts = new();
        _handler.OnHandle = async _ =>
        {
            await cts.CancelAsync();
            cts.Token.ThrowIfCancellationRequested();
        };

        Func<Task> act = () => processor.ProcessBatchAsync(cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task SqlClaimer_Should_ClaimDueRowsInOrder_When_UsingSqliteDialect()
    {
        IOutboxProcessor processor = CreateProcessor(options => options.Claimer = new SqlOutboxMessageClaimer(OutboxClaimDialect.Sqlite));
        DateTime t0 = _clock.GetUtcNow().UtcDateTime;
        await SeedAsync(new LegacyEvent("b"), t0.AddSeconds(-1));
        await SeedAsync(new LegacyEvent("a"), t0.AddSeconds(-2));
        OutboxMessage notDue = await SeedAsync(new LegacyEvent("later"));
        await using (PlainOutboxDbContext context = NewContext())
            await context.OutboxMessages.Where(m => m.Id == notDue.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(m => m.NextAttemptAt, t0.AddHours(1)));

        int claimed = await processor.ProcessBatchAsync();

        claimed.Should().Be(2);
        _handler.Handled.Select(h => h.Event).Should().Equal(new LegacyEvent("a"), new LegacyEvent("b"));
        _handler.Handled.Should().OnlyContain(h => h.Message.Attempts == 1);
    }

    [Fact]
    public async Task SqlClaimer_Should_RunCustomStatement()
    {
        await using PlainOutboxDbContext context = NewContext();
        await context.Database.EnsureCreatedAsync();
        SqlOutboxMessageClaimer claimer = new("""SELECT * FROM "OutboxMessages" WHERE {0} IS NOT NULL AND {1} IS NOT NULL AND {2} IS NOT NULL LIMIT {3}""");

        IReadOnlyList<OutboxMessage> claimed = await claimer.ClaimAsync(
            context, new OutboxClaimRequest(Guid.NewGuid(), 5, DateTime.UtcNow, DateTime.UtcNow), default);

        claimed.Should().BeEmpty();
    }

    [Fact]
    public void AddOutboxProcessor_Should_Throw_When_OptionIsOutOfRange()
    {
        ServiceCollection services = new();

        Action act = () => services.AddOutboxProcessor<PlainOutboxDbContext>(options => options.BatchSize = 0);

        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("*BatchSize*");
    }

    [Fact]
    public void AddOutboxProcessor_Should_Throw_When_PollingIntervalIsZero()
    {
        ServiceCollection services = new();

        Action act = () => services.AddOutboxProcessor<PlainOutboxDbContext>(options => options.PollingInterval = TimeSpan.Zero);

        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("*PollingInterval*");
    }

    [Fact]
    public async Task ConditionalUpdateClaimer_Should_ReadBackOnlyCandidates_When_AnotherRowCarriesTheToken()
    {
        CreateProcessor();
        Guid token = Guid.NewGuid();
        OutboxMessage stale = await SeedAsync(new LegacyEvent("stale"));
        OutboxMessage due = await SeedAsync(new LegacyEvent("due"));
        DateTime now = _clock.GetUtcNow().UtcDateTime;
        await using PlainOutboxDbContext context = NewContext();
        await context.OutboxMessages.Where(m => m.Id == stale.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.ProcessedAt, now).SetProperty(m => m.ClaimToken, token));

        IReadOnlyList<OutboxMessage> claimed = await ConditionalUpdateOutboxMessageClaimer.Instance.ClaimAsync(
            context, new OutboxClaimRequest(token, 10, now, now.AddMinutes(1)), default);

        claimed.Select(m => m.Id).Should().Equal(due.Id);
    }

    [Fact]
    public async Task ProcessBatchAsync_Should_ReleaseUnhandledMessages_When_LeaseEndsMidBatch()
    {
        IOutboxProcessor processor = CreateProcessor(options =>
        {
            options.LeaseDuration = TimeSpan.FromMinutes(1);
            options.MaxAttempts = 1;
        });
        DateTime t0 = _clock.GetUtcNow().UtcDateTime;
        OutboxMessage first = await SeedAsync(new LegacyEvent("first"), t0.AddSeconds(-2));
        OutboxMessage second = await SeedAsync(new LegacyEvent("second"), t0.AddSeconds(-1));
        _handler.OnHandle = _ =>
        {
            _clock.Advance(TimeSpan.FromMinutes(1));
            return Task.CompletedTask;
        };

        int claimed = await processor.ProcessBatchAsync();

        claimed.Should().Be(2);
        _handler.Handled.Select(h => h.Message.Id).Should().Equal(first.Id);
        (await ReadAsync(first.Id)).ProcessedAt.Should().NotBeNull();
        OutboxMessage released = await ReadAsync(second.Id);
        released.Attempts.Should().Be(0);
        released.ClaimToken.Should().BeNull();
        released.NextAttemptAt.Should().Be(_clock.GetUtcNow().UtcDateTime);
        released.DeadLetteredAt.Should().BeNull();
    }

    [Fact]
    public async Task PurgeProcessedOutboxMessagesAsync_Should_DeleteOnlyRowsProcessedBeforeTheCutoff()
    {
        CreateProcessor();
        DateTime now = _clock.GetUtcNow().UtcDateTime;
        OutboxMessage old = await SeedAsync(new LegacyEvent("old"));
        OutboxMessage recent = await SeedAsync(new LegacyEvent("recent"));
        OutboxMessage pending = await SeedAsync(new LegacyEvent("pending"));
        OutboxMessage dead = await SeedAsync(new LegacyEvent("dead"));
        await using PlainOutboxDbContext context = NewContext();
        await context.OutboxMessages.Where(m => m.Id == old.Id).ExecuteUpdateAsync(s => s.SetProperty(m => m.ProcessedAt, now.AddDays(-8)));
        await context.OutboxMessages.Where(m => m.Id == recent.Id).ExecuteUpdateAsync(s => s.SetProperty(m => m.ProcessedAt, now.AddDays(-1)));
        await context.OutboxMessages.Where(m => m.Id == dead.Id).ExecuteUpdateAsync(s => s.SetProperty(m => m.DeadLetteredAt, now.AddDays(-30)));

        int deleted = await context.PurgeProcessedOutboxMessagesAsync(now.AddDays(-7));

        deleted.Should().Be(1);
        (await context.OutboxMessages.Select(m => m.Id).ToListAsync()).Should().BeEquivalentTo([recent.Id, pending.Id, dead.Id]);
    }
}

public sealed class OutboxClaimSqlTests
{
    private static PlainOutboxDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<PlainOutboxDbContext>().UseSqlite("DataSource=:memory:").Options);

    [Fact]
    public void Build_Should_WriteSkipLockedStatement_When_DialectIsPostgreSql()
    {
        using PlainOutboxDbContext context = CreateContext();

        string sql = OutboxClaimSql.Build(context, OutboxClaimDialect.PostgreSql);

        sql.Should().Be(
            """UPDATE "OutboxMessages" SET "ClaimToken" = {0}, "NextAttemptAt" = {1}, "Attempts" = "Attempts" + 1 WHERE "Id" IN (SELECT "Id" FROM "OutboxMessages" WHERE "ProcessedAt" IS NULL AND "DeadLetteredAt" IS NULL AND "NextAttemptAt" <= {2} ORDER BY "OccurredAt", "Id" LIMIT {3} FOR UPDATE SKIP LOCKED) RETURNING *""");
    }

    [Fact]
    public void Build_Should_WriteReadPastStatement_When_DialectIsSqlServer()
    {
        using PlainOutboxDbContext context = CreateContext();

        string sql = OutboxClaimSql.Build(context, OutboxClaimDialect.SqlServer);

        sql.Should().Be(
            """WITH claimed AS (SELECT TOP ({3}) * FROM "OutboxMessages" WITH (UPDLOCK, READPAST, ROWLOCK) WHERE "ProcessedAt" IS NULL AND "DeadLetteredAt" IS NULL AND "NextAttemptAt" <= {2} ORDER BY "OccurredAt", "Id") UPDATE claimed SET "ClaimToken" = {0}, "NextAttemptAt" = {1}, "Attempts" = "Attempts" + 1 OUTPUT inserted.*""");
    }

    [Fact]
    public void Build_Should_Throw_When_ContextDoesNotMapOutbox()
    {
        using UnmappedOutboxDbContext context = new(
            new DbContextOptionsBuilder<UnmappedOutboxDbContext>().UseSqlite("DataSource=:memory:").Options,
            new ServiceCollection().AddLogging().BuildServiceProvider().GetRequiredService<IServiceScopeFactory>());

        Action act = () => OutboxClaimSql.Build(context, OutboxClaimDialect.PostgreSql);

        act.Should().Throw<InvalidOperationException>();
    }
}

public sealed class OutboxRetryBackoffTests
{
    [Theory]
    [InlineData(1, 10)]
    [InlineData(2, 20)]
    [InlineData(3, 40)]
    [InlineData(5, 100)]
    [InlineData(200, 100)]
    public void Compute_Should_DoubleUpToTheCap_When_JitterIsOff(int attempt, int expectedSeconds)
    {
        TimeSpan delay = OutboxRetryBackoff.Compute(attempt, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(100), useJitter: false);

        delay.Should().Be(TimeSpan.FromSeconds(expectedSeconds));
    }

    [Fact]
    public void Compute_Should_StayInUpperHalf_When_JitterIsOn()
    {
        for (int i = 0; i < 100; i++)
        {
            TimeSpan delay = OutboxRetryBackoff.Compute(3, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(100), useJitter: true);

            delay.Should().BeGreaterThanOrEqualTo(TimeSpan.FromSeconds(20)).And.BeLessThanOrEqualTo(TimeSpan.FromSeconds(40));
        }
    }

    [Fact]
    public void Compute_Should_Throw_When_ArgumentsAreInvalid()
    {
        Action zeroAttempt = () => OutboxRetryBackoff.Compute(0, TimeSpan.Zero, TimeSpan.Zero, false);
        Action capBelowBase = () => OutboxRetryBackoff.Compute(1, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1), false);

        zeroAttempt.Should().Throw<ArgumentOutOfRangeException>();
        capBelowBase.Should().Throw<ArgumentOutOfRangeException>();
    }
}
