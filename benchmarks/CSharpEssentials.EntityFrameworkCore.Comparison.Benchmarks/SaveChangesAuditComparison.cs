using System.Diagnostics.CodeAnalysis;
using CSharpEssentials.EntityFrameworkCore.Interceptors;

namespace CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks;

/// <summary>
/// SaveChanges of <see cref="Count"/> orders, Added or Modified, on four contexts: plain EF Core (reference row: it sets
/// no audit fields), a hand-written <c>SaveChangesAsync</c> override, CSE's <c>AuditInterceptor</c> added to the options,
/// and CSE's <c>BaseDbContext</c> attaching that interceptor from DI. The interceptor also creates a DI scope per save.
/// Every save runs in a transaction that is rolled back. Parity: every audited row writes <see cref="Count"/> rows and
/// stamps the same user on every entity.
/// </summary>
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "BenchmarkDotNet owns the lifetime: the contexts are created in [GlobalSetup] and disposed in [GlobalCleanup].")]
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class SaveChangesAuditComparison
{
    private const string User = "bench";

    private BenchDatabase _database = null!;
    private ServiceProvider _services = null!;
    private IServiceScope _scope = null!;
    private PlainContext _plain = null!;
    private HandWrittenAuditContext _handWritten = null!;
    private PlainContext _interceptor = null!;
    private CseContext _baseContext = null!;
    private AuditedOrder[] _seeded = [];

    [Params(1, 100, 1000)]
    public int Count { get; set; }

    [Params(EntityState.Added, EntityState.Modified)]
    public EntityState State { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _database = BenchDatabase.Create();
        _services = new ServiceCollection()
            .AddLogging()
            .AddAuditInterceptor(() => User)
            .AddSingleton(new CseContextMode(DbContextInterceptors.Audit, DispatchDomainEvents: false))
            .AddDbContext<CseContext>(options => options.UseSqlite(_database.Connection))
            .BuildServiceProvider();
        _scope = _services.CreateScope();
        _plain = _database.Plain();
        _handWritten = new HandWrittenAuditContext(_database.Options<HandWrittenAuditContext>(), TimeProvider.System, User);
        _interceptor = new PlainContext(_database.Options<PlainContext>(options => options.AddInterceptors(_services.GetRequiredService<AuditInterceptor>())));
        _baseContext = _scope.ServiceProvider.GetRequiredService<CseContext>();
        if (State == EntityState.Modified)
            _seeded = Seed();

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

    // https://learn.microsoft.com/ef/core/logging-events-diagnostics/interceptors#savechanges-interception
    [Benchmark]
    public Task<int> HandWritten_SavingChangesOverride() => Run(_handWritten, Batch());

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore/Readme.MD#audit-interceptor
    [Benchmark]
    public Task<int> CSharpEssentials_AuditInterceptor() => Run(_interceptor, Batch());

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore/Readme.MD#basedbcontext
    [Benchmark]
    public Task<int> CSharpEssentials_BaseDbContext_Audit() => Run(_baseContext, Batch());

    private AuditedOrder[] Batch()
    {
        if (State == EntityState.Modified)
            return _seeded;

        var batch = new AuditedOrder[Count];
        for (int i = 0; i < batch.Length; i++)
            batch[i] = new AuditedOrder { Name = "order", Amount = i };
        return batch;
    }

    private Task<int> Run(DbContext context, AuditedOrder[] batch)
    {
        DbSet<AuditedOrder> orders = context.Set<AuditedOrder>();
        if (State == EntityState.Added)
        {
            orders.AddRange(batch);
        }
        else
        {
            orders.AttachRange(batch);
            foreach (AuditedOrder order in batch)
                order.Amount++;
        }

        return BenchDatabase.SaveAndRollBackAsync(context);
    }

    private AuditedOrder[] Seed()
    {
        using PlainContext context = _database.Plain();
        AuditedOrder[] seeded = [.. Enumerable.Range(0, Count).Select(static i => new AuditedOrder { Name = "order", Amount = i })];
        context.Set<AuditedOrder>().AddRange(seeded);
        context.SaveChanges();
        return seeded;
    }

    private async Task Verify()
    {
        string scenario = $"SaveChangesAudit Count={Count} State={State}";
        Parity.True(scenario, await Run(_plain, Batch()) == Count, "the plain context did not write Count rows");
        Parity.Same(scenario,
            ("expected", $"{Count}|{User}"),
            ("HandWritten", await Stamp(_handWritten)),
            ("CSharpEssentials interceptor", await Stamp(_interceptor)),
            ("CSharpEssentials BaseDbContext", await Stamp(_baseContext)));
    }

    private async Task<string> Stamp(DbContext context)
    {
        AuditedOrder[] batch = Batch();
        int written = await Run(context, batch);
        string users = string.Join(',', batch.Select(order => State == EntityState.Added ? order.CreatedBy : order.UpdatedBy).Distinct());
        foreach (AuditedOrder order in batch)
            order.SetUpdatedInfo(DateTimeOffset.UnixEpoch, string.Empty);
        return $"{written}|{users}";
    }
}
