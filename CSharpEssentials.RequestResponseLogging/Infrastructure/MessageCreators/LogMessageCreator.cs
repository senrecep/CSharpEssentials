using System.Globalization;

namespace CSharpEssentials.RequestResponseLogging.Infrastructure.MessageCreators;

internal sealed class LogMessageCreator(LoggingOptions loggingOptions) : BaseLogMessageCreator, ILogMessageCreator
{
    private readonly LogFields[] _fields = [.. loggingOptions.LoggingFields];

    public (string logString, object?[]? values) Create(RequestResponseContext requestResponseContext)
    {
        var sb = new StringBuilder();

        foreach (LogFields logField in _fields)
        {
            string? generatedStr = GenerateLogStringByField(requestResponseContext, loggingOptions, logField);
            sb.AppendFormat(CultureInfo.InvariantCulture, "{0}: {1}{2}", logField, generatedStr, Environment.NewLine);
        }

        return (sb.ToString(), null);
    }
}
