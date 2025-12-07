namespace CSharpEssentials.Time;

public static class Extensions
{
#if NET6_0_OR_GREATER
    public static TimeOnly ToTimeOnly(this DateTime dateTime) => TimeOnly.FromDateTime(dateTime);

    public static DateOnly ToDateOnly(this DateTime dateTime) => DateOnly.FromDateTime(dateTime);
#endif
}
