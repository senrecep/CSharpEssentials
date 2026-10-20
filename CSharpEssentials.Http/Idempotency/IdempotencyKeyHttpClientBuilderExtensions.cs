#if NET9_0_OR_GREATER
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Http;

/// <summary>Registers <see cref="IdempotencyKeyHandler"/> on an <see cref="IHttpClientBuilder"/>.</summary>
public static class IdempotencyKeyHttpClientBuilderExtensions
{
    /// <summary>
    /// Adds <see cref="IdempotencyKeyHandler"/>, which gives POST and PATCH requests without an <c>Idempotency-Key</c>
    /// a generated one. Call it before <c>AddStandardResilienceHandler</c> (or any other retry handler) so the handler
    /// sits outside the retries and every retry sends the same key. Not available on netstandard2.1.
    /// </summary>
    /// <param name="builder">The client builder.</param>
    /// <returns>The same builder.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/>.</exception>
    public static IHttpClientBuilder AddIdempotencyKeys(this IHttpClientBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddHttpMessageHandler(static () => new IdempotencyKeyHandler());
    }
}
#endif
