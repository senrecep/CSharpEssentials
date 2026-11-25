using System.Collections.Immutable;
using System.Globalization;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpEssentials.Tests.Generators;

public class TypedResultAnalyzerTests
{
    private const string Header = """
        using System;
        using System.Threading.Tasks;
        using CSharpEssentials.Errors;
        using CSharpEssentials.ResultPattern;

        namespace Sample;

        public sealed record Dto(int Id);
        public sealed record Locked(int Id) : ConflictError("User.Locked", "User is locked.");

        public static class Errs
        {
            public static readonly NotFoundError NotFound = new("User.NotFound", "User not found.");
            public static readonly ConflictError Conflict = new("User.Conflict", "Conflict.");
        }

        """;

    private static readonly Lazy<AnalyzerAssembly> ResultsAssembly = new(static () => AnalyzerAssembly.Load("CSharpEssentials.Results.Generators.dll"));

    [Fact]
    public async Task Valid_Usage_Should_Report_Nothing()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
            public sealed class Service
            {
                private readonly Result<Dto, NotFoundError> _cached;
                private Result<Dto, NotFoundError> _initialized = Errs.NotFound;
                private Result<Dto, NotFoundError> _late;

                public Service(Result<Dto, NotFoundError> cached) => _cached = cached;

                public Result<Dto, NotFoundError> Property { get; } = new Dto(1);
                public required Result<Dto, NotFoundError> Required { get; init; }
                public Result<Dto, NotFoundError> Computed => _cached;
                public Result<Dto, NotFoundError>[] Empty { get; } = new Result<Dto, NotFoundError>[0];
                public Result<Dto, NotFoundError>[] Filled { get; } = new Result<Dto, NotFoundError>[] { new Dto(1) };

                public void Load(Result<Dto, NotFoundError> value) => _late = value;

                public bool IsDefault(Result<Dto, NotFoundError> r) => r == default || r.Equals(default(Result<Dto, NotFoundError>)) || _late != default;

                public Result<Dto, NotFoundError> Get(int id) => id == 0 ? Errs.NotFound : new Dto(id);

                public Result<Dto, NotFoundError, ConflictError> Widen(int id) =>
                    Get(id).BindWiden(d => (Result<Dto, ConflictError>)Errs.Conflict).Ensure(d => d.Id > 0, Errs.Conflict);
            }

            public sealed record Positional(Result<Dto, NotFoundError> Value);
            """);

        diagnostics.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Result<Dto, NotFoundError> r = default;", "'default'")]
    [InlineData("var r = default(Result<Dto, NotFoundError>);", "'default(...)'")]
    [InlineData("var r = new Result<Dto, NotFoundError>();", "A parameterless constructor call")]
    [InlineData("Result<Dto, NotFoundError> r = new();", "A parameterless constructor call")]
    [InlineData("var r = new Result<Dto, NotFoundError>[3];", "An array without an initializer")]
    public async Task CSE5001_Should_Report_A_Default_Creation(string statement, string what)
    {
        Diagnostic diagnostic = await SingleAsync("CSE5001", $$"""
            public static class C
            {
                public static void M()
                {
                    {{statement}}
                    _ = r;
                }
            }
            """);

        diagnostic.Severity.Should().Be(DiagnosticSeverity.Warning);
        Message(diagnostic).Should().Be($"{what} creates a default 'CSharpEssentials.ResultPattern.Result<Sample.Dto, CSharpEssentials.Errors.NotFoundError>', which holds no case");
    }

    [Fact]
    public async Task CSE5001_Should_Report_A_Returned_Default()
    {
        await SingleAsync("CSE5001", "public static class C { public static Result<Dto, NotFoundError> M() => default; }");
    }

    [Theory]
    [InlineData("private Result<Dto, NotFoundError> _field;", "Field '_field' without an initializer")]
    [InlineData("private static Result<Dto, NotFoundError> _field;", "Field '_field' without an initializer")]
    [InlineData("public Result<Dto, NotFoundError> Value { get; }", "Property 'Value' without an initializer")]
    [InlineData("public Result<Dto, NotFoundError> Value { get; private set; }", "Property 'Value' without an initializer")]
    [InlineData("private Result<Dto, NotFoundError> Value { get; set; }", "Property 'Value' without an initializer")]
    public async Task CSE5001_Should_Report_A_Member_Without_An_Initializer(string member, string what)
    {
        Diagnostic diagnostic = await SingleAsync("CSE5001", $"public sealed class C {{ {member} public object Read() => this; }}");

        Message(diagnostic).Should().StartWith(what);
    }

    [Fact]
    public async Task CSE5001_Should_Not_Report_A_Member_Other_Types_Can_Set()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
            public class Holder
            {
                public Result<Dto, NotFoundError> Outcome { get; init; }
                public Result<Dto, NotFoundError> Current { get; set; }
                protected Result<Dto, NotFoundError> _inherited;
                internal Result<Dto, NotFoundError> Shared;
            }

            public sealed class Derived : Holder
            {
                public Derived(Result<Dto, NotFoundError> r) => _inherited = r;
            }

            public static class Factory
            {
                public static Holder Create(Result<Dto, NotFoundError> r) => new() { Outcome = r, Current = r, Shared = r };
            }
            """);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task CSE5001_Should_Not_Report_A_Field_Written_Through_Out_Or_Deconstruction()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
            public sealed class C
            {
                private Result<Dto, NotFoundError> _a;
                private Result<Dto, NotFoundError> _b;

                public C(Result<Dto, NotFoundError> a)
                {
                    Init(out _a);
                    (_b, _) = (a, 1);
                }

                private static void Init(out Result<Dto, NotFoundError> value) => value = new Dto(1);
            }
            """);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task CSE5002_Should_Report_A_Repeated_Error_Type()
    {
        Diagnostic diagnostic = await SingleAsync("CSE5002", "public static class C { public static Result<Dto, NotFoundError, NotFoundError>? M() => null; }");

        Message(diagnostic).Should().Be("'CSharpEssentials.ResultPattern.Result<Sample.Dto, CSharpEssentials.Errors.NotFoundError, CSharpEssentials.Errors.NotFoundError>' has the error type 'CSharpEssentials.Errors.NotFoundError' in two slots");
    }

    [Theory]
    [InlineData("TypedError, NotFoundError", "CSharpEssentials.Errors.NotFoundError", "CSharpEssentials.Errors.TypedError")]
    [InlineData("ConflictError, Locked", "Sample.Locked", "CSharpEssentials.Errors.ConflictError")]
    [InlineData("NotFoundError, IError", "CSharpEssentials.Errors.NotFoundError", "CSharpEssentials.Errors.IError")]
    public async Task CSE5003_Should_Report_Overlapping_Slots(string slots, string derived, string baseSlot)
    {
        Diagnostic diagnostic = await SingleAsync("CSE5003", $"public static class C {{ public static Result<Dto, {slots}>? M() => null; }}");

        Message(diagnostic).Should().Contain($"the error slot '{derived}' also converts to the slot '{baseSlot}'");
    }

    [Fact]
    public async Task CSE5003_Should_Not_Report_A_User_Defined_Conversion()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
            public readonly record struct Gone(string Code)
            {
                public static implicit operator Error(Gone gone) => Error.NotFound(gone.Code, "Gone.");
            }

            public static class C { public static Result<Dto, Gone, Error>? M() => null; }
            """);

        diagnostics.Should().BeEmpty();
    }

    [Theory]
    [InlineData("NotFoundError, NotFoundError", "CSharpEssentials.Errors.NotFoundError")]
    [InlineData("object, NotFoundError", "CSharpEssentials.Errors.NotFoundError")]
    [InlineData("Locked, ConflictError", "CSharpEssentials.Errors.ConflictError")]
    public async Task CSE5004_Should_Report_A_Success_Type_That_Overlaps_An_Error_Slot(string arguments, string slot)
    {
        Diagnostic diagnostic = await SingleAsync("CSE5004", $"public static class C {{ public static Result<{arguments}>? M() => null; }}");

        Message(diagnostic).Should().EndWith($"the error slot '{slot}' convert to one another, so the static type decides between success and failure");
    }

    [Fact]
    public async Task Slot_Rules_Should_Ignore_Type_Parameters()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
            public static class C
            {
                public static Result<T, E1, E2>? M<T, E1, E2>() where T : notnull where E1 : notnull where E2 : notnull => null;
            }
            """);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task CSE5005_Should_Report_A_Kind_That_Disagrees_With_The_Base()
    {
        Diagnostic diagnostic = await SingleAsync("CSE5005", """
            [ErrorKind(ErrorType.Conflict)]
            public sealed record Missing() : NotFoundError("Missing", "Missing.");
            """);

        Message(diagnostic).Should().Be("'Sample.Missing' has [ErrorKind(Conflict)] but 'CSharpEssentials.Errors.NotFoundError' has kind NotFound");
    }

    [Theory]
    [InlineData("public sealed record Custom() : TypedError(\"Custom\", \"Custom.\") { public override ErrorType Type => ErrorType.NotFound; }", "Sample.Custom.Type")]
    [InlineData("public readonly record struct Custom(int Id) : IError { public string Code => \"Custom\"; public string Description => \"Custom.\"; public ErrorType Type { get => ErrorType.NotFound; } public int NumericType => (int)Type; public ErrorMetadata? Metadata => null; }", "Sample.Custom.Type")]
    public async Task CSE5005_Should_Report_A_Type_Property_That_Disagrees_With_The_Kind(string declaration, string property)
    {
        Diagnostic diagnostic = await SingleAsync("CSE5005", $"[ErrorKind(ErrorType.Conflict)]\n{declaration}");

        Message(diagnostic).Should().Be($"'Sample.Custom' has [ErrorKind(Conflict)] but '{property}' has kind NotFound");
    }

    [Fact]
    public async Task CSE5005_Should_Not_Report_A_Type_Property_That_Matches_The_Kind()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
            [ErrorKind(ErrorType.Conflict)]
            public readonly record struct OrderLocked(int OrderId) : IError
            {
                public string Code => "Order.Locked";
                public string Description => "Order is locked.";
                public ErrorType Type => ErrorType.Conflict;
                public int NumericType => (int)Type;
                public ErrorMetadata? Metadata => null;
            }
            """);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task CSE5005_Should_Not_Report_A_Matching_Or_Inherited_Kind()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
            [ErrorKind(ErrorType.NotFound)]
            public sealed record Missing() : NotFoundError("Missing", "Missing.");

            [ErrorKind(ErrorType.Domain)]
            public sealed class Custom { }
            """);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task CSE5006_Should_Report_Ensure_With_A_Derived_Error()
    {
        Diagnostic diagnostic = await SingleAsync("CSE5006", """
            public static class C
            {
                public static void M(Result<Dto, NotFoundError, ConflictError> r)
                {
                    var checkedResult = r.Ensure(d => d.Id > 0, new Locked(1));
                    _ = checkedResult;
                }
            }
            """);

        Message(diagnostic).Should().Be("'Ensure' returns 'CSharpEssentials.ResultPattern.Result<Sample.Dto, CSharpEssentials.Errors.NotFoundError, CSharpEssentials.Errors.ConflictError, Sample.Locked>', where the added slot 'Sample.Locked' converts to the slot 'CSharpEssentials.Errors.ConflictError'; cast the error to the slot type to keep the error set");
    }

    [Fact]
    public async Task CSE5006_Should_Report_BindWiden_With_A_Subset()
    {
        Diagnostic diagnostic = await SingleAsync("CSE5006", """
            public static class C
            {
                private static Result<string, NotFoundError> Name(Dto d) => d.Id.ToString(System.Globalization.CultureInfo.InvariantCulture);

                public static void M(Result<Dto, NotFoundError, ConflictError> r)
                {
                    var named = r.BindWiden(Name);
                    _ = named;
                }
            }
            """);

        Message(diagnostic).Should().EndWith("the added slot 'CSharpEssentials.Errors.NotFoundError' repeats the slot 'CSharpEssentials.Errors.NotFoundError'; call Bind<TOut> and let the inner result widen");
    }

    [Fact]
    public async Task CSE5006_Should_Report_The_Async_Instance_Form()
    {
        await SingleAsync("CSE5006", """
            public static class C
            {
                public static async Task M(Result<Dto, NotFoundError> r)
                {
                    var checkedResult = await r.EnsureAsync(d => Task.FromResult(d.Id > 0), new Missing());
                    _ = checkedResult;
                }
            }

            public sealed record Missing() : NotFoundError("Missing", "Missing.");
            """);
    }

    [Fact]
    public async Task CSE5006_Should_Name_A_Base_Type_Added_After_A_Derived_Slot()
    {
        Diagnostic diagnostic = await SingleAsync("CSE5006", """
            public static class C
            {
                public static void M(Result<Dto, NotFoundError> r, TypedError error)
                {
                    var checkedResult = r.Ensure(d => d.Id > 0, error);
                    _ = checkedResult;
                }
            }
            """);

        Message(diagnostic).Should().Contain("the added slot 'CSharpEssentials.Errors.TypedError' accepts the slot 'CSharpEssentials.Errors.NotFoundError'");
    }

    [Fact]
    public async Task CSE5006_Should_Not_Report_A_Call_That_Keeps_An_Overlapping_Receiver_Set()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
            public static class C
            {
                public static void M(Result<Dto, TypedError, NotFoundError> r, TypedError error)
                {
                    var checkedResult = r.Ensure(d => d.Id > 0, error);
                    _ = checkedResult;
                }
            }
            """);

        diagnostics.Select(static d => d.Id).Should().Equal("CSE5003");
    }

    [Fact]
    public async Task CSE5006_Should_Report_Query_Syntax_With_A_Subset()
    {
        Diagnostic diagnostic = await SingleAsync("CSE5006", """
            public static class C
            {
                private static Result<string, NotFoundError> Name(Dto d) => d.Id.ToString(System.Globalization.CultureInfo.InvariantCulture);

                public static void M(Result<Dto, NotFoundError, ConflictError> r)
                {
                    var named = from d in r from n in Name(d) select n;
                    _ = named;
                }
            }
            """);

        Message(diagnostic).Should().StartWith("'SelectMany' returns").And.EndWith("; rewrite the query as .Bind<TOut>(...) or make the inner result's error set disjoint");
    }

    [Fact]
    public async Task Analyzers_Should_Stay_Silent_Without_The_Typed_Results()
    {
        CSharpCompilation compilation = GeneratorHarness.CreateCompilation(["public class Result<T, E> { public static Result<T, E> M() => default!; }"]);

        ImmutableArray<Diagnostic> diagnostics = await AnalyzerHarness.GetAnalyzerDiagnosticsAsync(compilation, ResultsAssembly.Value.Analyzers);

        diagnostics.Should().BeEmpty();
    }

    private static async Task<Diagnostic> SingleAsync(string id, string source)
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(source);
        diagnostics.Select(static d => d.Id).Should().OnlyContain(d => d == id);
        return diagnostics.Should().ContainSingle().Subject;
    }

    private static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(string source)
    {
        CSharpCompilation compilation = GeneratorHarness.CreateCompilation([], [typeof(Result<int, NotFoundError>).Assembly, typeof(TypedError).Assembly])
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(Header + source, new CSharpParseOptions(LanguageVersion.Latest), path: "Source.cs"));
        compilation.GetDiagnostics().Where(static d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        return await AnalyzerHarness.GetAnalyzerDiagnosticsAsync(compilation, ResultsAssembly.Value.Analyzers);
    }

    private static string Message(Diagnostic diagnostic) => diagnostic.GetMessage(CultureInfo.InvariantCulture);
}
