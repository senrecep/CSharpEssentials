using CSharpEssentials.DependencyInjection;
using Examples.Endpoints.Todos;

namespace Examples.Endpoints.Services;

[Decorates(typeof(ITodoStore))]
public sealed partial class LoggingTodoStore(ITodoStore inner, ILogger<LoggingTodoStore> logger) : ITodoStore
{
    public Todo[] List() => inner.List();

    public Todo? Find(int id) => inner.Find(id);

    public Todo Add(string title)
    {
        Todo todo = inner.Add(title);
        LogAdded(logger, todo.Id);
        return todo;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Added todo {Id}")]
    private static partial void LogAdded(ILogger logger, int id);
}
