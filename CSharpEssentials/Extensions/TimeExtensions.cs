using System;
using CSharpEssentials.Maybe;

namespace CSharpEssentials.Time;

public static class TimeExtensions
{
    public static Maybe<DateTime> MsToDateTime(this long? value, DateTime? defaultValue = null) =>
       value.HasValue ? value.MsToDateTime() : DateTimeToMaybe(defaultValue);

    private static Maybe<DateTime> DateTimeToMaybe(DateTime? defaultValue = null) =>
        defaultValue.HasValue ? defaultValue.Value : Maybe.Maybe.None;
}
