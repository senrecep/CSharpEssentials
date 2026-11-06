using System.Diagnostics.CodeAnalysis;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CSharpEssentials.AspNetCore.Comparison.Benchmarks;

/// <summary>
/// GET /orders/{id}: id 42 returns <c>OrderDto("order-42", "pending", 3)</c> (Success), any other id a 404 "Order not
/// found" (Failure). Every row runs the same lookup on its own Production host. ManualMatch (labelled) is a hand-written
/// match with CSE types, so ManualMatch minus ResultFilter is the filter's own cost. Failure bodies differ (labelled):
/// CSE adds <c>errorCodes</c>, <c>instance</c> and a trace id; parity compares status, media type and the
/// <c>status</c> member.
/// </summary>
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "BenchmarkDotNet owns the lifetime: the hosts are created in [GlobalSetup] and disposed in [GlobalCleanup].")]
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class ResultFilterComparison
{
    private const string Result = "Result";

    private static readonly OrderDto Order = new("order-42", "pending", 3);

    private BenchHost _bcl = null!;
    private BenchHost _filter = null!;
    private BenchHost _manual = null!;
    private string _path = null!;

    [Params("Success", "Failure")]
    public string Outcome { get; set; } = "Success";

    [GlobalSetup]
    public void Setup()
    {
        _path = Outcome == "Success" ? "/orders/42" : "/orders/404";
        _bcl = BenchHost.Start(
            b => b.Services.AddProblemDetails(),
            app => app.MapGet("/orders/{id:int}", Results<Ok<OrderDto>, ProblemHttpResult> (int id, HandlerCounter counter) =>
            {
                counter.Increment();
                return id == 42 ? TypedResults.Ok(Order) : TypedResults.Problem(statusCode: StatusCodes.Status404NotFound, title: "Order not found");
            }));
        _filter = BenchHost.Start(
            b => b.Services.AddEnhancedProblemDetails(),
            app => app.MapGet("/orders/{id:int}", (int id, HandlerCounter counter) =>
            {
                counter.Increment();
                return Find(id);
            }).AddEndpointFilter<ResultEndpointFilter>());
        _manual = BenchHost.Start(
            b => b.Services.AddEnhancedProblemDetails(),
            app => app.MapGet("/orders/{id:int}", (int id, HandlerCounter counter) =>
            {
                counter.Increment();
                Result<OrderDto> result = Find(id);
                return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemResult();
            }));

        Probe bcl = _bcl.ProbeGet(_path);
        Probe filter = _filter.ProbeGet(_path);
        Probe manual = _manual.ProbeGet(_path);
        if (Outcome == "Success")
        {
            Parity.Same(Result,
                ("expected", Parity.Of(HttpStatusCode.OK, Payloads.Json(Order), 1)),
                (nameof(Bcl_TypedResults), Parity.Of(bcl.Status, bcl.Body, bcl.HandlerCalls)),
                (nameof(CSharpEssentials_ResultFilter), Parity.Of(filter.Status, filter.Body, filter.HandlerCalls)),
                (nameof(CSharpEssentials_ManualMatch), Parity.Of(manual.Status, manual.Body, manual.HandlerCalls)));
            Parity.Same(Result + " media type", ("expected", "application/json"), ("bcl", Parity.Of(bcl.MediaType)), ("filter", Parity.Of(filter.MediaType)), ("manual", Parity.Of(manual.MediaType)));
        }
        else
        {
            Parity.Same(Result,
                ("expected", "404|application/problem+json|status=404|calls=1"),
                (nameof(Bcl_TypedResults), Parity.Problem(bcl)),
                (nameof(CSharpEssentials_ResultFilter), Parity.Problem(filter)),
                (nameof(CSharpEssentials_ManualMatch), Parity.Problem(manual)));
            Bodies = $"Bcl: {Parity.Text(bcl.Body)}{Environment.NewLine}  CSE filter: {Parity.Text(filter.Body)}{Environment.NewLine}  CSE manual: {Parity.Text(manual.Body)}";
        }
    }

    internal string Bodies { get; private set; } = string.Empty;

    [GlobalCleanup]
    public void Cleanup()
    {
        _bcl.Stop();
        _filter.Stop();
        _manual.Stop();
    }

    // https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/responses#typedresults-versus-results
    [Benchmark(Baseline = true), BenchmarkCategory(Result)]
    public Task<HttpStatusCode> Bcl_TypedResults() => _bcl.GetAsync(_path);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore/Readme.MD#result-endpoint-filter
    [Benchmark, BenchmarkCategory(Result)]
    public Task<HttpStatusCode> CSharpEssentials_ResultFilter() => _filter.GetAsync(_path);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore/Readme.MD#returning-problems-from-your-own-code
    [Benchmark, BenchmarkCategory(Result)]
    public Task<HttpStatusCode> CSharpEssentials_ManualMatch() => _manual.GetAsync(_path);

    private static Result<OrderDto> Find(int id) =>
        id == 42 ? Result<OrderDto>.Success(Order) : Result<OrderDto>.Failure(Error.NotFound("order.notFound", "Order not found"));
}
