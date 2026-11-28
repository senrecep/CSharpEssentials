using System.Collections.Concurrent;

namespace Examples.Mediator;

/// <summary>Counts handler calls so the console output shows when a behavior skipped or serialized a handler.</summary>
internal sealed class Counters
{
    private readonly ConcurrentDictionary<string, int> _calls = new();
    private readonly ConcurrentDictionary<string, int> _running = new();
    private readonly ConcurrentDictionary<string, int> _maxRunning = new();

    public int this[string name] => _calls.GetValueOrDefault(name);

    public void Increment(string name) => _calls.AddOrUpdate(name, 1, static (_, count) => count + 1);

    public void Enter(string name)
    {
        int running = _running.AddOrUpdate(name, 1, static (_, count) => count + 1);
        _maxRunning.AddOrUpdate(name, running, (_, max) => Math.Max(max, running));
    }

    public void Leave(string name) => _running.AddOrUpdate(name, 0, static (_, count) => count - 1);

    public int MaxConcurrent(string name) => _maxRunning.GetValueOrDefault(name);
}
