using System.Diagnostics.Metrics;

using Mediator;
using CSharpEssentials.Errors;
using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;

using FluentAssertions;

using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Tests.Mediator;

public sealed class LoggingValidationFailureObserverModeTests
{
    private sealed record ModeTaggedCommand : ICommand<Result>;

    private static ValidationFailureContext Failure(ValidationMode mode) => new(
        typeof(ModeTaggedCommand),
        new ModeTaggedCommand(),
        [Error.Validation("NameRequired", "Name is required")],
        mode);

    [Theory]
    [InlineData(ValidationMode.Off, "off")]
    [InlineData((ValidationMode)42, "enforce")]
    public async Task Should_Log_At_Debug_With_Mode_Text(ValidationMode mode, string modeText)
    {
        CapturingLogger logger = new();

        await new LoggingValidationFailureObserver(logger).OnValidationFailedAsync(Failure(mode), default);

        (LogLevel level, string message) = logger.Entries.Should().ContainSingle().Subject;
        level.Should().Be(LogLevel.Debug);
        message.Should().Be($"Validation of ModeTaggedCommand failed in {modeText} mode with 1 error(s): NameRequired");
    }

    [Theory]
    [InlineData(ValidationMode.Enforce, "enforce")]
    [InlineData(ValidationMode.Off, "off")]
    [InlineData((ValidationMode)42, "enforce")]
    public async Task Should_Tag_Counter_With_Mode_Text(ValidationMode mode, string modeText)
    {
        List<string?> modes = [];
        using MeterListener listener = new();
        listener.InstrumentPublished = (instrument, meterListener) =>
        {
            if (instrument.Meter.Name == LoggingValidationFailureObserver.MeterName
                && instrument.Name == LoggingValidationFailureObserver.FailureCounterName)
                meterListener.EnableMeasurementEvents(instrument);
        };
        listener.SetMeasurementEventCallback<long>((_, _, tags, _) =>
        {
            string? request = null;
            string? tagMode = null;
            foreach (KeyValuePair<string, object?> tag in tags)
            {
                if (tag.Key == "request") request = tag.Value as string;
                if (tag.Key == "mode") tagMode = tag.Value as string;
            }
            if (request == nameof(ModeTaggedCommand))
                lock (modes) modes.Add(tagMode);
        });
        listener.Start();

        await new LoggingValidationFailureObserver().OnValidationFailedAsync(Failure(mode), default);

        modes.Should().ContainSingle().Which.Should().Be(modeText);
    }

    [Fact]
    public async Task Should_Log_Structured_Properties()
    {
        CapturingLogger logger = new();

        await new LoggingValidationFailureObserver(logger).OnValidationFailedAsync(Failure(ValidationMode.LogOnly), default);

        Dictionary<string, object?> properties = logger.Properties.Should().ContainSingle().Subject
            .ToDictionary(pair => pair.Key, pair => pair.Value);
        properties["RequestName"].Should().Be(nameof(ModeTaggedCommand));
        properties["ValidationMode"].Should().Be("log_only");
        properties["ErrorCount"].Should().Be(1);
        properties["ErrorCodes"].Should().Be("NameRequired");
    }

    private sealed class CapturingLogger : ILogger<LoggingValidationFailureObserver>
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = [];

        public List<List<KeyValuePair<string, object?>>> Properties { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Entries.Add((logLevel, formatter(state, exception)));
            if (state is IReadOnlyList<KeyValuePair<string, object?>> pairs)
                Properties.Add([.. pairs]);
        }
    }
}
