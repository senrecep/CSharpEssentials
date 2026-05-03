using CSharpEssentials.Core;
namespace CSharpEssentials.Maybe;

public readonly partial struct Maybe<T>
{
    /// <summary>
    /// Maps the value if the condition is true.
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="map"></param>
    /// <returns></returns>
    public Maybe<T> MapIf(bool condition, Func<T, T> map) =>
        condition ? Map(map) : this;

    /// <summary>
    /// Maps the value if the predicate returns true.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="map"></param>
    /// <returns></returns>
    public Maybe<T> MapIf(Func<bool> predicate, Func<T, T> map) =>
        predicate() ? Map(map) : this;

    /// <summary>
    /// Maps the value if the predicate returns true.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="map"></param>
    /// <returns></returns>
    public Maybe<T> MapIf(Func<T, bool> predicate, Func<T, T> map) =>
        HasValue && predicate(Value) ? Map(map) : this;

    /// <summary>
    /// Asynchronously maps the value if the condition is true.
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="map"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Maybe<T>> MapIfAsync(bool condition, Func<T, Task<T>> map, CancellationToken cancellationToken = default) =>
        condition ? await MapAsync(map, cancellationToken) : this;

    /// <summary>
    /// Asynchronously maps the value if the predicate returns true.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="map"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Maybe<T>> MapIfAsync(Func<bool> predicate, Func<T, Task<T>> map, CancellationToken cancellationToken = default) =>
        predicate() ? await MapAsync(map, cancellationToken) : this;

    /// <summary>
    /// Asynchronously maps the value if the predicate returns true.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="map"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Maybe<T>> MapIfAsync(Func<T, bool> predicate, Func<T, Task<T>> map, CancellationToken cancellationToken = default) =>
        HasValue && predicate(Value) ? await MapAsync(map, cancellationToken) : this;
}

public static partial class MaybeExtensions
{
    /// <summary>
    /// Maps the value if the condition is true.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="condition"></param>
    /// <param name="map"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Maybe<T>> MapIfAsync<T>(this Task<Maybe<T>> maybeTask, bool condition, Func<T, T> map, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.MapIf(condition, map);
    }

    /// <summary>
    /// Maps the value if the predicate returns true.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="predicate"></param>
    /// <param name="map"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Maybe<T>> MapIfAsync<T>(this Task<Maybe<T>> maybeTask, Func<bool> predicate, Func<T, T> map, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.MapIf(predicate, map);
    }

    /// <summary>
    /// Maps the value if the predicate returns true.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="predicate"></param>
    /// <param name="map"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Maybe<T>> MapIfAsync<T>(this Task<Maybe<T>> maybeTask, Func<T, bool> predicate, Func<T, T> map, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.MapIf(predicate, map);
    }

    /// <summary>
    /// Asynchronously maps the value if the condition is true.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="condition"></param>
    /// <param name="map"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Maybe<T>> MapIfAsync<T>(this Task<Maybe<T>> maybeTask, bool condition, Func<T, Task<T>> map, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return await maybe.MapIfAsync(condition, map, cancellationToken);
    }

    /// <summary>
    /// Asynchronously maps the value if the predicate returns true.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="predicate"></param>
    /// <param name="map"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Maybe<T>> MapIfAsync<T>(this Task<Maybe<T>> maybeTask, Func<bool> predicate, Func<T, Task<T>> map, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return await maybe.MapIfAsync(predicate, map, cancellationToken);
    }

    /// <summary>
    /// Asynchronously maps the value if the predicate returns true.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="predicate"></param>
    /// <param name="map"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Maybe<T>> MapIfAsync<T>(this Task<Maybe<T>> maybeTask, Func<T, bool> predicate, Func<T, Task<T>> map, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return await maybe.MapIfAsync(predicate, map, cancellationToken);
    }

    /// <summary>
    /// Maps the value if the condition is true.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="condition"></param>
    /// <param name="map"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Maybe<T>> MapIfAsync<T>(this ValueTask<Maybe<T>> maybeTask, bool condition, Func<T, T> map, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.MapIf(condition, map);
    }

    /// <summary>
    /// Maps the value if the predicate returns true.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="predicate"></param>
    /// <param name="map"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Maybe<T>> MapIfAsync<T>(this ValueTask<Maybe<T>> maybeTask, Func<bool> predicate, Func<T, T> map, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.MapIf(predicate, map);
    }

    /// <summary>
    /// Maps the value if the predicate returns true.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="predicate"></param>
    /// <param name="map"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Maybe<T>> MapIfAsync<T>(this ValueTask<Maybe<T>> maybeTask, Func<T, bool> predicate, Func<T, T> map, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.MapIf(predicate, map);
    }

    /// <summary>
    /// Asynchronously maps the value if the condition is true.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="condition"></param>
    /// <param name="map"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Maybe<T>> MapIfAsync<T>(this ValueTask<Maybe<T>> maybeTask, bool condition, Func<T, Task<T>> map, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return await maybe.MapIfAsync(condition, map, cancellationToken);
    }

    /// <summary>
    /// Asynchronously maps the value if the predicate returns true.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="predicate"></param>
    /// <param name="map"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Maybe<T>> MapIfAsync<T>(this ValueTask<Maybe<T>> maybeTask, Func<bool> predicate, Func<T, Task<T>> map, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return await maybe.MapIfAsync(predicate, map, cancellationToken);
    }

    /// <summary>
    /// Asynchronously maps the value if the predicate returns true.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="predicate"></param>
    /// <param name="map"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Maybe<T>> MapIfAsync<T>(this ValueTask<Maybe<T>> maybeTask, Func<T, bool> predicate, Func<T, Task<T>> map, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return await maybe.MapIfAsync(predicate, map, cancellationToken);
    }
}
