using System.Runtime.CompilerServices;
using CSharpEssentials.Core;

namespace CSharpEssentials.Errors;

public static class Extensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToIntType(this ErrorType type) => (int)type;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToHttpStatusCode(this ErrorType errorType)
    {
        return errorType switch
        {
            ErrorType.Failure => HttpCodes.InternalServerError,
            ErrorType.Unexpected => HttpCodes.InternalServerError,
            ErrorType.Validation => HttpCodes.BadRequest,
            ErrorType.Conflict => HttpCodes.Conflict,
            ErrorType.NotFound => HttpCodes.NotFound,
            ErrorType.Unauthorized => HttpCodes.Unauthorized,
            ErrorType.Forbidden => HttpCodes.Forbidden,
            ErrorType.Unknown => HttpCodes.InternalServerError,
            _ => HttpCodes.InternalServerError
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ErrorType ToErrorType(this int statusCode)
    {
        return statusCode switch
        {
            HttpCodes.BadRequest => ErrorType.Validation,
            HttpCodes.Unauthorized => ErrorType.Unauthorized,
            HttpCodes.Forbidden => ErrorType.Forbidden,
            HttpCodes.NotFound => ErrorType.NotFound,
            HttpCodes.Conflict => ErrorType.Conflict,
            HttpCodes.InternalServerError => ErrorType.Failure,
            _ => ErrorType.Unexpected
        };
    }
}
