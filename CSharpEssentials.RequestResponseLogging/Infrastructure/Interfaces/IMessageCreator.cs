namespace CSharpEssentials.RequestResponseLogging.Infrastructure.Interfaces;

internal interface IMessageCreator
{
    (string logString, object?[]? values) Create(RequestResponseContext requestResponseContext);
}
