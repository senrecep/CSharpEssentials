using System.Diagnostics.CodeAnalysis;
using CSharpEssentials.EntityFrameworkCore.Interceptors;

namespace CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks;

/// <summary>
/// SaveChanges of <see cref="Count"/> new orders that each raise <see cref="EventsPerEntity"/> events, published to a
/// counting publisher. Plain EF Core is the reference row (it dispatches nothing). The hand-written override and CSE's
/// <c>BaseDbContext</c> dispatch after the save; CSE's <c>DomainEventInterceptor</c> dispatches in <c>SavingChanges</c>,
/// before the write, which is a labelled semantic difference. Every save runs in a transaction that is rolled back.
/// Parity: every dispatching row writes <see cref="Count"/> rows, publishes <c>Count * EventsPerEntity</c> events and
/// leaves no event on any entity.
/// </summary>
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "BenchmarkDotNet owns the lifetime: the database and contexts are created in [GlobalSetup] and disposed in [GlobalCleanup].")]
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class SaveChangesDomainEventsComparison
{
    private readonly CountingPublisher _publisher = new();
    private BenchDatabase _database = null!;
    private ServiceProvider _services = null!;
    private IServiceScope _scope = null!;
    private PlainContext _plain = null!;
    private HandWrittenDispatchContext _handWritten = null!;
    private CseContext _baseContext = null!;
    private PlainContext _interceptor = null!;

    [Params(1, 100, 1000)]
    public int Count { get; set; }

    [Params(0, 1)]
    public int EventsPerEntity { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _database = BenchDatabase.Create();
        _services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IDomainEventPublisher>(_publisher)
            .AddSingleton<DomainEventInterceptor>()
            .AddSingleton(new CseContextMode(DbContextInterceptors.None, DispatchDomainEvents: true))
            .AddDbContext<CseContext>(options => options.UseSqlite(_database.Connection))
            .BuildServiceProvider();
        _scope = _services.CreateScope();
        _plain = _database.Plain();
        _handWritten = new HandWrittenDispatchContext(_database.Options<HandWrittenDispatchContext>(), _publisher);
        _baseContext = _scope.ServiceProvider.GetRequiredService<CseContext>();
        _interceptor = new PlainContext(_database.Options<PlainContext>(options => options.AddInterceptors(_services.GetRequiredService<DomainEventInterceptor>())));
        Verify().GetAwaiter().GetResult();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _plain.Dispose();
        _handWritten.Dispose();
        _interceptor.Dispose();
        _scope.Dispose();
        _services.Dispose();
        _database.Dispose();
    }

    // https://learn.microsoft.com/ef/core/saving/basic
    [Benchmark(Baseline = true)]
    public Task<int> Ef_SaveChanges() => Run(_plain, Batch());

    // https://learn.microsoft.com/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/domain-events-design-implementation
    [Benchmark]
    public Task<int> HandWritten_DispatchAfterSave() => Run(_handWritten, Batch());

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore/Readme.MD#basedbcontext
    [Benchmark]
    public Task<int> CSharpEssentials_BaseDbContext_Dispatch() => Run(_baseContext, Batch());

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore/Readme.MD#domain-event-interceptor
    [Benchmark]
    public Task<int> CSharpEssentials_DomainEventInterceptor() => Run(_interceptor, Batch());

    private AuditedOrder[] Batch()
    {
        var batch = new AuditedOrder[Count];
        for (int i = 0; i < batch.Length; i++)
        {
            var order = new AuditedOrder { Name = "order", Amount = i };
            for (int e = 0; e < EventsPerEntity; e++)
                order.Raise(new OrderPlaced(i));
            batch[i] = order;
        }

        return batch;
    }

    private static Task<int> Run(DbContext context, AuditedOrder[] batch)
    {
        context.Set<AuditedOrder>().AddRange(batch);
        return BenchDatabase.SaveAndRollBackAsync(context);
    }

    private async Task Verify()
    {
        string scenario = $"SaveChangesDomainEvents Count={Count} EventsPerEntity={EventsPerEntity}";
        Parity.True(scenario, await Run(_plain, Batch()) == Count, "the plain context did not write Count rows");
        Parity.Same(scenario,
            ("expected", $"{Count}|{Count * EventsPerEntity}|0"),
            ("HandWritten", await Dispatch(_handWritten)),
            ("CSharpEssentials BaseDbContext", await Dispatch(_baseContext)),
            ("CSharpEssentials interceptor", await Dispatch(_interceptor)));
    }

    private async Task<string> Dispatch(DbContext context)
    {
        _publisher.Reset();
        AuditedOrder[] batch = Batch();
        int written = await Run(context, batch);
        return $"{written}|{_publisher.Count}|{batch.Sum(static order => order.DomainEvents.Count)}";
    }
}
