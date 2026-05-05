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
        (string logString, List<string?> values) = MessageCreator.Create(requestResponseContext);
        string?[]? parameters = null;

        if (values is not null)
            parameters = [.. values];
#pragma warning disable CA2254
#if NET8_0_OR_GREATER
        if (_logger.IsEnabled(options.LoggingLevel))
            _logger.Log(options.LoggingLevel, logString, parameters ?? Array.Empty<object?>());
#else
        if (_logger.IsEnabled(_loggingLevel))
            _logger.Log(_loggingLevel, logString, parameters ?? Array.Empty<object?>());
#endif
#pragma warning restore CA2254

        return Task.CompletedTask;
    }
}
