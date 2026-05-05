using Mediator;
using System.Diagnostics;

using CSharpEssentials.Json;

using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Mediator;

public sealed partial class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ILoggableRequest
{
    public async ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;
        string responseName = typeof(TResponse).Name;

#if NET7_0_OR_GREATER
        long startTime = Stopwatch.GetTimestamp();
#else
        var stopwatch = Stopwatch.StartNew();
#endif

        LogRequest(requestName, message);
        TResponse response = await next(message, cancellationToken);
        LogResponse(responseName, message, response);

#if NET7_0_OR_GREATER
        TimeSpan elapsedTime = Stopwatch.GetElapsedTime(startTime);
        LogElapsedTime(logger, requestName, elapsedTime);
#else
        stopwatch.Stop();
        LogElapsedTime(logger, requestName, stopwatch.Elapsed);
#endif

        return response;
    }

    private void LogRequest(string requestName, TRequest message)
    {
        if (message is not IRequestLoggable)
        {
            LogHandling(logger, requestName);
            return;
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            string requestJson = message.ConvertToJson();
            LogHandlingWithPayload(logger, requestName, requestJson);
        }
    }

    private void LogResponse(string responseName, TRequest message, TResponse response)
    {
        if (message is not IResponseLoggable)
        {
            LogHandled(logger, responseName);
            return;
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            string responseJson = response.ConvertToJson();
            LogHandledWithPayload(logger, responseName, responseJson);
        }
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
