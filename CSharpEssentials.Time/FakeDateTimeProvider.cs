namespace CSharpEssentials.Time;

public sealed class FakeDateTimeProvider : IDateTimeProvider
{
#if NET9_0_OR_GREATER
    private readonly Lock _gate = new();
#else
    private readonly object _gate = new();
#endif
    private DateTimeOffset _utcNow;

    public FakeDateTimeProvider(DateTimeOffset fixedTime) => _utcNow = fixedTime;

    public DateTimeOffset UtcNow
    {
        get
        {
            lock (_gate)
                return _utcNow;
        }
    }

    public DateTime UtcNowDateTime => UtcNow.UtcDateTime;
    public TimeZoneInfo TimeZone => TimeZoneInfo.Utc;
    public TimeZoneInfo TimeZoneUtc => TimeZoneInfo.Utc;

#if NET6_0_OR_GREATER
    public DateOnly UtcNowDate => DateOnly.FromDateTime(UtcNow.UtcDateTime);
    public TimeOnly UtcNowTime => TimeOnly.FromDateTime(UtcNow.UtcDateTime);
#endif

    public void Advance(TimeSpan duration)
    {
        lock (_gate)
            _utcNow = _utcNow.Add(duration);
    }

    public void SetTime(DateTimeOffset time)
    {
        lock (_gate)
            _utcNow = time;
    }
}
