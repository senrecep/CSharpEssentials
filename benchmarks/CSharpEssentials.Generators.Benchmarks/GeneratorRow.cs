using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpEssentials.Generators.Benchmarks;

/// <summary>
/// One benchmark row: a generator set over a <see cref="ProjectShape"/>. Cold builds a fresh compilation and a fresh driver
/// per call. Incremental keeps a driver warmed on the original compilation and runs it on that compilation with the edited
/// tree swapped in; the driver is immutable, so every call starts from the same warm state.
/// </summary>
public sealed class GeneratorRow
{
    public static readonly GeneratorDriverOptions MeasuredOptions = new(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: false);

    private readonly CSharpCompilation _original;
    private readonly GeneratorDriver _warm;

    public GeneratorRow(string name, bool isCse, ImmutableArray<ISourceGenerator> generators, ProjectShape shape)
    {
        Name = name;
        IsCse = isCse;
        Generators = generators;
        Shape = shape;
        _original = shape.CreateCompilation();
        _warm = CreateDriver(generators, MeasuredOptions).RunGenerators(_original);
    }

    public string Name { get; }

    public bool IsCse { get; }

    public ImmutableArray<ISourceGenerator> Generators { get; }

    public ProjectShape Shape { get; }

    public static GeneratorDriver CreateDriver(ImmutableArray<ISourceGenerator> generators, GeneratorDriverOptions options) =>
        CSharpGeneratorDriver.Create(generators, parseOptions: ProjectShape.ParseOptions, optionsProvider: BenchOptionsProvider.Instance, driverOptions: options);

    public GeneratorDriver Cold() => CreateDriver(Generators, MeasuredOptions).RunGenerators(Shape.CreateCompilation());

    public GeneratorDriver Incremental() => _warm.RunGenerators(EditedCompilation());

    public Compilation OriginalCompilation() => _original;

    public Compilation EditedCompilation() => _original.ReplaceSyntaxTree(Shape.Original, Shape.Edited);
}
