# Blog notes: CSharpEssentials.Mediator, Mediator and MediatR

Raw material for a post. These notes are not the post. Every number comes from `CSharpEssentials.Mediator/BENCHMARKS.md`, which reads `benchmarks/results/mediator/{tfm}/results/*-report-github.md` and `benchmarks/results/mediator/aot/aot-report.md`. Competitors: MediatR 12.5.0, Mediator.SourceGenerator / Mediator.Abstractions 3.0.2 and AsyncKeyedLock 8.1.2.

## How these numbers were produced

- BenchmarkDotNet v0.14.0, default job (ColdStart job with 15 launches for cold startup), full runs of all twelve classes on 2026-11-05.
- 6-core, 12-thread x64 desktop CPU, CachyOS, SDK 11.0.100-rc.1. TFMs net8.0, net9.0, net10.0 and net11.0, run in that order, one after another. Primary runtime is .NET 10.0.11.
- Desktop session in the background; 1-minute load average 1.90 at the start of the suite and 1.63 at the end.
- Rows above 5% CV were rerun up to two times; no row of this suite stayed above 5%.
- The Native AOT report is a separate script run from 2026-11-06, after the CSE changes described below.

## Framing

- CSharpEssentials.Mediator (CSE) is not a dispatcher. It has no `IMediator`, no generator and no handler discovery. It is pipeline behaviors on top of martinothamar/Mediator, so its dispatch numbers are Mediator's.
- Part A compares MediatR with Mediator (send, publish, startup, AOT). Part B compares CSE's `ValidationBehavior` + `ExceptionHandlingBehavior` with the same two behaviors written by hand on Mediator and on MediatR. Section 7 does the same for validator fan-out, validation scope, idempotency, lock, caching and logging.
- MediatR is pinned to 12.5.0, the last Apache-2.0 release. The latest stable, 14.2.0, is dual-licensed (RPL-1.5 or a commercial license) and expects a license key, so it was not used.
- Each library keeps its documented default lifetime: MediatR transient, Mediator singleton. The concrete generated `Mediator` class column follows Mediator's own benchmarks; applications that inject `IMediator` get the `IMediator` row.

## What each library provides

- MediatR 12.5.0:
  - Runtime assembly scan for handlers; the first send per request type builds a wrapper with `MakeGenericType` + `Activator.CreateInstance` and caches it statically.
  - Behaviors through `AddBehavior` / `AddOpenBehavior`; pre/post and exception processors; two notification publishers; streams.
  - No validation, logging, caching or transaction behaviors. No `IsAotCompatible`, no trim annotations.
- Mediator 3.0.2:
  - A source generator writes the registrations and a concrete `Mediator` class with one `Send` overload per message type; a missing handler is a build error.
  - Behaviors through DI or the ordered `options.PipelineBehaviors` array (the AOT path). `IsAotCompatible=true`.
  - Its validation and logging behaviors are README examples, not shipped types.
- CSE 7.0.0:
  - Validation (with modes and failure observers), logging, exception-to-`Result`, caching, transactions, resource locks, idempotency, all returning `Result` / `Result<T>` failures.
  - Not marked `IsAotCompatible`. Failure responses are built without reflection; caching, logging and idempotency serialize with reflection-based `System.Text.Json`.

## The numbers that matter (net10 unless stated)

1. Send. Mediator through `IMediator`: 7.761 ns (0 behaviors) and 26.152 ns (2 behaviors), 0 B. MediatR: 68.270 and 185.857 ns, 200 B and 584 B. The concrete class: 2.886 and 13.967 ns.
2. Publish. Mediator 12.300 / 30.369 ns (1 / 5 handlers), 0 B. MediatR 103.404 / 301.915 ns, 288 B / 896 B.
3. Cold startup (fresh process, 50 handlers, first send). MediatR 33.06 ms, Mediator 36.56 ms (Ratio 1.11). Mediator is slower on net8, net10 and net11; net9 is a tie (32.17 vs 30.73 ms, inside the error margins). Warm startup goes the other way on net8 to net10 (net10: 216.8 vs 255.6 μs), but on net11 Mediator is slower there too (239.6 vs 216.3 μs).
4. Behaviors, valid input. Hand-written Mediator 198.0 ns, CSE 163.7 ns (Ratio 0.83), hand-written MediatR 329.6 ns. CSE and hand-written Mediator both allocate 216 B; MediatR 880 B. CSE is faster than hand-written Mediator on net9 to net11 and slower on net8 (199.9 vs 195.9 ns).
5. Behaviors, invalid input. Hand-written Mediator 273.6 ns / 712 B, CSE 303.9 ns / 752 B (Ratio 1.11), hand-written MediatR 403.3 ns / 1192 B. CSE is slower than hand-written Mediator on every TFM and faster than hand-written MediatR on every TFM.
6. Validation scope (what a request scope pays to build the behavior). CSE 481.4 ns / 976 B, hand-written Mediator 554.6 ns / 1232 B, hand-written MediatR 530.9 ns / 1232 B. CSE is the fastest row on every TFM.
7. Lock, one uncontended send. CSE 167.5 ns / 168 B, hand-written over AsyncKeyedLock 187.1 ns / 48 B (Mediator) and 339.9 ns / 520 B (MediatR).
8. Lock contention, 8 workers, per operation. One shared key: AsyncKeyedLock 772.06 ns, CSE 779.29 ns (a tie). One key per worker: 409.75 vs 340.94 ns. A new key per operation: 391.88 vs 338.65 ns. CSE allocates more in every row.
9. Caching. Hit: hand-written Mediator 490.1 ns, CSE 502.0 ns, hand-written MediatR 636.0 ns. Miss: 572.5, 583.1 and 863.3 ns.
10. Logging. With a disabled logger and no payload: 103.7 ns hand-written Mediator, 109.3 ns CSE, 229.8 ns hand-written MediatR. With an enabled logger and JSON payloads: 1,039.8, 1,042.5 and 1,308.2 ns.
11. Native AOT (net10, linux-x64, report from 2026-11-06). Mediator: 0 IL warnings, 2.2 MB, runs. MediatR: 23 IL warnings, crashes with "A suitable constructor for type 'MediatR.Mediator' could not be located". CSE: both samples publish with 0 IL warnings, 2.5 MB, and run. In the earlier 2026-11-02 report both crashed; the two causes are the first two items under "What changed in CSE".

## Where CSE loses (state it, do not soften it)

This is a summary. The full per-row, per-TFM list is in `CSharpEssentials.Mediator/BENCHMARKS.md`, sections 5 and 7.

- Part B, invalid input: slower than hand-written Mediator on every TFM (Ratio 1.09-1.13; net10 303.9 vs 273.6 ns). Allocates 40 B more on net10 and net11 and 48 B more on net9.
- Part B, valid input on net8 (netstandard2.1 build): 199.9 vs 195.9 ns against hand-written Mediator.
- Validator fan-out (three validators): slower than hand-written Mediator and allocates more in every row on every TFM (net10 Ratio 1.37-1.93). With invalid input it is also slower than hand-written MediatR on every TFM (net10, one group: 1,157.9 vs 821.5 ns). CSE groups by `Order`, goes through an async task per group, removes duplicate errors with `Distinct` and checks the failure observers. Not profiled.
- Idempotency: slower than both hand-written stores in most rows. The worst row is a failing send that releases its key, with a supplied fingerprint: 1,627.9 ns against 300.7 ns (hand-written Mediator) and 447.3 ns (hand-written MediatR) on net10, 763.5 ns on net11. The hand-written stores are a lower bound with none of CSE's lease, scope, mode or metrics features, but the size of this gap is not explained.
- Lock: 168 B per uncontended send against 48 B for hand-written AsyncKeyedLock. Under contention CSE allocates 303-305 B per operation against 220-226 B with one key, and 170 B against 50 B with many keys. With one shared key it is slower than AsyncKeyedLock on net9 and net11.
- Caching: 1.02-1.09 of hand-written Mediator in every row, same allocation.
- Logging: slower than hand-written Mediator with a disabled logger on net9 and on net10 without payloads (109.3 vs 103.7 ns). On net8 CSE allocates 40 B more in every row (a `Stopwatch` in the netstandard2.1 build).

## Where Mediator loses

- Cold startup: Ratio 1.10, 1.11 and 1.09 against MediatR on net8, net10 and net11 (net9 is a tie).
- Warm startup on net11: 239.6 vs 216.3 μs.
- With 2 behaviors on net8 and net9, the concrete class is slower than `IMediator` (26.860 vs 19.735 ns, 25.241 vs 17.533 ns). Not investigated.

## What changed in CSE since the earlier run

- The failure path no longer uses reflection: `FailureResponse<TResponse>.Factory` is a static `IFailureFactory<TResponse>` that takes the `Error[]` directly.
- `AddValidator<T, TValidator>` carries `[DynamicallyAccessedMembers(PublicConstructors)]`.
- With one validator, Enforce mode and no listening observer, a failure returns without an async state machine.
- `CachingBehavior` reads `CacheKey` once per send, and `LoggingBehavior` keeps the type names in static fields.
- The default idempotency fingerprint hashes the JSON as a stream on net9 and later, so its allocation no longer grows with the request (1184 B for both the normal and the 4096-character request on net10).
- The earlier AOT crashes (2026-11-02 report) came from the first two items. In the 2026-11-06 rerun both CSE samples run with 0 IL warnings. The samples cover validation and exception handling only; caching, logging and idempotency still serialize with reflection-based `System.Text.Json`.

## Caveats to keep in the post

- Part B and section 7 resolve behaviors from the root provider. With Mediator's singleton default the scoped behaviors and validators are captured once; this throws with `validateScopes: true`. Real apps that resolve per request pay scoped resolution that these rows do not include. The validation scope class measures that cost on its own.
- Each CSE container registers only the behavior under test. A real app that follows the Readme also registers `AddMediatorBehaviors()`.
- The cold-start allocation column comes from BenchmarkDotNet's second in-process invocation, not from the timed first call.
- Send and Publish use pass-through behaviors and trivial handlers: dispatch overhead only.
- The idempotency hand-written stores and the hand-written contention row are reference points, not feature matches. The contention row never removes a semaphore.
- AOT warning counts use `TrimmerSingleWarn=false`. No sample roots assemblies; whether that rescues MediatR was not tested. The 2 and 3 ms AOT wall times were taken at load 4.70 and are not a result.
- Outliers not investigated: `IMediator` with 2 behaviors on net10 and net11 (26.152 and 31.187 ns against 17.533-19.735 ns on net8 and net9); net8 Mediator publish; CSE idempotency on net11 (763.5 ns against 1,627.9 ns on net10); every one-key contention row on net11 is about half its other TFM values.
- The net8 column runs the netstandard2.1 build of every CSE package. Logging, locking and the default fingerprint take different code there.
- The MediatR wiki tracks the current release; the feature comparison cites it only where the 12.5.0 source agrees.

## Possible angles

- "A generated dispatcher, then the behaviors on top": Mediator's 0 B sends against MediatR's per-send allocation, and what CSE adds back on top of that.
- "Where CSE loses": invalid input against hand-written Mediator, validator fan-out, idempotency on the release path, lock allocation. Name the losses first.
- "Cold start is not free for generated code": Mediator is slower than MediatR from a fresh process on three of four TFMs, while the warm numbers mostly say the opposite.
- "Behaviors you would otherwise write yourself": idempotency, locking and caching have no counterpart in either dispatcher. Compare with the hand-written lower bound and say what the extra cost buys.
- "A keyed lock, two ways": CSE's lock against AsyncKeyedLock under no contention, full contention and key churn.
