using CSharpEssentials.Errors;

namespace CSharpEssentials.Http;

internal static class HttpExceptionErrors
{
    internal const string SsrfBlockedCode = "Http.SsrfBlocked";
    internal const string SsrfBlockedDescription = "The outbound request was blocked by the SSRF guard.";
    internal const string ReasonMetadataKey = "reason";
    internal const string TimeoutCode = "Http.Timeout";

    internal static Error ToError(Exception exception)
    {
#if NET9_0_OR_GREATER
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is SsrfBlockedException blocked)
                return Error.Forbidden(SsrfBlockedCode, SsrfBlockedDescription, new ErrorMetadata(ReasonMetadataKey, blocked.Reason));
        }
#endif
        if (exception is TaskCanceledException { InnerException: TimeoutException })
            return Error.Exception(TimeoutCode, exception, ErrorType.Unexpected);

        return Error.Exception(exception, ErrorType.Unexpected);
    }
}
