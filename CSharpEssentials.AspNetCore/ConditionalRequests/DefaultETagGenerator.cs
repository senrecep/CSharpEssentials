using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using HttpJsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// <see cref="IVersioned"/> gives a strong ETag; with <see cref="ConditionalRequestOptions.UseBodyHashFallback"/> any other
/// buffered value gets a weak ETag of its JSON (host <see cref="HttpJsonOptions"/>, also for MVC); otherwise no validators.
/// Streamed or deferred values (<see cref="IAsyncEnumerable{T}"/>, <see cref="Stream"/>, <see cref="PipeReader"/>,
/// <see cref="IQueryable"/>) are never hashed, so they are not enumerated or run twice.
/// </summary>
internal sealed class DefaultETagGenerator(ConditionalRequestOptions options, IOptions<HttpJsonOptions> jsonOptions) : IETagGenerator
{
    private static readonly ConcurrentDictionary<Type, bool> IsAsyncEnumerableByType = new();

    public ResourceValidators? GetValidators(object value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value is IVersioned versioned)
            return EntityTags.FromVersion(versioned.Version) is { } etag ? new ResourceValidators(etag, null) : null;
        if (!options.UseBodyHashFallback || !IsBuffered(value))
            return null;

        JsonSerializerOptions serializerOptions = jsonOptions.Value.SerializerOptions;
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(value, serializerOptions.GetTypeInfo(value.GetType()));
        return new ResourceValidators(new EntityTagHeaderValue($"\"{EntityTags.Hash(json)}\"", isWeak: true), null);
    }

    private static bool IsBuffered(object value) =>
        value is not (Stream or PipeReader or IQueryable)
        && !IsAsyncEnumerableByType.GetOrAdd(value.GetType(), static type => Array.Exists(
            type.GetInterfaces(),
            static i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>)));
}
