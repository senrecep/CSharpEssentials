using Mediator;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

using FluentValidation;
using FluentValidation.Results;

namespace CSharpEssentials.Mediator;

internal static class ValidationBehaviorCache
{
    public static readonly Type ResultType = typeof(Result);
    public static readonly Type GenericResultType = typeof(Result<>);
    public static readonly ConcurrentDictionary<Type, Func<Error[], object>> FailureFactories = new();
}

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
{
    private readonly Type _responseType = typeof(TResponse);
    private readonly IValidator<TRequest>[] _validatorArray = [.. validators];

    public async ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_validatorArray.Length == 0)
            return await next(message, cancellationToken);

        var context = new ValidationContext<TRequest>(message);
        ValidationResult[] validationFailures = await Task.WhenAll(
            _validatorArray.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        Error[] errors = [.. validationFailures
            .Where(validationResult => !validationResult.IsValid)
            .SelectMany(validationResult => validationResult.Errors)
            .Select(CreateErrorFromValidationFailure)
            .Distinct()];

        if (errors.Length == 0)
            return await next(message, cancellationToken);

        if (_responseType == ValidationBehaviorCache.ResultType)
            return (TResponse)(object)Result.Failure(errors);

        return CreateResponse(errors);
    }

    private TResponse CreateResponse(Error[] errors)
    {
        if (_responseType.GenericTypeArguments.Length == 0)
            throw new InvalidOperationException(
                $"ValidationBehavior requires TResponse to be Result or Result<T>, but got {_responseType.Name}.");

        Type genericType = ValidationBehaviorCache.GenericResultType.MakeGenericType(_responseType.GenericTypeArguments[0]);

        Func<Error[], object> factory = ValidationBehaviorCache.FailureFactories.GetOrAdd(genericType, static type =>
        {
            MethodInfo method = type.GetMethod(nameof(Result.Failure), [typeof(Error[])])
                ?? throw new InvalidOperationException($"Method {nameof(Result.Failure)} not found on {type.FullName}.");
            ParameterExpression param = Expression.Parameter(typeof(Error[]), "errors");
            MethodCallExpression call = Expression.Call(method, param);
            UnaryExpression boxed = Expression.Convert(call, typeof(object));
            return Expression.Lambda<Func<Error[], object>>(boxed, param).Compile();
        });

        return (TResponse)factory(errors);
    }

    private static Error CreateErrorFromValidationFailure(ValidationFailure validationResult) => Error.Validation(
        code: validationResult.ErrorCode,
        description: validationResult.ErrorMessage,
        metadata: new ErrorMetadata { [nameof(ValidationFailure.PropertyName)] = validationResult.PropertyName });
}
