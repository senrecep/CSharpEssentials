namespace CSharpEssentials.AspNetCore;

/// <summary>
/// How a Swashbuckle document describes optional route parameters such as <c>{id?}</c>, <c>{id:int?}</c> or
/// <c>{page=1}</c>. OpenAPI requires every path parameter to be <c>required: true</c>, so an optional segment cannot be
/// described on a single path. Set with <see cref="SwaggerOptionalRouteParameterExtensions.AddOptionalRouteParameters"/>.
/// </summary>
public enum OptionalRouteParameterMode
{
    /// <summary>
    /// The default. Each operation whose route ends with optional parameters is described once per form: without the
    /// optional segments, then with each of them, left to right (<c>/archive</c>, <c>/archive/{year}</c>,
    /// <c>/archive/{year}/{month}</c>). Every path parameter is required. A route with more than three trailing optional
    /// parameters, an optional parameter followed by a required segment, and a catch-all parameter are not split; their
    /// parameters are only marked required.
    /// </summary>
    SplitPaths = 0,

    /// <summary>Marks optional path parameters <c>required: true</c> and keeps one path per operation.</summary>
    RequiredOnly = 1,

    /// <summary>
    /// The output of earlier versions: optional path parameters of MVC actions get <c>required: false</c>,
    /// <c>allowEmptyValue: true</c> and a nullable schema with a <c>null</c> default. Not valid OpenAPI.
    /// </summary>
    [Obsolete("Emits invalid OpenAPI: path parameters must be required and allowEmptyValue is only valid for query parameters. Use SplitPaths or RequiredOnly.")]
    LegacyNonCompliant = 2,
}
