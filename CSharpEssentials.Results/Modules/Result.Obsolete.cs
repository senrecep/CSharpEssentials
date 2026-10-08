using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result
{
    /// <summary>Obsolete alias of <c>BindAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use BindAsync. Will be removed in 7.0.")]
    public Task<Result<TOut>> Bind<TOut>(Func<Task<Result<TOut>>> func) =>
        BindAsync(func);

    /// <summary>Obsolete alias of <c>BindAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use BindAsync. Will be removed in 7.0.")]
    public Task<Result> Bind(Func<Task<Result>> func) =>
        BindAsync(func);

    /// <summary>Obsolete alias of <c>BindAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use BindAsync. Will be removed in 7.0.")]
    public ValueTask<Result<TOut>> Bind<TOut>(Func<ValueTask<Result<TOut>>> valueTask) =>
        BindAsync(valueTask);

    /// <summary>Obsolete alias of <c>BindAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use BindAsync. Will be removed in 7.0.")]
    public ValueTask<Result> Bind(Func<ValueTask<Result>> valueTask) =>
        BindAsync(valueTask);
}

public static partial class ResultExtensions
{
    /// <summary>Obsolete alias of <c>MatchAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use MatchAsync. Will be removed in 7.0.")]
    public static Task<T> Match<T>(this Task<Result> task, Func<T> onSuccess, Func<Error[], T> onFailure, CancellationToken cancellationToken = default) =>
        task.MatchAsync<T>(onSuccess, onFailure, cancellationToken);

    /// <summary>Obsolete alias of <c>MatchFirstAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use MatchFirstAsync. Will be removed in 7.0.")]
    public static Task<T> MatchFirst<T>(this Task<Result> task, Func<T> onSuccess, Func<Error, T> onFailure, CancellationToken cancellationToken = default) =>
        task.MatchFirstAsync<T>(onSuccess, onFailure, cancellationToken);

    /// <summary>Obsolete alias of <c>MatchLastAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use MatchLastAsync. Will be removed in 7.0.")]
    public static Task<T> MatchLast<T>(this Task<Result> task, Func<T> onSuccess, Func<Error, T> onFailure, CancellationToken cancellationToken = default) =>
        task.MatchLastAsync<T>(onSuccess, onFailure, cancellationToken);

    /// <summary>Obsolete alias of <c>MatchAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use MatchAsync. Will be removed in 7.0.")]
    public static ValueTask<T> Match<T>(this ValueTask<Result> task, Func<T> onSuccess, Func<Error[], T> onFailure, CancellationToken cancellationToken = default) =>
        task.MatchAsync<T>(onSuccess, onFailure, cancellationToken);

    /// <summary>Obsolete alias of <c>MatchFirstAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use MatchFirstAsync. Will be removed in 7.0.")]
    public static ValueTask<T> MatchFirst<T>(this ValueTask<Result> task, Func<T> onSuccess, Func<Error, T> onFailure, CancellationToken cancellationToken = default) =>
        task.MatchFirstAsync<T>(onSuccess, onFailure, cancellationToken);

    /// <summary>Obsolete alias of <c>MatchLastAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use MatchLastAsync. Will be removed in 7.0.")]
    public static ValueTask<T> MatchLast<T>(this ValueTask<Result> task, Func<T> onSuccess, Func<Error, T> onFailure, CancellationToken cancellationToken = default) =>
        task.MatchLastAsync<T>(onSuccess, onFailure, cancellationToken);

    /// <summary>Obsolete alias of <c>SwitchAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use SwitchAsync. Will be removed in 7.0.")]
    public static Task Switch(this Task<Result> task, Action onSuccess, Action<Error[]> onFailure, CancellationToken cancellationToken) =>
        task.SwitchAsync(onSuccess, onFailure, cancellationToken);

    /// <summary>Obsolete alias of <c>SwitchFirstAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use SwitchFirstAsync. Will be removed in 7.0.")]
    public static Task SwitchFirst(this Task<Result> task, Action onSuccess, Action<Error> onFailure, CancellationToken cancellationToken) =>
        task.SwitchFirstAsync(onSuccess, onFailure, cancellationToken);

    /// <summary>Obsolete alias of <c>SwitchLastAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use SwitchLastAsync. Will be removed in 7.0.")]
    public static Task SwitchLast(this Task<Result> task, Action onSuccess, Action<Error> onFailure, CancellationToken cancellationToken) =>
        task.SwitchLastAsync(onSuccess, onFailure, cancellationToken);

    /// <summary>Obsolete alias of <c>SwitchAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use SwitchAsync. Will be removed in 7.0.")]
    public static ValueTask Switch(this ValueTask<Result> task, Action onSuccess, Action<Error[]> onFailure, CancellationToken cancellationToken = default) =>
        task.SwitchAsync(onSuccess, onFailure, cancellationToken);

    /// <summary>Obsolete alias of <c>SwitchFirstAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use SwitchFirstAsync. Will be removed in 7.0.")]
    public static ValueTask SwitchFirst(this ValueTask<Result> task, Action onSuccess, Action<Error> onFailure, CancellationToken cancellationToken = default) =>
        task.SwitchFirstAsync(onSuccess, onFailure, cancellationToken);

    /// <summary>Obsolete alias of <c>SwitchLastAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use SwitchLastAsync. Will be removed in 7.0.")]
    public static ValueTask SwitchLast(this ValueTask<Result> task, Action onSuccess, Action<Error> onFailure, CancellationToken cancellationToken = default) =>
        task.SwitchLastAsync(onSuccess, onFailure, cancellationToken);

    /// <summary>Obsolete alias of <c>ThenAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ThenAsync. Will be removed in 7.0.")]
    public static Task<Result> Then(this Task<Result> task, Func<Result> onSuccess, CancellationToken cancellationToken) =>
        task.ThenAsync(onSuccess, cancellationToken);

    /// <summary>Obsolete alias of <c>ThenDoAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ThenDoAsync. Will be removed in 7.0.")]
    public static Task<Result> ThenDo(this Task<Result> task, Action action, CancellationToken cancellationToken) =>
        task.ThenDoAsync(action, cancellationToken);

    /// <summary>Obsolete alias of <c>ThenAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ThenAsync. Will be removed in 7.0.")]
    public static ValueTask<Result> Then(this ValueTask<Result> task, Func<Result> onSuccess, CancellationToken cancellationToken = default) =>
        task.ThenAsync(onSuccess, cancellationToken);

    /// <summary>Obsolete alias of <c>ThenDoAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ThenDoAsync. Will be removed in 7.0.")]
    public static ValueTask<Result> ThenDo(this ValueTask<Result> task, Action action, CancellationToken cancellationToken = default) =>
        task.ThenDoAsync(action, cancellationToken);

    /// <summary>Obsolete alias of <c>ElseAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ElseAsync. Will be removed in 7.0.")]
    public static Task<Result> Else(this Task<Result> task, Func<Error[], Error> onFailure, CancellationToken cancellationToken = default) =>
        task.ElseAsync(onFailure, cancellationToken);

    /// <summary>Obsolete alias of <c>ElseAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ElseAsync. Will be removed in 7.0.")]
    public static Task<Result> Else(this Task<Result> task, Func<Error[], IEnumerable<Error>> onFailure, CancellationToken cancellationToken = default) =>
        task.ElseAsync(onFailure, cancellationToken);

    /// <summary>Obsolete alias of <c>ElseAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ElseAsync. Will be removed in 7.0.")]
    public static Task<Result> Else(this Task<Result> task, Error error, CancellationToken cancellationToken = default) =>
        task.ElseAsync(error, cancellationToken);

    /// <summary>Obsolete alias of <c>ElseAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ElseAsync. Will be removed in 7.0.")]
    public static ValueTask<Result> Else(this ValueTask<Result> task, Func<Error[], Error> onFailure, CancellationToken cancellationToken = default) =>
        task.ElseAsync(onFailure, cancellationToken);

    /// <summary>Obsolete alias of <c>ElseAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ElseAsync. Will be removed in 7.0.")]
    public static ValueTask<Result> Else(this ValueTask<Result> task, Func<Error[], IEnumerable<Error>> onFailure, CancellationToken cancellationToken = default) =>
        task.ElseAsync(onFailure, cancellationToken);

    /// <summary>Obsolete alias of <c>ElseAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ElseAsync. Will be removed in 7.0.")]
    public static ValueTask<Result> Else(this ValueTask<Result> task, Error error, CancellationToken cancellationToken = default) =>
        task.ElseAsync(error, cancellationToken);
}
