using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using MediatR;

namespace CSharpEssentials.Mediator.Benchmarks;

// https://github.com/jbogard/MediatR/blob/v12.5.0/README.md#registering-with-iservicecollection
// https://github.com/jbogard/MediatR/blob/v12.5.0/src/MediatR/IPipelineBehavior.cs
public sealed class MediatRExceptionHandlingBehavior : IPipelineBehavior<CreateOrder, Result<int>>
{
    public async Task<Result<int>> Handle(CreateOrder request, RequestHandlerDelegate<Result<int>> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Result<int>.Failure(Error.Exception(ex));
        }
    }
}
