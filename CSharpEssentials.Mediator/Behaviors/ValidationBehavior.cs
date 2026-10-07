using Mediator;

using CSharpEssentials.Errors;
using CSharpEssentials.Exceptions;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;

namespace CSharpEssentials.Mediator;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
{
    private readonly Type _responseType = typeof(TResponse);

    // Validators are grouped by Order (ascending) and materialized once at construction time.
    // Hot path never pays the GroupBy/OrderBy cost — groups are iterated directly.
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
        _orderedGroups = [..
            validators
                .GroupBy(v => v.Order)
                .OrderBy(g => g.Key)
                .Select(g => g.ToArray())];
        _observers = [.. observers];
        _defaultMode = options?.DefaultMode ?? ValidationMode.Enforce;
    }

    public async ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ValidationMode mode = message is IValidationModeOverride modeOverride
            ? modeOverride.ValidationMode
            : _defaultMode;

        if (mode == ValidationMode.Off || _orderedGroups.Length == 0)
            return await next(message, cancellationToken);

        Error[]? errors = await ValidateAsync(message, cancellationToken);
        if (errors is null)
            return await next(message, cancellationToken);

        if (_observers.Length > 0)
            await NotifyObserversAsync(message, errors, mode, cancellationToken);

        if (mode == ValidationMode.LogOnly)
            return await next(message, cancellationToken);

        return BuildFailureResponse(errors);
    }

    /// <summary>
    /// Runs the validators and returns the errors, or <see langword="null"/> when the request is valid.
    /// </summary>
    private async ValueTask<Error[]?> ValidateAsync(TRequest message, CancellationToken cancellationToken)
    {
        // Single validator fast path — skips group iteration and List allocation entirely.
        if (_orderedGroups is [{ Length: 1 } onlyGroup])
        {
            Result<TRequest> singleResult = await RunSafeAsync(onlyGroup[0], message, cancellationToken);
            return singleResult.IsFailure ? [.. singleResult.Errors] : null;
        }

        // Groups are pre-sorted by Order (ascending) in the constructor.
        // Within each group validators run concurrently; groups run sequentially.
        // Errors accumulate in group order so lower-Order errors appear first.
        // allErrors is null-initialized — no heap allocation on the happy path.
        List<Error>? allErrors = null;

        foreach (IValidator<TRequest>[] group in _orderedGroups)
        {
            Result<TRequest>[] groupResults = await RunGroupAsync(group, message, cancellationToken);

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

    private async ValueTask NotifyObserversAsync(
        TRequest message,
        Error[] errors,
        ValidationMode mode,
        CancellationToken cancellationToken)
    {
        ValidationFailureContext failure = new(typeof(TRequest), message, Array.AsReadOnly(errors), mode);
        foreach (IValidationFailureObserver observer in _observers)
            await observer.OnValidationFailedAsync(failure, cancellationToken);
    }

    /// <summary>
    /// Runs all validators in <paramref name="validators"/> concurrently using the ValueTask fan-out pattern.
    /// Sync-complete validators are consumed inline (zero allocation); truly async ones are awaited in order.
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
    /// Runs a single validator, converting any non-<see cref="OperationCanceledException"/> exception
    /// into a <see cref="Result{T}"/> failure so the pipeline never throws for validator bugs.
    /// <see cref="OperationCanceledException"/> always propagates so cancellation is respected.
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
    ///   <item><see cref="Result"/> — <c>Result.Failure(errors)</c> returned directly.</item>
    ///   <item><see cref="Result{T}"/> — <c>Result&lt;T&gt;.Failure(errors)</c> via compiled factory.</item>
    ///   <item>Any other type — <see cref="EnhancedValidationException"/> thrown for <c>GlobalExceptionHandler</c>.</item>
    /// </list>
    /// </summary>
    private TResponse BuildFailureResponse(Error[] errors)
    {
        if (_responseType == BehaviorCache.ResultType)
            return (TResponse)(object)Result.Failure(errors);

        if (_responseType.IsGenericType
            && _responseType.GetGenericTypeDefinition() == BehaviorCache.GenericResultType)
            return CreateGenericResultResponse(errors);

        // TResponse cannot carry error information — delegate to GlobalExceptionHandler.
        throw new EnhancedValidationException(errors);
    }

    private TResponse CreateGenericResultResponse(Error[] errors) =>
        (TResponse)BehaviorCache.GetOrCreateFactory(_responseType)(errors);
}
