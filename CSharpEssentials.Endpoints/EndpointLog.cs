using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Endpoints;

internal static partial class EndpointLog
{
    public const string Category = "CSharpEssentials.Endpoints";

    [LoggerMessage(Level = LogLevel.Debug, Message = "Mapped endpoint type {EndpointType}")]
    public static partial void Mapped(ILogger logger, Type endpointType);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Skipped endpoint type {EndpointType} because a mapping filter excluded it")]
    public static partial void Filtered(ILogger logger, Type endpointType);
}
