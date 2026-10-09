using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Maybe;

public static partial class MaybeExtensions
{
    /// <summary>Obsolete alias of <c>ToMaybeResultAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ToMaybeResultAsync. Will be removed in 7.0.")]
    public static Task<Result<T>> ToMaybeResult<T>(
        this Task<Maybe<T>> maybeTask,
        Error? error = null,
        CancellationToken cancellationToken = default) =>
        maybeTask.ToMaybeResultAsync<T>(error, cancellationToken);

    /// <summary>Obsolete alias of <c>ToMaybeResultAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ToMaybeResultAsync. Will be removed in 7.0.")]
    public static ValueTask<Result<T>> ToMaybeResult<T>(
        this ValueTask<Maybe<T>> maybeTask,
        Error? error = null,
        CancellationToken cancellationToken = default) =>
        maybeTask.ToMaybeResultAsync<T>(error, cancellationToken);

    /// <summary>Obsolete alias of <c>ToMaybeUnitResultAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ToMaybeUnitResultAsync. Will be removed in 7.0.")]
    public static Task<Result> ToMaybeUnitResult<T>(
        this Task<Maybe<T>> maybeTask,
        Error? error = null,
        CancellationToken cancellationToken = default) =>
        maybeTask.ToMaybeUnitResultAsync<T>(error, cancellationToken);

    /// <summary>Obsolete alias of <c>ToMaybeUnitResultAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ToMaybeUnitResultAsync. Will be removed in 7.0.")]
    public static ValueTask<Result> ToMaybeUnitResult<T>(
        this ValueTask<Maybe<T>> maybeTask,
        Error? error = null,
        CancellationToken cancellationToken = default) =>
        maybeTask.ToMaybeUnitResultAsync<T>(error, cancellationToken);
}
