using System.Globalization;
using System.Reflection;
using System.Runtime.Versioning;
using CSharpEssentials.Errors;
using CSharpEssentials.Results.Typed.Tests.Harness;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpEssentials.Results.Typed.Tests;

public sealed class CompileHarnessTests
{
    private const string AllArms = "public static int M(int i) => Get(i) switch { Dto d => d.Id, NotFoundError => 404, ConflictError => 409 };";
    private const string MissingArm = "public static int M(int i) => Get(i) switch { Dto d => d.Id, NotFoundError => 404 };";
    private const string DuplicateSlot = "public static R<Dto, NotFoundError, NotFoundError> Twice() => NotFound;";
    private const string ForeignError = "public static R<Dto, NotFoundError, ConflictError> Foreign() => new ValidationError(\"Order.Invalid\", \"invalid\");";

    private static readonly (LanguageVersion Version, string Id, string Name)[] BelowCSharp15Versions =
    [
        (LanguageVersion.CSharp14, "CS9327", "14.0"),
        (LanguageVersion.CSharp13, "CS9260", "13.0"),
#if TYPED_NETSTANDARD_ASSET
        // The default language version of a netstandard2.1 project.
        (LanguageVersion.CSharp8, "CS8400", "8.0"),
#endif
    ];

    // Snippets that switch on the union, keyed by name for readable theory rows.
    private static readonly Dictionary<string, string> SwitchSnippets = new(StringComparer.Ordinal)
    {
        [nameof(AllArms)] = AllArms,
        [nameof(MissingArm)] = MissingArm,
    };

    // Snippets that only convert into the union, with the id they report at every language version.
    private static readonly Dictionary<string, (string Source, string Id)> ConversionSnippets = new(StringComparer.Ordinal)
    {
        [nameof(DuplicateSlot)] = (DuplicateSlot, "CS0457"),
        [nameof(ForeignError)] = (ForeignError, "CS0029"),
    };

    [Fact]
    public void LanguageVersionLatest_Should_MapToCSharp15()
    {
        LanguageVersion effective = LanguageVersion.Latest.MapSpecifiedToEffectiveVersion();

        effective.Should().Be(LanguageVersion.CSharp15);
    }

    [Fact]
    public void ReferencePack_Should_UseTheRefPackOfTheExpectedErrorsAsset()
    {
        ReferencePack pack = ReferencePack.Current;

#if TYPED_NETSTANDARD_ASSET
        pack.Framework.Identifier.Should().Be(".NETStandard");
        pack.Directory.Should().EndWith("netstandard2.1");
#else
        pack.Framework.Identifier.Should().Be(".NETCoreApp");
        pack.Directory.Should().EndWith($"net{Environment.Version.Major}.0");
#endif
        pack.Library.Select(a => a.GetName().Name).Should().Contain(["CSharpEssentials.Core", "CSharpEssentials.Enums"]);
        pack.Library.Should().AllSatisfy(a => a.GetCustomAttribute<TargetFrameworkAttribute>()!.FrameworkName.Should().Be(pack.Framework.FullName, a.GetName().Name));
    }

    [Fact]
    public void Compile_Should_ReportNothing_When_EveryCaseHasAnArm()
    {
        IReadOnlyList<Diagnostic> diagnostics = CompileHarness.Compile(InlineUnion.With(AllArms), LanguageVersion.CSharp15);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void Compile_Should_ReportNothing_When_TheNullArmIsAddedToEveryCase()
    {
        const string members = "public static int M(int i) => Get(i) switch { Dto d => d.Id, NotFoundError => 404, ConflictError => 409, null => 500 };";

        IReadOnlyList<Diagnostic> diagnostics = CompileHarness.Compile(InlineUnion.With(members), LanguageVersion.CSharp15);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void Compile_Should_ReportCS8509NamingTheMissingCase_When_AnArmIsMissing()
    {
        IReadOnlyList<Diagnostic> diagnostics = CompileHarness.Compile(InlineUnion.With(MissingArm), LanguageVersion.CSharp15);

        diagnostics.Ids().Should().BeEquivalentTo("CS8509");
        diagnostics.Single().GetMessage(CultureInfo.InvariantCulture).Should().Contain("ConflictError");
    }

    [Fact]
    public void Compile_Should_ReportCS8846_When_TheOnlyArmForACaseHasAWhenClause()
    {
        const string members = "public static int M(int i) => Get(i) switch { Dto d => d.Id, NotFoundError nf when nf == NotFound => 404, ConflictError => 409 };";

        IReadOnlyList<Diagnostic> diagnostics = CompileHarness.Compile(InlineUnion.With(members), LanguageVersion.CSharp15);

        diagnostics.Ids().Should().BeEquivalentTo("CS8846");
    }

    [Fact]
    public void Compile_Should_ReportCS0161_When_AReturningSwitchStatementCoversEveryCase()
    {
        const string members = "public static int M(int i) { switch (Get(i)) { case Dto: return 1; case NotFoundError: return 2; case ConflictError: return 3; } }";

        IReadOnlyList<Diagnostic> diagnostics = CompileHarness.Compile(InlineUnion.With(members), LanguageVersion.CSharp15);

        diagnostics.Ids().Should().BeEquivalentTo("CS0161");
    }

    [Fact]
    public void Compile_Should_ReportCS0457_When_TwoSlotsHaveTheSameType()
    {
        IReadOnlyList<Diagnostic> diagnostics = CompileHarness.Compile(InlineUnion.With(DuplicateSlot), LanguageVersion.CSharp15);

        diagnostics.Ids().Should().BeEquivalentTo("CS0457");
    }

    [Fact]
    public void Compile_Should_ReportCS0029_When_TheErrorIsNotInTheSet()
    {
        IReadOnlyList<Diagnostic> diagnostics = CompileHarness.Compile(InlineUnion.With(ForeignError), LanguageVersion.CSharp15);

        diagnostics.Ids().Should().BeEquivalentTo("CS0029");
    }

    [Fact]
    public void Compile_Should_ReportCS0150_When_AStaticFieldIsUsedAsACasePattern()
    {
        const string members = "public static int M(int i) => Get(i) switch { Dto d => d.Id, NotFound => 1, NotFoundError => 404, ConflictError => 409 };";

        IReadOnlyList<Diagnostic> diagnostics = CompileHarness.Compile(InlineUnion.With(members), LanguageVersion.CSharp15);

        diagnostics.Ids().Should().BeEquivalentTo("CS0150");
    }

    [Fact]
    public void Compile_Should_ReportCS8510_When_TheBaseArmComesBeforeTheDerivedArm()
    {
        const string members = """
            public sealed record Leaf() : NotFoundError("User.NotFound", "User not found");
            public static int M(int i) => Get(i) switch { Dto d => d.Id, NotFoundError => 404, Leaf => 1, ConflictError => 409 };
            """;

        IReadOnlyList<Diagnostic> diagnostics = CompileHarness.Compile(InlineUnion.With(members), LanguageVersion.CSharp15);

        diagnostics.Ids().Should().BeEquivalentTo("CS8510");
    }

    [Fact]
    public void Compile_Should_ReportCS8714AndCS8655_When_TheValueTypeArgumentIsNullable()
    {
        const string members = "public static int M(R<Dto?, NotFoundError, ConflictError> r) => r switch { Dto d => d.Id, NotFoundError => 404, ConflictError => 409 };";

        IReadOnlyList<Diagnostic> diagnostics = CompileHarness.Compile(InlineUnion.With(members), LanguageVersion.CSharp15);

        diagnostics.Ids().Should().BeEquivalentTo("CS8714", "CS8655");
    }

    [Fact]
    public void Compile_Should_ReportNotExhaustive_When_AnErrorSlotIsMatchedOnlyByEveryErrorType()
    {
        // In-process Roslyn 5.12 reports CS8509 ('_') when the Type arms sit directly in the union switch and CS8524 (an unnamed
        // ErrorType value) when they sit in a nested switch. Either way, naming every ErrorType never makes the Error slot exhaustive.
        IEnumerable<ErrorType> types = Enum.GetValues<ErrorType>();
        string propertyArms = string.Concat(types.Select(type => $"Error {{ Type: ErrorType.{type} }} => {(int)type}, "));
        string valueArms = string.Concat(types.Select(type => $"ErrorType.{type} => {(int)type}, "));
        string direct = $"public static int M(R<Dto, Error, ConflictError> r) => r switch {{ Dto d => d.Id, {propertyArms}ConflictError => 409 }};";
        string nested = $"public static int M(R<Dto, Error, ConflictError> r) => r switch {{ Dto d => d.Id, Error e => e.Type switch {{ {valueArms}}}, ConflictError => 409 }};";

        CompileHarness.Compile(InlineUnion.With(direct), LanguageVersion.CSharp15).Ids().Should().BeEquivalentTo("CS8509");
        CompileHarness.Compile(InlineUnion.With(nested), LanguageVersion.CSharp15).Ids().Should().BeEquivalentTo("CS8524");
    }

    [Fact]
    public void Compile_Should_ReportCS0102_When_ACatalogDeclaresATypeAndAFieldWithTheSameName()
    {
        const string members = """
            public static class UserErrors
            {
                public sealed record NotFound() : NotFoundError("User.NotFound", "User not found");
                public static readonly NotFoundError NotFound = new("User.NotFound", "User not found");
            }
            """;

        IReadOnlyList<Diagnostic> diagnostics = CompileHarness.Compile(InlineUnion.With(members), LanguageVersion.CSharp15);

        diagnostics.Ids().Should().Contain("CS0102");
    }

    [Fact]
    public void Compile_Should_ReportCS0119_When_ANestedCatalogTypeIsReturnedByName()
    {
        const string members = """
            public static class UserErrors
            {
                public sealed record NotFound() : NotFoundError("User.NotFound", "User not found");
            }
            public static R<Dto, UserErrors.NotFound, ConflictError> Find() => UserErrors.NotFound;
            """;

        IReadOnlyList<Diagnostic> diagnostics = CompileHarness.Compile(InlineUnion.With(members), LanguageVersion.CSharp15);

        diagnostics.Ids().Should().BeEquivalentTo("CS0119");
    }

    [Fact]
    public void Compile_Should_ReportCS9385_When_AUnionStructHasOnlyImplicitOperators()
    {
        const string members = """
            [Union]
            public readonly struct OperatorsOnly : IUnion
            {
                private readonly object? _value;
                private OperatorsOnly(object? value) => _value = value;
                public object? Value => _value;
                public static implicit operator OperatorsOnly(Dto value) => new(value);
                public static implicit operator OperatorsOnly(NotFoundError error) => new(error);
            }
            """;

        IReadOnlyList<Diagnostic> diagnostics = CompileHarness.Compile(InlineUnion.With(members), LanguageVersion.CSharp15);

        diagnostics.Ids().Should().BeEquivalentTo("CS9385");
    }

    [Fact]
    public void Run_Should_Throw_When_ADefaultResultIsSwitchedWithoutANullArm()
    {
        const string members = "public static int M() => default(R<Dto, NotFoundError, ConflictError>) switch { Dto d => d.Id, NotFoundError => 404, ConflictError => 409 };";

        Action run = () => CompileHarness.Run(InlineUnion.With(members), "M");

        run.Should().ThrowExactly<System.Runtime.CompilerServices.SwitchExpressionException>();
    }

    [Fact]
    public void Run_Should_TakeTheNullArm_When_ADefaultResultIsSwitched()
    {
        const string members = "public static int M() => default(R<Dto, NotFoundError, ConflictError>) switch { Dto d => d.Id, NotFoundError => 404, ConflictError => 409, null => 500 };";

        CompileHarness.Run(InlineUnion.With(members), "M").Should().Be(500);
    }

    [Theory]
    [MemberData(nameof(SwitchSnippetsBelowCSharp15))]
    public void Compile_Should_ReportThatUnionsAreNotAvailable_When_TheLanguageVersionIsBelow15(
        LanguageVersion languageVersion, string id, string version, string snippet)
    {
        IReadOnlyList<Diagnostic> diagnostics = CompileHarness.Compile(InlineUnion.With(SwitchSnippets[snippet]), languageVersion);

        diagnostics.Should().Contain(d => d.Id == id && d.GetMessage(CultureInfo.InvariantCulture).Contains("union", StringComparison.OrdinalIgnoreCase) && d.GetMessage(CultureInfo.InvariantCulture).Contains(version, StringComparison.Ordinal));
    }

    [Theory]
    [MemberData(nameof(ConversionSnippetsBelowCSharp15))]
    public void Compile_Should_ReportTheSameConversionError_When_TheLanguageVersionIsBelow15(LanguageVersion languageVersion, string snippet)
    {
        (string source, string id) = ConversionSnippets[snippet];

        IReadOnlyList<Diagnostic> diagnostics = CompileHarness.Compile(InlineUnion.With(source), languageVersion);

        diagnostics.Ids().Should().Contain(id);
        diagnostics.Should().NotContain(d => d.GetMessage(CultureInfo.InvariantCulture).Contains("union", StringComparison.OrdinalIgnoreCase));
    }

    public static TheoryData<LanguageVersion, string, string, string> SwitchSnippetsBelowCSharp15()
    {
        var data = new TheoryData<LanguageVersion, string, string, string>();
        foreach ((LanguageVersion languageVersion, string id, string name) in BelowCSharp15Versions)
        {
            foreach (string snippet in SwitchSnippets.Keys)
                data.Add(languageVersion, id, name, snippet);
        }

        return data;
    }

    public static TheoryData<LanguageVersion, string> ConversionSnippetsBelowCSharp15()
    {
        var data = new TheoryData<LanguageVersion, string>();
        foreach ((LanguageVersion languageVersion, _, _) in BelowCSharp15Versions)
        {
            foreach (string snippet in ConversionSnippets.Keys)
                data.Add(languageVersion, snippet);
        }

        return data;
    }
}
