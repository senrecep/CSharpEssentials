using Microsoft.AspNetCore.Mvc;

namespace CSharpEssentials.Tests.AspNetCore.Idempotency;

/// <summary>Actions without <c>[Idempotent]</c>, so they have no filter that marks their handler as started.</summary>
[ApiController]
[Route("mvc/plain")]
internal sealed class PlainOrdersController(IdempotencyProbe probe) : ControllerBase
{
    [HttpPost]
    public IActionResult Create() => Created("/mvc/plain/1", new { execution = probe.Execute() });

    [HttpPost("matrix")]
    public async Task Matrix(string outcome)
    {
        probe.Execute();
        await IdempotencyOutcomes.RunAsync(HttpContext, outcome);
    }
}
