using Microsoft.AspNetCore.Http;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Endpoint metadata of <see cref="ConditionalRequestExtensions.WithIfMatch{TBuilder}(TBuilder, bool)"/> and its loader overload.
/// One object, so an endpoint replaces both settings of its group.
/// </summary>
/// <param name="Required">Whether a missing <c>If-Match</c> returns 428.</param>
/// <param name="LoadCurrent">Loads the current resource before the handler; <see langword="null"/> to leave the check to the handler.</param>
internal sealed record IfMatchMetadata(bool Required, Func<HttpContext, CancellationToken, ValueTask<object?>>? LoadCurrent);
