# CSharpEssentials.RequestResponseLogging: Performance benchmarks (draft)

This page compares CSharpEssentials.RequestResponseLogging (CSE) with the framework's HTTP logging middleware (`AddHttpLogging` / `UseHttpLogging`) and Serilog.AspNetCore 10.0.0 (`UseSerilogRequestLogging`). It covers logging request and response bodies of 1 KB, 64 KB and 1 MB, and a metadata-only event per request. The suite uses BenchmarkDotNet v0.14.0 and targets .NET 8, 9, 10 and 11.

The classes live in `benchmarks/CSharpEssentials.AspNetCore.Comparison.Benchmarks/` (suite key `aspnetcore`), next to the classes documented in [CSharpEssentials.AspNetCore/BENCHMARKS.md](../CSharpEssentials.AspNetCore/BENCHMARKS.md) and [CSharpEssentials.Idempotency/BENCHMARKS.md](../CSharpEssentials.Idempotency/BENCHMARKS.md).

**Status: draft.** The suite builds, and its parity self-check passes on every TFM. No full default-job run has been done, so this page has no numbers yet (see [Results](#results)).

| Package | Version | Published | License (nuspec expression) | Asset the suite loads |
|---|---|---|---|---|
| Serilog.AspNetCore | 10.0.0 | 2025-11-28 | Apache-2.0 | `lib/net8.0`, `lib/net9.0`, `lib/net10.0` (net11 loads `lib/net10.0`) |

HttpLogging ships in the `Microsoft.AspNetCore.App` shared framework (8.0.30, 9.0.19, 10.0.11, 11.0.0-rc.1). CSE.RequestResponseLogging targets net8-net11 natively. Serilog.AspNetCore 10 needs Microsoft.Extensions.* and System.Text.Json 10.0.0, which is why transitive pinning is off for this project. Its assembly reports `AssemblyInformationalVersion` `10.0.0-main-0ddce6f`.

---

## What was measured

| # | Class | Measured unit | Rows | Parity |
|---|---|---|---|---|
| 1 | `RequestLoggingComparison` | POST /echo, request and response JSON of exactly `BodySize` = 1 KB, 64 KB, 1 MB | No logging middleware (baseline), CSE `LoggingOptions.CreateAllFields()`, HttpLogging with method, path, query, both bodies, status and duration and 2 MiB body limits, HttpLogging `All` with the default 32 KB limits (labelled) | Status 200, the expected response bytes and one handler call on every row; one event per request for each logging row and none for the baseline; for CSE and HttpLogging_Bodies the message contains the last 32 characters of the request body and, checked separately, of the response body |
| 2 | `RequestLoggingMetadataComparison` | GET /ping, no request body, 1 KB JSON response | No logging (baseline), CSE method + path + timing with `SkipRequestResponseLogging` (labelled), Serilog request logging, HttpLogging method + path + status + duration | 200 with the same body, one event per request per logging row, none for the baseline |

- Every class has `[MemoryDiagnoser]`; every variant runs on its own Production `TestServer` host and reads the whole response.
- **Logging is on for the middleware under test, and only for it.** Each host logs through a counting provider (MEL) or sink (Serilog) that renders every message, as a real provider would, and only adds its length to a counter. MEL hosts: `ClearProviders()`, the counting provider, `SetMinimumLevel(Warning)`, then `Information` for `RequestResponseLogger` and `Microsoft.AspNetCore.HttpLogging`. The Serilog host: `MinimumLevel.Warning()` with an `Information` override for `Serilog.AspNetCore.RequestLoggingMiddleware`. The hosting "Request starting/finished" events stay below the floor on every host, the baseline included.
- This matters for fairness: with logging off, HttpLogging and Serilog skip their work on `IsEnabled == false`, while CSE builds the whole message before its own `IsEnabled` check (`LogWriters/LoggerFactoryLogWriter.cs:34` against `:41`/`:44`). A disabled-logger comparison would measure that difference instead of the logging.
- HttpLogging logs the request body only as the app reads it, so the handler reads the whole request (`CopyToAsync(Stream.Null)`) before it writes the response.

### Inputs

- Request and response are `{"data":"…"}` of exactly `BodySize` bytes, pseudo-random `[a-z0-9]` from two different fixed seeds, so parity can tell the request body from the response body in a message.
- `application/json` is in HttpLogging's default `MediaTypeOptions` on every TFM: the 1 KB `HttpLogging_Defaults` message and every `HttpLogging_Bodies` message contain both bodies, so no `AddText` was needed.

---

## Semantic differences

From the self-check output on all four TFMs (message lengths in characters):

| Rendered message | 1 KB | 64 KB | 1 MB |
|---|---|---|---|
| CSE all fields | 2205 | 131231 | 2097315 |
| HttpLogging_Bodies | 2212 | 131235-131236 | 2097316 |
| HttpLogging_Defaults | 2329 | 65841 (both bodies truncated at 32 KB) | 65843 (truncated) |

- **Full response buffering.** CSE replaces the response stream with a `RecyclableMemoryStream`, lets the endpoint write the whole response into it, decodes it to a string, then copies it to the client in 4 KB chunks. The response is not streamed. HttpLogging tees the stream: the client gets the bytes as they are written, and only the logged prefix is kept.
- **Request capture.** CSE calls `EnableBuffering`, copies the body into a `RecyclableMemoryStream`, decodes it to a string and rewinds; HttpLogging records the request as the app reads it. Above 30 KB (the 64 KB and 1 MB rows), `EnableBuffering` spills the request to a temp file under `ASPNETCORE_TEMP`/`Path.GetTempPath()`, so the CSE rows include file writes and reads; HttpLogging does none.
- **Fields.** CSE's all-fields set logs the host name, the query string, an empty header list (no `HeaderKeys` configured) and both lengths, and no status code (CSE has no status code field). HttpLogging_Bodies logs method, path, query, both bodies, status and duration. HttpLogging_Defaults (`All`) also logs request and response headers, with redaction.
- **Metadata event.** CSE: `Method: GET / Path: /ping / ResponseTiming: {mm:ss.fff}` (50 characters rendered, one placeholder per field). Serilog: `HTTP "GET" "/ping" responded 200 in {Elapsed:0.0000} ms` (45 characters rendered). HttpLogging: `Request and Response: Method: GET PathBase: Path: /ping StatusCode: 200 Duration: {ms}` (87-89 characters rendered, depending on the duration digits). Serilog and HttpLogging include the status code; CSE cannot.
- **The skip attribute is what stops capture.** CSE captures both bodies whatever `LoggingFields` says; only the skip attributes stop it (`Infrastructure/Middlewares/BaseMiddleware.cs:99-108,141-153`). The metadata CSE row therefore needs `SkipRequestResponseLogging`; without it a metadata-only field list still buffers both bodies.
- **Never compare the metadata rows with the body rows.** The Serilog row exists only in the metadata class.

---

## Feature matrix

From CSE source and the competitors' docs; not measured.

| Feature | CSE | HttpLogging (framework) | Serilog.AspNetCore 10.0.0 |
|---|---|---|---|
| Request body / response body | Yes / yes | Yes / yes, 32 KB each by default | No |
| Full response buffering | Yes, breaks streaming | No (tee) | No |
| Headers | Allowlist, case-sensitive | Allowlist with redaction | No |
| Status code | No | Yes | Yes |
| One combined event | Yes | With `CombineLogs` (net8+) | Yes |
| Per-endpoint skip | Skip attributes | `[HttpLogging]` attribute / `WithHttpLogging()` (net8+) | `GetLevel` / filter |
| Ignore paths | Prefix match (no segment boundary: `/health` also skips `/healthz`) | No | Filter |
| Size cap | Request 10 MB by `Content-Length` only, response none | Limits on both | n/a |
| Structured properties | `UseSeparateContext` | Yes | Yes |
| AOT/trim flagged | Not flagged | Yes | No (assembly not marked trimmable or AOT-compatible) |
| License | MIT | MIT | Apache-2.0 |

---

## Results

**Pending the full default-job run.** The numbers will come from the default BenchmarkDotNet job on net8.0, net9.0, net10.0 and net11.0, and the reports will be committed under `benchmarks/results/aspnetcore/{tfm}/results/`. Short-job (`--quick`) numbers are not published here.

---

## Caveats

- **Expected CSE costs, from reading the code** (not yet measured):
  - `Infrastructure/Middlewares/BaseMiddleware.cs:153-178`: the request body is copied several times (`EnableBuffering`, then a `RecyclableMemoryStream`, then a string).
  - `BaseMiddleware.cs:204-247`: the whole response is buffered, decoded to a string, then copied to the client in 4 KB chunks.
  - `Infrastructure/MessageCreators/LogMessageWithContextCreator.cs:23-32`: the message template is rebuilt per request although it depends only on the options.
  - `LogWriters/LoggerFactoryLogWriter.cs:34-38`: the `List<string?>` of values is copied to an `object[]`.
  - `LoggerFactoryLogWriter.cs:34` against `:41`/`:44`: the message is created before the `IsEnabled` check, so a disabled logger still pays for the bodies.
  - `BaseMiddleware.cs:99-108,141-153`: bodies are captured unless a skip attribute is present, whatever `LoggingFields` says.
- The 64 KB and 1 MB CSE body rows write the request to the temp directory (see Request capture), so its storage (tmpfs or disk) affects them; the run records which one backed `Path.GetTempPath()`.
- The counting provider renders the message but writes nothing, so I/O cost of a real sink is not included for any row.
- **TestServer, not Kestrel.** On a real connection, CSE's full buffering also delays the first byte of the response, which this suite does not measure.
- The CSE column is the working tree on branch `perf/bench-aspnetcore`, which may not match the version published on NuGet.

---

## How to reproduce

```bash
cd benchmarks
./run-benchmarks.sh --project aspnetcore                       # all TFMs, every class of the suite, default job
./run-benchmarks.sh --project aspnetcore --quick --tfm net10.0 # one TFM, Short job, one filter per class
dotnet run -c Release -f net10.0 --project CSharpEssentials.AspNetCore.Comparison.Benchmarks -- --self-check
```

Results are stored per framework under `benchmarks/results/aspnetcore/{tfm}/results/`.
