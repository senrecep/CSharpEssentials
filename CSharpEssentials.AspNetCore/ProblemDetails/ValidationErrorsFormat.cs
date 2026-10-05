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
}
