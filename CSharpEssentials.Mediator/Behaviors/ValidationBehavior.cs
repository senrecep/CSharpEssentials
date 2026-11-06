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
            return ValidateGroupsAsync(message, mode, next, cancellationToken);

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

    private async ValueTask<TResponse> ValidateGroupsAsync(
        TRequest message,
        ValidationMode mode,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        Error[]? errors = await ValidateAsync(message, cancellationToken).ConfigureAwait(false);
        if (errors is null)
            return await next(message, cancellationToken).ConfigureAwait(false);

        return await CompleteFailureAsync(message, errors, mode, AnyObserverListening(mode), next, cancellationToken).ConfigureAwait(false);
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

    /// <summary>
    /// Runs the validator groups and returns the errors, or <see langword="null"/> when the request is valid.
    /// </summary>
    private async ValueTask<Error[]?> ValidateAsync(TRequest message, CancellationToken cancellationToken)
    {
        // The constructor sorts groups by Order, ascending.
        // Validators in a group run concurrently; groups run one after another.
        // Errors are collected in group order, so lower-Order errors come first.
        // allErrors stays null until the first failure, so a valid request allocates no list.
        List<Error>? allErrors = null;

        foreach (IValidator<TRequest>[] group in _orderedGroups)
        {
            Result<TRequest>[] groupResults = await RunGroupAsync(group, message, cancellationToken).ConfigureAwait(false);

            foreach (Result<TRequest> r in groupResults)
            {
                if (r.IsFailure)
                {
                    allErrors ??= [];
                    allErrors.AddRange(r.Errors);
                }
            }
        }

        if (allErrors is null)
            return null;

        Error[] errors = [.. allErrors.Distinct()];
        return errors.Length == 0 ? null : errors;
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
    /// Starts every validator in <paramref name="validators"/> before awaiting any of them.
    /// Results of validators that complete synchronously are read inline with no task allocation; the others are awaited in order.
    /// </summary>
    private static async Task<Result<TRequest>[]> RunGroupAsync(
        IValidator<TRequest>[] validators,
        TRequest message,
        CancellationToken ct)
    {
        int count = validators.Length;
        var results = new Result<TRequest>[count];
        Task<Result<TRequest>>[]? pendingTasks = null;
        int[]? pendingIndices = null;
        int pendingCount = 0;

        for (int i = 0; i < count; i++)
        {
            ValueTask<Result<TRequest>> vt = RunSafeAsync(validators[i], message, ct);
            if (vt.IsCompletedSuccessfully)
            {
                results[i] = vt.GetAwaiter().GetResult();
            }
            else
            {
                pendingTasks ??= new Task<Result<TRequest>>[count];
                pendingIndices ??= new int[count];
                pendingIndices[pendingCount] = i;
                pendingTasks[pendingCount++] = vt.AsTask();
            }
        }

        for (int p = 0; p < pendingCount; p++)
            results[pendingIndices![p]] = await pendingTasks![p].ConfigureAwait(false);

        return results;
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
