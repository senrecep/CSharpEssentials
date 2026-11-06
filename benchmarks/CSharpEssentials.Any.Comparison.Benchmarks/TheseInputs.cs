using CSharpEssentials.These;

namespace CSharpEssentials.Any.Comparison.Benchmarks;

/// <summary>The same <see cref="TheseState"/> as a CSE <c>These&lt;string, int&gt;</c> and as the hand-written <see cref="Outcome"/>.</summary>
internal static class TheseInputs
{
    public static These<string, int> Cse(TheseState state) => state switch
    {
        TheseState.Left => These<string, int>.Left(Inputs.Warning),
        TheseState.Right => These<string, int>.Right(Inputs.IntValue),
        TheseState.Both => These<string, int>.Both(Inputs.Warning, Inputs.IntValue),
        _ => throw new ArgumentOutOfRangeException(nameof(state)),
    };

    public static Outcome HandWritten(TheseState state) => state switch
    {
        TheseState.Left => new Outcome(true, false, Inputs.Warning, 0),
        TheseState.Right => new Outcome(false, true, null, Inputs.IntValue),
        TheseState.Both => new Outcome(true, true, Inputs.Warning, Inputs.IntValue),
        _ => throw new ArgumentOutOfRangeException(nameof(state)),
    };
}
