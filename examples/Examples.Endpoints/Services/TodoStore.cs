using System.Collections.Concurrent;
using CSharpEssentials.DependencyInjection;
using Examples.Endpoints.Todos;

namespace Examples.Endpoints.Services;

[RegisterSingleton]
public sealed class TodoStore : ITodoStore
{
    private readonly ConcurrentDictionary<int, Todo> _todos = new();
    private int _lastId;

    public Todo[] List() => [.. _todos.Values.OrderBy(todo => todo.Id)];

    public Todo? Find(int id) => _todos.GetValueOrDefault(id);

    public Todo Add(string title)
    {
        Todo todo = new(Interlocked.Increment(ref _lastId), title, IsDone: false);
        _todos[todo.Id] = todo;
        return todo;
    }
}
