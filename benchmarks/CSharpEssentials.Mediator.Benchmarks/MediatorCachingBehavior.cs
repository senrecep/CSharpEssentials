using System.Text.Json;
using Mediator;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Mediator.Benchmarks;

// https://github.com/martinothamar/Mediator/blob/v3.0.2/README.md#44-use-pipeline-behaviors
// https://learn.microsoft.com/aspnet/core/performance/caching/distributed#distributed-memory-cache
// https://learn.microsoft.com/dotnet/core/extensions/logger-message-generator
public sealed partial class MediatorCachingBehavior(ILogger<MediatorCachingBehavior> logger, IDistributedCache cache) : IPipelineBehavior<GetProduct, ProductView>
{
    public async ValueTask<ProductView> Handle(GetProduct message, MessageHandlerDelegate<GetProduct, ProductView> next, CancellationToken cancellationToken)
    {
        string key = message.CacheKey;
        byte[]? cached = await cache.GetAsync(key, cancellationToken).ConfigureAwait(false);
        if (cached is { Length: > 0 })
        {
            LogCacheHit(logger, key);
            return JsonSerializer.Deserialize<ProductView>(cached) ?? throw new InvalidOperationException($"Failed to deserialize cached value for key {key}");
        }

        LogCacheMiss(logger, key);
        ProductView result = await next(message, cancellationToken).ConfigureAwait(false);
        byte[] serialized = JsonSerializer.SerializeToUtf8Bytes(result);
        await cache.SetAsync(key, serialized, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = message.Expiration }, cancellationToken).ConfigureAwait(false);
        LogCacheSet(logger, key);
        return result;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Cache hit for key {CacheKey}")]
    private static partial void LogCacheHit(ILogger logger, string cacheKey);

    [LoggerMessage(Level = LogLevel.Information, Message = "Cache miss for key {CacheKey}")]
    private static partial void LogCacheMiss(ILogger logger, string cacheKey);

    [LoggerMessage(Level = LogLevel.Information, Message = "Cache set for key {CacheKey}")]
    private static partial void LogCacheSet(ILogger logger, string cacheKey);
}
