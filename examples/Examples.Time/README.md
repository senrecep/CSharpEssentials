# CSharpEssentials.Time example

A console app for [CSharpEssentials.Time](../../CSharpEssentials.Time/Readme.MD): the injectable clock, both test fakes, time zone lookup and the date helpers.

## Run

```bash
dotnet run --project examples/Examples.Time
```

The first section reads the real clock, so its output changes per run. Every other section uses a fake clock and prints the same values each time.

## What each section shows

| Section | Shows |
|---|---|
| DateTimeProvider over the system clock | `UtcNow`, `UtcNowDateTime`, `UtcNowDate`, `UtcNowTime`, `TimeZone`, `TimeZoneUtc` |
| FakeDateTimeProvider in a test | A `TrialService` that takes `IDateTimeProvider`; the test moves time with `Advance` and `SetTime` |
| DateTimeProvider over FakeTimeProvider | One fake drives the clock and `Task.Delay`; `TimeZone` follows `SetLocalTimeZone` |
| Time zones | `TimeZones.Find` with an IANA and a Windows id, `TryFind`, `NowIn`, `TodayIn` |
| Date helpers | `NextDayOfWeek`, `PreviousDayOfWeek` (with `includeCurrent`), `ToDateOnly`, `ToTimeOnly`, the three `GetAge` overloads and its exception |

`FakeTimeProvider` comes from `Microsoft.Extensions.TimeProvider.Testing`, which the project references.
