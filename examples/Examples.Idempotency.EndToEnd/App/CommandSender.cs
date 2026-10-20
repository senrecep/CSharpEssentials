using Mediator;

using Microsoft.Extensions.DependencyInjection;

namespace Examples.Idempotency.EndToEnd.App;

/// <summary>
/// Sends a command through the registered <see cref="IPipelineBehavior{TMessage, TResponse}"/> behaviors to its
/// handler, in registration order (the first registered behavior is the outermost). An application that references
/// the Mediator source generator sends through its generated <c>IMediator</c> instead; this example only references
/// <c>Mediator.Abstractions</c>, so it composes the same pipeline from the service provider.
/// </summary>
public sealed class CommandSender(IServiceProvider services)
{
    public ValueTask<TResponse> SendAsync<TCommand, TResponse>(TCommand command, CancellationToken cancellationToken)
        where TCommand : ICommand<TResponse>
    {
        ICommandHandler<TCommand, TResponse> handler = services.GetRequiredService<ICommandHandler<TCommand, TResponse>>();
        MessageHandlerDelegate<TCommand, TResponse> next = (message, token) => handler.Handle(message, token);
        foreach (IPipelineBehavior<TCommand, TResponse> behavior in services.GetServices<IPipelineBehavior<TCommand, TResponse>>().Reverse())
        {
            MessageHandlerDelegate<TCommand, TResponse> inner = next;
            next = (message, token) => behavior.Handle(message, inner, token);
        }

        return next(command, cancellationToken);
    }
}
