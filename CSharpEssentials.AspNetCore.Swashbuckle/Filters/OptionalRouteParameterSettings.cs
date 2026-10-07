namespace CSharpEssentials.AspNetCore.Swagger.Filters;

/// <summary>The arguments of <see cref="OptionalRouteParameterDocumentFilter"/>.</summary>
internal sealed record OptionalRouteParameterSettings(
    OptionalRouteParameterMode Mode,
    Func<string, IReadOnlyList<string>, string>? OperationIdSelector);
