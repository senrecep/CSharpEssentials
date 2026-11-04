using System.Diagnostics.CodeAnalysis;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Net.Http.Headers;

namespace CSharpEssentials.AspNetCore.Comparison.Benchmarks;

/// <summary>
/// GET /articles/7 returning an article whose JSON is exactly <see cref="Size"/> bytes (pseudo-random body text).
/// BodyHash rows compute an ETag for an arbitrary body: CSE a weak SHA-256 of a second serialization, Marvin a strong MD5
/// of its store key and the buffered body, plus Cache-Control, Expires, Last-Modified and Vary and a validator store
/// entry (labelled). On a matching If-None-Match, Marvin answers 304 from its validator store before the handler runs,
/// CSE runs the handler, hashes and then answers 304 (labelled; parity records the handler calls). Versioned rows take
/// the ETag from a version property: a hand-written header against CSE's <c>IVersioned</c>. Each 304 row sends the ETag
/// its own host returned.
/// </summary>
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "BenchmarkDotNet owns the lifetime: the hosts are created in [GlobalSetup] and disposed in [GlobalCleanup].")]
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class ConditionalGetComparison
{
    private const string BodyHash200 = "BodyHash200";
    private const string BodyHash304 = "BodyHash304";
    private const string Versioned200 = "Versioned200";
    private const string Versioned304 = "Versioned304";
    private const string Path = "/articles/7";

    private static readonly EntityTagHeaderValue VersionTag = new("\"17\"");

    private BenchHost _plain = null!;
    private BenchHost _cseHash = null!;
    private BenchHost _marvin = null!;
    private BenchHost _handWritten = null!;
    private BenchHost _cseVersioned = null!;
    private string _cseHashTag = null!;
    private string _marvinTag = null!;

    [Params(1024, 65536)]
    public int Size { get; set; }

    internal string ETags { get; private set; } = string.Empty;

    [GlobalSetup]
    public void Setup()
    {
        int overhead = Payloads.Overhead(new ArticleDto(7, "conditional-get", string.Empty));
        string body = Payloads.Text(Size - overhead, Payloads.ArticleSeed);
        ArticleDto article = new(7, "conditional-get", body);
        VersionedArticleDto versioned = new(7, "conditional-get", body);
        byte[] expected = Payloads.Json(article);
        Parity.True(BodyHash200, expected.Length == Size && Payloads.Json(versioned).AsSpan().SequenceEqual(expected), "the article JSON is not exactly Size bytes or the versioned JSON differs");

        _plain = BenchHost.Start(static _ => { }, app => app.MapGet(Path, (HandlerCounter counter) =>
        {
            counter.Increment();
            return TypedResults.Ok(article);
        }));
        _cseHash = BenchHost.Start(static b => b.Services.AddConditionalRequests(static o => o.UseBodyHashFallback = true), app => app.MapGet(Path, (HandlerCounter counter) =>
        {
            counter.Increment();
            return TypedResults.Ok(article);
        }).WithConditionalGet());
        _marvin = BenchHost.Start(static b => b.Services.AddHttpCacheHeaders(), app =>
        {
            app.UseRouting();
            app.UseHttpCacheHeaders();
            app.MapGet(Path, (HandlerCounter counter) =>
            {
                counter.Increment();
                return TypedResults.Ok(article);
            });
        });
        _handWritten = BenchHost.Start(static _ => { }, app => app.MapGet(Path, Results<Ok<VersionedArticleDto>, StatusCodeHttpResult> (HttpContext http, HandlerCounter counter) =>
        {
            counter.Increment();
            http.Response.GetTypedHeaders().ETag = VersionTag;
            return http.Request.GetTypedHeaders().IfNoneMatch.Any(static tag => tag.Compare(VersionTag, useStrongComparison: false))
                ? TypedResults.StatusCode(StatusCodes.Status304NotModified)
                : TypedResults.Ok(versioned);
        }));
        _cseVersioned = BenchHost.Start(static b => b.Services.AddConditionalRequests(), app => app.MapGet(Path, (HandlerCounter counter) =>
        {
            counter.Increment();
            return TypedResults.Ok(versioned);
        }).WithConditionalGet());

        string ok = Parity.Of(HttpStatusCode.OK, expected, 1);
        Probe plain = _plain.ProbeGet(Path);
        Probe cseHash = _cseHash.ProbeGet(Path);
        Probe marvin = _marvin.ProbeGet(Path);
        Probe handWritten = _handWritten.ProbeGet(Path);
        Probe cseVersioned = _cseVersioned.ProbeGet(Path);
        Parity.Same(BodyHash200, ("expected", ok),
            (nameof(Bcl_NoETag), Parity.Of(plain.Status, plain.Body, plain.HandlerCalls)),
            (nameof(CSharpEssentials_BodyHash), Parity.Of(cseHash.Status, cseHash.Body, cseHash.HandlerCalls)),
            (nameof(Marvin_Default), Parity.Of(marvin.Status, marvin.Body, marvin.HandlerCalls)));
        Parity.Same(Versioned200, ("expected", ok),
            (nameof(Bcl_HandWrittenETag), Parity.Of(handWritten.Status, handWritten.Body, handWritten.HandlerCalls)),
            (nameof(CSharpEssentials_Versioned), Parity.Of(cseVersioned.Status, cseVersioned.Body, cseVersioned.HandlerCalls)));
        Parity.True(BodyHash200, plain.ETag is null && cseHash.ETag is not null && marvin.ETag is not null, "ETag presence differs from the design");
        Parity.Same(Versioned200, ("expected", "\"17\""), (nameof(Bcl_HandWrittenETag), Parity.Of(handWritten.ETag)), (nameof(CSharpEssentials_Versioned), Parity.Of(cseVersioned.ETag)));
        _cseHashTag = cseHash.ETag!;
        _marvinTag = marvin.ETag!;

        Probe plain304 = _plain.ProbeGet(Path, _cseHashTag);
        Probe cseHash304 = _cseHash.ProbeGet(Path, _cseHashTag);
        Probe marvin304 = _marvin.ProbeGet(Path, _marvinTag);
        Probe handWritten304 = _handWritten.ProbeGet(Path, "\"17\"");
        Probe cseVersioned304 = _cseVersioned.ProbeGet(Path, "\"17\"");
        Parity.Same(BodyHash304 + " baseline", ("expected", ok), (nameof(Bcl_NoETag_IfNoneMatch), Parity.Of(plain304.Status, plain304.Body, plain304.HandlerCalls)));
        Parity.Same(BodyHash304, ("expected", Parity.Of(HttpStatusCode.NotModified, [], 1)), (nameof(CSharpEssentials_BodyHash_NotModified), Parity.Of(cseHash304.Status, cseHash304.Body, cseHash304.HandlerCalls)));
        Parity.Same(BodyHash304 + " (Marvin skips the handler)", ("expected", Parity.Of(HttpStatusCode.NotModified, [], 0)), (nameof(Marvin_NotModified), Parity.Of(marvin304.Status, marvin304.Body, marvin304.HandlerCalls)));
        Parity.Same(Versioned304, ("expected", Parity.Of(HttpStatusCode.NotModified, [], 1)),
            (nameof(Bcl_HandWrittenETag_NotModified), Parity.Of(handWritten304.Status, handWritten304.Body, handWritten304.HandlerCalls)),
            (nameof(CSharpEssentials_Versioned_NotModified), Parity.Of(cseVersioned304.Status, cseVersioned304.Body, cseVersioned304.HandlerCalls)));
        ETags = $"CSE body hash {_cseHashTag} [{cseHash.HeaderNames}]; Marvin {_marvinTag} [{marvin.HeaderNames}]; hand-written {handWritten.ETag}; CSE versioned {cseVersioned.ETag} [{cseVersioned.HeaderNames}]; 304 handler calls: CSE {cseHash304.HandlerCalls}, Marvin {marvin304.HandlerCalls}";
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _plain.Stop();
        _cseHash.Stop();
        _marvin.Stop();
        _handWritten.Stop();
        _cseVersioned.Stop();
    }

    // https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/responses
    [Benchmark(Baseline = true), BenchmarkCategory(BodyHash200)]
    public Task<HttpStatusCode> Bcl_NoETag() => _plain.GetAsync(Path);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore/Readme.MD#conditional-requests
    [Benchmark, BenchmarkCategory(BodyHash200)]
    public Task<HttpStatusCode> CSharpEssentials_BodyHash() => _cseHash.GetAsync(Path);

    // https://github.com/KevinDockx/HttpCacheHeaders#usage
    [Benchmark, BenchmarkCategory(BodyHash200)]
    public Task<HttpStatusCode> Marvin_Default() => _marvin.GetAsync(Path);

    // https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/responses
    [Benchmark(Baseline = true), BenchmarkCategory(BodyHash304)]
    public Task<HttpStatusCode> Bcl_NoETag_IfNoneMatch() => _plain.GetAsync(Path, _cseHashTag);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore/Readme.MD#conditional-requests
    [Benchmark, BenchmarkCategory(BodyHash304)]
    public Task<HttpStatusCode> CSharpEssentials_BodyHash_NotModified() => _cseHash.GetAsync(Path, _cseHashTag);

    // https://github.com/KevinDockx/HttpCacheHeaders#marking-for-invalidation
    [Benchmark, BenchmarkCategory(BodyHash304)]
    public Task<HttpStatusCode> Marvin_NotModified() => _marvin.GetAsync(Path, _marvinTag);

    // https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.http.headers.responseheaders.etag
    [Benchmark(Baseline = true), BenchmarkCategory(Versioned200)]
    public Task<HttpStatusCode> Bcl_HandWrittenETag() => _handWritten.GetAsync(Path);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore/Readme.MD#conditional-requests
    [Benchmark, BenchmarkCategory(Versioned200)]
    public Task<HttpStatusCode> CSharpEssentials_Versioned() => _cseVersioned.GetAsync(Path);

    // https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.http.headers.responseheaders.etag
    [Benchmark(Baseline = true), BenchmarkCategory(Versioned304)]
    public Task<HttpStatusCode> Bcl_HandWrittenETag_NotModified() => _handWritten.GetAsync(Path, "\"17\"");

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore/Readme.MD#conditional-requests
    [Benchmark, BenchmarkCategory(Versioned304)]
    public Task<HttpStatusCode> CSharpEssentials_Versioned_NotModified() => _cseVersioned.GetAsync(Path, "\"17\"");
}
