namespace CSharpEssentials.RequestResponseLogging;

public class LoggingOptions
{

    public LogLevel LoggingLevel { get; set; } = LogLevel.Information;

    public HashSet<string> HeaderKeys { get; set; } = [];
    public List<LogFields> LoggingFields { get; set; } = [];

    public bool UseSeparateContext { get; set; } = true;

    public string LoggerCategoryName { get; set; } = "RequestResponseLogger";

    public static LoggingOptions CreateAllFields() => new()
    {
        UseSeparateContext = true,
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
