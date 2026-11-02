namespace CSharpEssentials.Results.Comparison.Benchmarks;

/// <summary>
/// Self-check run from every [GlobalSetup]: each library must produce the same outcome (success value, or error count
/// and first error message) for the same input, otherwise the benchmark refuses to run.
/// </summary>
internal static class Parity
{
    public static string Of(Result<int> r) => r.IsSuccess ? Ok(r.Value) : Fail(r.Errors.Length, r.Errors[0].Description);

    public static string Of(Result r) => r.IsSuccess ? "ok" : Fail(r.Errors.Length, r.Errors[0].Description);

    /// <summary>CFE keeps one string; Combine joins messages with ", ".</summary>
    public static string Of(Cfe.Result<int> r) => r.IsSuccess ? Ok(r.Value) : CfeFail(r.Error);

    public static string Of(Cfe.Result r) => r.IsSuccess ? "ok" : CfeFail(r.Error);

    public static string Of(Eo.ErrorOr<int> r) => r.IsError ? Fail(r.Errors.Count, r.FirstError.Description) : Ok(r.Value);

    public static string Of(Eo.ErrorOr<Eo.Success> r) => r.IsError ? Fail(r.Errors.Count, r.FirstError.Description) : "ok";

    public static string Of(Fr.Result<int> r) => r.IsSuccess ? Ok(r.Value) : Fail(r.Errors.Count, r.Errors[0].Message);

    public static string Of(Fr.Result r) => r.IsSuccess ? "ok" : Fail(r.Errors.Count, r.Errors[0].Message);

    public static string Of(Maybe<int> m) => m.HasValue ? Ok(m.GetValueOrDefault(0)) : "none";

    public static string Of(Cfe.Maybe<int> m) => m.HasValue ? Ok(m.GetValueOrDefault(0)) : "none";

    public static string Of(int value) => Ok(value);

    public static string Of(string value) => value;

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

    private static string Ok(int value) => "ok:" + value.ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static string Fail(int count, string message) =>
        "fail:" + count.ToString(System.Globalization.CultureInfo.InvariantCulture) + ":" + message;

    private static string CfeFail(string error)
    {
        string[] parts = error.Split(", ");
        return Fail(parts.Length, parts[0]);
    }
}
