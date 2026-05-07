
namespace CSharpEssentials.RequestResponseLogging.LogWriters;

internal sealed class NullLogWriter : ILogWriter
{
    public IMessageCreator? MessageCreator => null;

    public Task Write(RequestResponseContext requestResponseContext) => Task.CompletedTask;
}
