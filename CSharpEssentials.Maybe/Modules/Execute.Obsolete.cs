using System.Runtime.CompilerServices;

namespace CSharpEssentials.Maybe;

public readonly partial struct Maybe<T>
{
    /// <summary>Obsolete alias of <c>ExecuteAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ExecuteAsync. Will be removed in 7.0.")]
    [OverloadResolutionPriority(1)]
    public Task Execute(
        Func<T, Task> action,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(action, cancellationToken);

    /// <summary>Obsolete alias of <c>ExecuteAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ExecuteAsync. Will be removed in 7.0.")]
    public Task Execute(
        Func<T, ValueTask> valueTask,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(valueTask, cancellationToken);

    /// <summary>Obsolete alias of <c>ExecuteNoValueAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ExecuteNoValueAsync. Will be removed in 7.0.")]
    [OverloadResolutionPriority(1)]
    public Task ExecuteNoValue(
        Func<Task> action,
        CancellationToken cancellationToken = default) =>
        ExecuteNoValueAsync(action, cancellationToken);

    /// <summary>Obsolete alias of <c>ExecuteNoValueAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ExecuteNoValueAsync. Will be removed in 7.0.")]
    public Task ExecuteNoValue(
        Func<ValueTask> valueTask,
        CancellationToken cancellationToken = default) =>
        ExecuteNoValueAsync(valueTask, cancellationToken);
}

public static partial class MaybeExtensions
{
    /// <summary>Obsolete alias of <c>ExecuteAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ExecuteAsync. Will be removed in 7.0.")]
    public static Task Execute<T>(
        this Task<Maybe<T>> maybeTask,
        Action<T> action,
        CancellationToken cancellationToken = default) =>
        maybeTask.ExecuteAsync<T>(action, cancellationToken);

    /// <summary>Obsolete alias of <c>ExecuteAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ExecuteAsync. Will be removed in 7.0.")]
    public static Task Execute<T>(
        this Task<Maybe<T>> maybeTask,
        Func<T, Task> asyncAction,
        CancellationToken cancellationToken = default) =>
        maybeTask.ExecuteAsync<T>(asyncAction, cancellationToken);

    /// <summary>Obsolete alias of <c>ExecuteAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ExecuteAsync. Will be removed in 7.0.")]
    public static Task Execute<T>(
        this ValueTask<Maybe<T>> maybeTask,
        Action<T> action,
        CancellationToken cancellationToken = default) =>
        maybeTask.ExecuteAsync<T>(action, cancellationToken);

    /// <summary>Obsolete alias of <c>ExecuteAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ExecuteAsync. Will be removed in 7.0.")]
    public static Task Execute<T>(
        this ValueTask<Maybe<T>> maybeTask,
        Func<T, ValueTask> valueTask,
        CancellationToken cancellationToken = default) =>
        maybeTask.ExecuteAsync<T>(valueTask, cancellationToken);

    /// <summary>Obsolete alias of <c>ExecuteNoValueAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ExecuteNoValueAsync. Will be removed in 7.0.")]
    public static Task ExecuteNoValue<T>(
        this Task<Maybe<T>> maybeTask,
        Action action,
        CancellationToken cancellationToken = default) =>
        maybeTask.ExecuteNoValueAsync<T>(action, cancellationToken);

    /// <summary>Obsolete alias of <c>ExecuteNoValueAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ExecuteNoValueAsync. Will be removed in 7.0.")]
    public static Task ExecuteNoValue<T>(
        this Task<Maybe<T>> maybeTask,
        Func<Task> asyncAction,
        CancellationToken cancellationToken = default) =>
        maybeTask.ExecuteNoValueAsync<T>(asyncAction, cancellationToken);

    /// <summary>Obsolete alias of <c>ExecuteNoValueAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ExecuteNoValueAsync. Will be removed in 7.0.")]
    public static Task ExecuteNoValue<T>(
        this ValueTask<Maybe<T>> maybeTask,
        Action action,
        CancellationToken cancellationToken = default) =>
        maybeTask.ExecuteNoValueAsync<T>(action, cancellationToken);

    /// <summary>Obsolete alias of <c>ExecuteNoValueAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ExecuteNoValueAsync. Will be removed in 7.0.")]
    public static Task ExecuteNoValue<T>(
        this ValueTask<Maybe<T>> maybeTask,
        Func<ValueTask> valueTask,
        CancellationToken cancellationToken = default) =>
        maybeTask.ExecuteNoValueAsync<T>(valueTask, cancellationToken);
}
