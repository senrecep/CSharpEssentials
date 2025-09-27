using CSharpEssentials.Errors;

namespace CSharpEssentials.Rules;

public static class RuleErrors
{
    public static Error RuleEngineEvaluateError(string ruleType, Exception ex) => Error.Exception($"RuleEngine.Evaluate.{ruleType}", ex);
    public static Error RuleEngineNotFoundError(string ruleType) => Error.NotFound($"RuleEngine.Evaluate.{ruleType}");
}
