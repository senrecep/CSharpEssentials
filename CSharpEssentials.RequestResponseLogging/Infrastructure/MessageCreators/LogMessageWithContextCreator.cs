using System.Globalization;

namespace CSharpEssentials.RequestResponseLogging.Infrastructure.MessageCreators;

internal sealed class LogMessageWithContextCreator(LoggingOptions loggingOptions) : BaseLogMessageCreator, ILogMessageCreator
{
    public (string logString, List<string?>? values) Create(RequestResponseContext requestResponseContext)
    {
        List<string?> valueList = loggingOptions.LoggingFields.Count > 0 ?
        new List<string?>(loggingOptions.LoggingFields.Count) : [];

        var sb = new StringBuilder();

        foreach (LogFields logField in loggingOptions.LoggingFields)
        {
            string? generatedStr = GenerateLogStringByField(requestResponseContext, loggingOptions, logField);
            sb.AppendFormat(CultureInfo.InvariantCulture, "{0}: {1}{2}", logField, "{" + logField + "}", Environment.NewLine);
            valueList.Add(generatedStr);
        }

        return (sb.ToString(), valueList);
    }
}
