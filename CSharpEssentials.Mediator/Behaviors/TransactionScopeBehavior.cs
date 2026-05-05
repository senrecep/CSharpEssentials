using Mediator;
using System.Transactions;

namespace CSharpEssentials.Mediator;

public sealed class TransactionScopeBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ITransactionalRequest, IMessage
{
    public async ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);
        TResponse response = await next(message, cancellationToken);
        transactionScope.Complete();
        return response;
    }
}
