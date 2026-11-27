using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using HttpJsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;
using MvcJsonOptions = Microsoft.AspNetCore.Mvc.JsonOptions;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Registered by <c>AddEnhancedProblemDetails</c>: puts <see cref="TypedResultHttpJsonGuard"/> first in the Minimal API and
/// MVC JSON converters, so it wins over the type-level converter of the typed results.
/// </summary>
internal sealed class TypedResultHttpJsonGuardSetup : IPostConfigureOptions<HttpJsonOptions>, IPostConfigureOptions<MvcJsonOptions>
{
    public void PostConfigure(string? name, HttpJsonOptions options) => Add(options.SerializerOptions.Converters);

    public void PostConfigure(string? name, MvcJsonOptions options) => Add(options.JsonSerializerOptions.Converters);

    private static void Add(IList<JsonConverter> converters)
    {
        if (!converters.Any(static converter => converter is TypedResultHttpJsonGuard))
            converters.Insert(0, new TypedResultHttpJsonGuard());
    }
}
