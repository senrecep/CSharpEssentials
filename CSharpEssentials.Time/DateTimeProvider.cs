namespace CSharpEssentials.Time;

public sealed class DateTimeProvider(TimeProvider timeProvider) : IDateTimeProvider
{
    public DateTimeOffset UtcNow => timeProvider.GetUtcNow();
    public DateTime UtcNowDateTime => UtcNow.DateTime;
    public DateOnly UtcNowDate => UtcNow.DateTime.ToDateOnly();
    public TimeOnly UtcNowTime => UtcNow.DateTime.ToTimeOnly();

    public TimeZoneInfo TimeZone => TimeZoneInfo.Local;

    public TimeZoneInfo TimeZoneUtc => TimeZoneInfo.Utc;
}
