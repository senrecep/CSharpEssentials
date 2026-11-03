using CSharpEssentials.ResultPattern;
using MediatR;

namespace CSharpEssentials.Mediator.Benchmarks;

public sealed class MediatRCreateCustomerHandler : IRequestHandler<CreateCustomer, Result<int>>
{
    public Task<Result<int>> Handle(CreateCustomer request, CancellationToken cancellationToken)
    {
        HandlerProbe.HandlerCalls++;
        return Task.FromResult(Result<int>.Success(request.Age + 1));
    }
}
