namespace CSharpEssentials.Time;

public sealed class DateTimeProvider : IDateTimeProvider
{
    private readonly TimeProvider _timeProvider;

    public DateTimeProvider(TimeProvider timeProvider) => _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));

    public DateTimeOffset UtcNow => _timeProvider.GetUtcNow();
    public DateTime UtcNowDateTime => UtcNow.UtcDateTime;
#if NET6_0_OR_GREATER
    public DateOnly UtcNowDate => UtcNow.UtcDateTime.ToDateOnly();
    public TimeOnly UtcNowTime => UtcNow.UtcDateTime.ToTimeOnly();
#endif

    public TimeZoneInfo TimeZone => TimeZoneInfo.Local;

    public TimeZoneInfo TimeZoneUtc => TimeZoneInfo.Utc;
}
