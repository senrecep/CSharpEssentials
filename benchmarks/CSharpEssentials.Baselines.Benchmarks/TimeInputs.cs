using System.Diagnostics.CodeAnalysis;

namespace CSharpEssentials.Baselines.Benchmarks;

/// <summary>Fixed inputs for the conversion rows, built once from a seeded generator so every row reads the same values.</summary>
internal static class TimeInputs
{
    public const int Count = 1024;

    public static readonly DateTime[] DateTimes = Build();

    public static readonly DayOfWeek[] Targets = [.. Enumerable.Range(0, Count).Select(static i => (DayOfWeek)(i % 7))];

#if NET9_0_OR_GREATER
    public static readonly DateOnly Today = new(2026, 11, 4);

    public static readonly DateOnly[] Dates = [.. DateTimes.Select(static d => DateOnly.FromDateTime(d))];

    public static readonly DateOnly[] BirthDates = [.. Dates.Select(static d => d > Today ? d.AddYears(-50) : d)];
#endif

    [SuppressMessage("Security", "CA5394:Do not use insecure randomness", Justification = "A fixed seed makes the benchmark inputs reproducible; it protects nothing.")]
    private static DateTime[] Build()
    {
        Random random = new(42);
        long min = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;
        long max = new DateTime(2041, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;
        var values = new DateTime[Count];
        for (int i = 0; i < values.Length; i++)
            values[i] = new DateTime(random.NextInt64(min, max), DateTimeKind.Utc);
        return values;
    }
}
