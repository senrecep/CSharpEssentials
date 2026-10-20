using CSharpEssentials.AspNetCore.OpenApi;
using Microsoft.AspNetCore.OpenApi;

namespace CSharpEssentials.AspNetCore;

/// <summary>Describes <c>Idempotency-Key</c> handling in the documents of Microsoft.AspNetCore.OpenApi.</summary>
public static class OpenApiIdempotencyExtensions
{
    /// <summary>
    /// Describes every endpoint marked with <c>.WithIdempotency()</c> or <see cref="IdempotentAttribute"/> whose effective
    /// mode is <see cref="CSharpEssentials.Idempotency.IdempotencyMode.Enforce"/> and whose method is one of
    /// <see cref="IdempotencyOptions.Methods"/>: the key header (required when the endpoint's <c>RequireKey</c> is on), the
    /// <c>Idempotency-Replayed</c> header on its 2xx responses, and the <c>400</c>, <c>409</c> and <c>422</c> problem
    /// responses with their <c>code</c> values. The <c>409</c> carries <c>Idempotency.InFlight</c> and <c>Retry-After</c>; an
    /// operation that already has a <c>409</c> keeps its own description, which the idempotency one follows. Other endpoints
    /// are not changed. The values come from <see cref="IdempotencyOptions"/> and the endpoint's profile when the document is built.
    /// </summary>
    /// <example><code>services.AddOpenApi(options => options.AddIdempotencyConventions());</code></example>
    /// <param name="options">The options of one OpenAPI document.</param>
    /// <returns><paramref name="options"/>.</returns>
    public static OpenApiOptions AddIdempotencyConventions(this OpenApiOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.AddOperationTransformer<IdempotencyOperationTransformer>();
        return options;
    }
}
