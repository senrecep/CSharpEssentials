using System.Runtime.CompilerServices;
using CSharpEssentials.Core;
using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result
{
    public static Result Try(Action action, Func<Exception, Error> errorHandler)
    {
        try
        {
            action();
            return Success();
        }
        catch (Exception ex)
        {
            return errorHandler(ex);
        }
    }

    public static Result<TValue> Try<TValue>(Func<TValue> func, Func<Exception, Error> errorHandler)
    {
        try
        {
            return func();
        }
        catch (Exception ex)
        {
            return errorHandler(ex);
        }
    }

    public static Result<TValue> Try<TValue>(Func<Result<TValue>> func, Func<Exception, Error> errorHandler)
    {
        try
        {
            return func();
        }
        catch (Exception ex)
        {
            return errorHandler(ex);
        }
    }

    public static Result Try(Func<Result> func, Func<Exception, Error> errorHandler)
    {
        try
        {
            return func();
        }
        catch (Exception ex)
        {
            return errorHandler(ex);
        }
    }

    [OverloadResolutionPriority(1)]
    public static async Task<Result> TryAsync(Func<Task> action, Func<Exception, Error> errorHandler, CancellationToken cancellationToken = default)
    {
        try
        {
            await action().WithCancellation(cancellationToken).ConfigureAwait(false);
            return Success();
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            return errorHandler(ex);
        }
    }

    [OverloadResolutionPriority(1)]
    public static async Task<Result<TValue>> TryAsync<TValue>(Func<Task<TValue>> func, Func<Exception, Error> errorHandler, CancellationToken cancellationToken = default)
    {
        try
        {
            return await func().WithCancellation(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            return errorHandler(ex);
        }
    }

    [OverloadResolutionPriority(1)]
    public static async Task<Result<TValue>> TryAsync<TValue>(Func<Task<Result<TValue>>> func, Func<Exception, Error> errorHandler, CancellationToken cancellationToken = default)
    {
        try
        {
            return await func().WithCancellation(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            return errorHandler(ex);
        }
    }

    [OverloadResolutionPriority(1)]
    public static async Task<Result> TryAsync(Func<Task<Result>> func, Func<Exception, Error> errorHandler, CancellationToken cancellationToken = default)
    {
        try
        {
            return await func().WithCancellation(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            return errorHandler(ex);
        }
    }

#if NET9_0_OR_GREATER
    public static async ValueTask<Result> TryAsync(Func<ValueTask> action, Func<Exception, Error> errorHandler, CancellationToken cancellationToken = default)
    {
        try
        {
            await action().WithCancellation(cancellationToken).ConfigureAwait(false);
            return Success();
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            return errorHandler(ex);
        }
    }

    public static async ValueTask<Result<TValue>> TryAsync<TValue>(Func<ValueTask<TValue>> func, Func<Exception, Error> errorHandler, CancellationToken cancellationToken = default)
    {
        try
        {
            return await func().WithCancellation(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            return errorHandler(ex);
        }
    }

    public static async ValueTask<Result<TValue>> TryAsync<TValue>(Func<ValueTask<Result<TValue>>> func, Func<Exception, Error> errorHandler, CancellationToken cancellationToken = default)
    {
        try
        {
            return await func().WithCancellation(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            return errorHandler(ex);
        }
    }

    public static async ValueTask<Result> TryAsync(Func<ValueTask<Result>> func, Func<Exception, Error> errorHandler, CancellationToken cancellationToken = default)
    {
        try
        {
            return await func().WithCancellation(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            return errorHandler(ex);
        }
    }
#endif
}
