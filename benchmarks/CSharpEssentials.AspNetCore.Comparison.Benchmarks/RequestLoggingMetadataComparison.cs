using System.Diagnostics.CodeAnalysis;
using CSharpEssentials.RequestResponseLogging;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Logging;
using Serilog;

namespace CSharpEssentials.AspNetCore.Comparison.Benchmarks;

/// <summary>
/// GET /ping with no request body and a 1 KB JSON response: one metadata event per request. CSE logs method, path and
/// timing with both bodies skipped by <c>SkipRequestResponseLogging</c> (labelled: CSE has no status code field);
/// Serilog's request logging and HttpLogging's metadata fields include the status code. Never compare these rows with
/// the body rows of <see cref="RequestLoggingComparison"/>.
/// </summary>
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "BenchmarkDotNet owns the lifetime: the hosts are created in [GlobalSetup] and disposed in [GlobalCleanup].")]
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class RequestLoggingMetadataComparison
{
    private const string Metadata = "Metadata";
    private const string Path = "/ping";

    private BenchHost _plain = null!;
    private BenchHost _cse = null!;
    private BenchHost _serilog = null!;
    private BenchHost _httpLogging = null!;

    internal string Facts { get; private set; } = string.Empty;

    [GlobalSetup]
    public void Setup()
    {
        byte[] response = Payloads.DataEnvelope(1024, Payloads.ResponseLogSeed);
        CountingLoggerProvider plainLog = null!;
        CountingLoggerProvider cseLog = null!;
        CountingSerilogSink serilogSink = null!;
        CountingLoggerProvider httpLog = null!;

        _plain = BenchHost.Start(b => plainLog = LoggingHosts.UseCountingLogger(b), app => MapPing(app, response));
        _cse = BenchHost.Start(b => cseLog = LoggingHosts.UseCountingLogger(b), app =>
        {
            app.UseRouting();
            app.AddRequestResponseLogging(o => o.UseLogger(app.Services.GetRequiredService<ILoggerFactory>(), static l => l.LoggingFields = [LogFields.Method, LogFields.Path, LogFields.ResponseTiming]));
            MapPing(app, response).WithMetadata(new SkipRequestResponseLoggingAttribute());
        });
        _serilog = BenchHost.Start(b => serilogSink = LoggingHosts.UseCountingSerilog(b), app =>
        {
            app.UseSerilogRequestLogging();
            MapPing(app, response);
        });
        _httpLogging = BenchHost.Start(b =>
        {
            httpLog = LoggingHosts.UseCountingLogger(b);
            b.Services.AddHttpLogging(static o =>
            {
                o.LoggingFields = HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode | HttpLoggingFields.Duration;
                o.CombineLogs = true;
            });
        }, app =>
        {
            app.UseHttpLogging();
            MapPing(app, response);
        });

        string ok = Parity.Of(HttpStatusCode.OK, response, 1);
        Parity.Same(Metadata,
            ("expected", $"{ok}|events=0|{ok}|events=1|{ok}|events=1|{ok}|events=1"),
            ("all rows", string.Join("|",
                Check(_plain, plainLog.Tally, out _),
                Check(_cse, cseLog.Tally, out string cseMessage),
                Check(_serilog, serilogSink.Tally, out string serilogMessage),
                Check(_httpLogging, httpLog.Tally, out string httpMessage))));
        Facts = $"CSE ({cseMessage.Length}): {cseMessage.ReplaceLineEndings(" / ")}{Environment.NewLine}  Serilog ({serilogMessage.Length}): {serilogMessage}{Environment.NewLine}  HttpLogging ({httpMessage.Length}): {httpMessage.ReplaceLineEndings(" / ")}";
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _plain.Stop();
        _cse.Stop();
        _serilog.Stop();
        _httpLogging.Stop();
    }

    // https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis
    [Benchmark(Baseline = true), BenchmarkCategory(Metadata)]
    public Task<HttpStatusCode> Plain_NoLogging() => _plain.GetAsync(Path);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.RequestResponseLogging/Readme.MD#per-endpoint-control and #custom-fields
    [Benchmark, BenchmarkCategory(Metadata)]
    public Task<HttpStatusCode> CSharpEssentials_SkipBodies() => _cse.GetAsync(Path);

    // https://github.com/serilog/serilog-aspnetcore#request-logging
    [Benchmark, BenchmarkCategory(Metadata)]
    public Task<HttpStatusCode> Serilog_RequestLogging() => _serilog.GetAsync(Path);

    // https://learn.microsoft.com/aspnet/core/fundamentals/http-logging/#http-logging-options
    [Benchmark, BenchmarkCategory(Metadata)]
    public Task<HttpStatusCode> HttpLogging_Metadata() => _httpLogging.GetAsync(Path);

    private static IEndpointConventionBuilder MapPing(WebApplication app, byte[] response)
    {
        HandlerCounter counter = app.Services.GetRequiredService<HandlerCounter>();
        return app.MapGet(Path, async http =>
        {
            counter.Increment();
            http.Response.ContentType = "application/json";
            await http.Response.Body.WriteAsync(response, http.RequestAborted).ConfigureAwait(false);
        });
    }

    private static string Check(BenchHost host, LogTally tally, out string message)
    {
        tally.Capture = true;
        int before = tally.Events;
        Probe probe = host.ProbeGet(Path);
        int events = tally.Events - before;
        tally.Capture = false;
        message = events == 0 ? string.Empty : tally.LastMessage!;
        return $"{Parity.Of(probe.Status, probe.Body, probe.HandlerCalls)}|events={Parity.Of(events)}";
    }
}
