using System.Diagnostics.CodeAnalysis;
using CSharpEssentials.Errors;
using Hellang.Middleware.ProblemDetails;

namespace CSharpEssentials.AspNetCore.Comparison.Benchmarks;

/// <summary>
/// problem+json on three Production hosts: the framework's <c>AddProblemDetails</c> with UseExceptionHandler and
/// UseStatusCodePages, CSE's enhanced problem details with GlobalExceptionHandler, and Hellang's middleware (which needs
/// MVC registered and writes through the MVC ObjectResult executor). Exception: the handler throws. StatusCode: the
/// handler returns an empty 404. ErrorResult: the handler returns a 404 problem from code (Hellang has no error-value API,
/// so no row). Bodies are labelled: titles, trace id formats and CSE's <c>errorCodes</c> differ, so parity compares
/// status, media type and the <c>status</c> member only.
/// </summary>
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "BenchmarkDotNet owns the lifetime: the hosts are created in [GlobalSetup] and disposed in [GlobalCleanup].")]
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class ProblemDetailsComparison
{
    private const string Exception = "Exception";
    private const string StatusCode = "StatusCode";
    private const string ErrorResult = "ErrorResult";

    private BenchHost _bcl = null!;
    private BenchHost _cse = null!;
    private BenchHost _hellang = null!;

    internal string Bodies { get; private set; } = string.Empty;

    [GlobalSetup]
    public void Setup()
    {
        _bcl = BenchHost.Start(static b => ProblemDetailsServiceCollectionExtensions.AddProblemDetails(b.Services), static app =>
        {
            app.UseExceptionHandler();
            app.UseStatusCodePages();
            Map(app);
            app.MapGet("/problem", static (HandlerCounter counter) =>
            {
                counter.Increment();
                return TypedResults.Problem(statusCode: StatusCodes.Status404NotFound, title: "Order not found", detail: "No order 42");
            });
        });
        _cse = BenchHost.Start(static b =>
        {
            b.Services.AddEnhancedProblemDetails();
            b.Services.AddExceptionHandler<GlobalExceptionHandler>();
        }, static app =>
        {
            app.UseEnhancedProblemDetails();
            Map(app);
            app.MapGet("/problem", static (HandlerCounter counter) =>
            {
                counter.Increment();
                return Error.NotFound("order.notFound", "No order 42").ToProblemResult();
            });
        });
        _hellang = BenchHost.Start(static b =>
        {
            b.Services.AddControllers();
            b.Services.AddProblemDetails(static o => o.MapToStatusCode<System.Exception>(StatusCodes.Status500InternalServerError));
        }, static app =>
        {
            app.UseProblemDetails();
            Map(app);
        });

        Bodies = string.Concat(
            Check(Exception, "/throw", "500", ("Bcl", _bcl), ("CSharpEssentials", _cse), ("Hellang", _hellang)),
            Check(StatusCode, "/status", "404", ("Bcl", _bcl), ("CSharpEssentials", _cse), ("Hellang", _hellang)),
            Check(ErrorResult, "/problem", "404", ("Bcl", _bcl), ("CSharpEssentials", _cse)));
    }


    [GlobalCleanup]
    public void Cleanup()
    {
        _bcl.Stop();
        _cse.Stop();
        _hellang.Stop();
    }

    // https://learn.microsoft.com/aspnet/core/fundamentals/error-handling-api#problem-details
    [Benchmark(Baseline = true), BenchmarkCategory(Exception)]
    public Task<HttpStatusCode> Bcl_AddProblemDetails() => _bcl.GetAsync("/throw");

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore/Readme.MD#problem-details
    [Benchmark, BenchmarkCategory(Exception)]
    public Task<HttpStatusCode> CSharpEssentials_GlobalExceptionHandler() => _cse.GetAsync("/throw");

    // https://github.com/khellang/Middleware/blob/2a04612/samples/ProblemDetails.MinimalApiSample/Program.cs
    [Benchmark, BenchmarkCategory(Exception)]
    public Task<HttpStatusCode> Hellang_UseProblemDetails() => _hellang.GetAsync("/throw");

    // https://learn.microsoft.com/aspnet/core/fundamentals/error-handling-api#problem-details
    [Benchmark(Baseline = true), BenchmarkCategory(StatusCode)]
    public Task<HttpStatusCode> Bcl_StatusCode() => _bcl.GetAsync("/status");

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore/Readme.MD#problem-details
    [Benchmark, BenchmarkCategory(StatusCode)]
    public Task<HttpStatusCode> CSharpEssentials_StatusCode() => _cse.GetAsync("/status");

    // https://github.com/khellang/Middleware/blob/2a04612/samples/ProblemDetails.MinimalApiSample/Program.cs
    [Benchmark, BenchmarkCategory(StatusCode)]
    public Task<HttpStatusCode> Hellang_StatusCode() => _hellang.GetAsync("/status");

    // https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/responses#problem-and-validationproblem
    [Benchmark(Baseline = true), BenchmarkCategory(ErrorResult)]
    public Task<HttpStatusCode> Bcl_TypedResultsProblem() => _bcl.GetAsync("/problem");

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore/Readme.MD#returning-problems-from-your-own-code
    [Benchmark, BenchmarkCategory(ErrorResult)]
    public Task<HttpStatusCode> CSharpEssentials_ToProblemResult() => _cse.GetAsync("/problem");

    private static string Check(string category, string path, string status, params (string Name, BenchHost Host)[] hosts)
    {
        List<(string, string)> outcomes = [("expected", $"{status}|application/problem+json|status={status}|calls=1")];
        System.Text.StringBuilder bodies = new();
        foreach ((string name, BenchHost host) in hosts)
        {
            Probe probe = host.ProbeGet(path);
            outcomes.Add((name, Parity.Problem(probe)));
            bodies.Append("  ").Append(category).Append(' ').Append(name).Append(": ").AppendLine(Parity.Text(probe.Body));
        }

        Parity.Same(category, [.. outcomes]);
        return bodies.ToString();
    }

    private static void Map(WebApplication app)
    {
        app.MapGet("/throw", static IResult (HandlerCounter counter) =>
        {
            counter.Increment();
            throw new InvalidOperationException("boom");
        });
        app.MapGet("/status", static (HandlerCounter counter) =>
        {
            counter.Increment();
            return TypedResults.NotFound();
        });
    }
}
