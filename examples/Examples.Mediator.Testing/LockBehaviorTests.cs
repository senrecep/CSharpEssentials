using CSharpEssentials.Locking;
using CSharpEssentials.Locking.Testing;
using CSharpEssentials.ResultPattern;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Examples.Mediator.Testing;

public sealed class LockBehaviorTests : IAsyncDisposable
{
    private readonly RecordingResourceLock _locks = new();
    private readonly ServiceProvider _provider;
    private readonly AsyncServiceScope _scope;

    public LockBehaviorTests()
    {
        ServiceCollection services = new();
        services.AddSingleton<IResourceLock>(_locks);
        services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);
        services.AddMediatorBehaviors();
        services.AddMediatorLockBehavior();
        _provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        _scope = _provider.CreateAsyncScope();
    }

    private ISender Sender => _scope.ServiceProvider.GetRequiredService<ISender>();

    [Fact]
    public async Task Handler_Runs_Under_The_Order_Lock_And_Releases_It()
    {
        Result result = await Sender.Send(new CapturePayment(42));

        Assert.True(result.IsSuccess);
        LockAcquisition acquisition = Assert.Single(_locks.Acquisitions);
        Assert.Equal("order:42", acquisition.Key);
        Assert.Equal(TimeSpan.FromMilliseconds(100), acquisition.Timeout);
        Assert.Equal(["order:42"], _locks.Releases);
        Assert.Empty(_locks.HeldKeys);
    }

    [Fact]
    public async Task Held_Key_Times_Out_As_A_Failed_Result()
    {
        await using (await _locks.AcquireAsync("order:42", timeout: null))
        {
            Result result = await Sender.Send(new CapturePayment(42));

            Assert.True(result.IsFailure);
            Assert.Equal(nameof(TimeoutException), result.FirstError.Code);
        }
    }

    [Fact]
    public async Task Lock_Failure_Reaches_The_Caller_As_A_Failed_Result()
    {
        _locks.FailWith = new InvalidOperationException("lock service down");

        Result result = await Sender.Send(new CapturePayment(7));

        Assert.Equal(nameof(InvalidOperationException), result.FirstError.Code);
        Assert.Single(_locks.Acquisitions);
        Assert.Empty(_locks.Releases);
    }

    public async ValueTask DisposeAsync()
    {
        await _scope.DisposeAsync();
        await _provider.DisposeAsync();
    }
}
