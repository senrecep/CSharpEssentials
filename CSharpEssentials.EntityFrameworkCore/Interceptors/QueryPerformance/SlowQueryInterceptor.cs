using System.Data.Common;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.EntityFrameworkCore.Interceptors;

/// <summary>
/// Logs database commands that exceed a configurable duration threshold.
/// <para>
/// Register an optional <see cref="ISlowQueryHandler"/> in DI to receive
/// slow query notifications for custom metrics, alerting, or dashboards.
/// </para>
/// </summary>
public sealed class SlowQueryInterceptor(
    ILogger<SlowQueryInterceptor> logger,
    SlowQueryOptions? options = null,
    ISlowQueryHandler? slowQueryHandler = null) : DbCommandInterceptor
{
    private readonly ILogger<SlowQueryInterceptor> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly double _thresholdMilliseconds = (options?.Threshold ?? TimeSpan.FromSeconds(1)).TotalMilliseconds;

    public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
    {
        HandleIfSlow(command, eventData.Duration);
        return base.ReaderExecuted(command, eventData, result);
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default)
    {
        HandleIfSlow(command, eventData.Duration);
        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override object? ScalarExecuted(DbCommand command, CommandExecutedEventData eventData, object? result)
    {
        HandleIfSlow(command, eventData.Duration);
        return base.ScalarExecuted(command, eventData, result);
    }

    public override ValueTask<object?> ScalarExecutedAsync(DbCommand command, CommandExecutedEventData eventData, object? result, CancellationToken cancellationToken = default)
    {
        HandleIfSlow(command, eventData.Duration);
        return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override int NonQueryExecuted(DbCommand command, CommandExecutedEventData eventData, int result)
    {
        HandleIfSlow(command, eventData.Duration);
        return base.NonQueryExecuted(command, eventData, result);
    }

    public override ValueTask<int> NonQueryExecutedAsync(DbCommand command, CommandExecutedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        HandleIfSlow(command, eventData.Duration);
        return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    private void HandleIfSlow(DbCommand command, TimeSpan elapsed, [CallerMemberName] string methodName = "")
    {
        if (elapsed.TotalMilliseconds < _thresholdMilliseconds)
            return;

        string parameters = string.Join(", ",
            command.Parameters.Cast<DbParameter>().Select(p => $"{p.ParameterName}={p.Value}"));

        _logger.LogWarning(
            "Slow query ({ElapsedTime}): {CommandText}, Parameters: {Parameters}, Transaction: {Transaction}, Database: {Database} in {MethodName}",
            elapsed,
            command.CommandText,
            parameters,
            command.Transaction is not null ? "Yes" : "No",
            command.Connection?.Database,
            methodName);

        slowQueryHandler?.OnSlowQuery(new SlowQueryContext
        {
            CommandText = command.CommandText,
            Parameters = parameters,
            ElapsedTime = elapsed,
            Database = command.Connection?.Database,
            HasTransaction = command.Transaction is not null,
            MethodName = methodName
        });
    }
}
