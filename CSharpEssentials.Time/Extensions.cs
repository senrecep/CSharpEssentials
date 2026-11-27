namespace CSharpEssentials.Time;

public static class Extensions
{
    public static DateTime NextDayOfWeek(this DateTime date, DayOfWeek dayOfWeek, bool includeCurrent = false) =>
        date.AddDays(DaysUntil(date.DayOfWeek, dayOfWeek, includeCurrent));

    public static DateTime PreviousDayOfWeek(this DateTime date, DayOfWeek dayOfWeek, bool includeCurrent = false) =>
        date.AddDays(-DaysUntil(dayOfWeek, date.DayOfWeek, includeCurrent));

    /// <summary>The current time of <paramref name="dateTimeProvider"/> in <paramref name="zone"/>, with that zone's offset.</summary>
    /// <param name="dateTimeProvider">The clock.</param>
    /// <param name="zone">The time zone, for example from <see cref="TimeZones.Find"/>.</param>
    /// <returns>The current time in <paramref name="zone"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dateTimeProvider"/> or <paramref name="zone"/> is <see langword="null"/>.</exception>
    public static DateTimeOffset NowIn(this IDateTimeProvider dateTimeProvider, TimeZoneInfo zone)
    {
        _ = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _ = zone ?? throw new ArgumentNullException(nameof(zone));
        return TimeZoneInfo.ConvertTime(dateTimeProvider.UtcNow, zone);
    }

    private static int DaysUntil(DayOfWeek from, DayOfWeek to, bool includeCurrent)
    {
        int days = ((int)to - (int)from + 7) % 7;
        return days == 0 && !includeCurrent ? 7 : days;
    }

#if NET6_0_OR_GREATER
    public static TimeOnly ToTimeOnly(this DateTime dateTime) => TimeOnly.FromDateTime(dateTime);

    public static DateOnly ToDateOnly(this DateTime dateTime) => DateOnly.FromDateTime(dateTime);

    public static DateOnly NextDayOfWeek(this DateOnly date, DayOfWeek dayOfWeek, bool includeCurrent = false) =>
        date.AddDays(DaysUntil(date.DayOfWeek, dayOfWeek, includeCurrent));

    public static DateOnly PreviousDayOfWeek(this DateOnly date, DayOfWeek dayOfWeek, bool includeCurrent = false) =>
        date.AddDays(-DaysUntil(dayOfWeek, date.DayOfWeek, includeCurrent));

    public static int GetAge(this DateOnly birthDate, DateOnly today)
    {
        if (birthDate > today)
            throw new ArgumentOutOfRangeException(nameof(birthDate), birthDate, "Birth date cannot be after the reference date.");

        int age = today.Year - birthDate.Year;
        return birthDate > today.AddYears(-age) ? age - 1 : age;
    }

    public static int GetAge(this DateOnly birthDate, IDateTimeProvider dateTimeProvider)
    {
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        return birthDate.GetAge(dateTimeProvider.TodayIn(dateTimeProvider.TimeZone));
    }

    /// <summary>The age in whole years on today's date in <paramref name="zone"/>, for people whose birthday turns at their own midnight.</summary>
    /// <param name="birthDate">The birth date.</param>
    /// <param name="dateTimeProvider">The clock.</param>
    /// <param name="zone">The person's time zone.</param>
    /// <returns>The completed years.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dateTimeProvider"/> or <paramref name="zone"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="birthDate"/> is after today in <paramref name="zone"/>.</exception>
    public static int GetAge(this DateOnly birthDate, IDateTimeProvider dateTimeProvider, TimeZoneInfo zone) =>
        birthDate.GetAge(dateTimeProvider.TodayIn(zone));

    /// <summary>Today's date in <paramref name="zone"/> by the clock of <paramref name="dateTimeProvider"/>.</summary>
    /// <param name="dateTimeProvider">The clock.</param>
    /// <param name="zone">The time zone, for example from <see cref="TimeZones.Find"/>.</param>
    /// <returns>The calendar date in <paramref name="zone"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dateTimeProvider"/> or <paramref name="zone"/> is <see langword="null"/>.</exception>
    public static DateOnly TodayIn(this IDateTimeProvider dateTimeProvider, TimeZoneInfo zone) =>
        DateOnly.FromDateTime(dateTimeProvider.NowIn(zone).DateTime);
#endif
}
