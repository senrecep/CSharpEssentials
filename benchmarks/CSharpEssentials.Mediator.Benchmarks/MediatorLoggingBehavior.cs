using System.Diagnostics;
using System.Text.Json;
using CSharpEssentials.Json;
using Mediator;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Mediator.Benchmarks;

// https://github.com/martinothamar/Mediator/blob/v3.0.2/README.md#44-use-pipeline-behaviors
// https://learn.microsoft.com/dotnet/core/extensions/logger-message-generator
public sealed partial class MediatorLoggingBehavior<TRequest, TResponse>(ILogger<MediatorLoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ILoggableRequest
{
    private static readonly string RequestName = typeof(TRequest).Name;
    private static readonly string ResponseName = typeof(TResponse).Name;

    public async ValueTask<TResponse> Handle(TRequest message, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        long start = Stopwatch.GetTimestamp();
        if (message is not IRequestLoggable)
        {
            LogHandling(logger, RequestName);
        }
        else if (logger.IsEnabled(LogLevel.Information))
        {
            string requestJson = JsonSerializer.Serialize(message, EnhancedJsonSerializerOptions.DefaultOptions);
            LogHandlingWithPayload(logger, RequestName, requestJson);
        }

        TResponse response = await next(message, cancellationToken).ConfigureAwait(false);
        if (message is not IResponseLoggable)
        {
            LogHandled(logger, ResponseName);
        }
        else if (logger.IsEnabled(LogLevel.Information))
        {
            string responseJson = JsonSerializer.Serialize(response, EnhancedJsonSerializerOptions.DefaultOptions);
            LogHandledWithPayload(logger, ResponseName, responseJson);
        }

        TimeSpan elapsedTime = Stopwatch.GetElapsedTime(start);
        LogElapsedTime(logger, RequestName, elapsedTime);
        return response;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Handling {RequestName} request")]
    private static partial void LogHandling(ILogger logger, string requestName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Handling {RequestName} request: {RequestJson}")]
    private static partial void LogHandlingWithPayload(ILogger logger, string requestName, string requestJson);

    [LoggerMessage(Level = LogLevel.Information, Message = "Handled {ResponseName} response")]
    private static partial void LogHandled(ILogger logger, string responseName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Handled {ResponseName} response: {ResponseJson}")]
    private static partial void LogHandledWithPayload(ILogger logger, string responseName, string responseJson);

    [LoggerMessage(Level = LogLevel.Information, Message = "Request {RequestName} took {ElapsedTime}")]
    private static partial void LogElapsedTime(ILogger logger, string requestName, TimeSpan elapsedTime);
}
