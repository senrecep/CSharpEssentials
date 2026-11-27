# CSharpEssentials.Time

A testable time abstraction built on .NET `TimeProvider`. Code that reads the time through `IDateTimeProvider` does not depend on the system clock, so unit tests can control the time.

Benchmarks of the clock reads (`UtcNow`, `UtcNowDateTime`, `UtcNowDate`, `UtcNowTime`) against `TimeProvider.System` and `DateTimeOffset.UtcNow`, and of the DateOnly, TimeOnly, weekday and age helpers against the BCL and hand-written code, are in [BENCHMARKS.md](./BENCHMARKS.md).

## Installation

```bash
dotnet add package CSharpEssentials.Time
```

Targets `net11.0`, `net10.0`, `net9.0` and `netstandard2.1` (which adds `Microsoft.Bcl.TimeProvider`). The `DateOnly` and `TimeOnly` members (`UtcNowDate`, `UtcNowTime`, `ToDateOnly`, `ToTimeOnly`, the `DateOnly` weekday overloads, `TodayIn` and `GetAge`) are not in the netstandard2.1 build.

## Usage

### IDateTimeProvider and DateTimeProvider

```csharp
using CSharpEssentials.Time;

public class OrderService(IDateTimeProvider time)
{
    public Order CreateOrder() => new()
    {
        CreatedAt = time.UtcNow,
        CreatedDate = time.UtcNowDate
    };
}

builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
```

`IDateTimeProvider` exposes `UtcNow`, `UtcNowDateTime` (always `DateTimeKind.Utc`), `UtcNowDate`, `UtcNowTime`, `TimeZone` and `TimeZoneUtc`. `DateTimeProvider` wraps a `TimeProvider`, so `TimeProvider.System` or a test provider such as `FakeTimeProvider` both work. `UtcNow` reads `TimeProvider.GetUtcNow()`, `UtcNowDate` and `UtcNowTime` are the UTC date and time of day, `TimeZone` is the `LocalTimeZone` of the `TimeProvider` (`TimeZoneInfo.Local` for `TimeProvider.System`) and `TimeZoneUtc` is `TimeZoneInfo.Utc`.

### Testing with a fake clock

In tests, wrap `FakeTimeProvider` from `Microsoft.Extensions.TimeProvider.Testing`: `new DateTimeProvider(fakeTimeProvider)`. The same fake then drives `IDateTimeProvider`, timers, `Task.Delay(TimeSpan, TimeProvider)` and anything else that takes the `TimeProvider`, and `SetLocalTimeZone` sets `TimeZone`. This is the clock to use when the code under test waits or schedules.

```csharp
var fakeTime = new FakeTimeProvider(new DateTimeOffset(2025, 1, 15, 10, 0, 0, TimeSpan.Zero));
var time = new DateTimeProvider(fakeTime);

fakeTime.Advance(TimeSpan.FromHours(2));   // time.UtcNow moves, and so do timers on fakeTime
```

### FakeDateTimeProvider

The package also ships `FakeDateTimeProvider`, a small clock for code that only reads `IDateTimeProvider`. It starts at a fixed time and moves only with `Advance` and `SetTime`. Its `TimeZone` is UTC, and it is safe to use from several threads. It is not a `TimeProvider`: timers and delays never see its time, so use the `FakeTimeProvider` setup above for code that waits. The sample asserts with FluentAssertions, which is not a dependency of this package.

```csharp
var fixedTime = new DateTimeOffset(2025, 1, 15, 10, 0, 0, TimeSpan.Zero);
var fake = new FakeDateTimeProvider(fixedTime);

Order order = new OrderService(fake).CreateOrder();
order.CreatedAt.Should().Be(fixedTime);

fake.Advance(TimeSpan.FromHours(2));
fake.UtcNow.Should().Be(fixedTime.AddHours(2));

fake.SetTime(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
fake.UtcNowDate.Should().Be(new DateOnly(2026, 1, 1));
fake.UtcNowTime.Should().Be(new TimeOnly(0, 0, 0));
```

### Extensions

```csharp
DateTime now = DateTime.UtcNow;

DateOnly date = now.ToDateOnly();
TimeOnly time = now.ToTimeOnly();

DateOnly today = new(2024, 6, 15);                     // Saturday
today.NextDayOfWeek(DayOfWeek.Monday);                 // 2024-06-17
today.PreviousDayOfWeek(DayOfWeek.Saturday);           // 2024-06-08
today.NextDayOfWeek(DayOfWeek.Saturday, includeCurrent: true); // 2024-06-15

new DateOnly(2000, 6, 16).GetAge(today);               // 23
birthDate.GetAge(dateTimeProvider);                    // today in the provider's time zone
```

### Time zones

`TimeZones.Find(id)` takes an IANA id (`Europe/Istanbul`) or a Windows id (`Turkey Standard Time`) on any operating system and throws `TimeZoneNotFoundException` naming the id when neither is known. `TimeZones.TryFind(id, out zone)` returns `false` instead, also for a `null` or blank id. On .NET 8 and later an id the system does not know directly is converted between IANA and Windows with `TimeZoneInfo.TryConvertIanaIdToWindowsId` or `TryConvertWindowsIdToIanaId` and looked up again; that needs ICU, so with `InvariantGlobalization` only the system's own ids resolve.

`NowIn(zone)` and `TodayIn(zone)` give the provider's current time and date in a user's zone, and `GetAge(provider, zone)` counts the age on that date:

```csharp
TimeZoneInfo zone = TimeZones.Find(user.TimeZoneId);

DateTimeOffset localNow = time.NowIn(zone);   // the provider's UtcNow with the zone's offset
DateOnly today = time.TodayIn(zone);           // 2024-06-16 in Istanbul while it is 2024-06-15 22:30 UTC
int age = user.BirthDate.GetAge(time, zone);
```

`NextDayOfWeek` and `PreviousDayOfWeek` exist for `DateTime` and `DateOnly` and skip the current day unless `includeCurrent: true`. `GetAge` counts whole years and throws `ArgumentOutOfRangeException` when the birth date is after the reference date.
