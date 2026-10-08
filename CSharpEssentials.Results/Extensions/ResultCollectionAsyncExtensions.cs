using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public static partial class ResultExtensions
{
    private static T EnsureDelegate<T>(T selector) where T : class
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(selector);
#else
        if (selector is null)
            throw new ArgumentNullException(nameof(selector));
#endif

        return selector;
    }

    private static int CountHint<T>(IEnumerable<T> source) => source switch
    {
        ICollection<T> collection => collection.Count,
        IReadOnlyCollection<T> collection => collection.Count,
        _ => 0
    };

    private static List<TValue> CreateList<TValue>(int capacity)
    {
        List<TValue> list = [];
        if (capacity > 0)
            list.Capacity = capacity;

        return list;
    }

    private static Result<TValue[]> ToSequenceResult<TValue>(List<TValue> successes, List<Error>? errors) =>
        errors is null ? successes.ToArray() : errors;

    [OverloadResolutionPriority(1)]
    public static Task<Result<TOut[]>> TraverseAsync<TSource, TOut>(
        this IEnumerable<TSource> source,
        Func<TSource, Task<Result<TOut>>> selector,
        CancellationToken cancellationToken = default) =>
        TraverseCoreAsync(EnsureSource(source), EnsureDelegate(selector), cancellationToken);

    [OverloadResolutionPriority(1)]
    public static Task<Result<TOut[]>> TraverseAsync<TSource, TOut>(
        this IEnumerable<TSource> source,
        Func<TSource, CancellationToken, Task<Result<TOut>>> selector,
        CancellationToken cancellationToken = default) =>
        TraverseCoreAsync(EnsureSource(source), EnsureDelegate(selector), cancellationToken);

#if NET9_0_OR_GREATER
    public static ValueTask<Result<TOut[]>> TraverseAsync<TSource, TOut>(
        this IEnumerable<TSource> source,
        Func<TSource, ValueTask<Result<TOut>>> selector,
        CancellationToken cancellationToken = default) =>
        TraverseCoreAsync(EnsureSource(source), EnsureDelegate(selector), cancellationToken);

    public static ValueTask<Result<TOut[]>> TraverseAsync<TSource, TOut>(
        this IEnumerable<TSource> source,
        Func<TSource, CancellationToken, ValueTask<Result<TOut>>> selector,
        CancellationToken cancellationToken = default) =>
        TraverseCoreAsync(EnsureSource(source), EnsureDelegate(selector), cancellationToken);
#endif

    public static Task<Result<TValue[]>> SequenceAsync<TValue>(
        this IEnumerable<Task<Result<TValue>>> source,
        CancellationToken cancellationToken = default) =>
        SequenceCoreAsync(EnsureSource(source), cancellationToken);

    public static ValueTask<Result<TValue[]>> SequenceAsync<TValue>(
        this IEnumerable<ValueTask<Result<TValue>>> source,
        CancellationToken cancellationToken = default) =>
        SequenceCoreAsync(EnsureSource(source), cancellationToken);

    private static async Task<Result<TOut[]>> TraverseCoreAsync<TSource, TOut>(
        IEnumerable<TSource> source,
        Func<TSource, Task<Result<TOut>>> selector,
        CancellationToken cancellationToken)
    {
        List<TOut> successes = CreateList<TOut>(CountHint(source));
        List<Error>? errors = null;

        foreach (TSource item in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Result<TOut> result = await selector(item).ConfigureAwait(false);
            if (result.IsSuccess)
                successes.Add(result.Value);
            else
                (errors ??= []).AddRange(result.ErrorsOrEmptyArray);
        }

        return ToSequenceResult(successes, errors);
    }

    private static async Task<Result<TOut[]>> TraverseCoreAsync<TSource, TOut>(
        IEnumerable<TSource> source,
        Func<TSource, CancellationToken, Task<Result<TOut>>> selector,
        CancellationToken cancellationToken)
    {
        List<TOut> successes = CreateList<TOut>(CountHint(source));
        List<Error>? errors = null;

        foreach (TSource item in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Result<TOut> result = await selector(item, cancellationToken).ConfigureAwait(false);
            if (result.IsSuccess)
                successes.Add(result.Value);
            else
                (errors ??= []).AddRange(result.ErrorsOrEmptyArray);
        }

        return ToSequenceResult(successes, errors);
    }

#if NET9_0_OR_GREATER
    private static async ValueTask<Result<TOut[]>> TraverseCoreAsync<TSource, TOut>(
        IEnumerable<TSource> source,
        Func<TSource, ValueTask<Result<TOut>>> selector,
        CancellationToken cancellationToken)
    {
        List<TOut> successes = CreateList<TOut>(CountHint(source));
        List<Error>? errors = null;

        foreach (TSource item in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Result<TOut> result = await selector(item).ConfigureAwait(false);
            if (result.IsSuccess)
                successes.Add(result.Value);
            else
                (errors ??= []).AddRange(result.ErrorsOrEmptyArray);
        }

        return ToSequenceResult(successes, errors);
    }

    private static async ValueTask<Result<TOut[]>> TraverseCoreAsync<TSource, TOut>(
        IEnumerable<TSource> source,
        Func<TSource, CancellationToken, ValueTask<Result<TOut>>> selector,
        CancellationToken cancellationToken)
    {
        List<TOut> successes = CreateList<TOut>(CountHint(source));
        List<Error>? errors = null;

        foreach (TSource item in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Result<TOut> result = await selector(item, cancellationToken).ConfigureAwait(false);
            if (result.IsSuccess)
                successes.Add(result.Value);
            else
                (errors ??= []).AddRange(result.ErrorsOrEmptyArray);
        }

        return ToSequenceResult(successes, errors);
    }
#endif

    private static async Task<Result<TValue[]>> SequenceCoreAsync<TValue>(
        IEnumerable<Task<Result<TValue>>> source,
        CancellationToken cancellationToken)
    {
        List<TValue> successes = CreateList<TValue>(CountHint(source));
        List<Error>? errors = null;
        ExceptionDispatchInfo? fault = null;

        foreach (Task<Result<TValue>> task in source)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Result<TValue> result;
            try
            {
                result = await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                fault ??= ExceptionDispatchInfo.Capture(ex);
                continue;
            }

            if (result.IsSuccess)
                successes.Add(result.Value);
            else
                (errors ??= []).AddRange(result.ErrorsOrEmptyArray);
        }

        fault?.Throw();
        return ToSequenceResult(successes, errors);
    }

    private static async ValueTask<Result<TValue[]>> SequenceCoreAsync<TValue>(
        IEnumerable<ValueTask<Result<TValue>>> source,
        CancellationToken cancellationToken)
    {
        List<TValue> successes = CreateList<TValue>(CountHint(source));
        List<Error>? errors = null;
        ExceptionDispatchInfo? fault = null;

        foreach (ValueTask<Result<TValue>> task in source)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Result<TValue> result;
            try
            {
                result = await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                fault ??= ExceptionDispatchInfo.Capture(ex);
                continue;
            }

            if (result.IsSuccess)
                successes.Add(result.Value);
            else
                (errors ??= []).AddRange(result.ErrorsOrEmptyArray);
        }

        fault?.Throw();
        return ToSequenceResult(successes, errors);
    }
}
