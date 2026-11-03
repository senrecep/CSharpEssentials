using CSharpEssentials.ResultPattern;
using Mediator;

namespace CSharpEssentials.Mediator.Benchmarks;

public sealed class MediatorCreateCustomerHandler : ICommandHandler<CreateCustomer, Result<int>>
{
    public ValueTask<Result<int>> Handle(CreateCustomer command, CancellationToken cancellationToken)
    {
        HandlerProbe.HandlerCalls++;
        return new(Result<int>.Success(command.Age + 1));
    }
}
