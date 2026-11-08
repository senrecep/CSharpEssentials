# Blog notes: CSharpEssentials.Http against HttpClient, Refit, Flurl.Http and RestSharp

These are notes for a post, not the post. The numbers come from the full default-job run under `benchmarks/results/http/{tfm}/results/`; [CSharpEssentials.Http/BENCHMARKS.md](../../CSharpEssentials.Http/BENCHMARKS.md#results) has every row on every TFM.

The suite runs on .NET 8, 9, 10 and 11 with BenchmarkDotNet 0.14.0. The libraries compared are `HttpClient` with System.Net.Http.Json, Refit 16.4.0 (MIT), Flurl.Http 4.0.2 (MIT), RestSharp 114.1.0 (Apache-2.0) and, for the resilience row, Polly.Core 8.9.0 (BSD-3-Clause) and Microsoft.Extensions.Http.Resilience 10.11.0 (MIT).

## Framing

- Working title: "What does a Result-returning HttpClient wrapper cost?"
- CSE.Http is not a typed client like Refit, and not a fluent HTTP DSL like Flurl or RestSharp. It is a set of `HttpClient` extensions that return `Result<T>` and map status codes to typed errors. The post should say that first, so the comparison is not read as "which REST library is fastest".
- The question the numbers answer: on top of the same `HttpClient`, what does each library add per call, in time and in allocations, for a GET, a POST, a 404, a request build and a query string?

## What each library provides

- HttpClient + System.Net.Http.Json: `GetFromJsonAsync`, `PostAsJsonAsync`, `ReadFromJsonAsync`, `JsonTypeInfo` overloads for source-generated JSON. Errors are exceptions or a manual status check.
- Refit 16.4.0: typed interfaces with attributes, implemented by a source generator (`RestService.ForGenerated`). `IApiResponse<T>` for calls that must not throw; `ForGenerated(client, context)` for source-generated JSON. Assembly metadata says trimmable and AOT compatible.
- Flurl.Http 4.0.2: a fluent URL and request builder (`Request().GetJsonAsync()`), `AllowHttpStatus` to opt out of exceptions, its own timeout handling (it sets the wrapped `HttpClient.Timeout` to infinite).
- RestSharp 114.1.0: `RestClient` and `RestRequest` with URL segments and query parameters, pluggable serializers, `Execute*` methods that return a response instead of throwing.
- CSE: `GetFromJsonAsResultAsync` and friends, a status-code-to-`ErrorType` table, `HttpRequestBuilder` (route values, query, headers, JSON body, idempotency key, redirects), query-string helpers, Polly resilience through CSharpEssentials.Resilience, an SSRF guard on net9+.

## Feature matrix

| Feature | CSE | BCL | Refit 16.4.0 | Flurl.Http 4.0.2 | RestSharp 114.1.0 |
|---|---|---|---|---|---|
| Non-2xx without exceptions | `Result` | Manual status check | `IApiResponse<T>` | `AllowHttpStatus` | `Execute*` |
| Status code to typed error | Built in | No | No | No | No |
| Typed interface client | No | No | Yes | No | No |
| Build a request without sending | `Build()` | `new HttpRequestMessage` | No | URL only | URI only |
| Query string from an object | Reflection | No | `[Query]` | Reflection | Reflection (`AddObject<T>`, or `GetAsync<T>(resource, object)`) |
| Source-generated JSON | `JsonTypeInfo<T>` overloads (not benchmarked yet) | Yes | Yes | Through options | Through options |
| Disposes the response | Yes | Yes | Yes | Yes | Yes |
| SSRF guard | Yes (net9+) | No | No | No | No |
| Trim/AOT metadata | AOT compatible on net9.0+ builds; reflection-based overloads annotated | Annotated | Trimmable + AOT compatible | None | None |

## Methodology

- Six BenchmarkDotNet classes for this package in `benchmarks/CSharpEssentials.Http.Comparison.Benchmarks/`, each with `[MemoryDiagnoser]`. CSE is the baseline in every category that has a CSE row.
- No network. One `HttpClient` over an in-memory handler is shared by every row, so `HttpClient.SendAsync` costs the same for all. The handler drains every request body (lazy and eager serializers do the same work) and returns a new response per call.
- One shared Web `JsonSerializerOptions` instance reaches every like-for-like row through that library's documented hook. Default-options rows and source-generated rows are separate, labelled categories.
- Every class checks parity in `[GlobalSetup]`, and `--self-check` runs the same checks without BenchmarkDotNet: same request bytes, same URI, same decoded value, same status. The setup also checks that no library changed the shared client. That check caught Flurl setting the timeout to infinite; the shared client now starts that way for every row. With an infinite client timeout, the CSE, BCL and Refit rows allocate no `CancellationTokenSource`. Flurl inherits `Timeout = InfiniteTimeSpan` and links a CTS per call, which also makes `HttpClient` link one. RestSharp always creates a 100 s CTS and a linked CTS. Both are library defaults.
- Rows that do different work are labelled, not compared as if they were the same: the 404 paths, the request-build rows for Flurl and RestSharp (URL builder plus a hand-made message), and the Microsoft.Extensions.Http.Resilience rows (retry around the send only, factory logging and telemetry on).
- Every competitor row follows its documented usage and carries a link to that documentation.

## How these numbers were produced

BenchmarkDotNet v0.14.0, default job, full runs on net8.0, net9.0, net10.0 and net11.0, one TFM after another on one Linux machine. The 1-minute load average was 1.74 at the start of the suite and 1.60 at the end. No row kept a coefficient of variation above 5%, so none is marked noisy. The resilience class was rerun on main after pull requests #230 to #256 landed, with newer runtime patches (.NET 8.0.33, 9.0.22, 10.0.14). Reports: `benchmarks/results/http/{tfm}/results/`.

## The numbers that matter

Headline rows on net10.0, as `Mean, Allocated` from the reports. On the other TFMs, CSE is ahead of or behind each competitor in the same rows as on net10.0, except in the two rows noted below the table.

| Row (net10.0) | CSE | BCL | Refit 16.4.0 | Flurl.Http 4.0.2 | RestSharp 114.1.0 |
|---|---|---|---|---|---|
| GET, Count 1 | 1.857 μs, 2.55 KB | 1.729 μs, 2.2 KB | 2.499 μs, 2.83 KB | 3.626 μs, 4.99 KB | 10.081 μs, 30.63 KB |
| GET, Count 50 | 21.337 μs, 18.36 KB | 21.141 μs, 14.97 KB | 23.694 μs, 18.64 KB | 23.392 μs, 20.8 KB | 33.767 μs, 66.46 KB |
| POST | 2.598 μs, 2.81 KB | 2.336 μs, 2.45 KB | 3.406 μs, 3.36 KB | 5.260 μs, 6.43 KB | 12.006 μs, 30.77 KB |
| 404 without exceptions | 879.5 ns, 1.75 KB | 635.4 ns, 1.36 KB | 2,623.9 ns, 3.91 KB | 3,329.0 ns, 4.89 KB | 9,167.7 ns, 30.08 KB |
| Build a request, no send | 641.8 ns, 1648 B | 127.1 ns, 392 B (hand-written) | no row | 2,738.7 ns, 5472 B (URL builder + hand-made message) | 2,300.7 ns, 3936 B (`BuildUri` + hand-made message) |
| Query string, AppendToUri | 374.5 ns, 784 B | 348.1 ns, 656 B (`QueryHelpers`) | no row | 3,327.3 ns, 6312 B | no row |
| Query string, Dictionary | 208.8 ns, 544 B | 224.3 ns, 544 B (`QueryHelpers`); 182.5 ns, 488 B (hand-written loop) | no row | 2,393.8 ns, 4728 B | no row |

Reports: [GetJson](../../benchmarks/results/http/net10.0/results/CSharpEssentials.Http.Comparison.Benchmarks.GetJsonComparison-report-github.md), [PostJson](../../benchmarks/results/http/net10.0/results/CSharpEssentials.Http.Comparison.Benchmarks.PostJsonComparison-report-github.md), [NotFound](../../benchmarks/results/http/net10.0/results/CSharpEssentials.Http.Comparison.Benchmarks.NotFoundComparison-report-github.md), [RequestBuild](../../benchmarks/results/http/net10.0/results/CSharpEssentials.Http.Comparison.Benchmarks.RequestBuildComparison-report-github.md), [QueryString](../../benchmarks/results/http/net10.0/results/CSharpEssentials.Http.Comparison.Benchmarks.QueryStringComparison-report-github.md).

The two rows where CSE's position changes by TFM: GET Count 50, where CSE is ahead of the BCL on net11.0 only (19.970 μs against 20.131 μs), and the Dictionary query string, where `QueryHelpers` is ahead of CSE on net8.0 (221.4 ns against 223.2 ns) and net9.0 (194.2 ns against 216.2 ns).

HTTP call through retry(3) and timeout(30 s), net10.0 ([report](../../benchmarks/results/http/net10.0/results/CSharpEssentials.Http.Comparison.Benchmarks.HttpResilienceComparison-report-github.md)):

| Row (net10.0) | Mean | Allocated |
|---|---|---|
| CSE `CreateResiliencePolicy` over `GetFromJsonAsResultAsync` | 2.914 μs | 2.73 KB |
| Bare Polly pipeline over `GetFromJsonAsync` | 2.451 μs | 2.27 KB |
| Microsoft.Extensions.Http.Resilience, custom handler | 3.246 μs | 2.91 KB |
| Microsoft.Extensions.Http.Resilience, standard handler | 4.052 μs | 2.95 KB |
| `IHttpClientFactory` client, no resilience (reference) | 2.050 μs | 2.54 KB |

On net8.0 the custom handler is slightly ahead of CSE (4.145 μs against 4.189 μs, ratio 0.99); on net9.0 and net11.0 it is slower, as on net10.0.

## Where CSE loses

Measured; the full list with every TFM is in [the Results section](../../CSharpEssentials.Http/BENCHMARKS.md#where-cse-loses).

- GET against the BCL. CSE is slower at Count 1 on every TFM (1.857 μs against 1.729 μs on net10.0) and at Count 50 on net8.0 to net10.0, and it allocates more on every TFM (18.36 KB against 14.97 KB at Count 50 on net10.0). CSE wraps the call in its own try/catch and an async layer (`CSharpEssentials.Http/HttpClientResultExtensions.cs:15-29`), and `GetCoreAsync` uses `GetAsync`, which buffers the whole body before deserializing (`:257`). The BCL `GetFromJsonAsync` streams the body.
- POST and 404 against the BCL: slower and more allocation on every TFM (404: 879.5 ns against 635.4 ns on net10.0). The BCL 404 row only checks the status code; CSE also builds a typed `Error`.
- Request build against a hand-written `HttpRequestMessage`: 641.8 ns against 127.1 ns and 1648 B against 392 B on net10.0. `HttpRequestBuilder` parses the URI up to three times: the template (`HttpRequestBuilder.cs:31`), the route-substituted string (`:230`), and the string with the query appended (`QueryStringExtensions.cs:218`). It also boxes the int query value through the object overload.
- Query strings: `QueryHelpers` beats CSE on AppendToUri on every TFM (348.1 ns against 374.5 ns, 656 B against 784 B on net10.0), and a hand-written `Uri.EscapeDataString` loop beats CSE on the Dictionary row on every TFM (182.5 ns against 208.8 ns, 488 B against 544 B on net10.0).
- Policy-wrapped GET against bare Polly: slower on every TFM (2.914 μs against 2.451 μs on net10.0). On net8.0 the Microsoft.Extensions.Http.Resilience custom handler is also slightly faster than CSE (4.145 μs against 4.189 μs). The CSE row wraps `GetFromJsonAsResultAsync` (buffered, own try/catch) while the Polly row wraps the streamed BCL `GetFromJsonAsync`, so the gap includes the GET gap above, not only the policy wrapper.

## Where CSE wins

- Against Refit, Flurl.Http and RestSharp, CSE is ahead in time and allocation in every shared row on every TFM. In the non-throwing 404, Refit builds an `ApiException` and RestSharp reads the body to a string and deserializes it into `User` (`RestClient.Extensions.cs:47-48` at 114.1.0): 879.5 ns for CSE against 2,623.9 ns (Refit), 3,329.0 ns (Flurl) and 9,167.7 ns (RestSharp) on net10.0.
- The Dictionary query string against `QueryHelpers` on net10.0 and net11.0 (208.8 ns against 224.3 ns on net10.0, same 544 B).
- The policy-wrapped GET against both Microsoft.Extensions.Http.Resilience handlers on net9.0 to net11.0, with less allocation on every TFM. Those handlers do more work (factory logging, telemetry, and five strategies in the standard handler), so this is not a like-for-like win.

Features only CSE ships (not a speed claim):
- A typed error for every status code, without exceptions.
- Request building without sending, with route values and enum conventions.
- The SSRF guard and the Idempotency-Key helpers.

## Caveats to keep in the post

- Sockets, TLS and the server are not measured. On a real network the per-call library overhead is a small part of the total.
- On net8 every JSON row uses the System.Text.Json 9.0.4 and System.Net.Http.Json 9.0.0 packages, because CSE.Http runs its netstandard2.1 build there.
- No CSE source-gen row yet: the `JsonTypeInfo<T>` overloads exist, but the suite does not measure them.
- The `IHttpClientFactory` rows include the factory's logging handlers and resilience telemetry.

## Possible angles

- "Result instead of exceptions, and what it costs": the 404 path is where the Result model should pay off.
- "Same HttpClient, different wrappers": the stub makes the per-library overhead visible on its own.
- "The bug the benchmark found": an earlier version did not dispose its `HttpResponseMessage`; the code measured here does.
- "Typed clients and source generation": Refit's generated path against hand-written calls.
