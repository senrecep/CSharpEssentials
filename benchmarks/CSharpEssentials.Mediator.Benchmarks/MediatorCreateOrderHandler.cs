using CSharpEssentials.ResultPattern;
using Mediator;

namespace CSharpEssentials.Mediator.Benchmarks;

public sealed class MediatorCreateOrderHandler : ICommandHandler<CreateOrder, Result<int>>
{
    public ValueTask<Result<int>> Handle(CreateOrder command, CancellationToken cancellationToken) =>
        command.Name == MediatorSelfCheck.ThrowingName
            ? throw new InvalidOperationException(MediatorSelfCheck.ThrowingName)
            : new(Result<int>.Success(command.Quantity + 1));
}
