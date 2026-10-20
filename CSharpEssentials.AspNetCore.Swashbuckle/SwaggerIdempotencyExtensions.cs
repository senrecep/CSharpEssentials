using CSharpEssentials.AspNetCore.Swagger.Filters;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CSharpEssentials.AspNetCore;

/// <summary>Describes <c>Idempotency-Key</c> handling in Swashbuckle documents.</summary>
public static class SwaggerIdempotencyExtensions
{
    /// <summary>
    /// Describes every endpoint marked with <c>.WithIdempotency()</c> or <see cref="IdempotentAttribute"/> whose effective
    /// mode is <see cref="CSharpEssentials.Idempotency.IdempotencyMode.Enforce"/> and whose method is one of
    /// <see cref="IdempotencyOptions.Methods"/>: the key header (required when the endpoint's <c>RequireKey</c> is on), the
    /// <c>Idempotency-Replayed</c> header on its 2xx responses, and the <c>400</c>, <c>409</c> and <c>422</c> problem
    /// responses with their <c>code</c> values. The <c>409</c> carries <c>Idempotency.InFlight</c> and <c>Retry-After</c>; an
    /// operation that already has a <c>409</c> keeps its own description, which the idempotency one follows. Other endpoints
    /// are not changed. The values come from <see cref="IdempotencyOptions"/> and the endpoint's profile when the document is built.
    /// Calling it twice adds the filter once. <c>AddSwagger</c> calls it.
    /// </summary>
    /// <param name="options">The Swashbuckle options.</param>
    /// <returns><paramref name="options"/>.</returns>
    public static SwaggerGenOptions AddIdempotencyConventions(this SwaggerGenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (!options.OperationFilterDescriptors.Exists(static descriptor => descriptor.Type == typeof(IdempotencyOperationFilter)))
            options.OperationFilter<IdempotencyOperationFilter>();
        return options;
    }
}
