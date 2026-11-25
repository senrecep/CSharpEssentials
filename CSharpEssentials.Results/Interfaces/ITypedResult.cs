namespace CSharpEssentials.ResultPattern.Interfaces;

/// <summary>
/// The case a typed <c>Result&lt;T, E1..E3&gt;</c> holds, readable through one interface whatever its type arguments are.
/// </summary>
public interface ITypedResult : IResultBase
{
    /// <summary>Gets a value indicating whether the result holds a case. It is <see langword="false"/> only for <see langword="default"/>.</summary>
    bool HasValue { get; }

    /// <summary>Gets the case the result holds as <see cref="object"/>, or <see langword="null"/> for <see langword="default"/>.</summary>
    object? Value { get; }
}
