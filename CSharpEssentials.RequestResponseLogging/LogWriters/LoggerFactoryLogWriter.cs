namespace CSharpEssentials.RequestResponseLogging.LogWriters;

#if NET8_0_OR_GREATER
internal sealed class LoggerFactoryLogWriter(ILoggerFactory loggerFactory,
                              LoggingOptions options) : ILogWriter
{
    private readonly ILogger _logger = loggerFactory.CreateLogger(options.LoggerCategoryName);
#else
internal sealed class LoggerFactoryLogWriter : ILogWriter
{
    private readonly ILogger _logger;
    private readonly LogLevel _loggingLevel;

    public LoggerFactoryLogWriter(ILoggerFactory loggerFactory, LoggingOptions options)
    {
        _logger = loggerFactory.CreateLogger(options.LoggerCategoryName);
        _loggingLevel = options.LoggingLevel;
        MessageCreator = options.UseSeparateContext
                            ? new LogMessageWithContextCreator(options)
                            : new LogMessageCreator(options);
    }
#endif

#if NET8_0_OR_GREATER
    public IMessageCreator MessageCreator { get; } = options.UseSeparateContext
                            ? new LogMessageWithContextCreator(options)
                            : new LogMessageCreator(options);
#else
    public IMessageCreator MessageCreator { get; }
#endif

    public Task Write(RequestResponseContext requestResponseContext)
    {
        (string logString, List<string?>? values) = MessageCreator.Create(requestResponseContext);

#if NET8_0_OR_GREATER
        LogLevel level = options.LoggingLevel;
#else
        LogLevel level = _loggingLevel;
#endif
        if (!_logger.IsEnabled(level))
            return Task.CompletedTask;

        if (values is null)
        {
            _logger.Log(level, "{LogMessage}", logString);
            return Task.CompletedTask;
        }

        object?[] parameters = [.. values];
#pragma warning disable CA2254
        _logger.Log(level, logString, parameters);
#pragma warning restore CA2254

        return Task.CompletedTask;
    }
}
