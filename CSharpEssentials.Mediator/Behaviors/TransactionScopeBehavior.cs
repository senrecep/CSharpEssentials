using Mediator;
using System.Transactions;

namespace CSharpEssentials.Mediator;

/// <summary>
/// Wraps <see cref="ITransactionalRequest"/> handlers in an ambient <see cref="TransactionScope"/>.
/// The scope completes unless the handler returns a failed <c>Result</c> / <c>Result&lt;T&gt;</c> or throws.
/// </summary>
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
        TResponse response = await next(message, cancellationToken).ConfigureAwait(false);
        if (TransactionOutcome.ShouldCommit(response))
            transactionScope.Complete();
        return response;
    }
}
