using System.Globalization;

namespace CSharpEssentials.RequestResponseLogging.Infrastructure.MessageCreators;

internal sealed class LogMessageWithContextCreator : BaseLogMessageCreator, ILogMessageCreator
{
    private readonly LoggingOptions _loggingOptions;

    public LogMessageWithContextCreator(LoggingOptions loggingOptions) => _loggingOptions = loggingOptions;

    public (string logString, object?[]? values) Create(RequestResponseContext requestResponseContext)
    {
        object?[] values = new object?[_loggingOptions.LoggingFields.Count];
        var sb = new StringBuilder();

        for (int i = 0; i < values.Length; i++)
        {
            LogFields logField = _loggingOptions.LoggingFields[i];
            sb.AppendFormat(CultureInfo.InvariantCulture, "{0}: {1}{2}", logField, "{" + logField + "}", Environment.NewLine);
            values[i] = GenerateLogStringByField(requestResponseContext, _loggingOptions, logField);
        }

        return (sb.ToString(), values);
    }
}
