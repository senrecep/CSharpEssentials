using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace CSharpEssentials.Core;

public static class GeneralExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEmpty(
        [NotNullWhen(returnValue: false)] this string? str) =>
        string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNotEmpty(
        [NotNullWhen(returnValue: true)] this string? str) => !str.IsEmpty();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNotNull<T>(
        [NotNullWhen(returnValue: true)] this T? item) => item is not null;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNull<T>(
        [NotNullWhen(returnValue: false)] this T? item) => item is null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsTrue(this bool value) => value;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsFalse(this bool value) => !value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ExplicitCast<T>(this object obj) => (T)obj;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToStringFromGuid(this Guid value) => Guider.ToStringFromGuid(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid ToGuidFromString(this ReadOnlySpan<char> id) => Guider.ToGuidFromString(id);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid ToGuidFromString(this string id) => Guider.ToGuidFromString(id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime MsToDateTime(this long value) =>
        DateTimeOffset.FromUnixTimeMilliseconds(value).DateTime;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TGroup GetTypeGroup<TGroup, TType>(this TType type, int group = 100)
        where TGroup : Enum
        where TType : Enum, IConvertible
    {
        int groupValue = type.ToInt32(null) / group * group;
        return (TGroup)Enum.ToObject(typeof(TGroup), groupValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<T> AsTask<T>(this T obj) => Task.FromResult(obj);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<T> AsValueTask<T>(this T obj) => ValueTask.FromResult(obj);

    public static async Task WithCancellation(this Task task, CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<bool>();
        await using (cancellationToken.Register(() => tcs.TrySetResult(true), useSynchronizationContext: false))
            if (task != await Task.WhenAny(task, tcs.Task))
                throw new OperationCanceledException(cancellationToken);

        await task.ConfigureAwait(false);
    }

    public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<bool>();
        await using (cancellationToken.Register(() => tcs.TrySetResult(true), useSynchronizationContext: false))
            if (task != await Task.WhenAny(task, tcs.Task))
                throw new OperationCanceledException(cancellationToken);

        return await task.ConfigureAwait(false);
    }

    public static async ValueTask<T> WithCancellation<T>(this ValueTask<T> valueTask, CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<bool>();
        await using (cancellationToken.Register(() => tcs.TrySetResult(true), useSynchronizationContext: false))
        {
            Task<T> task = valueTask.AsTask();

            if (task != await Task.WhenAny(task, tcs.Task))
            {
                throw new OperationCanceledException(cancellationToken);
            }

            return await task.ConfigureAwait(false);
        }
    }

    public static async ValueTask WithCancellation(this ValueTask valueTask, CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<bool>();
        await using (cancellationToken.Register(() => tcs.TrySetResult(true), useSynchronizationContext: false))
        {
            Task task = valueTask.AsTask();

            if (task != await Task.WhenAny(task, tcs.Task))
            {
                throw new OperationCanceledException(cancellationToken);
            }

            await task.ConfigureAwait(false);
        }
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IfTrue(this bool condition, Action action)
    {
        if (condition)
            action();
        return condition;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IfFalse(this bool condition, Action action)
    {
        if (!condition)
            action();
        return condition;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IfNotNull<T>(
        [NotNullWhen(returnValue: true)] this T? obj, Action<T> action)
    {
        if (obj.IsNotNull())
            action(obj);
        return obj.IsNotNull();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IfNotNull<T>(
         [NotNullWhen(returnValue: true)] this T? obj, Action action)
    {
        if (obj.IsNotNull())
            action();
        return obj.IsNotNull();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IfNotNull<T>(
        [NotNullWhen(returnValue: true)] this T? obj, Action<T> action, Action elseAction)
    {
        if (obj.IsNotNull())
            action(obj);
        else
            elseAction();
        return obj.IsNotNull();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IfNotNull<T>(
        [NotNullWhen(returnValue: true)] this T? obj, Action action, Action elseAction)
    {
        if (obj.IsNotNull())
            action();
        else
            elseAction();
        return obj.IsNotNull();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IfNull<T>(
        [NotNullWhen(returnValue: false)] this T? obj, Action action)
    {
        if (obj.IsNull())
            action();
        return obj.IsNull();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IfNull<T>(
        [NotNullWhen(returnValue: false)] this T? obj, Action action, Action<T> elseAction)
    {
        if (obj.IsNull())
            action();
        else
            elseAction(obj);
        return obj.IsNull();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IfNull<T>(
        [NotNullWhen(returnValue: false)] this T? obj, Action action, Action elseAction)
    {
        if (obj.IsNull())
            action();
        else
            elseAction();
        return obj.IsNull();
    }
}
