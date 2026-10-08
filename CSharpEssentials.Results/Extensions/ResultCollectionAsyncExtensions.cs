using CSharpEssentials.Core;
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

    private static void Accumulate<TValue>(Result<TValue> result, List<TValue> successes, ref List<Error>? errors)
    {
        if (result.IsSuccess)
            successes.Add(result.Value);
        else
            (errors ??= []).AddRange(result.ErrorsOrEmptyArray);
    }

    private static void ObserveRemaining<TItem>(IEnumerable<TItem> source, int consumed, Func<TItem, Task> toTask)
    {
        if (source is not ICollection<TItem> and not IReadOnlyCollection<TItem>)
            return;

        int index = 0;
        foreach (TItem item in source)
        {
            if (index++ < consumed)
                continue;

            _ = toTask(item).ContinueWith(
                static t => _ = t.Exception,
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }
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
            Accumulate(result, successes, ref errors);
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
            Accumulate(result, successes, ref errors);
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
            Accumulate(result, successes, ref errors);
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
            Accumulate(result, successes, ref errors);
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
        int started = 0;

        try
        {
            foreach (Task<Result<TValue>> task in source)
            {
                cancellationToken.ThrowIfCancellationRequested();
                started++;

                Result<TValue> result;
                try
                {
                    result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    fault ??= ExceptionDispatchInfo.Capture(ex);
                    continue;
                }

                Accumulate(result, successes, ref errors);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            ObserveRemaining(source, started, static t => t);
            throw;
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
        int started = 0;

        try
        {
            foreach (ValueTask<Result<TValue>> task in source)
            {
                cancellationToken.ThrowIfCancellationRequested();
                started++;

                Result<TValue> result;
                try
                {
                    result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    fault ??= ExceptionDispatchInfo.Capture(ex);
                    continue;
                }

                Accumulate(result, successes, ref errors);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            ObserveRemaining(source, started, static t => t.AsTask());
            throw;
        }

        fault?.Throw();
        return ToSequenceResult(successes, errors);
    }
}
