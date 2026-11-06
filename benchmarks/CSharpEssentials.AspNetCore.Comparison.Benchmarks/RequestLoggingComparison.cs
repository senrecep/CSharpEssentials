using System.Diagnostics.CodeAnalysis;
using CSharpEssentials.RequestResponseLogging;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.AspNetCore.Comparison.Benchmarks;

/// <summary>
/// POST /echo with a <see cref="BodySize"/>-byte JSON request; the handler reads the request and writes a different
/// <see cref="BodySize"/>-byte JSON response (both pseudo-random, different seeds). Every host counts and renders the
/// events of the middleware under test only. Bodies compares CSE's all-fields logger with HttpLogging set to the same
/// kind of fields and 2 MiB body limits (labelled: CSE buffers the whole response before sending it and buffers or
/// spills the request; HttpLogging tees the stream; CSE logs the host name, HttpLogging the status code).
/// HttpLogging_Defaults (labelled) keeps the 32 KB default limits, so the 64 KB and 1 MB bodies are truncated.
/// </summary>
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "BenchmarkDotNet owns the lifetime: the hosts are created in [GlobalSetup] and disposed in [GlobalCleanup].")]
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class RequestLoggingComparison
{
    private const string Bodies = "Bodies";
    private const string Path = "/echo";
    private const int BodyLimit = 2 * 1024 * 1024;

    private BenchHost _plain = null!;
    private BenchHost _cse = null!;
    private BenchHost _httpLogging = null!;
    private BenchHost _httpLoggingDefaults = null!;
    private byte[] _request = null!;

    [Params(1024, 65536, 1048576)]
    public int BodySize { get; set; } = 1024;

    internal string Facts { get; private set; } = string.Empty;

    [GlobalSetup]
    public void Setup()
    {
        _request = Payloads.DataEnvelope(BodySize, Payloads.RequestLogSeed);
        byte[] response = Payloads.DataEnvelope(BodySize, Payloads.ResponseLogSeed);
        CountingLoggerProvider plainLog = null!;
        CountingLoggerProvider cseLog = null!;
        CountingLoggerProvider httpLog = null!;
        CountingLoggerProvider defaultsLog = null!;

        _plain = BenchHost.Start(b => plainLog = LoggingHosts.UseCountingLogger(b), app => MapEcho(app, response));
        _cse = BenchHost.Start(b => cseLog = LoggingHosts.UseCountingLogger(b), app =>
        {
            app.UseRouting();
            app.AddRequestResponseLogging(o => o.UseLogger(app.Services.GetRequiredService<ILoggerFactory>(), LoggingOptions.CreateAllFields()));
            MapEcho(app, response);
        });
        _httpLogging = BenchHost.Start(b =>
        {
            httpLog = LoggingHosts.UseCountingLogger(b);
            b.Services.AddHttpLogging(static o =>
            {
                o.LoggingFields = HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.RequestQuery | HttpLoggingFields.RequestBody
                    | HttpLoggingFields.ResponseStatusCode | HttpLoggingFields.ResponseBody | HttpLoggingFields.Duration;
                o.RequestBodyLogLimit = BodyLimit;
                o.ResponseBodyLogLimit = BodyLimit;
                o.CombineLogs = true;
            });
        }, app =>
        {
            app.UseHttpLogging();
            MapEcho(app, response);
        });
        _httpLoggingDefaults = BenchHost.Start(b =>
        {
            defaultsLog = LoggingHosts.UseCountingLogger(b);
            b.Services.AddHttpLogging(static o =>
            {
                o.LoggingFields = HttpLoggingFields.All;
                o.CombineLogs = true;
            });
        }, app =>
        {
            app.UseHttpLogging();
            MapEcho(app, response);
        });

        string ok = Parity.Of(HttpStatusCode.OK, response, 1);
        string requestTail = Parity.Text(_request)[^34..^2];
        string responseTail = Parity.Text(response)[^34..^2];
        Parity.Same(Bodies,
            ("expected", $"{ok}|events=0|{ok}|events=1|request=True|response=True|{ok}|events=1|request=True|response=True|{ok}|events=1"),
            ("all rows", string.Join("|",
                Check(_plain, plainLog.Tally, requestTail, responseTail, out _, tails: false),
                Check(_cse, cseLog.Tally, requestTail, responseTail, out int cseLength, tails: true),
                Check(_httpLogging, httpLog.Tally, requestTail, responseTail, out int httpLength, tails: true),
                Check(_httpLoggingDefaults, defaultsLog.Tally, requestTail, responseTail, out int defaultsLength, tails: false))));
        string defaultsTails = $"request={defaultsLog.Tally.LastMessage!.Contains(requestTail, StringComparison.Ordinal)} response={defaultsLog.Tally.LastMessage.Contains(responseTail, StringComparison.Ordinal)}";
        Facts = $"message length CSE {cseLength}, HttpLogging_Bodies {httpLength}, HttpLogging_Defaults {defaultsLength} ({defaultsTails})";
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _plain.Stop();
        _cse.Stop();
        _httpLogging.Stop();
        _httpLoggingDefaults.Stop();
    }

    // https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis
    [Benchmark(Baseline = true), BenchmarkCategory(Bodies)]
    public Task<HttpStatusCode> Plain_NoLogging() => _plain.PostAsync(Path, _request);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.RequestResponseLogging/Readme.MD#registration
    [Benchmark, BenchmarkCategory(Bodies)]
    public Task<HttpStatusCode> CSharpEssentials_AllFields() => _cse.PostAsync(Path, _request);

    // https://learn.microsoft.com/aspnet/core/fundamentals/http-logging/#http-logging-options (#requestbodyloglimit-and-responsebodyloglimit, #combinelogs)
    [Benchmark, BenchmarkCategory(Bodies)]
    public Task<HttpStatusCode> HttpLogging_Bodies() => _httpLogging.PostAsync(Path, _request);

    // https://learn.microsoft.com/aspnet/core/fundamentals/http-logging/#http-logging-options
    [Benchmark, BenchmarkCategory(Bodies)]
    public Task<HttpStatusCode> HttpLogging_Defaults() => _httpLoggingDefaults.PostAsync(Path, _request);

    private static void MapEcho(WebApplication app, byte[] response)
    {
        HandlerCounter counter = app.Services.GetRequiredService<HandlerCounter>();
        app.MapPost(Path, async http =>
        {
            counter.Increment();
            await http.Request.Body.CopyToAsync(Stream.Null, http.RequestAborted).ConfigureAwait(false);
            http.Response.ContentType = "application/json";
            await http.Response.Body.WriteAsync(response, http.RequestAborted).ConfigureAwait(false);
        });
    }

    private string Check(BenchHost host, LogTally tally, string requestTail, string responseTail, out int length, bool tails)
    {
        tally.Capture = true;
        int before = tally.Events;
        Probe probe = host.ProbePost(Path, _request);
        int events = tally.Events - before;
        tally.Capture = false;
        string message = events == 0 ? string.Empty : tally.LastMessage!;
        length = message.Length;
        string outcome = $"{Parity.Of(probe.Status, probe.Body, probe.HandlerCalls)}|events={Parity.Of(events)}";
        return tails
            ? $"{outcome}|request={message.Contains(requestTail, StringComparison.Ordinal)}|response={message.Contains(responseTail, StringComparison.Ordinal)}"
            : outcome;
    }
}
