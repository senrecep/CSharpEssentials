using System.Globalization;

namespace CSharpEssentials.RequestResponseLogging.Infrastructure.MessageCreators;

internal sealed class LogMessageWithContextCreator : BaseLogMessageCreator, ILogMessageCreator
{
    private readonly LoggingOptions _loggingOptions;
    private readonly LogFields[] _fields;
    private readonly string _template;

    public LogMessageWithContextCreator(LoggingOptions loggingOptions)
    {
        _loggingOptions = loggingOptions;
        _fields = [.. loggingOptions.LoggingFields];
        var sb = new StringBuilder();
        foreach (LogFields logField in _fields)
            sb.AppendFormat(CultureInfo.InvariantCulture, "{0}: {1}{2}", logField, "{" + logField + "}", Environment.NewLine);

        _template = sb.ToString();
    }

    public (string logString, object?[]? values) Create(RequestResponseContext requestResponseContext)
    {
        object?[] values = new object?[_fields.Length];
        for (int i = 0; i < values.Length; i++)
            values[i] = GenerateLogStringByField(requestResponseContext, _loggingOptions, _fields[i]);

        return (_template, values);
    }
}
