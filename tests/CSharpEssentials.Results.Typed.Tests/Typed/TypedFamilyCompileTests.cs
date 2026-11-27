using CSharpEssentials.Results.Typed.Tests.Harness;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpEssentials.Results.Typed.Tests.Typed;

/// <summary>
/// Compiles calls to Tap, TapError, TryCatch, Finally, GetValue, Select/SelectMany, Combine and the collection helpers the way
/// callers write them: query syntax with the same, a disjoint and an overlapping error set, the "any error" members on slots that
/// are not <c>IError</c>, and the members that work without <c>OverloadResolutionPriority</c>. On C# 12 the same-set and the widening
/// <c>SelectMany</c> both fit a query whose inner result has the same errors, and the compiler reports CS1940.
/// </summary>
public sealed class TypedFamilyCompileTests
{
    private const string Helpers = """
        public static readonly ValidationError Invalid = new("Order.Invalid", "invalid");
            public static Result<int, NotFoundError> Find(int id) => id == 2 ? UserErrors.NotFound : id;
            public static Result<string, NotFoundError> Name(int id) => id == 2 ? UserErrors.NotFound : "n" + id;
            public static Result<string, ValidationError> Check(int id) => id == 3 ? Invalid : "c" + id;
            public sealed class Plain { }

        """;

    private static IReadOnlyList<Diagnostic> Compile(string members, LanguageVersion languageVersion = LanguageVersion.CSharp15) =>
        CompileHarness.Compile(TypedSnippet.With(Helpers + members), languageVersion);

    [Theory]
    [InlineData(LanguageVersion.CSharp13)]
    [InlineData(LanguageVersion.CSharp15)]
    public void Compile_Should_ReportNothing_When_QuerySyntaxJoinsResultsWithTheSameErrors(LanguageVersion languageVersion)
    {
        const string members = "public static Result<string, NotFoundError> M(int i) => from a in Find(i) from b in Name(a) select b + a;";

        IReadOnlyList<Diagnostic> diagnostics = Compile(members, languageVersion);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void Compile_Should_ReportCS1940_When_QuerySyntaxJoinsResultsWithTheSameErrorsOnCSharp12()
    {
        const string members = "public static Result<string, NotFoundError> M(int i) => from a in Find(i) from b in Name(a) select b + a;";

        IReadOnlyList<Diagnostic> diagnostics = Compile(members, LanguageVersion.CSharp12);

        diagnostics.Ids().Should().BeEquivalentTo("CS1940");
    }

    [Fact]
    public void Compile_Should_ReportNothing_When_QuerySyntaxAddsTheErrorsOfTheInnerResult()
    {
        const string members = "public static Result<string, NotFoundError, ValidationError> M(int i) => from a in Find(i) from b in Check(a) select b;";

        IReadOnlyList<Diagnostic> diagnostics = Compile(members);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void Compile_Should_ReportCS0029_When_QuerySyntaxGetsAnInnerResultWithASubsetOfTheErrors()
    {
        const string members = "public static Result<string, NotFoundError, UserErrors.Locked> M(int i) => from d in Get(i) from n in Name(d.Id) select n;";

        IReadOnlyList<Diagnostic> diagnostics = Compile(members);

        diagnostics.Ids().Should().BeEquivalentTo("CS0029");
    }

    [Fact]
    public void Compile_Should_ReportNothing_When_TheRemainingFamiliesAreCalledBelowCSharp13()
    {
        const string members = """
            public static int M(int i)
            {
                Result<int, NotFoundError> found = Find(i).Tap(v => i += v).TapIf(v => v > 0, _ => i++).TapError((NotFoundError _) => i--);
                found = found.TapError((Error e) => i += e.Code.Length).Finally(r => { i++; });
                Result<string, NotFoundError, System.FormatException> parsed = found.TryCatch<string, System.FormatException>(v => v.ToString());
                Result<(int, int), NotFoundError> both = Result<int, NotFoundError>.Combine(found, Find(i + 1));
                Result<int[], NotFoundError> all = new[] { 1, 2 }.Traverse(Find);
                (int[] values, Error[] errors) = new[] { found, Find(2) }.Partition();
                Result<string, NotFoundError, ValidationError> widened = from a in Find(i) from b in Check(a) select b;
                return found.GetValueOrDefault() + found.GetValueOr(1) + found.GetValueOr((Error e) => e.Code.Length)
                    + found.Finally(r => r.IsSuccess ? 1 : 0) + both.GetValueOrDefault().Item1 + all.GetValueOr([]).Length + values.Length
                    + errors.Length + (parsed.IsSuccess ? 1 : 0) + (widened.IsSuccess ? 1 : 0) + found.Select(v => v + 1).GetValueOrThrow();
            }
            """;

        IReadOnlyList<Diagnostic> diagnostics = Compile(members, LanguageVersion.CSharp12);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void Compile_Should_ReportCS1661_When_TapErrorAnyIsCalledOnASlotThatIsNotAnIError()
    {
        const string members = "public static Result<int, Plain> M(Result<int, Plain> r) => r.TapError((Error _) => { });";

        IReadOnlyList<Diagnostic> diagnostics = Compile(members);

        diagnostics.Ids().Should().BeEquivalentTo("CS1661", "CS1678");
    }

    [Fact]
    public void Compile_Should_ReportCS1660_When_GetValueOrAnyIsCalledOnASlotThatIsNotAnIError()
    {
        const string members = "public static int M(Result<int, NotFoundError, Plain> r) => r.GetValueOr((Error e) => e.Code.Length);";

        IReadOnlyList<Diagnostic> diagnostics = Compile(members);

        diagnostics.Ids().Should().BeEquivalentTo("CS1660");
    }

    [Fact]
    public void Compile_Should_BindTheSlotOverload_When_AnUntypedTapErrorLambdaIsUsedOnATaskSource()
    {
        const string members = """
            public static System.Threading.Tasks.Task<Result<int, NotFoundError>> M(System.Threading.Tasks.Task<Result<int, NotFoundError>> source) =>
                source.TapErrorAsync(e => { NotFoundError kind = e; return System.Threading.Tasks.Task.CompletedTask; });
            """;

        IReadOnlyList<Diagnostic> diagnostics = Compile(members);

        diagnostics.Should().BeEmpty();
    }
}
