namespace CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks;

/// <summary>CSE's <see cref="BaseDbContext{TContext}"/>, registered with <c>AddDbContext</c> as its Readme shows.</summary>
public sealed class CseContext(DbContextOptions<CseContext> options, IServiceScopeFactory serviceScopeFactory, CseContextMode mode)
    : BaseDbContext<CseContext>(options, serviceScopeFactory)
{
    protected override DbContextInterceptors InterceptorsFromServices => mode.Interceptors;

    protected override bool DispatchDomainEventsOnSaveChanges => mode.DispatchDomainEvents;

    protected override void OnModelCreating(ModelBuilder modelBuilder) => BenchModel.Configure(modelBuilder);
}
