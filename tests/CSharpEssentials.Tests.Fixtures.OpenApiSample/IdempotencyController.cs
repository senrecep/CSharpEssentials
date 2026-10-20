using CSharpEssentials.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace CSharpEssentials.Tests.Fixtures.OpenApiSample;

/// <summary>Internal so the default controller discovery of other hosts never finds it; added by <see cref="IdempotencyApi.AddIdempotencyControllers"/>.</summary>
[ApiController]
[Route("idempotency/mvc")]
[ApiExplorerSettings(GroupName = IdempotencyApi.Document)]
internal sealed class IdempotencyController : ControllerBase
{
    [HttpPost("orders")]
    [Idempotent(RequireKey = true)]
    public int Create([FromQuery] int value) => value;

    [HttpPost("plain")]
    public int Plain([FromQuery] int value) => value;
}
