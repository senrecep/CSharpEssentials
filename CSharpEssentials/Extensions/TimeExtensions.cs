using System;
using CSharpEssentials.Core;
using CSharpEssentials.Maybe;

namespace CSharpEssentials.Time;

public static class TimeExtensions
{
    public static Maybe<DateTime> MsToDateTime(this long? value, DateTime? defaultValue = null) =>
       value.HasValue ? GeneralExtensions.MsToDateTime(value.Value) : DateTimeToMaybe(defaultValue);

    private static Maybe<DateTime> DateTimeToMaybe(DateTime? defaultValue = null) =>
        defaultValue.HasValue ? defaultValue.Value : Maybe.Maybe.None;
}
