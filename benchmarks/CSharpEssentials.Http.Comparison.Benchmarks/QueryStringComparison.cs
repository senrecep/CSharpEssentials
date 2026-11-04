using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Flurl;
using Microsoft.AspNetCore.WebUtilities;

namespace CSharpEssentials.Http.Comparison.Benchmarks;

/// <summary>
/// Builds <c>Q=c%23%20tips&amp;Page=2&amp;Include=profile</c> from a POCO (Object), from a
/// <c>Dictionary&lt;string, string?&gt;</c> (Dictionary) and appended to <c>https://bench.local/search</c> (AppendToUri).
/// CSE and Flurl read the POCO's public properties by reflection. QueryHelpers returns a leading '?', which the setup
/// strips before comparing. The BCL row is a hand-written <c>Uri.EscapeDataString</c> loop. Refit and RestSharp have no
/// standalone query-string builder. <see cref="EncodingProbe"/> shows how each library encodes other reserved characters.
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class QueryStringComparison
{
    private const string Object = "Object";
    private const string Dictionary = "Dictionary";
    private const string AppendToUri = "AppendToUri";

    [GlobalSetup]
    public void Setup()
    {
        Parity.Same(Object,
            ("expected", Inputs.SearchQueryString),
            (nameof(CSharpEssentials_Object), CSharpEssentials_Object().Value),
            (nameof(Flurl_Object), Flurl_Object()));
        Parity.Same(Dictionary,
            ("expected", Inputs.SearchQueryString),
            (nameof(CSharpEssentials_Dictionary), CSharpEssentials_Dictionary().Value),
            (nameof(AspNetCore_QueryHelpers), AspNetCore_QueryHelpers().TrimStart('?')),
            (nameof(Flurl_Dictionary), Flurl_Dictionary()),
            (nameof(Bcl_Dictionary), Bcl_Dictionary()));
        Parity.Same(AppendToUri,
            ("expected", $"{Inputs.SearchUriString}?{Inputs.SearchQueryString}"),
            (nameof(CSharpEssentials_AppendToUri), CSharpEssentials_AppendToUri().Value.AbsoluteUri),
            (nameof(AspNetCore_AppendToUri), AspNetCore_AppendToUri().AbsoluteUri),
            (nameof(Flurl_AppendToUri), Flurl_AppendToUri().AbsoluteUri));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Http/Readme.MD#query-string-builder
    [Benchmark(Baseline = true), BenchmarkCategory(Object)]
    public Result<string> CSharpEssentials_Object() => Inputs.Search.ToQueryString();

    // https://flurl.dev/docs/fluent-url/
    [Benchmark, BenchmarkCategory(Object)]
    public string Flurl_Object() => new Url().SetQueryParams(Inputs.Search).Query;

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Http/Readme.MD#query-string-builder
    [Benchmark(Baseline = true), BenchmarkCategory(Dictionary)]
    public Result<string> CSharpEssentials_Dictionary() => Inputs.SearchDictionary.ToQueryString();

    // https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.webutilities.queryhelpers.addquerystring
    [Benchmark, BenchmarkCategory(Dictionary)]
    public string AspNetCore_QueryHelpers() => QueryHelpers.AddQueryString(string.Empty, Inputs.SearchDictionary);

    // https://flurl.dev/docs/fluent-url/
    [Benchmark, BenchmarkCategory(Dictionary)]
    public string Flurl_Dictionary() => new Url().SetQueryParams(Inputs.SearchDictionary).Query;

    // https://learn.microsoft.com/dotnet/api/system.uri.escapedatastring
    [Benchmark, BenchmarkCategory(Dictionary)]
    public string Bcl_Dictionary()
    {
        var builder = new StringBuilder();
        foreach (KeyValuePair<string, string?> parameter in Inputs.SearchDictionary)
        {
            if (parameter.Value is null)
            {
                continue;
            }

            if (builder.Length > 0)
            {
                builder.Append('&');
            }

            builder.Append(Uri.EscapeDataString(parameter.Key)).Append('=').Append(Uri.EscapeDataString(parameter.Value));
        }

        return builder.ToString();
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Http/Readme.MD#query-string-builder
    [Benchmark(Baseline = true), BenchmarkCategory(AppendToUri)]
    public Result<Uri> CSharpEssentials_AppendToUri() => Inputs.SearchUri.WithQueryString(Inputs.SearchDictionary);

    // https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.webutilities.queryhelpers.addquerystring
    [Benchmark, BenchmarkCategory(AppendToUri)]
    public Uri AspNetCore_AppendToUri() => new(QueryHelpers.AddQueryString(Inputs.SearchUriString, Inputs.SearchDictionary));

    // https://flurl.dev/docs/fluent-url/
    [Benchmark, BenchmarkCategory(AppendToUri)]
    public Uri Flurl_AppendToUri() => new Url(Inputs.SearchUriString).SetQueryParams(Inputs.SearchDictionary).ToUri();

    public static string EncodingProbe()
    {
        var values = new Dictionary<string, string?> { ["v"] = "a+b/c?d&e=f ü" };
        return $"CSE '{values.ToQueryString().Value}', QueryHelpers '{QueryHelpers.AddQueryString(string.Empty, values).TrimStart('?')}', Flurl '{new Url().SetQueryParams(values).Query}', Uri.EscapeDataString 'v={Uri.EscapeDataString(values["v"]!)}'";
    }
}
