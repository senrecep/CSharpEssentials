using CSharpEssentials.Errors;

namespace CSharpEssentials.Results.Typed.Tests.Typed;

/// <summary>A struct error type that implements <see cref="IError"/>.</summary>
public readonly struct RateLimited(string code) : IError
{
    public string Code { get; } = code;
    public string Description => "Too many requests.";
    public ErrorType Type => ErrorType.Failure;
    public int NumericType => (int)Type;
    public ErrorMetadata? Metadata => null;
}
