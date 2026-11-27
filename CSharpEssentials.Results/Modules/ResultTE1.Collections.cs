// Produced by scripts/generate-typed-results.py; do not edit.
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using CSharpEssentials.Core;
using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public static partial class ResultExtensions
{
    /// <summary>Collects the success values of <paramref name="source"/> in order. It stops at the first error and returns it;
    /// the later results are not looked at.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <param name="source">The results.</param>
    /// <returns>A result that holds every value, or the first error.</returns>
    /// <exception cref="InvalidOperationException">A result looked at before the first error is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static Result<T[], E1> Sequence<T, E1>(this IEnumerable<Result<T, E1>> source)
        where T : notnull
        where E1 : notnull
    {
        source = EnsureSource(source);
        List<T> values = CreateList<T>(CountHint(source));
        foreach (Result<T, E1> result in source)
        {
            if (result.Tag != TypedResultLogic.SuccessTag)
                return new(TypedResultLogic.ErrorTag(result.Tag), default!, result.RawError);

            values.Add(result.RawValue);
        }

        return new(TypedResultLogic.SuccessTag, values.ToArray(), null);
    }

    /// <summary>Calls <paramref name="selector"/> on each item in order and collects the success values. It stops at the first
    /// error and returns it; <paramref name="selector"/> is not called for the later items.</summary>
    /// <typeparam name="TSource">The item type.</typeparam>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <param name="source">The items.</param>
    /// <param name="selector">Called with each item.</param>
    /// <returns>A result that holds every value, or the first error.</returns>
    /// <exception cref="InvalidOperationException">A result looked at before the first error is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static Result<T[], E1> Traverse<TSource, T, E1>(this IEnumerable<TSource> source, Func<TSource, Result<T, E1>> selector)
        where T : notnull
        where E1 : notnull
    {
        source = EnsureSource(source);
        selector = EnsureSelector(selector);
        List<T> values = CreateList<T>(CountHint(source));
        foreach (TSource element in source)
        {
            Result<T, E1> result = selector(element);
            if (result.Tag != TypedResultLogic.SuccessTag)
                return new(TypedResultLogic.ErrorTag(result.Tag), default!, result.RawError);

            values.Add(result.RawValue);
        }

        return new(TypedResultLogic.SuccessTag, values.ToArray(), null);
    }

    /// <summary>Splits <paramref name="source"/> into its success values and its errors, both in order. Each error is converted
    /// to <see cref="Error"/> as by <c>ToResult</c>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <param name="source">The results.</param>
    /// <returns>The success values and the errors.</returns>
    /// <exception cref="InvalidOperationException">A result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static (T[] Successes, Error[] Errors) Partition<T, E1>(this IEnumerable<Result<T, E1>> source)
        where T : notnull
        where E1 : notnull
    {
        List<T> successes = [];
        List<Error> errors = [];
        foreach (Result<T, E1> result in EnsureSource(source))
        {
            TypedResultLogic.EnsureInitialized(result.Tag);
            if (result.Tag == TypedResultLogic.SuccessTag)
                successes.Add(result.RawValue);
            else
                errors.Add(result.CurrentError());
        }

        return (successes.ToArray(), errors.ToArray());
    }

    /// <summary>Calls <paramref name="selector"/> on each item in order and awaits it before the next one. It stops at the first
    /// error and returns it; <paramref name="selector"/> is not called for the later items.</summary>
    /// <typeparam name="TSource">The item type.</typeparam>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <param name="source">The items.</param>
    /// <param name="selector">Called with each item.</param>
    /// <param name="cancellationToken">Checked before each item.</param>
    /// <returns>A result that holds every value, or the first error.</returns>
    /// <exception cref="InvalidOperationException">A result looked at before the first error is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    [OverloadResolutionPriority(1)]
    public static async Task<Result<T[], E1>> TraverseAsync<TSource, T, E1>(this IEnumerable<TSource> source, Func<TSource, Task<Result<T, E1>>> selector, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
    {
        source = EnsureSource(source);
        selector = EnsureDelegate(selector);
        List<T> values = CreateList<T>(CountHint(source));
        foreach (TSource element in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Result<T, E1> result = await selector(element).ConfigureAwait(false);
            if (result.Tag != TypedResultLogic.SuccessTag)
                return new(TypedResultLogic.ErrorTag(result.Tag), default!, result.RawError);

            values.Add(result.RawValue);
        }

        return new(TypedResultLogic.SuccessTag, values.ToArray(), null);
    }

    /// <summary>Calls <paramref name="selector"/> on each item in order and awaits it before the next one. It stops at the first
    /// error and returns it; <paramref name="selector"/> is not called for the later items.</summary>
    /// <typeparam name="TSource">The item type.</typeparam>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <param name="source">The items.</param>
    /// <param name="selector">Called with each item and the token.</param>
    /// <param name="cancellationToken">Checked before each item.</param>
    /// <returns>A result that holds every value, or the first error.</returns>
    /// <exception cref="InvalidOperationException">A result looked at before the first error is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    [OverloadResolutionPriority(1)]
    public static async Task<Result<T[], E1>> TraverseAsync<TSource, T, E1>(this IEnumerable<TSource> source, Func<TSource, CancellationToken, Task<Result<T, E1>>> selector, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
    {
        source = EnsureSource(source);
        selector = EnsureDelegate(selector);
        List<T> values = CreateList<T>(CountHint(source));
        foreach (TSource element in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Result<T, E1> result = await selector(element, cancellationToken).ConfigureAwait(false);
            if (result.Tag != TypedResultLogic.SuccessTag)
                return new(TypedResultLogic.ErrorTag(result.Tag), default!, result.RawError);

            values.Add(result.RawValue);
        }

        return new(TypedResultLogic.SuccessTag, values.ToArray(), null);
    }

    /// <summary>Awaits the results of <paramref name="source"/> in order. It stops at the first error and returns it; the tasks
    /// after it are not awaited, and a fault of theirs is observed so it does not surface as an unobserved exception.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <param name="source">The pending results.</param>
    /// <param name="cancellationToken">Checked before each result and observed while awaiting it.</param>
    /// <returns>A result that holds every value, or the first error.</returns>
    /// <exception cref="InvalidOperationException">A result looked at before the first error is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<T[], E1>> SequenceAsync<T, E1>(this IEnumerable<Task<Result<T, E1>>> source, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
    {
        source = EnsureSource(source);
        List<T> values = CreateList<T>(CountHint(source));
        int started = 0;
        bool drained = false;
        try
        {
            foreach (Task<Result<T, E1>> pending in source)
            {
                cancellationToken.ThrowIfCancellationRequested();
                started++;
                Result<T, E1> result = await pending.WithCancellation(cancellationToken).ConfigureAwait(false);
                if (result.Tag != TypedResultLogic.SuccessTag)
                    return new(TypedResultLogic.ErrorTag(result.Tag), default!, result.RawError);

                values.Add(result.RawValue);
            }

            drained = true;
        }
        finally
        {
            if (!drained)
                ObserveRemaining(source, started, static t => t);
        }

        return new(TypedResultLogic.SuccessTag, values.ToArray(), null);
    }

    /// <summary>Awaits the results of <paramref name="source"/> in order. It stops at the first error and returns it; the tasks
    /// after it are not awaited, and a fault of theirs is observed so it does not surface as an unobserved exception.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <param name="source">The pending results.</param>
    /// <param name="cancellationToken">Checked before each result and observed while awaiting it.</param>
    /// <returns>A result that holds every value, or the first error.</returns>
    /// <exception cref="InvalidOperationException">A result looked at before the first error is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<T[], E1>> SequenceAsync<T, E1>(this IEnumerable<ValueTask<Result<T, E1>>> source, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
    {
        source = EnsureSource(source);
        List<T> values = CreateList<T>(CountHint(source));
        int started = 0;
        bool drained = false;
        try
        {
            foreach (ValueTask<Result<T, E1>> pending in source)
            {
                cancellationToken.ThrowIfCancellationRequested();
                started++;
                Result<T, E1> result = await pending.WithCancellation(cancellationToken).ConfigureAwait(false);
                if (result.Tag != TypedResultLogic.SuccessTag)
                    return new(TypedResultLogic.ErrorTag(result.Tag), default!, result.RawError);

                values.Add(result.RawValue);
            }

            drained = true;
        }
        finally
        {
            if (!drained)
                ObserveRemaining(source, started, static t => t.AsTask());
        }

        return new(TypedResultLogic.SuccessTag, values.ToArray(), null);
    }

#if NET9_0_OR_GREATER
    /// <summary>Calls <paramref name="selector"/> on each item in order and awaits it before the next one. It stops at the first
    /// error and returns it; <paramref name="selector"/> is not called for the later items.</summary>
    /// <typeparam name="TSource">The item type.</typeparam>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <param name="source">The items.</param>
    /// <param name="selector">Called with each item.</param>
    /// <param name="cancellationToken">Checked before each item.</param>
    /// <returns>A result that holds every value, or the first error.</returns>
    /// <exception cref="InvalidOperationException">A result looked at before the first error is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<T[], E1>> TraverseAsync<TSource, T, E1>(this IEnumerable<TSource> source, Func<TSource, ValueTask<Result<T, E1>>> selector, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
    {
        source = EnsureSource(source);
        selector = EnsureDelegate(selector);
        List<T> values = CreateList<T>(CountHint(source));
        foreach (TSource element in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Result<T, E1> result = await selector(element).ConfigureAwait(false);
            if (result.Tag != TypedResultLogic.SuccessTag)
                return new(TypedResultLogic.ErrorTag(result.Tag), default!, result.RawError);

            values.Add(result.RawValue);
        }

        return new(TypedResultLogic.SuccessTag, values.ToArray(), null);
    }

    /// <summary>Calls <paramref name="selector"/> on each item in order and awaits it before the next one. It stops at the first
    /// error and returns it; <paramref name="selector"/> is not called for the later items.</summary>
    /// <typeparam name="TSource">The item type.</typeparam>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <param name="source">The items.</param>
    /// <param name="selector">Called with each item and the token.</param>
    /// <param name="cancellationToken">Checked before each item.</param>
    /// <returns>A result that holds every value, or the first error.</returns>
    /// <exception cref="InvalidOperationException">A result looked at before the first error is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<T[], E1>> TraverseAsync<TSource, T, E1>(this IEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<Result<T, E1>>> selector, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
    {
        source = EnsureSource(source);
        selector = EnsureDelegate(selector);
        List<T> values = CreateList<T>(CountHint(source));
        foreach (TSource element in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Result<T, E1> result = await selector(element, cancellationToken).ConfigureAwait(false);
            if (result.Tag != TypedResultLogic.SuccessTag)
                return new(TypedResultLogic.ErrorTag(result.Tag), default!, result.RawError);

            values.Add(result.RawValue);
        }

        return new(TypedResultLogic.SuccessTag, values.ToArray(), null);
    }
#endif
}
