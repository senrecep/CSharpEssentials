# CSharpEssentials.Idempotency: performance benchmarks (draft)

This page compares the CSE idempotency stack with IdempotentAPI 2.6.0 (with IdempotentAPI.MinimalAPI 3.2.0 and IdempotentAPI.Cache.DistributedCache 2.2.0) for an HTTP replay, and measures the transport-independent executor, inbox, record stores and deterministic keys against hand-written baselines. The HTTP rows use the middleware and endpoint filter of CSharpEssentials.AspNetCore on top of this package. The suite uses BenchmarkDotNet v0.14.0 and targets .NET 8, 9, 10 and 11.

The classes live in `benchmarks/CSharpEssentials.AspNetCore.Comparison.Benchmarks/` (suite key `aspnetcore`), next to the classes documented in [CSharpEssentials.AspNetCore/BENCHMARKS.md](../CSharpEssentials.AspNetCore/BENCHMARKS.md) and [CSharpEssentials.RequestResponseLogging/BENCHMARKS.md](../CSharpEssentials.RequestResponseLogging/BENCHMARKS.md). The older `benchmarks/CSharpEssentials.Idempotency.Benchmarks/` suite (middleware overhead and fingerprint buffering, net9-net11) is unchanged.

**Status: draft.** The suite builds, and its parity self-check passes on every TFM. No full default-job run has been done, so this page has no numbers yet (see [Results](#results)).

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

**Pending the full default-job run.** The numbers will come from the default BenchmarkDotNet job on net8.0, net9.0, net10.0 and net11.0, and the reports will be committed under `benchmarks/results/aspnetcore/{tfm}/results/`. Short-job (`--quick`) numbers are not published here.

---

## Caveats

- **Expected CSE costs, from reading the code** (not yet measured):
  - `IdempotencyExecutor.cs:58`: an interpolated string is built on every call, before the mode check, only to name the operation in a possible error.
  - `InMemoryIdempotencyRecordStore.cs:65` with `IdempotencyRecordReservation.cs:43-44`: a new reservation object per replay.
  - `InMemoryIdempotencyRecordStore.cs:47`: a `Guid` token string and an `Entry` are allocated before the lookup, also when the key already exists.
  - `InMemoryIdempotencyRecordStore.cs:82`: `payload.ToArray()` copies the payload before the token check.
  - `IdempotencyInbox.cs:51`: `"messaging:" + consumer` is concatenated per message.
  - `DistributedCacheIdempotencyRecordStore.cs:52,83,103`: the storage key is rebuilt on every call.
  - `Internal/RecordEntrySerializer.cs:13-39`: `MemoryStream`/`BinaryWriter` copies per entry.
  - `IdempotencyKeys.cs:113-119`: four strings per hashed key; `:79-94`: several arrays and an `IncrementalHash` per UUIDv5.
  - On the HTTP path, the response is captured through `ResponseCaptureStream` and `IdempotencyEntrySerializer` (about four copies).
- **The contention class is noisy.** It depends on the core count and the scheduler; compare it only on the same machine.
- **TestServer, not Kestrel**, for the HTTP class.
- The CSE column is the working tree on branch `perf/bench-aspnetcore`, which may not match the version published on NuGet.

---

## How to reproduce

```bash
cd benchmarks
./run-benchmarks.sh --project aspnetcore                       # all TFMs, every class of the suite, default job
dotnet run -c Release -f net10.0 --project CSharpEssentials.AspNetCore.Comparison.Benchmarks -- --self-check
```

Results are stored per framework under `benchmarks/results/aspnetcore/{tfm}/results/`.
