using CSharpEssentials.AspNetCore.Swagger.Filters;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CSharpEssentials.AspNetCore;

/// <summary>Optional route parameter conventions for Swashbuckle documents.</summary>
public static class SwaggerOptionalRouteParameterExtensions
{
    /// <summary>
    /// Sets how optional route parameters are described (see <see cref="OptionalRouteParameterMode"/>). Replaces an earlier
    /// call, so a call after <c>AddSwagger</c> (which uses <see cref="OptionalRouteParameterMode.SplitPaths"/>) wins.
    /// </summary>
    /// <param name="options">The Swashbuckle options.</param>
    /// <param name="mode">The mode.</param>
    /// <param name="operationIdSelector">
    /// <see cref="OptionalRouteParameterMode.SplitPaths"/> only: the operationId of a form without some optional segments,
    /// from the operationId of the full form and the names of the omitted parameters. The default is
    /// <c>{operationId}Without{Param}</c>, with <c>And</c> between several parameters. A form whose full operation has no
    /// operationId gets none. A generated operationId that another operation already uses throws an
    /// <see cref="InvalidOperationException"/> when the document is generated.
    /// </param>
    /// <returns><paramref name="options"/>.</returns>
    public static SwaggerGenOptions AddOptionalRouteParameters(
        this SwaggerGenOptions options,
        OptionalRouteParameterMode mode = OptionalRouteParameterMode.SplitPaths,
        Func<string, IReadOnlyList<string>, string>? operationIdSelector = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (!Enum.IsDefined(mode))
            throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown optional route parameter mode.");

        options.OperationFilterDescriptors.RemoveAll(static descriptor => descriptor.Type == typeof(ReApplyOptionalRouteParameterOperationFilter));
        options.DocumentFilterDescriptors.RemoveAll(static descriptor => descriptor.Type == typeof(OptionalRouteParameterDocumentFilter));

        if (mode is OptionalRouteParameterMode.SplitPaths or OptionalRouteParameterMode.RequiredOnly)
            options.DocumentFilter<OptionalRouteParameterDocumentFilter>(new OptionalRouteParameterSettings(mode, operationIdSelector));
        else
            options.OperationFilter<ReApplyOptionalRouteParameterOperationFilter>();
        return options;
    }
}
