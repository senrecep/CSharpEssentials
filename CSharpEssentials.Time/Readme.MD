# CSharpEssentials.Time

`IDateTimeProvider` is a clock interface built on .NET `TimeProvider`. Services read the time through it instead of `DateTime.UtcNow`, so tests can fix or move the time. The package also has a fake clock, an IANA and Windows time zone lookup, and `DateTime`/`DateOnly` helpers for weekdays and ages.

Benchmarks of the clock reads (`UtcNow`, `UtcNowDateTime`, `UtcNowDate`, `UtcNowTime`) against `TimeProvider.System` and `DateTimeOffset.UtcNow`, and of the DateOnly, TimeOnly, weekday and age helpers against the BCL and hand-written code, are in [BENCHMARKS.md](./BENCHMARKS.md).

## Install

```bash
dotnet add package CSharpEssentials.Time
```

Targets `net11.0`, `net10.0`, `net9.0` and `netstandard2.1` (which adds `Microsoft.Bcl.TimeProvider`). The meta-package `CSharpEssentials` includes it. The `DateOnly` and `TimeOnly` members (`UtcNowDate`, `UtcNowTime`, `ToDateOnly`, `ToTimeOnly`, the `DateOnly` weekday overloads, `TodayIn` and `GetAge`) are not in the `netstandard2.1` build.

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials.Time
using CSharpEssentials.Time;

var clock = new FakeDateTimeProvider(new DateTimeOffset(2025, 1, 15, 10, 0, 0, TimeSpan.Zero));
var invoices = new InvoiceService(clock);

Console.WriteLine($"{invoices.DueDate():yyyy-MM-dd}"); // 2025-02-14
clock.Advance(TimeSpan.FromDays(20));
Console.WriteLine($"{invoices.DueDate():yyyy-MM-dd}"); // 2025-03-06

IDateTimeProvider real = new DateTimeProvider(TimeProvider.System);
Console.WriteLine(real.UtcNow.Offset);        // 00:00:00

public sealed class InvoiceService(IDateTimeProvider clock)
{
    public DateOnly DueDate() => clock.UtcNowDate.AddDays(30);
}
```

In an app, register the clock once:

```csharp
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
```

## IDateTimeProvider and DateTimeProvider

| Member | Value |
|---|---|
| `UtcNow` | `DateTimeOffset` with offset zero |
| `UtcNowDateTime` | `DateTime` with `Kind = Utc` |
| `UtcNowDate` | `DateOnly` of the UTC date |
| `UtcNowTime` | `TimeOnly` of the UTC time of day |
| `TimeZone` | `DateTimeProvider`: the `LocalTimeZone` of the wrapped `TimeProvider` (`TimeZoneInfo.Local` for `TimeProvider.System`). `FakeDateTimeProvider`: UTC |
| `TimeZoneUtc` | `TimeZoneInfo.Utc` |

`DateTimeProvider` is a sealed wrapper around any `TimeProvider`: `TimeProvider.System` in production, `FakeTimeProvider` in tests. A `null` provider throws `ArgumentNullException`.

## Testing

There are two fakes. Pick by what the code under test does.

| Code under test | Fake |
|---|---|
| Only reads `IDateTimeProvider` | `FakeDateTimeProvider` (in this package) |
| Also waits, uses timers or `Task.Delay(TimeSpan, TimeProvider)` | `new DateTimeProvider(fakeTimeProvider)` with `FakeTimeProvider` from `Microsoft.Extensions.TimeProvider.Testing` |

```csharp Program.cs
// dotnet add package CSharpEssentials.Time
// dotnet add package Microsoft.Extensions.TimeProvider.Testing
using CSharpEssentials.Time;
using Microsoft.Extensions.Time.Testing;

var simple = new FakeDateTimeProvider(new DateTimeOffset(2025, 1, 15, 10, 0, 0, TimeSpan.Zero));
simple.Advance(TimeSpan.FromHours(2));
Console.WriteLine($"{simple.UtcNow:O}");          // 2025-01-15T12:00:00.0000000+00:00
simple.SetTime(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
Console.WriteLine($"{simple.UtcNowDate:yyyy-MM-dd}"); // 2026-01-01

var fake = new FakeTimeProvider(new DateTimeOffset(2025, 1, 15, 10, 0, 0, TimeSpan.Zero));
IDateTimeProvider clock = new DateTimeProvider(fake);
Task delay = Task.Delay(TimeSpan.FromMinutes(5), fake);
fake.Advance(TimeSpan.FromMinutes(5));            // moves the clock and fires the delay
await delay;
Console.WriteLine(clock.UtcNow.TimeOfDay);        // 10:05:00

fake.SetLocalTimeZone(TimeZones.Find("Europe/Istanbul"));
Console.WriteLine(clock.TimeZone.BaseUtcOffset);  // 03:00:00
```

`FakeDateTimeProvider` starts at the time you pass and changes only with `Advance(TimeSpan)` and `SetTime(DateTimeOffset)`. It is thread-safe. It is not a `TimeProvider`, so timers and delays never see its time.

## Time zones

`TimeZones.Find(id)` takes an IANA id (`Europe/Istanbul`) or a Windows id (`Turkey Standard Time`) on any operating system. `TimeZones.TryFind(id, out zone)` returns `false` instead of throwing, also for a `null` or blank id. The provider extensions give the current time and date in a zone:

```csharp Program.cs
// dotnet add package CSharpEssentials.Time
using CSharpEssentials.Time;

var clock = new FakeDateTimeProvider(new DateTimeOffset(2024, 6, 15, 22, 30, 0, TimeSpan.Zero));
TimeZoneInfo istanbul = TimeZones.Find("Europe/Istanbul");

Console.WriteLine(clock.NowIn(istanbul).ToString("yyyy-MM-dd HH:mm zzz")); // 2024-06-16 01:30 +03:00
Console.WriteLine(clock.TodayIn(istanbul).ToString("yyyy-MM-dd"));          // 2024-06-16
Console.WriteLine(clock.UtcNowDate.ToString("yyyy-MM-dd"));                 // 2024-06-15

var birthDate = new DateOnly(2000, 6, 16);
Console.WriteLine(birthDate.GetAge(clock));             // 23: today is 2024-06-15 in the clock's zone (UTC)
Console.WriteLine(birthDate.GetAge(clock, istanbul));   // 24: it is already 2024-06-16 in Istanbul

Console.WriteLine(TimeZones.TryFind("Mars/Olympus", out TimeZoneInfo? zone)); // False
try
{
    TimeZones.Find("Mars/Olympus");
}
catch (TimeZoneNotFoundException ex)
{
    Console.WriteLine(ex.Message);
}
```

- `Find` throws `ArgumentNullException` for `null` and `TimeZoneNotFoundException` (naming the id) when neither lookup finds it.
- On the .NET 9 and later builds, an id the system does not know directly is converted between IANA and Windows with `TimeZoneInfo.TryConvertIanaIdToWindowsId` or `TryConvertWindowsIdToIanaId` and looked up again. That needs ICU, so with `InvariantGlobalization` only the system's own ids resolve. The `netstandard2.1` build relies on `TimeZoneInfo.FindSystemTimeZoneById`, which converts on .NET 6 and later.
- `NowIn(zone)` returns the provider's `UtcNow` converted to the zone, with the zone's offset. `TodayIn(zone)` is its date.

## Date helpers

```csharp Program.cs
// dotnet add package CSharpEssentials.Time
using CSharpEssentials.Time;

var saturday = new DateOnly(2024, 6, 15);
Console.WriteLine($"{saturday.NextDayOfWeek(DayOfWeek.Monday):yyyy-MM-dd}");                         // 2024-06-17
Console.WriteLine($"{saturday.NextDayOfWeek(DayOfWeek.Saturday):yyyy-MM-dd}");                       // 2024-06-22
Console.WriteLine($"{saturday.NextDayOfWeek(DayOfWeek.Saturday, includeCurrent: true):yyyy-MM-dd}"); // 2024-06-15
Console.WriteLine($"{saturday.PreviousDayOfWeek(DayOfWeek.Friday):yyyy-MM-dd}");                     // 2024-06-14

var noon = new DateTime(2024, 6, 15, 12, 30, 0, DateTimeKind.Utc);
Console.WriteLine($"{noon.NextDayOfWeek(DayOfWeek.Monday):yyyy-MM-dd HH:mm}"); // 2024-06-17 12:30, time of day kept
Console.WriteLine($"{noon.ToDateOnly():yyyy-MM-dd}");   // 2024-06-15
Console.WriteLine($"{noon.ToTimeOnly():HH:mm}");        // 12:30

Console.WriteLine(new DateOnly(2000, 2, 29).GetAge(new DateOnly(2025, 2, 28))); // 24
Console.WriteLine(new DateOnly(2000, 2, 29).GetAge(new DateOnly(2025, 3, 1)));  // 25
```

- `NextDayOfWeek` and `PreviousDayOfWeek` exist for `DateTime` and `DateOnly`. They skip the current day unless `includeCurrent: true`.
- `GetAge(today)` counts completed years. `GetAge(provider)` uses today in the provider's `TimeZone`; `GetAge(provider, zone)` uses today in `zone`. A birth date after the reference date throws `ArgumentOutOfRangeException`.
- `ToDateOnly` and `ToTimeOnly` keep the parts of the value as they are; they do not convert time zones.

## Upgrading to 9.0

`DateTimeProvider.TimeZone` returns the `LocalTimeZone` of the wrapped `TimeProvider` instead of always `TimeZoneInfo.Local`. Nothing changes with `TimeProvider.System`. Over `FakeTimeProvider` it is UTC unless the test calls `SetLocalTimeZone`, and `GetAge(dateTimeProvider)` follows it.

## See also

- [Agent skill](https://github.com/senrecep/CSharpEssentials/blob/main/.well-known/agent-skills/csharpessentials-time/SKILL.md) and [example project](https://github.com/senrecep/CSharpEssentials/blob/main/examples/Examples.Time).
- `Guider.NewGuid(TimeProvider)` in [CSharpEssentials.Core](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Core/README.MD) stamps GUIDs with the same clock.
