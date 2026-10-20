using Microsoft.Extensions.Options;

namespace CSharpEssentials.AspNetCore;

/// <summary>Applies the delegate given to <c>AddIdempotency</c> every time <see cref="IdempotencyOptions"/> are built.</summary>
internal sealed class IdempotencyOptionsSetup(Action<IdempotencyOptions> configure) : IConfigureOptions<IdempotencyOptions>
{
    public void Configure(IdempotencyOptions options) => configure(options);
}
