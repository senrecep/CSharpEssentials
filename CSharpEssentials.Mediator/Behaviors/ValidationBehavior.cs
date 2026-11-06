using Mediator;

using CSharpEssentials.Errors;
using CSharpEssentials.Exceptions;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;

namespace CSharpEssentials.Mediator;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
{
    // Validators are grouped by Order (ascending) and materialized once at construction time.
    // Hot path never pays the GroupBy/OrderBy cost, groups are iterated directly.
    private readonly IValidator<TRequest>[][] _orderedGroups;
    private readonly IValidationFailureObserver[] _observers;
    private readonly ValidationMode _defaultMode;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        : this(validators, [], null)
    {
    }

    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        IEnumerable<IValidationFailureObserver> observers,
        ValidationBehaviorOptions? options = null)
    {
        _orderedGroups = Group([.. validators]);
        _observers = [.. observers];
        _defaultMode = options?.DefaultMode ?? ValidationMode.Enforce;
    }

    private static IValidator<TRequest>[][] Group(IValidator<TRequest>[] validators)
    {
        if (validators.Length == 0)
            return [];

        int order = validators[0].Order;
        for (int index = 1; index < validators.Length; index++)
        {
            if (validators[index].Order != order)
                return [.. validators.GroupBy(v => v.Order).OrderBy(g => g.Key).Select(g => g.ToArray())];
        }

        return [validators];
    }

    public ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return new ValueTask<TResponse>(Task.FromCanceled<TResponse>(cancellationToken));

        try
        {
            return HandleCore(message, next, cancellationToken);
        }
        catch (OperationCanceledException oce)
        {
            TaskCompletionSource<TResponse> canceled = new();
            canceled.TrySetCanceled(oce.CancellationToken);
            return new ValueTask<TResponse>(canceled.Task);
        }
        catch (Exception ex)
        {
            return new ValueTask<TResponse>(Task.FromException<TResponse>(ex));
        }
    }

    private ValueTask<TResponse> HandleCore(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        ValidationMode mode = message is IValidationModeOverride modeOverride
            ? modeOverride.ValidationMode
            : _defaultMode;

        if (mode == ValidationMode.Off || _orderedGroups.Length == 0)
            return next(message, cancellationToken);

        if (_orderedGroups is not [[IValidator<TRequest> validator]])
            return ValidateGroups(message, mode, next, cancellationToken);

        ValueTask<Result<TRequest>> validation = RunSafeAsync(validator, message, cancellationToken);
        if (!validation.IsCompletedSuccessfully)
            return AwaitValidationAsync(validation, message, mode, next, cancellationToken);

        Result<TRequest> result = validation.GetAwaiter().GetResult();
        if (result.IsSuccess)
            return next(message, cancellationToken);

        bool notify = AnyObserverListening(mode);
        if (mode == ValidationMode.Enforce && FailureResponse<TResponse>.Factory is { } factory && !notify)
            return new ValueTask<TResponse>(factory.CreateFailure(result.Errors));

        return CompleteFailureAsync(message, result.Errors, mode, notify, next, cancellationToken);
    }

    private async ValueTask<TResponse> AwaitValidationAsync(
        ValueTask<Result<TRequest>> validation,
        TRequest message,
        ValidationMode mode,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        Result<TRequest> result = await validation.ConfigureAwait(false);
        if (result.IsSuccess)
            return await next(message, cancellationToken).ConfigureAwait(false);

        return await CompleteFailureAsync(message, result.Errors, mode, AnyObserverListening(mode), next, cancellationToken).ConfigureAwait(false);
    }

    private ValueTask<TResponse> ValidateGroups(
        TRequest message,
        ValidationMode mode,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        List<Error>? errors = null;
        for (int groupIndex = 0; groupIndex < _orderedGroups.Length; groupIndex++)
        {
            IValidator<TRequest>[] group = _orderedGroups[groupIndex];
            for (int index = 0; index < group.Length; index++)
            {
                ValueTask<Result<TRequest>> validation = RunSafeAsync(group[index], message, cancellationToken);
                if (!validation.IsCompletedSuccessfully)
                    return ValidateGroupsAsync(message, mode, next, groupIndex, index, validation, errors, cancellationToken);

                errors = Collect(validation.GetAwaiter().GetResult(), errors);
            }
        }

        Error[]? distinct = Deduplicate(errors);
        if (distinct is null)
            return next(message, cancellationToken);

        bool notify = AnyObserverListening(mode);
        if (mode == ValidationMode.Enforce && FailureResponse<TResponse>.Factory is { } factory && !notify)
            return new ValueTask<TResponse>(factory.CreateFailure(distinct));

        return CompleteFailureAsync(message, distinct, mode, notify, next, cancellationToken);
    }

    /// <summary>
    /// Continues <see cref="ValidateGroups"/> from the first validator that did not complete synchronously.
    /// The rest of that validator's group is started before anything is awaited, so validators of one group still run
    /// concurrently; groups still run one after another and errors keep validator order.
    /// </summary>
    private async ValueTask<TResponse> ValidateGroupsAsync(
        TRequest message,
        ValidationMode mode,
        MessageHandlerDelegate<TRequest, TResponse> next,
        int groupIndex,
        int validatorIndex,
        ValueTask<Result<TRequest>> pending,
        List<Error>? errors,
        CancellationToken cancellationToken)
    {
        for (int g = groupIndex; g < _orderedGroups.Length; g++)
        {
            IValidator<TRequest>[] group = _orderedGroups[g];
            int first = g == groupIndex ? validatorIndex : 0;
            int count = group.Length - first;
            var results = new Result<TRequest>[count];
            var tasks = new Task<Result<TRequest>>?[count];
            for (int i = 0; i < count; i++)
            {
                ValueTask<Result<TRequest>> validation = g == groupIndex && i == 0
                    ? pending
                    : RunSafeAsync(group[first + i], message, cancellationToken);
                if (validation.IsCompletedSuccessfully)
                    results[i] = validation.GetAwaiter().GetResult();
                else
                    tasks[i] = validation.AsTask();
            }

            for (int i = 0; i < count; i++)
                errors = Collect(tasks[i] is { } task ? await task.ConfigureAwait(false) : results[i], errors);
        }

        Error[]? distinct = Deduplicate(errors);
        if (distinct is null)
            return await next(message, cancellationToken).ConfigureAwait(false);

        return await CompleteFailureAsync(message, distinct, mode, AnyObserverListening(mode), next, cancellationToken).ConfigureAwait(false);
    }

    private static List<Error>? Collect(Result<TRequest> result, List<Error>? errors)
    {
        if (result.IsFailure)
        {
            errors ??= [];
            errors.AddRange(result.Errors);
        }

        return errors;
    }

    /// <summary>
    /// Returns the errors without duplicates in first-seen order, or <see langword="null"/> when there are none.
    /// Small lists are checked pairwise so the common no-duplicate case copies the list once and allocates no set.
    /// </summary>
    private static Error[]? Deduplicate(List<Error>? errors)
    {
        if (errors is null || errors.Count == 0)
            return null;

        if (errors.Count > 16)
            return [.. errors.Distinct()];

        for (int i = 1; i < errors.Count; i++)
        {
            for (int j = 0; j < i; j++)
            {
                if (errors[i].Equals(errors[j]))
                    return [.. errors.Distinct()];
            }
        }

        return [.. errors];
    }

    private async ValueTask<TResponse> CompleteFailureAsync(
        TRequest message,
        Error[] errors,
        ValidationMode mode,
        bool notify,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (notify)
            await NotifyObserversAsync(message, errors, mode, cancellationToken).ConfigureAwait(false);

        if (mode == ValidationMode.LogOnly)
            return await next(message, cancellationToken).ConfigureAwait(false);

        return BuildFailureResponse(errors);
    }

    private bool AnyObserverListening(ValidationMode mode)
    {
        foreach (IValidationFailureObserver observer in _observers)
        {
            if (observer is not LoggingValidationFailureObserver defaultObserver || defaultObserver.IsListening(mode))
                return true;
        }

        return false;
    }

    private async ValueTask NotifyObserversAsync(
        TRequest message,
        Error[] errors,
        ValidationMode mode,
        CancellationToken cancellationToken)
    {
        ValidationFailureContext failure = new(typeof(TRequest), message, Array.AsReadOnly(errors), mode);
        foreach (IValidationFailureObserver observer in _observers)
            await observer.OnValidationFailedAsync(failure, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Runs one validator and turns any exception other than <see cref="OperationCanceledException"/>
    /// into a <see cref="Result{T}"/> failure, so a faulty validator does not throw out of the pipeline.
    /// <see cref="OperationCanceledException"/> always propagates, so cancellation still works.
    /// </summary>
    private static ValueTask<Result<TRequest>> RunSafeAsync(
        IValidator<TRequest> validator,
        TRequest message,
        CancellationToken ct)
    {
        try
        {
            ValueTask<Result<TRequest>> vt = validator.ValidateAsync(message, ct);
            // Sync-complete fast path: return directly, no state-machine allocation.
            if (vt.IsCompletedSuccessfully)
                return vt;
            return WrapAsync(vt);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new ValueTask<Result<TRequest>>(Error.Exception("Validator.Exception", ex));
        }

        static async ValueTask<Result<TRequest>> WrapAsync(ValueTask<Result<TRequest>> vt)
        {
            try
            {
                return await vt.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return Error.Exception("Validator.Exception", ex);
            }
        }
    }

    /// <summary>
    /// Maps validation errors to <typeparamref name="TResponse"/>:
    /// <list type="bullet">
    ///   <item><see cref="Result"/> and <see cref="Result{T}"/>: a failure carrying <paramref name="errors"/>, built without reflection.</item>
    ///   <item>Any other type: <see cref="EnhancedValidationException"/> thrown for <c>GlobalExceptionHandler</c>.</item>
    /// </list>
    /// </summary>
    private static TResponse BuildFailureResponse(Error[] errors) =>
        FailureResponse<TResponse>.Factory is { } factory
            ? factory.CreateFailure(errors)
            : throw new EnhancedValidationException(errors);
}
