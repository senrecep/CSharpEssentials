using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Runs the enrichment first, then any <see cref="ProblemDetailsOptions.CustomizeProblemDetails"/> the application set,
/// so application code keeps the last word.
/// </summary>
internal sealed class EnhancedProblemDetailsPostConfigure : IPostConfigureOptions<ProblemDetailsOptions>
{
    public void PostConfigure(string? name, ProblemDetailsOptions options)
    {
        Action<ProblemDetailsContext>? previous = options.CustomizeProblemDetails;
        options.CustomizeProblemDetails = context =>
        {
            ProblemDetailsEnrichment.Enrich(context);
            previous?.Invoke(context);
        };
    }
}
