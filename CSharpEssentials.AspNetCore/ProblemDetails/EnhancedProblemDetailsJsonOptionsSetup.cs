using System.Text.Json;
using Microsoft.Extensions.Options;
using HttpJsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Registered by <c>AddEnhancedProblemDetails</c>: appends <see cref="EnhancedProblemDetailsJsonContext"/> to the
/// Minimal API JSON resolvers, so <see cref="Microsoft.AspNetCore.Http.IProblemDetailsService"/> can write the
/// enhanced problem when reflection-based serialization is disabled (Native AOT).
/// </summary>
internal sealed class EnhancedProblemDetailsJsonOptionsSetup : IPostConfigureOptions<HttpJsonOptions>
{
    public void PostConfigure(string? name, HttpJsonOptions options)
    {
        JsonSerializerOptions serializerOptions = options.SerializerOptions;
        if (serializerOptions.TypeInfoResolver is null && JsonSerializer.IsReflectionEnabledByDefault)
            return;
        if (!serializerOptions.TypeInfoResolverChain.Contains(EnhancedProblemDetailsJsonContext.Default))
            serializerOptions.TypeInfoResolverChain.Add(EnhancedProblemDetailsJsonContext.Default);
    }
}
