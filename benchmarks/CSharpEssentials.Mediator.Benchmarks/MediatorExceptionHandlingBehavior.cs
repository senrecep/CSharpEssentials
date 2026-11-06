using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using Mediator;

namespace CSharpEssentials.Mediator.Benchmarks;

// https://github.com/martinothamar/Mediator/blob/v3.0.2/README.md#44-use-pipeline-behaviors
public sealed class MediatorExceptionHandlingBehavior : IPipelineBehavior<CreateOrder, Result<int>>
{
    public async ValueTask<Result<int>> Handle(CreateOrder message, MessageHandlerDelegate<CreateOrder, Result<int>> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next(message, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Result<int>.Failure(Error.Exception(ex));
        }
    }
}
