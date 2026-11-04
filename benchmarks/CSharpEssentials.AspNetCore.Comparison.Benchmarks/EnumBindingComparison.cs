using System.Diagnostics.CodeAnalysis;

namespace CSharpEssentials.AspNetCore.Comparison.Benchmarks;

/// <summary>
/// GET /orders?status={value} into <c>OrderStatus?</c> (<c>[StringEnum] Pending, PendingApproval</c>): Canonical
/// <c>PendingApproval</c>, Normalized <c>pending_approval</c> (the JSON spelling), Invalid <c>bogus</c>. The handler
/// returns the bound value as a string. Normalized and Invalid are labelled: the framework rejects the JSON spelling
/// with an empty 400 where CSE binds it, and on Invalid both answer 400 but only CSE writes a problem body listing the
/// allowed values. PassThrough measures the middleware on an endpoint without an enum parameter.
/// </summary>
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "BenchmarkDotNet owns the lifetime: the hosts are created in [GlobalSetup] and disposed in [GlobalCleanup].")]
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class EnumBindingComparison
{
    private const string Bind = "Bind";
    private const string PassThrough = "PassThrough";

    private BenchHost _bcl = null!;
    private BenchHost _cse = null!;
    private string _path = null!;

    [Params("Canonical", "Normalized", "Invalid")]
    public string Input { get; set; } = "Canonical";

    internal string BclOutcome { get; private set; } = string.Empty;

    [GlobalSetup]
    public void Setup()
    {
        string value = Input switch
        {
            "Canonical" => "PendingApproval",
            "Normalized" => "pending_approval",
            _ => "bogus",
        };
        _path = "/orders?status=" + value;
        _bcl = BenchHost.Start(static _ => { }, static app =>
        {
            app.UseRouting();
            Map(app);
        });
        _cse = BenchHost.Start(static b => b.Services.AddEnumConventions(), static app =>
        {
            app.UseRouting();
            app.UseEnumBinding();
            Map(app);
        });

        Probe bcl = _bcl.ProbeGet(_path);
        Probe cse = _cse.ProbeGet(_path);
        BclOutcome = $"{(int)bcl.Status} {Parity.Text(bcl.Body)} calls={bcl.HandlerCalls}";
        string bound = Parity.Of(HttpStatusCode.OK, "\"PendingApproval\""u8.ToArray(), 1);
        switch (Input)
        {
            case "Canonical":
                Parity.Same(Bind, ("expected", bound), (nameof(Bcl_NoMiddleware), Parity.Of(bcl.Status, bcl.Body, bcl.HandlerCalls)), (nameof(CSharpEssentials_UseEnumBinding), Parity.Of(cse.Status, cse.Body, cse.HandlerCalls)));
                break;
            case "Normalized":
                Parity.Same(Bind, ("expected", bound), (nameof(CSharpEssentials_UseEnumBinding), Parity.Of(cse.Status, cse.Body, cse.HandlerCalls)));
                Parity.True(Bind, bcl.Status == HttpStatusCode.OK ? bcl.HandlerCalls == 1 : bcl.Status == HttpStatusCode.BadRequest && bcl.HandlerCalls == 0, "the framework neither bound nor rejected the JSON spelling: " + BclOutcome);
                break;
            default:
                Parity.Same(Bind, ("expected", "400|calls=0"), (nameof(Bcl_NoMiddleware), $"{(int)bcl.Status}|calls={bcl.HandlerCalls}"), (nameof(CSharpEssentials_UseEnumBinding), $"{(int)cse.Status}|calls={cse.HandlerCalls}"));
                Parity.Same(Bind + " media type", ("expected", "application/problem+json"), (nameof(CSharpEssentials_UseEnumBinding), Parity.Of(cse.MediaType)));
                break;
        }

        string pong = Parity.Of(HttpStatusCode.OK, "\"pong\""u8.ToArray(), 1);
        Probe bclPing = _bcl.ProbeGet("/ping");
        Probe csePing = _cse.ProbeGet("/ping");
        Parity.Same(PassThrough, ("expected", pong), (nameof(Bcl_NoMiddleware_NoEnum), Parity.Of(bclPing.Status, bclPing.Body, bclPing.HandlerCalls)), (nameof(CSharpEssentials_UseEnumBinding_NoEnum), Parity.Of(csePing.Status, csePing.Body, csePing.HandlerCalls)));
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _bcl.Stop();
        _cse.Stop();
    }

    // https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/parameter-binding
    [Benchmark(Baseline = true), BenchmarkCategory(Bind)]
    public Task<HttpStatusCode> Bcl_NoMiddleware() => _bcl.GetAsync(_path);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore/Readme.MD#enum-conventions-json-binding-and-wire-format
    [Benchmark, BenchmarkCategory(Bind)]
    public Task<HttpStatusCode> CSharpEssentials_UseEnumBinding() => _cse.GetAsync(_path);

    // https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/parameter-binding
    [Benchmark(Baseline = true), BenchmarkCategory(PassThrough)]
    public Task<HttpStatusCode> Bcl_NoMiddleware_NoEnum() => _bcl.GetAsync("/ping");

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore/Readme.MD#enum-conventions-json-binding-and-wire-format
    [Benchmark, BenchmarkCategory(PassThrough)]
    public Task<HttpStatusCode> CSharpEssentials_UseEnumBinding_NoEnum() => _cse.GetAsync("/ping");

    private static void Map(WebApplication app)
    {
        app.MapGet("/orders", static (OrderStatus? status, HandlerCounter counter) =>
        {
            counter.Increment();
            return TypedResults.Ok(status?.ToString() ?? "none");
        });
        app.MapGet("/ping", static (HandlerCounter counter) =>
        {
            counter.Increment();
            return TypedResults.Ok("pong");
        });
    }
}
