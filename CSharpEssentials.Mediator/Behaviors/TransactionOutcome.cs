using CSharpEssentials.ResultPattern.Interfaces;

namespace CSharpEssentials.Mediator;

internal static class TransactionOutcome
{
    public static bool ShouldCommit<TResponse>(TResponse response) =>
#if NET10_0_OR_GREATER
        response is not IResultBase { IsFailure: true };
#else
        !ResponseFailure.IsFailure(response);
#endif
}
