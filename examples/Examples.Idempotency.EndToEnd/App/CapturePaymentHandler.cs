using CSharpEssentials.Errors;
using CSharpEssentials.Idempotency;
using CSharpEssentials.ResultPattern;

using Mediator;

namespace Examples.Idempotency.EndToEnd.App;

/// <summary>
/// Writes the payment, charges it through the payment provider and stores the charge id. It runs inside the
/// transaction of <c>TransactionBehavior</c>, after <c>IdempotencyBehavior</c> claimed the key in the same transaction.
/// The provider call is outside that transaction, so the provider gets a key derived from the merchant and the
/// payment's key (<see cref="IdempotencyKeys.For"/>): when the transaction rolls back after the charge and the payment
/// is retried, the provider returns the first charge instead of charging twice.
/// </summary>
public sealed class CapturePaymentHandler(PaymentsDbContext db, IPaymentProvider provider)
    : ICommandHandler<CapturePayment, Result<CapturedPayment>>
{
    public const string ChargeOperation = "charge";

    public async ValueTask<Result<CapturedPayment>> Handle(CapturePayment command, CancellationToken cancellationToken)
    {
        if (command.Amount <= 0)
            return Error.Validation("Payment.InvalidAmount", "The amount must be positive.");

        Payment payment = new()
        {
            Id = Guid.NewGuid(),
            MerchantId = command.MerchantId,
            Reference = command.Reference,
            Amount = command.Amount,
        };
        db.Payments.Add(payment);
        await db.SaveChangesAsync(cancellationToken);

        string chargeKey = IdempotencyKeys.For(ChargeOperation, command.MerchantId, command.Key);
        payment.ProviderChargeId = await provider.ChargeAsync(chargeKey, command.MerchantId, command.Reference, command.Amount, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return new CapturedPayment(payment.Id, payment.ProviderChargeId, payment.Amount);
    }
}
