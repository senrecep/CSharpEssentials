using System.Text.Json.Serialization;

namespace CSharpEssentials.Results.Comparison.Benchmarks;

/// <summary>
/// The Dunet union for the typed comparisons: a success case and one case per error type of <see cref="Typed"/>. Dunet
/// makes the base an abstract record, so every case is a class instance. The <see cref="JsonDerivedTypeAttribute"/>s are
/// Dunet's documented System.Text.Json setup.
/// </summary>
// https://github.com/domn1995/dunet/blob/945c38e48c048d7062edc392e19196b3cef1227f/Readme.md#serializationdeserialization
[Dunet.Union]
[JsonDerivedType(typeof(Ok), nameof(Ok))]
[JsonDerivedType(typeof(Missing), nameof(Missing))]
[JsonDerivedType(typeof(Stale), nameof(Stale))]
[JsonDerivedType(typeof(Invalid), nameof(Invalid))]
public partial record OrderOutcome
{
    public sealed partial record Ok(int Value);

    public sealed partial record Missing(NotFoundError Error);

    public sealed partial record Stale(ConflictError Error);

    public sealed partial record Invalid(ValidationError Error);
}
