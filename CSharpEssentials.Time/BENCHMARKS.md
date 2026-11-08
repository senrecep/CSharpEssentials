# CSharpEssentials.Time: performance benchmarks

Full default-job results for net8.0, net9.0, net10.0 and net11.0 are in [Results](#results). The parity self-check passes on every TFM. Short version: the CSE clock reads cost the same as `TimeProvider.System` (ratios 0.99 to 1.01), and `DateTime.UtcNow` is faster than both (ratio 0.95 to 0.96 against CSE). The conversions and weekday helpers are level with the BCL and hand-written code on most rows. Every row where CSE is slower is listed in [Where CSE loses](#where-cse-loses).

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
| `ClockComparison`, category UtcNowDate (.NET 9+) | One read as a `DateOnly` | CSE `UtcNowDate` (baseline), BCL `DateOnly.FromDateTime(GetUtcNow().UtcDateTime)` | Like-for-like |
| `ClockComparison`, category UtcNowTime (.NET 9+) | One read as a `TimeOnly` | CSE `UtcNowTime` (baseline), BCL `TimeOnly.FromDateTime(GetUtcNow().UtcDateTime)` | Same as UtcNowDate |
| `TimeConversionComparison`, category ToDateOnly (.NET 9+) | One `DateTime` to `DateOnly` | CSE `ToDateOnly` (baseline), BCL `DateOnly.FromDateTime` | Like-for-like. CSE forwards to the BCL |
| `TimeConversionComparison`, category ToTimeOnly (.NET 9+) | One `DateTime` to `TimeOnly` | CSE `ToTimeOnly` (baseline), BCL `TimeOnly.FromDateTime` | Like-for-like. CSE forwards to the BCL |
| `TimeConversionComparison`, category NextDayOfWeekDateTime | Next given weekday after a `DateTime` | CSE `NextDayOfWeek` (baseline), hand-written modulo and `AddDays` | Hand-written equivalent. The BCL has no such API |
| `TimeConversionComparison`, category NextDayOfWeekDateOnly (.NET 9+) | Next given weekday after a `DateOnly` | CSE `NextDayOfWeek` (baseline), hand-written modulo and `AddDays` | Hand-written equivalent |
| `TimeConversionComparison`, category PreviousDayOfWeekDateOnly (.NET 9+) | Previous given weekday before a `DateOnly` | CSE `PreviousDayOfWeek` (baseline), hand-written modulo and `AddDays` | Hand-written equivalent |
| `TimeConversionComparison`, category GetAge (.NET 9+) | Age in whole years on a fixed reference date | CSE `GetAge(DateOnly)` (baseline), hand-written year difference with an `AddYears` check | Same output. CSE also checks that the birth date is not in the future |

`includeCurrent` keeps its default `false` in every weekday row. `PreviousDayOfWeek(DateTime)` is left out. It is the mirror of `NextDayOfWeek(DateTime)` with the same arithmetic.

## Methodology

- Every class has `[MemoryDiagnoser]`. CSE is the baseline in each category. A doc URL sits above every row: the package Readme for CSE rows and the .NET API docs for BCL and hand-written rows.
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

- `DateTimeProvider.UtcNowDate` and `UtcNowTime` use `UtcNow.UtcDateTime` (`CSharpEssentials.Time/DateTimeProvider.cs:12-13`), the same instant as the BCL rows. With a `TimeProvider` that returns a non-zero offset they still return the UTC date and time of day.
- `GetAge` throws `ArgumentOutOfRangeException` when the birth date is after the reference date. The hand-written row has no such check. The inputs never trip it, so both rows return the same ages.

## Left out

- `FakeDateTimeProvider` against `FakeTimeProvider`. `FakeTimeProvider` ships in the Microsoft.Extensions.TimeProvider.Testing package, not in the BCL, so it falls outside a suite limited to BCL and hand-written baselines.
- `GetAge(IDateTimeProvider)`. It adds `TimeZoneInfo.ConvertTime` to the local zone, so its cost depends on the machine's time zone.

## Results

### How these numbers were produced

- BenchmarkDotNet v0.14.0, default job, full runs (no `--quick`), one suite at a time, on a 6-core, 12-thread x64 desktop CPU on Linux (AVX2). SDK and runtime builds are in the header of each report.
- TFMs: net8.0 (.NET 8.0.30, netstandard2.1 build of CSE), net9.0 (.NET 9.0.19), net10.0 (.NET 10.0.11) and net11.0 (.NET 11.0.0).
- The 1-minute load average stayed between 1.55 and 2.60 during the baselines suite. No row of `ClockComparison` or `TimeConversionComparison` has a CV above 5%.
- Reports: [`benchmarks/results/baselines/{tfm}/results/`](../benchmarks/results/baselines/), files `CSharpEssentials.Baselines.Benchmarks.ClockComparison-report-github.md` and `CSharpEssentials.Baselines.Benchmarks.TimeConversionComparison-report-github.md` (and `.csv`) per TFM. Every number below is copied from the `-report-github.md` files.

How to read the tables: each cell is `Mean (Ratio)` for that TFM. The ratio is against the CSE row of the same category and TFM. `n/a` means the row does not exist on that TFM. No row allocates on any TFM (Allocated is `-` in every report), so there is no allocation table. Conversion rows are per conversion (`OperationsPerInvoke = 1024`).

### Clock reads (`ClockComparison`)

| Category | Method | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|
| UtcNow | `CSharpEssentials_UtcNow` | 27.97 ns (1.00) | 27.96 ns (1.00) | 27.63 ns (1.00) | 27.67 ns (1.00) |
| UtcNow | `CSharpEssentials_SealedUtcNow` | 27.87 ns (1.00) | 27.95 ns (1.00) | 27.44 ns (0.99) | 27.20 ns (0.98) |
| UtcNow | `Bcl_TimeProviderGetUtcNow` | 28.22 ns (1.01) | 28.10 ns (1.00) | 27.44 ns (0.99) | 27.44 ns (0.99) |
| UtcNow | `Bcl_DateTimeOffsetUtcNow` | 27.61 ns (0.99) | 27.73 ns (0.99) | 27.06 ns (0.98) | 27.16 ns (0.98) |
| UtcNowDate | `CSharpEssentials_UtcNowDate` | n/a | 29.87 ns (1.00) | 29.50 ns (1.00) | 29.68 ns (1.00) |
| UtcNowDate | `Bcl_TimeProviderDateOnly` | n/a | 29.97 ns (1.00) | 29.76 ns (1.01) | 29.89 ns (1.01) |
| UtcNowDateTime | `CSharpEssentials_UtcNowDateTime` | 28.08 ns (1.00) | 27.95 ns (1.00) | 28.29 ns (1.00) | 28.23 ns (1.00) |
| UtcNowDateTime | `Bcl_TimeProviderUtcDateTime` | 28.18 ns (1.00) | 28.04 ns (1.00) | 28.24 ns (1.00) | 28.00 ns (0.99) |
| UtcNowDateTime | `Bcl_DateTimeUtcNow` | 26.55 ns (0.95) | 26.84 ns (0.96) | 26.84 ns (0.95) | 26.78 ns (0.95) |
| UtcNowTime | `CSharpEssentials_UtcNowTime` | n/a | 31.92 ns (1.00) | 30.52 ns (1.00) | 30.60 ns (1.00) |
| UtcNowTime | `Bcl_TimeProviderTimeOnly` | n/a | 32.01 ns (1.00) | 30.97 ns (1.01) | 30.87 ns (1.01) |

### Conversions and helpers (`TimeConversionComparison`)

| Category | Method | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|
| GetAge | `CSharpEssentials_GetAge` | n/a | 19.015 ns (1.00) | 11.368 ns (1.00) | 11.286 ns (1.00) |
| GetAge | `HandWritten_GetAge` | n/a | 18.717 ns (0.98) | 11.494 ns (1.01) | 11.210 ns (0.99) |
| NextDayOfWeekDateOnly | `CSharpEssentials_NextDayOfWeekDate` | n/a | 2.200 ns (1.00) | 2.126 ns (1.00) | 2.245 ns (1.00) |
| NextDayOfWeekDateOnly | `HandWritten_NextDayOfWeekDate` | n/a | 2.243 ns (1.02) | 2.143 ns (1.01) | 2.127 ns (0.95) |
| NextDayOfWeekDateTime | `CSharpEssentials_NextDayOfWeek` | 4.627 ns (1.00) | 6.377 ns (1.00) | 5.641 ns (1.00) | 5.387 ns (1.00) |
| NextDayOfWeekDateTime | `HandWritten_NextDayOfWeek` | 5.601 ns (1.21) | 7.098 ns (1.11) | 6.322 ns (1.12) | 5.386 ns (1.00) |
| PreviousDayOfWeekDateOnly | `CSharpEssentials_PreviousDayOfWeekDate` | n/a | 2.368 ns (1.00) | 2.191 ns (1.00) | 2.150 ns (1.00) |
| PreviousDayOfWeekDateOnly | `HandWritten_PreviousDayOfWeekDate` | n/a | 2.212 ns (0.93) | 2.171 ns (0.99) | 2.151 ns (1.00) |
| ToDateOnly | `CSharpEssentials_ToDateOnly` | n/a | 1.022 ns (1.00) | 1.022 ns (1.00) | 1.022 ns (1.00) |
| ToDateOnly | `Bcl_DateOnlyFromDateTime` | n/a | 1.021 ns (1.00) | 1.022 ns (1.00) | 1.021 ns (1.00) |
| ToTimeOnly | `CSharpEssentials_ToTimeOnly` | n/a | 1.088 ns (1.00) | 1.044 ns (1.00) | 1.040 ns (1.00) |
| ToTimeOnly | `Bcl_TimeOnlyFromDateTime` | n/a | 1.194 ns (1.10) | 1.039 ns (1.00) | 1.135 ns (1.09) |

### Where CSE loses

Rows with a ratio of 0.99 to 1.01 are treated as level. These rows are faster than CSE by more than that:

- `Bcl_DateTimeUtcNow` in UtcNowDateTime: 26.55 ns (0.95) on net8.0, 26.84 ns (0.96) on net9.0, 26.84 ns (0.95) on net10.0, 26.78 ns (0.95) on net11.0. It returns a `DateTime` with no clock abstraction, so it cannot be swapped for a fake clock in tests.
- `Bcl_DateTimeOffsetUtcNow` in UtcNow: 27.06 ns (0.98) on net10.0 and 27.16 ns (0.98) on net11.0. Same caveat: no abstraction.
- `HandWritten_PreviousDayOfWeekDate`: 2.212 ns (0.93) on net9.0.
- `HandWritten_NextDayOfWeekDate`: 2.127 ns (0.95) on net11.0.
- `HandWritten_GetAge`: 18.717 ns (0.98) on net9.0.

Level rows where the competitor's mean is still lower than the CSE row:

- `Bcl_DateTimeOffsetUtcNow`: 27.61 ns (0.99) against 27.97 ns on net8.0, 27.73 ns (0.99) against 27.96 ns on net9.0.
- `Bcl_TimeProviderGetUtcNow`: 27.44 ns (0.99) on net10.0 (CSE 27.63 ns) and net11.0 (CSE 27.67 ns).
- `Bcl_TimeProviderUtcDateTime`: 28.24 ns (1.00) against 28.29 ns on net10.0, 28.00 ns (0.99) against 28.23 ns on net11.0.
- `HandWritten_PreviousDayOfWeekDate`: 2.171 ns (0.99) against 2.191 ns on net10.0.
- `HandWritten_GetAge`: 11.210 ns (0.99) against 11.286 ns on net11.0.
- `HandWritten_NextDayOfWeek`: 5.386 ns (1.00) against 5.387 ns on net11.0.
- `Bcl_DateOnlyFromDateTime`: 1.021 ns (1.00) against 1.022 ns on net9.0 and net11.0.
- `Bcl_TimeOnlyFromDateTime`: 1.039 ns (1.00) against 1.044 ns on net10.0.

### Notes on the results

- `ToDateOnly` and `ToTimeOnly` forward to the BCL. `ToDateOnly` ties with the BCL on every TFM, and `ToTimeOnly` ties on net10.0. On net9.0 (1.10) and net11.0 (1.09) the BCL row is slower than the CSE row, although CSE only forwards to that same call. This is not a CSE win.
- The weekday helpers do the same modulo arithmetic as the hand-written rows. The `DateOnly` helpers are within 0.99 to 1.02 of the hand-written code, except the two losses above. The `DateTime` helper is faster than the hand-written row on net8.0 (1.21), net9.0 (1.11) and net10.0 (1.12) and level on net11.0 (1.00). The cause is not known.
- `GetAge` adds one comparison for its guard and is within 0.98 to 1.01 of the hand-written row.
- `UtcNow` adds one interface call and one virtual call over `DateTimeOffset.UtcNow`. In the UtcNow category every row is within 0.98 to 1.01 of the CSE interface row. The widest gap between the interface row and the sealed row is on net11.0: 27.67 ns against 27.20 ns (0.98).

## How to reproduce

```bash
cd benchmarks
./run-benchmarks.sh --project baselines                        # all TFMs (net8-net11), Time and Clone classes
./run-benchmarks.sh --project baselines --tfm net10.0          # one TFM
./run-benchmarks.sh --project baselines --quick --tfm net10.0  # one TFM, Short job
dotnet run -c Release --framework net10.0 --project CSharpEssentials.Baselines.Benchmarks -- --self-check
```

To run only the Time classes, call BenchmarkDotNet directly: `dotnet run -c Release --framework net10.0 --project CSharpEssentials.Baselines.Benchmarks -- --filter "*.ClockComparison.*" "*.TimeConversionComparison.*"`. `--quick` without `--tfm` runs on net9.0, where every row exists.
