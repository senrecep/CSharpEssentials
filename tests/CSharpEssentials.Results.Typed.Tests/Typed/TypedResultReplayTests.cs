using System.Collections.Concurrent;
using CSharpEssentials.Errors;
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

/// <summary>
/// The cache and the idempotency store replay a typed response through its JSON envelope: the replayed result holds the same
/// slot and error type as the one the handler returned, including a nested error type and a struct error.
/// </summary>
public sealed class TypedResultReplayTests
{
    private int _runs;

    private ValueTask<Result<UserDto, NotFoundError, UserErrors.Locked, RateLimited>> Run(Result<UserDto, NotFoundError, UserErrors.Locked, RateLimited> result)
    {
        _runs++;
        return new ValueTask<Result<UserDto, NotFoundError, UserErrors.Locked, RateLimited>>(result);
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
        Result<UserDto, NotFoundError, UserErrors.Locked, RateLimited> replayed = await behavior.Handle(new GetCachedUserQuery(1), (_, _) => Run(new UserDto(99)), default);

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
        Result<UserDto, NotFoundError, UserErrors.Locked, RateLimited> replayed = await behavior.Handle(new LockUserCommand(1), (_, _) => Run(new UserDto(99)), default);

        _runs.Should().Be(1);
        AssertSameCase(replayed, first);
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
