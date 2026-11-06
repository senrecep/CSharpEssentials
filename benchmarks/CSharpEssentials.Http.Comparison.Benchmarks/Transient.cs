using Polly;

namespace CSharpEssentials.Http.Comparison.Benchmarks;

/// <summary>Hand-written Polly predicates that mirror the CSE classifier: exceptions and failed results are transient, except Unauthorized, Forbidden, NotFound and Validation.</summary>
internal static class Transient
{
    public static bool IsTransient(Outcome<Result<int>> outcome) =>
        outcome.Exception is not null
        || outcome.Result.IsFailure
            && outcome.Result.FirstError.Type is not (ErrorType.Unauthorized or ErrorType.Forbidden or ErrorType.NotFound or ErrorType.Validation);
}
