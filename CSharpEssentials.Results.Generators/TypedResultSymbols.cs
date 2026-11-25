using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpEssentials.Results.Generators;

/// <summary>
/// The typed result definitions of one compilation and the slot relations the analyzers check.
/// </summary>
internal sealed class TypedResultSymbols
{
    private static readonly string[] MetadataNames =
    [
        "CSharpEssentials.ResultPattern.Result`2",
        "CSharpEssentials.ResultPattern.Result`3",
        "CSharpEssentials.ResultPattern.Result`4",
    ];

    private readonly ImmutableArray<INamedTypeSymbol> _definitions;

    private TypedResultSymbols(CSharpCompilation compilation, ImmutableArray<INamedTypeSymbol> definitions)
    {
        Compilation = compilation;
        _definitions = definitions;
    }

    public CSharpCompilation Compilation { get; }

    public IAssemblySymbol Assembly => _definitions[0].ContainingAssembly;

    public static TypedResultSymbols? Create(Compilation compilation)
    {
        if (compilation is not CSharpCompilation csharp)
            return null;

        var definitions = MetadataNames
            .Select(compilation.GetTypeByMetadataName)
            .OfType<INamedTypeSymbol>()
            .ToImmutableArray();
        return definitions.IsEmpty ? null : new TypedResultSymbols(csharp, definitions);
    }

    public bool IsTypedResult(ITypeSymbol? type) =>
        type is INamedTypeSymbol { IsUnboundGenericType: false } named &&
        _definitions.Contains(named.OriginalDefinition, SymbolEqualityComparer.Default);

    public INamedTypeSymbol? AsTypedResult(ITypeSymbol? type) =>
        IsTypedResult(type) ? (INamedTypeSymbol)type! : null;

    public static bool AreSame(ITypeSymbol left, ITypeSymbol right) =>
        SymbolEqualityComparer.Default.Equals(left, right);

    public bool Fits(ITypeSymbol source, ITypeSymbol destination)
    {
        if (source is ITypeParameterSymbol or IErrorTypeSymbol || destination is ITypeParameterSymbol or IErrorTypeSymbol || AreSame(source, destination))
            return false;

        Conversion conversion = Compilation.ClassifyConversion(source, destination);
        return conversion.IsImplicit && !conversion.IsUserDefined && (conversion.IsReference || conversion.IsBoxing);
    }

    public static ImmutableArray<ITypeSymbol> ErrorSlots(INamedTypeSymbol result) => result.TypeArguments.RemoveAt(0);

    public IEnumerable<SlotPair> RepeatedOrOverlapping(INamedTypeSymbol result)
    {
        ImmutableArray<ITypeSymbol> slots = ErrorSlots(result);
        for (int later = 1; later < slots.Length; later++)
        {
            for (int earlier = 0; earlier < later; earlier++)
            {
                if (Relate(slots[later], slots[earlier]) is { } relation)
                    yield return new SlotPair(later, slots[later], slots[earlier], relation);
            }
        }
    }

    private SlotRelation? Relate(ITypeSymbol later, ITypeSymbol earlier)
    {
        if (AreSame(later, earlier))
            return SlotRelation.Repeats;
        if (Fits(later, earlier))
            return SlotRelation.ConvertsTo;
        return Fits(earlier, later) ? SlotRelation.Accepts : null;
    }
}
