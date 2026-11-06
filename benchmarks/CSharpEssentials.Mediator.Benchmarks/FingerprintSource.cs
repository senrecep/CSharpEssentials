namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>Where the fingerprint of a <see cref="PlaceOrder"/> comes from.</summary>
public enum FingerprintSource
{
    /// <summary>The request supplies <c>"fp-1"</c>; Notes is <c>"n"</c>.</summary>
    Supplied,

    /// <summary>The SHA-256 of the request as JSON; Notes is <c>"n"</c>.</summary>
    Default,

    /// <summary>The SHA-256 of the request as JSON; Notes is 4096 <c>'x'</c> characters.</summary>
    DefaultLarge,
}
