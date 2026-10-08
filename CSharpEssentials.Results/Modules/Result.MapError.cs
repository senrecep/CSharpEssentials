using CSharpEssentials.Core;
using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result
{
    public Result MapError(Func<Error[], Error[]> errorMapper)
    {
        if (IsSuccess)
            return this;
        return errorMapper(Errors);
    }

    public Result MapError(Func<Error, Error> errorMapper)
    {
        if (IsSuccess)
            return this;
        var mappedErrors = new Error[_errors.Length];
        for (int i = 0; i < _errors.Length; i++)
            mappedErrors[i] = errorMapper(_errors[i]);
        return mappedErrors;
    }

    public Task<Result> MapErrorAsync(Func<Error, Task<Error>> errorMapper, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return this.AsTask();
        return MapEachErrorAsync(_errors, errorMapper, cancellationToken);
    }

    public ValueTask<Result> MapErrorAsync(Func<Error, ValueTask<Error>> errorMapper, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return this.AsValueTask();
        return MapEachErrorAsync(_errors, errorMapper, cancellationToken);
    }

    public Task<Result> MapErrorAsync(Func<Error[], Task<Error[]>> errorMapper, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return this.AsTask();
        return MapErrorsAsync(_errors, errorMapper, cancellationToken);
    }

    public ValueTask<Result> MapErrorAsync(Func<Error[], ValueTask<Error[]>> errorMapper, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return this.AsValueTask();
        return MapErrorsAsync(_errors, errorMapper, cancellationToken);
    }

    private static async Task<Result> MapEachErrorAsync(Error[] errors, Func<Error, Task<Error>> errorMapper, CancellationToken cancellationToken)
    {
        var mappedErrors = new Error[errors.Length];
        for (int i = 0; i < errors.Length; i++)
            mappedErrors[i] = await errorMapper(errors[i]).WithCancellation(cancellationToken);
        return mappedErrors;
    }

    private static async ValueTask<Result> MapEachErrorAsync(Error[] errors, Func<Error, ValueTask<Error>> errorMapper, CancellationToken cancellationToken)
    {
        var mappedErrors = new Error[errors.Length];
        for (int i = 0; i < errors.Length; i++)
            mappedErrors[i] = await errorMapper(errors[i]).WithCancellation(cancellationToken);
        return mappedErrors;
    }

    private static async Task<Result> MapErrorsAsync(Error[] errors, Func<Error[], Task<Error[]>> errorMapper, CancellationToken cancellationToken) =>
        await errorMapper(errors).WithCancellation(cancellationToken);

    private static async ValueTask<Result> MapErrorsAsync(Error[] errors, Func<Error[], ValueTask<Error[]>> errorMapper, CancellationToken cancellationToken) =>
        await errorMapper(errors).WithCancellation(cancellationToken);
}

public static partial class ResultExtensions
{
    public static async Task<Result> MapErrorAsync(this Task<Result> task, Func<Error, Error> errorMapper, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return result.MapError(errorMapper);
    }

    public static async Task<Result> MapErrorAsync(this Task<Result> task, Func<Error[], Error[]> errorMapper, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return result.MapError(errorMapper);
    }

    public static async Task<Result> MapErrorAsync(this Task<Result> task, Func<Error, Task<Error>> errorMapper, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return await result.MapErrorAsync(errorMapper, cancellationToken);
    }

    public static async Task<Result> MapErrorAsync(this Task<Result> task, Func<Error[], Task<Error[]>> errorMapper, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return await result.MapErrorAsync(errorMapper, cancellationToken);
    }

    public static async ValueTask<Result> MapErrorAsync(this ValueTask<Result> task, Func<Error, Error> errorMapper, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return result.MapError(errorMapper);
    }

    public static async ValueTask<Result> MapErrorAsync(this ValueTask<Result> task, Func<Error[], Error[]> errorMapper, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return result.MapError(errorMapper);
    }

    public static async ValueTask<Result> MapErrorAsync(this ValueTask<Result> task, Func<Error, ValueTask<Error>> errorMapper, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return await result.MapErrorAsync(errorMapper, cancellationToken);
    }

    public static async ValueTask<Result> MapErrorAsync(this ValueTask<Result> task, Func<Error[], ValueTask<Error[]>> errorMapper, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return await result.MapErrorAsync(errorMapper, cancellationToken);
    }
}
