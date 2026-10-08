using CSharpEssentials.Core;
using FluentAssertions;

namespace CSharpEssentials.Tests.Core;

[Collection(NonParallelGcCollection.Name)]
public class WithCancellationUnobservedTests
{
    [Fact]
    public async Task WithCancellation_WhenAbandonedTaskFaultsLater_ShouldNotRaiseUnobservedTaskException()
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
            await AbandonAndFaultAsync(marker);
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

    private static async Task AbandonAndFaultAsync(string marker)
    {
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();
        var tcs = new TaskCompletionSource<int>();

        await Assert.ThrowsAsync<OperationCanceledException>(() => tcs.Task.WithCancellation(cts.Token));
        tcs.SetException(new InvalidOperationException(marker));
    }
}
