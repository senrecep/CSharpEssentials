using CSharpEssentials.Idempotency;
using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;
using Mediator;

namespace Examples.Mediator;

internal sealed record CapturePayment(string IdempotencyKey, string TenantId, decimal Amount)
    : ICommand<Result<Guid>>, IIdempotentRequest
{
    IdempotencyScope? IIdempotentRequest.IdempotencyScope => IdempotencyScope.Tenant(TenantId);
}

internal sealed class CapturePaymentHandler(Counters counters) : ICommandHandler<CapturePayment, Result<Guid>>
{
    public ValueTask<Result<Guid>> Handle(CapturePayment command, CancellationToken cancellationToken)
    {
        counters.Increment("CapturePayment");
        return new(Result<Guid>.Success(Guid.NewGuid()));
    }
}

internal sealed record RefundPayment(string IdempotencyKey) : ICommand<Result<Guid>>, IIdempotentRequest;

internal sealed class RefundPaymentHandler : ICommandHandler<RefundPayment, Result<Guid>>
{
    public ValueTask<Result<Guid>> Handle(RefundPayment command, CancellationToken cancellationToken) =>
        new(Result<Guid>.Success(Guid.NewGuid()));
}

internal sealed record ChargeCard(string IdempotencyKey) : ICommand<Result>, IIdempotentRequest
{
    IdempotencyScope? IIdempotentRequest.IdempotencyScope => IdempotencyScope.Global;
}

internal sealed class ChargeCardHandler : ICommandHandler<ChargeCard, Result>
{
    public ValueTask<Result> Handle(ChargeCard command, CancellationToken cancellationToken) =>
        throw new TimeoutException("The card provider did not answer.");
}
