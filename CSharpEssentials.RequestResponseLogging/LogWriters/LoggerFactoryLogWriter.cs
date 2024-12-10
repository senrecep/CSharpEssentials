namespace CSharpEssentials.RequestResponseLogging.LogWriters;

internal sealed class LoggerFactoryLogWriter(ILoggerFactory loggerFactory,
                              LoggingOptions options) : ILogWriter
{
    private readonly ILogger _logger = loggerFactory.CreateLogger(options.LoggerCategoryName);

    public IMessageCreator MessageCreator { get; } = options.UseSeparateContext
                            ? new LogMessageWithContextCreator(options)
                            : new LogMessageCreator(options);

    public Task Write(RequestResponseContext requestResponseContext)
    {
        (string logString, List<string?> values) = MessageCreator.Create(requestResponseContext);
        string?[]? parameters = null;

        if (values is not null)
            parameters = [.. values.AsReadOnly()];
#pragma warning disable CA2254
        _logger.Log(options.LoggingLevel, logString, parameters ?? []);
#pragma warning restore CA2254

        return Task.CompletedTask;
    }
}