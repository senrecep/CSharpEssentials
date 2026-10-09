using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Validates every non-null handler argument of type <typeparamref name="T"/> with the registered
/// <see cref="IValidator{T}"/> services before the handler runs. Validators run in ascending
/// <see cref="IValidator{T}.Order"/>; their errors are combined and returned as a problem response
/// (<see cref="Extensions.ToProblemResult(Error[], ErrorMetadata?, int?)"/>), so the handler is not called.
/// </summary>
/// <typeparam name="T">The argument type to validate.</typeparam>
/// <remarks>
/// Validators are resolved from <see cref="HttpContext.RequestServices"/> on each request. When none is registered,
/// the filter throws <see cref="InvalidOperationException"/> instead of letting unvalidated input through.
/// Exceptions thrown by validators are not caught.
/// </remarks>
public sealed class ValidationEndpointFilter<T> : IEndpointFilter
{
    /// <inheritdoc />
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        T[] arguments = [.. context.Arguments.OfType<T>()];
        if (arguments.Length == 0)
            return await next(context).ConfigureAwait(false);

        IValidator<T>[] validators = [.. context.HttpContext.RequestServices
            .GetServices<IValidator<T>>()
            .OrderBy(static validator => validator.Order)];
        if (validators.Length == 0)
            throw new InvalidOperationException(
                $"No {nameof(IValidator<>)}<{typeof(T).FullName}> is registered, but the endpoint '{context.HttpContext.GetEndpoint()?.DisplayName}' requires validation of '{typeof(T).FullName}'.");

        List<Error>? errors = null;
        foreach (T argument in arguments)
        {
            foreach (IValidator<T> validator in validators)
            {
                Result<T> result = await validator.ValidateAsync(argument, context.HttpContext.RequestAborted).ConfigureAwait(false);
                if (result.IsFailure)
                    (errors ??= []).AddRange(result.Errors);
            }
        }

        return errors is null
            ? await next(context).ConfigureAwait(false)
            : errors.Distinct().ToArray().ToProblemResult();
    }
}
