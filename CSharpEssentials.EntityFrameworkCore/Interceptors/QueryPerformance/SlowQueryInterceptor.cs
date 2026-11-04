using System.Data.Common;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.EntityFrameworkCore.Interceptors;

/// <summary>
/// Logs database commands that exceed a configurable duration threshold.
/// <para>
/// Register an optional <see cref="ISlowQueryHandler"/> in DI to receive
/// slow query notifications for custom metrics, alerting, or dashboards. An exception thrown by the handler is
/// logged and does not fail the command.
/// </para>
/// <para>
/// Parameter values are included only when sensitive data logging is enabled on the context
/// (<c>EnableSensitiveDataLogging</c>); otherwise each parameter is written as <c>name=?</c>.
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
        HandleIfSlow(command, eventData);
        return base.ReaderExecuted(command, eventData, result);
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default)
    {
        HandleIfSlow(command, eventData);
        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override object? ScalarExecuted(DbCommand command, CommandExecutedEventData eventData, object? result)
    {
        HandleIfSlow(command, eventData);
        return base.ScalarExecuted(command, eventData, result);
    }

    public override ValueTask<object?> ScalarExecutedAsync(DbCommand command, CommandExecutedEventData eventData, object? result, CancellationToken cancellationToken = default)
    {
        HandleIfSlow(command, eventData);
        return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override int NonQueryExecuted(DbCommand command, CommandExecutedEventData eventData, int result)
    {
        HandleIfSlow(command, eventData);
        return base.NonQueryExecuted(command, eventData, result);
    }

    public override ValueTask<int> NonQueryExecutedAsync(DbCommand command, CommandExecutedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        HandleIfSlow(command, eventData);
        return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    private void HandleIfSlow(DbCommand command, CommandExecutedEventData eventData, [CallerMemberName] string methodName = "")
    {
        TimeSpan elapsed = eventData.Duration;
        if (elapsed.TotalMilliseconds < _thresholdMilliseconds)
            return;

        bool logValues = eventData.LogParameterValues;
        string parameters = string.Join(", ",
            command.Parameters.Cast<DbParameter>().Select(p => logValues ? $"{p.ParameterName}={p.Value}" : $"{p.ParameterName}=?"));

        _logger.LogWarning(
            "Slow query ({ElapsedTime}): {CommandText}, Parameters: {Parameters}, Transaction: {Transaction}, Database: {Database} in {MethodName}",
            elapsed,
            command.CommandText,
            parameters,
            command.Transaction is not null ? "Yes" : "No",
            command.Connection?.Database,
            methodName);

        if (slowQueryHandler is null)
            return;

        try
        {
            slowQueryHandler.OnSlowQuery(new SlowQueryContext
            {
                CommandText = command.CommandText,
                Parameters = parameters,
                ElapsedTime = elapsed,
                Database = command.Connection?.Database,
                HasTransaction = command.Transaction is not null,
                MethodName = methodName
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Slow query handler {Handler} threw an exception.", slowQueryHandler.GetType().Name);
        }
    }
}
