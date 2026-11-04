namespace CSharpEssentials.ResultPattern.Interfaces;

/// <summary>
/// The value of a <see cref="Result{TValue}"/> as <see cref="object"/>, so callers that only hold a boxed result can read
/// it without reflection.
/// </summary>
internal interface IValueResult : IResultBase
{
    /// <summary>The value when successful, otherwise <see langword="null"/>.</summary>
    object? BoxedValue { get; }
}
