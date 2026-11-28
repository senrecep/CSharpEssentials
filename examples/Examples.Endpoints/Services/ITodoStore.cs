using Examples.Endpoints.Todos;

namespace Examples.Endpoints.Services;

public interface ITodoStore
{
    Todo[] List();

    Todo? Find(int id);

    Todo Add(string title);

    bool MarkDone(int id);

    bool Remove(int id);
}
