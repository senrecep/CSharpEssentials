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

- BenchmarkDotNet v0.14.0, default job (full runs, not `--quick`), `[MemoryDiagnoser]` on every class. Classes 2 to 5: SDK 11.0.100-rc.1; runtimes .NET 8.0.30, 9.0.19, 10.0.11 and 11.0.0 (11.0.26.42628). `IdempotentReplayComparison` and the `CSharpEssentials.Idempotency.Benchmarks` suite were rerun after the performance changes in #230-#256: SDK 11.0.100-rc.3; runtimes .NET 8.0.33, 9.0.22, 10.0.14 and 11.0.0 (11.0.26.52348). Versions are as the report headers print them, each TFM in its own run. A range with no TFM named spans net8.0 to net11.0 (net9.0 to net11.0 for the `CSharpEssentials.Idempotency.Benchmarks` rows). Differences and multiples in the prose are computed from the means and allocations in the tables.
- One Linux x64 machine, 6 physical cores and 12 threads. The suites ran one after another, never in parallel.
- `aspnetcore` suite, classes 2 to 5: the 1-minute load average was 1.60 when it started and 2.02 when it ended (5-minute average 1.70 to 2.65). Classes with a row above 5% CV were rerun, up to twice. `IdempotencyStoreContentionBenchmarks` on net11.0 comes from a rerun that started at a 1-minute load of 3.71 and ended at 6.96.
- `IdempotentReplayComparison` rerun: nine rows on net8.0 and net9.0 are above 5% CV, seven of them 256 KB rows; they are marked noisy wherever they appear.
- `CSharpEssentials.Idempotency.Benchmarks` suite (net9.0 to net11.0 only, the project has no net8.0 target): two net9.0 rows are above 5% CV (`NewKey` 6.2%, `Plain` at 1 KB 6.8%) and are marked noisy.
- Reports: `benchmarks/results/aspnetcore/{tfm}/results/` (for example [net10.0](../benchmarks/results/aspnetcore/net10.0/results/)) and `benchmarks/results/idempotency/{tfm}/results/` (for example [net10.0](../benchmarks/results/idempotency/net10.0/results/)), as `*-report-github.md`, `.csv` and `.html`.

Each cell is `Mean (Ratio), Allocated`, copied from the github report of that TFM. The ratio is against the baseline row of the same category and parameter.

### HTTP replay against IdempotentAPI

| Method | ResponseSize | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|
| `Plain_NoIdempotency_NewKey` | 1024 | 14.63 μs (1.00), 11.49 KB | 13.49 μs (1.00), 11.38 KB | 13.41 μs (1.00), 11.97 KB | 14.65 μs (1.00), 13.34 KB |
| `CSharpEssentials_NewKey` | 1024 | 42.02 μs (2.88), 21.11 KB | 38.82 μs (2.88), 20.67 KB | 37.76 μs (2.82), 20.85 KB | 36.21 μs (2.47), 22.64 KB |
| `IdempotentAPI_NewKey` | 1024 | 130.43 μs (8.93), 56.83 KB | 123.64 μs (9.18), 56.63 KB | 119.57 μs (8.92), 57.29 KB | 106.81 μs (7.29), 58.65 KB |
| `Plain_NoIdempotency_NewKey` | 262144 | 180.32 μs (1.01), 403.69 KB, noisy (CV 11.7%) | 151.42 μs (1.02), 396.49 KB, noisy (CV 13.5%) | 102.76 μs (1.00), 268.95 KB | 84.26 μs (1.00), 270.45 KB |
| `CSharpEssentials_NewKey` | 262144 | 206.72 μs (1.16), 924.42 KB | 192.78 μs (1.30), 924.42 KB | 189.84 μs (1.85), 796.12 KB | 190.68 μs (2.26), 798.02 KB |
| `IdempotentAPI_NewKey` | 262144 | 4,609.39 μs (25.88), 2407.54 KB | 5,296.44 μs (35.62), 2400.24 KB | 5,170.61 μs (50.34), 2272.98 KB | 5,210.03 μs (61.84), 2274.51 KB |
| `Plain_NoIdempotency` | 1024 | 14.45 μs (1.00), 11.41 KB | 13.08 μs (1.00), 11.29 KB | 13.37 μs (1.00), 11.88 KB | 13.22 μs (1.00), 13.22 KB |
| `CSharpEssentials_Replay` | 1024 | 19.63 μs (1.36), 14.67 KB | 17.14 μs (1.31), 13.46 KB, noisy (CV 5.2%) | 14.34 μs (1.07), 13.31 KB | 15.01 μs (1.14), 14.72 KB |
| `IdempotentAPI_Replay` | 1024 | 107.99 μs (7.49), 46.63 KB | 99.06 μs (7.58), 46.35 KB | 92.90 μs (6.95), 47.62 KB | 80.02 μs (6.06), 48.98 KB |
| `CSharpEssentials_Replay_InMemoryStore` | 1024 | 18.15 μs (1.26), 13.41 KB, noisy (CV 6.0%) | 16.12 μs (1.23), 12.74 KB | 13.91 μs (1.04), 12.59 KB | 14.74 μs (1.12), 14 KB |
| `Plain_NoIdempotency` | 262144 | 187.21 μs (1.02), 403.61 KB, noisy (CV 14.8%) | 142.70 μs (1.00), 396.21 KB | 103.25 μs (1.00), 268.93 KB | 96.44 μs (1.00), 270.47 KB |
| `CSharpEssentials_Replay` | 262144 | 143.77 μs (0.78), 277.83 KB | 127.07 μs (0.89), 277.28 KB, noisy (CV 11.6%) | 85.63 μs (0.83), 276.81 KB | 94.11 μs (0.98), 278.24 KB |
| `IdempotentAPI_Replay` | 262144 | 2,037.07 μs (11.10), 4649.56 KB, noisy (CV 8.3%) | 1,876.38 μs (13.18), 4642.97 KB | 1,498.60 μs (14.52), 4517.6 KB | 1,443.11 μs (14.97), 4518.51 KB |
| `CSharpEssentials_Replay_InMemoryStore` | 262144 | 133.19 μs (0.73), 277.28 KB, noisy (CV 14.6%) | 124.85 μs (0.88), 276.39 KB, noisy (CV 9.8%) | 90.78 μs (0.88), 276.12 KB | 94.58 μs (0.98), 277.6 KB |

- Replay of a completed key: CSE over `MemoryDistributedCache` is faster than IdempotentAPI and allocates less on every TFM, at both sizes. At 256 KB on net10.0: 85.63 μs and 276.81 KB against 1,498.60 μs and 4517.6 KB.
- Against the plain endpoint, a CSE replay costs 1.07 to 1.36 at 1 KB. At 256 KB it is 0.77 to 0.98 of the plain endpoint (0.83 on net10.0, where neither row is noisy; on net8.0 and net9.0 the plain or the CSE row is noisy). A replay skips the handler and the JSON serialization and writes the stored bytes, which at 256 KB costs less than serializing the payload.
- New key: CSE is faster than IdempotentAPI and allocates less on every TFM, at both sizes. At 256 KB on net10.0: 189.84 μs and 796.12 KB against 5,170.61 μs and 2272.98 KB.
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
| `NoIdempotency` | 9.993 μs (1.00), 9.47 KB | 9.263 μs (1.00), 9.34 KB | 9.695 μs (1.00), 10.77 KB |
| `NewKey` | 25.282 μs (2.53), 14.36 KB, noisy (CV 6.2%) | 20.320 μs (2.19), 14.13 KB | 21.487 μs (2.22), 15.69 KB |
| `Replay` | 14.720 μs (1.47), 11.34 KB | 12.781 μs (1.38), 11.19 KB | 13.388 μs (1.38), 12.6 KB |

| Method | BodySize | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `Plain` | 1024 | 10.68 μs (1.00), 9.5 KB, noisy (CV 6.8%) | 9.323 μs (1.00), 9.38 KB | 9.169 μs (1.00), 10.79 KB |
| `KeyedReplay` | 1024 | 15.23 μs (1.43), 11.37 KB | 13.763 μs (1.48), 11.22 KB | 13.258 μs (1.45), 12.62 KB |
| `KeyedNewKey` | 1024 | 26.30 μs (2.47), 14.42 KB | 22.200 μs (2.38), 14.15 KB | 22.644 μs (2.47), 15.78 KB |
| `Plain` | 65536 | 23.69 μs (1.00), 12.52 KB | 22.154 μs (1.00), 12.39 KB | 23.945 μs (1.00), 13.72 KB |
| `KeyedReplay` | 65536 | 147.95 μs (6.24), 15.32 KB | 136.080 μs (6.14), 15.17 KB | 137.100 μs (5.73), 16.47 KB |
| `KeyedNewKey` | 65536 | 168.85 μs (7.13), 18.95 KB | 156.267 μs (7.05), 18.8 KB | 160.884 μs (6.72), 20.77 KB |
| `Plain` | 1048576 | 130.40 μs (1.00), 38.88 KB | 127.180 μs (1.00), 38.75 KB | 127.720 μs (1.00), 41.43 KB |
| `KeyedReplay` | 1048576 | 1,285.99 μs (9.86), 52.46 KB | 1,250.851 μs (9.84), 52.31 KB | 1,263.592 μs (9.89), 54.56 KB |
| `KeyedNewKey` | 1048576 | 1,373.08 μs (10.53), 56.04 KB | 1,331.182 μs (10.47), 55.79 KB | 1,353.084 μs (10.60), 58.78 KB |

- With a small body, a replay costs 1.38 to 1.47 of the route without idempotency, and a new key 2.19 to 2.53.
- The fingerprint reads and hashes the whole request body before the store lookup, so the cost grows with the request size: a keyed replay is 1.43 to 1.48 of the plain request at 1 KB, 5.73 to 6.25 at 64 KB and 9.84 to 9.89 at 1 MB. Allocation stays at 1.17 to 1.35 of the plain request, because the body goes through a pooled 16 KB buffer, and above 30 KB the default `EnableBuffering` threshold puts it in a temporary file.

### Where CSE loses

- **`ToUuidV5` against the hand-written UUIDv5, all TFMs:** 1,400.02 against 733.36 ns (net8.0), 1,305.69 against 736.82 ns (net9.0), 891.03 against 582.37 ns (net10.0), 885.82 against 587.74 ns (net11.0), with 456 to 488 B allocated against none.
- **Executor against the hand-written dedupe:** a CSE replay is 129.085 to 181.050 ns with 152 B, the `ConcurrentDictionary` lookup 12.380 to 46.223 ns with none. The CSE row does more (lease, fingerprint, retention), but the gap is real.
- **Large keyed requests:** a keyed replay with a 64 KB body takes 136.080 μs against 22.154 μs for the plain request on net10.0, and 1,250.851 μs against 127.180 μs at 1 MB.

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
  - On the HTTP path, a new key captures the response in an `ArrayPool` buffer in `ResponseCaptureStream`, then `IdempotencyEntrySerializer` and the distributed store each build one exact-size entry. The NewKey 256 KB rows allocate 521 to 528 KB more than the plain endpoint, about two times the response size.
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
