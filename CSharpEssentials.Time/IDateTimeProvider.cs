namespace CSharpEssentials.Time;

public interface IDateTimeProvider
{
    TimeZoneInfo TimeZone { get; }
    TimeZoneInfo TimeZoneUtc { get; }

    DateTime UtcNowDateTime { get; }
    DateTimeOffset UtcNow { get; }

#if NET6_0_OR_GREATER
    DateOnly UtcNowDate { get; }
    TimeOnly UtcNowTime { get; }
#endif
}
