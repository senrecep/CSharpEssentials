# Blog notes: CSharpEssentials.Http against HttpClient, Refit, Flurl.Http and RestSharp (draft)

These are notes for a post, not the post. The numbers will come from `CSharpEssentials.Http/BENCHMARKS.md` once the full default-job run is committed under `benchmarks/results/http/{tfm}/results/`. There are no numbers yet.

The suite runs on .NET 8, 9, 10 and 11 with BenchmarkDotNet 0.14.0. The libraries compared are `HttpClient` with System.Net.Http.Json, Refit 16.4.0 (MIT), Flurl.Http 4.0.2 (MIT), RestSharp 114.1.0 (Apache-2.0) and, for the resilience row, Polly.Core 8.9.0 (BSD-3-Clause) and Microsoft.Extensions.Http.Resilience 10.11.0 (MIT).

## Framing

- Working title: "What does a Result-returning HttpClient wrapper cost?"
- CSE.Http is not a typed client like Refit, and not a fluent HTTP DSL like Flurl or RestSharp. It is a set of `HttpClient` extensions that return `Result<T>` and map status codes to typed errors. The post should say that first, so the comparison is not read as "which REST library is fastest".
- The question the numbers answer: on top of the same `HttpClient`, what does each library add per call, in time and in allocations, for a GET, a POST, a 404, a request build and a query string?

## What each library provides

- **HttpClient + System.Net.Http.Json:** `GetFromJsonAsync`, `PostAsJsonAsync`, `ReadFromJsonAsync`, `JsonTypeInfo` overloads for source-generated JSON. Errors are exceptions or a manual status check.
- **Refit 16.4.0:** typed interfaces with attributes, implemented by a source generator (`RestService.ForGenerated`). `IApiResponse<T>` for calls that must not throw; `ForGenerated(client, context)` for source-generated JSON. Assembly metadata says trimmable and AOT compatible.
- **Flurl.Http 4.0.2:** a fluent URL and request builder (`Request().GetJsonAsync()`), `AllowHttpStatus` to opt out of exceptions, its own timeout handling (it sets the wrapped `HttpClient.Timeout` to infinite).
- **RestSharp 114.1.0:** `RestClient` and `RestRequest` with URL segments and query parameters, pluggable serializers, `Execute*` methods that return a response instead of throwing.
- **CSE:** `GetFromJsonAsResultAsync` and friends, a status-code-to-`ErrorType` table, `HttpRequestBuilder` (route values, query, headers, JSON body, idempotency key, redirects), query-string helpers, Polly resilience through CSharpEssentials.Resilience, an SSRF guard on net9+.

## Feature matrix

| Feature | CSE | BCL | Refit 16.4.0 | Flurl.Http 4.0.2 | RestSharp 114.1.0 |
|---|---|---|---|---|---|
| Non-2xx without exceptions | `Result` | Manual status check | `IApiResponse<T>` | `AllowHttpStatus` | `Execute*` |
| Status code to typed error | Built in | No | No | No | No |
| Typed interface client | No | No | Yes | No | No |
| Build a request without sending | `Build()` | `new HttpRequestMessage` | No | URL only | URI only |
| Query string from an object | Reflection | No | `[Query]` | Reflection | Reflection (`AddObject<T>`, or `GetAsync<T>(resource, object)`) |
| Source-generated JSON | Not on this branch | Yes | Yes | Through options | Through options |
| Disposes the response | No (bug) | Yes | Yes | Yes | Yes |
| SSRF guard | Yes (net9+) | No | No | No | No |
| Trim/AOT metadata | None | Annotated | Trimmable + AOT compatible | None | None |

## Methodology

- Six BenchmarkDotNet classes for this package in `benchmarks/CSharpEssentials.Http.Comparison.Benchmarks/`, each with `[MemoryDiagnoser]`. CSE is the baseline in every category that has a CSE row.
- No network. One `HttpClient` over an in-memory handler is shared by every row, so `HttpClient.SendAsync` costs the same for all. The handler drains every request body (lazy and eager serializers do the same work) and returns a new response per call.
- One shared Web `JsonSerializerOptions` instance reaches every like-for-like row through that library's documented hook. Default-options rows and source-generated rows are separate, labelled categories.
- Every class checks parity in `[GlobalSetup]`, and `--self-check` runs the same checks without BenchmarkDotNet: same request bytes, same URI, same decoded value, same status. The setup also checks that no library changed the shared client. That check caught Flurl setting the timeout to infinite; the shared client now starts that way for every row. With an infinite client timeout, the CSE, BCL and Refit rows allocate no `CancellationTokenSource`. Flurl inherits `Timeout = InfiniteTimeSpan` and links a CTS per call, which also makes `HttpClient` link one. RestSharp always creates a 100 s CTS and a linked CTS. Both are library defaults.
- Rows that do different work are labelled, not compared as if they were the same: the 404 paths, the request-build rows for Flurl and RestSharp (URL builder plus a hand-made message), and the Microsoft.Extensions.Http.Resilience rows (retry around the send only, factory logging and telemetry on).
- Every competitor row follows its documented usage and carries a link to that documentation.

## The numbers that matter

Pending the full run. These are the rows to pull:

1. GetJson Get at Count 1 and 50: CSE against BCL, Refit, Flurl, RestSharp; allocated bytes matter as much as time.
2. PostJson Post: the eager serializers (Flurl, RestSharp) against the lazy ones.
3. NotFound: what a non-throwing 404 costs in each library.
4. RequestBuild: CSE's builder against a hand-written message.
5. QueryString AppendToUri: CSE's `UriBuilder` round trip against `QueryHelpers` and Flurl.
6. HttpResilience: CSE's policy against bare Polly and the two Microsoft.Extensions.Http.Resilience handlers.

## Where CSE is expected to lose (state it, do not soften it)

These predictions come from reading the code. Nothing has been measured yet.

- **GET against the BCL.** CSE wraps the call in `Result.TryAsync` with a capturing async lambda (`CSharpEssentials.Http/HttpClientResultExtensions.cs:16-23`), uses `GetAsync`, which buffers the whole body (`:19`), and then deserializes (`:155`). The BCL `GetFromJsonAsync` streams the body and has no extra layer.
- **The undisposed response.** No CSE method disposes its `HttpResponseMessage` (`HttpClientResultExtensions.cs:19,36,51,66,81,97,111,123,137`). Against the stub that is only GC pressure; on a real connection it is the more serious problem.
- **Request build.** `HttpRequestBuilder.Build()` parses the URI three times: the template (`HttpRequestBuilder.cs:30`), the route-substituted string (`:212`) and the `UriBuilder` round trip that appends the query (`QueryStringExtensions.cs:193-198`). It also boxes the int query value through the object overload.
- **Policy-wrapped GET.** CSE adds a closure and an async layer per call on top of Polly (`CSharpEssentials.Resilience/ResiliencePolicy.cs:103`). The CSE row also wraps `GetFromJsonAsResultAsync` (buffered, `Result.TryAsync`) while the Polly row wraps the streamed BCL `GetFromJsonAsync`, so the delta includes the GET gap above, not only the wrapper.

## Where CSE may win or tie

To be confirmed by the run:
- The 404 path, where CSE builds an `Error` and reads no body, while Refit builds an `ApiException` and RestSharp reads the body to a string and deserializes it into `User` (`RestClient.Extensions.cs:47-48` at 114.1.0).
- Query strings from a dictionary, where CSE is a single `StringBuilder` pass, the same as the hand-written reference.

Features only CSE ships (not a speed claim):
- A typed error for every status code, without exceptions.
- Request building without sending, with route values and enum conventions.
- The SSRF guard and the Idempotency-Key helpers.

## Caveats to keep in the post

- Sockets, TLS and the server are not measured. On a real network the per-call library overhead is a small part of the total.
- On net8 every JSON row uses the System.Text.Json 9.0.4 and System.Net.Http.Json 9.0.0 packages, because CSE.Http runs its netstandard2.1 build there.
- No CSE source-gen row yet: the `JsonTypeInfo` overloads are a pending change.
- The `IHttpClientFactory` rows include the factory's logging handlers and resilience telemetry.

## Possible angles

- "Result instead of exceptions, and what it costs": the 404 path is where the Result model should pay off.
- "Same HttpClient, different wrappers": the stub makes the per-library overhead visible on its own.
- "The bug the benchmark found": the undisposed `HttpResponseMessage`.
- "Typed clients and source generation": Refit's generated path against hand-written calls.
