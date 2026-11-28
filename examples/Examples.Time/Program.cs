using CSharpEssentials.Time;
using Microsoft.Extensions.Time.Testing;

Console.WriteLine("--- DateTimeProvider over the system clock ---");
IDateTimeProvider system = new DateTimeProvider(TimeProvider.System);
Console.WriteLine($"UtcNow         {system.UtcNow:O}");
Console.WriteLine($"UtcNowDateTime kind {system.UtcNowDateTime.Kind}");
Console.WriteLine($"UtcNowDate     {system.UtcNowDate:yyyy-MM-dd}");
Console.WriteLine($"UtcNowTime     {system.UtcNowTime:HH:mm:ss}");
Console.WriteLine($"TimeZone       {system.TimeZone.Id} (TimeZoneInfo.Local)");
Console.WriteLine($"TimeZoneUtc    {system.TimeZoneUtc.Id}");

Console.WriteLine("\n--- FakeDateTimeProvider in a test ---");
var fake = new FakeDateTimeProvider(new DateTimeOffset(2025, 1, 15, 10, 0, 0, TimeSpan.Zero));
var trials = new TrialService(fake);
Console.WriteLine($"trial ends     {trials.EndsOn():yyyy-MM-dd}");
fake.Advance(TimeSpan.FromDays(15));
Console.WriteLine($"after Advance  expired: {trials.IsExpired(new DateOnly(2025, 1, 29))}");
fake.SetTime(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
Console.WriteLine($"after SetTime  {fake.UtcNowDate:yyyy-MM-dd}, zone {fake.TimeZone.Id}");

Console.WriteLine("\n--- DateTimeProvider over FakeTimeProvider (timers and delays) ---");
var fakeTime = new FakeTimeProvider(new DateTimeOffset(2025, 1, 15, 10, 0, 0, TimeSpan.Zero));
IDateTimeProvider clock = new DateTimeProvider(fakeTime);
Task reminder = Task.Delay(TimeSpan.FromMinutes(5), fakeTime);
Console.WriteLine($"delay done before Advance: {reminder.IsCompleted}");
fakeTime.Advance(TimeSpan.FromMinutes(5));
await reminder;
Console.WriteLine($"delay done after Advance:  {reminder.IsCompleted}, clock {clock.UtcNow:HH:mm}");
fakeTime.SetLocalTimeZone(TimeZones.Find("Europe/Istanbul"));
Console.WriteLine($"TimeZone follows the provider: {clock.TimeZone.BaseUtcOffset}");

Console.WriteLine("\n--- Time zones ---");
var lateEvening = new FakeDateTimeProvider(new DateTimeOffset(2024, 6, 15, 22, 30, 0, TimeSpan.Zero));
TimeZoneInfo istanbul = TimeZones.Find("Europe/Istanbul");
TimeZoneInfo fromWindowsId = TimeZones.Find("Turkey Standard Time");
Console.WriteLine($"Find by IANA and Windows id: {istanbul.BaseUtcOffset} {fromWindowsId.BaseUtcOffset}");
Console.WriteLine($"TryFind unknown id: {TimeZones.TryFind("Mars/Olympus", out _)}");
Console.WriteLine($"NowIn          {lateEvening.NowIn(istanbul):yyyy-MM-dd HH:mm zzz}");
Console.WriteLine($"TodayIn        {lateEvening.TodayIn(istanbul):yyyy-MM-dd} (UTC date is {lateEvening.UtcNowDate:yyyy-MM-dd})");

Console.WriteLine("\n--- Date helpers ---");
var saturday = new DateOnly(2024, 6, 15);
Console.WriteLine($"NextDayOfWeek(Monday)                    {saturday.NextDayOfWeek(DayOfWeek.Monday):yyyy-MM-dd}");
Console.WriteLine($"NextDayOfWeek(Saturday)                  {saturday.NextDayOfWeek(DayOfWeek.Saturday):yyyy-MM-dd}");
Console.WriteLine($"NextDayOfWeek(Saturday, includeCurrent)  {saturday.NextDayOfWeek(DayOfWeek.Saturday, includeCurrent: true):yyyy-MM-dd}");
Console.WriteLine($"PreviousDayOfWeek(Friday)                {saturday.PreviousDayOfWeek(DayOfWeek.Friday):yyyy-MM-dd}");
var noon = new DateTime(2024, 6, 15, 12, 30, 0, DateTimeKind.Utc);
Console.WriteLine($"DateTime.PreviousDayOfWeek(Monday)       {noon.PreviousDayOfWeek(DayOfWeek.Monday):yyyy-MM-dd HH:mm}");
Console.WriteLine($"ToDateOnly / ToTimeOnly                  {noon.ToDateOnly():yyyy-MM-dd} {noon.ToTimeOnly():HH:mm}");

var birthDate = new DateOnly(2000, 6, 16);
Console.WriteLine($"GetAge(date)                             {birthDate.GetAge(new DateOnly(2024, 6, 15))}");
Console.WriteLine($"GetAge(provider)                         {birthDate.GetAge(lateEvening)}");
Console.WriteLine($"GetAge(provider, Istanbul)               {birthDate.GetAge(lateEvening, istanbul)}");
try
{
    new DateOnly(2030, 1, 1).GetAge(new DateOnly(2025, 1, 1));
}
catch (ArgumentOutOfRangeException)
{
    Console.WriteLine("GetAge with a future birth date         ArgumentOutOfRangeException");
}

sealed class TrialService(IDateTimeProvider clock)
{
    private readonly DateOnly _start = clock.UtcNowDate;

    public DateOnly EndsOn() => _start.AddDays(14);

    public bool IsExpired(DateOnly endsOn) => clock.UtcNowDate > endsOn;
}
