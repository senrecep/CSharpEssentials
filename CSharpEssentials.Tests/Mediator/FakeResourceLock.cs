using CSharpEssentials.Locking;
using CSharpEssentials.Maybe;

namespace CSharpEssentials.Tests.Mediator;

internal sealed class FakeResourceLock : IResourceLock
{
    public List<string> Keys { get; } = [];
    public List<TimeSpan?> Timeouts { get; } = [];
    public List<CancellationToken> Tokens { get; } = [];
    public bool Held { get; private set; }
    public int Releases { get; private set; }
    public Exception? FailWith { get; set; }

    public ValueTask<IAsyncDisposable> AcquireAsync(string key, TimeSpan? timeout, CancellationToken cancellationToken = default)
    {
        Keys.Add(key);
        Timeouts.Add(timeout);
        Tokens.Add(cancellationToken);
        if (FailWith is not null)
            throw FailWith;

        Held = true;
        return new ValueTask<IAsyncDisposable>(CreateRelease());
    }

    private Release CreateRelease() => new(this);

    public ValueTask<Maybe<IAsyncDisposable>> TryAcquireAsync(string key, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    private sealed class Release(FakeResourceLock owner) : IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            owner.Held = false;
            owner.Releases++;
            return default;
        }
    }
}
