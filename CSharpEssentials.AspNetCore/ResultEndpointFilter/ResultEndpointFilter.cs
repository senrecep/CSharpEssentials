using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Converts <c>Result</c>/<c>Result&lt;T&gt;</c> return values: success → 200, failure → <see cref="IResultErrorMapper"/>
/// when registered, otherwise a problem response (<see cref="Extensions.ToProblemResult(Error[], ErrorMetadata?, int?)"/>).
/// The mapper is resolved from <see cref="HttpContext.RequestServices"/> on each request, so it can have any lifetime.
/// </summary>
public sealed class ResultEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        object? result = await next(context).ConfigureAwait(false);
        if (result is IValueResult valueResult)
        {
            return valueResult.IsSuccess
                ? TypedResults.Ok(valueResult.BoxedValue)
                : GetMapper(context)?.Map(valueResult.Errors) ?? valueResult.Errors.ToProblemResult();
        }

        if (result is CSharpEssentials.ResultPattern.Interfaces.IResult r)
        {
            return r.IsSuccess
                ? Results.Ok()
                : GetMapper(context)?.Map(r.Errors) ?? r.Errors.ToProblemResult();
        }

        return result;
    }

    private static IResultErrorMapper? GetMapper(EndpointFilterInvocationContext context) =>
        context.HttpContext.RequestServices?.GetService<IResultErrorMapper>();
}
