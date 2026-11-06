using MediatR;

namespace CSharpEssentials.Mediator.Benchmarks;

// https://github.com/jbogard/MediatR/blob/v12.5.0/README.md#registering-with-iservicecollection
// https://github.com/jbogard/MediatR/blob/v12.5.0/src/MediatR/IPipelineBehavior.cs
public sealed class MediatRPassThroughA<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        Probe.BehaviorCalls++;
        return next(cancellationToken);
    }
}
