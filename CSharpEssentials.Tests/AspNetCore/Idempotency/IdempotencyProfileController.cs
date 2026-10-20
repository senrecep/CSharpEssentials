using CSharpEssentials.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace CSharpEssentials.Tests.AspNetCore.Idempotency;

/// <summary>A controller that selects the profile <c>p</c> and an action that overrides the lease on top of it.</summary>
[ApiController]
[Route("mvc/profiled")]
[Idempotent(Profile = "p")]
internal sealed class IdempotencyProfileController(IdempotencyProbe probe) : ControllerBase
{
    [HttpPost]
    [Idempotent(LeaseSeconds = 3 * 60)]
    public Task Create(string outcome)
    {
        probe.Execute();
        return IdempotencyOutcomes.RunAsync(HttpContext, outcome);
    }
}
