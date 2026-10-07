using CSharpEssentials.Transactions;

using Mediator;

namespace CSharpEssentials.Mediator;

/// <summary>
/// Runs <see cref="ITransactionalRequest"/> handlers through the registered <see cref="ITransactionRunner"/>.
/// The transaction commits unless the handler returns a failed <c>Result</c> / <c>Result&lt;T&gt;</c> or throws.
/// </summary>
public sealed class TransactionBehavior<TRequest, TResponse>(ITransactionRunner transactionRunner)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ITransactionalRequest, IMessage
{
    public ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken) =>
        transactionRunner.ExecuteAsync(
            token => next(message, token),
            TransactionOutcome.ShouldCommit,
            cancellationToken);
}
