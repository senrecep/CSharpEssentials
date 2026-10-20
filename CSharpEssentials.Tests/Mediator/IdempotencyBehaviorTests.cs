using System.Security.Claims;

using CSharpEssentials.Errors;
using CSharpEssentials.Exceptions;
using CSharpEssentials.Idempotency;
using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Tests.AspNetCore.Idempotency;
using CSharpEssentials.Tests.Idempotency;

using FluentAssertions;

using Mediator;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;

namespace CSharpEssentials.Tests.Mediator;

public sealed class IdempotencyBehaviorTests : IDisposable
{
    private static readonly ClaimsPrincipal Alice = UserPrincipal("alice");

    private readonly FakeTimeProvider _time = new();
    private readonly RecordingStore _store;
    private readonly CapturingLoggerProvider _logs = new();
    private readonly LoggerFactory _loggerFactory;
    private readonly IdempotencyExecutorOptions _executorOptions = new();
    private readonly IdempotencyBehaviorOptions _options = new();
    private ClaimsPrincipal? _principal = Alice;
    private int _runs;

    public IdempotencyBehaviorTests()
    {
        _store = new RecordingStore(new InMemoryIdempotencyRecordStore(_time));
        _loggerFactory = new LoggerFactory([_logs]);
    }

    public void Dispose() => _loggerFactory.Dispose();

    [Fact]
    public async Task Handle_Should_Run_The_Handler_Once_And_Replay_The_Response()
    {
        IdempotencyBehavior<IdemCommand, Result<int>> behavior = Create<IdemCommand, Result<int>>();

        Result<int> first = await behavior.Handle(new IdemCommand("k1", 5), (_, _) => Run(42), CancellationToken.None);
        Result<int> retry = await behavior.Handle(new IdemCommand("k1", 5), (_, _) => Run(99), CancellationToken.None);

        first.Value.Should().Be(42);
        retry.Value.Should().Be(42);
        _runs.Should().Be(1);
    }

    [Fact]
    public async Task Handle_Should_Return_InFlight_Conflict_For_A_Parallel_Duplicate()
    {
        IdempotencyBehavior<IdemCommand, Result<int>> behavior = Create<IdemCommand, Result<int>>();
        TaskCompletionSource gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<Result<int>> first = behavior.Handle(new IdemCommand("k1", 5), async (_, _) =>
        {
            await gate.Task;
            return 42;
        }, CancellationToken.None).AsTask();

        Result<int> duplicate = await behavior.Handle(new IdemCommand("k1", 5), (_, _) => Run(99), CancellationToken.None);
        gate.SetResult();

        duplicate.IsFailure.Should().BeTrue();
        duplicate.FirstError.Code.Should().Be(IdempotencyErrorCodes.InFlight);
        duplicate.FirstError.Type.Should().Be(ErrorType.Conflict);
        (await first).Value.Should().Be(42);
        _runs.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Should_Release_The_Key_When_A_Transactional_Request_Fails()
    {
        IdempotencyBehavior<IdemTransactionalCommand, Result<int>> behavior = Create<IdemTransactionalCommand, Result<int>>();

        Result<int> failed = await behavior.Handle(new IdemTransactionalCommand("k1"), (_, _) => Fail(Error.Validation("Order.Invalid")), CancellationToken.None);
        Result<int> retry = await behavior.Handle(new IdemTransactionalCommand("k1"), (_, _) => Run(42), CancellationToken.None);

        failed.IsFailure.Should().BeTrue();
        retry.Value.Should().Be(42);
        _runs.Should().Be(2);
        _store.Releases.Should().Be(1);
    }

    [Fact]
    public async Task Handle_Should_Replay_A_Business_Failure_For_The_Whole_Retention()
    {
        IdempotencyBehavior<IdemCommand, Result<int>> behavior = Create<IdemCommand, Result<int>>();

        await behavior.Handle(new IdemCommand("k1", 5), (_, _) => Fail(Error.Validation("Order.Invalid")), CancellationToken.None);
        _time.Advance(_executorOptions.Retention - TimeSpan.FromMinutes(1));
        Result<int> beforeExpiry = await behavior.Handle(new IdemCommand("k1", 5), (_, _) => Run(42), CancellationToken.None);
        _time.Advance(TimeSpan.FromMinutes(2));
        Result<int> afterExpiry = await behavior.Handle(new IdemCommand("k1", 5), (_, _) => Run(42), CancellationToken.None);

        _store.Retentions.Should().Equal(_executorOptions.Retention, _executorOptions.Retention);
        beforeExpiry.IsFailure.Should().BeTrue();
        beforeExpiry.FirstError.Code.Should().Be("Order.Invalid");
        afterExpiry.Value.Should().Be(42);
        _runs.Should().Be(2);
    }

    [Fact]
    public async Task Handle_Should_Replay_An_Unexpected_Failure_Only_For_The_Failure_Retention()
    {
        IdempotencyBehavior<IdemCommand, Result<int>> behavior = Create<IdemCommand, Result<int>>();

        await behavior.Handle(new IdemCommand("k1", 5), (_, _) => Fail(Error.Unexpected("Db.Down")), CancellationToken.None);
        _time.Advance(_executorOptions.FailureRetention - TimeSpan.FromMinutes(1));
        Result<int> beforeExpiry = await behavior.Handle(new IdemCommand("k1", 5), (_, _) => Run(42), CancellationToken.None);
        _time.Advance(TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(1));
        Result<int> afterExpiry = await behavior.Handle(new IdemCommand("k1", 5), (_, _) => Run(42), CancellationToken.None);

        _store.Retentions.Should().StartWith(TimeSpan.FromMinutes(10));
        beforeExpiry.FirstError.Code.Should().Be("Db.Down");
        afterExpiry.Value.Should().Be(42);
        _runs.Should().Be(2);
    }

    [Fact]
    public async Task Handle_Should_Apply_The_Request_Type_Failure_Retention_To_An_Unexpected_Failure()
    {
        _options.ForRequest<IdemCommand>(settings => settings.FailureRetention = TimeSpan.FromMinutes(1));
        IdempotencyBehavior<IdemCommand, Result<int>> behavior = Create<IdemCommand, Result<int>>();

        await behavior.Handle(new IdemCommand("k1", 5), (_, _) => Fail(Error.Unexpected("Db.Down")), CancellationToken.None);
        await behavior.Handle(new IdemCommand("k2", 5), (_, _) => Fail(Error.NotFound("Order.Missing")), CancellationToken.None);

        _store.Retentions.Should().Equal(TimeSpan.FromMinutes(1), _executorOptions.Retention);
    }

    [Fact]
    public async Task Handle_Should_Release_Failures_When_StoreFailures_Is_Off()
    {
        _options.StoreFailures = false;
        IdempotencyBehavior<IdemCommand, Result<int>> behavior = Create<IdemCommand, Result<int>>();

        await behavior.Handle(new IdemCommand("k1", 5), (_, _) => Fail(Error.Validation("Order.Invalid")), CancellationToken.None);
        Result<int> retry = await behavior.Handle(new IdemCommand("k1", 5), (_, _) => Run(42), CancellationToken.None);

        retry.Value.Should().Be(42);
        _store.Releases.Should().Be(1);
    }

    [Fact]
    public async Task Handle_Should_Reject_A_Key_Reused_With_Another_Request()
    {
        IdempotencyBehavior<IdemCommand, Result<int>> behavior = Create<IdemCommand, Result<int>>();

        await behavior.Handle(new IdemCommand("k1", 5), (_, _) => Run(42), CancellationToken.None);
        Result<int> reused = await behavior.Handle(new IdemCommand("k1", 6), (_, _) => Run(99), CancellationToken.None);

        reused.FirstError.Code.Should().Be(IdempotencyErrorCodes.KeyReused);
        reused.FirstError.Type.Should().Be(ErrorType.Validation);
        _runs.Should().Be(1);
    }

    [Fact]
    public async Task Handle_Should_Use_The_Fingerprint_Of_The_Request_When_It_Sets_One()
    {
        IdempotencyBehavior<IdemFingerprintCommand, Result<int>> behavior = Create<IdemFingerprintCommand, Result<int>>();

        await behavior.Handle(new IdemFingerprintCommand("k1", "order-7", "first note"), (_, _) => Run(42), CancellationToken.None);
        Result<int> retry = await behavior.Handle(new IdemFingerprintCommand("k1", "order-7", "second note"), (_, _) => Run(99), CancellationToken.None);

        retry.Value.Should().Be(42);
        _store.Fingerprints.Should().Equal("order-7", "order-7");
    }

    [Fact]
    public async Task Handle_Should_Keep_The_Same_Key_Of_Two_Callers_Apart()
    {
        IdempotencyBehavior<IdemCommand, Result<int>> behavior = Create<IdemCommand, Result<int>>();

        await behavior.Handle(new IdemCommand("k1", 5), (_, _) => Run(42), CancellationToken.None);
        _principal = UserPrincipal("bob");
        Result<int> bob = await behavior.Handle(new IdemCommand("k1", 5), (_, _) => Run(99), CancellationToken.None);

        bob.Value.Should().Be(99);
        _store.Keys.Select(key => key.Scope).Should().Equal(IdempotencyScope.User("alice").Value, IdempotencyScope.User("bob").Value);
    }

    [Fact]
    public async Task Handle_Should_Prefer_The_Scope_Of_The_Request_Over_The_Resolver()
    {
        IdempotencyBehavior<IdemTenantCommand, Result<int>> behavior = Create<IdemTenantCommand, Result<int>>();

        await behavior.Handle(new IdemTenantCommand("k1", "acme"), (_, _) => Run(42), CancellationToken.None);

        _store.Keys.Should().Equal(new IdempotencyRecordKey(typeof(IdemTenantCommand).FullName!, IdempotencyScope.Tenant("acme"), "k1"));
    }

    [Fact]
    public async Task Handle_Should_Feed_The_Principal_Of_The_Scope_Accessor_To_The_Resolver()
    {
        _options.ScopeResolver = IdempotencyScopes.Client;
        _principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(IdempotencyScopes.ClientIdClaimType, "billing-service")], "test"));
        IdempotencyBehavior<IdemCommand, Result<int>> behavior = Create<IdemCommand, Result<int>>();

        await behavior.Handle(new IdemCommand("k1", 5), (_, _) => Run(42), CancellationToken.None);

        _store.Keys.Select(key => key.Scope).Should().Equal(IdempotencyScope.Client("billing-service").Value);
    }

    [Fact]
    public async Task Handle_Should_Keep_The_Key_In_Flight_When_The_Handler_Throws()
    {
        IdempotencyBehavior<IdemCommand, Result<int>> behavior = Create<IdemCommand, Result<int>>();
        Func<Task> throwing = () => behavior.Handle(new IdemCommand("k1", 5), (_, _) => throw new InvalidOperationException("boom"), CancellationToken.None).AsTask();

        await throwing.Should().ThrowAsync<InvalidOperationException>();
        Result<int> duringLease = await behavior.Handle(new IdemCommand("k1", 5), (_, _) => Run(42), CancellationToken.None);
        _time.Advance(_executorOptions.Lease + TimeSpan.FromSeconds(1));
        Result<int> afterLease = await behavior.Handle(new IdemCommand("k1", 5), (_, _) => Run(42), CancellationToken.None);

        duringLease.FirstError.Code.Should().Be(IdempotencyErrorCodes.InFlight);
        afterLease.Value.Should().Be(42);
    }

    [Fact]
    public async Task Handle_Should_Return_InFlight_To_A_Retry_After_ExceptionHandling_Converted_The_Exception()
    {
        IdempotencyBehavior<IdemCommand, Result<int>> idempotency = Create<IdemCommand, Result<int>>();
        ExceptionHandlingBehavior<IdemCommand, Result<int>> exceptionHandling = new();

        Result<int> first = await exceptionHandling.Handle(new IdemCommand("k1", 5),
            (message, token) => idempotency.Handle(message, (_, _) => throw new InvalidOperationException("boom"), token), CancellationToken.None);
        Result<int> retry = await exceptionHandling.Handle(new IdemCommand("k1", 5),
            (message, token) => idempotency.Handle(message, (_, _) => Run(42), token), CancellationToken.None);

        first.IsFailure.Should().BeTrue();
        first.FirstError.Code.Should().NotBe(IdempotencyErrorCodes.InFlight);
        retry.FirstError.Code.Should().Be(IdempotencyErrorCodes.InFlight);
        _runs.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Should_Use_The_Retention_Of_The_Request_Type()
    {
        _options.ForRequest<IdemCommand>(settings => settings.Retention = TimeSpan.FromHours(2));
        IdempotencyBehavior<IdemCommand, Result<int>> behavior = Create<IdemCommand, Result<int>>();

        await behavior.Handle(new IdemCommand("k1", 5), (_, _) => Run(42), CancellationToken.None);

        _store.Retentions.Should().Equal(TimeSpan.FromHours(2));
    }

    [Fact]
    public async Task Handle_Should_Use_The_Profile_Of_The_Request_Type()
    {
        _executorOptions.AddProfile("payments", profile => profile.Retention = TimeSpan.FromDays(2));
        _options.ForRequest<IdemCommand>(settings => settings.Profile = "payments");
        IdempotencyBehavior<IdemCommand, Result<int>> behavior = Create<IdemCommand, Result<int>>();

        await behavior.Handle(new IdemCommand("k1", 5), (_, _) => Run(42), CancellationToken.None);

        _store.Retentions.Should().Equal(TimeSpan.FromDays(2));
    }

    [Fact]
    public async Task Handle_Should_Prefer_The_Retention_And_Lease_Of_The_Request()
    {
        _options.ForRequest<IdemTimedCommand>(settings =>
        {
            settings.Retention = TimeSpan.FromHours(2);
            settings.Lease = TimeSpan.FromMinutes(3);
        });
        IdempotencyBehavior<IdemTimedCommand, Result<int>> behavior = Create<IdemTimedCommand, Result<int>>();

        await behavior.Handle(new IdemTimedCommand("k1", TimeSpan.FromHours(5), TimeSpan.FromMinutes(4)), (_, _) => Run(42), CancellationToken.None);

        _store.Retentions.Should().Equal(TimeSpan.FromHours(5));
        _store.Leases.Should().Equal(TimeSpan.FromMinutes(4));
    }

    [Fact]
    public async Task Handle_Should_Reject_An_Unscoped_Key_By_Default()
    {
        _principal = null;
        IdempotencyBehavior<IdemCommand, Result<int>> behavior = Create<IdemCommand, Result<int>>();

        Result<int> result = await behavior.Handle(new IdemCommand("k1", 5), (_, _) => Run(42), CancellationToken.None);

        result.FirstError.Code.Should().Be(IdempotencyErrorCodes.ScopeRequired);
        _runs.Should().Be(0);
        _store.Reservations.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Should_Run_An_Unscoped_Key_Without_Idempotency_When_Skip_Is_Set()
    {
        _principal = null;
        _options.UnscopedKeyBehavior = UnscopedKeyBehavior.Skip;
        IdempotencyBehavior<IdemCommand, Result<int>> behavior = Create<IdemCommand, Result<int>>();

        await behavior.Handle(new IdemCommand("k1", 5), (_, _) => Run(42), CancellationToken.None);
        await behavior.Handle(new IdemCommand("k1", 5), (_, _) => Run(42), CancellationToken.None);

        _runs.Should().Be(2);
        _store.Reservations.Should().Be(0);
        _logs.Named(IdempotencyErrorCodes.UnscopedKeySkipped).Should().HaveCount(2)
            .And.OnlyContain(entry => entry.Level == LogLevel.Warning);
    }

    [Fact]
    public async Task Handle_Should_Store_An_Unscoped_Key_Globally_When_Global_Is_Set()
    {
        _principal = null;
        _options.UnscopedKeyBehavior = UnscopedKeyBehavior.Global;
        IdempotencyBehavior<IdemCommand, Result<int>> behavior = Create<IdemCommand, Result<int>>();

        await behavior.Handle(new IdemCommand("k1", 5), (_, _) => Run(42), CancellationToken.None);
        Result<int> retry = await behavior.Handle(new IdemCommand("k1", 5), (_, _) => Run(99), CancellationToken.None);

        retry.Value.Should().Be(42);
        _runs.Should().Be(1);
        _store.Keys.Select(key => key.Scope).Should().Equal(null, null);
    }

    [Theory]
    [InlineData("", IdempotencyErrorCodes.KeyMissing)]
    [InlineData("has space", IdempotencyErrorCodes.KeyInvalid)]
    [InlineData("quote\"", IdempotencyErrorCodes.KeyInvalid)]
    public async Task Handle_Should_Reject_A_Missing_Or_Invalid_Key(string key, string code)
    {
        IdempotencyBehavior<IdemCommand, Result<int>> behavior = Create<IdemCommand, Result<int>>();

        Result<int> result = await behavior.Handle(new IdemCommand(key, 5), (_, _) => Run(42), CancellationToken.None);

        result.FirstError.Code.Should().Be(code);
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        _runs.Should().Be(0);
        _store.Reservations.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Should_Reject_A_Key_Longer_Than_The_Maximum()
    {
        IdempotencyBehavior<IdemCommand, Result<int>> behavior = Create<IdemCommand, Result<int>>();

        Result<int> result = await behavior.Handle(new IdemCommand(new string('a', IdempotencyKeys.MaxKeyLength + 1), 5), (_, _) => Run(42), CancellationToken.None);

        result.FirstError.Code.Should().Be(IdempotencyErrorCodes.KeyInvalid);
    }

    [Fact]
    public async Task Handle_In_Observe_Mode_Should_Run_A_Request_With_An_Invalid_Key_And_Log_It()
    {
        _executorOptions.Mode = IdempotencyMode.Observe;
        IdempotencyBehavior<IdemCommand, Result<int>> behavior = Create<IdemCommand, Result<int>>();

        Result<int> result = await behavior.Handle(new IdemCommand("has space", 5), (_, _) => Run(42), CancellationToken.None);

        result.Value.Should().Be(42);
        _logs.Named("Idempotency.Observe.WouldRejectKey").Should().ContainSingle()
            .Which.Message.Should().Contain(IdempotencyErrorCodes.KeyInvalid);
    }

    [Fact]
    public async Task Handle_In_Observe_Mode_Should_Run_An_Unscoped_Request_And_Log_It()
    {
        _executorOptions.Mode = IdempotencyMode.Observe;
        _principal = null;
        IdempotencyBehavior<IdemCommand, Result<int>> behavior = Create<IdemCommand, Result<int>>();

        Result<int> result = await behavior.Handle(new IdemCommand("k1", 5), (_, _) => Run(42), CancellationToken.None);

        result.Value.Should().Be(42);
        _logs.Named("Idempotency.Observe.WouldRejectScopeRequired").Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_In_Observe_Mode_Should_Run_The_Request_When_The_Resolver_Throws()
    {
        _executorOptions.Mode = IdempotencyMode.Observe;
        _options.ScopeResolver = new ThrowingResolver();
        IdempotencyBehavior<IdemCommand, Result<int>> behavior = Create<IdemCommand, Result<int>>();

        Result<int> result = await behavior.Handle(new IdemCommand("k1", 5), (_, _) => Run(42), CancellationToken.None);

        result.Value.Should().Be(42);
        _logs.Named("Idempotency.Observe.ResolutionFailed").Should().ContainSingle()
            .Which.Level.Should().Be(LogLevel.Warning);
    }

    [Fact]
    public async Task Handle_In_Enforce_Mode_Should_Propagate_A_Resolver_Exception()
    {
        _options.ScopeResolver = new ThrowingResolver();
        IdempotencyBehavior<IdemCommand, Result<int>> behavior = Create<IdemCommand, Result<int>>();

        Func<Task> act = () => behavior.Handle(new IdemCommand("k1", 5), (_, _) => Run(42), CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("no scope");
        _runs.Should().Be(0);
    }

    [Fact]
    public async Task Handle_In_Observe_Mode_Should_Run_A_Retry_Again()
    {
        _executorOptions.Mode = IdempotencyMode.Observe;
        IdempotencyBehavior<IdemCommand, Result<int>> behavior = Create<IdemCommand, Result<int>>();

        await behavior.Handle(new IdemCommand("k1", 5), (_, _) => Run(42), CancellationToken.None);
        Result<int> retry = await behavior.Handle(new IdemCommand("k1", 5), (_, _) => Run(99), CancellationToken.None);

        retry.Value.Should().Be(99);
        _runs.Should().Be(2);
        _logs.Named("Idempotency.Observe.WouldReplay").Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_In_Off_Mode_Should_Only_Call_The_Next_Step()
    {
        _options.ForRequest<IdemCommand>(settings => settings.Mode = IdempotencyMode.Off);
        _options.ScopeResolver = new ThrowingResolver();
        IdempotencyBehavior<IdemCommand, Result<int>> behavior = Create<IdemCommand, Result<int>>();

        await behavior.Handle(new IdemCommand("", 5), (_, _) => Run(42), CancellationToken.None);
        await behavior.Handle(new IdemCommand("k1", 5), (_, _) => Run(42), CancellationToken.None);
        await behavior.Handle(new IdemCommand("k1", 5), (_, _) => Run(42), CancellationToken.None);

        _runs.Should().Be(3);
        _store.Reservations.Should().Be(0);
        _logs.Entries.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Replay_A_Non_Generic_Result()
    {
        IdempotencyBehavior<IdemPlainCommand, Result> behavior = Create<IdemPlainCommand, Result>();

        await behavior.Handle(new IdemPlainCommand("k1"), (_, _) => RunPlain(Result.Failure(Error.Conflict("Order.Paid"))), CancellationToken.None);
        Result retry = await behavior.Handle(new IdemPlainCommand("k1"), (_, _) => RunPlain(Result.Success()), CancellationToken.None);

        retry.IsFailure.Should().BeTrue();
        retry.FirstError.Code.Should().Be("Order.Paid");
        _runs.Should().Be(1);
    }

    [Fact]
    public async Task Handle_Should_Return_A_Non_Generic_Failure_For_A_Rejected_Request()
    {
        _principal = null;
        IdempotencyBehavior<IdemPlainCommand, Result> behavior = Create<IdemPlainCommand, Result>();

        Result result = await behavior.Handle(new IdemPlainCommand("k1"), (_, _) => RunPlain(Result.Success()), CancellationToken.None);

        result.FirstError.Code.Should().Be(IdempotencyErrorCodes.ScopeRequired);
    }

    [Fact]
    public async Task Handle_Should_Replay_The_Code_Type_And_Metadata_Of_A_Stored_Failure()
    {
        IdempotencyBehavior<IdemCommand, Result<int>> behavior = Create<IdemCommand, Result<int>>();
        Error error = Error.NotFound("Order.Missing", "No such order.", new ErrorMetadata(new KeyValuePair<string, object?>("orderId", "o-7")));

        await behavior.Handle(new IdemCommand("k1", 5), (_, _) => Fail(error), CancellationToken.None);
        Result<int> retry = await behavior.Handle(new IdemCommand("k1", 5), (_, _) => Run(42), CancellationToken.None);

        retry.FirstError.Code.Should().Be("Order.Missing");
        retry.FirstError.Description.Should().Be("No such order.");
        retry.FirstError.Type.Should().Be(ErrorType.NotFound);
        retry.FirstError.Metadata.Should().ContainKey("orderId");
        _runs.Should().Be(1);
    }

    [Fact]
    public async Task Handle_Should_Replay_A_Response_That_Is_Not_A_Result()
    {
        IdempotencyBehavior<IdemNumberCommand, int> behavior = Create<IdemNumberCommand, int>();

        await behavior.Handle(new IdemNumberCommand("k1"), (_, _) => RunNumber(42), CancellationToken.None);
        int retry = await behavior.Handle(new IdemNumberCommand("k1"), (_, _) => RunNumber(99), CancellationToken.None);

        retry.Should().Be(42);
        _runs.Should().Be(1);
    }

    [Fact]
    public async Task Handle_Should_Throw_For_A_Rejected_Request_Whose_Response_Is_Not_A_Result()
    {
        _principal = null;
        IdempotencyBehavior<IdemNumberCommand, int> behavior = Create<IdemNumberCommand, int>();

        Func<Task> act = () => behavior.Handle(new IdemNumberCommand("k1"), (_, _) => RunNumber(42), CancellationToken.None).AsTask();

        (await act.Should().ThrowAsync<EnhancedValidationException>())
            .Which.Errors.Should().ContainSingle(error => error.Code == IdempotencyErrorCodes.ScopeRequired);
    }

    [Fact]
    public async Task ForRequest_Should_Configure_The_Same_Settings_When_Called_Again()
    {
        _options.ForRequest<IdemCommand>(settings => settings.Retention = TimeSpan.FromHours(2));
        _options.ForRequest<IdemCommand>(settings => settings.Lease = TimeSpan.FromMinutes(3));
        IdempotencyBehavior<IdemCommand, Result<int>> behavior = Create<IdemCommand, Result<int>>();

        await behavior.Handle(new IdemCommand("k1", 5), (_, _) => Run(42), CancellationToken.None);

        _store.Retentions.Should().Equal(TimeSpan.FromHours(2));
        _store.Leases.Should().Equal(TimeSpan.FromMinutes(3));
    }

    [Fact]
    public void ForRequest_Should_Reject_A_Null_Configure()
    {
        Action act = () => _options.ForRequest<IdemCommand>(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    private static ClaimsPrincipal UserPrincipal(string userId) =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "test"));

    private IdempotencyBehavior<TRequest, TResponse> Create<TRequest, TResponse>()
        where TRequest : IIdempotentRequest, IMessage =>
        new(
            new IdempotencyExecutor(_store, new TestOptionsMonitor<IdempotencyExecutorOptions>(_executorOptions), logger: _loggerFactory.CreateLogger<IdempotencyExecutor>()),
            _executorOptions,
            _options,
            _loggerFactory.CreateLogger<IdempotencyBehavior<TRequest, TResponse>>(),
            new PrincipalAccessor(() => _principal));

    private ValueTask<Result<int>> Run(int value)
    {
        _runs++;
        return new ValueTask<Result<int>>(value);
    }

    private ValueTask<Result<int>> Fail(Error error)
    {
        _runs++;
        return new ValueTask<Result<int>>(Result<int>.Failure(error));
    }

    private ValueTask<Result> RunPlain(Result result)
    {
        _runs++;
        return new ValueTask<Result>(result);
    }

    private ValueTask<int> RunNumber(int value)
    {
        _runs++;
        return new ValueTask<int>(value);
    }

    private sealed class PrincipalAccessor(Func<ClaimsPrincipal?> principal) : IIdempotencyScopeAccessor
    {
        public ClaimsPrincipal? Principal => principal();
    }

    private sealed class ThrowingResolver : IIdempotencyScopeResolver
    {
        public IdempotencyScope? Resolve(ClaimsPrincipal? principal) => throw new InvalidOperationException("no scope");
    }

    private sealed class RecordingStore(IIdempotencyRecordStore inner) : IIdempotencyRecordStore
    {
        public List<IdempotencyRecordKey> Keys { get; } = [];

        public List<string> Fingerprints { get; } = [];

        public List<TimeSpan> Leases { get; } = [];

        public List<TimeSpan> Retentions { get; } = [];

        public int Reservations => Keys.Count;

        public int Releases { get; private set; }

        public ValueTask<IdempotencyRecordReservation> TryReserveAsync(
            IdempotencyRecordKey key, string fingerprint, TimeSpan lease, CancellationToken cancellationToken = default)
        {
            Keys.Add(key);
            Fingerprints.Add(fingerprint);
            Leases.Add(lease);
            return inner.TryReserveAsync(key, fingerprint, lease, cancellationToken);
        }

        public ValueTask<bool> CompleteAsync(
            IdempotencyRecordKey key, string token, string fingerprint, ReadOnlyMemory<byte> payload, TimeSpan retention, CancellationToken cancellationToken = default)
        {
            Retentions.Add(retention);
            return inner.CompleteAsync(key, token, fingerprint, payload, retention, cancellationToken);
        }

        public ValueTask<bool> ReleaseAsync(IdempotencyRecordKey key, string token, CancellationToken cancellationToken = default)
        {
            Releases++;
            return inner.ReleaseAsync(key, token, cancellationToken);
        }
    }
}

internal sealed record IdemCommand(string IdempotencyKey, int Amount) : ICommand<Result<int>>, IIdempotentRequest;

internal sealed record IdemTransactionalCommand(string IdempotencyKey) : ICommand<Result<int>>, IIdempotentRequest, ITransactionalRequest;

internal sealed record IdemPlainCommand(string IdempotencyKey) : ICommand<Result>, IIdempotentRequest;

internal sealed record IdemNumberCommand(string IdempotencyKey) : ICommand<int>, IIdempotentRequest;

internal sealed record IdemTenantCommand(string IdempotencyKey, string TenantId) : ICommand<Result<int>>, IIdempotentRequest
{
    IdempotencyScope? IIdempotentRequest.IdempotencyScope => IdempotencyScope.Tenant(TenantId);
}

internal sealed record IdemFingerprintCommand(string IdempotencyKey, string OrderId, string Note) : ICommand<Result<int>>, IIdempotentRequest
{
    string? IIdempotentRequest.IdempotencyFingerprint => OrderId;
}

internal sealed record IdemTimedCommand(string IdempotencyKey, TimeSpan Retention, TimeSpan Lease) : ICommand<Result<int>>, IIdempotentRequest
{
    TimeSpan? IIdempotentRequest.IdempotencyRetention => Retention;

    TimeSpan? IIdempotentRequest.IdempotencyLease => Lease;
}
