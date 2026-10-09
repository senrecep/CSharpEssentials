using System.Runtime.CompilerServices;

namespace CSharpEssentials.Maybe;

public readonly partial struct Maybe<T>
{
    /// <summary>Obsolete alias of <c>OrAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use OrAsync. Will be removed in 7.0.")]
    [OverloadResolutionPriority(1)]
    public Task<Maybe<T>> Or(
        Func<Task<T>> fallbackOperation,
        CancellationToken cancellationToken = default) =>
        OrAsync(fallbackOperation, cancellationToken);

    /// <summary>Obsolete alias of <c>OrAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use OrAsync. Will be removed in 7.0.")]
    [OverloadResolutionPriority(1)]
    public Task<Maybe<T>> Or(
        Task<Maybe<T>> fallback,
        CancellationToken cancellationToken = default) =>
        OrAsync(fallback, cancellationToken);

    /// <summary>Obsolete alias of <c>OrAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use OrAsync. Will be removed in 7.0.")]
    [OverloadResolutionPriority(1)]
    public Task<Maybe<T>> Or(
        Func<Task<Maybe<T>>> fallbackOperation,
        CancellationToken cancellationToken = default) =>
        OrAsync(fallbackOperation, cancellationToken);

    /// <summary>Obsolete alias of <c>OrAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use OrAsync. Will be removed in 7.0.")]
    public ValueTask<Maybe<T>> Or(
        Func<ValueTask<T>> valueTaskFallbackOperation,
        CancellationToken cancellationToken = default) =>
        OrAsync(valueTaskFallbackOperation, cancellationToken);

    /// <summary>Obsolete alias of <c>OrAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use OrAsync. Will be removed in 7.0.")]
    public ValueTask<Maybe<T>> Or(
        ValueTask<Maybe<T>> valueTaskFallback,
        CancellationToken cancellationToken = default) =>
        OrAsync(valueTaskFallback, cancellationToken);

    /// <summary>Obsolete alias of <c>OrAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use OrAsync. Will be removed in 7.0.")]
    public ValueTask<Maybe<T>> Or(
        Func<ValueTask<Maybe<T>>> valueTaskFallbackOperation,
        CancellationToken cancellationToken = default) =>
        OrAsync(valueTaskFallbackOperation, cancellationToken);
}

public static partial class MaybeExtensions
{
    /// <summary>Obsolete alias of <c>OrAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use OrAsync. Will be removed in 7.0.")]
    public static Task<Maybe<T>> Or<T>(
        this Task<Maybe<T>> maybeTask,
        T fallback,
        CancellationToken cancellationToken = default) =>
        maybeTask.OrAsync<T>(fallback, cancellationToken);

    /// <summary>Obsolete alias of <c>OrAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use OrAsync. Will be removed in 7.0.")]
    public static Task<Maybe<T>> Or<T>(
        this Task<Maybe<T>> maybeTask,
        Func<T> fallbackOperation,
        CancellationToken cancellationToken = default) =>
        maybeTask.OrAsync<T>(fallbackOperation, cancellationToken);

    /// <summary>Obsolete alias of <c>OrAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use OrAsync. Will be removed in 7.0.")]
    public static Task<Maybe<T>> Or<T>(
        this Task<Maybe<T>> maybeTask,
        Maybe<T> fallback,
        CancellationToken cancellationToken = default) =>
        maybeTask.OrAsync<T>(fallback, cancellationToken);

    /// <summary>Obsolete alias of <c>OrAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use OrAsync. Will be removed in 7.0.")]
    public static Task<Maybe<T>> Or<T>(
        this Task<Maybe<T>> maybeTask,
        Task<T> fallback,
        CancellationToken cancellationToken = default) =>
        maybeTask.OrAsync<T>(fallback, cancellationToken);

    /// <summary>Obsolete alias of <c>OrAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use OrAsync. Will be removed in 7.0.")]
    public static Task<Maybe<T>> Or<T>(
        this Task<Maybe<T>> maybeTask,
        Func<Task<T>> fallbackOperation,
        CancellationToken cancellationToken = default) =>
        maybeTask.OrAsync<T>(fallbackOperation, cancellationToken);

    /// <summary>Obsolete alias of <c>OrAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use OrAsync. Will be removed in 7.0.")]
    public static Task<Maybe<T>> Or<T>(
        this Task<Maybe<T>> maybeTask,
        Func<Maybe<T>> fallbackOperation,
        CancellationToken cancellationToken = default) =>
        maybeTask.OrAsync<T>(fallbackOperation, cancellationToken);

    /// <summary>Obsolete alias of <c>OrAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use OrAsync. Will be removed in 7.0.")]
    public static Task<Maybe<T>> Or<T>(
        this Task<Maybe<T>> maybeTask,
        Func<Task<Maybe<T>>> fallbackOperation,
        CancellationToken cancellationToken = default) =>
        maybeTask.OrAsync<T>(fallbackOperation, cancellationToken);

    /// <summary>Obsolete alias of <c>OrAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use OrAsync. Will be removed in 7.0.")]
    public static ValueTask<Maybe<T>> Or<T>(
        this ValueTask<Maybe<T>> maybeTask,
        T fallback,
        CancellationToken cancellationToken = default) =>
        maybeTask.OrAsync<T>(fallback, cancellationToken);

    /// <summary>Obsolete alias of <c>OrAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use OrAsync. Will be removed in 7.0.")]
    public static ValueTask<Maybe<T>> Or<T>(
        this ValueTask<Maybe<T>> maybeTask,
        Func<T> fallbackOperation,
        CancellationToken cancellationToken = default) =>
        maybeTask.OrAsync<T>(fallbackOperation, cancellationToken);

    /// <summary>Obsolete alias of <c>OrAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use OrAsync. Will be removed in 7.0.")]
    public static ValueTask<Maybe<T>> Or<T>(
        this ValueTask<Maybe<T>> maybeTask,
        Maybe<T> fallback,
        CancellationToken cancellationToken = default) =>
        maybeTask.OrAsync<T>(fallback, cancellationToken);

    /// <summary>Obsolete alias of <c>OrAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use OrAsync. Will be removed in 7.0.")]
    public static ValueTask<Maybe<T>> Or<T>(
        this ValueTask<Maybe<T>> maybeTask,
        Func<Maybe<T>> fallbackOperation,
        CancellationToken cancellationToken = default) =>
        maybeTask.OrAsync<T>(fallbackOperation, cancellationToken);

    /// <summary>Obsolete alias of <c>OrAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use OrAsync. Will be removed in 7.0.")]
    public static ValueTask<Maybe<T>> Or<T>(
        this ValueTask<Maybe<T>> maybeTask,
        ValueTask<T> fallback,
        CancellationToken cancellationToken = default) =>
        maybeTask.OrAsync<T>(fallback, cancellationToken);

    /// <summary>Obsolete alias of <c>OrAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use OrAsync. Will be removed in 7.0.")]
    public static ValueTask<Maybe<T>> Or<T>(
        this ValueTask<Maybe<T>> maybeTask,
        Func<ValueTask<T>> fallbackOperation,
        CancellationToken cancellationToken = default) =>
        maybeTask.OrAsync<T>(fallbackOperation, cancellationToken);

    /// <summary>Obsolete alias of <c>OrAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use OrAsync. Will be removed in 7.0.")]
    public static ValueTask<Maybe<T>> Or<T>(
        this ValueTask<Maybe<T>> maybeTask,
        Func<ValueTask<Maybe<T>>> fallbackOperation,
        CancellationToken cancellationToken = default) =>
        maybeTask.OrAsync<T>(fallbackOperation, cancellationToken);
}
