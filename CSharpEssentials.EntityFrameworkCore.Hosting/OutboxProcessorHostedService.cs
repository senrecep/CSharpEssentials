using CSharpEssentials.EntityFrameworkCore.Outbox;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.EntityFrameworkCore.Hosting;

/// <summary>
/// Runs <see cref="IOutboxProcessor.ProcessBatchAsync"/> in a loop while the host runs. After a full batch it
/// continues at once; after a batch smaller than <see cref="OutboxProcessorOptions.BatchSize"/>, or a batch that
/// threw (the error is logged), it waits <see cref="OutboxProcessorOptions.PollingInterval"/>. Stopping the host
/// cancels the batch in progress; its claimed rows become due again when their lease ends.
/// </summary>
/// <param name="processor">The processor to drive.</param>
/// <param name="options">Batch size and polling interval.</param>
/// <param name="logger">Logs failed batches.</param>
/// <param name="timeProvider">The clock of the polling delay; <see cref="TimeProvider.System"/> when <see langword="null"/>.</param>
public sealed partial class OutboxProcessorHostedService(
    IOutboxProcessor processor,
    OutboxProcessorOptions options,
    ILogger<OutboxProcessorHostedService> logger,
    TimeProvider? timeProvider = null) : BackgroundService
{
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            int claimed;
            try
            {
                claimed = await processor.ProcessBatchAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                LogBatchFailed(exception);
                claimed = 0;
            }

            if (claimed >= options.BatchSize)
                continue;

            try
            {
                await Task.Delay(options.PollingInterval, _timeProvider, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "An outbox batch failed; retrying after the polling interval")]
    private partial void LogBatchFailed(Exception exception);
}
