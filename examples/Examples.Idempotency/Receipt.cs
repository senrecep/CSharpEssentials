namespace Examples.Idempotency;

internal sealed record Receipt(Guid Id, decimal Amount, bool Declined);
