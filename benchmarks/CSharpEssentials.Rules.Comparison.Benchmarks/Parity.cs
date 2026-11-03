using RulesEngine.Models;

namespace CSharpEssentials.Rules.Comparison.Benchmarks;

/// <summary>
/// Self-check run from every [GlobalSetup]. The key is <c>ok</c> or <c>fail:&lt;error count&gt;:&lt;first error code&gt;</c>; for the
/// reference engines the first code is the first failing rule name in ordinal order.
/// </summary>
internal static class Parity
{
    public static string Of(Result result) => result.IsSuccess ? "ok" : Fail(result.Errors.Length, result.Errors[0].Code);

    public static string Of(Result<int> result) => result.IsSuccess ? "ok" : Fail(result.Errors.Length, result.Errors[0].Code);

    public static string Of(Violations violations) => Of(violations.Names);

    public static string Of(List<RuleResultTree> results) => Of([.. results.Where(r => !r.IsSuccess).Select(r => r.Rule.RuleName)]);

    public static void Same(string scenario, params (string Library, string Outcome)[] outcomes)
    {
        (string firstLibrary, string expected) = outcomes[0];
        foreach ((string library, string outcome) in outcomes)
        {
            if (!string.Equals(outcome, expected, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Parity check failed in {scenario}: {firstLibrary} gave '{expected}' but {library} gave '{outcome}'.");
            }
        }
    }

    private static string Of(List<string> names) =>
        names.Count == 0 ? "ok" : Fail(names.Count, names.Order(StringComparer.Ordinal).First());

    private static string Fail(int count, string code) =>
        "fail:" + count.ToString(System.Globalization.CultureInfo.InvariantCulture) + ":" + code;
}
