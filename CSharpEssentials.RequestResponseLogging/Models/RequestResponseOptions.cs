namespace CSharpEssentials.RequestResponseLogging;

public class RequestResponseOptions
{
    internal string[] IgnoredPaths = [];
    internal LoggingOptions LoggingOptions = new();
    internal ILoggerFactory? LoggerFactory;

    internal ILogWriter? LogWriter { get; set; }

    internal Func<RequestResponseContext, Task>? ReqResHandler { get; set; }

    internal bool LogWriterUsing => LogWriter is not null;

    internal bool HandlerUsing => ReqResHandler is not null;


    public void IgnorePaths(params string[] paths)
    {
        IgnoredPaths = paths;
    }

    internal void UseLogWriter(ILogWriter logWriter)
    {
        LogWriter = logWriter;
    }

    public void UseHandler(Func<RequestResponseContext, Task> Handler)
    {
        ReqResHandler = Handler;
    }

    public void UseLogger(ILoggerFactory loggerFactory, Action<LoggingOptions> options)
    {
        LoggingOptions = new LoggingOptions();
        options.Invoke(LoggingOptions);

        LoggerFactory = loggerFactory;
    }

    public void UseLogger(ILoggerFactory loggerFactory, LoggingOptions loggingOptions)
    {

        LoggingOptions = loggingOptions;

        LoggerFactory = loggerFactory;
    }
}
