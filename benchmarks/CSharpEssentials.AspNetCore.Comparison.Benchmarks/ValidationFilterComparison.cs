using System.Diagnostics.CodeAnalysis;
using CSharpEssentials.Validation.Extensions;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using FluentValidator = FluentValidation.IValidator<CSharpEssentials.AspNetCore.Comparison.Benchmarks.CreateUser>;

namespace CSharpEssentials.AspNetCore.Comparison.Benchmarks;

/// <summary>
/// POST /users with <c>CreateUser</c>: Valid <c>{"name":"ada","email":"ada-bench-local","age":36}</c>, Invalid
/// <c>{"name":"","email":"","age":0}</c>. Each library runs one validator type with <see cref="Rules"/> rules (Name not
/// empty; with 3 also Email not empty and Age greater than 0; no email-format rule, the regexes differ). The baseline
/// checks the same rules by hand in the handler. SharpGrip runs FluentValidation 12 (labelled: another engine). Invalid
/// bodies are labelled: CSE lists <c>errors[]</c>, the framework and SharpGrip write an <c>errors</c> dictionary;
/// parity counts the entries. The framework's net10+ DataAnnotations <c>AddValidation()</c> is not included: it has no
/// net8/net9 support.
/// </summary>
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "BenchmarkDotNet owns the lifetime: the hosts are created in [GlobalSetup] and disposed in [GlobalCleanup].")]
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class ValidationFilterComparison
{
    private const string Validate = "Validate";
    private const string Path = "/users";

    private static readonly byte[] ValidBody = """{"name":"ada","email":"ada-bench-local","age":36}"""u8.ToArray();
    private static readonly byte[] InvalidBody = """{"name":"","email":"","age":0}"""u8.ToArray();

    private BenchHost _bcl = null!;
    private BenchHost _cse = null!;
    private BenchHost _sharpGrip = null!;
    private byte[] _body = null!;

    [Params(1, 3)]
    public int Rules { get; set; } = 1;

    [Params("Valid", "Invalid")]
    public string Outcome { get; set; } = "Valid";

    internal string Bodies { get; private set; } = string.Empty;

    [GlobalSetup]
    public void Setup()
    {
        _body = Outcome == "Valid" ? ValidBody : InvalidBody;
        int rules = Rules;
        _bcl = BenchHost.Start(static _ => { }, app => app.MapPost(Path, IResult (CreateUser request, HandlerCounter counter) =>
        {
            Dictionary<string, string[]>? errors = null;
            if (string.IsNullOrEmpty(request.Name))
                (errors ??= [])["Name"] = ["'Name' must not be empty."];
            if (rules == 3 && string.IsNullOrEmpty(request.Email))
                (errors ??= [])["Email"] = ["'Email' must not be empty."];
            if (rules == 3 && request.Age <= 0)
                (errors ??= [])["Age"] = ["'Age' must be greater than '0'."];
            if (errors is not null)
                return TypedResults.ValidationProblem(errors);

            counter.Increment();
            return TypedResults.Ok(request.Name);
        }));
        _cse = BenchHost.Start(b =>
        {
            b.Services.AddEnhancedProblemDetails();
            if (rules == 1)
                b.Services.AddValidator<CreateUser, CreateUserValidator1>();
            else
                b.Services.AddValidator<CreateUser, CreateUserValidator3>();
        }, static app => app.MapPost(Path, Handle).WithValidation<CreateUser>());
        _sharpGrip = BenchHost.Start(b =>
        {
            if (rules == 1)
                b.Services.AddScoped<FluentValidator, CreateUserFluentValidator1>();
            else
                b.Services.AddScoped<FluentValidator, CreateUserFluentValidator3>();
            b.Services.AddFluentValidationAutoValidation();
        }, static app => app.MapPost(Path, Handle).AddFluentValidationAutoValidation());

        Probe bcl = _bcl.ProbePost(Path, _body);
        Probe cse = _cse.ProbePost(Path, _body);
        Probe sharpGrip = _sharpGrip.ProbePost(Path, _body);
        if (Outcome == "Valid")
        {
            string ok = Parity.Of(HttpStatusCode.OK, "\"ada\""u8.ToArray(), 1);
            Parity.Same(Validate, ("expected", ok),
                (nameof(Bcl_Inline), Parity.Of(bcl.Status, bcl.Body, bcl.HandlerCalls)),
                (nameof(CSharpEssentials_WithValidation), Parity.Of(cse.Status, cse.Body, cse.HandlerCalls)),
                (nameof(SharpGrip_AutoValidation), Parity.Of(sharpGrip.Status, sharpGrip.Body, sharpGrip.HandlerCalls)));
            return;
        }

        string rejected = $"400|errors={Parity.Of(Rules)}|calls=0";
        Parity.Same(Validate, ("expected", rejected),
            (nameof(Bcl_Inline), Rejected(bcl)),
            (nameof(CSharpEssentials_WithValidation), Rejected(cse)),
            (nameof(SharpGrip_AutoValidation), Rejected(sharpGrip)));
        Parity.Same(Validate + " media type", ("expected", "application/problem+json"), (nameof(Bcl_Inline), Parity.Of(bcl.MediaType)), (nameof(CSharpEssentials_WithValidation), Parity.Of(cse.MediaType)));
        Bodies = $"  Bcl ({bcl.MediaType}): {Parity.Text(bcl.Body)}{Environment.NewLine}  CSE ({cse.MediaType}): {Parity.Text(cse.Body)}{Environment.NewLine}  SharpGrip ({sharpGrip.MediaType}): {Parity.Text(sharpGrip.Body)}{Environment.NewLine}";
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _bcl.Stop();
        _cse.Stop();
        _sharpGrip.Stop();
    }

    // https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/responses#problem-and-validationproblem
    [Benchmark(Baseline = true), BenchmarkCategory(Validate)]
    public Task<HttpStatusCode> Bcl_Inline() => _bcl.PostAsync(Path, _body);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore/Readme.MD#validation-endpoint-filter
    [Benchmark, BenchmarkCategory(Validate)]
    public Task<HttpStatusCode> CSharpEssentials_WithValidation() => _cse.PostAsync(Path, _body);

    // https://github.com/SharpGrip/FluentValidation.AutoValidation#minimal-apis-endpoints- and https://docs.fluentvalidation.net/en/latest/di.html
    [Benchmark, BenchmarkCategory(Validate)]
    public Task<HttpStatusCode> SharpGrip_AutoValidation() => _sharpGrip.PostAsync(Path, _body);

    private static Microsoft.AspNetCore.Http.HttpResults.Ok<string> Handle(CreateUser request, HandlerCounter counter)
    {
        counter.Increment();
        return TypedResults.Ok(request.Name);
    }

    private static string Rejected(Probe probe) =>
        $"{(int)probe.Status}|errors={Parity.Of(Parity.ErrorCount(probe.Body))}|calls={Parity.Of(probe.HandlerCalls)}";
}
