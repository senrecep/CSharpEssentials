using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;
using Mediator;

namespace Examples.Mediator.Testing;

public sealed record CapturePayment(int OrderId) : ICommand<Result>, ILockedRequest
{
    public string LockKey => $"order:{OrderId}";

    public TimeSpan? LockTimeout => TimeSpan.FromMilliseconds(100);
}
