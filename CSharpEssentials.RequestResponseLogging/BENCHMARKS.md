# CSharpEssentials.RequestResponseLogging: performance benchmarks

This page compares CSharpEssentials.RequestResponseLogging (CSE) with the framework's HTTP logging middleware (`AddHttpLogging` / `UseHttpLogging`) and Serilog.AspNetCore 10.0.0 (`UseSerilogRequestLogging`). It covers logging request and response bodies of 1 KB, 64 KB and 1 MB, and a metadata-only event per request. The suite uses BenchmarkDotNet v0.14.0 and targets .NET 8, 9, 10 and 11.

The classes live in `benchmarks/CSharpEssentials.AspNetCore.Comparison.Benchmarks/` (suite key `aspnetcore`), next to the classes documented in [CSharpEssentials.AspNetCore/BENCHMARKS.md](../CSharpEssentials.AspNetCore/BENCHMARKS.md) and [CSharpEssentials.Idempotency/BENCHMARKS.md](../CSharpEssentials.Idempotency/BENCHMARKS.md).

The parity self-check passes on every TFM, and full default-job results for net8.0 to net11.0 are in [Results](#results).

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
- This matters for fairness: with logging off, HttpLogging and Serilog skip their work on `IsEnabled == false`. CSE checks `IsEnabled` before it builds the message (`LogWriters/LoggerFactoryLogWriter.cs:49`), but its middleware has already captured both bodies by then. A disabled-logger comparison would measure that difference instead of the logging.
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

- **Response capture.** CSE wraps the response stream in a `ResponseCaptureStream` that writes each chunk to the client and to a `RecyclableMemoryStream`, then decodes the whole copy to a string after the endpoint finishes. The client gets the bytes as they are written; the copy has no size cap. HttpLogging also tees the stream, and keeps only the logged prefix up to its limit.
- **Request capture.** CSE calls `EnableBuffering` with a memory threshold of the `Content-Length` clamped to 30 KB to 1 MB, reads the body through a `StreamReader` into a string and rewinds; HttpLogging records the request as the app reads it. The 64 KB request stays in memory. The 1 MB request is at the 1 MB threshold; a larger one would go to a temporary file under `ASPNETCORE_TEMP`/`Path.GetTempPath()`.
- **Fields.** CSE's all-fields set logs the host name, the query string, an empty header list (no `HeaderKeys` configured) and both lengths, and no status code (CSE has no status code field). HttpLogging_Bodies logs method, path, query, both bodies, status and duration. HttpLogging_Defaults (`All`) also logs request and response headers, with redaction.
- **Metadata event.** CSE: `Method: GET / Path: /ping / ResponseTiming: {mm:ss.fff}` (50 characters rendered, one placeholder per field). Serilog: `HTTP "GET" "/ping" responded 200 in {Elapsed:0.0000} ms` (45 characters rendered). HttpLogging: `Request and Response: Method: GET PathBase: Path: /ping StatusCode: 200 Duration: {ms}` (87-89 characters rendered, depending on the duration digits). Serilog and HttpLogging include the status code; CSE has no status code field.
- **What stops capture.** With no handler, CSE captures the request body only when `LoggingFields` has `Request` or `RequestLength`, and the response body only when it has `Response` or `ResponseLength`; a skip attribute stops capture in every case (`Infrastructure/Middlewares/BaseMiddleware.cs:104-106,137-150`). The metadata CSE row (method, path, timing) would skip both bodies without `SkipRequestResponseLogging` too; the suite keeps the attribute on that row.
- **Never compare the metadata rows with the body rows.** The Serilog row exists only in the metadata class.

---

## Feature matrix

From CSE source and the competitors' docs; not measured.

| Feature | CSE | HttpLogging (framework) | Serilog.AspNetCore 10.0.0 |
|---|---|---|---|
| Request body / response body | Yes / yes | Yes / yes, 32 KB each by default | No |
| Response capture | Tee plus a full in-memory copy, no size cap | Tee, logged prefix only | No |
| Headers | Allowlist, case-sensitive | Allowlist with redaction | No |
| Status code | No | Yes | Yes |
| One combined event | Yes | With `CombineLogs` (net8+) | Yes |
| Per-endpoint skip | Skip attributes | `[HttpLogging]` attribute / `WithHttpLogging()` (net8+) | `GetLevel` / filter |
| Ignore paths | Prefix match (no segment boundary: `/health` also skips `/healthz`) | No | Filter |
| Size cap | Request 10 MB (by `Content-Length` and while reading), response none | Limits on both | n/a |
| Structured properties | `UseSeparateContext` | Yes | Yes |
| AOT/trim flagged | Not flagged | Yes | No (assembly not marked trimmable or AOT-compatible) |
| License | MIT | MIT | Apache-2.0 |

---

## Results

### How these numbers were produced

- BenchmarkDotNet v0.14.0, default job (full runs, not `--quick`), `[MemoryDiagnoser]` on both classes. SDK 11.0.100-rc.1; runtimes .NET 8.0.30, 9.0.19, 10.0.11 and 11.0.0 (11.0.26.42628), as the report headers print them, each TFM in its own run. A range with no TFM named spans net8.0 to net11.0 (net9.0 to net11.0 for the `CSharpEssentials.Idempotency.Benchmarks` rows). Differences and multiples in the prose are computed from the means and allocations in the tables.
- One Linux x64 machine, 6 physical cores and 12 threads (the exact CPU is in each report header). The suites ran one after another, never in parallel.
- The 1-minute load average was 1.60 when the `aspnetcore` suite started and 2.02 when it ended (5-minute average 1.70 to 2.65). Classes with a row above 5% CV were rerun, up to twice. The `RequestLoggingComparison` report for net9.0 comes from a rerun at a 1-minute load of 2.48 at its start and 2.35 at its end. After the reruns, no row on this page is above 5% CV.
- Reports: `benchmarks/results/aspnetcore/{tfm}/results/*-report-github.md` (also `.csv` and `.html`), for example [net10.0](../benchmarks/results/aspnetcore/net10.0/results/).

Each cell is `Mean (Ratio), Allocated`, copied from the github report of that TFM. The ratio is against `Plain_NoLogging` with the same `BodySize`.

### Request and response bodies

| Method | BodySize | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|
| `Plain_NoLogging` | 1024 | 11.74 μs (1.00), 10.59 KB | 11.49 μs (1.00), 10.54 KB | 10.47 μs (1.00), 10.42 KB | 14.12 μs (1.00), 12.38 KB |
| `CSharpEssentials_AllFields` | 1024 | 25.19 μs (2.15), 40.94 KB | 22.03 μs (1.92), 40.9 KB | 21.02 μs (2.01), 40.71 KB | 28.26 μs (2.00), 42.12 KB |
| `HttpLogging_Bodies` | 1024 | 18.11 μs (1.54), 25.06 KB | 17.46 μs (1.52), 25.04 KB | 15.90 μs (1.52), 24.91 KB | 23.79 μs (1.69), 21.58 KB |
| `HttpLogging_Defaults` | 1024 | 18.84 μs (1.61), 25.44 KB | 18.08 μs (1.57), 25.38 KB | 16.40 μs (1.57), 25.26 KB | 25.20 μs (1.79), 21.94 KB |
| `Plain_NoLogging` | 65536 | 41.66 μs (1.00), 78.36 KB | 40.32 μs (1.00), 78.31 KB | 40.64 μs (1.00), 78.21 KB | 50.34 μs (1.00), 81.24 KB |
| `CSharpEssentials_AllFields` | 65536 | 243.45 μs (5.84), 893.59 KB | 225.18 μs (5.58), 893.48 KB | 218.85 μs (5.39), 893.38 KB | 231.60 μs (4.60), 895.63 KB |
| `HttpLogging_Bodies` | 65536 | 277.95 μs (6.67), 1093.28 KB | 286.47 μs (7.10), 1093.26 KB | 202.13 μs (4.98), 1093.47 KB | 270.41 μs (5.37), 1095.7 KB |
| `HttpLogging_Defaults` | 65536 | 120.83 μs (2.90), 579.47 KB | 121.87 μs (3.02), 579.35 KB | 127.65 μs (3.14), 579.17 KB | 134.19 μs (2.67), 581.71 KB |
| `Plain_NoLogging` | 1048576 | 508.93 μs (1.00), 1988.69 KB | 642.03 μs (1.00), 1988.13 KB | 501.71 μs (1.00), 1092.22 KB | 506.68 μs (1.00), 1096.41 KB |
| `CSharpEssentials_AllFields` | 1048576 | 3,414.21 μs (6.71), 14343.37 KB | 3,603.47 μs (5.61), 14343.09 KB | 3,517.92 μs (7.01), 13447.32 KB | 3,701.84 μs (7.31), 13451.19 KB |
| `HttpLogging_Bodies` | 1048576 | 3,632.79 μs (7.14), 19941.43 KB | 4,339.94 μs (6.76), 19942.6 KB | 4,680.96 μs (9.33), 19045.95 KB | 4,953.94 μs (9.78), 19051.26 KB |
| `HttpLogging_Defaults` | 1048576 | 643.18 μs (1.26), 2488.74 KB | 658.34 μs (1.03), 2489.06 KB | 770.80 μs (1.54), 1592.46 KB | 542.46 μs (1.07), 1596.75 KB |

- 1 KB: CSE costs 1.92 to 2.15 of the plain request and is slower than `HttpLogging_Bodies` on every TFM, with more allocated (see below).
- 64 KB: CSE is faster than `HttpLogging_Bodies` on net8.0, net9.0 and net11.0 and slower on net10.0 (218.85 against 202.13 μs). It allocates less on every TFM (893.38 KB against 1093.47 KB on net10.0).
- 1 MB: CSE is faster than `HttpLogging_Bodies` and allocates less on every TFM. On net10.0: 3,517.92 μs and 13447.32 KB against 4,680.96 μs and 19045.95 KB. Both still cost 5.61 to 9.78 times the plain request, because both put two 1 MB bodies into one log message.
- `HttpLogging_Defaults` stops each body at 32 KB, so from 64 KB on it does less work than the other two rows (643.18 μs at 1 MB on net8.0). It is there to show the default, not as a like-for-like row.

### Metadata only

| Method | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `Plain_NoLogging` | 9.774 μs (1.00), 9.71 KB | 9.461 μs (1.00), 9.66 KB | 8.817 μs (1.00), 9.53 KB | 12.30 μs (1.00), 11.51 KB |
| `CSharpEssentials_SkipBodies` | 10.813 μs (1.11), 10.51 KB | 10.300 μs (1.09), 10.43 KB | 10.010 μs (1.14), 10.24 KB | 12.86 μs (1.05), 12.17 KB |
| `Serilog_RequestLogging` | 13.151 μs (1.35), 12.51 KB | 12.484 μs (1.32), 12.39 KB | 11.871 μs (1.35), 12.17 KB | 16.89 μs (1.37), 13.82 KB |
| `HttpLogging_Metadata` | 10.878 μs (1.11), 10.36 KB | 10.336 μs (1.09), 10.31 KB | 9.795 μs (1.11), 10.19 KB | 13.22 μs (1.08), 12.14 KB |

- CSE (method, path, timing, both bodies skipped) costs 1.05 to 1.14 of the plain request, HttpLogging 1.08 to 1.11 and Serilog 1.32 to 1.37. CSE is faster than Serilog on every TFM and allocates less.
- Against HttpLogging, CSE is faster on net8.0, net9.0 and net11.0 and slower on net10.0. CSE allocates slightly more on every TFM.
- HttpLogging and Serilog log the status code; CSE has no status code field, so its event carries less.

### Where CSE loses

- **1 KB bodies, all TFMs: slower than `HttpLogging_Bodies` and allocates more.** 25.19 against 18.11 μs and 40.94 against 25.06 KB (net8.0); 22.03 against 17.46 μs and 40.9 against 25.04 KB (net9.0); 21.02 against 15.90 μs and 40.71 against 24.91 KB (net10.0); 28.26 against 23.79 μs and 42.12 against 21.58 KB (net11.0).
- **64 KB bodies, net10.0: slower than `HttpLogging_Bodies`.** 218.85 against 202.13 μs.
- **`HttpLogging_Defaults`, all sizes and TFMs: faster and allocates less than CSE.** At 1 KB, where it logs both whole bodies plus headers: 18.84 against 25.19 μs and 25.44 against 40.94 KB (net8.0), 18.08 against 22.03 μs and 25.38 against 40.9 KB (net9.0), 16.40 against 21.02 μs and 25.26 against 40.71 KB (net10.0), 25.20 against 28.26 μs and 21.94 against 42.12 KB (net11.0). At 64 KB and 1 MB it truncates each body at 32 KB, so it does less work (net10.0: 127.65 against 218.85 μs at 64 KB, 770.80 against 3,517.92 μs at 1 MB).
- **Metadata, all TFMs: allocates more than HttpLogging.** 10.51 against 10.36 KB (net8.0), 10.43 against 10.31 KB (net9.0), 10.24 against 10.19 KB (net10.0), 12.17 against 12.14 KB (net11.0). On net10.0 it is also slower: 10.010 against 9.795 μs.

---

## Caveats

- **CSE code notes**, from the worktree source:
  - `Infrastructure/Middlewares/BaseMiddleware.cs:152-200`: the request body is buffered by `EnableBuffering`, then read through a `StreamReader` into a `StringBuilder` and turned into a string, so a logged request exists as buffered bytes, builder chunks and a final string.
  - `BaseMiddleware.cs:220-270`: the response is teed into a `RecyclableMemoryStream` with no size cap and decoded to one string after the endpoint finishes.
  - `Infrastructure/MessageCreators/LogMessageWithContextCreator.cs` builds the template once per options; `LogMessageCreator.cs` (used when `UseSeparateContext` is off) formats a new message string per request, which is its job. `LoggingOptions.CreateAllFields()` turns `UseSeparateContext` on.
  - `LogWriters/LoggerFactoryLogWriter.cs:49` checks `IsEnabled` before it builds the message, but the middleware has already captured the bodies by then.
- The counting provider renders the message but writes nothing, so I/O cost of a real sink is not included for any row.
- **TestServer, not Kestrel.** Sockets, TLS and HTTP parsing are not measured.
- The CSE column is the working tree on branch `integ/all` (all open pull requests merged locally), which may not match the version published on NuGet.

---

## How to reproduce

```bash
cd benchmarks
./run-benchmarks.sh --project aspnetcore                       # all TFMs, every class of the suite, default job
./run-benchmarks.sh --project aspnetcore --quick --tfm net10.0 # one TFM, Short job, one filter per class
dotnet run -c Release -f net10.0 --project CSharpEssentials.AspNetCore.Comparison.Benchmarks -- --self-check
```

Results are stored per framework under `benchmarks/results/aspnetcore/{tfm}/results/`.
