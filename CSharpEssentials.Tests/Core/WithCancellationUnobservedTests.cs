using CSharpEssentials.Core;
using FluentAssertions;

namespace CSharpEssentials.Tests.Core;

[Collection(NonParallelGcCollection.Name)]
public class WithCancellationUnobservedTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task WithCancellation_WhenAbandonedTaskFaultsLater_ShouldNotRaiseUnobservedTaskException(bool cancelBeforeWaiting)
    {
        string marker = Guid.NewGuid().ToString();
        bool unobserved = false;
        void Handler(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            if (e.Exception.Flatten().InnerExceptions.Any(ex => ex.Message == marker))
                unobserved = true;
        }

        TaskScheduler.UnobservedTaskException += Handler;
        try
        {
            await AbandonAndFaultAsync(marker, cancelBeforeWaiting);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            unobserved.Should().BeFalse();
        }
        finally
        {
            TaskScheduler.UnobservedTaskException -= Handler;
        }
    }

    private static async Task AbandonAndFaultAsync(string marker, bool cancelBeforeWaiting)
    {
        using CancellationTokenSource cts = new();
        if (cancelBeforeWaiting)
            await cts.CancelAsync();
        var tcs = new TaskCompletionSource<int>();

        Task<int> waiting = tcs.Task.WithCancellation(cts.Token);
        if (!cancelBeforeWaiting)
            await cts.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => waiting);
        tcs.SetException(new InvalidOperationException(marker));
    }
}
