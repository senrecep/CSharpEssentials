using CSharpEssentials.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CSharpEssentials.Tests.Fixtures.OpenApiSample;

/// <summary>Internal so the default controller discovery of other hosts never finds it; added by <see cref="IfMatchApi.AddIfMatchControllers"/>.</summary>
[ApiController]
[Route("if-match/mvc")]
[ApiExplorerSettings(GroupName = IfMatchApi.Document)]
[IfMatch]
internal sealed class IfMatchController : ControllerBase
{
    [HttpPut("optional")]
    public int Optional([FromQuery] int value) => value;

    [HttpPut("required")]
    [IfMatch(Required = true)]
    public int Required([FromQuery] int value) => value;

    [HttpPut("own-412")]
    [ProducesResponseType<string>(StatusCodes.Status412PreconditionFailed, "text/plain")]
    public int Own412([FromQuery] int value) => value;

    [HttpGet("read")]
    public int Read([FromQuery] int value) => value;
}
