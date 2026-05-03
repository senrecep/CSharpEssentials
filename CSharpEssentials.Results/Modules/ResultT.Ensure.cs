using CSharpEssentials.Core;
using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result<TValue>
{
    public Result<TValue> Ensure(Func<TValue, bool> predicate, Error error)
    {
        if (IsFailure)
            return this;
        return predicate(Value) ? this : error.ToResult<TValue>();
    }

    public Result<TValue> Ensure(Func<TValue, bool> predicate, Func<TValue, Error> errorFactory)
    {
        if (IsFailure)
            return this;
        return predicate(Value) ? this : errorFactory(Value).ToResult<TValue>();
    }

    public Result<TValue> EnsureNotNull(Error error)
    {
        if (IsFailure)
            return this;
        return Value is not null ? this : error.ToResult<TValue>();
    }

    public Result<TValue> EnsureNotNull(Func<TValue, Error> errorFactory)
    {
        if (IsFailure)
            return this;
        return Value is not null ? this : errorFactory(Value).ToResult<TValue>();
    }

    public async Task<Result<TValue>> EnsureAsync(Func<TValue, Task<bool>> predicate, Error error, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return this;
        return await predicate(Value).WithCancellation(cancellationToken) ? this : error.ToResult<TValue>();
    }

    public async Task<Result<TValue>> EnsureAsync(Func<TValue, Task<bool>> predicate, Func<TValue, Error> errorFactory, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return this;
        return await predicate(Value).WithCancellation(cancellationToken) ? this : errorFactory(Value).ToResult<TValue>();
    }
}

public static partial class ResultExtensions
{
    public static async Task<Result<TValue>> EnsureAsync<TValue>(this Task<Result<TValue>> task, Func<TValue, Task<bool>> predicate, Error error, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.EnsureAsync(predicate, error, cancellationToken);
    }

    public static async Task<Result<TValue>> EnsureAsync<TValue>(this Task<Result<TValue>> task, Func<TValue, Task<bool>> predicate, Func<TValue, Error> errorFactory, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.EnsureAsync(predicate, errorFactory, cancellationToken);
    }

    public static async ValueTask<Result<TValue>> EnsureAsync<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, Task<bool>> predicate, Error error, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.EnsureAsync(predicate, error, cancellationToken);
    }

    public static async ValueTask<Result<TValue>> EnsureAsync<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, Task<bool>> predicate, Func<TValue, Error> errorFactory, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.EnsureAsync(predicate, errorFactory, cancellationToken);
    }
}
