using CSharpEssentials.Entity.Interfaces;

namespace CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks;

/// <summary>
/// CSharpEssentials.Entity's domain event list against a hand-written one, without a database, for
/// <see cref="Events"/> events. Read: read the list and index through it; CSE's <c>EntityBase.DomainEvents</c> copies to a
/// new list and wraps it on every read, the hand-written snapshot copies to an array, and the cached view (labelled: live,
/// not a snapshot) copies nothing. RaiseClear: raise every event, then clear. Collect1k: 1,000 entities raise their events,
/// which are collected and cleared the way CSE's <c>DomainEventCollector</c> does (<c>[.. entity.DomainEvents]</c>), a CPU
/// stand-in for the per-entity cost of a save. Parity: each pair reads and collects the same event instances in the same order.
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class DomainEventsReadComparison
{
    private const string Read = "Read";
    private const string RaiseClear = "RaiseClear";
    private const string Collect1k = "Collect1k";
    private const int Entities = 1_000;

    private readonly HandWrittenEventHolder _holder = new();
    private readonly AuditedOrder _entity = new();
    private readonly HandWrittenEventHolder _raiseHolder = new();
    private readonly AuditedOrder _raiseEntity = new();
    private HandWrittenEventHolder[] _holders = [];
    private AuditedOrder[] _entities = [];
    private IDomainEvent[] _events = [];

    [Params(0, 1, 10)]
    public int Events { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _events = [.. Enumerable.Range(0, Events).Select(static i => new OrderPlaced(i))];
        foreach (IDomainEvent domainEvent in _events)
        {
            _holder.Raise(domainEvent);
            _entity.Raise(domainEvent);
        }

        _holders = [.. Enumerable.Range(0, Entities).Select(static _ => new HandWrittenEventHolder())];
        _entities = [.. Enumerable.Range(0, Entities).Select(static _ => new AuditedOrder())];

        string scenario = $"DomainEventsRead Events={Events}";
        string expected = Sequence(_events);
        Parity.Same(scenario,
            ("expected", expected),
            ("HandWritten snapshot", Sequence(_holder.Snapshot)),
            ("HandWritten view", Sequence(_holder.View)),
            ("CSharpEssentials", Sequence(_entity.DomainEvents)));
        Parity.Same(scenario + " collect",
            ("expected", Sequence([.. Enumerable.Range(0, Entities).SelectMany(_ => _events)])),
            ("HandWritten", Sequence(CollectHandWritten())),
            ("CSharpEssentials", Sequence(CollectCse())));
        Parity.Same(scenario + " raise and clear",
            ("expected", "0"),
            ("HandWritten", (HandWritten_List() - Events + _raiseHolder.View.Count).ToString(System.Globalization.CultureInfo.InvariantCulture)),
            ("CSharpEssentials", (CSharpEssentials_EntityBase_RaiseClear() - Events + _raiseEntity.DomainEvents.Count).ToString(System.Globalization.CultureInfo.InvariantCulture)));
    }

    // https://learn.microsoft.com/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/domain-events-design-implementation
    [Benchmark(Baseline = true), BenchmarkCategory(Read)]
    public int HandWritten_ListToArray() => Iterate(_holder.Snapshot);

    // https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1.asreadonly
    [Benchmark, BenchmarkCategory(Read)]
    public int HandWritten_AsReadOnlyView() => Iterate(_holder.View);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Entity/Readme.MD#domain-events
    [Benchmark, BenchmarkCategory(Read)]
    public int CSharpEssentials_EntityBase_DomainEvents() => Iterate(_entity.DomainEvents);

    // https://learn.microsoft.com/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/domain-events-design-implementation
    [Benchmark(Baseline = true), BenchmarkCategory(RaiseClear)]
    public int HandWritten_List()
    {
        foreach (IDomainEvent domainEvent in _events)
            _raiseHolder.Raise(domainEvent);
        _raiseHolder.Clear();
        return _events.Length;
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Entity/Readme.MD#domain-events
    [Benchmark, BenchmarkCategory(RaiseClear)]
    public int CSharpEssentials_EntityBase_RaiseClear()
    {
        foreach (IDomainEvent domainEvent in _events)
            _raiseEntity.Raise(domainEvent);
        _raiseEntity.ClearDomainEvents();
        return _events.Length;
    }

    // https://learn.microsoft.com/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/domain-events-design-implementation
    [Benchmark(Baseline = true), BenchmarkCategory(Collect1k)]
    public int HandWritten_Collect() => CollectHandWritten().Count;

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Entity/Readme.MD#domain-events
    [Benchmark, BenchmarkCategory(Collect1k)]
    public int CSharpEssentials_Collect() => CollectCse().Count;

    private List<IDomainEvent> CollectHandWritten()
    {
        List<IDomainEvent> collected = [];
        foreach (HandWrittenEventHolder holder in _holders)
        {
            foreach (IDomainEvent domainEvent in _events)
                holder.Raise(domainEvent);
            collected.AddRange(holder.Snapshot);
            holder.Clear();
        }

        return collected;
    }

    private List<IDomainEvent> CollectCse()
    {
        List<IDomainEvent> collected = [];
        foreach (AuditedOrder entity in _entities)
        {
            foreach (IDomainEvent domainEvent in _events)
                entity.Raise(domainEvent);
            IDomainEvent[] events = [.. entity.DomainEvents];
            if (events.Length == 0)
                continue;

            collected.AddRange(events);
            entity.ClearDomainEvents();
        }

        return collected;
    }

    private static int Iterate(IReadOnlyList<IDomainEvent> events)
    {
        int total = 0;
        for (int i = 0; i < events.Count; i++)
            total += ((OrderPlaced)events[i]).Ordinal + 1;
        return total;
    }

    private static string Sequence(IReadOnlyList<IDomainEvent> events) =>
        string.Join(',', events.Select(static e => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(e)));
}
