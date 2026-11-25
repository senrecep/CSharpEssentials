namespace CSharpEssentials.AspNetCore;

/// <summary>
/// The shape of the <c>errors</c> field when <see cref="ProblemErrorFields.ValidationErrors"/> is enabled.
/// </summary>
public enum ValidationErrorsFormat
{
    /// <summary>
    /// <c>[{ "code": "...", "description": "..." }]</c>.
    /// </summary>
    List = 0,

    /// <summary>
    /// <c>{ "code": ["description", ...] }</c>, grouped by error code. The code is used as-is.
    /// </summary>
    Dictionary = 1,

    /// <summary>
    /// <c>{ "Name": ["description", ...] }</c>, grouped by the property path in the
    /// <see cref="Errors.ErrorMetadata.FieldKey"/> metadata entry, the shape of ASP.NET Core's
    /// <c>ValidationProblemDetails</c>. An error with no field is grouped by its code.
    /// </summary>
    ByField = 2,
}
