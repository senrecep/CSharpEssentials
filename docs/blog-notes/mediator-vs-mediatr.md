# Blog notes: CSharpEssentials.Mediator, Mediator and MediatR

Raw material for a post. These notes are not the post. Every number comes from `CSharpEssentials.Mediator/BENCHMARKS.md`, which reads `benchmarks/results/mediator/{tfm}/results/*.csv` and `benchmarks/results/mediator/aot/aot-report.md`. The machine was an AMD Ryzen 5 5600 running a desktop session, with a 1-minute load average of 1.10-2.35 at the sampled points (net10 ran first, after a busier period: 5-minute average 4.73). The SDK was 11.0.100-rc.1, with BenchmarkDotNet 0.14.0. Primary runtime is .NET 10.0.11. Competitors: MediatR 12.5.0 and Mediator.SourceGenerator / Mediator.Abstractions 3.0.2.

## Framing

- CSharpEssentials.Mediator (CSE) is not a dispatcher. It has no `IMediator`, no generator and no handler discovery. It is pipeline behaviors on top of martinothamar/Mediator, so its dispatch numbers are Mediator's.
- Part A compares MediatR with Mediator (send, publish, startup, AOT). Part B compares CSE's `ValidationBehavior` + `ExceptionHandlingBehavior` with the same two behaviors written by hand on Mediator and on MediatR.
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
  - Not marked `IsAotCompatible`; the Readme says the behaviors use reflection.

## The numbers that matter (net10 unless stated)

1. Send. Mediator through `IMediator`: 7.799 ns (0 behaviors) and 31.110 ns (2 behaviors), 0 B. MediatR: 67.444 and 180.570 ns, 200 B and 584 B. The concrete class: 3.344 and 14.825 ns.
2. Publish. Mediator 12.465 / 30.013 ns (1 / 5 handlers), 0 B. MediatR 104.059 / 311.734 ns, 288 B / 896 B.
3. Cold startup (fresh process, 50 handlers, first send). MediatR 32.41 ms, Mediator 35.55 ms (1.10×). Mediator is slower on every TFM (1.07-1.10×). Warm startup goes the other way (228.0 vs 253.4 μs), but the warm unit hides MediatR's first-send reflection.
4. Behaviors, valid input. Hand-written Mediator 183.5 ns, CSE 236.2 ns (1.29×), hand-written MediatR 370.7 ns. CSE and hand-written Mediator both allocate 216 B; MediatR 880 B.
5. Behaviors, invalid input. Hand-written Mediator 262.9 ns / 712 B, CSE 439.8 ns / 1,032 B (1.67×), hand-written MediatR 428.7 ns / 1,192 B. CSE is the slowest column on every TFM; it allocates less than hand-written MediatR.
6. Native AOT (net10, linux-x64). Mediator: 0 IL warnings, 2.2 MB, runs. MediatR: 23 IL warnings, crashes with "A suitable constructor for type 'MediatR.Mediator' could not be located". CSE: crashes in both samples (findings a and b below).

## Where CSE loses (state it, do not soften it)

- With valid input CSE takes 1.16-1.53× the time of hand-written Mediator on every TFM (+52.7 ns on net10, no extra allocation). The cause was not profiled.
- With invalid input CSE takes 1.67-1.84× the time of hand-written Mediator and 1.03-1.16× that of hand-written MediatR on every TFM, and allocates 320 B more than hand-written Mediator on net10.
- Known findings, not fixed in this work:
  - (a) `AddValidator<T, TValidator>` (`CSharpEssentials.Validation/Extensions/ValidatorServiceCollectionExtensions.cs:34`) lacks `[DynamicallyAccessedMembers(PublicConstructors)]` on `TValidator` (IL2087). Under Native AOT the validator's constructor is trimmed and startup throws.
  - (b) `BehaviorCache.cs:46` finds `Result<T>.Failure` with `GetMethod` (IL2070). Under Native AOT the invalid-input path throws "Method Failure not found".
  - (c) The failure path does extra work: the errors are copied at `ValidationBehavior.cs:76`, then again in `ResultT.cs:39` because the factory binds the `IEnumerable<Error>` overload instead of the copy-free `Failure(Error[])`; a generic type check runs on every failure (`ValidationBehavior.cs:211-212`); the result is boxed in the factory and unboxed at `ValidationBehavior.cs:220`. Not profiled item by item.

## Where Mediator loses

- Cold startup, 1.07-1.10× MediatR on every TFM.
- With 2 behaviors on net8 and net9, the concrete class is slower than `IMediator` (26.793 vs 18.630 ns, 25.261 vs 17.105 ns). Not investigated.

## Caveats to keep in the post

- Part B resolves behaviors from the root provider. With Mediator's singleton default the scoped validation behavior and validator are captured once; this throws with `validateScopes: true`. Real apps that resolve per request pay scoped resolution that these rows do not include.
- The cold-start allocation column comes from BenchmarkDotNet's second in-process invocation, not from the timed first call.
- Send and Publish use pass-through behaviors and trivial handlers: dispatch overhead only.
- AOT warning counts use `TrimmerSingleWarn=false`. No sample roots assemblies; whether that rescues MediatR was not tested. The 2 ms AOT wall time was taken at load 2.50 and is not a result.
- Outliers not investigated: net10 `IMediator` with 2 behaviors (31.110 ns against 17.105-21.272 ns elsewhere, net10 ran at the highest load); net8 Mediator publish 3.2-4.9× its net10/net11 values; net8 CSE valid 1.53×.
- The net8 column runs CSE's netstandard2.1 build.
- The MediatR wiki tracks the current release; the feature comparison cites it only where the 12.5.0 source agrees.

## Possible angles

- "A generated dispatcher, then the behaviors on top": Mediator's 0 B sends against MediatR's per-send allocation, and what CSE adds back on top of that.
- "Where CSE loses": valid input against hand-written Mediator, invalid input against both, and the two AOT crashes. Name the losses first.
- "Cold start is not free for generated code": Mediator 1.07-1.10× MediatR from a fresh process, while the warm numbers say the opposite.
