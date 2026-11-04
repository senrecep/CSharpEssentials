using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using CSharpEssentials.Time;

namespace CSharpEssentials.Baselines.Benchmarks;

/// <summary>
/// Reads the current UTC clock. CSE is <see cref="IDateTimeProvider"/> over <see cref="TimeProvider.System"/>, held as the
/// interface the way dependency injection hands it out. The sealed row reads the same instance through its concrete type.
/// Clock values change between calls, so parity checks that every row lands between two clock reads taken before and after.
/// The UtcNowDate and UtcNowTime rows exist on .NET 9 and later only: net8.0 runs the netstandard2.1 build, which has
/// no DateOnly or TimeOnly members.
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class ClockComparison
{
    private const string UtcNow = "UtcNow";
    private const string UtcNowDateTime = "UtcNowDateTime";
#if NET9_0_OR_GREATER
    private const string UtcNowDate = "UtcNowDate";
    private const string UtcNowTime = "UtcNowTime";
#endif

    [SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance", Justification = "The interface type is the injected idiom being measured; the sealed row measures the concrete type.")]
    private IDateTimeProvider _cse = null!;
    private DateTimeProvider _cseSealed = null!;
    private TimeProvider _timeProvider = null!;

    [GlobalSetup]
    public void Setup()
    {
        _cseSealed = new DateTimeProvider(TimeProvider.System);
        _cse = _cseSealed;
        _timeProvider = TimeProvider.System;

        DateTimeOffset before = DateTimeOffset.UtcNow;
        (string Row, DateTimeOffset Value)[] offsets =
        [
            (nameof(CSharpEssentials_UtcNow), CSharpEssentials_UtcNow()),
            (nameof(CSharpEssentials_SealedUtcNow), CSharpEssentials_SealedUtcNow()),
            (nameof(Bcl_TimeProviderGetUtcNow), Bcl_TimeProviderGetUtcNow()),
            (nameof(Bcl_DateTimeOffsetUtcNow), Bcl_DateTimeOffsetUtcNow()),
        ];
        (string Row, DateTime Value)[] dateTimes =
        [
            (nameof(CSharpEssentials_UtcNowDateTime), CSharpEssentials_UtcNowDateTime()),
            (nameof(Bcl_TimeProviderUtcDateTime), Bcl_TimeProviderUtcDateTime()),
            (nameof(Bcl_DateTimeUtcNow), Bcl_DateTimeUtcNow()),
        ];
#if NET9_0_OR_GREATER
        (string Row, DateOnly Value)[] dates =
        [
            (nameof(CSharpEssentials_UtcNowDate), CSharpEssentials_UtcNowDate()),
            (nameof(Bcl_TimeProviderDateOnly), Bcl_TimeProviderDateOnly()),
        ];
        (string Row, TimeOnly Value)[] times =
        [
            (nameof(CSharpEssentials_UtcNowTime), CSharpEssentials_UtcNowTime()),
            (nameof(Bcl_TimeProviderTimeOnly), Bcl_TimeProviderTimeOnly()),
        ];
#endif
        DateTimeOffset after = DateTimeOffset.UtcNow;

        foreach (var (row, value) in offsets)
            Parity.Holds(UtcNow, row, value.Offset == TimeSpan.Zero && value >= before && value <= after);

        foreach (var (row, value) in dateTimes)
        {
            Parity.Holds(UtcNowDateTime, row,
                value.Kind == DateTimeKind.Utc && value >= before.UtcDateTime && value <= after.UtcDateTime);
        }

#if NET9_0_OR_GREATER
        var firstDate = DateOnly.FromDateTime(before.UtcDateTime);
        var lastDate = DateOnly.FromDateTime(after.UtcDateTime);
        foreach (var (row, value) in dates)
            Parity.Holds(UtcNowDate, row, value == firstDate || value == lastDate);

        var firstTime = TimeOnly.FromDateTime(before.UtcDateTime);
        var lastTime = TimeOnly.FromDateTime(after.UtcDateTime);
        foreach (var (row, value) in times)
        {
            Parity.Holds(UtcNowTime, row, firstTime <= lastTime
                ? value >= firstTime && value <= lastTime
                : value >= firstTime || value <= lastTime);
        }
#endif
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Time/Readme.MD#datetimeprovider
    [Benchmark(Baseline = true), BenchmarkCategory(UtcNow)]
    public DateTimeOffset CSharpEssentials_UtcNow() => _cse.UtcNow;

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Time/Readme.MD#datetimeprovider
    [Benchmark, BenchmarkCategory(UtcNow)]
    public DateTimeOffset CSharpEssentials_SealedUtcNow() => _cseSealed.UtcNow;

    // https://learn.microsoft.com/dotnet/api/system.timeprovider.getutcnow
    [Benchmark, BenchmarkCategory(UtcNow)]
    public DateTimeOffset Bcl_TimeProviderGetUtcNow() => _timeProvider.GetUtcNow();

    // https://learn.microsoft.com/dotnet/api/system.datetimeoffset.utcnow
    [Benchmark, BenchmarkCategory(UtcNow)]
    public DateTimeOffset Bcl_DateTimeOffsetUtcNow() => DateTimeOffset.UtcNow;

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Time/Readme.MD#datetimeprovider
    [Benchmark(Baseline = true), BenchmarkCategory(UtcNowDateTime)]
    public DateTime CSharpEssentials_UtcNowDateTime() => _cse.UtcNowDateTime;

    // https://learn.microsoft.com/dotnet/api/system.datetimeoffset.utcdatetime
    [Benchmark, BenchmarkCategory(UtcNowDateTime)]
    public DateTime Bcl_TimeProviderUtcDateTime() => _timeProvider.GetUtcNow().UtcDateTime;

    // https://learn.microsoft.com/dotnet/api/system.datetime.utcnow
    [Benchmark, BenchmarkCategory(UtcNowDateTime)]
    public DateTime Bcl_DateTimeUtcNow() => DateTime.UtcNow;

#if NET9_0_OR_GREATER
    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Time/Readme.MD#datetimeprovider
    [Benchmark(Baseline = true), BenchmarkCategory(UtcNowDate)]
    public DateOnly CSharpEssentials_UtcNowDate() => _cse.UtcNowDate;

    // https://learn.microsoft.com/dotnet/api/system.dateonly.fromdatetime
    [Benchmark, BenchmarkCategory(UtcNowDate)]
    public DateOnly Bcl_TimeProviderDateOnly() => DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Time/Readme.MD#datetimeprovider
    [Benchmark(Baseline = true), BenchmarkCategory(UtcNowTime)]
    public TimeOnly CSharpEssentials_UtcNowTime() => _cse.UtcNowTime;

    // https://learn.microsoft.com/dotnet/api/system.timeonly.fromdatetime
    [Benchmark, BenchmarkCategory(UtcNowTime)]
    public TimeOnly Bcl_TimeProviderTimeOnly() => TimeOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);
#endif
}
