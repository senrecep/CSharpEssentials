using CSharpEssentials.Idempotency;

namespace Examples.Idempotency.EndToEnd.App;

/// <summary>How the consumer settled one delivery of a message.</summary>
public sealed record MessageDelivery(string MessageId, IdempotencyExecutionStatus Status);
