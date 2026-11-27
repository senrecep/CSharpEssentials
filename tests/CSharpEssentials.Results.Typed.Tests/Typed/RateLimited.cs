using System.Text.Json.Serialization;
using CSharpEssentials.Errors;

namespace CSharpEssentials.Results.Typed.Tests.Typed;

/// <summary>A struct error type that implements <see cref="IError"/>.</summary>
internal readonly struct RateLimited : IError
{
    [JsonConstructor]
    public RateLimited(string code) => Code = code;

    public string Code { get; }
    public string Description => "Too many requests.";
    public ErrorType Type => ErrorType.Failure;
    public int NumericType => (int)Type;
    public ErrorMetadata? Metadata => null;
}
