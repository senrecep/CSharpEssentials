namespace CSharpEssentials.EntityFrameworkCore;

/// <summary>
/// Selects which CSharpEssentials interceptors <see cref="BaseDbContext{TContext}"/>
/// resolves from dependency injection and attaches in <c>OnConfiguring</c>.
/// </summary>
[Flags]
public enum DbContextInterceptors
{
    /// <summary>
    /// No interceptors are attached automatically (default).
    /// </summary>
    None = 0,

    /// <summary>
    /// Attaches <see cref="Interceptors.AuditInterceptor"/> when it is registered in DI.
    /// </summary>
    Audit = 1,

    /// <summary>
    /// Attaches <see cref="Interceptors.DomainEventInterceptor"/> when it is registered in DI.
    /// </summary>
    DomainEvents = 2,

    /// <summary>
    /// Attaches <see cref="Interceptors.SlowQueryInterceptor"/> when it is registered in DI.
    /// </summary>
    SlowQuery = 4,

    /// <summary>
    /// Attaches every supported interceptor that is registered in DI.
    /// </summary>
    All = Audit | DomainEvents | SlowQuery
}
