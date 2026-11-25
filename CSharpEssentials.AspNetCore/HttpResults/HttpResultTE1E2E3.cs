using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CSharpEssentials.Errors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// The HTTP response of a <see cref="ResultPattern.Result{T, E1, E2, E3}"/>: <typeparamref name="TSuccess"/> for the success value, and
/// for an error the same problem response as <see cref="Extensions.ToProblemResult(Error, ErrorMetadata?, int?)"/>, built from
/// the registered <see cref="IErrorStatusCodeMapper"/> and <see cref="EnhancedProblemDetailsOptions"/>. Create it with
/// <c>ToHttpResult()</c>, <c>ToOk()</c>, <c>ToCreated()</c> or <c>ToNoContent()</c>.
/// </summary>
/// <remarks>
/// The endpoint metadata (and so the OpenAPI document) lists the responses of <typeparamref name="TSuccess"/> and one
/// <c>application/problem+json</c> response per status that <see cref="IErrorStatusCodeMapper.GetDeclaredStatusCodes(System.Type)"/>
/// declares for each error type. An error type without <see cref="ErrorKindAttribute"/>, such as <see cref="Error"/>, declares
/// nothing: add <c>ProducesProblem(status)</c> for it.
/// </remarks>
/// <typeparam name="TSuccess">The response of the success value.</typeparam>
/// <typeparam name="E1">The first error type. Its <see cref="ErrorKindAttribute"/> gives the declared status.</typeparam>
/// <typeparam name="E2">The second error type. Its <see cref="ErrorKindAttribute"/> gives the declared status.</typeparam>
/// <typeparam name="E3">The third error type. Its <see cref="ErrorKindAttribute"/> gives the declared status.</typeparam>
[SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching Result<T, E1..E3>.")]
public sealed class HttpResult<TSuccess, E1, E2, E3> : IResult, INestedHttpResult, IEndpointMetadataProvider
    where TSuccess : IResult, IEndpointMetadataProvider
    where E1 : IError
    where E2 : IError
    where E3 : IError
{
    internal HttpResult(IResult result) => Result = result;

    /// <summary>Gets the response that executes: <typeparamref name="TSuccess"/> or the problem response.</summary>
    public IResult Result { get; }

    /// <inheritdoc />
    public Task ExecuteAsync(HttpContext httpContext) => Result.ExecuteAsync(httpContext);

    /// <inheritdoc />
    [SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "IEndpointMetadataProvider is a static abstract interface member.")]
    public static void PopulateMetadata(MethodInfo method, EndpointBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(builder);
        TSuccess.PopulateMetadata(method, builder);
        HttpResultMetadata.AddProblems(builder, typeof(E1), typeof(E2), typeof(E3));
    }
}
