using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result<TValue>
{
    /// <summary>Obsolete alias of <c>BindAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use BindAsync. Will be removed in 7.0.")]
    public Task<Result<TOut>> Bind<TOut>(Func<TValue, Task<Result<TOut>>> func) =>
        BindAsync(func);

    /// <summary>Obsolete alias of <c>BindAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use BindAsync. Will be removed in 7.0.")]
    public Task<Result> Bind(Func<TValue, Task<Result>> func) =>
        BindAsync(func);

    /// <summary>Obsolete alias of <c>BindAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use BindAsync. Will be removed in 7.0.")]
    public ValueTask<Result<TOut>> Bind<TOut>(Func<TValue, ValueTask<Result<TOut>>> valueTask) =>
        BindAsync(valueTask);

    /// <summary>Obsolete alias of <c>BindAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use BindAsync. Will be removed in 7.0.")]
    public ValueTask<Result> Bind(Func<TValue, ValueTask<Result>> valueTask) =>
        BindAsync(valueTask);
}

public static partial class ResultExtensions
{
    /// <summary>Obsolete alias of <c>MatchAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use MatchAsync. Will be removed in 7.0.")]
    public static Task<T> Match<TValue, T>(this Task<Result<TValue>> task, Func<TValue, T> onSuccess, Func<Error[], T> onError, CancellationToken cancellationToken = default) =>
        task.MatchAsync<TValue, T>(onSuccess, onError, cancellationToken);

    /// <summary>Obsolete alias of <c>MatchFirstAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use MatchFirstAsync. Will be removed in 7.0.")]
    public static Task<T> MatchFirst<TValue, T>(this Task<Result<TValue>> task, Func<TValue, T> onSuccess, Func<Error, T> onError, CancellationToken cancellationToken = default) =>
        task.MatchFirstAsync<TValue, T>(onSuccess, onError, cancellationToken);

    /// <summary>Obsolete alias of <c>MatchLastAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use MatchLastAsync. Will be removed in 7.0.")]
    public static Task<T> MatchLast<TValue, T>(this Task<Result<TValue>> task, Func<TValue, T> onSuccess, Func<Error, T> onError, CancellationToken cancellationToken = default) =>
        task.MatchLastAsync<TValue, T>(onSuccess, onError, cancellationToken);

    /// <summary>Obsolete alias of <c>MatchAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use MatchAsync. Will be removed in 7.0.")]
    public static ValueTask<T> Match<TValue, T>(this ValueTask<Result<TValue>> task, Func<TValue, T> onSuccess, Func<Error[], T> onError, CancellationToken cancellationToken = default) =>
        task.MatchAsync<TValue, T>(onSuccess, onError, cancellationToken);

    /// <summary>Obsolete alias of <c>MatchFirstAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use MatchFirstAsync. Will be removed in 7.0.")]
    public static ValueTask<T> MatchFirst<TValue, T>(this ValueTask<Result<TValue>> task, Func<TValue, T> onSuccess, Func<Error, T> onError, CancellationToken cancellationToken = default) =>
        task.MatchFirstAsync<TValue, T>(onSuccess, onError, cancellationToken);

    /// <summary>Obsolete alias of <c>MatchLastAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use MatchLastAsync. Will be removed in 7.0.")]
    public static ValueTask<T> MatchLast<TValue, T>(this ValueTask<Result<TValue>> task, Func<TValue, T> onSuccess, Func<Error, T> onError, CancellationToken cancellationToken = default) =>
        task.MatchLastAsync<TValue, T>(onSuccess, onError, cancellationToken);

    /// <summary>Obsolete alias of <c>SwitchAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use SwitchAsync. Will be removed in 7.0.")]
    public static Task Switch<TValue>(this Task<Result<TValue>> task, Action<TValue> onSuccess, Action<Error[]> onError, CancellationToken cancellationToken = default) =>
        task.SwitchAsync<TValue>(onSuccess, onError, cancellationToken);

    /// <summary>Obsolete alias of <c>SwitchFirstAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use SwitchFirstAsync. Will be removed in 7.0.")]
    public static Task SwitchFirst<TValue>(this Task<Result<TValue>> task, Action<TValue> onSuccess, Action<Error> onError, CancellationToken cancellationToken = default) =>
        task.SwitchFirstAsync<TValue>(onSuccess, onError, cancellationToken);

    /// <summary>Obsolete alias of <c>SwitchLastAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use SwitchLastAsync. Will be removed in 7.0.")]
    public static Task SwitchLast<TValue>(this Task<Result<TValue>> task, Action<TValue> onSuccess, Action<Error> onError, CancellationToken cancellationToken = default) =>
        task.SwitchLastAsync<TValue>(onSuccess, onError, cancellationToken);

    /// <summary>Obsolete alias of <c>SwitchAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use SwitchAsync. Will be removed in 7.0.")]
    public static ValueTask Switch<TValue>(this ValueTask<Result<TValue>> task, Action<TValue> onSuccess, Action<Error[]> onError, CancellationToken cancellationToken = default) =>
        task.SwitchAsync<TValue>(onSuccess, onError, cancellationToken);

    /// <summary>Obsolete alias of <c>SwitchFirstAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use SwitchFirstAsync. Will be removed in 7.0.")]
    public static ValueTask SwitchFirst<TValue>(this ValueTask<Result<TValue>> task, Action<TValue> onSuccess, Action<Error> onError, CancellationToken cancellationToken = default) =>
        task.SwitchFirstAsync<TValue>(onSuccess, onError, cancellationToken);

    /// <summary>Obsolete alias of <c>SwitchLastAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use SwitchLastAsync. Will be removed in 7.0.")]
    public static ValueTask SwitchLast<TValue>(this ValueTask<Result<TValue>> task, Action<TValue> onSuccess, Action<Error> onError, CancellationToken cancellationToken = default) =>
        task.SwitchLastAsync<TValue>(onSuccess, onError, cancellationToken);

    /// <summary>Obsolete alias of <c>ThenAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ThenAsync. Will be removed in 7.0.")]
    public static Task<Result<T>> Then<TValue, T>(this Task<Result<TValue>> task, Func<TValue, Result<T>> onSuccess, CancellationToken cancellationToken = default) =>
        task.ThenAsync<TValue, T>(onSuccess, cancellationToken);

    /// <summary>Obsolete alias of <c>ThenAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ThenAsync. Will be removed in 7.0.")]
    public static Task<Result<T>> Then<TValue, T>(this Task<Result<TValue>> task, Func<TValue, T> onSuccess, CancellationToken cancellationToken = default) =>
        task.ThenAsync<TValue, T>(onSuccess, cancellationToken);

    /// <summary>Obsolete alias of <c>ThenDoAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ThenDoAsync. Will be removed in 7.0.")]
    public static Task<Result<TValue>> ThenDo<TValue>(this Task<Result<TValue>> task, Action<TValue> action, CancellationToken cancellationToken = default) =>
        task.ThenDoAsync<TValue>(action, cancellationToken);

    /// <summary>Obsolete alias of <c>ThenAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ThenAsync. Will be removed in 7.0.")]
    public static ValueTask<Result<T>> Then<TValue, T>(this ValueTask<Result<TValue>> task, Func<TValue, Result<T>> onSuccess, CancellationToken cancellationToken = default) =>
        task.ThenAsync<TValue, T>(onSuccess, cancellationToken);

    /// <summary>Obsolete alias of <c>ThenAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ThenAsync. Will be removed in 7.0.")]
    public static ValueTask<Result<T>> Then<TValue, T>(this ValueTask<Result<TValue>> task, Func<TValue, T> onSuccess, CancellationToken cancellationToken = default) =>
        task.ThenAsync<TValue, T>(onSuccess, cancellationToken);

    /// <summary>Obsolete alias of <c>ThenDoAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ThenDoAsync. Will be removed in 7.0.")]
    public static ValueTask<Result<TValue>> ThenDo<TValue>(this ValueTask<Result<TValue>> task, Action<TValue> action, CancellationToken cancellationToken = default) =>
        task.ThenDoAsync<TValue>(action, cancellationToken);

    /// <summary>Obsolete alias of <c>ElseAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ElseAsync. Will be removed in 7.0.")]
    public static Task<Result<TValue>> Else<TValue>(this Task<Result<TValue>> task, Func<Error[], TValue> onFailure, CancellationToken cancellationToken = default) =>
        task.ElseAsync<TValue>(onFailure, cancellationToken);

    /// <summary>Obsolete alias of <c>ElseAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ElseAsync. Will be removed in 7.0.")]
    public static Task<Result<TValue>> Else<TValue>(this Task<Result<TValue>> task, TValue onFailure, CancellationToken cancellationToken = default) =>
        task.ElseAsync<TValue>(onFailure, cancellationToken);

    /// <summary>Obsolete alias of <c>ElseAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ElseAsync. Will be removed in 7.0.")]
    public static Task<Result<TValue>> Else<TValue>(this Task<Result<TValue>> task, Func<Error[], Error> onFailure, CancellationToken cancellationToken = default) =>
        task.ElseAsync<TValue>(onFailure, cancellationToken);

    /// <summary>Obsolete alias of <c>ElseAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ElseAsync. Will be removed in 7.0.")]
    public static Task<Result<TValue>> Else<TValue>(this Task<Result<TValue>> task, Func<Error[], Error[]> onFailure, CancellationToken cancellationToken = default) =>
        task.ElseAsync<TValue>(onFailure, cancellationToken);

    /// <summary>Obsolete alias of <c>ElseAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ElseAsync. Will be removed in 7.0.")]
    public static Task<Result<TValue>> Else<TValue>(this Task<Result<TValue>> task, Error error, CancellationToken cancellationToken = default) =>
        task.ElseAsync<TValue>(error, cancellationToken);

    /// <summary>Obsolete alias of <c>ElseAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ElseAsync. Will be removed in 7.0.")]
    public static ValueTask<Result<TValue>> Else<TValue>(this ValueTask<Result<TValue>> task, Func<Error[], TValue> onFailure, CancellationToken cancellationToken = default) =>
        task.ElseAsync<TValue>(onFailure, cancellationToken);

    /// <summary>Obsolete alias of <c>ElseAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ElseAsync. Will be removed in 7.0.")]
    public static ValueTask<Result<TValue>> Else<TValue>(this ValueTask<Result<TValue>> task, TValue onFailure, CancellationToken cancellationToken = default) =>
        task.ElseAsync<TValue>(onFailure, cancellationToken);

    /// <summary>Obsolete alias of <c>ElseAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ElseAsync. Will be removed in 7.0.")]
    public static ValueTask<Result<TValue>> Else<TValue>(this ValueTask<Result<TValue>> task, Func<Error[], Error> onFailure, CancellationToken cancellationToken = default) =>
        task.ElseAsync<TValue>(onFailure, cancellationToken);

    /// <summary>Obsolete alias of <c>ElseAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ElseAsync. Will be removed in 7.0.")]
    public static ValueTask<Result<TValue>> Else<TValue>(this ValueTask<Result<TValue>> task, Func<Error[], Error[]> onFailure, CancellationToken cancellationToken = default) =>
        task.ElseAsync<TValue>(onFailure, cancellationToken);

    /// <summary>Obsolete alias of <c>ElseAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use ElseAsync. Will be removed in 7.0.")]
    public static ValueTask<Result<TValue>> Else<TValue>(this ValueTask<Result<TValue>> task, Error error, CancellationToken cancellationToken = default) =>
        task.ElseAsync<TValue>(error, cancellationToken);

    /// <summary>Obsolete alias of <c>FailIfAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use FailIfAsync. Will be removed in 7.0.")]
    public static Task<Result<TValue>> FailIf<TValue>(this Task<Result<TValue>> task, Func<TValue, bool> onSuccess, Error error, CancellationToken cancellationToken = default) =>
        task.FailIfAsync<TValue>(onSuccess, error, cancellationToken);

    /// <summary>Obsolete alias of <c>FailIfAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use FailIfAsync. Will be removed in 7.0.")]
    public static Task<Result<TValue>> FailIf<TValue>(this Task<Result<TValue>> task, Func<TValue, bool> onSuccess, Func<TValue, Error> func, CancellationToken cancellationToken = default) =>
        task.FailIfAsync<TValue>(onSuccess, func, cancellationToken);

    /// <summary>Obsolete alias of <c>FailIfAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use FailIfAsync. Will be removed in 7.0.")]
    public static ValueTask<Result<TValue>> FailIf<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, bool> onSuccess, Error error, CancellationToken cancellationToken = default) =>
        task.FailIfAsync<TValue>(onSuccess, error, cancellationToken);

    /// <summary>Obsolete alias of <c>FailIfAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use FailIfAsync. Will be removed in 7.0.")]
    public static ValueTask<Result<TValue>> FailIf<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, bool> onSuccess, Func<TValue, Error> func, CancellationToken cancellationToken = default) =>
        task.FailIfAsync<TValue>(onSuccess, func, cancellationToken);
}
