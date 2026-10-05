using CSharpEssentials.Errors;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// The problem an exception is turned into by an <see cref="IExceptionProblemMapper"/>.
/// </summary>
/// <param name="StatusCode">The status code. When <see langword="null"/> it is derived from <paramref name="Errors"/>
/// with the <see cref="IErrorStatusCodeMapper"/>, or 500 when there are no errors.</param>
/// <param name="Title">The title. When <see langword="null"/> the default title for the status is used.</param>
/// <param name="Detail">The detail. Never put raw exception messages of unknown exceptions here.</param>
/// <param name="Errors">Errors to describe the problem with, written like any other error-based problem.</param>
public sealed record ExceptionProblem(
    int? StatusCode = null,
    string? Title = null,
    string? Detail = null,
    IReadOnlyList<Error>? Errors = null);
