namespace CSharpEssentials.RequestResponseLogging;

public class LoggingOptions
{

    public LogLevel LoggingLevel { get; set; } = LogLevel.Information;

#if NET8_0_OR_GREATER
    public HashSet<string> HeaderKeys { get; set; } = [];
    public List<LogFields> LoggingFields { get; set; } = [];
#else
    public HashSet<string> HeaderKeys { get; set; } = new HashSet<string>();
    public List<LogFields> LoggingFields { get; set; } = new List<LogFields>();
#endif

    public bool UseSeparateContext { get; set; } = true;

    public string LoggerCategoryName { get; set; } = "RequestResponseLogger";

    public static LoggingOptions CreateAllFields() => new()
    {
        UseSeparateContext = true,
#if NET8_0_OR_GREATER
        LoggingFields =
            [
                LogFields.Request,
                LogFields.Response,
                LogFields.HostName,
                LogFields.Path,
                LogFields.Method,
                LogFields.QueryString,
                LogFields.Headers,
                LogFields.ResponseTiming,
                LogFields.RequestLength,
                LogFields.ResponseLength
            ]
#else
        LoggingFields = new List<LogFields>
            {
                LogFields.Request,
                LogFields.Response,
                LogFields.HostName,
                LogFields.Path,
                LogFields.Method,
                LogFields.QueryString,
                LogFields.Headers,
                LogFields.ResponseTiming,
                LogFields.RequestLength,
                LogFields.ResponseLength
            }
#endif
    };
}

public enum LogFields
{
    Request,
    Response,
    HostName,
    Path,
    Method,
    QueryString,
    Headers,
    ResponseTiming,
    RequestLength,
    ResponseLength,
}
