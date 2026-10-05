namespace CSharpEssentials.Time;

public static class Extensions
{
    public static DateTime NextDayOfWeek(this DateTime date, DayOfWeek dayOfWeek, bool includeCurrent = false) =>
        date.AddDays(DaysUntil(date.DayOfWeek, dayOfWeek, includeCurrent));

    public static DateTime PreviousDayOfWeek(this DateTime date, DayOfWeek dayOfWeek, bool includeCurrent = false) =>
        date.AddDays(-DaysUntil(dayOfWeek, date.DayOfWeek, includeCurrent));

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
        DateTimeOffset localNow = TimeZoneInfo.ConvertTime(dateTimeProvider.UtcNow, dateTimeProvider.TimeZone);
        return birthDate.GetAge(DateOnly.FromDateTime(localNow.DateTime));
    }
#endif
}
