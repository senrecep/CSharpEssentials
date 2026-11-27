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
    private const string Helpers = """
        public static readonly ValidationError Invalid = new("Order.Invalid", "invalid");
            public static Result<string, NotFoundError> Name(int id) => id == 2 ? UserErrors.NotFound : "n" + id;
            public static Result<string, ValidationError> Check(int id) => id == 3 ? Invalid : "c" + id;

        """;

    [Theory]
    [InlineData("public static int M(int i) => Get(i) switch { Dto d => d.Id, NotFoundError => 404, UserErrors.Locked => 423 };")]
    [InlineData("public static int M(int i) => Get(i) switch { Dto d => d.Id, NotFoundError { Code: UserErrors.Codes.Deleted } => 410, NotFoundError => 404, UserErrors.Locked l => l.Id };")]
    [InlineData("public static int M(Result<Dto, NotFoundError> r) => r switch { Dto d => d.Id, NotFoundError => 404 };")]
    [InlineData("public static int M(Result<int, NotFoundError, ConflictError, ValidationError> r) => r switch { int v => v, NotFoundError => 404, ConflictError => 409, ValidationError => 400 };")]
    [InlineData("public static int M(Result<int, NotFoundError, ConflictError, ValidationError> r) => r switch { int v => v, NotFoundError => 404, ConflictError => 409, ValidationError => 400, null => 500 };")]
    [InlineData("public static Result<Dto, NotFoundError, UserErrors.Locked> M(Result<Dto, NotFoundError> r) => r;")]
    [InlineData("public static Result<Dto, NotFoundError, UserErrors.Locked> M(Result<Dto, UserErrors.Locked> r) => r;")]
    [InlineData("public static Result<Dto, UserErrors.Locked, NotFoundError> M(int i) => Get(i);")]
    [InlineData("public static Result<Dto, NotFoundError, UserErrors.Locked, ValidationError> M(int i) => Get(i);")]
    [InlineData("public static Result<Dto, ValidationError, UserErrors.Locked, NotFoundError> M(int i) => Get(i);")]
    [InlineData("public static Result<Dto, NotFoundError> M() => UserErrors.NotFound;")]
    [InlineData("public static bool M(int i) => Get(i) == default || default(Result<Dto, NotFoundError>) != UserErrors.NotFound;")]
    public void Compile_Should_ReportNothing_When_EveryCaseHasAnArmOrTheResultWidens(string members)
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
                Result<int, NotFoundError, UserErrors.Locked> bound = result.Bind<int>(d => d.Id).Map(id => id + 1);
                Result<int, NotFoundError, UserErrors.Locked, ValidationError> widened = bound.BindWiden<int, ValidationError>(id => id);
                status += widened.Recover((ValidationError _) => 0).Recover((UserErrors.Locked _) => 0).Recover((NotFoundError _) => 0);
                Result<int, NotFoundError, ValidationError> checkedId = bound
                    .Ensure(id => id > 0, UserErrors.NotFound)
                    .FailIf(id => id > 99, new UserErrors.Locked(i))
                    .MapError((UserErrors.Locked _) => new ValidationError("User.Invalid", "invalid"))
                    .Compensate((NotFoundError _) => new Result<int, ValidationError>(0));
                status += checkedId.IsSuccess ? 1 : 0;
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

    [Theory]
    [InlineData("public static Result<string, NotFoundError, UserErrors.Locked> M(int i) => Get(i).Bind<string>(d => Name(d.Id));")]
    [InlineData("public static Result<string, NotFoundError, UserErrors.Locked> M(int i) => Get(i).Bind(d => new Result<string, NotFoundError, UserErrors.Locked>(\"n\"));")]
    [InlineData("public static Result<string, NotFoundError, UserErrors.Locked, ValidationError> M(int i) => Get(i).BindWiden(d => Check(d.Id));")]
    [InlineData("public static Result<string, NotFoundError, ValidationError> M(Result<int, NotFoundError> r) => r.BindWiden(Check);")]
    [InlineData("public static Result<string, NotFoundError, UserErrors.Locked, ValidationError> M(int i) => Get(i).BindWiden<string, ValidationError>(d => { if (d.Id > 4) return Invalid; return \"ok\"; });")]
    [InlineData("public static Result<Dto, UserErrors.Locked> M(int i) => Get(i).Recover((NotFoundError _) => new Dto(0));")]
    [InlineData("public static Result<Dto, NotFoundError, UserErrors.Locked> M(int i) => Get(i).Ensure(d => d.Id > 0, UserErrors.NotFound);")]
    [InlineData("public static Result<Dto, NotFoundError, UserErrors.Locked, ValidationError> M(int i) => Get(i).Ensure(d => d.Id > 0, Invalid);")]
    [InlineData("public static Result<Dto, NotFoundError, ConflictError> M(Result<Dto, NotFoundError, ConflictError> r) => r.Ensure(d => d.Id > 0, (ConflictError)new UserErrors.Locked(1));")]
    public void Compile_Should_ReportNothing_When_ACombinatorIsCalledInItsDocumentedShape(string members)
    {
        IReadOnlyList<Diagnostic> diagnostics = CompileHarness.Compile(TypedSnippet.With(Helpers + members), LanguageVersion.CSharp15);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void Compile_Should_ReportCS0411_When_AnUntypedLambdaReturnsAValueAndAnError()
    {
        const string members = "public static object M(int i) => Get(i).BindWiden(d => { if (d.Id > 4) return Invalid; return \"ok\"; });";

        IReadOnlyList<Diagnostic> diagnostics = CompileHarness.Compile(TypedSnippet.With(Helpers + members), LanguageVersion.CSharp15);

        diagnostics.Ids().Should().BeEquivalentTo("CS0411");
    }

    [Fact]
    public void Compile_Should_ReportCS0121_When_AnUntypedRecoverLambdaFitsTwoSlots()
    {
        const string members = "public static object M(int i) => Get(i).Recover(e => new Dto(0));";

        IReadOnlyList<Diagnostic> diagnostics = CompileHarness.Compile(TypedSnippet.With(Helpers + members), LanguageVersion.CSharp15);

        diagnostics.Ids().Should().BeEquivalentTo("CS0121");
    }

    [Fact]
    public void Compile_Should_ReportCS0029_When_BindWidenAddsASlotTheResultAlreadyHas()
    {
        // The inner error set {NotFoundError} is a subset of the outer one: BindWiden duplicates the slot, Bind<string> widens.
        const string members = "public static Result<string, NotFoundError, UserErrors.Locked> M(int i) => Get(i).BindWiden(d => Name(d.Id));";

        IReadOnlyList<Diagnostic> diagnostics = CompileHarness.Compile(TypedSnippet.With(Helpers + members), LanguageVersion.CSharp15);

        diagnostics.Ids().Should().BeEquivalentTo("CS0029");
        diagnostics.Single().GetMessage(CultureInfo.InvariantCulture).Should().Contain("Result<string, CSharpEssentials.Errors.NotFoundError, UserErrors.Locked, CSharpEssentials.Errors.NotFoundError>");
    }

    [Fact]
    public void Compile_Should_ReportCS0411_When_BindGetsAnInnerResultWithASubsetOfTheErrors()
    {
        const string members = "public static object M(int i) => Get(i).Bind(d => Name(d.Id));";

        IReadOnlyList<Diagnostic> diagnostics = CompileHarness.Compile(TypedSnippet.With(Helpers + members), LanguageVersion.CSharp15);

        diagnostics.Ids().Should().BeEquivalentTo("CS0411");
    }

    [Fact]
    public void Compile_Should_ReportCS0029_When_EnsureGetsADerivedErrorOfAnExistingSlot()
    {
        // Identity beats the conversion to ConflictError, so Ensure<TError> adds a UserErrors.Locked slot.
        const string members = "public static Result<Dto, NotFoundError, ConflictError> M(Result<Dto, NotFoundError, ConflictError> r) => r.Ensure(d => d.Id > 0, new UserErrors.Locked(1));";

        IReadOnlyList<Diagnostic> diagnostics = CompileHarness.Compile(TypedSnippet.With(Helpers + members), LanguageVersion.CSharp15);

        diagnostics.Ids().Should().BeEquivalentTo("CS0029");
        diagnostics.Single().GetMessage(CultureInfo.InvariantCulture).Should().Contain("Result<Dto, CSharpEssentials.Errors.NotFoundError, CSharpEssentials.Errors.ConflictError, UserErrors.Locked>");
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
