using CSharpEssentials.ResultPattern;

using Mediator;

namespace Examples.Idempotency.EndToEnd.App;

/// <summary>Writes the settlement the payment provider reported.</summary>
public sealed class RecordSettlementHandler(PaymentsDbContext db) : ICommandHandler<RecordSettlement, Result<Guid>>
{
    public async ValueTask<Result<Guid>> Handle(RecordSettlement command, CancellationToken cancellationToken)
    {
        Settlement settlement = new()
        {
            Id = Guid.NewGuid(),
            ProviderEventId = command.EventId,
            AccountId = command.AccountId,
            Amount = command.Amount,
        };
        db.Settlements.Add(settlement);
        await db.SaveChangesAsync(cancellationToken);
        return settlement.Id;
    }
}
