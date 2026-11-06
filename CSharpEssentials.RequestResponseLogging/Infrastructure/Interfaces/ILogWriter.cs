namespace CSharpEssentials.RequestResponseLogging.Infrastructure.Interfaces;

internal interface ILogWriter
{
    IMessageCreator? MessageCreator { get; }
    bool NeedsRequestBody => true;
    bool NeedsResponseBody => true;
    Task Write(RequestResponseContext requestResponseContext);
}
