namespace CSharpEssentials.RequestResponseLogging.Infrastructure.Interfaces;

internal interface IMessageCreator
{
    (string logString, List<string?>? values) Create(RequestResponseContext requestResponseContext);
}
