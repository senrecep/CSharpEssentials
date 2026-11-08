namespace CSharpEssentials.Maybe;

public readonly partial struct Maybe<T>
{
    /// <summary>Obsolete alias of <c>WhereAsync</c>. Will be removed in a future major version.</summary>
    [Obsolete("Use WhereAsync. Will be removed in a future major version.")]
    public Task<Maybe<T>> Where(Func<T, Task<bool>> predicate, CancellationToken cancellationToken = default) =>
        WhereAsync(predicate, cancellationToken);

    /// <summary>Obsolete alias of <c>WhereAsync</c>. Will be removed in a future major version.</summary>
    [Obsolete("Use WhereAsync. Will be removed in a future major version.")]
    public ValueTask<Maybe<T>> Where(Func<T, ValueTask<bool>> predicate, CancellationToken cancellationToken = default) =>
        WhereAsync(predicate, cancellationToken);
}

public static partial class MaybeExtensions
{
    /// <summary>Obsolete alias of <c>WhereAsync</c>. Will be removed in a future major version.</summary>
    [Obsolete("Use WhereAsync. Will be removed in a future major version.")]
    public static Task<Maybe<T>> Where<T>(this Task<Maybe<T>> maybeTask, Func<T, bool> predicate, CancellationToken cancellationToken = default) =>
        maybeTask.WhereAsync<T>(predicate, cancellationToken);

    /// <summary>Obsolete alias of <c>WhereAsync</c>. Will be removed in a future major version.</summary>
    [Obsolete("Use WhereAsync. Will be removed in a future major version.")]
    public static Task<Maybe<T>> Where<T>(this Task<Maybe<T>> maybeTask, Func<T, Task<bool>> predicate, CancellationToken cancellationToken = default) =>
        maybeTask.WhereAsync<T>(predicate, cancellationToken);

    /// <summary>Obsolete alias of <c>WhereAsync</c>. Will be removed in a future major version.</summary>
    [Obsolete("Use WhereAsync. Will be removed in a future major version.")]
    public static ValueTask<Maybe<T>> Where<T>(this ValueTask<Maybe<T>> maybeTask, Func<T, bool> predicate, CancellationToken cancellationToken = default) =>
        maybeTask.WhereAsync<T>(predicate, cancellationToken);

    /// <summary>Obsolete alias of <c>WhereAsync</c>. Will be removed in a future major version.</summary>
    [Obsolete("Use WhereAsync. Will be removed in a future major version.")]
    public static ValueTask<Maybe<T>> Where<T>(this ValueTask<Maybe<T>> maybeTask, Func<T, ValueTask<bool>> predicate, CancellationToken cancellationToken = default) =>
        maybeTask.WhereAsync<T>(predicate, cancellationToken);
}
