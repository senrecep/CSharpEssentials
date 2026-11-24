using System.Globalization;
using CSharpEssentials.Results.Typed.Tests.Harness;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpEssentials.Results.Typed.Tests;

public sealed class CompileHarnessTests
{
    private const string AllArms = "public static int M(int i) => Get(i) switch { Dto d => d.Id, NotFoundError => 404, ConflictError => 409 };";
    private const string MissingArm = "public static int M(int i) => Get(i) switch { Dto d => d.Id, NotFoundError => 404 };";

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
        const string members = "public static R<Dto, NotFoundError, NotFoundError> Twice() => NotFound;";

        IReadOnlyList<Diagnostic> diagnostics = CompileHarness.Compile(InlineUnion.With(members), LanguageVersion.CSharp15);

        diagnostics.Ids().Should().BeEquivalentTo("CS0457");
    }

    [Fact]
    public void Compile_Should_ReportCS0029_When_TheErrorIsNotInTheSet()
    {
        const string members = "public static R<Dto, NotFoundError, ConflictError> Foreign() => new ValidationError(\"Order.Invalid\", \"invalid\");";

        IReadOnlyList<Diagnostic> diagnostics = CompileHarness.Compile(InlineUnion.With(members), LanguageVersion.CSharp15);

        diagnostics.Ids().Should().BeEquivalentTo("CS0029");
    }

    [Theory]
    [InlineData(LanguageVersion.CSharp14, "CS9327", "14.0")]
    [InlineData(LanguageVersion.CSharp13, "CS9260", "13.0")]
    public void Compile_Should_ReportThatUnionsAreNotAvailable_When_TheLanguageVersionIsBelow15(
        LanguageVersion languageVersion, string id, string version)
    {
        IReadOnlyList<Diagnostic> diagnostics = CompileHarness.Compile(InlineUnion.With(AllArms), languageVersion);

        diagnostics.Should().Contain(d => d.Id == id && d.GetMessage(CultureInfo.InvariantCulture).Contains("union", StringComparison.OrdinalIgnoreCase) && d.GetMessage(CultureInfo.InvariantCulture).Contains(version, StringComparison.Ordinal));
    }
}
