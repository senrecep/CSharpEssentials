using System.Diagnostics.CodeAnalysis;
using CSharpEssentials.EntityFrameworkCore.Interceptors;

namespace CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks;

/// <summary>
/// Creating and disposing a context, initialized by touching its change tracker so <c>OnConfiguring</c> runs, then the
/// same plus one <c>AnyAsync</c> on a one-row table. Rows: <c>new</c>, an <c>AddDbContext</c> scope, a pooled factory
/// (labelled: <c>BaseDbContext</c> cannot be pooled because its constructor needs <c>IServiceScopeFactory</c>), CSE's
/// <c>BaseDbContext</c> through <c>AddDbContext</c>, and the same with the audit and slow query interceptors attached
/// from DI. Each <c>BaseDbContext</c> creates a GUID, a DI scope and an Information log entry (disabled here: no logging
/// provider), and is finalizable; that GC cost shows in the Gen columns. Parity: every query row finds the row.
/// </summary>
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "BenchmarkDotNet owns the lifetime: the database and contexts are created in [GlobalSetup] and disposed in [GlobalCleanup].")]
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class DbContextLifetimeComparison
{
    private const string CreateDispose = "CreateDispose";
    private const string CreateQueryDispose = "CreateQueryDispose";

    private BenchDatabase _database = null!;
    private DbContextOptions<PlainContext> _options = null!;
    private ServiceProvider _ef = null!;
    private ServiceProvider _pooled = null!;
    private IDbContextFactory<PlainContext> _factory = null!;
    private ServiceProvider _cse = null!;
    private ServiceProvider _cseInterceptors = null!;

    [GlobalSetup]
    public void Setup()
    {
        _database = BenchDatabase.Create();
        using (PlainContext context = _database.Plain())
        {
            context.Set<Country>().Add(new Country { Code = "C1", Name = "Country 1" });
            context.SaveChanges();
        }

        _options = _database.Options<PlainContext>();
        _ef = new ServiceCollection().AddLogging().AddDbContext<PlainContext>(o => o.UseSqlite(_database.Connection)).BuildServiceProvider();
        _pooled = new ServiceCollection().AddLogging().AddPooledDbContextFactory<PlainContext>(o => o.UseSqlite(_database.Connection)).BuildServiceProvider();
        _factory = _pooled.GetRequiredService<IDbContextFactory<PlainContext>>();
        _cse = new ServiceCollection()
            .AddLogging()
            .AddSingleton(new CseContextMode(DbContextInterceptors.None, DispatchDomainEvents: false))
            .AddDbContext<CseContext>(o => o.UseSqlite(_database.Connection))
            .BuildServiceProvider();
        _cseInterceptors = new ServiceCollection()
            .AddLogging()
            .AddAuditInterceptor(() => "bench")
            .AddSlowQueryInterceptor()
            .AddSingleton(new CseContextMode(DbContextInterceptors.Audit | DbContextInterceptors.SlowQuery, DispatchDomainEvents: false))
            .AddDbContext<CseContext>(o => o.UseSqlite(_database.Connection))
            .BuildServiceProvider();

        Verify().GetAwaiter().GetResult();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _ef.Dispose();
        _pooled.Dispose();
        _cse.Dispose();
        _cseInterceptors.Dispose();
        _database.Dispose();
    }

    // https://learn.microsoft.com/ef/core/dbcontext-configuration/#basic-dbcontext-initialization-with-new
    [Benchmark(Baseline = true), BenchmarkCategory(CreateDispose)]
    public Task<int> Ef_New() => New(query: false);

    // https://learn.microsoft.com/ef/core/dbcontext-configuration/#dbcontext-in-dependency-injection-for-aspnet-core
    [Benchmark, BenchmarkCategory(CreateDispose)]
    public Task<int> Ef_AddDbContext_Scope() => Scoped<PlainContext>(_ef, query: false);

    // https://learn.microsoft.com/ef/core/performance/advanced-performance-topics#dbcontext-pooling
    [Benchmark, BenchmarkCategory(CreateDispose)]
    public Task<int> Ef_PooledDbContextFactory() => Pooled(query: false);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore/Readme.MD#basedbcontext
    [Benchmark, BenchmarkCategory(CreateDispose)]
    public Task<int> CSharpEssentials_BaseDbContext() => Scoped<CseContext>(_cse, query: false);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore/Readme.MD#basedbcontext
    [Benchmark, BenchmarkCategory(CreateDispose)]
    public Task<int> CSharpEssentials_BaseDbContext_Interceptors() => Scoped<CseContext>(_cseInterceptors, query: false);

    // https://learn.microsoft.com/ef/core/dbcontext-configuration/#basic-dbcontext-initialization-with-new
    [Benchmark(Baseline = true), BenchmarkCategory(CreateQueryDispose)]
    public Task<int> Ef_New_Query() => New(query: true);

    // https://learn.microsoft.com/ef/core/dbcontext-configuration/#dbcontext-in-dependency-injection-for-aspnet-core
    [Benchmark, BenchmarkCategory(CreateQueryDispose)]
    public Task<int> Ef_AddDbContext_Scope_Query() => Scoped<PlainContext>(_ef, query: true);

    // https://learn.microsoft.com/ef/core/performance/advanced-performance-topics#dbcontext-pooling
    [Benchmark, BenchmarkCategory(CreateQueryDispose)]
    public Task<int> Ef_PooledDbContextFactory_Query() => Pooled(query: true);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore/Readme.MD#basedbcontext
    [Benchmark, BenchmarkCategory(CreateQueryDispose)]
    public Task<int> CSharpEssentials_BaseDbContext_Query() => Scoped<CseContext>(_cse, query: true);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore/Readme.MD#basedbcontext
    [Benchmark, BenchmarkCategory(CreateQueryDispose)]
    public Task<int> CSharpEssentials_BaseDbContext_Interceptors_Query() => Scoped<CseContext>(_cseInterceptors, query: true);

    private async Task<int> New(bool query)
    {
        await using var context = new PlainContext(_options);
        return await Use(context, query);
    }

    private async Task<int> Pooled(bool query)
    {
        await using PlainContext context = await _factory.CreateDbContextAsync();
        return await Use(context, query);
    }

    private static async Task<int> Scoped<TContext>(ServiceProvider services, bool query)
        where TContext : DbContext
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();
        return await Use(scope.ServiceProvider.GetRequiredService<TContext>(), query);
    }

    private static async Task<int> Use(DbContext context, bool query)
    {
        if (!query)
            return context.ChangeTracker.AutoDetectChangesEnabled ? 1 : 0;

        return await context.Set<Country>().AnyAsync() ? 1 : 0;
    }

    private async Task Verify()
    {
        Parity.Same("DbContextLifetime CreateDispose",
            ("expected", "1"),
            ("Ef new", (await Ef_New()).ToString(System.Globalization.CultureInfo.InvariantCulture)),
            ("Ef scope", (await Ef_AddDbContext_Scope()).ToString(System.Globalization.CultureInfo.InvariantCulture)),
            ("Ef pooled", (await Ef_PooledDbContextFactory()).ToString(System.Globalization.CultureInfo.InvariantCulture)),
            ("CSharpEssentials", (await CSharpEssentials_BaseDbContext()).ToString(System.Globalization.CultureInfo.InvariantCulture)),
            ("CSharpEssentials interceptors", (await CSharpEssentials_BaseDbContext_Interceptors()).ToString(System.Globalization.CultureInfo.InvariantCulture)));
        Parity.Same("DbContextLifetime CreateQueryDispose",
            ("expected", "1"),
            ("Ef new", (await Ef_New_Query()).ToString(System.Globalization.CultureInfo.InvariantCulture)),
            ("Ef scope", (await Ef_AddDbContext_Scope_Query()).ToString(System.Globalization.CultureInfo.InvariantCulture)),
            ("Ef pooled", (await Ef_PooledDbContextFactory_Query()).ToString(System.Globalization.CultureInfo.InvariantCulture)),
            ("CSharpEssentials", (await CSharpEssentials_BaseDbContext_Query()).ToString(System.Globalization.CultureInfo.InvariantCulture)),
            ("CSharpEssentials interceptors", (await CSharpEssentials_BaseDbContext_Interceptors_Query()).ToString(System.Globalization.CultureInfo.InvariantCulture)));
    }
}
