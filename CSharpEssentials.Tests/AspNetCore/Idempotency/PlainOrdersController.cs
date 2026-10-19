using Microsoft.AspNetCore.Mvc;

namespace CSharpEssentials.Tests.AspNetCore.Idempotency;

/// <summary>An action without <c>[Idempotent]</c>, so it has no filter that marks its handler as started.</summary>
[ApiController]
[Route("mvc/plain")]
internal sealed class PlainOrdersController(IdempotencyProbe probe) : ControllerBase
{
    [HttpPost]
    public IActionResult Create() => Created("/mvc/plain/1", new { execution = probe.Execute() });
}
