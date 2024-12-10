
using System.Data.Common;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.EntityFrameworkCore.Interceptors;
public class SlowQueryInterceptor(ILogger<SlowQueryInterceptor> logger) : DbCommandInterceptor
{
    private readonly ILogger<SlowQueryInterceptor> _logger = logger;
    private readonly double _thresholdMilliseconds = TimeSpan.FromSeconds(1).TotalMilliseconds;

    private void LogIfSlowQuery(DbCommand command, TimeSpan elapsedTime, [CallerMemberName] string methodName = "")
    {
        if (elapsedTime.TotalMilliseconds < _thresholdMilliseconds)
            return;

        string parameters = string.Join(", ", command.Parameters.Cast<DbParameter>().Select(p => $"{p.ParameterName}={p.Value}"));

        _logger.LogWarning("Slow query ({ElapsedTime}): {CommandText}, Parameters: {Parameters}, Transaction: {Transaction}, Database: {Database} in {MethodName}",
            elapsedTime,
            command.CommandText,
            parameters,
            command.Transaction != null ? "Yes" : "No",
            command.Connection?.Database,
            methodName);
    }

    public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
    {
        LogIfSlowQuery(command, eventData.Duration);
        return base.ReaderExecuted(command, eventData, result);
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default)
    {
        LogIfSlowQuery(command, eventData.Duration);
        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override object? ScalarExecuted(DbCommand command, CommandExecutedEventData eventData, object? result)
    {
        LogIfSlowQuery(command, eventData.Duration);
        return base.ScalarExecuted(command, eventData, result);
    }

    public override ValueTask<object?> ScalarExecutedAsync(DbCommand command, CommandExecutedEventData eventData, object? result, CancellationToken cancellationToken = default)
    {
        LogIfSlowQuery(command, eventData.Duration);
        return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override int NonQueryExecuted(DbCommand command, CommandExecutedEventData eventData, int result)
    {
        LogIfSlowQuery(command, eventData.Duration);
        return base.NonQueryExecuted(command, eventData, result);
    }

    public override ValueTask<int> NonQueryExecutedAsync(DbCommand command, CommandExecutedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        LogIfSlowQuery(command, eventData.Duration);
        return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }
}
