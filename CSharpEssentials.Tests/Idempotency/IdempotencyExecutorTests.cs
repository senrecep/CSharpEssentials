using System.Text;
using CSharpEssentials.Idempotency;
using CSharpEssentials.Tests.AspNetCore.Idempotency;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;

namespace CSharpEssentials.Tests.Idempotency;

public sealed class IdempotencyExecutorTests : IDisposable
{
    private static readonly IdempotencyRecordKey Key = new("orders", IdempotencyScope.User("alice"), "k1");

    private static readonly IIdempotencyResultCodec<string> Codec = IdempotencyResultCodec.Create<string>(
        static value => Encoding.UTF8.GetBytes(value),
        static payload => Encoding.UTF8.GetString(payload.Span),
        static value => value switch
        {
            "failure" => IdempotencyResultHandling.StoreAsFailure,
            "rolled-back" => IdempotencyResultHandling.Release,
            _ => IdempotencyResultHandling.Store,
        });

    private readonly FakeTimeProvider _time = new();
    private readonly RecordingStore _store;
    private readonly CapturingLoggerProvider _logs = new();
    private readonly LoggerFactory _loggerFactory;
    private readonly IdempotencyExecutorOptions _options = new();
    private int _runs;

    public IdempotencyExecutorTests()
    {
        _store = new RecordingStore(new InMemoryIdempotencyRecordStore(_time));
        _loggerFactory = new LoggerFactory([_logs]);
    }

    public void Dispose()
    {
        _loggerFactory.Dispose();
        _logs.Dispose();
    }

    [Fact]
    public async Task ExecuteAsync_Should_Store_The_Result_And_Replay_It()
    {
        IdempotencyExecution<string> first = await RunAsync("done");
        IdempotencyExecution<string> second = await RunAsync("other");

        first.Should().Be(new IdempotencyExecution<string>(IdempotencyExecutionStatus.Executed, "done"));
        second.Should().Be(new IdempotencyExecution<string>(IdempotencyExecutionStatus.Replayed, "done"));
        second.HasValue.Should().BeTrue();
        _runs.Should().Be(1);
        _store.Retentions.Should().Equal(_options.Retention);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Store_A_Failure_For_The_Failure_Retention()
    {
        await RunAsync("failure");
        IdempotencyExecution<string> replayed = await RunAsync("failure");
        _time.Advance(_options.FailureRetention + TimeSpan.FromSeconds(1));
        IdempotencyExecution<string> afterRetention = await RunAsync("failure");

        replayed.Status.Should().Be(IdempotencyExecutionStatus.Replayed);
        afterRetention.Status.Should().Be(IdempotencyExecutionStatus.Executed);
        _runs.Should().Be(2);
        _store.Retentions.Should().Equal(_options.FailureRetention, _options.FailureRetention);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Release_The_Key_When_The_Codec_Says_Release()
    {
        IdempotencyExecution<string> first = await RunAsync("rolled-back");
        IdempotencyExecution<string> second = await RunAsync("rolled-back");

        first.Status.Should().Be(IdempotencyExecutionStatus.Executed);
        second.Status.Should().Be(IdempotencyExecutionStatus.Executed);
        _runs.Should().Be(2);
        _store.Releases.Should().Be(2);
        _store.Retentions.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_Should_Keep_The_Key_In_Flight_Until_The_Lease_Expires_When_The_Operation_Throws()
    {
        Func<Task> failing = () => CreateExecutor().ExecuteAsync<string>(Key, "fp", null, _ => throw new InvalidOperationException("boom"), Codec).AsTask();

        await failing.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
        IdempotencyExecution<string> duringLease = await RunAsync("done");
        _time.Advance(_options.Lease + TimeSpan.FromSeconds(1));
        IdempotencyExecution<string> afterLease = await RunAsync("done");

        duringLease.Should().Be(new IdempotencyExecution<string>(IdempotencyExecutionStatus.InFlight, null));
        duringLease.HasValue.Should().BeFalse();
        afterLease.Status.Should().Be(IdempotencyExecutionStatus.Executed);
        _runs.Should().Be(1);
        _store.Releases.Should().Be(0);
        _logs.Named("Idempotency.KeptInFlight").Should().ContainSingle();
    }

    [Fact]
    public async Task ExecuteAsync_Should_Keep_The_Key_In_Flight_When_The_Operation_Is_Cancelled_After_It_Started()
    {
        using CancellationTokenSource cancellation = new();
        Func<Task> cancelled = () => CreateExecutor().ExecuteAsync<string>(
            Key,
            "fp",
            null,
            async token =>
            {
                await cancellation.CancelAsync();
                token.ThrowIfCancellationRequested();
                return "never";
            },
            Codec,
            cancellation.Token).AsTask();

        await cancelled.Should().ThrowAsync<OperationCanceledException>();
        IdempotencyExecution<string> retry = await RunAsync("done");

        retry.Status.Should().Be(IdempotencyExecutionStatus.InFlight);
        _store.Releases.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Store_The_Result_Of_An_Operation_That_Completed_Despite_Cancellation()
    {
        using CancellationTokenSource cancellation = new();
        IdempotencyExecution<string> execution = await CreateExecutor().ExecuteAsync(
            Key,
            "fp",
            null,
            async _ =>
            {
                await cancellation.CancelAsync();
                return "done";
            },
            Codec,
            cancellation.Token);
        IdempotencyExecution<string> retry = await RunAsync("other");

        execution.Status.Should().Be(IdempotencyExecutionStatus.Executed);
        retry.Should().Be(new IdempotencyExecution<string>(IdempotencyExecutionStatus.Replayed, "done"));
    }

    [Fact]
    public async Task ExecuteAsync_Should_Release_The_Key_When_Cancelled_Before_The_Operation_Starts()
    {
        using CancellationTokenSource cancellation = new();
        _store.AfterReserve = cancellation.Cancel;
        Func<Task> cancelled = () => CreateExecutor().ExecuteAsync(Key, "fp", null, Operation("never"), Codec, cancellation.Token).AsTask();

        await cancelled.Should().ThrowAsync<OperationCanceledException>();
        _store.AfterReserve = null;
        IdempotencyExecution<string> retry = await RunAsync("done");

        _store.Releases.Should().Be(1);
        retry.Status.Should().Be(IdempotencyExecutionStatus.Executed);
        _runs.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Not_Touch_The_Store_When_Cancelled_Before_The_Call()
    {
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();

        Func<Task> cancelled = () => CreateExecutor().ExecuteAsync(Key, "fp", null, Operation("never"), Codec, cancellation.Token).AsTask();

        await cancelled.Should().ThrowAsync<OperationCanceledException>();
        _runs.Should().Be(0);
        _store.Releases.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Return_KeyReused_For_Another_Fingerprint_Without_Running()
    {
        await RunAsync("done");

        IdempotencyExecution<string> reused = await CreateExecutor().ExecuteAsync(Key, "other-fp", null, Operation("other"), Codec);

        reused.Should().Be(new IdempotencyExecution<string>(IdempotencyExecutionStatus.KeyReused, null));
        _runs.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Run_A_Concurrent_Duplicate_Once()
    {
        TaskCompletionSource gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<IdempotencyExecution<string>> first = CreateExecutor().ExecuteAsync(
            Key,
            "fp",
            null,
            async _ =>
            {
                Interlocked.Increment(ref _runs);
                await gate.Task;
                return "done";
            },
            Codec).AsTask();

        IdempotencyExecution<string>[] duplicates = await Task.WhenAll(
            Enumerable.Range(0, 8).Select(_ => CreateExecutor().ExecuteAsync(Key, "fp", null, Operation("duplicate"), Codec).AsTask()));
        gate.SetResult();
        IdempotencyExecution<string> completed = await first;
        IdempotencyExecution<string> afterwards = await CreateExecutor().ExecuteAsync(Key, "fp", null, Operation("late"), Codec);

        duplicates.Should().OnlyContain(execution => execution.Status == IdempotencyExecutionStatus.InFlight);
        completed.Should().Be(new IdempotencyExecution<string>(IdempotencyExecutionStatus.Executed, "done"));
        afterwards.Should().Be(new IdempotencyExecution<string>(IdempotencyExecutionStatus.Replayed, "done"));
        _runs.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Return_The_Result_And_Keep_The_Key_In_Flight_When_Storing_Fails()
    {
        _store.FailComplete = true;

        IdempotencyExecution<string> execution = await RunAsync("done");
        _store.FailComplete = false;
        IdempotencyExecution<string> retry = await RunAsync("done");

        execution.Should().Be(new IdempotencyExecution<string>(IdempotencyExecutionStatus.Executed, "done"));
        retry.Status.Should().Be(IdempotencyExecutionStatus.InFlight);
        _logs.Named("Idempotency.StoreFailed").Should().ContainSingle();
    }

    [Fact]
    public async Task ExecuteAsync_Should_Keep_The_Key_In_Flight_When_The_Codec_Cannot_Serialize()
    {
        IIdempotencyResultCodec<string> codec = IdempotencyResultCodec.Create<string>(
            static _ => throw new FormatException("not serializable"),
            static _ => "never");

        IdempotencyExecution<string> execution = await CreateExecutor().ExecuteAsync(Key, "fp", null, Operation("done"), codec);
        IdempotencyExecution<string> retry = await RunAsync("done");

        execution.Status.Should().Be(IdempotencyExecutionStatus.Executed);
        retry.Status.Should().Be(IdempotencyExecutionStatus.InFlight);
        _logs.Named("Idempotency.StoreFailed").Should().ContainSingle();
    }

    [Fact]
    public async Task ExecuteAsync_Should_Log_A_Stale_Reservation_When_The_Store_Rejects_The_Token()
    {
        _store.CompleteResult = false;

        IdempotencyExecution<string> execution = await RunAsync("done");

        execution.Status.Should().Be(IdempotencyExecutionStatus.Executed);
        _logs.Named("Idempotency.StaleReservation").Should().ContainSingle();
    }

    [Fact]
    public async Task ExecuteAsync_Should_Throw_Without_Running_When_The_Stored_Result_Cannot_Be_Read()
    {
        await RunAsync("done");
        IIdempotencyResultCodec<string> codec = IdempotencyResultCodec.Create<string>(
            static value => Encoding.UTF8.GetBytes(value),
            static _ => throw new FormatException("unreadable"));

        Func<Task> replay = () => CreateExecutor().ExecuteAsync(Key, "fp", null, Operation("other"), codec).AsTask();

        (await replay.Should().ThrowAsync<InvalidOperationException>().WithMessage("*'orders'*cannot be read*"))
            .WithInnerException<FormatException>();
        _runs.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Propagate_A_Reservation_Failure_In_Enforce_Mode()
    {
        _store.FailReserve = true;

        Func<Task> act = () => RunAsync("done");

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("store down");
        _runs.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Not_Touch_The_Store_In_Off_Mode()
    {
        _options.Mode = IdempotencyMode.Off;

        await RunAsync("done");
        await RunAsync("done");

        _runs.Should().Be(2);
        _store.Calls.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Store_In_Observe_Mode_So_Enforce_Replays_It()
    {
        await RunAsync("done", new IdempotencyExecutionOptions { Mode = IdempotencyMode.Observe });

        IdempotencyExecution<string> enforced = await RunAsync("other");

        enforced.Should().Be(new IdempotencyExecution<string>(IdempotencyExecutionStatus.Replayed, "done"));
        _runs.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Run_And_Log_In_Observe_Mode_When_Enforce_Would_Replay()
    {
        await RunAsync("done");

        IdempotencyExecution<string> observed = await RunAsync("again", new IdempotencyExecutionOptions { Mode = IdempotencyMode.Observe });
        IdempotencyExecution<string> enforced = await RunAsync("other");

        observed.Should().Be(new IdempotencyExecution<string>(IdempotencyExecutionStatus.Executed, "again"));
        enforced.Should().Be(new IdempotencyExecution<string>(IdempotencyExecutionStatus.Replayed, "done"));
        _runs.Should().Be(2);
        _logs.Named("Idempotency.Observe.WouldReplay").Should().ContainSingle();
    }

    [Fact]
    public async Task ExecuteAsync_Should_Run_And_Log_In_Observe_Mode_When_Enforce_Would_Reject_A_Reused_Key()
    {
        await RunAsync("done");

        IdempotencyExecution<string> observed = await CreateExecutor().ExecuteAsync(
            Key, "other-fp", new IdempotencyExecutionOptions { Mode = IdempotencyMode.Observe }, Operation("again"), Codec);

        observed.Status.Should().Be(IdempotencyExecutionStatus.Executed);
        _runs.Should().Be(2);
        _logs.Named("Idempotency.Observe.WouldRejectKeyReused").Should().ContainSingle();
    }

    [Fact]
    public async Task ExecuteAsync_Should_Run_And_Log_In_Observe_Mode_When_Enforce_Would_Reject_An_In_Flight_Key()
    {
        await _store.TryReserveAsync(Key, "fp", TimeSpan.FromMinutes(1));

        IdempotencyExecution<string> observed = await RunAsync("again", new IdempotencyExecutionOptions { Mode = IdempotencyMode.Observe });

        observed.Status.Should().Be(IdempotencyExecutionStatus.Executed);
        _runs.Should().Be(1);
        _logs.Named("Idempotency.Observe.WouldRejectInFlight").Should().ContainSingle();
    }

    [Fact]
    public async Task ExecuteAsync_Should_Run_In_Observe_Mode_When_The_Store_Fails()
    {
        _store.FailReserve = true;

        IdempotencyExecution<string> observed = await RunAsync("done", new IdempotencyExecutionOptions { Mode = IdempotencyMode.Observe });

        observed.Status.Should().Be(IdempotencyExecutionStatus.Executed);
        _logs.Named("Idempotency.Observe.ReserveFailed").Should().ContainSingle()
            .Which.EventId.Id.Should().Be(7015);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Reserve_Through_The_Detached_View_And_Store_Through_The_Transactional_Store_In_Observe_Mode()
    {
        RecordingStore detached = new(new InMemoryIdempotencyRecordStore(_time));
        IdempotencyExecutor executor = new(new TransactionalStore(_store, detached), new TestOptionsMonitor<IdempotencyExecutorOptions>(_options));

        await executor.ExecuteAsync(Key, "fp", new IdempotencyExecutionOptions { Mode = IdempotencyMode.Observe }, Operation("done"), Codec);

        detached.Calls.Should().Be(2);
        detached.Releases.Should().Be(1);
        _store.Calls.Should().Be(1);
        _store.Retentions.Should().ContainSingle();
    }

    [Fact]
    public async Task ExecuteAsync_Should_Release_Through_The_Detached_View_Only_In_Observe_Mode()
    {
        RecordingStore detached = new(new InMemoryIdempotencyRecordStore(_time));
        IdempotencyExecutor executor = new(new TransactionalStore(_store, detached), new TestOptionsMonitor<IdempotencyExecutorOptions>(_options));

        await executor.ExecuteAsync(Key, "fp", new IdempotencyExecutionOptions { Mode = IdempotencyMode.Observe }, Operation("rolled-back"), Codec);

        detached.Calls.Should().Be(2);
        detached.Releases.Should().Be(1);
        _store.Calls.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Use_A_Transactional_Store_Itself_In_Enforce_Mode()
    {
        RecordingStore detached = new(new InMemoryIdempotencyRecordStore(_time));
        IdempotencyExecutor executor = new(new TransactionalStore(_store, detached), new TestOptionsMonitor<IdempotencyExecutorOptions>(_options));

        await executor.ExecuteAsync(Key, "fp", new IdempotencyExecutionOptions { Mode = IdempotencyMode.Enforce }, Operation("done"), Codec);

        _store.Calls.Should().Be(2);
        detached.Calls.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Reserve_With_The_Lease_Of_The_Operation_Over_Its_Profile_Over_The_Defaults()
    {
        _options.AddProfile("long", profile => profile.Lease = TimeSpan.FromMinutes(5));

        await RunAsync("a");
        await RunAsync("b", new IdempotencyExecutionOptions { Profile = "long" }, "k2");
        await RunAsync("c", new IdempotencyExecutionOptions { Profile = "long", Lease = TimeSpan.FromMinutes(2) }, "k3");

        _store.Leases.Should().Equal(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(2));
    }

    [Fact]
    public async Task ExecuteAsync_Should_Store_With_The_Retention_Of_The_Operation_Over_Its_Profile_Over_The_Defaults()
    {
        _options.AddProfile("week", profile => profile.Retention = TimeSpan.FromDays(7));

        await RunAsync("a");
        await RunAsync("b", new IdempotencyExecutionOptions { Profile = "WEEK" }, "k2");
        await RunAsync("c", new IdempotencyExecutionOptions { Profile = "week", Retention = TimeSpan.FromDays(2) }, "k3");
        await RunAsync("d", new IdempotencyExecutionOptions { Profile = IdempotencyExecutorOptions.MessagingProfile }, "k4");

        _store.Retentions.Should().Equal(TimeSpan.FromHours(24), TimeSpan.FromDays(7), TimeSpan.FromDays(2), TimeSpan.FromDays(7));
    }

    [Fact]
    public async Task ExecuteAsync_Should_Throw_For_An_Unknown_Profile()
    {
        Func<Task> act = () => RunAsync("a", new IdempotencyExecutionOptions { Profile = "missing" });

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*'missing'*not registered*'messaging'*");
        _runs.Should().Be(0);
    }

    [Theory]
    [InlineData(0, 1, 1, "Retention")]
    [InlineData(60, 0, 1, "Lease")]
    [InlineData(60, 61, 1, "Lease")]
    [InlineData(60, 1, 0, "FailureRetention")]
    [InlineData(60, 1, 61, "FailureRetention")]
    public async Task ExecuteAsync_Should_Throw_For_Invalid_Settings(int retentionMinutes, int leaseMinutes, int failureMinutes, string setting)
    {
        IdempotencyExecutionOptions options = new()
        {
            Retention = TimeSpan.FromMinutes(retentionMinutes),
            Lease = TimeSpan.FromMinutes(leaseMinutes),
            FailureRetention = TimeSpan.FromMinutes(failureMinutes),
        };

        Func<Task> act = () => RunAsync("a", options);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage($"*{setting} (*");
        _runs.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Throw_For_A_Lease_Above_MaxLease()
    {
        Func<Task> act = () => RunAsync("a", new IdempotencyExecutionOptions { Lease = TimeSpan.FromMinutes(16) });

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*MaxLease*");
    }

    [Fact]
    public async Task ExecuteAsync_Should_Throw_For_An_Undefined_Mode()
    {
        Func<Task> act = () => RunAsync("a", new IdempotencyExecutionOptions { Mode = (IdempotencyMode)7 });

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Mode (7)*");
    }

    [Fact]
    public async Task ExecuteAsync_Should_Use_The_Keyed_Store_Named_By_StoreKey()
    {
        RecordingStore payments = new(new InMemoryIdempotencyRecordStore(_time));
        ServiceCollection services = new();
        services.AddKeyedSingleton<IIdempotencyRecordStore>("payments", payments);
        await using ServiceProvider provider = services.BuildServiceProvider();
        _options.AddProfile("payments", profile => profile.StoreKey = "payments");
        IdempotencyExecutor executor = new(_store, new TestOptionsMonitor<IdempotencyExecutorOptions>(_options), provider);

        await executor.ExecuteAsync(Key, "fp", new IdempotencyExecutionOptions { Profile = "payments" }, Operation("paid"), Codec);

        payments.Calls.Should().Be(2);
        _store.Calls.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Throw_For_A_StoreKey_Without_A_Service_Provider()
    {
        Func<Task> act = () => RunAsync("a", new IdempotencyExecutionOptions { StoreKey = "payments" });

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*'payments'*IServiceProvider*");
    }

    [Fact]
    public async Task ExecuteAsync_Should_Reject_Missing_Arguments()
    {
        Func<Task> noKey = () => CreateExecutor().ExecuteAsync(null!, "fp", null, Operation("a"), Codec).AsTask();
        Func<Task> noFingerprint = () => CreateExecutor().ExecuteAsync(Key, "", null, Operation("a"), Codec).AsTask();
        Func<Task> noOperation = () => CreateExecutor().ExecuteAsync(Key, "fp", null, null!, Codec).AsTask();
        Func<Task> noCodec = () => CreateExecutor().ExecuteAsync(Key, "fp", null, Operation("a"), null!).AsTask();

        await noKey.Should().ThrowAsync<ArgumentNullException>();
        await noFingerprint.Should().ThrowAsync<ArgumentException>();
        await noOperation.Should().ThrowAsync<ArgumentNullException>();
        await noCodec.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Logs_Should_Name_The_Namespace_But_Never_The_Key_Or_The_Scope()
    {
        Func<Task> failing = () => CreateExecutor().ExecuteAsync<string>(Key, "fp", null, _ => throw new InvalidOperationException(), Codec).AsTask();

        await failing.Should().ThrowAsync<InvalidOperationException>();

        CapturedLog log = _logs.Named("Idempotency.KeptInFlight").Single();
        log.Message.Should().Contain("orders").And.Contain(Key.ToLogHash()).And.NotContain("k1").And.NotContain("alice");
        log.EventId.Id.Should().Be(7003);
    }

    [Fact]
    public void ToLogHash_Should_Be_Sixteen_Hex_Digits_That_Differ_Per_Key()
    {
        string hash = Key.ToLogHash();

        hash.Should().MatchRegex("^[0-9a-f]{16}$");
        hash.Should().Be(new IdempotencyRecordKey("orders", IdempotencyScope.User("alice"), "k1").ToLogHash());
        hash.Should().NotBe(new IdempotencyRecordKey("orders", IdempotencyScope.User("bob"), "k1").ToLogHash());
    }

    [Fact]
    public async Task ExecuteAsync_Should_Replay_After_A_Runtime_Switch_From_Observe_To_Enforce_What_Observe_Stored()
    {
        TestOptionsMonitor<IdempotencyExecutorOptions> monitor = new(new IdempotencyExecutorOptions { Mode = IdempotencyMode.Observe });
        IdempotencyExecutor executor = new(_store, monitor);

        IdempotencyExecution<string> first = await executor.ExecuteAsync(Key, "fp", null, Operation("A"), Codec);
        IdempotencyExecution<string> observed = await executor.ExecuteAsync(Key, "fp", null, Operation("B"), Codec);
        monitor.CurrentValue = new IdempotencyExecutorOptions { Mode = IdempotencyMode.Enforce };
        IdempotencyExecution<string> enforced = await executor.ExecuteAsync(Key, "fp", null, Operation("C"), Codec);

        first.Should().Be(new IdempotencyExecution<string>(IdempotencyExecutionStatus.Executed, "A"));
        observed.Should().Be(new IdempotencyExecution<string>(IdempotencyExecutionStatus.Executed, "B"));
        enforced.Should().Be(new IdempotencyExecution<string>(IdempotencyExecutionStatus.Replayed, "A"));
        _runs.Should().Be(2);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Reject_An_In_Flight_Key_After_A_Runtime_Switch_From_Observe_To_Enforce()
    {
        TestOptionsMonitor<IdempotencyExecutorOptions> monitor = new(new IdempotencyExecutorOptions { Mode = IdempotencyMode.Observe });
        IdempotencyExecutor executor = new(_store, monitor);
        await _store.TryReserveAsync(Key, "fp", TimeSpan.FromMinutes(1));

        IdempotencyExecution<string> observed = await executor.ExecuteAsync(Key, "fp", null, Operation("A"), Codec);
        monitor.CurrentValue = new IdempotencyExecutorOptions { Mode = IdempotencyMode.Enforce };
        IdempotencyExecution<string> enforced = await executor.ExecuteAsync(Key, "fp", null, Operation("B"), Codec);

        observed.Status.Should().Be(IdempotencyExecutionStatus.Executed);
        enforced.Status.Should().Be(IdempotencyExecutionStatus.InFlight);
        _runs.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Count_Each_Decision_In_Enforce_Mode()
    {
        const string Namespace = "metrics-executor-enforce";
        using IdempotencyDecisionListener listener = new(Namespace);
        IdempotencyRecordKey key = new(Namespace, IdempotencyScope.User("alice"), "k1");
        IdempotencyRecordKey inFlight = new(Namespace, IdempotencyScope.User("alice"), "k2");
        IdempotencyRecordKey released = new(Namespace, IdempotencyScope.User("alice"), "k3");
        await _store.TryReserveAsync(inFlight, "fp", TimeSpan.FromMinutes(1));

        await CreateExecutor().ExecuteAsync(key, "fp", null, Operation("done"), Codec);
        await CreateExecutor().ExecuteAsync(key, "fp", null, Operation("done"), Codec);
        await CreateExecutor().ExecuteAsync(key, "other-fp", null, Operation("done"), Codec);
        await CreateExecutor().ExecuteAsync(inFlight, "fp", null, Operation("done"), Codec);
        await CreateExecutor().ExecuteAsync(released, "fp", null, Operation("rolled-back"), Codec);

        listener.Decisions.Select(static decision => decision.Decision).Should().Equal(
            "reserved", "stored", "replay", "key_reused", "in_flight", "reserved", "released");
        listener.Decisions.Should().OnlyContain(static decision =>
            decision.Mode == "enforce" && decision.Source == "executor" && decision.TagCount == 4);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Count_What_Observe_Mode_Would_Do()
    {
        const string Namespace = "metrics-executor-observe";
        using IdempotencyDecisionListener listener = new(Namespace);
        IdempotencyRecordKey key = new(Namespace, IdempotencyScope.User("alice"), "k1");
        IdempotencyExecutionOptions observe = new() { Mode = IdempotencyMode.Observe };

        await CreateExecutor().ExecuteAsync(key, "fp", observe, Operation("done"), Codec);
        await CreateExecutor().ExecuteAsync(key, "fp", observe, Operation("done"), Codec);

        listener.Decisions.Should().Equal(
            new IdempotencyDecision("reserved", "observe", "executor", 4),
            new IdempotencyDecision("stored", "observe", "executor", 4),
            new IdempotencyDecision("replay", "observe", "executor", 4));
    }

    [Fact]
    public async Task ExecuteAsync_Should_Count_Nothing_In_Off_Mode()
    {
        const string Namespace = "metrics-executor-off";
        using IdempotencyDecisionListener listener = new(Namespace);
        IdempotencyRecordKey key = new(Namespace, IdempotencyScope.User("alice"), "k1");

        await CreateExecutor().ExecuteAsync(key, "fp", new IdempotencyExecutionOptions { Mode = IdempotencyMode.Off }, Operation("done"), Codec);

        listener.Decisions.Should().BeEmpty();
        _store.Calls.Should().Be(0);
    }

    private IdempotencyExecutor CreateExecutor() =>
        new(_store, new TestOptionsMonitor<IdempotencyExecutorOptions>(_options), logger: new Logger<IdempotencyExecutor>(_loggerFactory));

    private Task<IdempotencyExecution<string>> RunAsync(string result, IdempotencyExecutionOptions? options = null, string key = "k1") =>
        CreateExecutor().ExecuteAsync(new IdempotencyRecordKey("orders", IdempotencyScope.User("alice"), key), "fp", options, Operation(result), Codec).AsTask();

    private Func<CancellationToken, ValueTask<string>> Operation(string result) => _ =>
    {
        Interlocked.Increment(ref _runs);
        return new ValueTask<string>(result);
    };

    private sealed class TransactionalStore(IIdempotencyRecordStore inner, IIdempotencyRecordStore detached) : ITransactionalIdempotencyRecordStore
    {
        public IIdempotencyRecordStore Detached => detached;

        public ValueTask<IdempotencyRecordReservation> TryReserveAsync(
            IdempotencyRecordKey key, string fingerprint, TimeSpan lease, CancellationToken cancellationToken = default) =>
            inner.TryReserveAsync(key, fingerprint, lease, cancellationToken);

        public ValueTask<bool> CompleteAsync(
            IdempotencyRecordKey key, string token, string fingerprint, ReadOnlyMemory<byte> payload, TimeSpan retention, CancellationToken cancellationToken = default) =>
            inner.CompleteAsync(key, token, fingerprint, payload, retention, cancellationToken);

        public ValueTask<bool> ReleaseAsync(IdempotencyRecordKey key, string token, CancellationToken cancellationToken = default) =>
            inner.ReleaseAsync(key, token, cancellationToken);
    }

    private sealed class RecordingStore(IIdempotencyRecordStore inner) : IIdempotencyRecordStore
    {
        private int _calls;

        public int Calls => _calls;

        public int Releases { get; private set; }

        public List<TimeSpan> Leases { get; } = [];

        public List<TimeSpan> Retentions { get; } = [];

        public Action? AfterReserve { get; set; }

        public bool FailReserve { get; set; }

        public bool FailComplete { get; set; }

        public bool CompleteResult { get; set; } = true;

        public async ValueTask<IdempotencyRecordReservation> TryReserveAsync(
            IdempotencyRecordKey key, string fingerprint, TimeSpan lease, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _calls);
            cancellationToken.ThrowIfCancellationRequested();
            if (FailReserve)
                throw new InvalidOperationException("store down");
            lock (Leases)
                Leases.Add(lease);
            IdempotencyRecordReservation reservation = await inner.TryReserveAsync(key, fingerprint, lease, cancellationToken);
            AfterReserve?.Invoke();
            return reservation;
        }

        public async ValueTask<bool> CompleteAsync(
            IdempotencyRecordKey key, string token, string fingerprint, ReadOnlyMemory<byte> payload, TimeSpan retention, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _calls);
            if (FailComplete)
                throw new InvalidOperationException("store down");
            if (!CompleteResult)
                return false;
            Retentions.Add(retention);
            return await inner.CompleteAsync(key, token, fingerprint, payload, retention, cancellationToken);
        }

        public ValueTask<bool> ReleaseAsync(IdempotencyRecordKey key, string token, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _calls);
            Releases++;
            return inner.ReleaseAsync(key, token, cancellationToken);
        }
    }
}
