using CSharpEssentials.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace CSharpEssentials.Tests.AspNetCore.Idempotency;

[ApiController]
[Route("mvc/orders")]
internal sealed class IdempotentOrdersController(IdempotencyProbe probe) : ControllerBase
{
    [HttpPost]
    [Idempotent]
    public IActionResult Create() => Created("/mvc/orders/1", new { execution = probe.Execute() });

    [HttpPost("matrix")]
    [Idempotent]
    public async Task Matrix(string outcome)
    {
        probe.Execute();
        await IdempotencyOutcomes.RunAsync(HttpContext, outcome);
    }
}
