namespace CSharpEssentials.Time;

public sealed class FakeDateTimeProvider : IDateTimeProvider
{
    private DateTimeOffset UtcNowValue { get; set; }

    public FakeDateTimeProvider(DateTimeOffset fixedTime) => UtcNowValue = fixedTime;

    public DateTimeOffset UtcNow => UtcNowValue;
    public DateTime UtcNowDateTime => UtcNowValue.UtcDateTime;
    public TimeZoneInfo TimeZone => TimeZoneInfo.Utc;
    public TimeZoneInfo TimeZoneUtc => TimeZoneInfo.Utc;

#if NET6_0_OR_GREATER
    public DateOnly UtcNowDate => DateOnly.FromDateTime(UtcNowValue.UtcDateTime);
    public TimeOnly UtcNowTime => TimeOnly.FromDateTime(UtcNowValue.UtcDateTime);
#endif

    public void Advance(TimeSpan duration) => UtcNowValue = UtcNowValue.Add(duration);
    public void SetTime(DateTimeOffset time) => UtcNowValue = time;
}
