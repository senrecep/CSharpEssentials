using CSharpEssentials.Time;

Console.WriteLine("========================================");
Console.WriteLine("CSharpEssentials.Time Example");
Console.WriteLine("========================================\n");

// ============================================================================
// DATE TIME PROVIDER
// ============================================================================
Console.WriteLine("--- DateTimeProvider ---");

IDateTimeProvider provider = new DateTimeProvider(TimeProvider.System);
Console.WriteLine($"UtcNowDateTime: {provider.UtcNowDateTime}");
Console.WriteLine($"UtcNow: {provider.UtcNow}");
Console.WriteLine($"TimeZone: {provider.TimeZone}");
Console.WriteLine($"TimeZoneUtc: {provider.TimeZoneUtc}");
#if NET6_0_OR_GREATER
Console.WriteLine($"UtcNowDate: {provider.UtcNowDate}");
Console.WriteLine($"UtcNowTime: {provider.UtcNowTime}");
#endif
Console.WriteLine();

// ============================================================================
// DATE TIME EXTENSIONS
// ============================================================================
Console.WriteLine("--- DateTime Extensions ---");

DateTime now = DateTime.Now;
Console.WriteLine($"Now: {now}");

#if NET6_0_OR_GREATER
Console.WriteLine($"ToDateOnly: {now.ToDateOnly()}");
Console.WriteLine($"ToTimeOnly: {now.ToTimeOnly()}");
#endif
Console.WriteLine();

// ============================================================================
// TIMEZONE CONVERSIONS
// ============================================================================
Console.WriteLine("--- TimeZone Conversions ---");

DateTime utcTime = new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc);
DateTime local = utcTime.ToLocalTime();
Console.WriteLine($"UTC: {utcTime:O}");
Console.WriteLine($"Local: {local:O}");
Console.WriteLine();

// ============================================================================
// CUSTOM PROVIDER (TESTING)
// ============================================================================
Console.WriteLine("--- Custom Provider (for testing) ---");

DateTime fixedTime = new DateTime(2025, 1, 1, 10, 30, 0, DateTimeKind.Utc);
IDateTimeProvider fixedProvider = new FixedDateTimeProvider(fixedTime);
Console.WriteLine($"Fixed UtcNowDateTime: {fixedProvider.UtcNowDateTime}");
Console.WriteLine($"Fixed UtcNow: {fixedProvider.UtcNow}");
#if NET6_0_OR_GREATER
Console.WriteLine($"Fixed UtcNowDate: {fixedProvider.UtcNowDate}");
Console.WriteLine($"Fixed UtcNowTime: {fixedProvider.UtcNowTime}");
#endif
Console.WriteLine();

Console.WriteLine("========================================");
Console.WriteLine("Demo complete.");
Console.WriteLine("========================================");

// ============================================================================
// FIXED PROVIDER FOR TESTING
// ============================================================================

public class FixedDateTimeProvider : IDateTimeProvider
{
    private readonly DateTime _fixedTime;

    public FixedDateTimeProvider(DateTime fixedTime)
    {
        _fixedTime = fixedTime;
    }

    public TimeZoneInfo TimeZone => TimeZoneInfo.Local;
    public TimeZoneInfo TimeZoneUtc => TimeZoneInfo.Utc;
    public DateTime UtcNowDateTime => _fixedTime;
    public DateTimeOffset UtcNow => new DateTimeOffset(_fixedTime);

#if NET6_0_OR_GREATER
    public DateOnly UtcNowDate => DateOnly.FromDateTime(_fixedTime);
    public TimeOnly UtcNowTime => TimeOnly.FromDateTime(_fixedTime);
#endif
}
