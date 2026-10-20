namespace CSharpEssentials.Tests.Idempotency;

internal sealed record IdempotencyDecision(string Decision, string Mode, string Source, int TagCount);
