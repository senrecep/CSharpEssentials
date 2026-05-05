using Mediator;
using System.Text;
using System.Text.Json;

using CSharpEssentials.Errors;
using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;

using FluentAssertions;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

using Moq;

namespace CSharpEssentials.Tests.Mediator;

public sealed record TestCacheableQuery(string Id) : IQuery<Result<string>>, ICacheable
{
    public string CacheKey => $"test:{Id}";
    public TimeSpan Expiration => TimeSpan.FromMinutes(5);
    public bool BypassCache => false;
    public bool CacheFailures => false;
}

public sealed record TestCacheableQueryWithFailureCache(string Id) : IQuery<Result<string>>, ICacheable
{
    public string CacheKey => $"test:{Id}";
    public TimeSpan Expiration => TimeSpan.FromMinutes(5);
    public bool BypassCache => false;
    public bool CacheFailures => true;
}

public sealed record TestBypassCacheQuery(string Id) : IQuery<Result<string>>, ICacheable
{
    public string CacheKey => $"test:{Id}";
    public TimeSpan Expiration => TimeSpan.FromMinutes(5);
    public bool BypassCache => true;
    public bool CacheFailures => false;
}

public sealed record TestZeroExpirationQuery(string Id) : IQuery<Result<string>>, ICacheable
{
    public string CacheKey => $"test:{Id}";
    public TimeSpan Expiration => TimeSpan.Zero;
    public bool BypassCache => false;
    public bool CacheFailures => false;
}

public class CachingBehaviorTests
{
    private static readonly MessageHandlerDelegate<TestCacheableQuery, Result<string>> SuccessNext =
        (message, ct) => new ValueTask<Result<string>>(Result.Success<string>("from-handler"));

    private static readonly MessageHandlerDelegate<TestCacheableQuery, Result<string>> FailureNext =
        (message, ct) => new ValueTask<Result<string>>(Result.Failure<string>(Error.NotFound("NotFound", "Not found")));

    private static readonly MessageHandlerDelegate<TestCacheableQueryWithFailureCache, Result<string>> FailureNextWithFailureCache =
        (message, ct) => new ValueTask<Result<string>>(Result.Failure<string>(Error.NotFound("NotFound", "Not found")));

    [Fact]
    public async Task Handle_Should_Return_Cached_Value_On_Cache_Hit()
    {
        var logger = new Mock<ILogger<CachingBehavior<TestCacheableQuery, Result<string>>>>();
        var cache = new Mock<IDistributedCache>();
        byte[] cachedBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Result.Success<string>("cached")));
        cache.Setup(c => c.GetAsync("test:1", It.IsAny<CancellationToken>())).ReturnsAsync(cachedBytes);

        var behavior = new CachingBehavior<TestCacheableQuery, Result<string>>(logger.Object, cache.Object);
        var query = new TestCacheableQuery("1");

        Result<string> result = await behavior.Handle(query, SuccessNext, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("cached");
        cache.Verify(c => c.GetAsync("test:1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Call_Handler_And_Set_Cache_On_Miss()
    {
        var logger = new Mock<ILogger<CachingBehavior<TestCacheableQuery, Result<string>>>>();
        var cache = new Mock<IDistributedCache>();
        cache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((byte[]?)null);

        var behavior = new CachingBehavior<TestCacheableQuery, Result<string>>(logger.Object, cache.Object);
        var query = new TestCacheableQuery("1");

        Result<string> result = await behavior.Handle(query, SuccessNext, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("from-handler");
        cache.Verify(c => c.SetAsync("test:1", It.IsAny<byte[]>(), It.Is<DistributedCacheEntryOptions>(o => o.AbsoluteExpirationRelativeToNow == TimeSpan.FromMinutes(5)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Not_Cache_Failures_When_CacheFailures_Is_False()
    {
        var logger = new Mock<ILogger<CachingBehavior<TestCacheableQuery, Result<string>>>>();
        var cache = new Mock<IDistributedCache>();
        cache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((byte[]?)null);

        var behavior = new CachingBehavior<TestCacheableQuery, Result<string>>(logger.Object, cache.Object);
        var query = new TestCacheableQuery("1");

        Result<string> result = await behavior.Handle(query, FailureNext, default);

        result.IsFailure.Should().BeTrue();
        cache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Skip_Cache_Lookup_When_BypassCache_Is_True()
    {
        var logger = new Mock<ILogger<CachingBehavior<TestBypassCacheQuery, Result<string>>>>();
        var cache = new Mock<IDistributedCache>();

        var behavior = new CachingBehavior<TestBypassCacheQuery, Result<string>>(logger.Object, cache.Object);
        var query = new TestBypassCacheQuery("1");

        Result<string> result = await behavior.Handle(
            query,
            (_, _) => new ValueTask<Result<string>>(Result.Success<string>("from-handler")),
            default);

        result.Value.Should().Be("from-handler");
        cache.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        cache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Not_Set_AbsoluteExpiration_When_Expiration_Is_Zero()
    {
        var logger = new Mock<ILogger<CachingBehavior<TestZeroExpirationQuery, Result<string>>>>();
        var cache = new Mock<IDistributedCache>();
        cache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((byte[]?)null);

        var behavior = new CachingBehavior<TestZeroExpirationQuery, Result<string>>(logger.Object, cache.Object);
        var query = new TestZeroExpirationQuery("1");

        await behavior.Handle(
            query,
            (_, _) => new ValueTask<Result<string>>(Result.Success<string>("value")),
            default);

        cache.Verify(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.Is<DistributedCacheEntryOptions>(o => o.AbsoluteExpirationRelativeToNow == null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Cache_Failures_When_CacheFailures_Is_True()
    {
        var logger = new Mock<ILogger<CachingBehavior<TestCacheableQueryWithFailureCache, Result<string>>>>();
        var cache = new Mock<IDistributedCache>();
        cache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((byte[]?)null);

        var behavior = new CachingBehavior<TestCacheableQueryWithFailureCache, Result<string>>(logger.Object, cache.Object);
        var query = new TestCacheableQueryWithFailureCache("1");

        Result<string> result = await behavior.Handle(query, FailureNextWithFailureCache, default);

        result.IsFailure.Should().BeTrue();
        cache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
