using CSharpEssentials.Errors;

namespace CSharpEssentials.Results.Typed.Tests.Typed;

/// <summary>An error type that does not implement <see cref="IError"/>.</summary>
internal sealed class PlainError(string reason)
{
    public string Reason { get; } = reason;
}
