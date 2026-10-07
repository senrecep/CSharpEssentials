using CSharpEssentials.ResultPattern.Interfaces;

namespace CSharpEssentials.Mediator;

internal static class TransactionOutcome
{
    public static bool ShouldCommit<TResponse>(TResponse response) =>
        response is not IResultBase { IsFailure: true };
}
