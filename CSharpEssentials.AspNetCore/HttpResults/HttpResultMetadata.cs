using System.Reflection;
using CSharpEssentials.Errors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Adds the problem responses of <see cref="HttpResult{TSuccess, E1}"/> and its siblings to the endpoint metadata.
/// </summary>
internal static class HttpResultMetadata
{
    private static readonly string[] ProblemContentTypes = [EnhancedProblemDetailsWriter.ContentType];

    /// <summary>
    /// Adds one problem response per status that the registered <see cref="IErrorStatusCodeMapper"/> declares for each error type.
    /// The mapper is resolved in a scope, so a scoped mapper works too.
    /// </summary>
    internal static void AddProblems(EndpointBuilder builder, params Type[] errorTypes)
    {
        using IServiceScope? scope = builder.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope();
        IErrorStatusCodeMapper mapper = (scope?.ServiceProvider ?? builder.ApplicationServices).GetService<IErrorStatusCodeMapper>()
            ?? DefaultErrorStatusCodeMapper.Instance;
        foreach (Type errorType in errorTypes)
        {
            foreach (int statusCode in mapper.GetDeclaredStatusCodes(errorType))
                AddProblem(builder, statusCode);
        }
    }

    /// <summary>Adds a problem response with <paramref name="statusCode"/> unless the endpoint already declares that status.</summary>
    internal static void AddProblem(EndpointBuilder builder, int statusCode)
    {
        foreach (object item in builder.Metadata)
        {
            if (item is IProducesResponseTypeMetadata produces && produces.StatusCode == statusCode)
                return;
        }

        builder.Metadata.Add(new ProducesResponseTypeMetadata(statusCode, typeof(EnhancedProblemDetails), ProblemContentTypes));
    }

    /// <summary>
    /// The status <paramref name="mapper"/> sends for an error of the kind that <see cref="ErrorKindAttribute"/> declares on
    /// <paramref name="errorType"/>, or none without the attribute.
    /// </summary>
    internal static IEnumerable<int> DeclaredStatusCodes(IErrorStatusCodeMapper mapper, Type errorType)
    {
        ArgumentNullException.ThrowIfNull(errorType);
        return errorType.GetCustomAttribute<ErrorKindAttribute>(inherit: true) is { } kind
            ? [mapper.GetStatusCode(Error.Create(errorType.Name, errorType.Name, kind.Type))]
            : [];
    }
}
