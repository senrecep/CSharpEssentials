using System.Globalization;

namespace CSharpEssentials.RequestResponseLogging.Infrastructure.MessageCreators;

internal sealed class LogMessageWithContextCreator : BaseLogMessageCreator, ILogMessageCreator
{
    private readonly LoggingOptions _loggingOptions;

    public LogMessageWithContextCreator(LoggingOptions loggingOptions) => _loggingOptions = loggingOptions;

    public (string logString, List<string?>? values) Create(RequestResponseContext requestResponseContext)
    {
#if NET8_0_OR_GREATER
#pragma warning disable IDE0028
        List<string?> valueList = _loggingOptions.LoggingFields.Count > 0 ?
        new List<string?>(_loggingOptions.LoggingFields.Count) : new List<string?>();
#pragma warning restore IDE0028
#else
        List<string?> valueList = _loggingOptions.LoggingFields.Count > 0 ?
        new List<string?>(_loggingOptions.LoggingFields.Count) : new List<string?>();
#endif

        var sb = new StringBuilder();

        foreach (LogFields logField in _loggingOptions.LoggingFields)
        {
            string? generatedStr = GenerateLogStringByField(requestResponseContext, _loggingOptions, logField);
            sb.AppendFormat(CultureInfo.InvariantCulture, "{0}: {1}{2}", logField, "{" + logField + "}", Environment.NewLine);
            valueList.Add(generatedStr);
        }

        return (sb.ToString(), valueList);
    }
}
