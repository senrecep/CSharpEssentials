namespace CSharpEssentials.ResultPattern.Interfaces;

/// <summary>
/// Reads whether a result struct is a failure through one cached instance per closed type, so generic pipeline code on
/// net8 and net9 can test a struct response without boxing it. Those JITs box a struct that is tested against
/// <see cref="IResultBase"/> inside shared generic code; net10 and later remove the box themselves.
/// </summary>
/// <typeparam name="TSelf">The result type that implements the probe.</typeparam>
internal interface IFailureProbe<TSelf>
{
    bool IsFailureOf(in TSelf value);
}
