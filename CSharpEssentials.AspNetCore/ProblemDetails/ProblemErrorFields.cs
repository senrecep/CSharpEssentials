namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Which error collections are written to a problem response built from <see cref="Errors.Error"/> values.
/// </summary>
[Flags]
public enum ProblemErrorFields
{
    /// <summary>
    /// No error collections; only <c>title</c>/<c>detail</c> describe the problem.
    /// </summary>
    None = 0,

    /// <summary>
    /// <c>errorCodes</c>: the distinct codes of all errors.
    /// </summary>
    Codes = 1,

    /// <summary>
    /// <c>errors</c>: only <see cref="Errors.ErrorType.Validation"/> errors, as code and description
    /// (shape set by <see cref="EnhancedProblemDetailsOptions.ValidationErrorsFormat"/>).
    /// </summary>
    ValidationErrors = 2,

    /// <summary>
    /// <c>errorMessages</c>: the distinct descriptions of all errors.
    /// </summary>
    Messages = 4,

    /// <summary>
    /// <c>errors</c>: every <see cref="Errors.Error"/> with all of its fields, including metadata (3.x shape).
    /// Takes precedence over <see cref="ValidationErrors"/>.
    /// </summary>
    AllErrors = 8,

    /// <summary>
    /// Every collection (3.x behavior).
    /// </summary>
    All = Codes | ValidationErrors | Messages | AllErrors,
}
