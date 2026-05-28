using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Resilience;

public static class ResilienceFuncExtensions
{
    public static async Task<Result> ExecuteAsync(
        this Func<Task> func,
        CancellationToken cancellationToken = default)
    {
        return await ResiliencePolicy.Create()
            .ExecuteAsync(async _ => await func(), cancellationToken);
    }

    public static async Task<Result<T>> ExecuteAsync<T>(
        this Func<Task<T>> func,
        CancellationToken cancellationToken = default)
    {
        return await ResiliencePolicy<T>.Create()
            .ExecuteAsync(async _ => await func(), cancellationToken);
    }

    public static async Task<Result<T>> ExecuteAsync<T>(
        this Func<Task<Result<T>>> func,
        CancellationToken cancellationToken = default)
    {
        return await ResiliencePolicy<T>.Create()
            .ExecuteAsync(async _ => await func(), cancellationToken);
    }

    public static async Task<Result> ExecuteAsync(
        this Func<CancellationToken, Task> func,
        CancellationToken cancellationToken = default)
    {
        return await ResiliencePolicy.Create()
            .ExecuteAsync(func, cancellationToken);
    }

    public static async Task<Result<T>> ExecuteAsync<T>(
        this Func<CancellationToken, Task<T>> func,
        CancellationToken cancellationToken = default)
    {
        return await ResiliencePolicy<T>.Create()
            .ExecuteAsync(func, cancellationToken);
    }

    public static async Task<Result<T>> ExecuteAsync<T>(
        this Func<CancellationToken, Task<Result<T>>> func,
        CancellationToken cancellationToken = default)
    {
        return await ResiliencePolicy<T>.Create()
            .ExecuteAsync(func, cancellationToken);
    }
}
