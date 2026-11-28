using CSharpEssentials.ResultPattern;
using Mediator;

namespace Examples.Mediator.Testing;

public sealed class CapturePaymentHandler : ICommandHandler<CapturePayment, Result>
{
    public ValueTask<Result> Handle(CapturePayment command, CancellationToken cancellationToken) => new(Result.Success());
}
