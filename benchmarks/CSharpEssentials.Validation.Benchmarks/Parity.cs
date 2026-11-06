using CSharpEssentials.ResultPattern.Interfaces;
using Annotations = System.ComponentModel.DataAnnotations;
using FvResult = FluentValidation.Results.ValidationResult;

namespace CSharpEssentials.Validation.Benchmarks;

/// <summary>
/// Self-check run from every [GlobalSetup]. The key is <c>ok</c> or <c>fail:</c> plus the sorted, distinct failing property
/// paths. Error counts per property are left out: FluentValidation's default rule-level cascade is Continue and CSE's is Stop.
/// </summary>
internal static class Parity
{
    public static string Of(object outcome) => outcome switch
    {
        IResultBase result => Of(result),
        FvResult result => Of(result),
        _ => throw new InvalidOperationException($"No parity key for {outcome.GetType().Name}."),
    };

    public static string Of(IResultBase result) =>
        result.IsSuccess ? "ok" : Fail(result.Errors.Select(e => e.Code[..e.Code.LastIndexOf('.')]));

    public static string Of(FvResult result) =>
        result.IsValid ? "ok" : Fail(result.Errors.Select(e => e.PropertyName));

    public static string Of(List<Annotations.ValidationResult> results) =>
        results.Count == 0 ? "ok" : Fail(results.SelectMany(r => r.MemberNames));

    public static string Of(IReadOnlyDictionary<string, string[]>? errors) =>
        errors is null || errors.Count == 0 ? "ok" : Fail(errors.Keys);

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

    public static void Failed(string scenario, params (string Library, string Outcome)[] outcomes)
    {
        foreach ((string library, string outcome) in outcomes)
        {
            if (!outcome.StartsWith("fail:", StringComparison.Ordinal))
                throw new InvalidOperationException($"Self-check failed in {scenario}: {library} accepted the invalid input ('{outcome}').");
        }
    }

    public static T NotNull<T>(string scenario, T? value) where T : class =>
        value ?? throw new InvalidOperationException($"Self-check failed in {scenario}: {typeof(T).Name} was null.");

    private static string Fail(IEnumerable<string> paths) =>
        "fail:" + string.Join(",", paths.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal));
}
