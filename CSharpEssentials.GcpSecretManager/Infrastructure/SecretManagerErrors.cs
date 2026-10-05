using CSharpEssentials.Errors;
using Grpc.Core;

namespace CSharpEssentials.GcpSecretManager.Infrastructure;

internal static class SecretManagerErrors
{
    private const string CodePrefix = "SecretManager.";
    private const string ResourceExhaustedCode = CodePrefix + nameof(StatusCode.ResourceExhausted);
    private const string UnavailableCode = CodePrefix + nameof(StatusCode.Unavailable);

    public static Error FromException(Exception exception) =>
        exception is RpcException rpcException
            ? Error.Exception(CodePrefix + rpcException.StatusCode, rpcException, ErrorType.Unexpected)
            : Error.Exception(exception, ErrorType.Unexpected);

    public static bool IsTransient(Error error) =>
        error.Code is ResourceExhaustedCode or UnavailableCode;
}
