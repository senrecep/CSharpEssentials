using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Routing.Patterns;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// The optional route parameters of one API description: whether the route has an optional or catch-all parameter, and the
/// names of the optional parameters that end it (simple segments only, left to right).
/// </summary>
internal sealed record OptionalRouteShape(bool HasOptionalOrCatchAll, IReadOnlyList<string> TrailingOptional)
{
    public static OptionalRouteShape? Analyze(ApiDescription description, string relativePath)
    {
        // Parsing is skipped for routes that cannot have an optional or catch-all parameter.
        if (relativePath.AsSpan().IndexOfAny('?', '=', '*') < 0
            && !description.ParameterDescriptions.Any(static candidate => candidate.RouteInfo?.IsOptional == true))
            return null;

        RoutePattern pattern;
        try
        {
            pattern = RoutePatternFactory.Parse(relativePath);
        }
        catch (RoutePatternException)
        {
            return null;
        }

        bool hasOptionalOrCatchAll = pattern.Parameters.Any(parameter => parameter.IsCatchAll || IsOptional(description, parameter));

        List<string> trailing = [];
        for (int index = pattern.PathSegments.Count - 1; index >= 0; index--)
        {
            RoutePatternPathSegment segment = pattern.PathSegments[index];
            if (!segment.IsSimple || segment.Parts[0] is not RoutePatternParameterPart parameter || parameter.IsCatchAll || !IsOptional(description, parameter))
                break;
            trailing.Insert(0, parameter.Name);
        }
        return new OptionalRouteShape(hasOptionalOrCatchAll, trailing);
    }

    // MVC reports a sanitized RelativePath ("{id}" for "{id?}"), so its optional parameters are known from RouteInfo only.
    private static bool IsOptional(ApiDescription description, RoutePatternParameterPart parameter) =>
        !parameter.IsCatchAll
        && (parameter.IsOptional
            || parameter.Default is not null
            || description.ParameterDescriptions.Any(candidate =>
                candidate.RouteInfo is { IsOptional: true } && string.Equals(candidate.Name, parameter.Name, StringComparison.OrdinalIgnoreCase)));
}
