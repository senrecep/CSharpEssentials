using System.Runtime.CompilerServices;

namespace CSharpEssentials.Maybe;

public readonly partial struct Maybe<T>
{
    /// <summary>Obsolete alias of <c>MatchAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use MatchAsync. Will be removed in 7.0.")]
    [OverloadResolutionPriority(1)]
    public Task<TE> Match<TE>(
        Func<T, CancellationToken, Task<TE>> some,
        Func<CancellationToken, Task<TE>> none,
        CancellationToken cancellationToken = default) =>
        MatchAsync<TE>(some, none, cancellationToken);

    /// <summary>Obsolete alias of <c>MatchAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use MatchAsync. Will be removed in 7.0.")]
    [OverloadResolutionPriority(1)]
    public Task<TE> Match<TE, TContext>(
        Func<T, TContext, CancellationToken, Task<TE>> some,
        Func<TContext, CancellationToken, Task<TE>> none,
        TContext context,
        CancellationToken cancellationToken = default) =>
        MatchAsync<TE, TContext>(some, none, context, cancellationToken);

    /// <summary>Obsolete alias of <c>MatchAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use MatchAsync. Will be removed in 7.0.")]
    [OverloadResolutionPriority(1)]
    public Task Match(
        Func<T, CancellationToken, Task> some,
        Func<CancellationToken, Task> none,
        CancellationToken cancellationToken = default) =>
        MatchAsync(some, none, cancellationToken);

    /// <summary>Obsolete alias of <c>MatchAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use MatchAsync. Will be removed in 7.0.")]
    [OverloadResolutionPriority(1)]
    public Task Match<TContext>(
        Func<T, TContext, CancellationToken, Task> some,
        Func<TContext, CancellationToken, Task> none,
        TContext context,
        CancellationToken cancellationToken = default) =>
        MatchAsync<TContext>(some, none, context, cancellationToken);

    /// <summary>Obsolete alias of <c>MatchAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use MatchAsync. Will be removed in 7.0.")]
    public ValueTask<TE> Match<TE>(
        Func<T, CancellationToken, ValueTask<TE>> some,
        Func<CancellationToken, ValueTask<TE>> none,
        CancellationToken cancellationToken = default) =>
        MatchAsync<TE>(some, none, cancellationToken);

    /// <summary>Obsolete alias of <c>MatchAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use MatchAsync. Will be removed in 7.0.")]
    public ValueTask<TE> Match<TE, TContext>(
        Func<T, TContext, CancellationToken, ValueTask<TE>> some,
        Func<TContext, CancellationToken, ValueTask<TE>> none,
        TContext context,
        CancellationToken cancellationToken = default) =>
        MatchAsync<TE, TContext>(some, none, context, cancellationToken);

    /// <summary>Obsolete alias of <c>MatchAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use MatchAsync. Will be removed in 7.0.")]
    public ValueTask Match(
        Func<T, CancellationToken, ValueTask> some,
        Func<CancellationToken, ValueTask> none,
        CancellationToken cancellationToken = default) =>
        MatchAsync(some, none, cancellationToken);

    /// <summary>Obsolete alias of <c>MatchAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use MatchAsync. Will be removed in 7.0.")]
    public ValueTask Match<TContext>(
        Func<T, TContext, CancellationToken, ValueTask> some,
        Func<TContext, CancellationToken, ValueTask> none,
        TContext context,
        CancellationToken cancellationToken = default) =>
        MatchAsync<TContext>(some, none, context, cancellationToken);
}

public static partial class MaybeExtensions
{
    /// <summary>Obsolete alias of <c>MatchAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use MatchAsync. Will be removed in 7.0.")]
    public static ValueTask<TE> Match<TE, TKey, TValue>(
        this Maybe<KeyValuePair<TKey, TValue>> maybe,
        Func<TKey, TValue, CancellationToken, ValueTask<TE>> some,
        Func<CancellationToken, ValueTask<TE>> none,
        CancellationToken cancellationToken = default) =>
        maybe.MatchAsync<TE, TKey, TValue>(some, none, cancellationToken);

    /// <summary>Obsolete alias of <c>MatchAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use MatchAsync. Will be removed in 7.0.")]
    public static ValueTask<TE> Match<TE, TKey, TValue, TContext>(
        this Maybe<KeyValuePair<TKey, TValue>> maybe,
        Func<TKey, TValue, TContext, CancellationToken, ValueTask<TE>> some,
        Func<TContext, CancellationToken, ValueTask<TE>> none,
        TContext context,
        CancellationToken cancellationToken = default) =>
        maybe.MatchAsync<TE, TKey, TValue, TContext>(some, none, context, cancellationToken);

    /// <summary>Obsolete alias of <c>MatchAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use MatchAsync. Will be removed in 7.0.")]
    public static ValueTask Match<TKey, TValue>(
        this Maybe<KeyValuePair<TKey, TValue>> maybe,
        Func<TKey, TValue, CancellationToken, ValueTask> some,
        Func<CancellationToken, ValueTask> none,
        CancellationToken cancellationToken = default) =>
        maybe.MatchAsync<TKey, TValue>(some, none, cancellationToken);

    /// <summary>Obsolete alias of <c>MatchAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use MatchAsync. Will be removed in 7.0.")]
    public static ValueTask Match<TKey, TValue, TContext>(
        this Maybe<KeyValuePair<TKey, TValue>> maybe,
        Func<TKey, TValue, TContext, CancellationToken, ValueTask> some,
        Func<TContext, CancellationToken, ValueTask> none,
        TContext context,
        CancellationToken cancellationToken = default) =>
        maybe.MatchAsync<TKey, TValue, TContext>(some, none, context, cancellationToken);

    /// <summary>Obsolete alias of <c>MatchAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use MatchAsync. Will be removed in 7.0.")]
    [OverloadResolutionPriority(1)]
    public static Task<TE> Match<TE, TKey, TValue>(
        this Maybe<KeyValuePair<TKey, TValue>> maybe,
        Func<TKey, TValue, CancellationToken, Task<TE>> some,
        Func<CancellationToken, Task<TE>> none,
        CancellationToken cancellationToken = default) =>
        maybe.MatchAsync<TE, TKey, TValue>(some, none, cancellationToken);

    /// <summary>Obsolete alias of <c>MatchAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use MatchAsync. Will be removed in 7.0.")]
    [OverloadResolutionPriority(1)]
    public static Task<TE> Match<TE, TKey, TValue, TContext>(
        this Maybe<KeyValuePair<TKey, TValue>> maybe,
        Func<TKey, TValue, TContext, CancellationToken, Task<TE>> some,
        Func<TContext, CancellationToken, Task<TE>> none,
        TContext context,
        CancellationToken cancellationToken = default) =>
        maybe.MatchAsync<TE, TKey, TValue, TContext>(some, none, context, cancellationToken);

    /// <summary>Obsolete alias of <c>MatchAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use MatchAsync. Will be removed in 7.0.")]
    [OverloadResolutionPriority(1)]
    public static Task Match<TKey, TValue>(
        this Maybe<KeyValuePair<TKey, TValue>> maybe,
        Func<TKey, TValue, CancellationToken, Task> some,
        Func<CancellationToken, Task> none,
        CancellationToken cancellationToken = default) =>
        maybe.MatchAsync<TKey, TValue>(some, none, cancellationToken);

    /// <summary>Obsolete alias of <c>MatchAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use MatchAsync. Will be removed in 7.0.")]
    [OverloadResolutionPriority(1)]
    public static Task Match<TKey, TValue, TContext>(
        this Maybe<KeyValuePair<TKey, TValue>> maybe,
        Func<TKey, TValue, TContext, CancellationToken, Task> some,
        Func<TContext, CancellationToken, Task> none,
        TContext context,
        CancellationToken cancellationToken = default) =>
        maybe.MatchAsync<TKey, TValue, TContext>(some, none, context, cancellationToken);
}
