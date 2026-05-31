using Mediator;

using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Mediator;

/// <summary>
/// Pipeline behavior that catches unhandled handler exceptions and converts them
/// to <see cref="Result"/> / <see cref="Result{T}"/> failures.
/// <para>
/// When <typeparamref name="TResponse"/> cannot carry error information (e.g. a plain DTO),
/// the exception propagates unchanged — this behavior adds zero overhead in that case.
/// <see cref="OperationCanceledException"/> always propagates regardless of
/// <typeparamref name="TResponse"/>.
/// </para>
/// </summary>
public sealed class ExceptionHandlingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
{
    private readonly Type _responseType = typeof(TResponse);

    // Computed once at construction time — never pays the IsGenericType check on every request.
    private readonly bool _canHandleResponse;

    public ExceptionHandlingBehavior()
    {
        Type t = typeof(TResponse);
        _canHandleResponse = t == BehaviorCache.ResultType
            || t.IsGenericType && t.GetGenericTypeDefinition() == BehaviorCache.GenericResultType;
    }

    public ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        // When TResponse cannot carry error info, skip exception wrapping entirely — zero overhead.
        if (!_canHandleResponse)
            return next(message, cancellationToken);

        try
        {
            ValueTask<TResponse> vt = next(message, cancellationToken);
            // Sync-complete fast path — no state-machine allocation on the happy path.
            if (vt.IsCompletedSuccessfully)
                return vt;
            return WrapAsync(vt, _responseType);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new ValueTask<TResponse>(BuildFailureResponse(Error.Exception(ex), _responseType));
        }

        static async ValueTask<TResponse> WrapAsync(ValueTask<TResponse> vt, Type responseType)
        {
            try
            {
                return await vt.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return BuildFailureResponse(Error.Exception(ex), responseType);
            }
        }
    }

    /// <summary>
    /// Maps a single <paramref name="error"/> to <typeparamref name="TResponse"/>:
    /// <list type="bullet">
    ///   <item><see cref="Result"/> — <c>Result.Failure(error)</c> returned directly.</item>
    ///   <item><see cref="Result{T}"/> — <c>Result&lt;T&gt;.Failure(error)</c> via compiled factory.</item>
    /// </list>
    /// </summary>
    private static TResponse BuildFailureResponse(Error error, Type responseType)
    {
        if (responseType == BehaviorCache.ResultType)
            return (TResponse)(object)Result.Failure(error);

        return (TResponse)BehaviorCache.GetOrCreateFactory(responseType)([error]);
    }
}
