using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Mediator.Benchmarks;

// https://github.com/jbogard/MediatR/wiki/Behaviors
// https://learn.microsoft.com/aspnet/core/performance/caching/distributed#distributed-memory-cache
// https://learn.microsoft.com/dotnet/core/extensions/logger-message-generator
public sealed partial class MediatRCachingBehavior(ILogger<MediatRCachingBehavior> logger, IDistributedCache cache) : IPipelineBehavior<GetProduct, ProductView>
{
    public async Task<ProductView> Handle(GetProduct request, RequestHandlerDelegate<ProductView> next, CancellationToken cancellationToken)
    {
        string key = request.CacheKey;
        byte[]? cached = await cache.GetAsync(key, cancellationToken).ConfigureAwait(false);
        if (cached is { Length: > 0 })
        {
            LogCacheHit(logger, key);
            return JsonSerializer.Deserialize<ProductView>(cached) ?? throw new InvalidOperationException($"Failed to deserialize cached value for key {key}");
        }

        LogCacheMiss(logger, key);
        ProductView result = await next(cancellationToken).ConfigureAwait(false);
        byte[] serialized = JsonSerializer.SerializeToUtf8Bytes(result);
        await cache.SetAsync(key, serialized, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = request.Expiration }, cancellationToken).ConfigureAwait(false);
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
