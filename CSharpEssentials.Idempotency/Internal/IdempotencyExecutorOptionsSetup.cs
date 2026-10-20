using Microsoft.Extensions.Options;

namespace CSharpEssentials.Idempotency;

/// <summary>The configuration given to <c>AddIdempotencyExecutor</c>, kept apart so a later call can replace it.</summary>
internal sealed class IdempotencyExecutorOptionsSetup(Action<IdempotencyExecutorOptions> configure) : IConfigureOptions<IdempotencyExecutorOptions>
{
    public void Configure(IdempotencyExecutorOptions options) => configure(options);
}
