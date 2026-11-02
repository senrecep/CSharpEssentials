using CSharpEssentials.ResultPattern;
using MediatR;

namespace CSharpEssentials.Mediator.Benchmarks;

public sealed class MediatRCreateOrderHandler : IRequestHandler<CreateOrder, Result<int>>
{
    public Task<Result<int>> Handle(CreateOrder request, CancellationToken cancellationToken) =>
        request.Name == MediatorSelfCheck.ThrowingName
            ? throw new InvalidOperationException(MediatorSelfCheck.ThrowingName)
            : Task.FromResult(Result<int>.Success(request.Quantity + 1));
}
