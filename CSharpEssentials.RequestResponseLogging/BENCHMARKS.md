# CSharpEssentials.RequestResponseLogging: performance benchmarks

This page compares CSharpEssentials.RequestResponseLogging (CSE) with the framework's HTTP logging middleware (`AddHttpLogging` / `UseHttpLogging`) and Serilog.AspNetCore 10.0.0 (`UseSerilogRequestLogging`). It covers logging request and response bodies of 1 KB, 64 KB and 1 MB, and a metadata-only event per request. The suite uses BenchmarkDotNet v0.14.0 and targets .NET 8, 9, 10 and 11.

The classes live in `benchmarks/CSharpEssentials.AspNetCore.Comparison.Benchmarks/` (suite key `aspnetcore`), next to the classes documented in [CSharpEssentials.AspNetCore/BENCHMARKS.md](../CSharpEssentials.AspNetCore/BENCHMARKS.md) and [CSharpEssentials.Idempotency/BENCHMARKS.md](../CSharpEssentials.Idempotency/BENCHMARKS.md).

The parity self-check passes on every TFM.

| Package | Version | Published | License (nuspec expression) | Asset the suite loads |
|---|---|---|---|---|
| Serilog.AspNetCore | 10.0.0 | 2025-11-28 | Apache-2.0 | `lib/net8.0`, `lib/net9.0`, `lib/net10.0` (net11 loads `lib/net10.0`) |

HttpLogging ships in the `Microsoft.AspNetCore.App` shared framework (8.0.33, 9.0.22, 10.0.14 and 11.0.0 (11.0.26.52348), as the report headers print the runtimes). CSE.RequestResponseLogging targets net8-net11 natively. Serilog.AspNetCore 10 needs Microsoft.Extensions.* and System.Text.Json 10.0.0, which is why transitive pinning is off for this project. Its assembly reports `AssemblyInformationalVersion` `10.0.0-main-0ddce6f`.

---

## What was measured

| # | Class | Measured unit | Rows | Parity |
|---|---|---|---|---|
| 1 | `RequestLoggingComparison` | POST /echo, request and response JSON of exactly `BodySize` = 1 KB, 64 KB, 1 MB | No logging middleware (baseline), CSE `LoggingOptions.CreateAllFields()`, HttpLogging with method, path, query, both bodies, status and duration and 2 MiB body limits, HttpLogging `All` with the default 32 KB limits (labelled) | Status 200, the expected response bytes and one handler call on every row; one event per request for each logging row and none for the baseline; for CSE and HttpLogging_Bodies the message contains the last 32 characters of the request body and, checked separately, of the response body |
| 2 | `RequestLoggingMetadataComparison` | GET /ping, no request body, 1 KB JSON response | No logging (baseline), CSE method + path + timing with `SkipRequestResponseLogging` (labelled), Serilog request logging, HttpLogging method + path + status + duration | 200 with the same body, one event per request per logging row, none for the baseline |

- Every class has `[MemoryDiagnoser]`; every variant runs on its own Production `TestServer` host and reads the whole response.
- Logging is on for the middleware under test, and only for it: each host logs through a counting provider (MEL) or sink (Serilog) that renders every message, as a real provider would, and only adds its length to a counter. MEL hosts: `ClearProviders()`, the counting provider, `SetMinimumLevel(Warning)`, then `Information` for `RequestResponseLogger` and `Microsoft.AspNetCore.HttpLogging`. The Serilog host: `MinimumLevel.Warning()` with an `Information` override for `Serilog.AspNetCore.RequestLoggingMiddleware`. The hosting "Request starting/finished" events stay below the floor on every host, the baseline included.
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

- Response capture: CSE wraps the response stream in a `ResponseCaptureStream` that writes each chunk to the client and to a `RecyclableMemoryStream`, then decodes the whole copy to a string after the endpoint finishes. The client gets the bytes as they are written; the copy has no size cap. HttpLogging also tees the stream, and keeps only the logged prefix up to its limit.
- Request capture: CSE calls `EnableBuffering` with a memory threshold of the `Content-Length` clamped to 30 KB to 1 MB, reads the body in 4 KB chunks into a pooled `RecyclableMemoryStream`, decodes the UTF-8 text straight from its buffers and rewinds; HttpLogging records the request as the app reads it. The 64 KB request stays in memory. The 1 MB request is at the 1 MB threshold; a larger one would go to a temporary file under `ASPNETCORE_TEMP`/`Path.GetTempPath()`.
- Fields: CSE's all-fields set logs the host name, the query string, an empty header list (no `HeaderKeys` configured) and both lengths, and no status code (CSE has no status code field). HttpLogging_Bodies logs method, path, query, both bodies, status and duration. HttpLogging_Defaults (`All`) also logs request and response headers, with redaction.
- Metadata event: CSE: `Method: GET / Path: /ping / ResponseTiming: {mm:ss.fff}` (50 characters rendered, one placeholder per field). Serilog: `HTTP "GET" "/ping" responded 200 in {Elapsed:0.0000} ms` (45 characters rendered). HttpLogging: `Request and Response: Method: GET PathBase: Path: /ping StatusCode: 200 Duration: {ms}` (87-89 characters rendered, depending on the duration digits). Serilog and HttpLogging include the status code; CSE has no status code field.
- What stops capture: with no handler, CSE captures the request body only when `LoggingFields` has `Request` or `RequestLength`, and the response body only when it has `Response` or `ResponseLength`; a skip attribute stops capture in every case (`Infrastructure/Middlewares/BaseMiddleware.cs:105-106,137-149`). The metadata CSE row (method, path, timing) would skip both bodies without `SkipRequestResponseLogging` too; the suite keeps the attribute on that row.
- Never compare the metadata rows with the body rows: the Serilog row exists only in the metadata class.

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

- BenchmarkDotNet v0.14.0, default job (full runs, not `--quick`), `[MemoryDiagnoser]` on both classes. Both classes were rerun after the performance changes in #230-#256: SDK 11.0.100-rc.3; runtimes .NET 8.0.33, 9.0.22, 10.0.14 and 11.0.0 (11.0.26.52348). Versions are as the report headers print them, each TFM in its own run. A range with no TFM named spans net8.0 to net11.0. Differences and multiples in the prose are computed from the means and allocations in the tables.
- One machine, a 6-core, 12-thread x64 desktop CPU on Linux. The suites ran one after another, never in parallel.
- `RequestLoggingMetadataComparison` rerun: the load average was not recorded; no row is above 5% CV.
- `RequestLoggingComparison` rerun: one row is above 5% CV (`HttpLogging_Defaults` at 1 KB on net8.0, 5.4%) and is marked noisy.
- Reports: `benchmarks/results/aspnetcore/{tfm}/results/*-report-github.md` (also `.csv` and `.html`), for example [net10.0](../benchmarks/results/aspnetcore/net10.0/results/).

Each cell is `Mean (Ratio), Allocated`, copied from the github report of that TFM. The ratio is against `Plain_NoLogging` with the same `BodySize`.

### Request and response bodies

| Method | BodySize | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|
| `Plain_NoLogging` | 1024 | 11.68 μs (1.00), 10.59 KB | 10.36 μs (1.00), 10.54 KB | 9.318 μs (1.00), 10.41 KB | 10.72 μs (1.00), 13.18 KB |
| `CSharpEssentials_AllFields` | 1024 | 21.49 μs (1.84), 21.36 KB | 18.81 μs (1.82), 21.28 KB | 16.685 μs (1.79), 21.08 KB | 16.55 μs (1.54), 23.8 KB |
| `HttpLogging_Bodies` | 1024 | 17.60 μs (1.51), 25.05 KB | 15.33 μs (1.48), 24.98 KB | 13.661 μs (1.47), 24.87 KB | 15.91 μs (1.48), 22.8 KB |
| `HttpLogging_Defaults` | 1024 | 18.65 μs (1.60), 25.45 KB, noisy (CV 5.4%) | 16.26 μs (1.57), 25.34 KB | 13.993 μs (1.50), 25.22 KB | 16.16 μs (1.51), 23.08 KB |
| `Plain_NoLogging` | 65536 | 41.21 μs (1.00), 78.36 KB | 37.97 μs (1.00), 78.31 KB | 36.807 μs (1.00), 78.19 KB | 39.10 μs (1.00), 80.78 KB |
| `CSharpEssentials_AllFields` | 65536 | 151.19 μs (3.67), 594.68 KB | 170.47 μs (4.50), 594.63 KB | 165.691 μs (4.50), 594.44 KB | 139.79 μs (3.58), 597.57 KB |
| `HttpLogging_Bodies` | 65536 | 286.98 μs (6.97), 1093.32 KB | 248.86 μs (6.56), 1093.46 KB | 223.724 μs (6.08), 1093.17 KB | 242.72 μs (6.21), 1096.5 KB |
| `HttpLogging_Defaults` | 65536 | 117.17 μs (2.84), 579.37 KB | 109.90 μs (2.90), 579.4 KB | 106.896 μs (2.90), 579.28 KB | 111.84 μs (2.86), 582.17 KB |
| `Plain_NoLogging` | 1048576 | 571.48 μs (1.00), 1988.26 KB | 564.31 μs (1.00), 1988.22 KB | 432.244 μs (1.00), 1092.15 KB | 425.52 μs (1.00), 1097.42 KB |
| `CSharpEssentials_AllFields` | 1048576 | 2,283.50 μs (4.00), 10185.53 KB | 2,651.67 μs (4.70), 10185.52 KB | 2,107.251 μs (4.88), 9289.16 KB | 2,284.02 μs (5.37), 9294.03 KB |
| `HttpLogging_Bodies` | 1048576 | 3,280.59 μs (5.74), 19941.05 KB | 4,624.49 μs (8.20), 19941.45 KB | 4,256.239 μs (9.85), 19045.49 KB | 4,628.40 μs (10.88), 19050.27 KB |
| `HttpLogging_Defaults` | 1048576 | 589.77 μs (1.03), 2489.09 KB | 688.92 μs (1.22), 2488.41 KB | 637.724 μs (1.48), 1592.31 KB | 625.47 μs (1.47), 1597.19 KB |

- 1 KB: CSE costs 1.54 to 1.84 of the plain request and is slower than `HttpLogging_Bodies` on every TFM (see below). It allocates less than `HttpLogging_Bodies` on net8.0 to net10.0 (21.08 against 24.87 KB on net10.0) and more on net11.0.
- 64 KB: CSE is faster than `HttpLogging_Bodies` and allocates less on every TFM (net10.0: 165.691 μs and 594.44 KB against 223.724 μs and 1093.17 KB).
- 1 MB: CSE is faster than `HttpLogging_Bodies` and allocates less on every TFM. On net10.0: 2,107.251 μs and 9289.16 KB against 4,256.239 μs and 19045.49 KB. Both still cost 4.00 to 10.88 times the plain request, because both put two 1 MB bodies into one log message.
- `HttpLogging_Defaults` stops each body at 32 KB, so from 64 KB on it does less work than the other two rows (589.77 μs at 1 MB on net8.0). It is there to show the default, not as a like-for-like row.

### Metadata only

| Method | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `Plain_NoLogging` | 9.152 μs (1.00), 9.71 KB | 8.558 μs (1.00), 9.66 KB | 8.255 μs (1.00), 9.53 KB | 10.69 μs (1.00), 12.33 KB |
| `CSharpEssentials_SkipBodies` | 9.977 μs (1.09), 10.49 KB | 9.483 μs (1.11), 10.43 KB | 8.724 μs (1.06), 10.22 KB | 11.47 μs (1.07), 13.06 KB |
| `Serilog_RequestLogging` | 12.052 μs (1.32), 12.49 KB | 11.090 μs (1.30), 12.41 KB | 10.866 μs (1.32), 12.22 KB | 12.56 μs (1.18), 15 KB |
| `HttpLogging_Metadata` | 9.752 μs (1.07), 10.36 KB | 9.472 μs (1.11), 10.3 KB | 8.918 μs (1.08), 10.17 KB | 11.78 μs (1.10), 13.05 KB |

- CSE (method, path, timing, both bodies skipped) costs 1.06 to 1.11 of the plain request, HttpLogging 1.07 to 1.11 and Serilog 1.18 to 1.32. CSE is faster than Serilog on every TFM and allocates less.
- Against HttpLogging, CSE is within error on every TFM. CSE allocates slightly more on every TFM.
- HttpLogging and Serilog log the status code; CSE has no status code field, so its event carries less.

### Where CSE loses

- 1 KB bodies, all TFMs: slower than `HttpLogging_Bodies`: 21.49 against 17.60 μs (net8.0), 18.81 against 15.33 μs (net9.0), 16.685 against 13.661 μs (net10.0), 16.55 against 15.91 μs (net11.0). On net11.0 CSE also allocates more: 23.8 against 22.8 KB.
- `HttpLogging_Defaults`, all sizes and TFMs: faster than CSE: at 1 KB, where it logs both whole bodies plus headers: 18.65 against 21.49 μs (net8.0, the HttpLogging row is noisy, CV 5.4%), 16.26 against 18.81 μs (net9.0), 13.993 against 16.685 μs (net10.0), 16.16 against 16.55 μs (net11.0). At 1 KB it allocates less than CSE only on net11.0 (23.08 against 23.8 KB); on net8.0 to net10.0 CSE allocates less. At 64 KB and 1 MB it truncates each body at 32 KB, so it does less work and allocates less (net10.0: 106.896 against 165.691 μs at 64 KB, 637.724 against 2,107.251 μs at 1 MB).
- Metadata, all TFMs: allocates more than HttpLogging: 10.49 against 10.36 KB (net8.0), 10.43 against 10.3 KB (net9.0), 10.22 against 10.17 KB (net10.0), 13.06 against 13.05 KB (net11.0). Time is within error on every TFM.

---

## Caveats

- CSE code notes, from the source:
  - `Infrastructure/Middlewares/BaseMiddleware.cs:152-191,259-270`: the request body is buffered by `EnableBuffering`, copied into a pooled `RecyclableMemoryStream` and decoded with `Encoding.UTF8.GetString` over its buffers (a `StreamReader` runs only when the first byte suggests a byte order mark or UTF-16), so a logged request exists as buffered bytes, pooled blocks and one final string.
  - `BaseMiddleware.cs:210-257`: the response is teed into a `RecyclableMemoryStream` with no size cap and decoded the same way to one string after the endpoint finishes.
  - `Infrastructure/MessageCreators/LogMessageWithContextCreator.cs` builds the template once per options; `LogMessageCreator.cs` (used when `UseSeparateContext` is off) formats a new message string per request, which is its job. `LoggingOptions.CreateAllFields()` turns `UseSeparateContext` on.
  - `LogWriters/LoggerFactoryLogWriter.cs:49` checks `IsEnabled` before it builds the message, but the middleware has already captured the bodies by then.
- The counting provider renders the message but writes nothing, so I/O cost of a real sink is not included for any row.
- TestServer, not Kestrel: sockets, TLS and HTTP parsing are not measured.
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
