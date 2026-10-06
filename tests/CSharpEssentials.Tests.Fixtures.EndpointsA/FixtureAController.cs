using Microsoft.AspNetCore.Mvc;

namespace CSharpEssentials.Tests.Fixtures.EndpointsA;

[ApiController]
[Route("mvc/fixture-a")]
public sealed class FixtureAController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok("a-mvc");
}
