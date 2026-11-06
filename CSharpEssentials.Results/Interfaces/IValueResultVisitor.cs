namespace CSharpEssentials.ResultPattern.Interfaces;

/// <summary>
/// Receives the value of a <see cref="Result{TValue}"/> with its declared type, so callers that only hold a boxed result
/// can work with the typed value without reflection.
/// </summary>
internal interface IValueResultVisitor
{
    /// <summary>Called with the result's value and its declared type.</summary>
    object? Visit<TValue>(TValue value);
}
