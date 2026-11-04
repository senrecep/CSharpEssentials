using CSharpEssentials.Core;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace CSharpEssentials.EntityFrameworkCore;

/// <summary>
/// Creates the value generator of <see cref="OrderedGuidLayout.Version7"/>, which returns <see cref="Guider.NewMonotonicGuid"/>.
/// Use it per property with <c>HasValueGeneratorFactory&lt;Version7GuidValueGeneratorFactory&gt;()</c>; compiled models support it.
/// </summary>
public sealed class Version7GuidValueGeneratorFactory : ValueGeneratorFactory
{
    private static readonly OrderedGuidValueGenerator _generator = new(Guider.NewMonotonicGuid);

    /// <inheritdoc />
    public override ValueGenerator Create(IProperty property, ITypeBase typeBase) => _generator;
}
