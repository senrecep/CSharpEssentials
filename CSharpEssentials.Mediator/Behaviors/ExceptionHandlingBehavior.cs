using Mediator;

using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Mediator;

/// <summary>
/// Pipeline behavior that catches unhandled handler exceptions and converts them
/// to <see cref="Result"/> / <see cref="Result{T}"/> failures.
/// <para>
/// When <typeparamref name="TResponse"/> cannot carry error information (e.g. a plain DTO),
/// the exception propagates unchanged: this behavior adds zero overhead in that case.
/// <see cref="OperationCanceledException"/> always propagates regardless of
/// <typeparamref name="TResponse"/>.
/// </para>
/// </summary>
public sealed class ExceptionHandlingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
{
    public ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        // When TResponse cannot carry error info, skip exception wrapping entirely, zero overhead.
        if (FailureResponse<TResponse>.Factory is null)
            return next(message, cancellationToken);

        try
        {
            ValueTask<TResponse> vt = next(message, cancellationToken);
            // Sync-complete fast path, no state-machine allocation on the happy path.
            if (vt.IsCompletedSuccessfully)
                return vt;
            return WrapAsync(vt);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new ValueTask<TResponse>(BuildFailureResponse(Error.Exception(ex)));
        }

        static async ValueTask<TResponse> WrapAsync(ValueTask<TResponse> vt)
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
                return BuildFailureResponse(Error.Exception(ex));
            }
        }
    }

    private static TResponse BuildFailureResponse(Error error) => FailureResponse<TResponse>.Factory!.CreateFailure([error]);
}
