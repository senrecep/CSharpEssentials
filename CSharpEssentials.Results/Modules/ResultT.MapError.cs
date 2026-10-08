using CSharpEssentials.Core;
using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result<TValue>
{
    public Result<TValue> MapError(Func<Error[], Error[]> errorMapper)
    {
        if (IsSuccess)
            return this;
        return errorMapper(Errors);
    }

    public Result<TValue> MapError(Func<Error, Error> errorMapper)
    {
        if (IsSuccess)
            return this;
        var mappedErrors = new Error[_errors.Length];
        for (int i = 0; i < _errors.Length; i++)
            mappedErrors[i] = errorMapper(_errors[i]);
        return mappedErrors;
    }

    public Task<Result<TValue>> MapErrorAsync(Func<Error, Task<Error>> errorMapper, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return this.AsTask();
        return MapEachErrorAsync(_errors, errorMapper, cancellationToken);
    }

    public ValueTask<Result<TValue>> MapErrorAsync(Func<Error, ValueTask<Error>> errorMapper, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return this.AsValueTask();
        return MapEachErrorAsync(_errors, errorMapper, cancellationToken);
    }

    public Task<Result<TValue>> MapErrorAsync(Func<Error[], Task<Error[]>> errorMapper, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return this.AsTask();
        return MapErrorsAsync(_errors, errorMapper, cancellationToken);
    }

    public ValueTask<Result<TValue>> MapErrorAsync(Func<Error[], ValueTask<Error[]>> errorMapper, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return this.AsValueTask();
        return MapErrorsAsync(_errors, errorMapper, cancellationToken);
    }

    private static async Task<Result<TValue>> MapEachErrorAsync(Error[] errors, Func<Error, Task<Error>> errorMapper, CancellationToken cancellationToken)
    {
        var mappedErrors = new Error[errors.Length];
        for (int i = 0; i < errors.Length; i++)
            mappedErrors[i] = await errorMapper(errors[i]).WithCancellation(cancellationToken);
        return mappedErrors;
    }

    private static async ValueTask<Result<TValue>> MapEachErrorAsync(Error[] errors, Func<Error, ValueTask<Error>> errorMapper, CancellationToken cancellationToken)
    {
        var mappedErrors = new Error[errors.Length];
        for (int i = 0; i < errors.Length; i++)
            mappedErrors[i] = await errorMapper(errors[i]).WithCancellation(cancellationToken);
        return mappedErrors;
    }

    private static async Task<Result<TValue>> MapErrorsAsync(Error[] errors, Func<Error[], Task<Error[]>> errorMapper, CancellationToken cancellationToken) =>
        await errorMapper(errors).WithCancellation(cancellationToken);

    private static async ValueTask<Result<TValue>> MapErrorsAsync(Error[] errors, Func<Error[], ValueTask<Error[]>> errorMapper, CancellationToken cancellationToken) =>
        await errorMapper(errors).WithCancellation(cancellationToken);
}

public static partial class ResultExtensions
{
    public static async Task<Result<TValue>> MapErrorAsync<TValue>(this Task<Result<TValue>> task, Func<Error, Error> errorMapper, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.MapError(errorMapper);
    }

    public static async Task<Result<TValue>> MapErrorAsync<TValue>(this Task<Result<TValue>> task, Func<Error[], Error[]> errorMapper, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.MapError(errorMapper);
    }

    public static async Task<Result<TValue>> MapErrorAsync<TValue>(this Task<Result<TValue>> task, Func<Error, Task<Error>> errorMapper, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.MapErrorAsync(errorMapper, cancellationToken);
    }

    public static async Task<Result<TValue>> MapErrorAsync<TValue>(this Task<Result<TValue>> task, Func<Error[], Task<Error[]>> errorMapper, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.MapErrorAsync(errorMapper, cancellationToken);
    }

    public static async ValueTask<Result<TValue>> MapErrorAsync<TValue>(this ValueTask<Result<TValue>> task, Func<Error, Error> errorMapper, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.MapError(errorMapper);
    }

    public static async ValueTask<Result<TValue>> MapErrorAsync<TValue>(this ValueTask<Result<TValue>> task, Func<Error[], Error[]> errorMapper, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.MapError(errorMapper);
    }

    public static async ValueTask<Result<TValue>> MapErrorAsync<TValue>(this ValueTask<Result<TValue>> task, Func<Error, ValueTask<Error>> errorMapper, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.MapErrorAsync(errorMapper, cancellationToken);
    }

    public static async ValueTask<Result<TValue>> MapErrorAsync<TValue>(this ValueTask<Result<TValue>> task, Func<Error[], ValueTask<Error[]>> errorMapper, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.MapErrorAsync(errorMapper, cancellationToken);
    }
}
