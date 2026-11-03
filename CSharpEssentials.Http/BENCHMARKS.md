# CSharpEssentials.Http: Performance benchmarks (draft)

This page compares CSharpEssentials.Http (CSE) with plain `HttpClient` plus System.Net.Http.Json (BCL), Refit 16.4.0, Flurl.Http 4.0.2 and RestSharp 114.1.0. It covers JSON GET and POST, a 404 without exceptions, building a request without sending it, query strings, and an HTTP call through a retry and timeout policy, which is also compared with Microsoft.Extensions.Http.Resilience 10.11.0. The suite uses BenchmarkDotNet v0.14.0 and targets .NET 8, 9, 10 and 11. The same project also holds the CSharpEssentials.Resilience classes, which are documented in [CSharpEssentials.Resilience/BENCHMARKS.md](../CSharpEssentials.Resilience/BENCHMARKS.md).

**Status: draft.** The suite builds, and its parity self-check passes on every TFM. No full default-job run has been done, so this page has no numbers yet (see [Results](#results)).

The competitors are pinned exactly in `Directory.Packages.props`. Each pin is the latest stable release on NuGet as of 2026-11-03:

| Package | Version | Published | License (nuspec expression) | Asset the suite loads |
|---|---|---|---|---|
| Refit | 16.4.0 | 2026-10-17 | MIT | `lib/net8.0` to `lib/net11.0`, matching the TFM |
| Flurl.Http | 4.0.2 | 2024-01-17 | MIT | `lib/net6.0` on every TFM. It brings Flurl 4.0.0 (MIT, `lib/netstandard2.0`) |
| RestSharp | 114.1.0 | 2026-10-21 | Apache-2.0 | `lib/net8.0`, `lib/net9.0`, `lib/net10.0` (net11 loads `lib/net10.0`) |
| Microsoft.Extensions.Http.Resilience | 10.11.0 | 2026-10-14 | MIT | `lib/net8.0`, `lib/net9.0`, `lib/net10.0` |
| Polly.Core | 8.9.0 | 2026-10-28 | BSD-3-Clause | `lib/net8.0` on net8 and net9, `lib/net10.0` on net10 and net11 |

Refit pulls in ReactiveUI.Primitives 9.0.0 (with ReactiveUI.Primitives.Core and ReactiveUI.Disposables 9.0.0, all MIT) and System.Net.ServerSentEvents 10.0.13 (MIT).

`QueryHelpers` comes from the `Microsoft.AspNetCore.App` shared framework, which the project references on every TFM. The NuGet package Microsoft.AspNetCore.WebUtilities is not used, because it ships only for net10.0.

The repo pins Polly.Core to `[8.0.0,9.0.0)`, which resolves to 8.0.0. For this suite only, `Directory.Packages.props` pins it to 8.9.0: Microsoft.Extensions.Http.Resilience needs 8.4.2 or later, and every CSE row and Polly row then runs the same engine. The CSE packages themselves still restore 8.0.0 everywhere else. Transitive pinning is off for this project, as it is for the DI, mediator and endpoints suites: the host project BenchmarkDotNet generates fails restore with NU1109, because Microsoft.Extensions.Http.Resilience needs Microsoft.Extensions.* 9.0.21 or later.

What each TFM runs (`--self-check` prints this as its first line):

| TFM | System.Text.Json | System.Net.Http.Json | Polly.Core | CSE build |
|---|---|---|---|---|
| net8.0 | 9.0.4 (NuGet) | 9.0.0 (NuGet) | 8.9.0 | netstandard2.1 |
| net9.0 | 9.0.19 (framework) | 9.0.19 (framework) | 8.9.0 | net9.0 |
| net10.0 | 10.0.11 (framework) | 10.0.11 (framework) | 8.9.0 | net10.0 |
| net11.0 | 11.0.0-rc.1 (framework) | 11.0.0-rc.1 (framework) | 8.9.0 | net11.0 |

CSE.Http has no net8 target, so net8 runs its netstandard2.1 build. That build references the System.Net.Http.Json package, and every JSON row on net8, the BCL rows included, uses that package. The SSRF guard, which is absent from the netstandard2.1 build, is not used in this suite.

---

## What was measured

The code is in `benchmarks/CSharpEssentials.Http.Comparison.Benchmarks/`, in six BenchmarkDotNet classes for this package.
- Every class has `[MemoryDiagnoser]`.
- CSE is the baseline in every category that has a CSE row.
- Each class checks parity in `[GlobalSetup]`, and `--self-check` runs the same check. Every row in a category must give the same result for the same input, or the run fails.
- Each library and BCL row has a comment above it that links to the documentation it follows.
- No network is used. Every row sends through one `HttpClient` over an in-memory `StubHandler`, so the cost of `HttpClient.SendAsync` is the same for all of them (see [Inputs](#inputs)).

| # | Class | Measured unit | Rows | Parity |
|---|---|---|---|---|
| 1 | `GetJsonComparison` | GET /users and deserialize `User[]`, `Count` = 1 and 50 | Get: CSE `GetFromJsonAsResultAsync`, BCL `GetFromJsonAsync`, Refit `Task<User[]>`, Flurl `GetJsonAsync`, RestSharp `GetAsync<T>`, all with the shared Web options. Defaults (labelled): CSE and Flurl with their default options. SourceGen (labelled): BCL `JsonTypeInfo` and Refit `ForGenerated(client, context)` | Length plus the first and last user |
| 2 | `PostJsonComparison` | POST /users with a `NewUser`, read the 201 `User` | Post: CSE `PostAsJsonAsResultAsync`, BCL `PostAsJsonAsync` + `ReadFromJsonAsync`, Refit `[Body]`, Flurl `PostJsonAsync().ReceiveJson`, RestSharp `PostJsonAsync<TReq, TResp>`. Defaults (labelled): CSE with its default options | Method, absolute URI and body bytes as the stub received them, and the decoded user |
| 3 | `NotFoundComparison` | GET /missing answers 404 with problem+json | Each library's documented non-throwing path: CSE `Result`, BCL status check, Refit `IApiResponse<T>`, Flurl `AllowHttpStatus(404)`, RestSharp `ExecuteGetAsync<T>`, which also deserializes the error body into `User` | 404 on every row; CSE's error code is `Http.404` with type NotFound |
| 4 | `RequestBuildComparison` | Build `GET https://bench.local/users/42?include=profile&page=2` with `Accept: application/json`, no send | CSE `HttpRequestBuilder`, BCL hand-written `HttpRequestMessage` (reference), Flurl `Url` builder plus a hand-made message (labelled), RestSharp `BuildUri` plus a hand-made message (labelled) | Method, absolute URI and Accept header |
| 5 | `QueryStringComparison` | `q = "c# tips"`, `page = 2`, `include = "profile"` | Object: CSE `ToQueryString()`, Flurl `SetQueryParams(obj)`. Dictionary: CSE, ASP.NET Core `QueryHelpers.AddQueryString`, Flurl, a hand-written `Uri.EscapeDataString` loop (reference). AppendToUri: CSE `WithQueryString`, `QueryHelpers`, Flurl `Url` | The exact string `Q=c%23%20tips&Page=2&Include=profile`, or the absolute URI |
| 6 | `HttpResilienceComparison` | GET /users (one user) through retry(3, zero delay) then timeout(30 s), success path | CSE `CreateResiliencePolicy` over `GetFromJsonAsResultAsync`, bare Polly pipeline over `GetFromJsonAsync`, Microsoft.Extensions.Http.Resilience custom handler and standard handler (labelled), an `IHttpClientFactory` client with no resilience handler (reference) | The same `User[]` on every row |

### Inputs

- `User` is a sealed record `(int Id, string Name, string Email, int Age)` with ASCII strings only and no nulls, enums or dates. CSE's default options then write and read the same bytes as plain Web options, which is why the Defaults rows can be checked against the same expected value.
- `Inputs.WebOptions` is one `new JsonSerializerOptions(JsonSerializerDefaults.Web)` instance. Every like-for-like row gets it through its library's documented hook: the CSE and BCL `options` parameters, Refit `new RefitSettings(new SystemTextJsonContentSerializer(options))`, Flurl `DefaultJsonSerializer(options)` and RestSharp `UseSystemTextJson(options)`.
- The GET bodies are serialized once per `Count`. User `i` is `(i, "user{i}", "user{i}@bench.local", 20 + i % 50)`.
- The stub drains every request body into `Stream.Null` (or a `MemoryStream` while the parity check captures it). Without that, the lazily serialized bodies of CSE, the BCL and Refit would never be written, while Flurl and RestSharp serialize up front.
- The stub returns a new `HttpResponseMessage` over a `ByteArrayContent` per call, with `Content-Type: application/json; charset=utf-8` (problem+json for the 404).
- One `HttpClient(stub, disposeHandler: false)` with `BaseAddress = https://bench.local/` is shared by every row in a class. Refit wraps it with `ForGenerated`, Flurl with `new FlurlClient(client)` and RestSharp with `new RestClient(client, disposeHttpClient: false, ...)`.
- The shared client starts with `Timeout = InfiniteTimeSpan`. Flurl sets the timeout of a client it wraps to infinite (the self-check caught this), so without this every row would silently inherit Flurl's change. The setup checks that no library changed the client's headers, timeout or base address. With an infinite client timeout, the CSE, BCL and Refit rows allocate no `CancellationTokenSource`. Flurl inherits `Timeout = InfiniteTimeSpan` and links a CTS per call, which also makes `HttpClient` link one. RestSharp always creates a 100 s CTS and a linked CTS. Both are library defaults.
- Refit runs its source-generated client. `RestService.ForGenerated` with the shared Web options worked without a custom `TypeInfoResolver`.

---

## Semantic differences

The parity checks use inputs on which every row agrees. The libraries still differ in ways the numbers alone do not show. These were seen in the self-check output on net8 and net10 and are not timed.

- **CSE never disposes the `HttpResponseMessage`** (`CSharpEssentials.Http/HttpClientResultExtensions.cs:19,36,51,66,81,97,111,123,137`). With the stub this shows up only as GC pressure. With a real socket, an undisposed response whose body was not fully read keeps the pooled connection busy. The BCL `GetFromJsonAsync`, Refit, Flurl and RestSharp dispose it.
- **Buffering.** CSE's `GetFromJsonAsResultAsync` uses `GetAsync` (`ResponseContentRead`), so the body is buffered before it is deserialized. The BCL `GetFromJsonAsync` reads headers first and streams the body. RestSharp reads the body to a string before it deserializes.
- **CSE's POST value is typed `object`**, so System.Text.Json serializes it by its runtime type. This is the real CSE API.
- **Request Content-Type.** CSE, BCL, Refit and RestSharp send `application/json; charset=utf-8`, Flurl sends `application/json; charset=UTF-8`. Content-Type is not part of the POST parity check.
- **Flurl default options** are `{ PropertyNameCaseInsensitive = true }`, which write PascalCase. Only the labelled Defaults GET row uses them, and a read is not affected.
- **CSE default options** are the Readme's `EnhancedJsonSerializerOptions.DefaultOptions`: Web-like naming plus case-insensitive reads, relaxed escaping, ignore-nulls, IgnoreCycles and the polymorphic, enum and DateTime converter factories. Only the labelled Defaults rows use them.
- **404 paths.** The default calls of Refit (`Task<T>`, throws `ApiException`), Flurl (throws `FlurlHttpException`) and RestSharp (`GetAsync<T>`, throws) all throw on 404. The suite uses each library's non-throwing path. Refit's `IApiResponse<T>` still builds an `ApiException` object for the error, RestSharp reads the body to a string, and CSE builds an `Error` from the status code and does not read the body.
- **RestSharp 404 body.** RestSharp also deserializes the problem+json error body into `User` (`RestSerializers.DeserializeContent` attempts HTTP error bodies by design, `RestClient.Extensions.cs:47-48` at 114.1.0). CSE, the BCL and Flurl never touch the body; Refit only reads it to a string for `ApiException`.
- **Request building.** Refit builds requests only inside a generated call, so it has no row. Flurl.Http has no public build-without-send API, and RestSharp builds only the URI, so both rows are the library's URL builder plus the same hand-made `HttpRequestMessage` and header line. CSE's `WithQuery("page", 2)` boxes the int through the object overload, as in the Readme.
- **Query encoding.** CSE, `QueryHelpers`, Flurl and `Uri.EscapeDataString` all encode `c# tips` as `c%23%20tips`. A probe of `a+b/c?d&e=f ü` also gave the same text from all four: `a%2Bb%2Fc%3Fd%26e%3Df%20%C3%BC`. Refit and RestSharp have no standalone query-string builder.
- **Resilience rows.** CSE and bare Polly retry the whole call, deserialization included. CSE also retries failed `Result` values. Microsoft.Extensions.Http.Resilience retries only the send, on transient status codes and exceptions, inside an `IHttpClientFactory` pipeline that adds the factory's logging handlers and its own resilience telemetry. The standard handler runs five strategies (rate limiter, total timeout, retry, circuit breaker, attempt timeout), not two. CSE's row wraps `GetFromJsonAsResultAsync` (buffered, `Result.TryAsync`); the Polly and factory rows wrap the BCL `GetFromJsonAsync` (streamed). The CSE-vs-Polly difference includes the GetJsonComparison Get-category gap, not only the policy wrapper.
- **Circuit breaker defaults** differ inside CSE: `minimumThroughput` defaults to 5 in the Http helpers (`HttpClientResilienceExtensions.cs:22,25,85,88`) and to 10 in CSharpEssentials.Resilience. No row depends on it.

---

## Feature matrix

This table comes from reading CSE source, the competitors' XML docs, nuspecs and assembly metadata, plus the self-check probes noted above. None of it was measured. The competitor cells for response disposal, redirects and Refit's query parameters come from their public docs and were not re-checked against source.

| Feature | CSE | BCL HttpClient + System.Net.Http.Json | Refit 16.4.0 | Flurl.Http 4.0.2 | RestSharp 114.1.0 |
|---|---|---|---|---|---|
| Non-2xx without exceptions | Yes, `Result` | Manual `GetAsync` + status check | `IApiResponse<T>` (`Task<T>` throws `ApiException`) | `AllowHttpStatus` (the default throws) | `Execute*` (`GetAsync<T>` throws) |
| Status code to typed error | Built-in table | No | No | No | No |
| Typed interface client | No | No | Yes, source generator | No | No |
| Fluent request builder | `HttpRequestBuilder` | No | Attributes | Yes | `RestRequest` |
| Build a request without sending | `Build()` | `new HttpRequestMessage` | No | No (URL only) | `BuildUri` (URI only) |
| Query string from an object | Reflection | No | `[Query]` object parameter | Reflection | Reflection (`AddObject<T>`, or `GetAsync<T>(resource, object)`) |
| Query string from a dictionary | Yes | No (`QueryHelpers` in ASP.NET Core) | `IDictionary` parameter | Yes | `AddQueryParameter` per entry |
| Source-generated JSON (`JsonTypeInfo`) | Not on this branch | Yes | Yes, `ForGenerated(client, context)` | Through the options' `TypeInfoResolver` | Through the options |
| Default JSON options | Web + case-insensitive + relaxed escaping + ignore-nulls + IgnoreCycles + converter factories | Web | `GetDefaultJsonSerializerOptions()` (contents not checked) | `{ PropertyNameCaseInsensitive = true }` | Not checked |
| GET body buffering | Buffered (`ResponseContentRead`) | Streamed (`ResponseHeadersRead`) | Error bodies `ResponseHeadersRead` (XML docs); success path not checked | Not checked | Reads the body to a string |
| Disposes the response | No (bug, see above) | `GetFromJsonAsync` yes | Yes | Yes | Yes |
| Redirect helper | Yes | Handler `AllowAutoRedirect` | Handler | Redirect settings | `FollowRedirects` option |
| SSRF guard | Yes (net9+) | No | No | No | No |
| Idempotency-Key helpers | Yes | No | No | No | No |
| Resilience integration | CSharpEssentials.Resilience (Polly v8) | Microsoft.Extensions.Http.Resilience | Through `IHttpClientFactory` | None built in | None built in |
| Trim/AOT assembly metadata | None (`QueryString` object overloads are annotated `RequiresUnreferencedCode`/`RequiresDynamicCode` on net7+) | Annotated | `IsTrimmable` + `IsAotCompatible` | None | None |
| License | MIT | MIT | MIT | MIT | Apache-2.0 |

---

## Results

**Pending.** No full run has been done yet. The numbers will come from the default BenchmarkDotNet job on net8.0, net9.0, net10.0 and net11.0, and the reports will be committed under `benchmarks/results/http/{tfm}/results/`. Short-job (`--quick`) numbers are not published here.

---

## Caveats

- **A real connection is not measured.** The stub removes sockets, TLS and the server. A row's number is the library's own overhead around `HttpClient.SendAsync` plus JSON work. On a real network that overhead is a small part of the total.
- **The undisposed response** costs CSE nothing extra with the stub, but on a real `SocketsHttpHandler` it can hold a connection.
- **No CSE source-gen row.** This branch has no `JsonTypeInfo` overloads in CSharpEssentials.Http. A pending change adds them, and the SourceGen category should get a CSE row then.
- On net8.0 every JSON row runs the System.Text.Json 9.0.4 and System.Net.Http.Json 9.0.0 packages, not the in-box STJ 8. Numbers for net8 are not numbers for STJ 8.
- The `IHttpClientFactory` rows include the factory's logging handlers and Microsoft.Extensions.Http.Resilience's telemetry. Their difference from the bare Polly row is not the cost of retry alone.
- The CSE column is the working tree on branch `perf/bench-http`, which may not match the version published on NuGet.
- The feature matrix comes from reading source and docs. Cells marked "Not checked" were not confirmed.

---

## How to reproduce

```bash
cd benchmarks
./run-benchmarks.sh --project http                       # all TFMs (net8-net11), Http and Resilience classes, default job
./run-benchmarks.sh --project http --tfm net10.0         # one TFM
./run-benchmarks.sh --project http --quick --tfm net10.0 # one TFM, Short job, one filter per class
dotnet run -c Release -f net10.0 --project CSharpEssentials.Http.Comparison.Benchmarks -- --self-check
```

Without `--tfm`, `--quick` runs on net9.0, the script default. Short-job numbers cannot be compared with default-job numbers. Results are stored per framework under `benchmarks/results/http/{tfm}/results/`.
