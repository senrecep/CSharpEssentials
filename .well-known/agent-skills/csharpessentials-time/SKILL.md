---
name: csharpessentials-time
description: Use when code needs a testable clock. Inject IDateTimeProvider from CSharpEssentials.Time (DateTimeProvider over TimeProvider.System in production; FakeDateTimeProvider with Advance/SetTime, or DateTimeProvider over FakeTimeProvider, in tests). Also TimeZones.Find/TryFind for IANA or Windows ids, NowIn/TodayIn for a user's zone, GetAge, NextDayOfWeek/PreviousDayOfWeek and ToDateOnly/ToTimeOnly.
---

# CSharpEssentials.Time

```bash
dotnet add package CSharpEssentials.Time
```

```csharp
using CSharpEssentials.Time;   // IDateTimeProvider, DateTimeProvider, FakeDateTimeProvider, TimeZones, Extensions
```

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials.Time
using CSharpEssentials.Time;

var clock = new FakeDateTimeProvider(new DateTimeOffset(2025, 1, 15, 10, 0, 0, TimeSpan.Zero));
var sessions = new SessionService(clock);

Session session = sessions.Start();
Console.WriteLine(sessions.IsExpired(session));  // False
clock.Advance(TimeSpan.FromMinutes(31));
Console.WriteLine(sessions.IsExpired(session));  // True

public sealed record Session(DateTimeOffset StartedAt);

public sealed class SessionService(IDateTimeProvider clock)
{
    public Session Start() => new(clock.UtcNow);
    public bool IsExpired(Session session) => clock.UtcNow - session.StartedAt > TimeSpan.FromMinutes(30);
}
```

## Registering the clock

```csharp
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
```

`DateTimeProvider` has one constructor, `DateTimeProvider(TimeProvider timeProvider)`. There is no `AddDateTimeProvider()` extension.

## Reading the time

| Member | Type |
|---|---|
| `UtcNow` | `DateTimeOffset` (offset zero) |
| `UtcNowDateTime` | `DateTime` (`Kind = Utc`) |
| `UtcNowDate` | `DateOnly` (UTC date) |
| `UtcNowTime` | `TimeOnly` (UTC time of day) |
| `TimeZone` | `TimeProvider.LocalTimeZone` for `DateTimeProvider`; UTC for `FakeDateTimeProvider` |
| `TimeZoneUtc` | `TimeZoneInfo.Utc` |

## Testing code that waits

```csharp Program.cs
// dotnet add package CSharpEssentials.Time
// dotnet add package Microsoft.Extensions.TimeProvider.Testing
using CSharpEssentials.Time;
using Microsoft.Extensions.Time.Testing;

var fake = new FakeTimeProvider(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero));
IDateTimeProvider clock = new DateTimeProvider(fake);

Task wait = Task.Delay(TimeSpan.FromSeconds(10), fake);
fake.Advance(TimeSpan.FromSeconds(10));
await wait;
Console.WriteLine($"{clock.UtcNow:HH:mm:ss}"); // 00:00:10
```

`FakeDateTimeProvider` is not a `TimeProvider`; timers and `Task.Delay` cannot use it. Use `FakeTimeProvider` when the code under test waits.

## Handling user time zones

```csharp Program.cs
// dotnet add package CSharpEssentials.Time
using CSharpEssentials.Time;

var clock = new FakeDateTimeProvider(new DateTimeOffset(2024, 6, 15, 22, 30, 0, TimeSpan.Zero));

TimeZoneInfo zone = TimeZones.Find("Turkey Standard Time");  // Windows or IANA id, any OS
DateTimeOffset localNow = clock.NowIn(zone);
DateOnly localToday = clock.TodayIn(zone);
Console.WriteLine($"{localNow:HH:mm zzz} {localToday:yyyy-MM-dd}"); // 01:30 +03:00 2024-06-16

int age = new DateOnly(2000, 6, 16).GetAge(clock, zone);
Console.WriteLine(age);                                       // 24

if (!TimeZones.TryFind("Not/AZone", out TimeZoneInfo? userZone))
    Console.WriteLine("unknown zone, fall back to UTC");
```

## Date helpers

```csharp
DateOnly nextMonday = date.NextDayOfWeek(DayOfWeek.Monday);                    // skips today
DateOnly thisOrNext = date.NextDayOfWeek(DayOfWeek.Monday, includeCurrent: true);
DateTime lastFriday = dateTime.PreviousDayOfWeek(DayOfWeek.Friday);
DateOnly d = dateTime.ToDateOnly();
TimeOnly t = dateTime.ToTimeOnly();
int age = birthDate.GetAge(new DateOnly(2025, 5, 19));   // or GetAge(clock), GetAge(clock, zone)
```

## Rules

- Inject `IDateTimeProvider`; never call `DateTime.Now`, `DateTime.UtcNow` or `DateTime.Today` in services.
- The fake's methods are `Advance(TimeSpan)` and `SetTime(DateTimeOffset)`. There is no `SetUtcNow` on `FakeDateTimeProvider` (that one is on `FakeTimeProvider`).
- `GetAge` throws `ArgumentOutOfRangeException` when the birth date is after the reference date.
- `TimeZones.Find` throws `TimeZoneNotFoundException`; use `TryFind` for user input. With `InvariantGlobalization` only the system's own id style resolves.
- `UtcNowDate`, `UtcNowTime`, `ToDateOnly`, `ToTimeOnly`, `TodayIn`, `GetAge` and the `DateOnly` weekday overloads are missing from the `netstandard2.1` build.
- Since 9.0, `DateTimeProvider.TimeZone` follows `TimeProvider.LocalTimeZone`; over `FakeTimeProvider` it is UTC until `SetLocalTimeZone`.
