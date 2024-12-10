
namespace CSharpEssentials.RequestResponseLogging.LogWriters;

internal sealed class NullLogWriter : ILogWriter
{
    public IMessageCreator? MessageCreator { get; }

    public Task Write(RequestResponseContext requestResponseContext) => Task.CompletedTask;
}
