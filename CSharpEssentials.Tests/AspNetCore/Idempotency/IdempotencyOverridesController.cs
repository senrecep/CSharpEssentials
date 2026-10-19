using CSharpEssentials.AspNetCore;
using CSharpEssentials.Idempotency;
using Microsoft.AspNetCore.Mvc;

namespace CSharpEssentials.Tests.AspNetCore.Idempotency;

/// <summary>
/// MVC side of <see cref="IdempotencyOverridesTests"/>: <c>global</c> uses the options, <c>profile</c> the profile
/// <c>p</c>, and each setting's action overrides that setting on top of <c>p</c>.
/// </summary>
[ApiController]
[Route("mvc/overrides")]
internal sealed class IdempotencyOverridesController(IdempotencyProbe probe) : ControllerBase
{
    [HttpPost("global")]
    [Idempotent]
    public Task Global(string outcome) => RunAsync(outcome);

    [HttpPost("profile")]
    [Idempotent(Profile = "p")]
    public Task WithProfile(string outcome) => RunAsync(outcome);

    [HttpPost("retention")]
    [Idempotent(Profile = "p", RetentionSeconds = 3 * 3600)]
    public Task Retention(string outcome) => RunAsync(outcome);

    [HttpPost("lease")]
    [Idempotent(Profile = "p", LeaseSeconds = 3 * 60)]
    public Task Lease(string outcome) => RunAsync(outcome);

    [HttpPost("server-error-retention")]
    [Idempotent(Profile = "p", ServerErrorRetentionSeconds = 30 * 60)]
    public Task ServerErrorRetention(string outcome) => RunAsync(outcome);

    [HttpPost("require-key")]
    [Idempotent(Profile = "p", RequireKey = true)]
    public Task RequireKey(string outcome) => RunAsync(outcome);

    [HttpPost("unscoped")]
    [Idempotent(Profile = "p", UnscopedKeyBehavior = UnscopedKeyBehavior.Global)]
    public Task Unscoped(string outcome) => RunAsync(outcome);

    [HttpPost("store")]
    [Idempotent(Profile = "p", StoreKey = "endpoint-store")]
    public Task Store(string outcome) => RunAsync(outcome);

    [HttpPost("unknown-profile")]
    [Idempotent(Profile = "missing")]
    public Task UnknownProfile(string outcome) => RunAsync(outcome);

    private Task RunAsync(string outcome)
    {
        probe.Execute();
        return IdempotencyOutcomes.RunAsync(HttpContext, outcome);
    }
}
