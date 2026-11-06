using Serilog.Core;
using Serilog.Events;

namespace CSharpEssentials.AspNetCore.Comparison.Benchmarks;

/// <summary>A Serilog sink that renders every event's message and only counts it.</summary>
public sealed class CountingSerilogSink : ILogEventSink
{
    /// <summary>The counts of this sink.</summary>
    public LogTally Tally { get; } = new();

    /// <inheritdoc />
    public void Emit(LogEvent logEvent)
    {
        ArgumentNullException.ThrowIfNull(logEvent);
        Tally.Record(logEvent.RenderMessage(System.Globalization.CultureInfo.InvariantCulture));
    }
}
