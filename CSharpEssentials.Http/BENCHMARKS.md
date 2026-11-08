# CSharpEssentials.Http: performance benchmarks

This page compares CSharpEssentials.Http (CSE) with plain `HttpClient` plus System.Net.Http.Json (BCL), Refit 16.4.0, Flurl.Http 4.0.2 and RestSharp 114.1.0. It covers JSON GET and POST, a 404 without exceptions, building a request without sending it, query strings, and an HTTP call through a retry and timeout policy, which is also compared with Microsoft.Extensions.Http.Resilience 10.11.0. The suite uses BenchmarkDotNet v0.14.0 and targets .NET 8, 9, 10 and 11. The same project also holds the CSharpEssentials.Resilience classes, which are documented in [CSharpEssentials.Resilience/BENCHMARKS.md](../CSharpEssentials.Resilience/BENCHMARKS.md).

The parity self-check passes on every TFM, and full default-job results for all four TFMs are in [Results](#results).

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

- **Response disposal.** Every CSE call disposes its `HttpResponseMessage` with a `using` declaration (for GET in `GetCoreAsync`, `CSharpEssentials.Http/HttpClientResultExtensions.cs:257`). An earlier version of the package did not, and this suite is where that showed up. The BCL `GetFromJsonAsync`, Refit, Flurl and RestSharp dispose it too.
- **Buffering.** CSE's `GetFromJsonAsResultAsync` uses `GetAsync` (`ResponseContentRead`, `HttpClientResultExtensions.cs:257`), so the body is buffered before it is deserialized. The BCL `GetFromJsonAsync` reads headers first and streams the body. RestSharp reads the body to a string before it deserializes.
- **CSE's POST value is typed `object`**, so System.Text.Json serializes it by its runtime type. This is the real CSE API.
- **Request Content-Type.** CSE, BCL, Refit and RestSharp send `application/json; charset=utf-8`, Flurl sends `application/json; charset=UTF-8`. Content-Type is not part of the POST parity check.
- **Flurl default options** are `{ PropertyNameCaseInsensitive = true }`, which write PascalCase. Only the labelled Defaults GET row uses them, and a read is not affected.
- **CSE default options** are the Readme's `EnhancedJsonSerializerOptions.DefaultOptions`: Web-like naming plus case-insensitive reads, relaxed escaping, ignore-nulls, IgnoreCycles and the polymorphic, enum and DateTime converter factories. Only the labelled Defaults rows use them.
- **404 paths.** The default calls of Refit (`Task<T>`, throws `ApiException`), Flurl (throws `FlurlHttpException`) and RestSharp (`GetAsync<T>`, throws) all throw on 404. The suite uses each library's non-throwing path. Refit's `IApiResponse<T>` still builds an `ApiException` object for the error, RestSharp reads the body to a string, and CSE builds an `Error` from the status code and does not read the body.
- **RestSharp 404 body.** RestSharp also deserializes the problem+json error body into `User` (`RestSerializers.DeserializeContent` attempts HTTP error bodies by design, `RestClient.Extensions.cs:47-48` at 114.1.0). CSE, the BCL and Flurl never touch the body; Refit only reads it to a string for `ApiException`.
- **Request building.** Refit builds requests only inside a generated call, so it has no row. Flurl.Http has no public build-without-send API, and RestSharp builds only the URI, so both rows are the library's URL builder plus the same hand-made `HttpRequestMessage` and header line. CSE's `WithQuery("page", 2)` boxes the int through the object overload, as in the Readme.
- **Query encoding.** CSE, `QueryHelpers`, Flurl and `Uri.EscapeDataString` all encode `c# tips` as `c%23%20tips`. A probe of `a+b/c?d&e=f ü` also gave the same text from all four: `a%2Bb%2Fc%3Fd%26e%3Df%20%C3%BC`. Refit and RestSharp have no standalone query-string builder.
- **Resilience rows.** CSE and bare Polly retry the whole call, deserialization included. CSE also retries failed `Result` values. Microsoft.Extensions.Http.Resilience retries only the send, on transient status codes and exceptions, inside an `IHttpClientFactory` pipeline that adds the factory's logging handlers and its own resilience telemetry. The standard handler runs five strategies (rate limiter, total timeout, retry, circuit breaker, attempt timeout), not two. CSE's row wraps `GetFromJsonAsResultAsync` (buffered, with its own try/catch); the Polly and factory rows wrap the BCL `GetFromJsonAsync` (streamed). The CSE-vs-Polly difference includes the GetJsonComparison Get-category gap, not only the policy wrapper.
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
| Source-generated JSON (`JsonTypeInfo`) | `JsonTypeInfo<T>` overloads (no benchmark row yet) | Yes | Yes, `ForGenerated(client, context)` | Through the options' `TypeInfoResolver` | Through the options |
| Default JSON options | Web + case-insensitive + relaxed escaping + ignore-nulls + IgnoreCycles + converter factories | Web | `GetDefaultJsonSerializerOptions()` (contents not checked) | `{ PropertyNameCaseInsensitive = true }` | Not checked |
| GET body buffering | Buffered (`ResponseContentRead`) | Streamed (`ResponseHeadersRead`) | Error bodies `ResponseHeadersRead` (XML docs); success path not checked | Not checked | Reads the body to a string |
| Disposes the response | Yes | `GetFromJsonAsync` yes | Yes | Yes | Yes |
| Redirect helper | Yes | Handler `AllowAutoRedirect` | Handler | Redirect settings | `FollowRedirects` option |
| SSRF guard | Yes (net9+) | No | No | No | No |
| Idempotency-Key helpers | Yes | No | No | No | No |
| Resilience integration | CSharpEssentials.Resilience (Polly v8) | Microsoft.Extensions.Http.Resilience | Through `IHttpClientFactory` | None built in | None built in |
| Trim/AOT assembly metadata | `IsAotCompatible` on the net9.0+ builds (the csproj sets it for every TFM compatible with net8.0); the `JsonSerializerOptions` JSON overloads and the `QueryString` object overloads are annotated `RequiresUnreferencedCode`/`RequiresDynamicCode` | Annotated | `IsTrimmable` + `IsAotCompatible` | None | None |
| License | MIT | MIT | MIT | MIT | Apache-2.0 |

---

## Results

### How these numbers were produced

- BenchmarkDotNet v0.14.0, default job (not `--quick`), full runs of every class. The four TFMs ran one after another on one Linux machine; the hardware is in each report header.
- Runtimes from the report headers: .NET 8.0.30, 9.0.19, 10.0.11 and 11.0.0 (11.0.26.42628, built with the 11.0.100-rc.1 SDK). `HttpResilienceComparison` and `RetryIfFailedComparison` were rerun on main after pull requests #230 to #256 landed, with the 11.0.100-rc.3 SDK and .NET 8.0.33, 9.0.22, 10.0.14 and 11.0.0 (11.0.26.52348).
- The 1-minute load average was 1.74 when the suite started and 1.60 when it ended. No row in this suite kept a coefficient of variation above 5%, so no class was rerun for noise and no row is marked noisy. No mean is near 0 ns, so the timer-resolution caveat does not apply.
- Reports: `benchmarks/results/http/{tfm}/results/*-report-github.md`, with `.csv` and `.html` beside them. Each table below links the four github reports for its class.
- The CSE column is the repository source as of the run, a local integration of all open pull requests.

Each cell is `Mean (Ratio), Allocated`, copied from that TFM's report. The ratio is against the category baseline: the CSE row where the category has one, `Bcl_GetSourceGen` in SourceGen, and `CSharpEssentials_Build` in RequestBuild. "-" means the report shows no allocation.

### Summary

- Against Refit, Flurl.Http and RestSharp, CSE takes less time and allocates less in every row they share, on every TFM. RestSharp allocates the most in the GET, POST and 404 rows, for example 30.63 KB against CSE's 2.55 KB for GET Count 1 on net10.0.
- Against the BCL (`HttpClient` with System.Net.Http.Json, `QueryHelpers`, or hand-written code), CSE is slower in almost every row and allocates more in every row except the Dictionary query string against `QueryHelpers`, where both allocate 544 B. The list below gives each case.
- Through a retry and timeout policy, CSE is slower than a bare Polly pipeline and faster than both Microsoft.Extensions.Http.Resilience handlers on net9.0 to net11.0. On net8.0 the custom handler is slightly ahead of CSE (4.145 μs against 4.189 μs, ratio 0.99). CSE allocates less than both handler rows on every TFM.

### Where CSE loses

Every row where CSE is slower or allocates more than another row in its category:

- **GET, Count 1, against `Bcl_Get`.** Slower on every TFM: 2.440 μs against 2.087 μs (net8.0), 2.223 μs against 1.996 μs (net9.0), 1.857 μs against 1.729 μs (net10.0), 1.817 μs against 1.581 μs (net11.0). Allocates 2.54 KB against 2.18 KB (net8.0, net9.0) and 2.55 KB against 2.2 KB (net10.0, net11.0).
- **GET, Count 50, against `Bcl_Get`.** Slower on net8.0 (24.493 μs against 23.480 μs), net9.0 (23.869 μs against 22.743 μs) and net10.0 (21.337 μs against 21.141 μs). On net11.0 CSE is ahead, 19.970 μs against 20.131 μs. Allocates more on every TFM: 18.35 KB against 14.95 KB (net8.0, net9.0), 18.36 KB against 14.97 KB (net10.0), 18.36 KB against 14.98 KB (net11.0). CSE buffers the whole body before it deserializes; the BCL streams it.
- **POST against `Bcl_Post`.** CSE against the BCL: 3.400 μs against 2.864 μs (net8.0, BCL ratio 0.84), 3.041 μs against 2.668 μs (net9.0, 0.88), 2.598 μs against 2.336 μs (net10.0, 0.90), 2.496 μs against 2.213 μs (net11.0, 0.89). CSE allocates 2.79 KB against 2.42 KB (net8.0, net9.0), 2.81 KB against 2.45 KB (net10.0) and 2.8 KB against 2.45 KB (net11.0).
- **404 against `Bcl_NotFound`.** 1,172.5 ns against 781.8 ns (net8.0), 1,130.8 ns against 811.1 ns (net9.0), 879.5 ns against 635.4 ns (net10.0), 839.4 ns against 618.8 ns (net11.0). Allocated 1.81 KB against 1.35 KB (net8.0), 1.74 KB against 1.35 KB (net9.0), 1.75 KB against 1.36 KB (net10.0), 1.75 KB against 1.37 KB (net11.0). The BCL row only checks the status code; CSE also builds a typed `Error`.
- **Request build against the hand-written `Bcl_Build`.** 733.4 ns against 189.1 ns (net8.0), 722.7 ns against 161.4 ns (net9.0), 641.8 ns against 127.1 ns (net10.0), 537.55 ns against 99.34 ns (net11.0). Allocated 1648 B against 392 B (1656 B against 400 B on net11.0). The builder parses the URI up to three times and boxes the int query value.
- **Query string, AppendToUri, against `AspNetCore_AppendToUri` (`QueryHelpers`).** 398.6 ns against 337.9 ns (net8.0), 380.6 ns against 331.2 ns (net9.0), 374.5 ns against 348.1 ns (net10.0), 323.3 ns against 287.1 ns (net11.0). Allocated 784 B against 656 B on every TFM.
- **Query string, Dictionary, against the hand-written `Bcl_Dictionary` loop.** Ratio 0.83 (net8.0), 0.90 (net9.0), 0.87 (net10.0), 0.94 (net11.0) for the loop, and 488 B against 544 B on every TFM. `QueryHelpers` is also ahead on net8.0 (221.4 ns against 223.2 ns, ratio 0.99) and net9.0 (194.2 ns against 216.2 ns); CSE is ahead on net10.0 and net11.0. Both allocate 544 B.
- **HTTP resilience against `Polly_Pipeline`.** 4.189 μs against 3.256 μs (net8.0), 3.406 μs against 2.779 μs (net9.0), 2.914 μs against 2.451 μs (net10.0), 2.764 μs against 2.362 μs (net11.0). Allocated 2.73 KB against 2.26 KB (net8.0, net9.0), 2.27 KB (net10.0) and 2.28 KB (net11.0). This gap includes the GET gap above, because the Polly row wraps the streamed BCL call. `Factory_NoResilience` is a reference row with no retry and is faster than every policy row. On net8.0 the `MicrosoftResilience_Handler` row is also faster, 4.145 μs against 4.189 μs (ratio 0.99), while it allocates more (2.79 KB against 2.73 KB).

### Full tables

### GetJsonComparison

| Method | Categories | Count | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|---|
| `CSharpEssentials_GetDefaultOptions` | Defaults | 1 | 2.427 μs (1.00), 2.54 KB | 2.157 μs (1.00), 2.54 KB | 1.864 μs (1.00), 2.55 KB | 1.869 μs (1.00), 2.55 KB |
| `Flurl_GetDefaultOptions` | Defaults | 1 | 4.499 μs (1.85), 4.98 KB | 4.286 μs (1.99), 4.98 KB | 3.602 μs (1.93), 4.99 KB | 3.617 μs (1.94), 5.14 KB |
| `CSharpEssentials_GetDefaultOptions` | Defaults | 50 | 24.859 μs (1.00), 18.35 KB | 23.845 μs (1.00), 18.35 KB | 21.560 μs (1.00), 18.36 KB | 19.743 μs (1.00), 18.36 KB |
| `Flurl_GetDefaultOptions` | Defaults | 50 | 28.521 μs (1.15), 20.8 KB | 26.340 μs (1.10), 20.8 KB | 24.200 μs (1.12), 20.8 KB | 22.282 μs (1.13), 20.95 KB |
| `CSharpEssentials_Get` | Get | 1 | 2.440 μs (1.00), 2.54 KB | 2.223 μs (1.00), 2.54 KB | 1.857 μs (1.00), 2.55 KB | 1.817 μs (1.00), 2.55 KB |
| `Bcl_Get` | Get | 1 | 2.087 μs (0.86), 2.18 KB | 1.996 μs (0.90), 2.18 KB | 1.729 μs (0.93), 2.2 KB | 1.581 μs (0.87), 2.2 KB |
| `Refit_Get` | Get | 1 | 3.158 μs (1.29), 2.82 KB | 2.775 μs (1.25), 2.82 KB | 2.499 μs (1.35), 2.83 KB | 2.322 μs (1.28), 2.84 KB |
| `Flurl_Get` | Get | 1 | 4.644 μs (1.90), 4.98 KB | 4.218 μs (1.90), 4.98 KB | 3.626 μs (1.95), 4.99 KB | 3.482 μs (1.92), 5.14 KB |
| `RestSharp_Get` | Get | 1 | 12.726 μs (5.22), 31.02 KB | 12.480 μs (5.61), 30.89 KB | 10.081 μs (5.43), 30.63 KB | 9.439 μs (5.20), 30.61 KB |
| `CSharpEssentials_Get` | Get | 50 | 24.493 μs (1.00), 18.35 KB | 23.869 μs (1.00), 18.35 KB | 21.337 μs (1.00), 18.36 KB | 19.970 μs (1.00), 18.36 KB |
| `Bcl_Get` | Get | 50 | 23.480 μs (0.96), 14.95 KB | 22.743 μs (0.95), 14.95 KB | 21.141 μs (0.99), 14.97 KB | 20.131 μs (1.01), 14.98 KB |
| `Refit_Get` | Get | 50 | 27.264 μs (1.11), 18.63 KB | 24.633 μs (1.03), 18.63 KB | 23.694 μs (1.11), 18.64 KB | 20.538 μs (1.03), 18.65 KB |
| `Flurl_Get` | Get | 50 | 28.135 μs (1.15), 20.8 KB | 26.214 μs (1.10), 20.8 KB | 23.392 μs (1.10), 20.8 KB | 22.258 μs (1.11), 20.95 KB |
| `RestSharp_Get` | Get | 50 | 38.083 μs (1.55), 66.84 KB | 36.440 μs (1.53), 66.72 KB | 33.767 μs (1.58), 66.46 KB | 30.833 μs (1.54), 66.44 KB |
| `Bcl_GetSourceGen` | SourceGen | 1 | 2.083 μs (1.00), 2.19 KB | 1.953 μs (1.00), 2.19 KB | 1.741 μs (1.00), 2.2 KB | 1.625 μs (1.00), 2.21 KB |
| `Refit_GetSourceGen` | SourceGen | 1 | 3.217 μs (1.54), 2.83 KB | 2.953 μs (1.51), 2.83 KB | 2.523 μs (1.45), 2.84 KB | 2.404 μs (1.48), 2.84 KB |
| `Bcl_GetSourceGen` | SourceGen | 50 | 27.310 μs (1.00), 15.34 KB | 25.926 μs (1.00), 15.34 KB | 22.711 μs (1.00), 15.36 KB | 21.548 μs (1.00), 15.37 KB |
| `Refit_GetSourceGen` | SourceGen | 50 | 27.409 μs (1.00), 19.02 KB | 26.727 μs (1.03), 19.02 KB | 23.819 μs (1.05), 19.03 KB | 21.918 μs (1.02), 19.04 KB |

Reports: [net8.0](../benchmarks/results/http/net8.0/results/CSharpEssentials.Http.Comparison.Benchmarks.GetJsonComparison-report-github.md), [net9.0](../benchmarks/results/http/net9.0/results/CSharpEssentials.Http.Comparison.Benchmarks.GetJsonComparison-report-github.md), [net10.0](../benchmarks/results/http/net10.0/results/CSharpEssentials.Http.Comparison.Benchmarks.GetJsonComparison-report-github.md), [net11.0](../benchmarks/results/http/net11.0/results/CSharpEssentials.Http.Comparison.Benchmarks.GetJsonComparison-report-github.md).

### PostJsonComparison

| Method | Categories | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|
| `CSharpEssentials_PostDefaultOptions` | Defaults | 3.264 μs (1.00), 2.9 KB | 3.092 μs (1.00), 2.9 KB | 2.642 μs (1.00), 2.92 KB | 2.528 μs (1.00), 2.91 KB |
| `CSharpEssentials_Post` | Post | 3.400 μs (1.00), 2.79 KB | 3.041 μs (1.00), 2.79 KB | 2.598 μs (1.00), 2.81 KB | 2.496 μs (1.00), 2.8 KB |
| `Bcl_Post` | Post | 2.864 μs (0.84), 2.42 KB | 2.668 μs (0.88), 2.42 KB | 2.336 μs (0.90), 2.45 KB | 2.213 μs (0.89), 2.45 KB |
| `Refit_Post` | Post | 4.264 μs (1.25), 3.34 KB | 3.851 μs (1.27), 3.34 KB | 3.406 μs (1.31), 3.36 KB | 3.184 μs (1.28), 3.36 KB |
| `Flurl_Post` | Post | 6.884 μs (2.03), 6.58 KB | 6.301 μs (2.07), 6.42 KB | 5.260 μs (2.02), 6.43 KB | 5.121 μs (2.05), 6.56 KB |
| `RestSharp_Post` | Post | 15.156 μs (4.46), 31.38 KB | 14.542 μs (4.78), 31.12 KB | 12.006 μs (4.62), 30.77 KB | 10.878 μs (4.36), 30.75 KB |

Reports: [net8.0](../benchmarks/results/http/net8.0/results/CSharpEssentials.Http.Comparison.Benchmarks.PostJsonComparison-report-github.md), [net9.0](../benchmarks/results/http/net9.0/results/CSharpEssentials.Http.Comparison.Benchmarks.PostJsonComparison-report-github.md), [net10.0](../benchmarks/results/http/net10.0/results/CSharpEssentials.Http.Comparison.Benchmarks.PostJsonComparison-report-github.md), [net11.0](../benchmarks/results/http/net11.0/results/CSharpEssentials.Http.Comparison.Benchmarks.PostJsonComparison-report-github.md).

### NotFoundComparison

| Method | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_NotFound` | 1,172.5 ns (1.00), 1.81 KB | 1,130.8 ns (1.00), 1.74 KB | 879.5 ns (1.00), 1.75 KB | 839.4 ns (1.00), 1.75 KB |
| `Bcl_NotFound` | 781.8 ns (0.67), 1.35 KB | 811.1 ns (0.72), 1.35 KB | 635.4 ns (0.72), 1.36 KB | 618.8 ns (0.74), 1.37 KB |
| `Refit_NotFound` | 3,512.0 ns (3.00), 3.92 KB | 3,133.2 ns (2.77), 3.91 KB | 2,623.9 ns (2.98), 3.91 KB | 2,673.4 ns (3.18), 3.92 KB |
| `Flurl_NotFound` | 4,263.0 ns (3.64), 4.88 KB | 4,145.6 ns (3.67), 4.88 KB | 3,329.0 ns (3.78), 4.89 KB | 3,743.6 ns (4.46), 5.04 KB |
| `RestSharp_NotFound` | 12,188.0 ns (10.39), 30.47 KB | 11,622.2 ns (10.28), 30.34 KB | 9,167.7 ns (10.42), 30.08 KB | 9,246.7 ns (11.02), 30.06 KB |

Reports: [net8.0](../benchmarks/results/http/net8.0/results/CSharpEssentials.Http.Comparison.Benchmarks.NotFoundComparison-report-github.md), [net9.0](../benchmarks/results/http/net9.0/results/CSharpEssentials.Http.Comparison.Benchmarks.NotFoundComparison-report-github.md), [net10.0](../benchmarks/results/http/net10.0/results/CSharpEssentials.Http.Comparison.Benchmarks.NotFoundComparison-report-github.md), [net11.0](../benchmarks/results/http/net11.0/results/CSharpEssentials.Http.Comparison.Benchmarks.NotFoundComparison-report-github.md).

### RequestBuildComparison

| Method | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_Build` | 733.4 ns (1.00), 1648 B | 722.7 ns (1.00), 1648 B | 641.8 ns (1.00), 1648 B | 537.55 ns (1.00), 1656 B |
| `Bcl_Build` | 189.1 ns (0.26), 392 B | 161.4 ns (0.22), 392 B | 127.1 ns (0.20), 392 B | 99.34 ns (0.18), 400 B |
| `Flurl_Build` | 3,413.9 ns (4.66), 5624 B | 3,518.5 ns (4.87), 5624 B | 2,738.7 ns (4.27), 5472 B | 2,512.40 ns (4.67), 5504 B |
| `RestSharp_Build` | 2,981.3 ns (4.07), 4376 B | 2,873.0 ns (3.98), 4224 B | 2,300.7 ns (3.58), 3936 B | 2,098.22 ns (3.90), 3920 B |

Reports: [net8.0](../benchmarks/results/http/net8.0/results/CSharpEssentials.Http.Comparison.Benchmarks.RequestBuildComparison-report-github.md), [net9.0](../benchmarks/results/http/net9.0/results/CSharpEssentials.Http.Comparison.Benchmarks.RequestBuildComparison-report-github.md), [net10.0](../benchmarks/results/http/net10.0/results/CSharpEssentials.Http.Comparison.Benchmarks.RequestBuildComparison-report-github.md), [net11.0](../benchmarks/results/http/net11.0/results/CSharpEssentials.Http.Comparison.Benchmarks.RequestBuildComparison-report-github.md).

### QueryStringComparison

| Method | Categories | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|
| `CSharpEssentials_AppendToUri` | AppendToUri | 398.6 ns (1.00), 784 B | 380.6 ns (1.00), 784 B | 374.5 ns (1.00), 784 B | 323.3 ns (1.00), 784 B |
| `AspNetCore_AppendToUri` | AppendToUri | 337.9 ns (0.85), 656 B | 331.2 ns (0.87), 656 B | 348.1 ns (0.93), 656 B | 287.1 ns (0.89), 656 B |
| `Flurl_AppendToUri` | AppendToUri | 4,427.0 ns (11.11), 6600 B | 4,323.0 ns (11.36), 6600 B | 3,327.3 ns (8.88), 6312 B | 3,150.6 ns (9.75), 6448 B |
| `CSharpEssentials_Dictionary` | Dictionary | 223.2 ns (1.00), 544 B | 216.2 ns (1.00), 544 B | 208.8 ns (1.00), 544 B | 183.1 ns (1.00), 544 B |
| `AspNetCore_QueryHelpers` | Dictionary | 221.4 ns (0.99), 544 B | 194.2 ns (0.90), 544 B | 224.3 ns (1.07), 544 B | 199.8 ns (1.09), 544 B |
| `Flurl_Dictionary` | Dictionary | 3,084.1 ns (13.82), 5016 B | 3,195.2 ns (14.78), 5016 B | 2,393.8 ns (11.47), 4728 B | 2,223.0 ns (12.14), 4696 B |
| `Bcl_Dictionary` | Dictionary | 184.8 ns (0.83), 488 B | 194.4 ns (0.90), 488 B | 182.5 ns (0.87), 488 B | 172.1 ns (0.94), 488 B |
| `CSharpEssentials_Object` | Object | 478.6 ns (1.00), 808 B | 460.7 ns (1.00), 808 B | 448.7 ns (1.00), 808 B | 363.8 ns (1.00), 808 B |
| `Flurl_Object` | Object | 3,258.2 ns (6.81), 5488 B | 3,165.2 ns (6.87), 5488 B | 2,624.2 ns (5.85), 5200 B | 2,289.6 ns (6.29), 5432 B |

Reports: [net8.0](../benchmarks/results/http/net8.0/results/CSharpEssentials.Http.Comparison.Benchmarks.QueryStringComparison-report-github.md), [net9.0](../benchmarks/results/http/net9.0/results/CSharpEssentials.Http.Comparison.Benchmarks.QueryStringComparison-report-github.md), [net10.0](../benchmarks/results/http/net10.0/results/CSharpEssentials.Http.Comparison.Benchmarks.QueryStringComparison-report-github.md), [net11.0](../benchmarks/results/http/net11.0/results/CSharpEssentials.Http.Comparison.Benchmarks.QueryStringComparison-report-github.md).

### HttpResilienceComparison

| Method | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_Policy` | 4.189 μs (1.00), 2.73 KB | 3.406 μs (1.00), 2.73 KB | 2.914 μs (1.00), 2.73 KB | 2.764 μs (1.00), 2.73 KB |
| `Polly_Pipeline` | 3.256 μs (0.78), 2.26 KB | 2.779 μs (0.82), 2.26 KB | 2.451 μs (0.84), 2.27 KB | 2.362 μs (0.85), 2.28 KB |
| `MicrosoftResilience_Handler` | 4.145 μs (0.99), 2.79 KB | 3.784 μs (1.11), 2.9 KB | 3.246 μs (1.11), 2.91 KB | 3.146 μs (1.14), 2.92 KB |
| `MicrosoftResilience_Standard` | 5.172 μs (1.23), 2.83 KB | 4.756 μs (1.40), 2.94 KB | 4.052 μs (1.39), 2.95 KB | 3.931 μs (1.42), 2.96 KB |
| `Factory_NoResilience` | 2.421 μs (0.58), 2.41 KB | 2.377 μs (0.70), 2.52 KB | 2.050 μs (0.70), 2.54 KB | 1.919 μs (0.69), 2.55 KB |

Reports: [net8.0](../benchmarks/results/http/net8.0/results/CSharpEssentials.Http.Comparison.Benchmarks.HttpResilienceComparison-report-github.md), [net9.0](../benchmarks/results/http/net9.0/results/CSharpEssentials.Http.Comparison.Benchmarks.HttpResilienceComparison-report-github.md), [net10.0](../benchmarks/results/http/net10.0/results/CSharpEssentials.Http.Comparison.Benchmarks.HttpResilienceComparison-report-github.md), [net11.0](../benchmarks/results/http/net11.0/results/CSharpEssentials.Http.Comparison.Benchmarks.HttpResilienceComparison-report-github.md).

---

## Caveats

- **A real connection is not measured.** The stub removes sockets, TLS and the server. A row's number is the library's own overhead around `HttpClient.SendAsync` plus JSON work. On a real network that overhead is a small part of the total.
- **No CSE source-gen row.** CSharpEssentials.Http has `JsonTypeInfo<T>` overloads, but the suite has no CSE row in the SourceGen category yet, so `Bcl_GetSourceGen` is that category's baseline.
- On net8.0 every JSON row runs the System.Text.Json 9.0.4 and System.Net.Http.Json 9.0.0 packages, not the in-box STJ 8. Numbers for net8 are not numbers for STJ 8.
- The `IHttpClientFactory` rows include the factory's logging handlers and Microsoft.Extensions.Http.Resilience's telemetry. Their difference from the bare Polly row is not the cost of retry alone.
- The CSE column is the repository source at the time of the run (a local integration of all open pull requests), which may not match the version published on NuGet.
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
