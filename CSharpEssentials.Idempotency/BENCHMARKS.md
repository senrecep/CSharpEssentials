# CSharpEssentials.Idempotency: performance benchmarks

This page compares the CSE idempotency stack with IdempotentAPI 2.6.0 (with IdempotentAPI.MinimalAPI 3.2.0 and IdempotentAPI.Cache.DistributedCache 2.2.0) for an HTTP replay, and measures the transport-independent executor, inbox, record stores and deterministic keys against hand-written baselines. The HTTP rows use the middleware and endpoint filter of CSharpEssentials.AspNetCore on top of this package. The suite uses BenchmarkDotNet v0.14.0 and targets .NET 8, 9, 10 and 11.

The classes live in `benchmarks/CSharpEssentials.AspNetCore.Comparison.Benchmarks/` (suite key `aspnetcore`), next to the classes documented in [CSharpEssentials.AspNetCore/BENCHMARKS.md](../CSharpEssentials.AspNetCore/BENCHMARKS.md) and [CSharpEssentials.RequestResponseLogging/BENCHMARKS.md](../CSharpEssentials.RequestResponseLogging/BENCHMARKS.md). The older `benchmarks/CSharpEssentials.Idempotency.Benchmarks/` suite (middleware overhead and fingerprint buffering, net9-net11, results under `benchmarks/results/idempotency/`) is covered in [Results](#middleware-overhead-and-fingerprint-buffering-csharpessentialsidempotencybenchmarks).

The parity self-check passes on every TFM, and full default-job results for net8.0 to net11.0 are in [Results](#results).

The competitor packages are pinned exactly in `Directory.Packages.props`. Each pin is the latest stable release on NuGet as of 2026-11-03:

| Package | Version | Published | License | Asset the suite loads |
|---|---|---|---|---|
| IdempotentAPI | 2.6.0 | 2024-08-22 | MIT, from the repo (see below) | `lib/netstandard2.1` |
| IdempotentAPI.MinimalAPI | 3.2.0 | 2024-08-22 | MIT, from the repo | `lib/net7.0` |
| IdempotentAPI.Cache.DistributedCache | 2.2.0 | 2023-12-26 | MIT, from the repo | `lib/netstandard2.1` |

- **License metadata gap.** None of the IdempotentAPI nuspecs has `<license>` or `<licenseUrl>`, and the packages bundle no license file. The source repository `ikyriak/IdempotentAPI` has `LICENSE.md` with the MIT License at the release commit `b38045c` that the nuspecs name, and the repository's license metadata reports `spdx_id: MIT`. The package is open source, so it stays in the suite.
- Transitive packages: IdempotentAPI.AccessCache 2.2.0, IdempotentAPI.Cache.Abstractions 2.2.0, IdempotentAPI.DistributedAccessLock.Abstractions 2.1.0 and Newtonsoft.Json 13.0.1 (MIT). IdempotentAPI's old `Microsoft.AspNetCore.Mvc.Core 2.2.5` dependency is pruned by the SDK on every TFM; it does not appear in the restore assets.
- Transitive pinning is off for this project (see the AspNetCore page for why).

What each TFM runs (`--self-check` prints the ASP.NET Core and competitor versions as its first line):

| TFM | ASP.NET Core | CSE.Idempotency build | CSE.AspNetCore build |
|---|---|---|---|
| net8.0 | 8.0.30 | netstandard2.1 (+ Microsoft.Bcl.TimeProvider) | net8.0 |
| net9.0 | 9.0.19 | net9.0 | net9.0 |
| net10.0 | 10.0.11 | net10.0 | net10.0 |
| net11.0 | 11.0.0-rc.1 | net11.0 | net11.0 |

CSE.Idempotency has no net8 target, so net8 runs its netstandard2.1 build.

---

## What was measured

| # | Class | Measured unit | Rows | Parity |
|---|---|---|---|---|
| 1 | `IdempotentReplayComparison` | POST /orders with the same small `CreateOrder` body; the handler returns a `ReplayPayload` of `ResponseSize` = 1 KB or 256 KB JSON | Replay (a key completed in setup): plain endpoint (baseline), CSE over `DistributedCacheIdempotencyRecordStore`, IdempotentAPI over `AddIdempotentAPIUsingDistributedCache`, CSE over `InMemoryIdempotencyRecordStore` (labelled). NewKey (a new key per call): plain (baseline), CSE distributed, IdempotentAPI | The key sent twice gives 200 twice, one handler call, and the same bytes as the plain response on every host; two new keys give two handler calls |
| 2 | `IdempotencyExecutorBenchmarks` | `IdempotencyExecutor.ExecuteAsync` over the in-memory store, operation returns 42 | Execute: the operation alone (baseline), hand-written `ConcurrentDictionary.GetOrAdd` (labelled), CSE FirstRun, Replay, InFlight, KeyReused, Mode Off. Inbox: `IdempotencyInbox.ProcessAsync` new (baseline) and duplicate message | Statuses Executed / Replayed / InFlight / KeyReused / Executed, value 42 where defined |
| 3 | `IdempotencyStoreBenchmarks` | The stores alone, payload `PayloadSize` = 0, 1 KB, 64 KB | InMemory: reserve+complete a new key (baseline), reserve a completed key, reserve+release. Distributed (`MemoryDistributedCache`): reserve+complete (baseline), reserve a completed key | `true` for every complete/release; the completed reservation returns the payload length |
| 4 | `IdempotencyStoreContentionBenchmarks` | 1024 in-memory operations per invocation on `Threads` = 8 or 32 workers | Same completed key from every worker (baseline), a new key per operation | 1024 successful operations |
| 5 | `IdempotencyKeyBenchmarks` | Deterministic keys | For: short canonical key (baseline) and a key over 255 characters (hashed). Uuid: RFC 9562 section 5.5 by hand with `SHA1.HashData` and stackalloc (baseline) against `ToUuidV5`. StorageKey: global key and a user-and-tenant scope | The hand-written and CSE UUIDv5 are the same `Guid` |

- Every class has `[MemoryDiagnoser]`. The HTTP class runs each host as its own Production `TestServer` host and reads every response body.
- No competitor exposes an executor, inbox, store or key API without HTTP, so classes 2 to 5 compare CSE with the operation alone or with hand-written code.

### Inputs

- Request body for every replay row: `{"orderId":"order-1","amount":25,"currency":"eur"}`. `CreateOrder` and `ReplayPayload` carry `[Serializable]`, per IdempotentAPI's setup step; it does nothing for System.Text.Json.
- `ReplayPayload.Data` is pseudo-random `[a-z0-9]` text from a fixed `Random` seed, sized so the JSON is exactly `ResponseSize` bytes. A padded payload would flatter IdempotentAPI, which gzips what it stores.
- The handler returns `TypedResults.Ok(payload)`. IdempotentAPI's filter stores only `IValueHttpResult.Value`; any other `IResult` becomes an empty string.
- Both libraries read the key from `Idempotency-Key` (IdempotentAPI's default is `IdempotencyKey`, so its options set `HeaderKeyName`). CSE uses `ScopeResolver = IdempotencyScopes.Global`.
- The like-for-like CSE host and the IdempotentAPI host use the same `MemoryDistributedCache` (`AddDistributedMemoryCache()`), with its default 200 MB `SizeLimit`.
- NewKey rows run on their own hosts, so a cache that fills up cannot evict the key the Replay rows replay.
- Executor and store rows that use a new key per call move a fake clock past retention every 4096 keys, so the in-memory store sweeps and stays bounded; the distributed store rows start a new `MemoryDistributedCache` every 1024 keys, so the 200 MB limit never refuses a write there.

---

## Semantic differences

- **The handler counter is captured, not injected.** IdempotentAPI's minimal-API filter fingerprints every bound argument, injected services included. The first version of the suite injected `HandlerCounter` into the handler; its `Count` changed after the first call, so every retry got `400 The Idempotency header key value '…' was used in a different request.` Every replay host now captures the counter instead, so all rows run the same handler. A real app that injects a service with changing serializable state into an IdempotentAPI endpoint hits the same 400.
- **Fingerprint.** CSE hashes method, path, query and the raw body (SHA-256; the body is buffered with `EnableBuffering` and would spill to a temp file over 30 KB, which this small request never does). IdempotentAPI hashes the Newtonsoft-serialized bound arguments and the path, never the raw body; `HttpRequest`/`HttpContext` arguments are excluded. Both detect a changed `CreateOrder`.
- **Stored form.** CSE stores the response bytes plus allowlisted headers and writes them back on replay. IdempotentAPI stores the result object as Newtonsoft JSON with `TypeNameHandling.All`, gzipped; on replay it decompresses, deserializes, rebuilds the result type by reflection and serializes the value again through System.Text.Json. Its cost depends on how well the payload compresses.
- **Replay signal.** CSE replays add `Idempotency-Replayed: true` (headers seen: `Content-Length, Content-Type, Idempotency-Replayed`). IdempotentAPI replays add none (`Content-Type` only), so parity counts handler calls. The replayed bytes match the plain response byte for byte on both, at 1 KB and 256 KB.
- **In flight and key reuse.** CSE answers 409 with `Retry-After` while a request is in flight, and 422 when a key is reused with another request. IdempotentAPI answers 409 with no `Retry-After`, and 400 with a plain-text message for key reuse.
- **Cache limit.** `MemoryDistributedCache` sizes each entry by its value length. Over the default 200 MB `SizeLimit` it refuses new entries and compacts in the background. A long NewKey run reaches the limit for both libraries, so those rows include refused writes and compaction (labelled). The self-check filled a fresh CSE host and a fresh IdempotentAPI host with 1400 keys of 256 KB, then sent one more key twice: both still answered 200/200 with one handler call, so a refused write of older keys did not break the replay of the key just written. The 256 KB NewKey rows are kept.
- **InMemoryStore row.** `CSharpEssentials_Replay_InMemoryStore` uses CSE's default store, not the store the competitor uses; it is not like-for-like.
- **Hand-written baselines.** The `ConcurrentDictionary` dedupe has no lease, fingerprint, retention or mismatch detection. The hand-written UUIDv5 receives the canonical name prepared in setup, while `ToUuidV5` builds it on every call.

---

## Feature matrix

From CSE source and IdempotentAPI source at `b38045c`; not measured.

| Feature | CSE | IdempotentAPI 2.6.0 / MinimalAPI 3.2.0 |
|---|---|---|
| Header name | `Idempotency-Key` | `IdempotencyKey` by default, configurable |
| Fingerprint | Method + path + query + raw body, SHA-256 | Bound arguments + path |
| Stored form | Response bytes plus allowlisted headers | Newtonsoft result object, gzipped, plus all headers except an exclude list |
| Replay header | `Idempotency-Replayed` | None |
| In flight | 409 + `Retry-After` | 409 `ConflictResult`, no `Retry-After` (`Idempotency.cs` ApplyPreIdempotency, about line 275) |
| Key reused with another body | 422 | 400 `BadRequestObjectResult` with a plain-text message (about line 287) |
| Replay rebuild | Writes the stored bytes | Resolves the stored `ResultType` by name and calls its constructor by reflection (about lines 300-330) |
| Scope per user/tenant | Yes | No |
| Modes Observe/Enforce | Yes | `IsIdempotencyOptional` only |
| Token-fenced reservations | Yes | Optional distributed locks |
| Stores | In-memory, `IDistributedCache`, EF Core transactional | `IDistributedCache`, FusionCache |
| Transport-independent executor and inbox | Yes | No |
| MVC dependency | No for minimal APIs | Builds MVC `ActionExecutingContext` objects |
| Last release | Working tree | 2024-08-22 |
| AOT/trim flagged | No | No; uses Newtonsoft with `TypeNameHandling` and reflection |
| License | MIT | MIT in the repo, missing from the nuspecs |

---

## Results

### How these numbers were produced

- BenchmarkDotNet v0.14.0, default job (full runs, not `--quick`), `[MemoryDiagnoser]` on every class. SDK 11.0.100-rc.1; runtimes .NET 8.0.30, 9.0.19, 10.0.11 and 11.0.0 (11.0.26.42628), as the report headers print them, each TFM in its own run. A range with no TFM named spans net8.0 to net11.0 (net9.0 to net11.0 for the `CSharpEssentials.Idempotency.Benchmarks` rows). Differences and multiples in the prose are computed from the means and allocations in the tables.
- One Linux x64 machine, 6 physical cores and 12 threads (the exact CPU is in each report header). The suites ran one after another, never in parallel.
- `aspnetcore` suite (classes 1 to 5 above): the 1-minute load average was 1.60 when it started and 2.02 when it ended (5-minute average 1.70 to 2.65). Classes with a row above 5% CV were rerun, up to twice. The reports used here for `IdempotentReplayComparison` on net8.0 and net9.0 come from the second rerun (1-minute load 2.41 to 3.94), and on net11.0 from the first rerun (1-minute load 6.96 at its start, 2.23 at its end). `IdempotencyStoreContentionBenchmarks` on net11.0 comes from a rerun that started at a 1-minute load of 3.71 and ended at 6.96. Five 256 KB replay rows stayed above 5% CV after both reruns; they are marked noisy wherever they appear.
- `CSharpEssentials.Idempotency.Benchmarks` suite (net9.0 to net11.0 only, the project has no net8.0 target): the 1-minute load average was 2.65 at the start and 2.46 at the end (5-minute average 2.18 to 3.54). No row is above 5% CV.
- Reports: `benchmarks/results/aspnetcore/{tfm}/results/` (for example [net10.0](../benchmarks/results/aspnetcore/net10.0/results/)) and `benchmarks/results/idempotency/{tfm}/results/` (for example [net10.0](../benchmarks/results/idempotency/net10.0/results/)), as `*-report-github.md`, `.csv` and `.html`.

Each cell is `Mean (Ratio), Allocated`, copied from the github report of that TFM. The ratio is against the baseline row of the same category and parameter.

### HTTP replay against IdempotentAPI

| Method | ResponseSize | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|
| `Plain_NoIdempotency_NewKey` | 1024 | 15.12 μs (1.00), 11.49 KB | 14.26 μs (1.00), 11.38 KB | 14.25 μs (1.00), 11.97 KB | 16.56 μs (1.00), 13.3 KB |
| `CSharpEssentials_NewKey` | 1024 | 46.10 μs (3.05), 25.52 KB | 44.34 μs (3.11), 25.06 KB | 41.63 μs (2.92), 25.24 KB | 44.66 μs (2.70), 26.04 KB |
| `IdempotentAPI_NewKey` | 1024 | 136.69 μs (9.05), 56.89 KB | 133.16 μs (9.34), 56.63 KB | 127.38 μs (8.94), 57.29 KB | 119.67 μs (7.23), 58.65 KB |
| `Plain_NoIdempotency_NewKey` | 262144 | 153.57 μs (1.00), 403.63 KB | 140.87 μs (1.00), 396.48 KB, noisy (CV 5.4%) | 100.30 μs (1.00), 269.05 KB | 114.48 μs (1.00), 271.45 KB |
| `CSharpEssentials_NewKey` | 262144 | 358.57 μs (2.34), 2460.96 KB | 345.74 μs (2.46), 2461.39 KB | 345.16 μs (3.44), 2332.89 KB | 353.65 μs (3.09), 2334.81 KB |
| `IdempotentAPI_NewKey` | 262144 | 4,642.61 μs (30.30), 2407.54 KB | 5,552.47 μs (39.53), 2400.24 KB | 5,316.66 μs (53.03), 2273.06 KB | 5,433.38 μs (47.48), 2275.08 KB |
| `Plain_NoIdempotency` | 1024 | 15.12 μs (1.00), 11.4 KB | 14.14 μs (1.00), 11.29 KB | 14.94 μs (1.00), 11.89 KB | 17.23 μs (1.00), 13.22 KB |
| `CSharpEssentials_Replay` | 1024 | 22.19 μs (1.47), 17.89 KB | 19.92 μs (1.41), 16.68 KB | 17.35 μs (1.16), 16.52 KB | 19.63 μs (1.14), 17.94 KB |
| `IdempotentAPI_Replay` | 1024 | 116.59 μs (7.72), 46.63 KB | 104.84 μs (7.42), 46.35 KB | 100.42 μs (6.72), 47.62 KB | 88.16 μs (5.12), 48.98 KB |
| `CSharpEssentials_Replay_InMemoryStore` | 1024 | 20.51 μs (1.36), 15.6 KB | 19.17 μs (1.36), 14.92 KB | 15.80 μs (1.06), 14.77 KB | 17.22 μs (1.00), 16.17 KB |
| `Plain_NoIdempotency` | 262144 | 166.64 μs (1.01), 403.56 KB, noisy (CV 8.2%) | 139.60 μs (1.00), 396.37 KB | 91.71 μs (1.00), 268.94 KB | 113.51 μs (1.00), 271.28 KB |
| `CSharpEssentials_Replay` | 262144 | 277.90 μs (1.68), 1048.2 KB, noisy (CV 11.2%) | 257.36 μs (1.85), 1047.08 KB, noisy (CV 8.4%) | 226.91 μs (2.47), 1046.21 KB | 221.36 μs (1.95), 1048.56 KB |
| `IdempotentAPI_Replay` | 262144 | 2,446.48 μs (14.78), 4649.58 KB | 1,999.81 μs (14.34), 4648.05 KB, noisy (CV 7.3%) | 1,887.35 μs (20.58), 4516.21 KB | 1,771.16 μs (15.61), 4518.42 KB |
| `CSharpEssentials_Replay_InMemoryStore` | 262144 | 207.70 μs (1.25), 790.13 KB | 178.41 μs (1.28), 789.53 KB | 182.93 μs (2.00), 789.41 KB | 181.16 μs (1.60), 791.07 KB |

- Replay of a completed key: CSE over `MemoryDistributedCache` is faster than IdempotentAPI and allocates less on every TFM, at both sizes. At 256 KB on net10.0: 226.91 μs and 1046.21 KB against 1,887.35 μs and 4516.21 KB.
- Against the plain endpoint, a CSE replay costs 1.14 to 1.47 at 1 KB and 1.68 to 2.47 at 256 KB (the 1.68 on net8.0 and 1.85 on net9.0 are noisy rows, CV 11.2% and 8.4%). A replay never runs the handler, but it still reads the stored entry, deserializes it and writes the whole body.
- New key: CSE is faster than IdempotentAPI on every TFM (at 256 KB, 345.16 μs against 5,316.66 μs on net10.0). At 256 KB CSE allocates more than IdempotentAPI on every TFM (see below).
- `CSharpEssentials_Replay_InMemoryStore` uses CSE's default store, which IdempotentAPI does not offer; it is not like-for-like.

### Executor and inbox

| Method | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `Direct_Call` | 7.973 ns (1.00), - | 8.049 ns (1.00), - | 1.424 ns (1.00), - | 1.066 ns (1.00), - |
| `HandWritten_ConcurrentDictionary` | 46.223 ns (5.80), - | 23.431 ns (2.91), - | 15.138 ns (10.63), - | 12.380 ns (11.62), - |
| `CSharpEssentials_FirstRun` | 1,641.095 ns (205.83), 592 B | 1,566.874 ns (194.67), 592 B | 1,486.117 ns (1,043.78), 592 B | 603.325 ns (566.26), 592 B |
| `CSharpEssentials_Replay` | 171.365 ns (21.49), 152 B | 181.050 ns (22.49), 152 B | 130.799 ns (91.87), 152 B | 129.085 ns (121.15), 152 B |
| `CSharpEssentials_InFlight` | 151.481 ns (19.00), 96 B | 144.509 ns (17.95), 96 B | 117.662 ns (82.64), 96 B | 120.094 ns (112.72), 96 B |
| `CSharpEssentials_KeyReused` | 151.532 ns (19.01), 96 B | 143.841 ns (17.87), 96 B | 121.813 ns (85.56), 96 B | 124.695 ns (117.03), 96 B |
| `CSharpEssentials_ModeOff` | 74.482 ns (9.34), 96 B | 72.395 ns (8.99), 96 B | 59.187 ns (41.57), 96 B | 58.841 ns (55.23), 96 B |
| `CSharpEssentials_Inbox_New` | 2,222.718 ns (1.00), 1152 B | 2,113.967 ns (1.00), 1152 B | 1,926.299 ns (1.00), 1112 B | 1,027.703 ns (1.00), 1112 B |
| `CSharpEssentials_Inbox_Duplicate` | 477.924 ns (0.22), 744 B | 448.081 ns (0.21), 744 B | 418.617 ns (0.22), 704 B | 415.982 ns (0.40), 704 B |

- `Direct_Call` is 1.066 to 8.049 ns, so the Execute ratios (up to 1,043.78) mostly show how small the baseline is. Read the CSE rows as absolute costs: a replay is 129.085 to 181.050 ns with 152 B, a first run is 603.325 to 1,641.095 ns with 592 B.
- `CSharpEssentials_ModeOff` (idempotency off) still costs 58.841 to 74.482 ns and 96 B, from the interpolated string built before the mode check (see Caveats).
- The hand-written `ConcurrentDictionary.GetOrAdd` is 12.380 to 46.223 ns with no allocation. It has no lease, fingerprint, retention or mismatch check, so it is a floor, not an equivalent.
- The first run is much cheaper on net11.0 (603.325 ns) than on net8.0 to net10.0 (1,486.117 to 1,641.095 ns), and so is the inbox new message; the store rows below show the same step.

### Record stores

| Method | PayloadSize | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|
| `CSharpEssentials_Distributed_ReserveComplete` | 0 | 3,146.28 ns (1.00), 3456 B | 2,971.91 ns (1.00), 2909 B | 2,684.14 ns (1.00), 2528 B | 1,723.39 ns (1.00), 2525 B |
| `CSharpEssentials_Distributed_Replay` | 0 | 361.34 ns (0.11), 1136 B | 291.85 ns (0.10), 592 B | 280.68 ns (0.10), 592 B | 233.12 ns (0.14), 592 B |
| `CSharpEssentials_Distributed_ReserveComplete` | 1024 | 3,995.13 ns (1.00), 5576 B | 3,665.57 ns (1.00), 5034 B | 3,350.96 ns (1.00), 4646 B | 2,407.61 ns (1.00), 4629 B |
| `CSharpEssentials_Distributed_Replay` | 1024 | 425.28 ns (0.11), 2184 B | 364.59 ns (0.10), 1640 B | 342.49 ns (0.10), 1640 B | 295.20 ns (0.12), 1640 B |
| `CSharpEssentials_Distributed_ReserveComplete` | 65536 | 27,488.55 ns (1.00), 134600 B | 28,134.35 ns (1.00), 134057 B | 27,469.85 ns (1.00), 133674 B | 26,279.41 ns (1.00), 133676 B |
| `CSharpEssentials_Distributed_Replay` | 65536 | 3,355.09 ns (0.12), 66696 B | 3,416.61 ns (0.12), 66152 B | 3,347.91 ns (0.12), 66152 B | 3,039.07 ns (0.12), 66152 B |
| `CSharpEssentials_InMemory_ReserveComplete` | 0 | 1,383.85 ns (1.00), 432 B | 1,361.78 ns (1.00), 432 B | 1,238.02 ns (1.00), 432 B | 443.29 ns (1.00), 432 B |
| `CSharpEssentials_InMemory_ReserveHit` | 0 | 67.18 ns (0.05), 56 B | 73.31 ns (0.05), 56 B | 71.59 ns (0.06), 56 B | 58.04 ns (0.13), 56 B |
| `CSharpEssentials_InMemory_ReserveRelease` | 0 | 1,187.48 ns (0.86), 376 B | 1,125.09 ns (0.83), 376 B | 1,062.08 ns (0.86), 376 B | 295.62 ns (0.67), 376 B |
| `CSharpEssentials_InMemory_ReserveComplete` | 1024 | 1,586.28 ns (1.00), 1480 B | 1,521.67 ns (1.00), 1480 B | 1,428.43 ns (1.00), 1480 B | 601.67 ns (1.00), 1480 B |
| `CSharpEssentials_InMemory_ReserveHit` | 1024 | 67.84 ns (0.04), 56 B | 67.43 ns (0.04), 56 B | 73.01 ns (0.05), 56 B | 57.54 ns (0.10), 56 B |
| `CSharpEssentials_InMemory_ReserveRelease` | 1024 | 1,187.33 ns (0.75), 376 B | 1,138.23 ns (0.75), 376 B | 1,074.98 ns (0.75), 376 B | 288.99 ns (0.48), 376 B |
| `CSharpEssentials_InMemory_ReserveComplete` | 65536 | 14,835.13 ns (1.000), 65995 B | 14,774.55 ns (1.000), 65995 B | 14,640.26 ns (1.000), 65996 B | 14,033.57 ns (1.001), 65995 B |
| `CSharpEssentials_InMemory_ReserveHit` | 65536 | 67.42 ns (0.005), 56 B | 66.71 ns (0.005), 56 B | 74.91 ns (0.005), 56 B | 58.14 ns (0.004), 56 B |
| `CSharpEssentials_InMemory_ReserveRelease` | 65536 | 1,173.37 ns (0.079), 376 B | 1,123.28 ns (0.076), 376 B | 1,064.66 ns (0.073), 376 B | 292.36 ns (0.021), 376 B |

- A reservation hit on the in-memory store is 57.54 to 74.91 ns and 56 B at every payload size.
- The distributed store copies the payload into its entry format: at 64 KB, reserve+complete allocates 133674 to 134600 B (two copies of the payload) and a replay 66152 to 66696 B (one copy). The in-memory store keeps one copy on complete (65995 to 65996 B at 64 KB) and none on a hit.
- In-memory reserve+complete at 0 and 1 KB is 1,238.02 to 1,586.28 ns on net8.0 to net10.0 and 443.29 to 601.67 ns on net11.0. The reserve+release row shows the same gap (1,062.08 to 1,187.48 ns against 288.99 to 295.62 ns).

### Store contention

| Method | Threads | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|
| `CSharpEssentials_InMemory_Contended_SameKey` | 8 | 242.3 ns (1.00), 59 B | 248.4 ns (1.00), 59 B | 261.6 ns (1.00), 59 B | 255.0 ns (1.00), 59 B |
| `CSharpEssentials_InMemory_Contended_DistinctKeys` | 8 | 863.7 ns (3.56), 1483 B | 849.2 ns (3.42), 1483 B | 730.5 ns (2.79), 1483 B | 779.0 ns (3.06), 1483 B |
| `CSharpEssentials_InMemory_Contended_SameKey` | 32 | 296.6 ns (1.00), 61 B | 278.4 ns (1.00), 61 B | 283.3 ns (1.00), 61 B | 325.8 ns (1.00), 63 B |
| `CSharpEssentials_InMemory_Contended_DistinctKeys` | 32 | 853.9 ns (2.88), 1485 B | 863.7 ns (3.10), 1485 B | 775.3 ns (2.74), 1485 B | 817.1 ns (2.51), 1487 B |

- The mean is per operation (1024 operations per invocation). The same completed key from every worker costs 242.3 to 325.8 ns and 59 to 63 B; a new key per operation costs 730.5 to 863.7 ns and 1483 to 1487 B.
- This class depends on the core count and the scheduler. The net11.0 report comes from a rerun at a 1-minute load of 3.71 to 6.96, so compare its rows only with each other.

### Deterministic keys

| Method | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_For_Short` | 73.17 ns (1.00), 184 B | 65.85 ns (1.00), 184 B | 57.98 ns (1.00), 152 B | 44.64 ns (1.00), 152 B |
| `CSharpEssentials_For_Long` | 1,814.16 ns (24.80), 2464 B | 1,795.39 ns (27.26), 2464 B | 1,373.50 ns (23.69), 2432 B | 1,293.39 ns (28.97), 2432 B |
| `CSharpEssentials_ToStorageKey` | 84.83 ns (1.00), 232 B | 72.27 ns (1.00), 232 B | 80.71 ns (1.00), 232 B | 58.73 ns (1.00), 232 B |
| `CSharpEssentials_Scope_UserAndTenant` | 185.25 ns (2.18), 672 B | 189.07 ns (2.62), 672 B | 174.48 ns (2.16), 632 B | 135.71 ns (2.31), 632 B |
| `HandWritten_UuidV5` | 733.36 ns (1.00), - | 736.82 ns (1.00), - | 582.37 ns (1.00), - | 587.74 ns (1.00), - |
| `CSharpEssentials_ToUuidV5` | 1,400.02 ns (1.91), 488 B | 1,305.69 ns (1.77), 488 B | 891.03 ns (1.53), 456 B | 885.82 ns (1.51), 456 B |

- A short key (`For`) is 44.64 to 73.17 ns. A key over 255 characters is hashed: 1,293.39 to 1,814.16 ns and 2432 to 2464 B.
- `ToUuidV5` costs 1.51 to 1.91 of the hand-written UUIDv5 and allocates 456 to 488 B where the hand-written one allocates nothing. The hand-written version gets the canonical name prepared in setup; `ToUuidV5` builds it on every call.

### Middleware overhead and fingerprint buffering (`CSharpEssentials.Idempotency.Benchmarks`)

This older suite runs the CSE middleware and endpoint filter over the in-memory store on one host, with a handler that reads the whole request body. Its rows are not comparable with the `aspnetcore` replay rows above (different host, body and store).

| Method | net9.0 | net10.0 | net11.0 |
|---|---|---|---|
| `NoIdempotency` | 11.05 μs (1.00), 9.47 KB | 10.34 μs (1.00), 9.34 KB | 12.88 μs (1.00), 10.75 KB |
| `NewKey` | 30.04 μs (2.72), 14.69 KB | 25.35 μs (2.45), 14.48 KB | 30.46 μs (2.37), 15.39 KB |
| `Replay` | 16.26 μs (1.47), 11.53 KB | 14.61 μs (1.41), 11.38 KB | 16.25 μs (1.26), 12.78 KB |

| Method | BodySize | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `Plain` | 1024 | 11.33 μs (1.00), 9.5 KB | 10.33 μs (1.00), 9.38 KB | 13.09 μs (1.00), 10.78 KB |
| `KeyedReplay` | 1024 | 17.70 μs (1.56), 11.56 KB | 15.15 μs (1.47), 11.41 KB | 17.55 μs (1.34), 12.82 KB |
| `KeyedNewKey` | 1024 | 31.32 μs (2.77), 14.76 KB | 26.17 μs (2.53), 14.5 KB | 31.94 μs (2.44), 15.41 KB |
| `Plain` | 65536 | 26.06 μs (1.00), 12.51 KB | 23.81 μs (1.00), 12.38 KB | 27.59 μs (1.00), 13.99 KB |
| `KeyedReplay` | 65536 | 157.28 μs (6.04), 15.49 KB | 152.48 μs (6.41), 15.34 KB | 145.15 μs (5.27), 16.66 KB |
| `KeyedNewKey` | 65536 | 185.26 μs (7.11), 19.27 KB | 177.70 μs (7.47), 19.08 KB | 173.70 μs (6.31), 20.49 KB |
| `Plain` | 1048576 | 136.94 μs (1.00), 38.88 KB | 133.43 μs (1.00), 38.75 KB | 131.88 μs (1.00), 41.41 KB |
| `KeyedReplay` | 1048576 | 1,332.01 μs (9.73), 52.15 KB | 1,320.77 μs (9.90), 52.01 KB | 1,320.03 μs (10.01), 54.25 KB |
| `KeyedNewKey` | 1048576 | 1,428.49 μs (10.43), 56 KB | 1,413.76 μs (10.60), 55.77 KB | 1,429.64 μs (10.84), 57.94 KB |

- With a small body, a replay costs 1.26 to 1.47 of the route without idempotency, and a new key 2.37 to 2.72.
- The fingerprint reads and hashes the whole request body before the store lookup, so the cost grows with the request size: a keyed replay is 1.34 to 1.56 of the plain request at 1 KB, 5.27 to 6.41 at 64 KB and 9.73 to 10.01 at 1 MB. Allocation stays at 1.19 to 1.34 of the plain request, because the body goes through a pooled 16 KB buffer, and above 30 KB the default `EnableBuffering` threshold puts it in a temporary file.

### Where CSE loses

- **IdempotentReplay NewKey at 256 KB, all TFMs: CSE allocates more than IdempotentAPI.** 2460.96 against 2407.54 KB (net8.0), 2461.39 against 2400.24 KB (net9.0; the plain row of this pair is noisy, CV 5.4%), 2332.89 against 2273.06 KB (net10.0), 2334.81 against 2275.08 KB (net11.0). CSE is still faster on these rows. Against the plain endpoint, CSE allocates about 2 MB more for a 256 KB response.
- **`ToUuidV5` against the hand-written UUIDv5, all TFMs:** 1,400.02 against 733.36 ns (net8.0), 1,305.69 against 736.82 ns (net9.0), 891.03 against 582.37 ns (net10.0), 885.82 against 587.74 ns (net11.0), with 456 to 488 B allocated against none.
- **Executor against the hand-written dedupe:** a CSE replay is 129.085 to 181.050 ns with 152 B, the `ConcurrentDictionary` lookup 12.380 to 46.223 ns with none. The CSE row does more (lease, fingerprint, retention), but the gap is real.
- **Large keyed requests:** a keyed replay with a 64 KB body takes 152.48 μs against 23.81 μs for the plain request on net10.0, and 1,320.77 μs against 133.43 μs at 1 MB.

---

## Caveats

- **CSE code notes**, from the worktree source, with what the run shows:
  - `IdempotencyExecutor.cs:58`: an interpolated string is built on every call, before the mode check, only to name the operation in a possible error. `CSharpEssentials_ModeOff` shows it: 96 B per call with idempotency off.
  - `InMemoryIdempotencyRecordStore.cs:52,60`: the token `Guid` string and the `Entry` are created only when the key is missing or expired, so a hit allocates only its result (56 B).
  - `InMemoryIdempotencyRecordStore.cs:87`: `payload.ToArray()` copies the payload before the token check.
  - `IdempotencyInbox.cs:51`: `"messaging:" + consumer` is concatenated per message.
  - `DistributedCacheIdempotencyRecordStore.cs:52,83,103`: the storage key is rebuilt on every call.
  - `Internal/RecordEntrySerializer.cs:15-16`: a `MemoryStream`/`BinaryWriter` copy per entry, which is the second payload copy in the distributed reserve+complete rows.
  - `IdempotencyKeys.cs:96-119`: the canonical string, its UTF-8 bytes, the hash and the Base64 text per hashed key; `:79-93`: several arrays and an `IncrementalHash` per UUIDv5.
  - On the HTTP path, a new key captures the response in `ResponseCaptureStream` (a growing `MemoryStream`, then `ToArray()`), writes it again through `IdempotencyEntrySerializer` (`MemoryStream`, then `ToArray()`), and the distributed store copies it once more through `RecordEntrySerializer`. The NewKey 256 KB rows allocate about 2 MB more than the plain endpoint, about eight times the response size.
  - `CSharpEssentials.AspNetCore/Idempotency/IdempotencyMiddleware.cs:495`: the fingerprint calls `EnableBuffering()` with the default 30 KB threshold, so a larger request body goes to a temporary file before it is hashed.
- **The contention class is noisy.** It depends on the core count and the scheduler; compare it only on the same machine.
- **TestServer, not Kestrel**, for the HTTP classes.
- The CSE column is the working tree on branch `integ/all` (all open pull requests merged locally), which may not match the version published on NuGet.

---

## How to reproduce

```bash
cd benchmarks
./run-benchmarks.sh --project aspnetcore                       # all TFMs, every class of the suite, default job
./run-benchmarks.sh --project idempotency                      # the older suite, net9.0 to net11.0, default job
dotnet run -c Release -f net10.0 --project CSharpEssentials.AspNetCore.Comparison.Benchmarks -- --self-check
```

Results are stored per framework under `benchmarks/results/aspnetcore/{tfm}/results/` and `benchmarks/results/idempotency/{tfm}/results/`.
