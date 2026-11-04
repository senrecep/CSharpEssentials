using Mediator;
using System.Text.Json;

using CSharpEssentials.ResultPattern.Interfaces;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Mediator;

public sealed partial class CachingBehavior<TRequest, TResponse>(
    ILogger<CachingBehavior<TRequest, TResponse>> logger,
    IDistributedCache cache)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheable, IMessage
{
    public async ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (message.BypassCache)
            return await next(message, cancellationToken).ConfigureAwait(false);

        string cacheKey = message.CacheKey;
        byte[]? cachedBytes = await cache.GetAsync(cacheKey, cancellationToken).ConfigureAwait(false);
        if (cachedBytes is not null && cachedBytes.Length > 0)
        {
            LogCacheHit(logger, cacheKey);
            TResponse? deserialized = JsonSerializer.Deserialize<TResponse>(cachedBytes);
            return deserialized ?? throw new InvalidOperationException($"Failed to deserialize cached value for key {cacheKey}");
        }

        LogCacheMiss(logger, cacheKey);

        TResponse result = await next(message, cancellationToken).ConfigureAwait(false);

        if (!message.CacheFailures && result is IResultBase r && r.IsFailure)
            return result;

        byte[] serialized = JsonSerializer.SerializeToUtf8Bytes(result);
        var options = new DistributedCacheEntryOptions();
        if (message.Expiration > TimeSpan.Zero)
            options.AbsoluteExpirationRelativeToNow = message.Expiration;

        await cache.SetAsync(cacheKey, serialized, options, cancellationToken).ConfigureAwait(false);
        LogCacheSet(logger, cacheKey);

        return result;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Cache hit for key {CacheKey}")]
    private static partial void LogCacheHit(ILogger logger, string cacheKey);

    [LoggerMessage(Level = LogLevel.Information, Message = "Cache miss for key {CacheKey}")]
    private static partial void LogCacheMiss(ILogger logger, string cacheKey);

    [LoggerMessage(Level = LogLevel.Information, Message = "Cache set for key {CacheKey}")]
    private static partial void LogCacheSet(ILogger logger, string cacheKey);
}
