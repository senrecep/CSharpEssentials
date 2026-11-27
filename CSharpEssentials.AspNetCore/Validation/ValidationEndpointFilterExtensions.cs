using CSharpEssentials.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Adds <see cref="ValidationEndpointFilter{T}"/> to route handlers.
/// </summary>
public static class ValidationEndpointFilterExtensions
{
    /// <summary>
    /// Validates the handler argument of type <typeparamref name="T"/> with the registered <see cref="IValidator{T}"/>
    /// services and returns a problem response when validation fails. The endpoint declares that <c>400</c> problem response
    /// unless it already declares a <c>400</c>.
    /// </summary>
    /// <typeparam name="T">The argument type to validate.</typeparam>
    /// <param name="builder">The route handler builder.</param>
    /// <returns>The same builder.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the endpoint is built and the handler has no parameter of type <typeparamref name="T"/>.
    /// </exception>
    public static RouteHandlerBuilder WithValidation<T>(this RouteHandlerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        ValidationEndpointFilter<T> filter = new();
        builder.Add(static endpoint => HttpResultMetadata.AddProblem(endpoint, StatusCodes.Status400BadRequest));
        return builder.AddEndpointFilterFactory((factoryContext, next) =>
        {
            bool hasArgument = factoryContext.MethodInfo.GetParameters().Any(static parameter =>
                typeof(T).IsAssignableFrom(Nullable.GetUnderlyingType(parameter.ParameterType) ?? parameter.ParameterType));
            if (!hasArgument)
                throw new InvalidOperationException(
                    $"WithValidation<{typeof(T).FullName}>() was applied to handler '{factoryContext.MethodInfo.Name}', which has no parameter of that type.");

            return invocationContext => filter.InvokeAsync(invocationContext, next);
        });
    }
}
