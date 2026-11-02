using Mediator;
using System.Diagnostics.CodeAnalysis;
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

        byte[]? cachedBytes = await cache.GetAsync(message.CacheKey, cancellationToken).ConfigureAwait(false);
        if (cachedBytes is not null && cachedBytes.Length > 0)
        {
            if (TryDeserialize(cachedBytes, out TResponse? cached))
            {
                LogCacheHit(logger, message.CacheKey);
                return cached;
            }

            LogCacheEntryUnreadable(logger, message.CacheKey);
            await cache.RemoveAsync(message.CacheKey, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            LogCacheMiss(logger, message.CacheKey);
        }

        TResponse result = await next(message, cancellationToken).ConfigureAwait(false);

        if (!message.CacheFailures && result is IResultBase r && r.IsFailure)
            return result;

        byte[] serialized = JsonSerializer.SerializeToUtf8Bytes(result);
        var options = new DistributedCacheEntryOptions();
        if (message.Expiration > TimeSpan.Zero)
            options.AbsoluteExpirationRelativeToNow = message.Expiration;

        await cache.SetAsync(message.CacheKey, serialized, options, cancellationToken).ConfigureAwait(false);
        LogCacheSet(logger, message.CacheKey);

        return result;
    }

    private static bool TryDeserialize(byte[] bytes, [NotNullWhen(true)] out TResponse? value)
    {
        try
        {
            value = JsonSerializer.Deserialize<TResponse>(bytes);
            return value is not null;
        }
        catch (JsonException)
        {
            value = default;
            return false;
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Cached value for key {CacheKey} could not be deserialized; removing it and calling the handler")]
    private static partial void LogCacheEntryUnreadable(ILogger logger, string cacheKey);

    [LoggerMessage(Level = LogLevel.Information, Message = "Cache hit for key {CacheKey}")]
    private static partial void LogCacheHit(ILogger logger, string cacheKey);

    [LoggerMessage(Level = LogLevel.Information, Message = "Cache miss for key {CacheKey}")]
    private static partial void LogCacheMiss(ILogger logger, string cacheKey);

    [LoggerMessage(Level = LogLevel.Information, Message = "Cache set for key {CacheKey}")]
    private static partial void LogCacheSet(ILogger logger, string cacheKey);
}
