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

    /// <summary>The <see cref="TimeProvider.LocalTimeZone"/> of the wrapped provider; <see cref="TimeZoneInfo.Local"/> for <see cref="TimeProvider.System"/>.</summary>
    public TimeZoneInfo TimeZone => _timeProvider.LocalTimeZone;

    public TimeZoneInfo TimeZoneUtc => TimeZoneInfo.Utc;
}
