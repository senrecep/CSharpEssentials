using System.Diagnostics;
using System.Diagnostics.Metrics;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CSharpEssentials.Mediator;

/// <summary>
/// The default <see cref="IValidationFailureObserver"/>. Logs each failure and counts it on the
/// <c>cse.mediator.validation.failures</c> counter of the <c>CSharpEssentials.Mediator</c> meter,
/// tagged with <c>request</c> and <c>mode</c>. <see cref="ValidationMode.LogOnly"/> failures log at
/// Warning, <see cref="ValidationMode.Enforce"/> failures at Debug because the caller already gets them.
/// </summary>
public sealed partial class LoggingValidationFailureObserver(ILogger<LoggingValidationFailureObserver>? logger = null)
    : IValidationFailureObserver
{
    private readonly ILogger _logger = logger ?? NullLogger<LoggingValidationFailureObserver>.Instance;

    public const string MeterName = "CSharpEssentials.Mediator";
    public const string FailureCounterName = "cse.mediator.validation.failures";

    private static readonly Meter Meter = new(MeterName);
    private static readonly Counter<long> FailureCounter = Meter.CreateCounter<long>(
        FailureCounterName,
        unit: "{failure}",
        description: "Validation failures seen by the Mediator validation behavior.");

    public ValueTask OnValidationFailedAsync(ValidationFailureContext failure, CancellationToken cancellationToken)
    {
        string requestName = failure.RequestType.Name;
        string mode = ToTagValue(failure.Mode);

        TagList tags = new()
        {
            { "request", requestName },
            { "mode", mode }
        };
        FailureCounter.Add(1, tags);

        LogLevel level = failure.Mode == ValidationMode.LogOnly ? LogLevel.Warning : LogLevel.Debug;
        if (_logger.IsEnabled(level))
        {
            string errorCodes = string.Join(", ", failure.Errors.Select(error => error.Code));
            LogValidationFailed(_logger, level, requestName, mode, failure.Errors.Count, errorCodes);
        }

        return default;
    }

    private static string ToTagValue(ValidationMode mode) => mode switch
    {
        ValidationMode.LogOnly => "log_only",
        ValidationMode.Off => "off",
        ValidationMode.Enforce => "enforce",
        _ => "enforce"
    };

    [LoggerMessage(Message = "Validation of {RequestName} failed in {ValidationMode} mode with {ErrorCount} error(s): {ErrorCodes}")]
    private static partial void LogValidationFailed(
        ILogger logger, LogLevel level, string requestName, string validationMode, int errorCount, string errorCodes);
}
