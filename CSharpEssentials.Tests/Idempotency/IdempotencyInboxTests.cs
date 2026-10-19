using System.Text;
using CSharpEssentials.Idempotency;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace CSharpEssentials.Tests.Idempotency;

public sealed class IdempotencyInboxTests
{
    private static readonly IdempotencyScope Tenant = IdempotencyScope.Tenant("acme");

    private static readonly IIdempotencyResultCodec<string> Codec = IdempotencyResultCodec.Create<string>(
        static value => Encoding.UTF8.GetBytes(value),
        static payload => Encoding.UTF8.GetString(payload.Span));

    private readonly FakeTimeProvider _time = new();
    private readonly InMemoryIdempotencyRecordStore _store;
    private readonly IdempotencyExecutorOptions _options = new();
    private readonly List<TimeSpan> _retentions = [];
    private int _runs;

    public IdempotencyInboxTests() => _store = new InMemoryIdempotencyRecordStore(_time);

    [Fact]
    public async Task ProcessAsync_Should_Run_A_Message_Once_And_Replay_The_Result_To_A_Redelivery()
    {
        IdempotencyInbox inbox = CreateInbox();

        IdempotencyExecution<string> first = await inbox.ProcessAsync("billing", "m-1", Tenant, Handler("charged"), Codec);
        IdempotencyExecution<string> redelivery = await inbox.ProcessAsync("billing", "m-1", Tenant, Handler("again"), Codec);

        first.Should().Be(new IdempotencyExecution<string>(IdempotencyExecutionStatus.Executed, "charged"));
        redelivery.Should().Be(new IdempotencyExecution<string>(IdempotencyExecutionStatus.Replayed, "charged"));
        _runs.Should().Be(1);
    }

    [Fact]
    public async Task ProcessAsync_Should_Let_Two_Consumers_Process_The_Same_Message()
    {
        IdempotencyInbox inbox = CreateInbox();

        IdempotencyExecution<string> billing = await inbox.ProcessAsync("billing", "m-1", Tenant, Handler("charged"), Codec);
        IdempotencyExecution<string> shipping = await inbox.ProcessAsync("shipping", "m-1", Tenant, Handler("shipped"), Codec);

        billing.Status.Should().Be(IdempotencyExecutionStatus.Executed);
        shipping.Should().Be(new IdempotencyExecution<string>(IdempotencyExecutionStatus.Executed, "shipped"));
        _runs.Should().Be(2);
    }

    [Fact]
    public async Task ProcessAsync_Should_Keep_The_Same_Message_Id_Of_Two_Owners_Apart()
    {
        IdempotencyInbox inbox = CreateInbox();

        await inbox.ProcessAsync("billing", "m-1", Tenant, Handler("acme"), Codec);
        IdempotencyExecution<string> other = await inbox.ProcessAsync("billing", "m-1", IdempotencyScope.Tenant("globex"), Handler("globex"), Codec);

        other.Should().Be(new IdempotencyExecution<string>(IdempotencyExecutionStatus.Executed, "globex"));
    }

    [Fact]
    public async Task ProcessAsync_Should_Store_Results_For_The_Retention_Of_The_Messaging_Profile()
    {
        IdempotencyInbox inbox = CreateInbox();

        await inbox.ProcessAsync("billing", "m-1", Tenant, Handler("charged"), Codec);
        _time.Advance(TimeSpan.FromDays(6));
        IdempotencyExecution<string> sixDaysLater = await inbox.ProcessAsync("billing", "m-1", Tenant, Handler("again"), Codec);
        _time.Advance(TimeSpan.FromDays(1) + TimeSpan.FromSeconds(1));
        IdempotencyExecution<string> afterRetention = await inbox.ProcessAsync("billing", "m-1", Tenant, Handler("again"), Codec);

        _retentions.Should().StartWith(TimeSpan.FromDays(7));
        sixDaysLater.Status.Should().Be(IdempotencyExecutionStatus.Replayed);
        afterRetention.Status.Should().Be(IdempotencyExecutionStatus.Executed);
    }

    [Fact]
    public async Task ProcessAsync_Should_Apply_Consumer_Settings_Over_The_Messaging_Profile()
    {
        IdempotencyInbox inbox = CreateInbox();

        await inbox.ProcessAsync("billing", "m-1", Tenant, Handler("charged"), Codec, new IdempotencyExecutionOptions { Retention = TimeSpan.FromDays(2) });

        _retentions.Should().Equal(TimeSpan.FromDays(2));
    }

    [Fact]
    public async Task ProcessAsync_Should_Use_Another_Profile_When_The_Consumer_Names_One()
    {
        _options.AddProfile("audit", profile => profile.Retention = TimeSpan.FromDays(30));
        IdempotencyInbox inbox = CreateInbox();

        await inbox.ProcessAsync("billing", "m-1", Tenant, Handler("charged"), Codec, new IdempotencyExecutionOptions { Profile = "audit" });

        _retentions.Should().Equal(TimeSpan.FromDays(30));
    }

    [Fact]
    public async Task ProcessAsync_Should_Keep_A_Failed_Message_In_Flight_Until_The_Lease_Expires()
    {
        IdempotencyInbox inbox = CreateInbox();
        Func<Task> failing = () => inbox.ProcessAsync<string>("billing", "m-1", Tenant, _ => throw new InvalidOperationException("boom"), Codec).AsTask();

        await failing.Should().ThrowAsync<InvalidOperationException>();
        IdempotencyExecution<string> duringLease = await inbox.ProcessAsync("billing", "m-1", Tenant, Handler("charged"), Codec);
        _time.Advance(_options.Lease + TimeSpan.FromSeconds(1));
        IdempotencyExecution<string> afterLease = await inbox.ProcessAsync("billing", "m-1", Tenant, Handler("charged"), Codec);

        duringLease.Status.Should().Be(IdempotencyExecutionStatus.InFlight);
        afterLease.Should().Be(new IdempotencyExecution<string>(IdempotencyExecutionStatus.Executed, "charged"));
    }

    [Fact]
    public async Task ProcessAsync_Without_A_Result_Should_Run_A_Message_Once()
    {
        IdempotencyInbox inbox = CreateInbox();

        IdempotencyExecutionStatus first = await inbox.ProcessAsync("billing", "m-1", Tenant, Handle);
        IdempotencyExecutionStatus redelivery = await inbox.ProcessAsync("billing", "m-1", Tenant, Handle);

        first.Should().Be(IdempotencyExecutionStatus.Executed);
        redelivery.Should().Be(IdempotencyExecutionStatus.Replayed);
        _runs.Should().Be(1);
    }

    [Fact]
    public async Task ProcessAsync_Without_A_Result_Should_Report_A_Message_In_Flight()
    {
        TaskCompletionSource gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        IdempotencyInbox inbox = CreateInbox();
        Task<IdempotencyExecutionStatus> first = inbox.ProcessAsync("billing", "m-1", Tenant, async _ => await gate.Task).AsTask();

        IdempotencyExecutionStatus duplicate = await inbox.ProcessAsync("billing", "m-1", Tenant, Handle);
        gate.SetResult();

        duplicate.Should().Be(IdempotencyExecutionStatus.InFlight);
        (await first).Should().Be(IdempotencyExecutionStatus.Executed);
        _runs.Should().Be(0);
    }

    [Fact]
    public async Task ProcessAsync_Should_Reject_An_Empty_Consumer_Or_Message_Id()
    {
        Func<Task> noConsumer = () => CreateInbox().ProcessAsync("", "m-1", Tenant, Handle).AsTask();
        Func<Task> noMessageId = () => CreateInbox().ProcessAsync("billing", "", Tenant, Handle).AsTask();
        Func<Task> noHandler = () => CreateInbox().ProcessAsync("billing", "m-1", Tenant, null!).AsTask();

        await noConsumer.Should().ThrowAsync<ArgumentException>();
        await noMessageId.Should().ThrowAsync<ArgumentException>();
        await noHandler.Should().ThrowAsync<ArgumentNullException>();
        _runs.Should().Be(0);
    }

    [Fact]
    public void AddIdempotencyExecutor_Should_Register_The_Inbox()
    {
        ServiceCollection services = new();

        services.AddIdempotencyExecutor();
        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredService<IdempotencyInbox>().Should().NotBeNull();
    }

    private IdempotencyInbox CreateInbox() => new(new IdempotencyExecutor(new RetentionRecorder(_store, _retentions), _options));

    private Func<CancellationToken, ValueTask<string>> Handler(string result) => _ =>
    {
        _runs++;
        return new ValueTask<string>(result);
    };

    private ValueTask Handle(CancellationToken cancellationToken)
    {
        _runs++;
        return default;
    }

    private sealed class RetentionRecorder(IIdempotencyRecordStore inner, List<TimeSpan> retentions) : IIdempotencyRecordStore
    {
        public ValueTask<IdempotencyRecordReservation> TryReserveAsync(
            IdempotencyRecordKey key, string fingerprint, TimeSpan lease, CancellationToken cancellationToken = default) =>
            inner.TryReserveAsync(key, fingerprint, lease, cancellationToken);

        public ValueTask<bool> CompleteAsync(
            IdempotencyRecordKey key, string token, string fingerprint, ReadOnlyMemory<byte> payload, TimeSpan retention, CancellationToken cancellationToken = default)
        {
            retentions.Add(retention);
            return inner.CompleteAsync(key, token, fingerprint, payload, retention, cancellationToken);
        }

        public ValueTask<bool> ReleaseAsync(IdempotencyRecordKey key, string token, CancellationToken cancellationToken = default) =>
            inner.ReleaseAsync(key, token, cancellationToken);
    }
}
