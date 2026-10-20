using System.Collections.Concurrent;
using System.Globalization;

using Examples.Idempotency.EndToEnd.App;

namespace Examples.Idempotency.EndToEnd;

/// <summary>
/// Counts the charges that reach the payment provider and, like a real provider, dedupes them on the idempotency key:
/// a call with a key already charged returns that charge. With <c>holdFirstCharge</c> the first call waits for
/// <see cref="Release"/>, keeping its business transaction open. With <c>loseFirstResponse</c> the first call charges
/// and then fails, the way a timeout after the provider did the work does.
/// </summary>
public sealed class TestPaymentProvider(bool holdFirstCharge = false, bool loseFirstResponse = false) : IPaymentProvider
{
    private readonly ConcurrentDictionary<string, string> _charges = new(StringComparer.Ordinal);
    private readonly TaskCompletionSource _firstChargeEntered = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int _calls;
    private int _chargeIds;

    /// <summary>The distinct charges: one per idempotency key.</summary>
    public int Charges => _charges.Count;

    /// <summary>Every call, including the ones the provider deduped.</summary>
    public int Calls => _calls;

    public Task FirstChargeEntered => _firstChargeEntered.Task;

    public void Release() => _release.TrySetResult();

    public async ValueTask<string> ChargeAsync(string idempotencyKey, string merchantId, string reference, decimal amount, CancellationToken cancellationToken)
    {
        int call = Interlocked.Increment(ref _calls);
        string chargeId = _charges.GetOrAdd(
            idempotencyKey,
            _ => "ch_test_" + Interlocked.Increment(ref _chargeIds).ToString(CultureInfo.InvariantCulture));
        if (holdFirstCharge && call == 1)
        {
            _firstChargeEntered.SetResult();
            await _release.Task.WaitAsync(cancellationToken);
        }

        if (loseFirstResponse && call == 1)
            throw new HttpRequestException("The provider charged, but its response was lost.");

        return chargeId;
    }
}
