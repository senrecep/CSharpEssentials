namespace CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks;

/// <summary>Which <see cref="BaseDbContext{TContext}"/> features a <see cref="CseContext"/> turns on.</summary>
public sealed record CseContextMode(DbContextInterceptors Interceptors, bool DispatchDomainEvents);
