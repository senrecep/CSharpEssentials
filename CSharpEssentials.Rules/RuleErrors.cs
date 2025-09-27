using CSharpEssentials.Errors;

namespace CSharpEssentials.Rules;

internal static class RuleErrors
{
    internal static Error RuleEngineEvaluateError(string ruleType, Exception ex) => Error.Exception($"RuleEngine.Evaluate.{ruleType}", ex);
    internal static Error RuleEngineNotFoundError(string ruleType) => Error.NotFound($"RuleEngine.Evaluate.{ruleType}");

    internal static Error EmptyRuleArrayError => Error.Validation(
        code: "EMPTY_RULE_ARRAY",
        description: "The rule array is empty."
    );
}
