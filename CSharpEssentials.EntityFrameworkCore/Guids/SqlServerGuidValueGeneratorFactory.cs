using CSharpEssentials.Core;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace CSharpEssentials.EntityFrameworkCore;

/// <summary>
/// Creates the value generator of <see cref="OrderedGuidLayout.SqlServer"/>, which returns <see cref="Guider.NewSqlServerGuid"/>.
/// Use it per property with <c>HasValueGeneratorFactory&lt;SqlServerGuidValueGeneratorFactory&gt;()</c>; compiled models support it.
/// </summary>
public sealed class SqlServerGuidValueGeneratorFactory : ValueGeneratorFactory
{
    private static readonly OrderedGuidValueGenerator _generator = new(Guider.NewSqlServerGuid);

    /// <inheritdoc />
    public override ValueGenerator Create(IProperty property, ITypeBase typeBase) => _generator;
}
