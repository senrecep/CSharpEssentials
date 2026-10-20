using CSharpEssentials.Errors;
using CSharpEssentials.Idempotency;
using Microsoft.AspNetCore.Http;
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
    /// <remarks>
    /// <see cref="ConditionalRequestErrors.PreconditionFailed"/> maps to 412. The <see cref="IdempotencyErrorCodes"/>
    /// codes map to the statuses the idempotency middleware sends, whatever their <see cref="ErrorType"/>:
    /// <see cref="IdempotencyErrorCodes.KeyReused"/> to 422, <see cref="IdempotencyErrorCodes.InFlight"/> to 409, and
    /// <see cref="IdempotencyErrorCodes.KeyMissing"/>, <see cref="IdempotencyErrorCodes.KeyInvalid"/> and
    /// <see cref="IdempotencyErrorCodes.ScopeRequired"/> to 400.
    /// </remarks>
    public virtual int GetStatusCode(Error error)
    {
        if (ConditionalRequestErrors.IsPreconditionFailed(error))
            return StatusCodes.Status412PreconditionFailed;
        return error.Code switch
        {
            IdempotencyErrorCodes.KeyReused => StatusCodes.Status422UnprocessableEntity,
            IdempotencyErrorCodes.InFlight => StatusCodes.Status409Conflict,
            IdempotencyErrorCodes.KeyMissing or IdempotencyErrorCodes.KeyInvalid or IdempotencyErrorCodes.ScopeRequired
                => StatusCodes.Status400BadRequest,
            _ => error.Type.ToHttpStatusCode(),
        };
    }

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
