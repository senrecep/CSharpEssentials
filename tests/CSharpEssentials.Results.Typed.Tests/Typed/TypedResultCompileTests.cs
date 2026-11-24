using System.Globalization;
using CSharpEssentials.Results.Typed.Tests.Harness;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpEssentials.Results.Typed.Tests.Typed;

/// <summary>
/// Compiles consumer code against the shipped typed results with in-process Roslyn 5.12 at C# 15, on the ref pack of the
/// loaded asset (net11.0, net10.0, net9.0 or netstandard2.1).
/// </summary>
public sealed class TypedResultCompileTests
{
    [Theory]
    [InlineData("public static int M(int i) => Get(i) switch { Dto d => d.Id, NotFoundError => 404, UserErrors.Locked => 423 };")]
    [InlineData("public static int M(int i) => Get(i) switch { Dto d => d.Id, NotFoundError { Code: UserErrors.Codes.Deleted } => 410, NotFoundError => 404, UserErrors.Locked l => l.Id };")]
    [InlineData("public static int M(Result<Dto, NotFoundError> r) => r switch { Dto d => d.Id, NotFoundError => 404 };")]
    [InlineData("public static int M(Result<int, NotFoundError, ConflictError, ValidationError> r) => r switch { int v => v, NotFoundError => 404, ConflictError => 409, ValidationError => 400 };")]
    [InlineData("public static int M(Result<int, NotFoundError, ConflictError, ValidationError> r) => r switch { int v => v, NotFoundError => 404, ConflictError => 409, ValidationError => 400, null => 500 };")]
    public void Compile_Should_ReportNothing_When_EveryCaseHasAnArm(string members)
    {
        IReadOnlyList<Diagnostic> diagnostics = CompileHarness.Compile(TypedSnippet.With(members), LanguageVersion.CSharp15);

        diagnostics.Should().BeEmpty();
    }

    [Theory]
    [InlineData("public static Result<Dto, NotFoundError, UserErrors.Locked> M(Result<Dto, NotFoundError> r) => r;")]
    [InlineData("public static Result<Dto, NotFoundError, UserErrors.Locked> M(Result<Dto, UserErrors.Locked> r) => r;")]
    [InlineData("public static Result<Dto, UserErrors.Locked, NotFoundError> M(int i) => Get(i);")]
    [InlineData("public static Result<Dto, NotFoundError, UserErrors.Locked, ValidationError> M(int i) => Get(i);")]
    [InlineData("public static Result<Dto, ValidationError, UserErrors.Locked, NotFoundError> M(int i) => Get(i);")]
    [InlineData("public static Result<Dto, NotFoundError> M() => UserErrors.NotFound;")]
    public void Compile_Should_ReportNothing_When_WideningOrReordering(string members)
    {
        IReadOnlyList<Diagnostic> diagnostics = CompileHarness.Compile(TypedSnippet.With(members), LanguageVersion.CSharp15);

        diagnostics.Should().BeEmpty();
    }

    [Theory]
    [InlineData(LanguageVersion.CSharp12)]
    [InlineData(LanguageVersion.CSharp13)]
    [InlineData(LanguageVersion.CSharp14)]
    public void Compile_Should_ReportNothing_When_OnlyNonPatternMembersAreUsedBelowCSharp15(LanguageVersion languageVersion)
    {
        const string members = """
            public static Result<Dto, NotFoundError, UserErrors.Locked, ValidationError> Widen(int i) => Get(i);
            public static int M(int i)
            {
                Result<Dto, NotFoundError, UserErrors.Locked> result = Get(i);
                int status = result.Match(d => d.Id, _ => 404, _ => 423);
                result.Switch(_ => status++, _ => status--, _ => status--);
                if (result.TryGetValue(out NotFoundError? notFound) && notFound.Code == UserErrors.Codes.Deleted)
                    status = 410;
                return result.ToResult().IsSuccess && result.HasValue ? status : -status;
            }
            """;

        IReadOnlyList<Diagnostic> diagnostics = CompileHarness.Compile(TypedSnippet.With(members), languageVersion);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void Compile_Should_ReportCS8509NamingTheMissingCase_When_AnArmIsMissing()
    {
        const string members = "public static int M(int i) => Get(i) switch { Dto d => d.Id, NotFoundError => 404 };";

        IReadOnlyList<Diagnostic> diagnostics = CompileHarness.Compile(TypedSnippet.With(members), LanguageVersion.CSharp15);

        diagnostics.Ids().Should().BeEquivalentTo("CS8509");
        diagnostics.Single().GetMessage(CultureInfo.InvariantCulture).Should().Contain("Locked");
    }

    [Fact]
    public void Compile_Should_ReportCS8509_When_TheBaseKindArmIsMissingBehindACodeArm()
    {
        const string members = "public static int M(int i) => Get(i) switch { Dto d => d.Id, NotFoundError { Code: UserErrors.Codes.Deleted } => 410, UserErrors.Locked => 423 };";

        IReadOnlyList<Diagnostic> diagnostics = CompileHarness.Compile(TypedSnippet.With(members), LanguageVersion.CSharp15);

        diagnostics.Ids().Should().BeEquivalentTo("CS8509");
    }

    [Fact]
    public void Compile_Should_ReportCS0161_When_AReturningSwitchStatementHasNoNullCase()
    {
        const string members = "public static int M(int i) { switch (Get(i)) { case Dto: return 1; case NotFoundError: return 2; case UserErrors.Locked: return 3; } }";

        IReadOnlyList<Diagnostic> diagnostics = CompileHarness.Compile(TypedSnippet.With(members), LanguageVersion.CSharp15);

        diagnostics.Ids().Should().BeEquivalentTo("CS0161");
    }

    [Fact]
    public void Compile_Should_ReportNothing_When_AReturningSwitchStatementHasANullCase()
    {
        const string members = "public static int M(int i) { switch (Get(i)) { case Dto: return 1; case NotFoundError: return 2; case UserErrors.Locked: return 3; case null: return 0; } }";

        IReadOnlyList<Diagnostic> diagnostics = CompileHarness.Compile(TypedSnippet.With(members), LanguageVersion.CSharp15);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void Compile_Should_ReportCS0457_When_TwoSlotsHaveTheSameType()
    {
        const string members = "public static Result<Dto, NotFoundError, NotFoundError> M() => UserErrors.NotFound;";

        IReadOnlyList<Diagnostic> diagnostics = CompileHarness.Compile(TypedSnippet.With(members), LanguageVersion.CSharp15);

        diagnostics.Ids().Should().BeEquivalentTo("CS0457");
    }

    [Fact]
    public void Compile_Should_ReportCS0029_When_TheErrorIsNotInTheSet()
    {
        const string members = "public static Result<Dto, NotFoundError, UserErrors.Locked> M() => new ValidationError(\"User.Invalid\", \"invalid\");";

        IReadOnlyList<Diagnostic> diagnostics = CompileHarness.Compile(TypedSnippet.With(members), LanguageVersion.CSharp15);

        diagnostics.Ids().Should().BeEquivalentTo("CS0029");
    }

    [Fact]
    public void Compile_Should_ReportCS0029_When_AnErrorCatalogValueIsReturnedIntoATypedSlot()
    {
        const string members = "public static Result<Dto, NotFoundError> M() => CSharpEssentials.Errors.Error.NotFound(\"User.NotFound\", \"missing\");";

        IReadOnlyList<Diagnostic> diagnostics = CompileHarness.Compile(TypedSnippet.With(members), LanguageVersion.CSharp15);

        diagnostics.Ids().Should().BeEquivalentTo("CS0029");
    }

    [Fact]
    public void Compile_Should_ReportCS0150_When_AStaticErrorIsUsedAsACasePattern()
    {
        const string members = "public static int M(int i) => Get(i) switch { Dto d => d.Id, UserErrors.NotFound => 1, NotFoundError => 404, UserErrors.Locked => 423 };";

        IReadOnlyList<Diagnostic> diagnostics = CompileHarness.Compile(TypedSnippet.With(members), LanguageVersion.CSharp15);

        diagnostics.Ids().Should().Contain("CS0150");
    }

    [Fact]
    public void Run_Should_PutEachReturnInItsCase()
    {
        const string members = """
            public static string M() => string.Join(",", new[] { 0, 1, 5, 7 }.Select(i => Get(i) switch
            {
                Dto d => "ok" + d.Id,
                NotFoundError { Code: UserErrors.Codes.Deleted } => "gone",
                NotFoundError => "missing",
                UserErrors.Locked l => "locked" + l.Id,
            }));
            """;

        object? result = CompileHarness.Run("using System.Linq;\n" + TypedSnippet.With(members), "M");

        result.Should().Be("missing,gone,ok5,locked7");
    }
}
