namespace CSharpEssentials.RequestResponseLogging.Infrastructure.Interfaces;

internal interface ILogWriter
{
    IMessageCreator? MessageCreator { get; }
    Task Write(RequestResponseContext requestResponseContext);
}
