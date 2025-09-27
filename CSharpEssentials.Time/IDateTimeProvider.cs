namespace CSharpEssentials.Time;

public interface IDateTimeProvider
{
    TimeZoneInfo TimeZone { get; }
    TimeZoneInfo TimeZoneUtc { get; }

    DateTime UtcNowDateTime { get; }
    DateTimeOffset UtcNow { get; }

    DateOnly UtcNowDate { get; }
    TimeOnly UtcNowTime { get; }
}
