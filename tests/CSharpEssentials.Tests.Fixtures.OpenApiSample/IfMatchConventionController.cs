using Microsoft.AspNetCore.Mvc;

namespace CSharpEssentials.Tests.Fixtures.OpenApiSample;

/// <summary>
/// An MVC controller without <c>[IfMatch]</c>; the hosts give it <c>If-Match</c> through <c>MapControllers().WithIfMatch(required: true)</c>.
/// Internal and added by <see cref="IfMatchApi.AddIfMatchControllers"/>, like <see cref="IfMatchController"/>.
/// </summary>
[ApiController]
[Route("if-match/mvc-convention")]
[ApiExplorerSettings(GroupName = IfMatchApi.Document)]
internal sealed class IfMatchConventionController : ControllerBase
{
    [HttpPut("required")]
    public int Required([FromQuery] int value) => value;

    [HttpGet("read")]
    public int Read([FromQuery] int value) => value;
}
