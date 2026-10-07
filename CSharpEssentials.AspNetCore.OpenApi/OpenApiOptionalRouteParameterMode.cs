namespace CSharpEssentials.AspNetCore;

/// <summary>
/// How <see cref="OpenApiOptionalRouteParameterExtensions.AddOptionalRouteParameters"/> describes an optional route parameter
/// (<c>{id?}</c>, <c>{id:int?}</c>, <c>{page=1}</c>). OpenAPI requires every path parameter to be <c>required: true</c>, so a
/// route with an optional segment cannot be described on one path.
/// </summary>
public enum OpenApiOptionalRouteParameterMode
{
    /// <summary>
    /// Describes a route that ends with one to three optional parameters on one path per form, from the full path to the path
    /// without them. The shorter forms are copies of the operation with <c>{operationId}Without{Param}</c> as operationId.
    /// Other optional and catch-all parameters keep one path, marked required.
    /// </summary>
    SplitPaths = 0,

    /// <summary>Keeps one path per operation and marks its path parameters required, without a nullable schema.</summary>
    RequiredOnly = 1,
}
