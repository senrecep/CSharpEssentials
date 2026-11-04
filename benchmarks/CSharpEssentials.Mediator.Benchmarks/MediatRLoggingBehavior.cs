using System.Diagnostics;
using System.Text.Json;
using CSharpEssentials.Json;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Mediator.Benchmarks;

// https://github.com/jbogard/MediatR/wiki/Behaviors
// https://learn.microsoft.com/dotnet/core/extensions/logger-message-generator
public sealed partial class MediatRLoggingBehavior<TRequest, TResponse>(ILogger<MediatRLoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, ILoggableRequest
{
    private static readonly string RequestName = typeof(TRequest).Name;
    private static readonly string ResponseName = typeof(TResponse).Name;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        long start = Stopwatch.GetTimestamp();
        if (request is not IRequestLoggable)
        {
            LogHandling(logger, RequestName);
        }
        else if (logger.IsEnabled(LogLevel.Information))
        {
            string requestJson = JsonSerializer.Serialize(request, EnhancedJsonSerializerOptions.DefaultOptions);
            LogHandlingWithPayload(logger, RequestName, requestJson);
        }

        TResponse response = await next(cancellationToken).ConfigureAwait(false);
        if (request is not IResponseLoggable)
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
