using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using CSharpEssentials.Time;

namespace CSharpEssentials.Baselines.Benchmarks;

/// <summary>
/// Pure conversions over <see cref="TimeInputs.Count"/> fixed inputs per invocation, folded into a checksum. The reported
/// time is per conversion. The BCL has no weekday or age API, so those rows are hand-written equivalents. The CSE
/// <c>GetAge</c> also checks that the birth date is not after the reference date; the inputs never trip it. Only the
/// NextDayOfWeekDateTime category exists on net8.0, which runs the netstandard2.1 build without DateOnly or TimeOnly members.
/// Parity compares every row with an expected checksum computed by a brute-force loop in setup.
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class TimeConversionComparison
{
    private const string NextDayOfWeekDateTime = "NextDayOfWeekDateTime";
#if NET9_0_OR_GREATER
    private const string ToDateOnly = "ToDateOnly";
    private const string ToTimeOnly = "ToTimeOnly";
    private const string NextDayOfWeekDateOnly = "NextDayOfWeekDateOnly";
    private const string PreviousDayOfWeekDateOnly = "PreviousDayOfWeekDateOnly";
    private const string GetAge = "GetAge";

    private DateOnly[] _dates = [];
    private DateOnly[] _birthDates = [];
    private DateOnly _today;
#endif
    private DateTime[] _dateTimes = [];
    private DayOfWeek[] _targets = [];

    [GlobalSetup]
    public void Setup()
    {
        _dateTimes = TimeInputs.DateTimes;
        _targets = TimeInputs.Targets;

        long nextDateTime = 0;
        for (int i = 0; i < _dateTimes.Length; i++)
        {
            DateTime d = _dateTimes[i].AddDays(1);
            while (d.DayOfWeek != _targets[i])
                d = d.AddDays(1);
            nextDateTime += d.Ticks;
        }

        Parity.Same(NextDayOfWeekDateTime,
            ("expected", Parity.Of(nextDateTime)),
            (nameof(CSharpEssentials_NextDayOfWeek), Parity.Of(CSharpEssentials_NextDayOfWeek())),
            (nameof(HandWritten_NextDayOfWeek), Parity.Of(HandWritten_NextDayOfWeek())));

#if NET9_0_OR_GREATER
        _dates = TimeInputs.Dates;
        _birthDates = TimeInputs.BirthDates;
        _today = TimeInputs.Today;

        long dateNumbers = 0;
        long timeTicks = 0;
        long nextDate = 0;
        long previousDate = 0;
        long ages = 0;
        for (int i = 0; i < _dateTimes.Length; i++)
        {
            DateTime dt = _dateTimes[i];
            dateNumbers += new DateOnly(dt.Year, dt.Month, dt.Day).DayNumber;
            timeTicks += dt.TimeOfDay.Ticks;

            DateOnly next = _dates[i].AddDays(1);
            while (next.DayOfWeek != _targets[i])
                next = next.AddDays(1);
            nextDate += next.DayNumber;

            DateOnly previous = _dates[i].AddDays(-1);
            while (previous.DayOfWeek != _targets[i])
                previous = previous.AddDays(-1);
            previousDate += previous.DayNumber;

            DateOnly birth = _birthDates[i];
            int age = _today.Year - birth.Year;
            if (_today.Month < birth.Month || _today.Month == birth.Month && _today.Day < birth.Day)
                age--;
            ages += age;
        }

        Parity.Same(ToDateOnly,
            ("expected", Parity.Of(dateNumbers)),
            (nameof(CSharpEssentials_ToDateOnly), Parity.Of(CSharpEssentials_ToDateOnly())),
            (nameof(Bcl_DateOnlyFromDateTime), Parity.Of(Bcl_DateOnlyFromDateTime())));
        Parity.Same(ToTimeOnly,
            ("expected", Parity.Of(timeTicks)),
            (nameof(CSharpEssentials_ToTimeOnly), Parity.Of(CSharpEssentials_ToTimeOnly())),
            (nameof(Bcl_TimeOnlyFromDateTime), Parity.Of(Bcl_TimeOnlyFromDateTime())));
        Parity.Same(NextDayOfWeekDateOnly,
            ("expected", Parity.Of(nextDate)),
            (nameof(CSharpEssentials_NextDayOfWeekDate), Parity.Of(CSharpEssentials_NextDayOfWeekDate())),
            (nameof(HandWritten_NextDayOfWeekDate), Parity.Of(HandWritten_NextDayOfWeekDate())));
        Parity.Same(PreviousDayOfWeekDateOnly,
            ("expected", Parity.Of(previousDate)),
            (nameof(CSharpEssentials_PreviousDayOfWeekDate), Parity.Of(CSharpEssentials_PreviousDayOfWeekDate())),
            (nameof(HandWritten_PreviousDayOfWeekDate), Parity.Of(HandWritten_PreviousDayOfWeekDate())));
        Parity.Same(GetAge,
            ("expected", Parity.Of(ages)),
            (nameof(CSharpEssentials_GetAge), Parity.Of(CSharpEssentials_GetAge())),
            (nameof(HandWritten_GetAge), Parity.Of(HandWritten_GetAge())));
#endif
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Time/Readme.MD#extensions
    [Benchmark(Baseline = true, OperationsPerInvoke = TimeInputs.Count), BenchmarkCategory(NextDayOfWeekDateTime)]
    public long CSharpEssentials_NextDayOfWeek()
    {
        DateTime[] inputs = _dateTimes;
        DayOfWeek[] targets = _targets;
        long sum = 0;
        for (int i = 0; i < inputs.Length; i++)
            sum += inputs[i].NextDayOfWeek(targets[i]).Ticks;
        return sum;
    }

    // https://learn.microsoft.com/dotnet/api/system.datetime.adddays
    [Benchmark(OperationsPerInvoke = TimeInputs.Count), BenchmarkCategory(NextDayOfWeekDateTime)]
    public long HandWritten_NextDayOfWeek()
    {
        DateTime[] inputs = _dateTimes;
        DayOfWeek[] targets = _targets;
        long sum = 0;
        for (int i = 0; i < inputs.Length; i++)
        {
            DateTime dt = inputs[i];
            int days = ((int)targets[i] - (int)dt.DayOfWeek + 7) % 7;
            sum += dt.AddDays(days == 0 ? 7 : days).Ticks;
        }

        return sum;
    }

#if NET9_0_OR_GREATER
    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Time/Readme.MD#extensions
    [Benchmark(Baseline = true, OperationsPerInvoke = TimeInputs.Count), BenchmarkCategory(ToDateOnly)]
    public long CSharpEssentials_ToDateOnly()
    {
        DateTime[] inputs = _dateTimes;
        long sum = 0;
        for (int i = 0; i < inputs.Length; i++)
            sum += inputs[i].ToDateOnly().DayNumber;
        return sum;
    }

    // https://learn.microsoft.com/dotnet/api/system.dateonly.fromdatetime
    [Benchmark(OperationsPerInvoke = TimeInputs.Count), BenchmarkCategory(ToDateOnly)]
    public long Bcl_DateOnlyFromDateTime()
    {
        DateTime[] inputs = _dateTimes;
        long sum = 0;
        for (int i = 0; i < inputs.Length; i++)
            sum += DateOnly.FromDateTime(inputs[i]).DayNumber;
        return sum;
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Time/Readme.MD#extensions
    [Benchmark(Baseline = true, OperationsPerInvoke = TimeInputs.Count), BenchmarkCategory(ToTimeOnly)]
    public long CSharpEssentials_ToTimeOnly()
    {
        DateTime[] inputs = _dateTimes;
        long sum = 0;
        for (int i = 0; i < inputs.Length; i++)
            sum += inputs[i].ToTimeOnly().Ticks;
        return sum;
    }

    // https://learn.microsoft.com/dotnet/api/system.timeonly.fromdatetime
    [Benchmark(OperationsPerInvoke = TimeInputs.Count), BenchmarkCategory(ToTimeOnly)]
    public long Bcl_TimeOnlyFromDateTime()
    {
        DateTime[] inputs = _dateTimes;
        long sum = 0;
        for (int i = 0; i < inputs.Length; i++)
            sum += TimeOnly.FromDateTime(inputs[i]).Ticks;
        return sum;
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Time/Readme.MD#extensions
    [Benchmark(Baseline = true, OperationsPerInvoke = TimeInputs.Count), BenchmarkCategory(NextDayOfWeekDateOnly)]
    public long CSharpEssentials_NextDayOfWeekDate()
    {
        DateOnly[] inputs = _dates;
        DayOfWeek[] targets = _targets;
        long sum = 0;
        for (int i = 0; i < inputs.Length; i++)
            sum += inputs[i].NextDayOfWeek(targets[i]).DayNumber;
        return sum;
    }

    // https://learn.microsoft.com/dotnet/api/system.dateonly.adddays
    [Benchmark(OperationsPerInvoke = TimeInputs.Count), BenchmarkCategory(NextDayOfWeekDateOnly)]
    public long HandWritten_NextDayOfWeekDate()
    {
        DateOnly[] inputs = _dates;
        DayOfWeek[] targets = _targets;
        long sum = 0;
        for (int i = 0; i < inputs.Length; i++)
        {
            DateOnly date = inputs[i];
            int days = ((int)targets[i] - (int)date.DayOfWeek + 7) % 7;
            sum += date.AddDays(days == 0 ? 7 : days).DayNumber;
        }

        return sum;
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Time/Readme.MD#extensions
    [Benchmark(Baseline = true, OperationsPerInvoke = TimeInputs.Count), BenchmarkCategory(PreviousDayOfWeekDateOnly)]
    public long CSharpEssentials_PreviousDayOfWeekDate()
    {
        DateOnly[] inputs = _dates;
        DayOfWeek[] targets = _targets;
        long sum = 0;
        for (int i = 0; i < inputs.Length; i++)
            sum += inputs[i].PreviousDayOfWeek(targets[i]).DayNumber;
        return sum;
    }

    // https://learn.microsoft.com/dotnet/api/system.dateonly.adddays
    [Benchmark(OperationsPerInvoke = TimeInputs.Count), BenchmarkCategory(PreviousDayOfWeekDateOnly)]
    public long HandWritten_PreviousDayOfWeekDate()
    {
        DateOnly[] inputs = _dates;
        DayOfWeek[] targets = _targets;
        long sum = 0;
        for (int i = 0; i < inputs.Length; i++)
        {
            DateOnly date = inputs[i];
            int days = ((int)date.DayOfWeek - (int)targets[i] + 7) % 7;
            sum += date.AddDays(-(days == 0 ? 7 : days)).DayNumber;
        }

        return sum;
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Time/Readme.MD#extensions
    [Benchmark(Baseline = true, OperationsPerInvoke = TimeInputs.Count), BenchmarkCategory(GetAge)]
    public long CSharpEssentials_GetAge()
    {
        DateOnly[] births = _birthDates;
        DateOnly today = _today;
        long sum = 0;
        for (int i = 0; i < births.Length; i++)
            sum += births[i].GetAge(today);
        return sum;
    }

    // https://learn.microsoft.com/dotnet/api/system.dateonly.addyears
    [Benchmark(OperationsPerInvoke = TimeInputs.Count), BenchmarkCategory(GetAge)]
    public long HandWritten_GetAge()
    {
        DateOnly[] births = _birthDates;
        DateOnly today = _today;
        long sum = 0;
        for (int i = 0; i < births.Length; i++)
        {
            DateOnly birth = births[i];
            int age = today.Year - birth.Year;
            sum += birth > today.AddYears(-age) ? age - 1 : age;
        }

        return sum;
    }
#endif
}
