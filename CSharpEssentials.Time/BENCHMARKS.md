# CSharpEssentials.Time: performance benchmarks

**Status: draft.** The suite builds on net8.0 to net11.0, and its parity self-check passes on every TFM. No full default-job run has been done, so this page has no numbers yet. Short-job (`--quick`) numbers are not published.

CSharpEssentials.Time (CSE) is compared with the BCL: `TimeProvider`, `DateTimeOffset`, `DateTime`, `DateOnly` and `TimeOnly`. There is no competitor library, only BCL and hand-written baselines. The BCL has no weekday or age API, so those rows are hand-written code that does the same arithmetic.

The suite lives in `benchmarks/CSharpEssentials.Baselines.Benchmarks/`. It shares that project with the Clone benchmarks ([CSharpEssentials.Clone/BENCHMARKS.md](../CSharpEssentials.Clone/BENCHMARKS.md)).

---

## What is measured

| Class | Measured unit | Rows | Label |
|---|---|---|---|
| `ClockComparison`, category UtcNow | One read of the current UTC instant | CSE `IDateTimeProvider.UtcNow` (baseline), BCL `TimeProvider.System.GetUtcNow()` | Like-for-like |
| `ClockComparison`, category UtcNow | Same | BCL `DateTimeOffset.UtcNow` | Like-for-like on output. Not testable: no abstraction to swap in a fake clock |
| `ClockComparison`, category UtcNow | Same | CSE `DateTimeProvider.UtcNow` through the sealed class | Reference row. It shows the cost of the interface call |
| `ClockComparison`, category UtcNowDateTime | One read as a `DateTime` with `Kind = Utc` | CSE `UtcNowDateTime` (baseline), BCL `GetUtcNow().UtcDateTime`, BCL `DateTime.UtcNow` | Like-for-like |
| `ClockComparison`, category UtcNowDate (.NET 9+) | One read as a `DateOnly` | CSE `UtcNowDate` (baseline), BCL `DateOnly.FromDateTime(GetUtcNow().UtcDateTime)` | Like-for-like for a zero-offset provider such as `TimeProvider.System`. See semantic differences |
| `ClockComparison`, category UtcNowTime (.NET 9+) | One read as a `TimeOnly` | CSE `UtcNowTime` (baseline), BCL `TimeOnly.FromDateTime(GetUtcNow().UtcDateTime)` | Same as UtcNowDate |
| `TimeConversionComparison`, category ToDateOnly (.NET 9+) | One `DateTime` to `DateOnly` | CSE `ToDateOnly` (baseline), BCL `DateOnly.FromDateTime` | Like-for-like. CSE forwards to the BCL |
| `TimeConversionComparison`, category ToTimeOnly (.NET 9+) | One `DateTime` to `TimeOnly` | CSE `ToTimeOnly` (baseline), BCL `TimeOnly.FromDateTime` | Like-for-like. CSE forwards to the BCL |
| `TimeConversionComparison`, category NextDayOfWeekDateTime | Next given weekday after a `DateTime` | CSE `NextDayOfWeek` (baseline), hand-written modulo and `AddDays` | Hand-written equivalent. The BCL has no such API |
| `TimeConversionComparison`, category NextDayOfWeekDateOnly (.NET 9+) | Next given weekday after a `DateOnly` | CSE `NextDayOfWeek` (baseline), hand-written modulo and `AddDays` | Hand-written equivalent |
| `TimeConversionComparison`, category PreviousDayOfWeekDateOnly (.NET 9+) | Previous given weekday before a `DateOnly` | CSE `PreviousDayOfWeek` (baseline), hand-written modulo and `AddDays` | Hand-written equivalent |
| `TimeConversionComparison`, category GetAge (.NET 9+) | Age in whole years on a fixed reference date | CSE `GetAge(DateOnly)` (baseline), hand-written year difference with an `AddYears` check | Same output. CSE also checks that the birth date is not in the future |

`includeCurrent` keeps its default `false` in every weekday row. `PreviousDayOfWeek(DateTime)` is left out. It is the mirror of `NextDayOfWeek(DateTime)` with the same arithmetic.

## Methodology

- Every class has `[MemoryDiagnoser]`. CSE is the baseline in each category. A doc URL sits above every row: the package Readme for CSE rows and learn.microsoft.com for BCL and hand-written rows.
- CSE is used the way the Readme shows it. `DateTimeProvider` wraps `TimeProvider.System` and is held as `IDateTimeProvider`, the type dependency injection hands out.
- Clock reads are not batched. A single clock read is long enough to measure on its own.
- Conversions are batched. Each row loops over the same 1,024 fixed inputs, folds the results into a `long` checksum and returns it. `OperationsPerInvoke = 1024`, so the reported time is per conversion. Without the loop a single conversion would sit near the timer resolution.
- Inputs are built once from `new Random(42)`: `DateTime` values with `Kind = Utc` between 1990 and 2040, target weekdays cycling through all seven days, and birth dates moved 50 years back when they fall after the reference date 2026-11-04.
- The JIT may devirtualize the interface call. On net8 and later, Dynamic PGO adds guarded devirtualization, so the gap between the interface row and the sealed row can be close to zero. That is a finding, not a broken row, because every row still reads the clock.
- net8.0 runs the netstandard2.1 build of CSE, because the package has no net8 target. That build has no `DateOnly` or `TimeOnly` members. On net8 only the UtcNow, UtcNowDateTime and NextDayOfWeekDateTime categories exist.

## What the parity check enforces

`[GlobalSetup]` runs the check before every run, and `--self-check` runs it without BenchmarkDotNet. A failure throws and the run stops.

- Clock values change between calls, so they cannot be compared for equality. Setup reads `DateTimeOffset.UtcNow` before and after calling every row. Each `DateTimeOffset` row must have a zero offset and lie between the two reads. Each `DateTime` row must have `Kind = Utc` and lie between the two reads. Each `DateOnly` row must equal the date of one of the two reads. Each `TimeOnly` row must lie between the two times of day, with a wrap past midnight allowed.
- Conversion rows must all return the checksum of a brute-force loop computed in setup. The weekday loop steps one day at a time until the weekday matches. The age loop compares month and day. The `DateOnly` loop builds the date from year, month and day, and the `TimeOnly` loop reads `TimeOfDay`.

## Pinned versions

| Package | Version | License |
|---|---|---|
| BenchmarkDotNet | `[0.14.0,)` from `Directory.Packages.props`, resolves 0.14.0 | MIT |
| Microsoft.Bcl.TimeProvider | `[8.0.0,)`, resolves 8.0.0. Used only by the netstandard2.1 build. On net8 it forwards to the BCL `TimeProvider` | MIT |

There is no competitor library to pin. The baselines are the BCL and hand-written code.

## Feature matrix

From reading the source and docs, not measured.

| Feature | BCL (`TimeProvider`, `DateTimeOffset`, `DateOnly`, `TimeOnly`) | CSharpEssentials.Time |
|---|---|---|
| Current UTC instant | `TimeProvider.System.GetUtcNow()`, `DateTimeOffset.UtcNow` | `IDateTimeProvider.UtcNow` over a `TimeProvider` |
| UTC `DateTime` with `Kind = Utc` | `DateTime.UtcNow`, `GetUtcNow().UtcDateTime` | `UtcNowDateTime` |
| Current UTC date and time of day | `DateOnly.FromDateTime(...)`, `TimeOnly.FromDateTime(...)` | `UtcNowDate`, `UtcNowTime` (.NET 6+ builds only) |
| Mockable clock for tests | `TimeProvider` subclass. `FakeTimeProvider` ships in Microsoft.Extensions.TimeProvider.Testing (not measured) | `FakeDateTimeProvider` with `Advance` and `SetTime` (not thread-safe, `TimeZone` fixed to UTC), or `DateTimeProvider` over any test `TimeProvider` such as `FakeTimeProvider` |
| Timers and delays on the clock | `TimeProvider.CreateTimer`, `Task.Delay(TimeSpan, TimeProvider)` | None |
| Local time zone | `TimeProvider.LocalTimeZone` | `TimeZone` (`TimeZoneInfo.Local`), `TimeZoneUtc` |
| `DateTime` to `DateOnly` or `TimeOnly` | `DateOnly.FromDateTime`, `TimeOnly.FromDateTime` | `ToDateOnly`, `ToTimeOnly` (forwarders) |
| Next or previous weekday | None | `NextDayOfWeek`, `PreviousDayOfWeek` (`DateTime` on every build, `DateOnly` on .NET 6+) |
| Age in whole years | None | `GetAge(DateOnly)`, `GetAge(IDateTimeProvider)` |

## Semantic differences

- `DateTimeProvider.UtcNowDate` and `UtcNowTime` use `UtcNow.DateTime`, not `UtcNow.UtcDateTime` (`CSharpEssentials.Time/DateTimeProvider.cs:12-13`). With `TimeProvider.System` the offset is zero, so the result matches the BCL rows. With a `TimeProvider` that returns a non-zero offset, they return the local date and time of that offset, not UTC. This page describes it and does not change it.
- `GetAge` throws `ArgumentOutOfRangeException` when the birth date is after the reference date. The hand-written row has no such check. The inputs never trip it, so both rows return the same ages.
- On net8 the netstandard2.1 build runs. It has no `DateOnly` or `TimeOnly` members, so those rows do not exist there.

## Expected outcome

Read from the source. Check it against the full run.

- `ToDateOnly` and `ToTimeOnly` forward to the BCL, so CSE and the BCL should tie.
- The weekday helpers do the same modulo arithmetic as the hand-written rows, so they should tie too.
- `GetAge` adds one comparison for its guard. The gap should be small.
- `UtcNow` adds one interface call and one virtual call over `DateTimeOffset.UtcNow`. Dynamic PGO may remove most of that.

## Left out

- `FakeDateTimeProvider` against `FakeTimeProvider`. `FakeTimeProvider` ships in a Microsoft package, not in the BCL, so it falls outside a suite limited to BCL and hand-written baselines.
- `GetAge(IDateTimeProvider)`. It adds `TimeZoneInfo.ConvertTime` to the local zone, so its cost depends on the machine's time zone.

## Results

Pending the full default-job run (`./run-benchmarks.sh --project baselines`). The reports will be committed under `benchmarks/results/baselines/{tfm}/results/`.

## How to reproduce

```bash
cd benchmarks
./run-benchmarks.sh --project baselines                        # all TFMs (net8-net11), Time and Clone classes
./run-benchmarks.sh --project baselines --tfm net10.0          # one TFM
./run-benchmarks.sh --project baselines --quick --tfm net10.0  # one TFM, Short job
dotnet run -c Release --framework net10.0 --project CSharpEssentials.Baselines.Benchmarks -- --self-check
```

To run only the Time classes, call BenchmarkDotNet directly: `dotnet run -c Release --framework net10.0 --project CSharpEssentials.Baselines.Benchmarks -- --filter "*.ClockComparison.*" "*.TimeConversionComparison.*"`. `--quick` without `--tfm` runs on net9.0, where every row exists.
