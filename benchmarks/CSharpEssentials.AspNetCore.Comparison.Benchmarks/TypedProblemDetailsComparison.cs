using System.Diagnostics.CodeAnalysis;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.AspNetCore.Comparison.Benchmarks;

/// <summary>
/// One GET whose handler fails with "order 42 not found" and returns a 404 problem+json, on one host with CSE's enhanced
/// problem details. TypedToHttpResult: the handler returns <c>Result&lt;int, NotFoundError&gt;.ToHttpResult()</c>.
/// ToProblemResult: it returns <c>Error.NotFound(...).ToProblemResult()</c>, the untyped route. Hand-written: it returns
/// <c>TypedResults.Problem</c> with the same status, title and detail. The error is created inside the handler on every row.
/// Bodies differ (CSE adds <c>errorCodes</c> and a trace id), so parity compares status, media type and the <c>status</c>
/// member only, as in <see cref="ProblemDetailsComparison"/>.
/// </summary>
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "BenchmarkDotNet owns the lifetime: the host is created in [GlobalSetup] and disposed in [GlobalCleanup].")]
[MemoryDiagnoser]
public class TypedProblemDetailsComparison
{
    private BenchHost _host = null!;

    internal string Bodies { get; private set; } = string.Empty;

    [GlobalSetup]
    public void Setup()
    {
        _host = BenchHost.Start(static b => b.Services.AddEnhancedProblemDetails(), static app =>
        {
            app.UseEnhancedProblemDetails();
            app.MapGet("/typed", static (HandlerCounter counter) =>
            {
                counter.Increment();
                Result<int, NotFoundError> result = new NotFoundError("order.notFound", "No order 42");
                return result.ToHttpResult();
            });
            app.MapGet("/error", static (HandlerCounter counter) =>
            {
                counter.Increment();
                return Error.NotFound("order.notFound", "No order 42").ToProblemResult();
            });
            app.MapGet("/hand", static (HandlerCounter counter) =>
            {
                counter.Increment();
                return TypedResults.Problem(statusCode: StatusCodes.Status404NotFound, title: "order.notFound", detail: "No order 42");
            });
        });

        List<(string, string)> outcomes = [("expected", "404|application/problem+json|status=404|calls=1")];
        System.Text.StringBuilder bodies = new();
        foreach (string path in new[] { "/typed", "/error", "/hand" })
        {
            Probe probe = _host.ProbeGet(path);
            outcomes.Add((path, Parity.Problem(probe)));
            bodies.Append("  ").Append(path).Append(": ").AppendLine(Parity.Text(probe.Body));
        }

        Parity.Same(nameof(TypedProblemDetailsComparison), [.. outcomes]);
        Bodies = bodies.ToString();
    }

    [GlobalCleanup]
    public void Cleanup() => _host.Stop();

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore/Readme.MD#typed-results
    [Benchmark(Baseline = true)]
    public Task<HttpStatusCode> CSharpEssentials_TypedToHttpResult() => _host.GetAsync("/typed");

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore/Readme.MD#returning-problems-from-your-own-code
    [Benchmark]
    public Task<HttpStatusCode> CSharpEssentials_ToProblemResult() => _host.GetAsync("/error");

    // https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/responses#problem-and-validationproblem
    [Benchmark]
    public Task<HttpStatusCode> HandWritten_TypedResultsProblem() => _host.GetAsync("/hand");
}
