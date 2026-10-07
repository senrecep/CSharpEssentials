using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Distributed;

namespace CSharpEssentials.Tests.AspNetCore.Idempotency;

/// <summary>
/// <see cref="IDistributedCache"/> with absolute expiration on a <c>TimeProvider</c>.
/// </summary>
internal sealed class FakeDistributedCache(TimeProvider timeProvider) : IDistributedCache
{
    private readonly ConcurrentDictionary<string, (byte[] Value, DateTimeOffset? ExpiresAt)> _entries = new();

    public IReadOnlyCollection<string> Keys => [.. _entries.Keys];

    public byte[]? Get(string key) =>
        _entries.TryGetValue(key, out (byte[] Value, DateTimeOffset? ExpiresAt) entry)
        && (entry.ExpiresAt is null || entry.ExpiresAt > timeProvider.GetUtcNow())
            ? entry.Value
            : null;

    public Task<byte[]?> GetAsync(string key, CancellationToken token = default) => Task.FromResult(Get(key));

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options) =>
        _entries[key] = (value, options.AbsoluteExpirationRelativeToNow is { } ttl ? timeProvider.GetUtcNow() + ttl : null);

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
