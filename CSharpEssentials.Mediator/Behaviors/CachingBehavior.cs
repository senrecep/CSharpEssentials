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

        string cacheKey = message.CacheKey;
        byte[]? cachedBytes = await cache.GetAsync(cacheKey, cancellationToken).ConfigureAwait(false);
        if (cachedBytes is not null && cachedBytes.Length > 0)
        {
            if (TryDeserialize(cachedBytes, out TResponse? cached))
            {
                LogCacheHit(logger, cacheKey);
                return cached;
            }

            LogCacheEntryUnreadable(logger, cacheKey);
            await cache.RemoveAsync(cacheKey, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            LogCacheMiss(logger, cacheKey);
        }

        TResponse result = await next(message, cancellationToken).ConfigureAwait(false);

#if NET10_0_OR_GREATER
        if (!message.CacheFailures && result is IResultBase r && r.IsFailure)
#else
        if (!message.CacheFailures && ResponseFailure.IsFailure(result))
#endif
            return result;

        if (!TrySerialize(result, out byte[]? serialized))
        {
            LogCacheValueUnserializable(logger, cacheKey);
            return result;
        }

        var options = new DistributedCacheEntryOptions();
        if (message.Expiration > TimeSpan.Zero)
            options.AbsoluteExpirationRelativeToNow = message.Expiration;

        await cache.SetAsync(cacheKey, serialized, options, cancellationToken).ConfigureAwait(false);
        LogCacheSet(logger, cacheKey);

        return result;
    }

    private static bool TrySerialize(TResponse value, [NotNullWhen(true)] out byte[]? bytes)
    {
        try
        {
            bytes = JsonSerializer.SerializeToUtf8Bytes(value);
            return true;
        }
        catch (Exception exception) when (exception is NotSupportedException or JsonException)
        {
            bytes = null;
            return false;
        }
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

    [LoggerMessage(Level = LogLevel.Warning, Message = "The response for key {CacheKey} could not be serialized; it is returned without caching")]
    private static partial void LogCacheValueUnserializable(ILogger logger, string cacheKey);

    [LoggerMessage(Level = LogLevel.Information, Message = "Cache hit for key {CacheKey}")]
    private static partial void LogCacheHit(ILogger logger, string cacheKey);

    [LoggerMessage(Level = LogLevel.Information, Message = "Cache miss for key {CacheKey}")]
    private static partial void LogCacheMiss(ILogger logger, string cacheKey);

    [LoggerMessage(Level = LogLevel.Information, Message = "Cache set for key {CacheKey}")]
    private static partial void LogCacheSet(ILogger logger, string cacheKey);
}
