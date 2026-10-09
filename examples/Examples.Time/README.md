# CSharpEssentials.Time Example

This console application demonstrates date/time utilities from `CSharpEssentials.Time`.

## Features Demonstrated

| Feature | Description |
|---------|-------------|
| **DateTimeProvider** | `DateTimeProvider` over `TimeProvider.System`, reading `UtcNowDateTime`, `UtcNow`, `TimeZone`, `TimeZoneUtc`, `UtcNowDate` and `UtcNowTime` |
| **DateTime Extensions** | `ToDateOnly()` (NET6+), `ToTimeOnly()` (NET6+) |
| **TimeZone Conversions** | UTC to local time with `DateTime.ToLocalTime()` (plain .NET, not part of the package) |
| **Custom Provider** | A hand-written `FixedDateTimeProvider` implementing `IDateTimeProvider` (the package also ships `FakeDateTimeProvider` with `Advance` and `SetTime`) |

## Running

```bash
cd examples/Examples.Time
dotnet run
```
