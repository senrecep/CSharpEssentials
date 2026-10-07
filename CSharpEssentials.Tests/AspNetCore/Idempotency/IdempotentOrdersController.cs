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
}
