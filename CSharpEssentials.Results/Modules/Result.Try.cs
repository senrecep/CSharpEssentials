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

    public static async Task<Result> TryAsync(Func<Task> action, Func<Exception, Error> errorHandler, CancellationToken cancellationToken = default)
    {
        try
        {
            await action().WithCancellation(cancellationToken);
            return Success();
        }
        catch (Exception ex)
        {
            return errorHandler(ex);
        }
    }

    public static async Task<Result<TValue>> TryAsync<TValue>(Func<Task<TValue>> func, Func<Exception, Error> errorHandler, CancellationToken cancellationToken = default)
    {
        try
        {
            return await func().WithCancellation(cancellationToken);
        }
        catch (Exception ex)
        {
            return errorHandler(ex);
        }
    }

    public static async Task<Result<TValue>> TryAsync<TValue>(Func<Task<Result<TValue>>> func, Func<Exception, Error> errorHandler, CancellationToken cancellationToken = default)
    {
        try
        {
            return await func().WithCancellation(cancellationToken);
        }
        catch (Exception ex)
        {
            return errorHandler(ex);
        }
    }
}
