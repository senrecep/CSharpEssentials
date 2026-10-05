
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using Microsoft.AspNetCore.Http;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Converts <c>Result</c>/<c>Result&lt;T&gt;</c> return values: success → 200, failure → <see cref="IResultErrorMapper"/>
/// when registered, otherwise a problem response (<see cref="Extensions.ToProblemResult(Error[], ErrorMetadata?, int?)"/>).
/// </summary>
public sealed class ResultEndpointFilter(IResultErrorMapper? mapper = null) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        object? result = await next(context);
        if (result is null)
            return result;

        Type resultType = result.GetType();
        if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            bool isSuccess = GetPropertyValue<bool>(result, resultType, nameof(Result.IsSuccess));
            Error[] errors = GetPropertyValue<Error[]>(result, resultType, nameof(Result.Errors));

            if (isSuccess)
            {
                object? value = resultType.GetProperty("Value")?.GetValue(result);
                return TypedResults.Ok(value);
            }

            return mapper?.Map(errors) ?? errors.ToProblemResult();
        }

        if (result is CSharpEssentials.ResultPattern.Interfaces.IResult r)
        {
            return r.IsSuccess
                ? Results.Ok()
                : mapper?.Map(r.Errors) ?? r.Errors.ToProblemResult();
        }

        return result;
    }

    private static T GetPropertyValue<T>(object instance, Type type, string propertyName)
        => (T)type.GetProperty(propertyName)!.GetValue(instance)!;
}
