using Examples.Idempotency.EndToEnd.App;

namespace Examples.Idempotency.EndToEnd;

/// <summary>
/// Counts the charges that reach the payment provider. With <c>holdFirstCharge</c> the first charge waits for
/// <see cref="Release"/>, keeping its business transaction open.
/// </summary>
public sealed class TestPaymentProvider(bool holdFirstCharge = false) : IPaymentProvider
{
    private readonly TaskCompletionSource _firstChargeEntered = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int _charges;

    public int Charges => _charges;

    public Task FirstChargeEntered => _firstChargeEntered.Task;

    public void Release() => _release.TrySetResult();

    public async ValueTask<string> ChargeAsync(string merchantId, string reference, decimal amount, CancellationToken cancellationToken)
    {
        int charge = Interlocked.Increment(ref _charges);
        if (holdFirstCharge && charge == 1)
        {
            _firstChargeEntered.SetResult();
            await _release.Task.WaitAsync(cancellationToken);
        }

        return "ch_test_" + charge;
    }
}
