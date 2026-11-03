using System.Text.Json.Serialization;
using Dunet;

namespace CSharpEssentials.Any.Comparison.Benchmarks;

/// <summary>
/// The Dunet union for the Any comparison: a number case and a text case. Dunet requires a partial record and makes it
/// abstract, so the base cannot be sealed; the variants are. The <see cref="JsonDerivedTypeAttribute"/>s are Dunet's
/// documented System.Text.Json setup.
/// </summary>
// https://github.com/domn1995/dunet/blob/945c38e48c048d7062edc392e19196b3cef1227f/Readme.md#serializationdeserialization
[Union]
[JsonDerivedType(typeof(Number), nameof(Number))]
[JsonDerivedType(typeof(Text), nameof(Text))]
public partial record IntOrText
{
    public sealed partial record Number(int Value);

    public sealed partial record Text(string Value);
}
