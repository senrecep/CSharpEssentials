using CSharpEssentials.Errors;
using Microsoft.AspNetCore.WebUtilities;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Default mapping: status from <see cref="ErrorType"/>, the primary error is the one with the highest status,
/// the title comes from the error type (or the reason phrase when the status was overridden).
/// </summary>
public class DefaultErrorStatusCodeMapper : IErrorStatusCodeMapper
{
    /// <summary>
    /// A shared instance, used when no mapper is registered.
    /// </summary>
    public static DefaultErrorStatusCodeMapper Instance { get; } = new();

    /// <inheritdoc />
    public virtual int GetStatusCode(Error error) => error.Type.ToHttpStatusCode();

    /// <inheritdoc />
    public virtual Error SelectPrimaryError(IReadOnlyList<Error> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        if (errors.Count == 0)
            throw new ArgumentException("At least one error is required.", nameof(errors));
        Error primary = errors[0];
        int primaryStatus = GetStatusCode(primary);
        for (int i = 1; i < errors.Count; i++)
        {
            int status = GetStatusCode(errors[i]);
            if (status > primaryStatus)
            {
                primary = errors[i];
                primaryStatus = status;
            }
        }
        return primary;
    }

    /// <inheritdoc />
    public virtual string? GetTitle(Error primaryError, int statusCode)
    {
        if (statusCode == primaryError.Type.ToHttpStatusCode())
            return primaryError.Type.GetProblemTitle();
        string reason = ReasonPhrases.GetReasonPhrase(statusCode);
        return reason.Length == 0 ? null : reason;
    }
}
