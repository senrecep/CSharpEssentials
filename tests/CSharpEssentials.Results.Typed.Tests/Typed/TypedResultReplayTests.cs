using System.Collections.Concurrent;
using CSharpEssentials.Errors;
using CSharpEssentials.Exceptions;
using CSharpEssentials.Idempotency;
using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;
using FluentAssertions;
using Mediator;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CSharpEssentials.Results.Typed.Tests.Typed;

internal sealed record GetCachedUserQuery(int Id) : IQuery<Result<UserDto, NotFoundError, UserErrors.Locked, RateLimited>>, ICacheable
{
    public bool BypassCache => false;
    public bool CacheFailures => true;
    public string CacheKey => "user:" + Id;
    public TimeSpan Expiration => TimeSpan.FromMinutes(1);
}

internal sealed record LockUserCommand(int Id) : ICommand<Result<UserDto, NotFoundError, UserErrors.Locked, RateLimited>>, IIdempotentRequest
{
    public string IdempotencyKey => "lock-" + Id;

    IdempotencyScope? IIdempotentRequest.IdempotencyScope => IdempotencyScope.Global;
}

internal sealed record GetUserByIdQuery(int Id, bool CacheFailures = false) : IQuery<Result<UserDto, NotFoundError>>, ICacheable
{
    public bool BypassCache => false;
    public string CacheKey => "user-by-id:" + Id;
    public TimeSpan Expiration => TimeSpan.FromMinutes(1);
}

internal sealed record GetLockableUserQuery(int Id, bool CacheFailures = false) : IQuery<Result<UserDto, NotFoundError, UserErrors.Locked>>, ICacheable
{
    public bool BypassCache => false;
    public string CacheKey => "lockable-user:" + Id;
    public TimeSpan Expiration => TimeSpan.FromMinutes(1);
}

internal sealed record GetImportedUserQuery(int Id, bool CacheFailures = false) : IQuery<Result<UserDto, InvalidOperationException>>, ICacheable
{
    public bool BypassCache => false;
    public string CacheKey => "imported-user:" + Id;
    public TimeSpan Expiration => TimeSpan.FromMinutes(1);
}

internal sealed record CreateUserCommand(string IdempotencyKey) : ICommand<Result<UserDto, NotFoundError>>, IIdempotentRequest
{
    IdempotencyScope? IIdempotentRequest.IdempotencyScope => IdempotencyScope.Global;
}

internal sealed record LockUserByKeyCommand(string IdempotencyKey) : ICommand<Result<UserDto, NotFoundError, UserErrors.Locked>>, IIdempotentRequest
{
    IdempotencyScope? IIdempotentRequest.IdempotencyScope => IdempotencyScope.Global;
}

internal sealed record ImportUserCommand(string IdempotencyKey) : ICommand<Result<UserDto, InvalidOperationException>>, IIdempotentRequest
{
    IdempotencyScope? IIdempotentRequest.IdempotencyScope => IdempotencyScope.Global;
}

/// <summary>
/// The cache and the idempotency store replay a typed response through its JSON envelope: the replayed result holds the same
/// slot and error type as the one the handler returned, including a nested error type and a struct error.
/// </summary>
public sealed class TypedResultReplayTests
{
    private int _runs;

    private ValueTask<TResponse> Run<TResponse>(TResponse result)
    {
        _runs++;
        return new ValueTask<TResponse>(result);
    }

    public static TheoryData<int> Cases => [0, 1, 2, 3];

    private static Result<UserDto, NotFoundError, UserErrors.Locked, RateLimited> Case(int index) => index switch
    {
        0 => new UserDto(3),
        1 => UserErrors.NotFound,
        2 => new UserErrors.Locked(7),
        _ => new RateLimited("Rate.Limited"),
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task CachingBehavior_Should_ReplayTheSameCase_When_TheResponseIsCached(int index)
    {
        CachingBehavior<GetCachedUserQuery, Result<UserDto, NotFoundError, UserErrors.Locked, RateLimited>> behavior =
            new(NullLogger<CachingBehavior<GetCachedUserQuery, Result<UserDto, NotFoundError, UserErrors.Locked, RateLimited>>>.Instance, new DictionaryCache());
        Result<UserDto, NotFoundError, UserErrors.Locked, RateLimited> expected = Case(index);

        Result<UserDto, NotFoundError, UserErrors.Locked, RateLimited> first = await behavior.Handle(new GetCachedUserQuery(1), (_, _) => Run(expected), default);
        Result<UserDto, NotFoundError, UserErrors.Locked, RateLimited> replayed = await behavior.Handle(new GetCachedUserQuery(1), (_, _) => Run<Result<UserDto, NotFoundError, UserErrors.Locked, RateLimited>>(new UserDto(99)), default);

        _runs.Should().Be(1);
        AssertSameCase(replayed, first);
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task IdempotencyBehavior_Should_ReplayTheSameCase_When_TheKeyIsRetried(int index)
    {
        StaticOptionsMonitor<IdempotencyExecutorOptions> monitor = new(new IdempotencyExecutorOptions());
        IdempotencyExecutor executor = new(new InMemoryIdempotencyRecordStore(), monitor);
        IdempotencyBehavior<LockUserCommand, Result<UserDto, NotFoundError, UserErrors.Locked, RateLimited>> behavior =
            new(executor, monitor, new IdempotencyBehaviorOptions(), NullLogger<IdempotencyBehavior<LockUserCommand, Result<UserDto, NotFoundError, UserErrors.Locked, RateLimited>>>.Instance);
        Result<UserDto, NotFoundError, UserErrors.Locked, RateLimited> expected = Case(index);

        Result<UserDto, NotFoundError, UserErrors.Locked, RateLimited> first = await behavior.Handle(new LockUserCommand(1), (_, _) => Run(expected), default);
        Result<UserDto, NotFoundError, UserErrors.Locked, RateLimited> replayed = await behavior.Handle(new LockUserCommand(1), (_, _) => Run<Result<UserDto, NotFoundError, UserErrors.Locked, RateLimited>>(new UserDto(99)), default);

        _runs.Should().Be(1);
        AssertSameCase(replayed, first);
    }

    [Fact]
    public async Task CachingBehavior_Should_NotCacheATypedFailure_When_CacheFailuresIsOff()
    {
        await TwiceThroughCacheAsync(new GetUserByIdQuery(1), (Result<UserDto, NotFoundError>)UserErrors.NotFound);
        await TwiceThroughCacheAsync(new GetLockableUserQuery(1), (Result<UserDto, NotFoundError, UserErrors.Locked>)new UserErrors.Locked(1));

        _runs.Should().Be(4);
    }

    [Fact]
    public async Task CachingBehavior_Should_CacheATypedSuccess()
    {
        (Result<UserDto, NotFoundError> one, _) = await TwiceThroughCacheAsync(new GetUserByIdQuery(2), (Result<UserDto, NotFoundError>)new UserDto(2));
        (Result<UserDto, NotFoundError, UserErrors.Locked> two, _) =
            await TwiceThroughCacheAsync(new GetLockableUserQuery(2), (Result<UserDto, NotFoundError, UserErrors.Locked>)new UserDto(2));

        _runs.Should().Be(2);
        one.TryGetValue(out UserDto first).Should().BeTrue();
        first.Id.Should().Be(2);
        two.TryGetValue(out UserDto second).Should().BeTrue();
        second.Id.Should().Be(2);
    }

    [Fact]
    public async Task CachingBehavior_Should_ReplayTheTypedFailure_When_CacheFailuresIsOn()
    {
        (Result<UserDto, NotFoundError> one, _) = await TwiceThroughCacheAsync(new GetUserByIdQuery(3, CacheFailures: true), (Result<UserDto, NotFoundError>)UserErrors.NotFound);
        (Result<UserDto, NotFoundError, UserErrors.Locked> two, _) =
            await TwiceThroughCacheAsync(new GetLockableUserQuery(3, CacheFailures: true), (Result<UserDto, NotFoundError, UserErrors.Locked>)new UserErrors.Locked(3));

        _runs.Should().Be(2);
        one.TryGetValue(out NotFoundError notFound).Should().BeTrue();
        notFound.Code.Should().Be(UserErrors.Codes.NotFound);
        two.TryGetValue(out UserErrors.Locked locked).Should().BeTrue();
        locked.Id.Should().Be(3);
    }

    [Fact]
    public async Task CachingBehavior_Should_NotThrow_When_AFailureHoldsAnException()
    {
        (Result<UserDto, InvalidOperationException> replayed, _) = await TwiceThroughCacheAsync(new GetImportedUserQuery(1), (Result<UserDto, InvalidOperationException>)Thrown());

        _runs.Should().Be(2);
        replayed.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task CachingBehavior_Should_Throw_When_AFailureHoldingAnExceptionIsCached()
    {
        Func<Task> act = () => TwiceThroughCacheAsync(new GetImportedUserQuery(2, CacheFailures: true), (Result<UserDto, InvalidOperationException>)Thrown());

        await act.Should().ThrowExactlyAsync<NotSupportedException>();
    }

    [Fact]
    public async Task IdempotencyBehavior_Should_ReplayTheTypedFailure_When_StoreFailuresIsOnByDefault()
    {
        (Result<UserDto, NotFoundError> one, _) = await TwiceThroughIdempotencyAsync(new CreateUserCommand("create-1"), (Result<UserDto, NotFoundError>)UserErrors.NotFound);
        (Result<UserDto, NotFoundError, UserErrors.Locked> two, _) =
            await TwiceThroughIdempotencyAsync(new LockUserByKeyCommand("lock-1"), (Result<UserDto, NotFoundError, UserErrors.Locked>)new UserErrors.Locked(4));

        _runs.Should().Be(2);
        one.TryGetValue(out NotFoundError notFound).Should().BeTrue();
        notFound.Code.Should().Be(UserErrors.Codes.NotFound);
        two.TryGetValue(out UserErrors.Locked locked).Should().BeTrue();
        locked.Id.Should().Be(4);
    }

    [Fact]
    public async Task IdempotencyBehavior_Should_ReplayTheTypedSuccess()
    {
        (Result<UserDto, NotFoundError> one, _) = await TwiceThroughIdempotencyAsync(new CreateUserCommand("create-2"), (Result<UserDto, NotFoundError>)new UserDto(5));
        (Result<UserDto, NotFoundError, UserErrors.Locked> two, _) =
            await TwiceThroughIdempotencyAsync(new LockUserByKeyCommand("lock-2"), (Result<UserDto, NotFoundError, UserErrors.Locked>)new UserDto(6));

        _runs.Should().Be(2);
        one.TryGetValue(out UserDto first).Should().BeTrue();
        first.Id.Should().Be(5);
        two.TryGetValue(out UserDto second).Should().BeTrue();
        second.Id.Should().Be(6);
    }

    [Fact]
    public async Task IdempotencyBehavior_Should_RunTheHandlerAgain_When_StoreFailuresIsOff()
    {
        IdempotencyBehaviorOptions options = new() { StoreFailures = false };

        await TwiceThroughIdempotencyAsync(new CreateUserCommand("create-3"), (Result<UserDto, NotFoundError>)UserErrors.NotFound, options);
        await TwiceThroughIdempotencyAsync(new LockUserByKeyCommand("lock-3"), (Result<UserDto, NotFoundError, UserErrors.Locked>)new UserErrors.Locked(7), options);

        _runs.Should().Be(4);
    }

    [Fact]
    public async Task IdempotencyBehavior_Should_ThrowEnhancedValidationException_When_TheKeyIsMissing()
    {
        Func<Task> one = () => TwiceThroughIdempotencyAsync(new CreateUserCommand(""), (Result<UserDto, NotFoundError>)new UserDto(1));
        Func<Task> two = () => TwiceThroughIdempotencyAsync(new LockUserByKeyCommand(""), (Result<UserDto, NotFoundError, UserErrors.Locked>)new UserDto(1));

        (await one.Should().ThrowExactlyAsync<EnhancedValidationException>()).Which.Errors.Should().ContainSingle(e => e.Code == IdempotencyErrorCodes.KeyMissing);
        (await two.Should().ThrowExactlyAsync<EnhancedValidationException>()).Which.Errors.Should().ContainSingle(e => e.Code == IdempotencyErrorCodes.KeyMissing);
        _runs.Should().Be(0);
    }

    [Fact]
    public async Task IdempotencyBehavior_Should_KeepTheKeyInFlight_When_AStoredFailureHoldsAnException()
    {
        StaticOptionsMonitor<IdempotencyExecutorOptions> monitor = new(new IdempotencyExecutorOptions());
        IdempotencyBehavior<ImportUserCommand, Result<UserDto, InvalidOperationException>> behavior = new(
            new IdempotencyExecutor(new InMemoryIdempotencyRecordStore(), monitor),
            monitor,
            new IdempotencyBehaviorOptions(),
            NullLogger<IdempotencyBehavior<ImportUserCommand, Result<UserDto, InvalidOperationException>>>.Instance);
        InvalidOperationException exception = Thrown();

        Result<UserDto, InvalidOperationException> first = await behavior.Handle(new ImportUserCommand("import-1"), (_, _) => Run((Result<UserDto, InvalidOperationException>)exception), default);
        Func<Task> retry = () => behavior.Handle(new ImportUserCommand("import-1"), (_, _) => Run((Result<UserDto, InvalidOperationException>)exception), default).AsTask();

        first.TryGetValue(out InvalidOperationException error).Should().BeTrue();
        error.Should().BeSameAs(exception);
        (await retry.Should().ThrowExactlyAsync<EnhancedValidationException>()).Which.Errors.Should().ContainSingle(e => e.Code == IdempotencyErrorCodes.InFlight);
        _runs.Should().Be(1);
    }

    [Fact]
    public async Task IdempotencyBehavior_Should_ReleaseTheKey_When_AFailureHoldsAnExceptionAndStoreFailuresIsOff()
    {
        (Result<UserDto, InvalidOperationException> replayed, _) = await TwiceThroughIdempotencyAsync(
            new ImportUserCommand("import-2"), (Result<UserDto, InvalidOperationException>)Thrown(), new IdempotencyBehaviorOptions { StoreFailures = false });

        _runs.Should().Be(2);
        replayed.IsFailure.Should().BeTrue();
    }

    private static InvalidOperationException Thrown()
    {
        try
        {
            throw new InvalidOperationException("Import failed.");
        }
        catch (InvalidOperationException exception)
        {
            return exception;
        }
    }

    private async Task<(TResponse Replayed, TResponse First)> TwiceThroughCacheAsync<TRequest, TResponse>(TRequest request, TResponse result)
        where TRequest : ICacheable, IMessage
    {
        CachingBehavior<TRequest, TResponse> behavior = new(NullLogger<CachingBehavior<TRequest, TResponse>>.Instance, new DictionaryCache());
        TResponse first = await behavior.Handle(request, (_, _) => Run(result), default);
        TResponse replayed = await behavior.Handle(request, (_, _) => Run(result), default);
        return (replayed, first);
    }

    private async Task<(TResponse Replayed, TResponse First)> TwiceThroughIdempotencyAsync<TRequest, TResponse>(
        TRequest request,
        TResponse result,
        IdempotencyBehaviorOptions? options = null)
        where TRequest : IIdempotentRequest, IMessage
    {
        StaticOptionsMonitor<IdempotencyExecutorOptions> monitor = new(new IdempotencyExecutorOptions());
        IdempotencyExecutor executor = new(new InMemoryIdempotencyRecordStore(), monitor);
        IdempotencyBehavior<TRequest, TResponse> behavior =
            new(executor, monitor, options ?? new IdempotencyBehaviorOptions(), NullLogger<IdempotencyBehavior<TRequest, TResponse>>.Instance);
        TResponse first = await behavior.Handle(request, (_, _) => Run(result), default);
        TResponse replayed = await behavior.Handle(request, (_, _) => Run(result), default);
        return (replayed, first);
    }

    private static void AssertSameCase(
        Result<UserDto, NotFoundError, UserErrors.Locked, RateLimited> replayed,
        Result<UserDto, NotFoundError, UserErrors.Locked, RateLimited> original)
    {
        string described = original.Match(v => "dto" + v.Id, e => e.GetType().Name + e.Code, e => e.GetType().Name + e.Code + e.Id, e => "rate" + e.Code);
        replayed.Match(v => "dto" + v.Id, e => e.GetType().Name + e.Code, e => e.GetType().Name + e.Code + e.Id, e => "rate" + e.Code)
            .Should().Be(described);
    }

    private sealed class DictionaryCache : IDistributedCache
    {
        private readonly ConcurrentDictionary<string, byte[]> _entries = new();

        public byte[]? Get(string key) => _entries.TryGetValue(key, out byte[]? value) ? value : null;

        public Task<byte[]?> GetAsync(string key, CancellationToken token = default) => Task.FromResult(Get(key));

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => _entries[key] = value;

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            Set(key, value, options);
            return Task.CompletedTask;
        }

        public void Refresh(string key)
        {
        }

        public Task RefreshAsync(string key, CancellationToken token = default) => Task.CompletedTask;

        public void Remove(string key) => _entries.TryRemove(key, out _);

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            Remove(key);
            return Task.CompletedTask;
        }
    }

    private sealed class StaticOptionsMonitor<TOptions>(TOptions value) : IOptionsMonitor<TOptions>
    {
        public TOptions CurrentValue => value;

        public TOptions Get(string? name) => value;

        public IDisposable? OnChange(Action<TOptions, string?> listener) => null;
    }
}
