using System;
using System.Runtime.CompilerServices;

namespace CSharpEssentials.Time;

public static class Extensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeOnly ToTimeOnly(this DateTime dateTime) => TimeOnly.FromDateTime(dateTime);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateOnly ToDateOnly(this DateTime dateTime) => DateOnly.FromDateTime(dateTime);
}
