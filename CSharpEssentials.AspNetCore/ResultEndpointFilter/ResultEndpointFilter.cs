
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using Microsoft.AspNetCore.Http;

namespace CSharpEssentials.AspNetCore;

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

            return mapper?.Map(errors) ?? Results.BadRequest(errors);
        }

        if (result is CSharpEssentials.ResultPattern.Interfaces.IResult r)
        {
            return r.IsSuccess
                ? Results.Ok()
                : mapper?.Map(r.Errors) ?? Results.BadRequest(r.Errors);
        }

        return result;
    }

    private static T GetPropertyValue<T>(object instance, Type type, string propertyName)
        => (T)type.GetProperty(propertyName)!.GetValue(instance)!;
}
