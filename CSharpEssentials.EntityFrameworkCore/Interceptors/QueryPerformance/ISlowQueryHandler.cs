namespace CSharpEssentials.EntityFrameworkCore.Interceptors;

/// <summary>
/// Optional handler for slow query notifications. Implement to push
/// metrics (Prometheus, OpenTelemetry), trigger alerts, or log to
/// a dedicated slow-query store.
/// </summary>
public interface ISlowQueryHandler
{
    void OnSlowQuery(SlowQueryContext context);
}
