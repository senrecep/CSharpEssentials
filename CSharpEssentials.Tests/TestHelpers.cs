using System.Diagnostics;
using FluentAssertions;

namespace CSharpEssentials.Tests;

internal static class TestHelpers
{
    public static void ShouldBeApproximately(this TimeSpan actual, TimeSpan expected, TimeSpan tolerance)
    {
        TimeSpan difference = (actual - expected).Duration();
        difference.Should().BeLessThanOrEqualTo(tolerance);
    }

    public static async Task<(T Result, TimeSpan Elapsed)> MeasureExecutionTime<T>(Func<Task<T>> action)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            T result = await action();
            return (result, sw.Elapsed);
        }
        finally
        {
            sw.Stop();
        }
    }

    public static async Task<TimeSpan> MeasureExecutionTime(Func<Task> action)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            await action();
            return sw.Elapsed;
        }
        finally
        {
            sw.Stop();
        }
    }

    public static T MeasureExecutionTime<T>(Func<T> action, out TimeSpan elapsed)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            return action();
        }
        finally
        {
            sw.Stop();
            elapsed = sw.Elapsed;
        }
    }

    public static void MeasureExecutionTime(Action action, out TimeSpan elapsed)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            action();
        }
        finally
        {
            sw.Stop();
            elapsed = sw.Elapsed;
        }
    }

    public static void AssertDistribution<T>(IEnumerable<T> items, Func<T, int> categorySelector, int expectedCategories, double tolerance = 0.1)
    {
        var itemsList = items.ToList();
        var groups = itemsList.GroupBy(categorySelector).ToList();
        groups.Should().HaveCount(expectedCategories);

        double expectedCount = itemsList.Count / (double)expectedCategories;
        foreach (IGrouping<int, T> group in groups)
        {
            double ratio = group.Count() / expectedCount;
            ratio.Should().BeApproximately(1.0, tolerance, "Category {0} should be approximately evenly distributed", group.Key);
        }
    }

    public static CancellationToken CreateCancellationToken(bool cancelled = false)
    {
        if (cancelled)
        {
            using CancellationTokenSource cts = new();
            cts.Cancel();
            return cts.Token;
        }
        return CancellationToken.None;
    }
}
